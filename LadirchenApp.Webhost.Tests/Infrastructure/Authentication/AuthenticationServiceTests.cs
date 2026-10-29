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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

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
    var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
    var service = new AuthenticationService(
      database,
      repository,
      new NoOpFamilyAggregateChangePublisher(),
      TimeProvider.System,
      NullLogger<AuthenticationService>.Instance
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
    var initializedSnapshots = await repository.GetManyAsync(
      new FamilyId(familyId),
      [
        "contributions",
        "family-profile",
        "family-progression",
        "home-customization",
        "reward-shop",
        "savings",
      ],
      CancellationToken.None
    );
    var account = await database.AuthenticationAccounts.AsNoTracking().SingleAsync();
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
    await Assert.That(initializedSnapshots.Count).IsEqualTo(6);
    await Assert.That(initializedSnapshots["contributions"].SchemaVersion).IsEqualTo(2);
    await Assert.That(initializedSnapshots["savings"].SchemaVersion).IsEqualTo(4);
    await Assert.That(initializedSnapshots["family-progression"].SchemaVersion).IsEqualTo(4);

    foreach (var snapshot in initializedSnapshots.Values)
    {
      await Assert.That(snapshot.SchemaVersion)
        .IsEqualTo(FamilyAggregateSchema.GetVersion(snapshot.AggregateType));
    }

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
