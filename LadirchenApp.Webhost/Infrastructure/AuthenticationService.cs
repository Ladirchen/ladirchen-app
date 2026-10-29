using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class AuthenticationService
{
  private readonly LadirchenDbContext _database;
  private readonly ILogger<AuthenticationService> _logger;
  private readonly IFamilyAggregateChangePublisher _publisher;
  private readonly IFamilyAggregateSnapshotRepository _repository;
  private readonly TimeProvider _timeProvider;

  public AuthenticationService(
    LadirchenDbContext database,
    IFamilyAggregateSnapshotRepository repository,
    IFamilyAggregateChangePublisher publisher,
    TimeProvider timeProvider,
    ILogger<AuthenticationService> logger
  )
  {
    _database = database;
    _repository = repository;
    _publisher = publisher;
    _timeProvider = timeProvider;
    _logger = logger;
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
    await using var transaction = await _database.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      if (await _database.AuthenticationAccounts.AnyAsync(item => item.Username == username, cancellationToken))
      {
        throw new InvalidOperationException("The username already exists.");
      }

      var actorId = new FamilyMemberId(request.MemberId);
      var familyId = new FamilyId(request.FamilyId);
      var saves = InitialFamilyAggregateFactory.Create(familyId, actorId, request.GuardianName.Trim());
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
      IReadOnlyList<FamilyAggregateSnapshot> snapshots;

      try
      {
        snapshots = await _repository.SaveBatchAsync(saves, cancellationToken);
        _database.AuthenticationAccounts.Add(account);
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

      await PublishBestEffortAsync(snapshots);

      return ToSession(account);
    }
    catch
    {
      await transaction.RollbackAsync(CancellationToken.None);
      throw;
    }
  }

  private async Task PublishBestEffortAsync(IEnumerable<FamilyAggregateSnapshot> snapshots)
  {
    foreach (var snapshot in snapshots)
    {
      try
      {
        await _publisher.PublishAsync(snapshot, CancellationToken.None);
      }
      catch (Exception exception)
      {
        _logger.LogWarning(
          exception,
          "Family aggregate {AggregateType} was committed but its realtime notification failed.",
          snapshot.AggregateType
        );
      }
    }
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
