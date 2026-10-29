using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Models;
using LadirchenApp.Webhost.Tests.Application.Commands;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Authentication;

public sealed class AuthenticationServiceTests
{
  [Test]
  public async Task RegisteredCredentialsAreHashedAndAuthenticateAgainstThePersistentAccount()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    var unitOfWork = new DatabaseTypedAggregateUnitOfWork(database, TimeProvider.System);
    var familyRepository = new DatabaseFamilyRepository(database, unitOfWork);
    var economyRepository = new DatabaseFamilyEconomyRepository(database, unitOfWork);
    var progressionRepository = new DatabaseFamilyProgressionRepository(database, unitOfWork);
    var houseRepository = new DatabaseHouseCustomizationRepository(database, unitOfWork);
    var service = new AuthenticationService(
      database,
      unitOfWork,
      TimeProvider.System,
      new FamilyMembershipOutboxWriter(database, TimeProvider.System)
    );
    var familyId = Guid.NewGuid();
    const string memberId = "guardian-1";
    const string password = "correct horse battery staple";
    var registration = new RegisterAccountRequest(
      familyId,
      "alex@example.com",
      "Sun Family",
      "Alex",
      memberId,
      password,
      "  Alex  "
    );

    var availableBeforeRegistration = await service.IsRegistrationAvailableAsync(
      CancellationToken.None
    );
    var registeredSession = await service.RegisterAsync(registration, CancellationToken.None);
    var availableAfterRegistration = await service.IsRegistrationAvailableAsync(
      CancellationToken.None
    );
    var typedFamilyId = new FamilyId(familyId);
    var family = await familyRepository.LoadAsync(typedFamilyId, CancellationToken.None)
      ?? throw new InvalidOperationException();
    var economy = await economyRepository.LoadAsync(family.Root, CancellationToken.None)
      ?? throw new InvalidOperationException();
    var progression = await progressionRepository.LoadAsync(family.Root, CancellationToken.None)
      ?? throw new InvalidOperationException();
    var house = await houseRepository.LoadAsync(
      family.Root,
      progression.Root.StoredLevel,
      CancellationToken.None
    ) ?? throw new InvalidOperationException();
    var account = await database.AuthenticationAccounts.AsNoTracking().SingleAsync();
    var outboxMessages = await database.FamilyAggregateOutboxMessages
      .AsNoTracking()
      .ToArrayAsync();
    var rejectedSession = await service.AuthenticateAsync(
      new AuthenticateAccountRequest("alex", "incorrect password"),
      CancellationToken.None
    );
    var whitespacePasswordSession = await service.AuthenticateAsync(
      new AuthenticateAccountRequest("alex", "   "),
      CancellationToken.None
    );
    var oversizedPasswordSession = await service.AuthenticateAsync(
      new AuthenticateAccountRequest(
        "alex",
        new string('x', AuthenticationSecurityPolicy.MaximumPasswordLength + 1)
      ),
      CancellationToken.None
    );
    var malformedEmailSession = await service.AuthenticateAsync(
      new AuthenticateAccountRequest("member@invalid", password),
      CancellationToken.None
    );
    var authenticatedSession = await service.AuthenticateAsync(
      new AuthenticateAccountRequest("ALEX@EXAMPLE.COM", password),
      CancellationToken.None
    );

    await Assert.That(availableBeforeRegistration).IsTrue();
    await Assert.That(availableAfterRegistration).IsFalse();
    await Assert.That(registeredSession.FamilyId).IsEqualTo(familyId);
    await Assert.That(registeredSession.MemberId).IsEqualTo(memberId);
    await Assert.That(family.Revision).IsEqualTo(1);
    await Assert.That(economy.Revision).IsEqualTo(1);
    await Assert.That(progression.Revision).IsEqualTo(1);
    await Assert.That(house.Revision).IsEqualTo(1);
    await Assert.That(outboxMessages.Select(item => item.AggregateType)).IsEquivalentTo(
      [
        "contributions",
        "family-profile",
        "family-progression",
        "home-customization",
        "reward-shop",
        "savings",
      ]
    );
    await Assert.That(outboxMessages.All(item => item.CommandId is null)).IsTrue();

