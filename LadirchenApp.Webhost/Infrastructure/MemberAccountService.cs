using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Models;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class MemberAccountService
{
  private readonly LadirchenDbContext _database;
  private readonly IFamilyEconomyRepository _economyRepository;
  private readonly IFamilyRepository _familyRepository;
  private readonly IHouseCustomizationRepository _houseRepository;
  private readonly FamilyMembershipOutboxWriter _outboxWriter;
  private readonly IFamilyProgressionRepository _progressionRepository;
  private readonly TimeProvider _timeProvider;
  private readonly ITypedAggregateUnitOfWork _unitOfWork;

  public MemberAccountService(
    LadirchenDbContext database,
    IFamilyRepository familyRepository,
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository,
    IHouseCustomizationRepository houseRepository,
    ITypedAggregateUnitOfWork unitOfWork,
    FamilyMembershipOutboxWriter outboxWriter,
    TimeProvider timeProvider
  )
  {
    _database = database;
    _familyRepository = familyRepository;
    _economyRepository = economyRepository;
    _progressionRepository = progressionRepository;
    _houseRepository = houseRepository;
    _unitOfWork = unitOfWork;
    _outboxWriter = outboxWriter;
    _timeProvider = timeProvider;
  }

  public async Task<MemberAccountInvitationResponse> CreateInvitationAsync(
    FamilyId familyId,
    FamilyMemberId actorId,
    CreateMemberInvitationRequest request,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(familyId);
    ArgumentNullException.ThrowIfNull(actorId);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
    var normalizedEmail = AuthenticationCredentialPolicy.NormalizeEmail(request.Email);
    var role = ParseRole(request.Role);
    var guardianAccess = ParseGuardianAccess(role, request.GuardianAccess);
    await using var transaction = await _database.Database.BeginTransactionAsync(
      IsolationLevel.Serializable,
      cancellationToken
    );

    try
    {
      var loadedFamily = await _familyRepository.LoadAsync(familyId, cancellationToken)
        ?? throw new InvalidOperationException("The family has not been initialized.");
      var family = loadedFamily.Root;
      family.RequireAdministrator(actorId);
      var familyAccount = await _database.AuthenticationAccounts
        .AsNoTracking()
        .SingleOrDefaultAsync(
          item => item.FamilyId == familyId.Value && item.MemberId == actorId.Value,
          cancellationToken
        ) ?? throw new InvalidOperationException("The family administrator account is missing.");
      var now = _timeProvider.GetUtcNow();

      if (
        await _database.AuthenticationAccounts.AnyAsync(
          item => item.NormalizedEmail == normalizedEmail,
          cancellationToken
        )
      )
      {
        throw new InvalidOperationException("The email address already has an account.");
      }

      var existingInvitation = await _database.MemberAccountInvitations
        .FromSqlInterpolated(
          $"SELECT * FROM \"MemberAccountInvitations\" WHERE \"NormalizedEmail\" = {normalizedEmail} AND \"RedeemedAt\" IS NULL FOR UPDATE"
        )
        .SingleOrDefaultAsync(
          cancellationToken
        );

      if (existingInvitation is not null)
      {
        var pendingMember = family.Members.SingleOrDefault(
          item => item.Id.Value == existingInvitation.MemberId
        );

        if (
          existingInvitation.FamilyId != familyId.Value
          || pendingMember is null
          || !pendingMember.InvitationPending
          || !string.Equals(
            pendingMember.EmailAddress,
            normalizedEmail,
            StringComparison.OrdinalIgnoreCase
          )
          || !string.Equals(pendingMember.Name, request.Name.Trim(), StringComparison.Ordinal)
          || pendingMember.Role != role
          || pendingMember.Access != guardianAccess
        )
        {
          throw new InvalidOperationException(
            "The email address already has an account or invitation."
          );
        }

        var reissuedCode = CreateActivationCode();
        var reissuedExpiry = now.Add(AuthenticationSecurityPolicy.MemberInvitationLifetime);
        existingInvitation.ActivationCodeHash = HashActivationCode(reissuedCode);
        existingInvitation.CreatedAt = now;
        existingInvitation.ExpiresAt = reissuedExpiry;
        await _database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new MemberAccountInvitationResponse(
          reissuedCode,
          existingInvitation.EmailAddress,
          reissuedExpiry,
          existingInvitation.MemberId
        );
      }

      var loadedEconomy = await _economyRepository.LoadAsync(family, cancellationToken)
        ?? throw new InvalidOperationException("The family economy has not been initialized.");
      var loadedProgression = await _progressionRepository.LoadAsync(family, cancellationToken)
        ?? throw new InvalidOperationException("The family progression has not been initialized.");
      var loadedHouse = await _houseRepository.LoadAsync(
        family,
        loadedProgression.Root.StoredLevel,
        cancellationToken
      ) ?? throw new InvalidOperationException("The family house has not been initialized.");
      var memberId = new FamilyMemberId(Guid.NewGuid().ToString("N"));
      family.InviteMember(
        actorId,
        memberId,
        request.Name,
        request.Email,
        role,
        guardianAccess,
        now
      );
      var economy = loadedEconomy.Root.SynchronizeMembers(family);
      loadedProgression.Root.SynchronizeParticipants(family);
      loadedHouse.Root.SynchronizeFamilyEntities(family);
      var activationCode = CreateActivationCode();
      var expiresAt = now.Add(AuthenticationSecurityPolicy.MemberInvitationLifetime);
      _database.MemberAccountInvitations.Add(
        new MemberAccountInvitationEntity
        {
          ActivationCodeHash = HashActivationCode(activationCode),
          CreatedAt = now,
          EmailAddress = request.Email.Trim(),
          ExpiresAt = expiresAt,
          FamilyId = familyId.Value,
          FamilyName = familyAccount.FamilyName,
          InvitationId = Guid.NewGuid(),
          MemberId = memberId.Value,
          NormalizedEmail = normalizedEmail,
        }
      );
      var result = await _unitOfWork.SaveAsync(
        new TypedAggregateSaveBatch(
          familyId,
          actorId,
          family: new PendingFamilySave(family, loadedFamily.Revision),
          familyEconomy: new PendingFamilyEconomySave(
            economy,
            loadedEconomy.Presentation,
            loadedEconomy.Revision
          ),
          familyProgression: new PendingFamilyProgressionSave(
            loadedProgression.Root,
            loadedProgression.Revision
          ),
          houseCustomization: new PendingHouseCustomizationSave(
            loadedHouse.Root,
            loadedHouse.Revision
          )
        ),
        cancellationToken
      );
      _outboxWriter.AddChanges(result, familyId);
      await _database.SaveChangesAsync(cancellationToken);
      await transaction.CommitAsync(cancellationToken);

      return new MemberAccountInvitationResponse(
        activationCode,
        request.Email.Trim(),
        expiresAt,
        memberId.Value
      );
    }
    catch (FamilyAggregateConflictException)
    {
      await transaction.RollbackAsync(CancellationToken.None);
      throw;
    }
    catch (Exception exception) when (
      PostgreSqlConcurrencyExceptionClassifier.IsSerializableConflict(exception)
    )
    {
      await transaction.RollbackAsync(CancellationToken.None);

      throw FamilyAggregateConflictException.ConcurrentChange(exception);
    }
    catch (DbUpdateException exception)
    {
      await transaction.RollbackAsync(CancellationToken.None);

      throw new InvalidOperationException(
        "The member account invitation conflicts with an existing account or invitation.",
        exception
      );
    }
    catch
    {
      await transaction.RollbackAsync(CancellationToken.None);
      throw;
    }
  }

  public async Task<AuthenticationSessionResponse?> ActivateAsync(
    ActivateMemberAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(request);
    var username = AuthenticationCredentialPolicy.NormalizeUsername(request.Username);
    var normalizedEmail = AuthenticationCredentialPolicy.NormalizeEmail(request.Email);
    AuthenticationCredentialPolicy.ValidatePassword(request.Password);
    var activationCodeHash = HashActivationCode(request.ActivationCode);
    await using var transaction = await _database.Database.BeginTransactionAsync(
      IsolationLevel.Serializable,
      cancellationToken
    );

    try
    {
      var invitation = await _database.MemberAccountInvitations
        .FromSqlInterpolated(
          $"SELECT * FROM \"MemberAccountInvitations\" WHERE \"NormalizedEmail\" = {normalizedEmail} AND \"RedeemedAt\" IS NULL FOR UPDATE"
        )
        .SingleOrDefaultAsync(cancellationToken);
      var now = _timeProvider.GetUtcNow();

      if (
        invitation is null
        || invitation.ExpiresAt <= now
        || !CryptographicOperations.FixedTimeEquals(
          activationCodeHash,
          invitation.ActivationCodeHash
        )
      )
      {
        await transaction.RollbackAsync(CancellationToken.None);

        return null;
      }

      if (
        await _database.AuthenticationAccounts.AnyAsync(
          item => item.Username == username || item.NormalizedEmail == normalizedEmail,
          cancellationToken
        )
      )
      {
        throw new InvalidOperationException("The username or email address already has an account.");
      }

      var familyId = new FamilyId(invitation.FamilyId);
      var memberId = new FamilyMemberId(invitation.MemberId);
      var loadedFamily = await _familyRepository.LoadAsync(familyId, cancellationToken)
        ?? throw new InvalidOperationException("The invited family has not been initialized.");
      var family = loadedFamily.Root;
      var invitedMember = family.Members.SingleOrDefault(item => item.Id == memberId);

      if (
        invitedMember is null
        || !invitedMember.InvitationPending
        || !string.Equals(
          invitedMember.EmailAddress,
          invitation.EmailAddress,
          StringComparison.OrdinalIgnoreCase
        )
      )
      {
        throw new InvalidOperationException("The pending family invitation is inconsistent.");
      }

      var loadedEconomy = await _economyRepository.LoadAsync(family, cancellationToken)
        ?? throw new InvalidOperationException("The family economy has not been initialized.");
      var loadedProgression = await _progressionRepository.LoadAsync(family, cancellationToken)
        ?? throw new InvalidOperationException("The family progression has not been initialized.");
      var loadedHouse = await _houseRepository.LoadAsync(
        family,
        loadedProgression.Root.StoredLevel,
        cancellationToken
      ) ?? throw new InvalidOperationException("The family house has not been initialized.");
      family.AcceptInvitation(memberId, now);
      var economy = loadedEconomy.Root.SynchronizeMembers(family);
      loadedProgression.Root.SynchronizeParticipants(family);
      loadedHouse.Root.SynchronizeFamilyEntities(family);
      var passwordSalt = RandomNumberGenerator.GetBytes(16);
      var account = new AuthenticationAccountEntity
      {
        EmailAddress = invitation.EmailAddress,
        FamilyId = invitation.FamilyId,
        FamilyName = invitation.FamilyName,
        MemberId = invitation.MemberId,
        NormalizedEmail = normalizedEmail,
        PasswordHash = AuthenticationCredentialPolicy.HashPassword(
          request.Password,
          passwordSalt,
          AuthenticationSecurityPolicy.PasswordIterations
        ),
        PasswordIterations = AuthenticationSecurityPolicy.PasswordIterations,
        PasswordSalt = passwordSalt,
        Username = username,
      };
      invitation.RedeemedAt = now;
      _database.AuthenticationAccounts.Add(account);
      var result = await _unitOfWork.SaveAsync(
        new TypedAggregateSaveBatch(
          familyId,
          memberId,
          family: new PendingFamilySave(family, loadedFamily.Revision),
          familyEconomy: new PendingFamilyEconomySave(
            economy,
            loadedEconomy.Presentation,
            loadedEconomy.Revision
          ),
          familyProgression: new PendingFamilyProgressionSave(
            loadedProgression.Root,
            loadedProgression.Revision
          ),
          houseCustomization: new PendingHouseCustomizationSave(
            loadedHouse.Root,
            loadedHouse.Revision
          )
        ),
        cancellationToken
      );
      _outboxWriter.AddChanges(result, familyId);
      await _database.SaveChangesAsync(cancellationToken);
      await transaction.CommitAsync(cancellationToken);

      return new AuthenticationSessionResponse(
        account.FamilyId,
        account.FamilyName,
        account.MemberId
      );
    }
    catch (FamilyAggregateConflictException)
    {
      await transaction.RollbackAsync(CancellationToken.None);
      throw;
    }
    catch (Exception exception) when (
      PostgreSqlConcurrencyExceptionClassifier.IsSerializableConflict(exception)
    )
    {
      await transaction.RollbackAsync(CancellationToken.None);

      throw FamilyAggregateConflictException.ConcurrentChange(exception);
    }
    catch (DbUpdateException exception)
    {
      await transaction.RollbackAsync(CancellationToken.None);

      throw new InvalidOperationException(
        "The member account conflicts with an existing account.",
        exception
      );
    }
    catch
    {
      await transaction.RollbackAsync(CancellationToken.None);
      throw;
    }
  }

  private static string CreateActivationCode()
  {
    return Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
  }

  private static byte[] HashActivationCode(string activationCode)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(activationCode);
    var normalized = activationCode.Trim().ToUpperInvariant();

    return SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
  }

  private static GuardianAccess ParseGuardianAccess(MemberRole role, string? value)
  {
    if (role == MemberRole.Child)
    {
      return string.IsNullOrWhiteSpace(value) || value.Equals("supporter", StringComparison.OrdinalIgnoreCase)
        ? GuardianAccess.Supporter
        : throw new ArgumentException("Children cannot have guardian access.", nameof(value));
    }

    return value?.Trim().ToLowerInvariant() switch
    {
      null or "" or "supporter" => GuardianAccess.Supporter,
      "admin" => GuardianAccess.Admin,
      _ => throw new ArgumentException("The guardian access level is invalid.", nameof(value)),
    };
  }

  private static MemberRole ParseRole(string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    return value.Trim().ToLowerInvariant() switch
    {
      "child" => MemberRole.Child,
      "guardian" => MemberRole.Guardian,
      _ => throw new ArgumentException("The family member role is invalid.", nameof(value)),
    };
  }
}
