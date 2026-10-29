using System;
using System.Data;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Npgsql;

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
  public async Task GetManyReturnsOnlyTheRequestedFamilyAggregates()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var otherFamilyId = new FamilyId(Guid.NewGuid());
    var memberId = new FamilyMemberId(Guid.NewGuid().ToString("N"));

    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
    using var profileState = JsonDocument.Parse("""{"name":"Family"}""");
    using var savingsState = JsonDocument.Parse("""{"goals":[]}""");
    using var homeState = JsonDocument.Parse("""{"items":[]}""");
    using var otherProfileState = JsonDocument.Parse("""{"name":"Other family"}""");

    await repository.SaveBatchAsync(
      [
        CreateSave(familyId, "family-profile", profileState, memberId),
        CreateSave(familyId, "savings", savingsState, memberId),
        CreateSave(familyId, "home-customization", homeState, memberId),
        CreateSave(otherFamilyId, "family-profile", otherProfileState, memberId),
      ],
      CancellationToken.None
    );

    var snapshots = await repository.GetManyAsync(
      familyId,
      ["family-profile", "savings", "missing"],
      CancellationToken.None
    );

    await Assert.That(snapshots.Count).IsEqualTo(2);
    await Assert.That(snapshots.ContainsKey("family-profile")).IsTrue();
    await Assert.That(snapshots.ContainsKey("savings")).IsTrue();
    await Assert.That(snapshots.ContainsKey("home-customization")).IsFalse();
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

  [Test]
  public async Task ChangedReadDependencyRejectsTheWholeWriteBatch()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var memberId = new FamilyMemberId(Guid.NewGuid().ToString("N"));

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
      using var homeState = JsonDocument.Parse("""{"item":"chair"}""");
      using var progressionState = JsonDocument.Parse("""{"houseLevel":2}""");
      await repository.SaveBatchAsync(
        [
          CreateSave(familyId, "home-customization", homeState, memberId),
          CreateSave(familyId, "family-progression", progressionState, memberId),
        ],
        CancellationToken.None
      );
    }

    await using var commandDatabase = new LadirchenDbContext(options);
    var commandRepository = new DatabaseFamilyAggregateSnapshotRepository(
      commandDatabase,
      TimeProvider.System
    );
    var readProgression = await commandRepository.GetAsync(
      familyId,
      "family-progression",
      CancellationToken.None
    );
    await Assert.That(readProgression).IsNotNull();

    await using (var concurrentDatabase = new LadirchenDbContext(options))
    {
      var concurrentRepository = new DatabaseFamilyAggregateSnapshotRepository(
        concurrentDatabase,
        TimeProvider.System
      );
      using var lowerProgressionState = JsonDocument.Parse("""{"houseLevel":1}""");
      await concurrentRepository.SaveAsync(
        new SaveFamilyAggregateCommand(
          familyId,
          "family-progression",
          readProgression!.Revision,
          lowerProgressionState.RootElement,
          memberId
        ),
        1,
        CancellationToken.None
      );
    }

    using var changedHomeState = JsonDocument.Parse("""{"item":"sofa"}""");
    var homeSave = CreateSave(
      familyId,
      "home-customization",
      changedHomeState,
      memberId,
      expectedRevision: 1
    );
    var progressionGuard = new FamilyAggregateRevisionGuard(
      familyId,
      "family-progression",
      readProgression!.Revision
    );

    await Assert.That(async () =>
      await commandRepository.SaveBatchAsync(
        [homeSave],
        [progressionGuard],
        CancellationToken.None
      )
    ).Throws<FamilyAggregateConflictException>();

    await using var verificationDatabase = new LadirchenDbContext(options);
    var verificationRepository = new DatabaseFamilyAggregateSnapshotRepository(
      verificationDatabase,
      TimeProvider.System
    );
    var home = await verificationRepository.GetAsync(
      familyId,
      "home-customization",
      CancellationToken.None
    );

    await Assert.That(home).IsNotNull();
    await Assert.That(home!.Revision).IsEqualTo(1);
    await Assert.That(home.State.GetProperty("item").GetString()).IsEqualTo("chair");

    var currentProgression = await commandRepository.GetAsync(
      familyId,
      "family-progression",
      CancellationToken.None
    );
    await Assert.That(currentProgression).IsNotNull();
    var changed = await commandRepository.SaveBatchAsync(
      [homeSave],
      [
        new FamilyAggregateRevisionGuard(
          familyId,
          "family-progression",
          currentProgression!.Revision
        ),
      ],
      CancellationToken.None
    );

    await Assert.That(changed.Count).IsEqualTo(1);
    await Assert.That(changed[0].Revision).IsEqualTo(2);
    await Assert.That(changed[0].State.GetProperty("item").GetString()).IsEqualTo("sofa");
  }

  [Test]
  public async Task NonConcurrencyDatabaseFailuresAreNotReportedAsRevisionConflicts()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var memberId = new FamilyMemberId(Guid.NewGuid().ToString("N"));
    var aggregateType = new string('x', 101);

    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
    using var state = JsonDocument.Parse("""{"value":1}""");

    await Assert.That(async () =>
      await repository.SaveAsync(
        new SaveFamilyAggregateCommand(
          familyId,
          aggregateType,
          0,
          state.RootElement,
          memberId
        ),
        1,
        CancellationToken.None
      )
    ).Throws<DbUpdateException>();
  }

  [Test]
  public async Task ConcurrentInsertInAnAmbientTransactionDoesNotQueryTheAbortedTransaction()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var connectionString = postgres.GetConnectionString();
    var options = CreateOptions(connectionString);
    var familyId = new FamilyId(Guid.NewGuid());
    var memberId = new FamilyMemberId(Guid.NewGuid().ToString("N"));

    await using (var setupDatabase = new LadirchenDbContext(options))
    {
      await setupDatabase.Database.MigrateAsync();
    }

    await using var firstDatabase = new LadirchenDbContext(options);
    await using var secondDatabase = new LadirchenDbContext(options);
    await firstDatabase.Database.OpenConnectionAsync();
    await secondDatabase.Database.OpenConnectionAsync();
    var secondProcessId = ((NpgsqlConnection)secondDatabase.Database.GetDbConnection()).ProcessID;
    await using var firstTransaction = await firstDatabase.Database.BeginTransactionAsync(
      IsolationLevel.ReadCommitted
    );
    await using var secondTransaction = await secondDatabase.Database.BeginTransactionAsync(
      IsolationLevel.ReadCommitted
    );
    var firstRepository = new DatabaseFamilyAggregateSnapshotRepository(
      firstDatabase,
      TimeProvider.System
    );
    var secondRepository = new DatabaseFamilyAggregateSnapshotRepository(
      secondDatabase,
      TimeProvider.System
    );
    using var firstState = JsonDocument.Parse("""{"value":1}""");
    using var secondState = JsonDocument.Parse("""{"value":2}""");
    await firstRepository.SaveAsync(
      new SaveFamilyAggregateCommand(
        familyId,
        "savings",
        0,
        firstState.RootElement,
        memberId
      ),
      1,
      CancellationToken.None
    );
    var conflictingSave = secondRepository.SaveAsync(
      new SaveFamilyAggregateCommand(
        familyId,
        "savings",
        0,
        secondState.RootElement,
        memberId
      ),
      1,
      CancellationToken.None
    );
    await WaitForBlockingLockAsync(connectionString, secondProcessId);
    await firstTransaction.CommitAsync();
    Exception? failure = null;

    try
    {
      await conflictingSave;
    }
    catch (Exception exception)
    {
      failure = exception;
    }

    await Assert.That(failure is FamilyAggregateConflictException).IsTrue();
    var conflict = (FamilyAggregateConflictException)failure!;
    await Assert.That(conflict.AggregateType).IsEqualTo("savings");
    await Assert.That(conflict.ExpectedRevision).IsEqualTo(0);
    await Assert.That(conflict.ActualRevision).IsNull();
    await secondTransaction.RollbackAsync();
  }

  private static PendingFamilyAggregateSave CreateSave(
    FamilyId familyId,
    string aggregateType,
    JsonDocument state,
    FamilyMemberId memberId,
    long expectedRevision = 0
  )
  {
    return new PendingFamilyAggregateSave(
      new SaveFamilyAggregateCommand(
        familyId,
        aggregateType,
        expectedRevision,
        state.RootElement,
        memberId
      ),
      1
    );
  }

  private static DbContextOptions<LadirchenDbContext> CreateOptions(string connectionString)
  {
    return new DbContextOptionsBuilder<LadirchenDbContext>()
      .UseNpgsql(connectionString)
      .Options;
  }

  private static async Task WaitForBlockingLockAsync(string connectionString, int processId)
  {
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    await using var command = new NpgsqlCommand(
      "SELECT cardinality(pg_blocking_pids(@process_id)) > 0",
      connection
    );
    command.Parameters.AddWithValue("process_id", processId);
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

    while (!timeout.IsCancellationRequested)
    {
      var blocked = await command.ExecuteScalarAsync(timeout.Token);

      if (blocked is true)
      {
        return;
      }

      await Task.Delay(TimeSpan.FromMilliseconds(20), timeout.Token);
    }

    throw new TimeoutException("The concurrent aggregate insert did not wait for the expected lock.");
  }

  private static async Task<PostgreSqlContainer> StartPostgreSqlAsync()
  {
    var postgres = new PostgreSqlBuilder("postgres:18-alpine")
      .Build();
    await postgres.StartAsync();

    return postgres;
  }
}