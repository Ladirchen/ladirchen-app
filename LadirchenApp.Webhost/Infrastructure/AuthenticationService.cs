using System;
using System.Data;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Models;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class AuthenticationService
{
  private readonly LadirchenDbContext _database;
  private readonly TimeProvider _timeProvider;
  private readonly ITypedAggregateUnitOfWork _unitOfWork;

  public AuthenticationService(
    LadirchenDbContext database,
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider
  )
  {
    _database = database;
    _unitOfWork = unitOfWork;
    _timeProvider = timeProvider;
  }

  public async Task<AuthenticationSessionResponse?> AuthenticateAsync(
    AuthenticateAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(request);
    var username = NormalizeUsername(request.Username);
    var account = await _database.AuthenticationAccounts.SingleOrDefaultAsync(
      item => item.Username == username,
      cancellationToken
    );

    if (account is null)
    {
      PerformDummyPasswordHash(request.Password);

      return null;
    }

    var now = _timeProvider.GetUtcNow();

    if (account.LockedUntilUtc > now)
    {
      PerformDummyPasswordHash(request.Password);

      return null;
    }

    var actualHash = HashPassword(request.Password, account.PasswordSalt, account.PasswordIterations);

    if (!CryptographicOperations.FixedTimeEquals(actualHash, account.PasswordHash))
    {
      account.FailedLoginCount++;

      if (account.FailedLoginCount >= AuthenticationSecurityPolicy.MaximumFailedLoginAttempts)
      {
        account.FailedLoginCount = 0;
        account.LockedUntilUtc = now.Add(AuthenticationSecurityPolicy.AccountLockDuration);
      }

      await _database.SaveChangesAsync(cancellationToken);

      return null;
    }

    if (account.FailedLoginCount != 0 || account.LockedUntilUtc is not null)
    {
      account.FailedLoginCount = 0;
      account.LockedUntilUtc = null;
      await _database.SaveChangesAsync(cancellationToken);
    }

    return ToSession(account);
  }

  public async Task<AuthenticationSessionResponse> RegisterAsync(
    RegisterAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(request);
    ValidateRegistration(request);
    var username = NormalizeUsername(request.Username);
    await using var transaction = await _database.Database.BeginTransactionAsync(
      IsolationLevel.Serializable,
      cancellationToken
    );

    try
    {
      if (await _database.AuthenticationAccounts.AnyAsync(item => item.Username == username, cancellationToken))
      {
        throw new InvalidOperationException("The username already exists.");
      }

      var actorId = new FamilyMemberId(request.MemberId);
      var familyId = new FamilyId(request.FamilyId);
      var batch = InitialFamilyAggregateFactory.Create(familyId, actorId, request.GuardianName.Trim());
      var salt = RandomNumberGenerator.GetBytes(16);
      var account = new AuthenticationAccountEntity
      {
        Username = username,
        FamilyId = request.FamilyId,
        FamilyName = request.FamilyName.Trim(),
        MemberId = request.MemberId,
        PasswordHash = HashPassword(
          request.Password,
          salt,
          AuthenticationSecurityPolicy.PasswordIterations
        ),
        PasswordSalt = salt,
        PasswordIterations = AuthenticationSecurityPolicy.PasswordIterations,
      };

      try
      {
        var result = await _unitOfWork.SaveAsync(batch, cancellationToken);
        _database.AuthenticationAccounts.Add(account);
        AddInitialReadModelMessages(result, familyId);
        await _database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
      }
      catch (FamilyAggregateConflictException exception)
      {
        throw new InvalidOperationException("The family has already been initialized.", exception);
      }
      catch (DbUpdateException exception)
      {
        throw new InvalidOperationException("The account or family has already been registered.", exception);
      }

      return ToSession(account);
    }
    catch
    {
      await transaction.RollbackAsync(CancellationToken.None);
      throw;
    }
  }

  private void AddInitialReadModelMessages(
    TypedAggregateSaveResult result,
    FamilyId familyId
  )
  {
    var family = result.Family
      ?? throw new InvalidOperationException("The initial family was not persisted.");
    var economy = result.FamilyEconomy
      ?? throw new InvalidOperationException("The initial family economy was not persisted.");
    var progression = result.FamilyProgression
      ?? throw new InvalidOperationException("The initial family progression was not persisted.");
    var house = result.HouseCustomization
      ?? throw new InvalidOperationException("The initial house customization was not persisted.");
    AddOutboxMessage(familyId, "family-profile", family.Revision, family.Audit);
    AddOutboxMessage(familyId, "contributions", economy.Revision, economy.Audit);
    AddOutboxMessage(familyId, "savings", economy.Revision, economy.Audit);
    AddOutboxMessage(familyId, "reward-shop", economy.Revision, economy.Audit);
    AddOutboxMessage(
      familyId,
      "family-progression",
      progression.Revision,
      progression.Audit
    );
    AddOutboxMessage(
      familyId,
      "home-customization",
      house.Revision,
      house.Audit
    );
  }

  private void AddOutboxMessage(
    FamilyId familyId,
    string aggregateType,
    long revision,
    AggregateAudit audit
  )
  {
    _database.FamilyAggregateOutboxMessages.Add(
      new FamilyAggregateOutboxMessageEntity
      {
        AggregateType = aggregateType,
        FamilyId = familyId.Value,
        MessageId = Guid.NewGuid(),
        OccurredAt = _timeProvider.GetUtcNow(),
        Revision = revision,
        UpdatedAt = audit.UpdatedAt,
        UpdatedBy = audit.UpdatedBy.Value,
      }
    );
  }

  private static AuthenticationSessionResponse ToSession(AuthenticationAccountEntity account)
  {
    return new AuthenticationSessionResponse(account.FamilyId, account.FamilyName, account.MemberId);
  }

  private static byte[] HashPassword(string password, byte[] salt, int iterations)
  {
    return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
  }

  private static void PerformDummyPasswordHash(string password)
  {
    _ = HashPassword(password, new byte[16], AuthenticationSecurityPolicy.PasswordIterations);
  }

  private static string NormalizeUsername(string username)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(username);

    return username.Trim().ToLowerInvariant();
  }

  private static void ValidateRegistration(RegisterAccountRequest request)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(request.Username);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.FamilyName);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.GuardianName);

    if (request.Username.Trim().Length < 3 || request.Password.Length < 6)
    {
      throw new ArgumentException("The username or password does not meet the minimum length.");
    }

    if (request.FamilyName.Trim().Length < 2 || request.GuardianName.Trim().Length < 2)
    {
      throw new ArgumentException("The family and guardian names must contain at least two characters.");
    }
  }
}
