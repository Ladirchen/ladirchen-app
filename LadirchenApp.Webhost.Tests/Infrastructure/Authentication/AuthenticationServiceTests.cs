using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Models;

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
      TimeProvider.System
    );
    var familyId = Guid.NewGuid();
    const string memberId = "guardian-1";
    const string password = "correct horse battery staple";
    var registration = new RegisterAccountRequest(
      familyId,
      "Sun Family",
      "Alex",
      memberId,
      password,
      "  Alex  "
    );

    var registeredSession = await service.RegisterAsync(registration, CancellationToken.None);
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
      new AuthenticateAccountRequest("incorrect password", "alex"),
      CancellationToken.None
    );
    var authenticatedSession = await service.AuthenticateAsync(
      new AuthenticateAccountRequest(password, "ALEX"),
      CancellationToken.None
    );

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
    await Assert.That(account.PasswordHash.SequenceEqual(Encoding.UTF8.GetBytes(password))).IsFalse();
    await Assert.That(account.PasswordSalt).IsNotEmpty();
    await Assert.That(account.PasswordIterations).IsEqualTo(AuthenticationSecurityPolicy.PasswordIterations);
    await Assert.That(rejectedSession).IsNull();
    await Assert.That(authenticatedSession?.FamilyId).IsEqualTo(familyId);
    await Assert.That(authenticatedSession?.MemberId).IsEqualTo(memberId);
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
