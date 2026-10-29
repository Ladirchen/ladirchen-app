using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class DatabaseFamilyAggregateSnapshotRepositoryTests
{
  [Test]
  public async Task SavedSnapshotSurvivesARepositoryRestart()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var memberId = new FamilyMemberId(Guid.NewGuid().ToString("N"));

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
      using var state = JsonDocument.Parse("""{"name":"Persistent family"}""");

      await repository.SaveAsync(
        new SaveFamilyAggregateCommand(familyId, "family-profile", 0, state.RootElement, memberId),
        1,
        CancellationToken.None
      );
    }

    await using (var restartedDatabase = new LadirchenDbContext(options))
    {
      var restartedRepository = new DatabaseFamilyAggregateSnapshotRepository(
        restartedDatabase,
        TimeProvider.System
      );
      var snapshot = await restartedRepository.GetAsync(
        familyId,
        "family-profile",
        CancellationToken.None
      );

      await Assert.That(snapshot).IsNotNull();
      await Assert.That(snapshot!.Revision).IsEqualTo(1);
      await Assert.That(snapshot.State.GetProperty("name").GetString()).IsEqualTo("Persistent family");
    }
  }

  [Test]
  public async Task StaleRevisionIsRejectedWithoutOverwritingTheCurrentSnapshot()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var memberId = new FamilyMemberId(Guid.NewGuid().ToString("N"));

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
      using var initialState = JsonDocument.Parse("""{"value":1}""");
      using var updatedState = JsonDocument.Parse("""{"value":2}""");

      await repository.SaveAsync(
        new SaveFamilyAggregateCommand(familyId, "savings", 0, initialState.RootElement, memberId),
        2,
        CancellationToken.None
      );
      await repository.SaveAsync(
        new SaveFamilyAggregateCommand(familyId, "savings", 1, updatedState.RootElement, memberId),
        2,
        CancellationToken.None
      );
    }

    await using (var staleDatabase = new LadirchenDbContext(options))
    {
      var staleRepository = new DatabaseFamilyAggregateSnapshotRepository(staleDatabase, TimeProvider.System);
      using var staleState = JsonDocument.Parse("""{"value":99}""");
      var command = new SaveFamilyAggregateCommand(
        familyId,
        "savings",
        1,
        staleState.RootElement,
        memberId
      );

      await Assert.That(async () =>
        await staleRepository.SaveAsync(command, 2, CancellationToken.None)
      ).Throws<FamilyAggregateConflictException>();
    }

    await using (var verificationDatabase = new LadirchenDbContext(options))
    {
      var verificationRepository = new DatabaseFamilyAggregateSnapshotRepository(
        verificationDatabase,
        TimeProvider.System
      );
      var snapshot = await verificationRepository.GetAsync(familyId, "savings", CancellationToken.None);

      await Assert.That(snapshot).IsNotNull();
      await Assert.That(snapshot!.Revision).IsEqualTo(2);
      await Assert.That(snapshot.State.GetProperty("value").GetInt32()).IsEqualTo(2);
    }
  }

  [Test]
  public async Task BatchConflictDoesNotPartiallyPersistAnEarlierAggregate()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var memberId = new FamilyMemberId(Guid.NewGuid().ToString("N"));

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
      using var firstState = JsonDocument.Parse("""{"value":1}""");
      using var secondState = JsonDocument.Parse("""{"value":10}""");
      await repository.SaveBatchAsync(
        [
          new PendingFamilyAggregateSave(
            new SaveFamilyAggregateCommand(familyId, "first", 0, firstState.RootElement, memberId),
            1
          ),
          new PendingFamilyAggregateSave(
            new SaveFamilyAggregateCommand(familyId, "second", 0, secondState.RootElement, memberId),
            1
          ),
        ],
        CancellationToken.None
      );
    }

    await using (var database = new LadirchenDbContext(options))
    {
      var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
      using var currentSecondState = JsonDocument.Parse("""{"value":20}""");
      await repository.SaveAsync(
        new SaveFamilyAggregateCommand(
          familyId,
          "second",
          1,
          currentSecondState.RootElement,
          memberId
        ),
        1,
        CancellationToken.None
      );
    }

    await using (var database = new LadirchenDbContext(options))
    {
      var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
      using var changedFirstState = JsonDocument.Parse("""{"value":2}""");
      using var staleSecondState = JsonDocument.Parse("""{"value":99}""");
      var saves = new[]
      {
        new PendingFamilyAggregateSave(
          new SaveFamilyAggregateCommand(familyId, "first", 1, changedFirstState.RootElement, memberId),
          1
        ),
        new PendingFamilyAggregateSave(
          new SaveFamilyAggregateCommand(familyId, "second", 1, staleSecondState.RootElement, memberId),
          1
        ),
      };

      await Assert.That(async () =>
        await repository.SaveBatchAsync(saves, CancellationToken.None)
      ).Throws<FamilyAggregateConflictException>();
    }

    await using (var database = new LadirchenDbContext(options))
    {
      var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
      var first = await repository.GetAsync(familyId, "first", CancellationToken.None);
      var second = await repository.GetAsync(familyId, "second", CancellationToken.None);

      await Assert.That(first).IsNotNull();
      await Assert.That(first!.Revision).IsEqualTo(1);
      await Assert.That(first.State.GetProperty("value").GetInt32()).IsEqualTo(1);
      await Assert.That(second).IsNotNull();
      await Assert.That(second!.Revision).IsEqualTo(2);
      await Assert.That(second.State.GetProperty("value").GetInt32()).IsEqualTo(20);
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