    await Assert.That(account.Username).IsEqualTo("alex");
    await Assert.That(account.NormalizedEmail).IsEqualTo("alex@example.com");
    await Assert.That(family.Root.Members.Single(item => item.Id.Value == memberId).EmailAddress)
      .IsEqualTo("alex@example.com");
    await Assert.That(account.PasswordHash.SequenceEqual(Encoding.UTF8.GetBytes(password))).IsFalse();
    await Assert.That(account.PasswordSalt).IsNotEmpty();
    await Assert.That(account.PasswordIterations).IsEqualTo(AuthenticationSecurityPolicy.PasswordIterations);
    await Assert.That(rejectedSession).IsNull();
    await Assert.That(whitespacePasswordSession).IsNull();
    await Assert.That(oversizedPasswordSession).IsNull();
    await Assert.That(malformedEmailSession).IsNull();
    await Assert.That(authenticatedSession?.FamilyId).IsEqualTo(familyId);
    await Assert.That(authenticatedSession?.MemberId).IsEqualTo(memberId);
  }

  [Test]
  public async Task ConcurrentBootstrapRegistrationCreatesExactlyOneBetaFamily()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());

    await using (var migrationDatabase = new LadirchenDbContext(options))
    {
      await migrationDatabase.Database.MigrateAsync();
    }

    await using var firstDatabase = new LadirchenDbContext(options);
    await using var secondDatabase = new LadirchenDbContext(options);
    var firstService = CreateService(firstDatabase, TimeProvider.System);
    var secondService = CreateService(secondDatabase, TimeProvider.System);
    var first = TryRegisterAsync(
      firstService,
      Registration(Guid.NewGuid(), "first@example.com", "first")
    );
    var second = TryRegisterAsync(
      secondService,
      Registration(Guid.NewGuid(), "second@example.com", "second")
    );
    var outcomes = await Task.WhenAll(first, second);

    await using var verificationDatabase = new LadirchenDbContext(options);
    var registrationCount = await verificationDatabase.BetaFamilyRegistrations.CountAsync();
    var familyCount = await verificationDatabase.Families.CountAsync();
    var accountCount = await verificationDatabase.AuthenticationAccounts.CountAsync();

    await Assert.That(outcomes.Count(item => item is null)).IsEqualTo(1);
    await Assert.That(outcomes.Count(item =>
      item is InvalidOperationException or FamilyAggregateConflictException
    )).IsEqualTo(1);
    await Assert.That(registrationCount).IsEqualTo(1);
    await Assert.That(familyCount).IsEqualTo(1);
    await Assert.That(accountCount).IsEqualTo(1);
  }

  [Test]
  public async Task LoginLockoutAppliesToUsernameAndEmailIdentifiers()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    var now = new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);
    var service = CreateService(database, new FixedTimeProvider(now));
    var familyId = Guid.NewGuid();
    await service.RegisterAsync(
      Registration(familyId, "guardian@example.com", "guardian"),
      CancellationToken.None
    );

    for (var attempt = 0; attempt < AuthenticationSecurityPolicy.MaximumFailedLoginAttempts; attempt++)
    {
      var identifier = attempt % 2 == 0 ? "guardian" : "GUARDIAN@EXAMPLE.COM";
      var failed = await service.AuthenticateAsync(
        new AuthenticateAccountRequest(identifier, "incorrect password"),
        CancellationToken.None
      );
      await Assert.That(failed).IsNull();
    }

    var locked = await service.AuthenticateAsync(
      new AuthenticateAccountRequest("guardian@example.com", "secure password"),
      CancellationToken.None
    );
    var account = await database.AuthenticationAccounts.SingleAsync();
    var lockedUntil = account.LockedUntilUtc;
    var afterLockout = CreateService(
      database,
      new FixedTimeProvider(now.Add(AuthenticationSecurityPolicy.AccountLockDuration).AddMinutes(1))
    );
    var unlocked = await afterLockout.AuthenticateAsync(
      new AuthenticateAccountRequest("GUARDIAN", "secure password"),
      CancellationToken.None
    );

    await Assert.That(locked).IsNull();
    await Assert.That(lockedUntil).IsEqualTo(
      now.Add(AuthenticationSecurityPolicy.AccountLockDuration)
    );
    await Assert.That(unlocked?.FamilyId).IsEqualTo(familyId);
  }

  [Test]
  public async Task ParallelFailedLoginsCannotLoseLockoutUpdates()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var now = new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);
    var timeProvider = new FixedTimeProvider(now);

    await using (var setupDatabase = new LadirchenDbContext(options))
    {
      await setupDatabase.Database.MigrateAsync();
      var setupService = CreateService(setupDatabase, timeProvider);
      await setupService.RegisterAsync(
        Registration(Guid.NewGuid(), "guardian@example.com", "guardian"),
        CancellationToken.None
      );
    }

    var attempts = Enumerable.Range(
      0,
      AuthenticationSecurityPolicy.MaximumFailedLoginAttempts
    ).Select(async attempt =>
    {
      await using var attemptDatabase = new LadirchenDbContext(options);
      var attemptService = CreateService(attemptDatabase, timeProvider);
      var identifier = attempt % 2 == 0 ? "guardian" : "GUARDIAN@EXAMPLE.COM";

      return await attemptService.AuthenticateAsync(
        new AuthenticateAccountRequest(identifier, "incorrect password"),
        CancellationToken.None
      );
    });
    var sessions = await Task.WhenAll(attempts);

    await using var verificationDatabase = new LadirchenDbContext(options);
    var account = await verificationDatabase.AuthenticationAccounts
      .AsNoTracking()
      .SingleAsync();
    await using var loginDatabase = new LadirchenDbContext(options);
    var lockedSession = await CreateService(loginDatabase, timeProvider).AuthenticateAsync(
      new AuthenticateAccountRequest("guardian", "secure password"),
      CancellationToken.None
    );

    await Assert.That(sessions.All(item => item is null)).IsTrue();
    await Assert.That(account.FailedLoginCount).IsEqualTo(0);
    await Assert.That(account.LockedUntilUtc).IsEqualTo(
      now.Add(AuthenticationSecurityPolicy.AccountLockDuration)
    );
    await Assert.That(lockedSession).IsNull();
  }

  private static AuthenticationService CreateService(
    LadirchenDbContext database,
    TimeProvider timeProvider
  )
  {
    var unitOfWork = new DatabaseTypedAggregateUnitOfWork(database, timeProvider);

    return new AuthenticationService(
      database,
      unitOfWork,
      timeProvider,
      new FamilyMembershipOutboxWriter(database, timeProvider)
    );
  }

  private static RegisterAccountRequest Registration(
    Guid familyId,
    string email,
    string username
  )
  {
    return new RegisterAccountRequest(
      familyId,
      email,
      "Sun Family",
      "Guardian",
      $"{username}-member",
      "secure password",
      username
    );
  }

  private static async Task<Exception?> TryRegisterAsync(
    AuthenticationService service,
    RegisterAccountRequest request
  )
  {
    try
    {
      await service.RegisterAsync(request, CancellationToken.None);

      return null;
    }
    catch (Exception exception) when (
      exception is InvalidOperationException or FamilyAggregateConflictException
    )
    {
      return exception;
    }
  }

  private static DbContextOptions<LadirchenDbContext> CreateOptions(string connectionString)
  {
    return new DbContextOptionsBuilder<LadirchenDbContext>()
      .UseNpgsql(connectionString)
      .Options;
  }

  private static async Task<PostgreSqlContainer> StartPostgreSqlAsync()
  {
    var postgres = new PostgreSqlBuilder("postgres:18-alpine")
      .Build();
    await postgres.StartAsync();

    return postgres;
  }
}