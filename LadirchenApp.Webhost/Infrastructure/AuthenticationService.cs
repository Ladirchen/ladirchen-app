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
  private readonly FamilyMembershipOutboxWriter _outboxWriter;
  private readonly TimeProvider _timeProvider;
  private readonly ITypedAggregateUnitOfWork _unitOfWork;

  public AuthenticationService(
    LadirchenDbContext database,
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    FamilyMembershipOutboxWriter outboxWriter
  )
  {
    _database = database;
    _unitOfWork = unitOfWork;
    _timeProvider = timeProvider;
    _outboxWriter = outboxWriter;
  }

  public async Task<bool> IsRegistrationAvailableAsync(CancellationToken cancellationToken)
  {
    return !await _database.BetaFamilyRegistrations
      .AsNoTracking()
      .AnyAsync(cancellationToken);
  }

  public async Task<AuthenticationSessionResponse?> AuthenticateAsync(
    AuthenticateAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(request);
    var identifier = request.Identifier?.Trim().ToLowerInvariant();

    if (
      string.IsNullOrEmpty(identifier)
      || string.IsNullOrWhiteSpace(request.Password)
      || request.Password.Length > AuthenticationSecurityPolicy.MaximumPasswordLength
    )
    {
      AuthenticationCredentialPolicy.PerformDummyPasswordHash(request.Password ?? string.Empty);

      return null;
    }

    var isEmailIdentifier = identifier.Contains('@', StringComparison.Ordinal);

    if (isEmailIdentifier)
    {
      try
      {
        identifier = AuthenticationCredentialPolicy.NormalizeEmail(identifier);
      }
      catch (ArgumentException)
      {
        AuthenticationCredentialPolicy.PerformDummyPasswordHash(request.Password);

        return null;
      }
    }

    await using var transaction = await _database.Database.BeginTransactionAsync(
      IsolationLevel.ReadCommitted,
      cancellationToken
    );

    try
    {
      var account = await LoadAccountForUpdateAsync(
        identifier,
        isEmailIdentifier,
        cancellationToken
      );

      if (account is null)
      {
        AuthenticationCredentialPolicy.PerformDummyPasswordHash(request.Password);
        await transaction.CommitAsync(cancellationToken);

        return null;
      }

      var now = _timeProvider.GetUtcNow();

      if (account.LockedUntilUtc > now)
      {
        AuthenticationCredentialPolicy.PerformDummyPasswordHash(request.Password);
        await transaction.CommitAsync(cancellationToken);

        return null;
      }

      var actualHash = AuthenticationCredentialPolicy.HashPassword(
        request.Password,
        account.PasswordSalt,
        account.PasswordIterations
      );

      if (!CryptographicOperations.FixedTimeEquals(actualHash, account.PasswordHash))
      {
        account.FailedLoginCount++;

        if (account.FailedLoginCount >= AuthenticationSecurityPolicy.MaximumFailedLoginAttempts)
        {
          account.FailedLoginCount = 0;
          account.LockedUntilUtc = now.Add(AuthenticationSecurityPolicy.AccountLockDuration);
        }

        await _database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return null;
      }

      if (account.FailedLoginCount != 0 || account.LockedUntilUtc is not null)
      {
        account.FailedLoginCount = 0;
        account.LockedUntilUtc = null;
        await _database.SaveChangesAsync(cancellationToken);
      }

      await transaction.CommitAsync(cancellationToken);

      return ToSession(account);
    }
    catch
    {
      await transaction.RollbackAsync(CancellationToken.None);
      throw;
    }
  }

  public async Task<AuthenticationSessionResponse> RegisterAsync(
    RegisterAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(request);
    ValidateRegistration(request);
    var username = AuthenticationCredentialPolicy.NormalizeUsername(request.Username);
    var normalizedEmail = AuthenticationCredentialPolicy.NormalizeEmail(request.Email);
    await using var transaction = await _database.Database.BeginTransactionAsync(
      IsolationLevel.Serializable,
      cancellationToken
    );

    try
    {
      if (
        await _database.BetaFamilyRegistrations.AnyAsync(cancellationToken)
        || await _database.AuthenticationAccounts.AnyAsync(
          item => item.Username == username || item.NormalizedEmail == normalizedEmail,
          cancellationToken
        )
      )
      {
        throw new InvalidOperationException("The beta family has already been registered.");
      }

      var actorId = new FamilyMemberId(request.MemberId);
      var familyId = new FamilyId(request.FamilyId);
      var batch = InitialFamilyAggregateFactory.Create(
        familyId,
        actorId,
        request.GuardianName.Trim(),
        request.Email.Trim()
      );
      var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
      var account = new AuthenticationAccountEntity
      {
        Username = username,
        EmailAddress = request.Email.Trim(),
        NormalizedEmail = normalizedEmail,
        FamilyId = request.FamilyId,
        FamilyName = request.FamilyName.Trim(),
        MemberId = request.MemberId,
        PasswordHash = AuthenticationCredentialPolicy.HashPassword(
          request.Password,
          salt,
          AuthenticationSecurityPolicy.PasswordIterations
        ),
        PasswordSalt = salt,
        PasswordIterations = AuthenticationSecurityPolicy.PasswordIterations,
      };
      _database.BetaFamilyRegistrations.Add(
        new BetaFamilyRegistrationEntity
        {
          FamilyId = request.FamilyId,
          Id = 1,
          RegisteredAt = _timeProvider.GetUtcNow(),
        }
      );

      try
      {
        var result = await _unitOfWork.SaveAsync(batch, cancellationToken);
        _database.AuthenticationAccounts.Add(account);
        _outboxWriter.AddChanges(result, familyId);
        await _database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
      }
      catch (FamilyAggregateConflictException exception)
      {
        throw new InvalidOperationException("The family has already been initialized.", exception);
      }
      catch (Exception exception) when (
        PostgreSqlConcurrencyExceptionClassifier.IsSerializableConflict(exception)
      )
      {
        throw FamilyAggregateConflictException.ConcurrentChange(exception);
      }
      catch (DbUpdateException exception)
      {
        throw new InvalidOperationException("The account or family has already been registered.", exception);
      }

      return ToSession(account);
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
    catch
    {
      await transaction.RollbackAsync(CancellationToken.None);
      throw;
    }
  }

  private static AuthenticationSessionResponse ToSession(AuthenticationAccountEntity account)
  {
    return new AuthenticationSessionResponse(account.FamilyId, account.FamilyName, account.MemberId);
  }

  private Task<AuthenticationAccountEntity?> LoadAccountForUpdateAsync(
    string identifier,
    bool isEmailIdentifier,
    CancellationToken cancellationToken
  )
  {
    return isEmailIdentifier
      ? _database.AuthenticationAccounts
        .FromSqlInterpolated(
          $"SELECT * FROM \"AuthenticationAccounts\" WHERE \"NormalizedEmail\" = {identifier} FOR UPDATE"
        )
        .SingleOrDefaultAsync(cancellationToken)
      : _database.AuthenticationAccounts
        .FromSqlInterpolated(
          $"SELECT * FROM \"AuthenticationAccounts\" WHERE \"Username\" = {identifier} FOR UPDATE"
        )
        .SingleOrDefaultAsync(cancellationToken);
  }

  private static void ValidateRegistration(RegisterAccountRequest request)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(request.Username);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.Password);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.FamilyName);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.GuardianName);

    _ = AuthenticationCredentialPolicy.NormalizeUsername(request.Username);
    _ = AuthenticationCredentialPolicy.NormalizeEmail(request.Email);
    AuthenticationCredentialPolicy.ValidatePassword(request.Password);

    if (request.FamilyId == Guid.Empty || string.IsNullOrWhiteSpace(request.MemberId))
    {
      throw new ArgumentException("A family and guardian identity are required.");
    }

    if (request.FamilyName.Trim().Length < 2 || request.GuardianName.Trim().Length < 2)
    {
      throw new ArgumentException("The family and guardian names must contain at least two characters.");
    }
  }
}
