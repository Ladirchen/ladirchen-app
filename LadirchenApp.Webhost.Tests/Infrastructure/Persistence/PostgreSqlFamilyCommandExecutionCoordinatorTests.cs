using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class PostgreSqlFamilyCommandExecutionCoordinatorTests
{
  [Test]
  public async Task ACompletedCommandIsReplayedWithoutExecutingOrEnqueuingItAgain()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var command = CreateCommand();
    var executions = 0;
    FamilyCommandResult firstResult;

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var coordinator = new PostgreSqlFamilyCommandExecutionCoordinator(database, TimeProvider.System);
      firstResult = await coordinator.ExecuteOnceAsync(
        command,
        _ =>
        {
          executions++;

          return Task.FromResult(CreateResult(command));
        },
        CancellationToken.None
      );
    }

    await using (var restartedDatabase = new LadirchenDbContext(options))
    {
      var coordinator = new PostgreSqlFamilyCommandExecutionCoordinator(
        restartedDatabase,
        TimeProvider.System
      );
      var replayedResult = await coordinator.ExecuteOnceAsync(
        command,
        _ =>
        {
          executions++;

          return Task.FromResult(new FamilyCommandResult([]));
        },
        CancellationToken.None
      );

      await Assert.That(replayedResult.ChangedAggregates.Count).IsEqualTo(1);
      await Assert.That(replayedResult.ChangedAggregates[0].Revision).IsEqualTo(
        firstResult.ChangedAggregates[0].Revision
      );
      await Assert.That(replayedResult.ChangedAggregates[0].State.GetProperty("balance").GetInt32())
        .IsEqualTo(10);
      await Assert.That(executions).IsEqualTo(1);
      await Assert.That(await restartedDatabase.FamilyCommandReceipts.CountAsync()).IsEqualTo(1);
      await Assert.That(await restartedDatabase.FamilyAggregateOutboxMessages.CountAsync()).IsEqualTo(1);
    }
  }

  [Test]
  public async Task ConcurrentDeliveryOfTheSameCommandExecutesOnlyOnce()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var command = CreateCommand();
    var firstExecutionStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var completeFirstExecution = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var executions = 0;

    await using (var setupDatabase = new LadirchenDbContext(options))
    {
      await setupDatabase.Database.MigrateAsync();
    }

    await using var firstDatabase = new LadirchenDbContext(options);
    await using var secondDatabase = new LadirchenDbContext(options);
    var firstCoordinator = new PostgreSqlFamilyCommandExecutionCoordinator(
      firstDatabase,
      TimeProvider.System
    );
    var secondCoordinator = new PostgreSqlFamilyCommandExecutionCoordinator(
      secondDatabase,
      TimeProvider.System
    );
    var firstExecution = firstCoordinator.ExecuteOnceAsync(
      command,
      async _ =>
      {
        Interlocked.Increment(ref executions);
        firstExecutionStarted.SetResult();
        await completeFirstExecution.Task;

        return CreateResult(command);
      },
      CancellationToken.None
    );
    await firstExecutionStarted.Task;
    var secondExecution = secondCoordinator.ExecuteOnceAsync(
      command,
      _ =>
      {
        Interlocked.Increment(ref executions);

        return Task.FromResult(CreateResult(command));
      },
      CancellationToken.None
    );
    completeFirstExecution.SetResult();
    var results = await Task.WhenAll(firstExecution, secondExecution);

    await Assert.That(executions).IsEqualTo(1);
    await Assert.That(results[0].ChangedAggregates[0].Revision).IsEqualTo(2);
    await Assert.That(results[1].ChangedAggregates[0].Revision).IsEqualTo(2);
  }

  [Test]
  public async Task AFailedCommandRollsBackItsReceiptAndCanBeRetried()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var command = CreateCommand();

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var coordinator = new PostgreSqlFamilyCommandExecutionCoordinator(database, TimeProvider.System);
      var repository = new DatabaseFamilyAggregateSnapshotRepository(database, TimeProvider.System);
      using var state = JsonDocument.Parse("""{"balance":10}""");

      await Assert.That(async () =>
        await coordinator.ExecuteOnceAsync(
          command,
          async cancellationToken =>
          {
            await repository.SaveAsync(
              new SaveFamilyAggregateCommand(
                command.FamilyId,
                "savings",
                0,
                state.RootElement,
                command.ActorId
              ),
              1,
              cancellationToken
            );

            throw new InvalidOperationException("The command failed.");
          },
          CancellationToken.None
        )
      ).Throws<InvalidOperationException>();
    }

    await using (var restartedDatabase = new LadirchenDbContext(options))
    {
      var coordinator = new PostgreSqlFamilyCommandExecutionCoordinator(
        restartedDatabase,
        TimeProvider.System
      );
      var repository = new DatabaseFamilyAggregateSnapshotRepository(
        restartedDatabase,
        TimeProvider.System
      );
      await Assert.That(
        await repository.GetAsync(command.FamilyId, "savings", CancellationToken.None)
      ).IsNull();
      using var state = JsonDocument.Parse("""{"balance":10}""");
      var result = await coordinator.ExecuteOnceAsync(
        command,
        async cancellationToken =>
        {
          var snapshot = await repository.SaveAsync(
            new SaveFamilyAggregateCommand(
              command.FamilyId,
              "savings",
              0,
              state.RootElement,
              command.ActorId
            ),
            1,
            cancellationToken
          );

          return new FamilyCommandResult([snapshot]);
        },
        CancellationToken.None
      );

      await Assert.That(result.ChangedAggregates.Count).IsEqualTo(1);
      await Assert.That(await restartedDatabase.FamilyCommandReceipts.CountAsync()).IsEqualTo(1);
      await Assert.That(await restartedDatabase.FamilyAggregateOutboxMessages.CountAsync()).IsEqualTo(1);
    }
  }

  [Test]
  public async Task ReusingACommandIdForDifferentInputIsRejected()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var command = CreateCommand();

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var coordinator = new PostgreSqlFamilyCommandExecutionCoordinator(database, TimeProvider.System);
      await coordinator.ExecuteOnceAsync(
        command,
        _ => Task.FromResult(CreateResult(command)),
        CancellationToken.None
      );
    }

    using var changedPayload = JsonDocument.Parse("""{"amount":20}""");
    var conflictingCommand = command with
    {
      Payload = changedPayload.RootElement.Clone()
    };
    await using var restartedDatabase = new LadirchenDbContext(options);
    var restartedCoordinator = new PostgreSqlFamilyCommandExecutionCoordinator(
      restartedDatabase,
      TimeProvider.System
    );

    await Assert.That(async () =>
      await restartedCoordinator.ExecuteOnceAsync(
        conflictingCommand,
        _ => Task.FromResult(new FamilyCommandResult([])),
        CancellationToken.None
      )
    ).Throws<FamilyCommandException>();
  }

  [Test]
  public async Task SerializableWriteSkewIsReportedAsAnApplicationConflict()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var memberId = new FamilyMemberId("child-1");

    await using (var setupDatabase = new LadirchenDbContext(options))
    {
      await setupDatabase.Database.MigrateAsync();
      var repository = new DatabaseFamilyAggregateSnapshotRepository(
        setupDatabase,
        TimeProvider.System
      );
      using var leftState = JsonDocument.Parse("""{"value":1}""");
      using var rightState = JsonDocument.Parse("""{"value":1}""");
      await repository.SaveBatchAsync(
        [
          new PendingFamilyAggregateSave(
            new SaveFamilyAggregateCommand(
              familyId,
              "left",
              0,
              leftState.RootElement,
              memberId
            ),
            1
          ),
          new PendingFamilyAggregateSave(
            new SaveFamilyAggregateCommand(
              familyId,
              "right",
              0,
              rightState.RootElement,
              memberId
            ),
            1
          ),
        ],
        CancellationToken.None
      );
    }

    using var payload = JsonDocument.Parse("""{"value":2}""");
    var firstCommand = new FamilyCommand(
      Guid.NewGuid(),
      familyId,
      memberId,
      "test.write-right",
      payload.RootElement.Clone()
    );
    var secondCommand = firstCommand with
    {
      CommandId = Guid.NewGuid(),
      CommandType = "test.write-left",
    };
    var firstReadCompleted = new TaskCompletionSource(
      TaskCreationOptions.RunContinuationsAsynchronously
    );
    var secondReadCompleted = new TaskCompletionSource(
      TaskCreationOptions.RunContinuationsAsynchronously
    );
    await using var firstDatabase = new LadirchenDbContext(options);
    await using var secondDatabase = new LadirchenDbContext(options);
    var firstRepository = new DatabaseFamilyAggregateSnapshotRepository(
      firstDatabase,
      TimeProvider.System
    );
    var secondRepository = new DatabaseFamilyAggregateSnapshotRepository(
      secondDatabase,
      TimeProvider.System
    );
    var firstCoordinator = new PostgreSqlFamilyCommandExecutionCoordinator(
      firstDatabase,
      TimeProvider.System
    );
    var secondCoordinator = new PostgreSqlFamilyCommandExecutionCoordinator(
      secondDatabase,
      TimeProvider.System
    );
    var firstExecution = firstCoordinator.ExecuteOnceAsync(
      firstCommand,
      async cancellationToken =>
      {
        _ = await firstRepository.GetAsync(familyId, "left", cancellationToken);
        _ = await firstRepository.GetAsync(familyId, "right", cancellationToken);
        firstReadCompleted.SetResult();
        await secondReadCompleted.Task.WaitAsync(cancellationToken);
        using var changedState = JsonDocument.Parse("""{"value":2}""");
        var changed = await firstRepository.SaveAsync(
          new SaveFamilyAggregateCommand(
            familyId,
            "right",
            1,
            changedState.RootElement,
            memberId
          ),
          1,
          cancellationToken
        );

        return new FamilyCommandResult([changed]);
      },
      CancellationToken.None
    );
    var secondExecution = secondCoordinator.ExecuteOnceAsync(
      secondCommand,
      async cancellationToken =>
      {
        _ = await secondRepository.GetAsync(familyId, "left", cancellationToken);
        _ = await secondRepository.GetAsync(familyId, "right", cancellationToken);
        secondReadCompleted.SetResult();
        await firstReadCompleted.Task.WaitAsync(cancellationToken);
        using var changedState = JsonDocument.Parse("""{"value":2}""");
        var changed = await secondRepository.SaveAsync(
          new SaveFamilyAggregateCommand(
            familyId,
            "left",
            1,
            changedState.RootElement,
            memberId
          ),
          1,
          cancellationToken
        );

        return new FamilyCommandResult([changed]);
      },
      CancellationToken.None
    );
    Exception? firstFailure = null;
    Exception? secondFailure = null;

    try
    {
      await firstExecution;
    }
    catch (Exception exception)
    {
      firstFailure = exception;
    }

    try
    {
      await secondExecution;
    }
    catch (Exception exception)
    {
      secondFailure = exception;
    }

    var firstIsConflict = firstFailure is FamilyAggregateConflictException;
    var secondIsConflict = secondFailure is FamilyAggregateConflictException;

    await Assert.That(firstIsConflict ^ secondIsConflict).IsTrue();
    await Assert.That(firstFailure is null ^ secondFailure is null).IsTrue();
    await Assert.That(ContainsSerializationFailure(firstFailure ?? secondFailure)).IsTrue();
  }

  private static bool ContainsSerializationFailure(Exception? exception)
  {
    var current = exception;

    while (current is not null)
    {
      if (current is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure })
      {
        return true;
      }

      current = current.InnerException;
    }

    return false;
  }

  private static FamilyCommand CreateCommand()
  {
    using var payload = JsonDocument.Parse("""{"amount":10}""");

    return new FamilyCommand(
      Guid.NewGuid(),
      new FamilyId(Guid.NewGuid()),
      new FamilyMemberId("child-1"),
      "savings.deposit",
      payload.RootElement.Clone()
    );
  }

  private static FamilyCommandResult CreateResult(FamilyCommand command)
  {
    using var state = JsonDocument.Parse("""{"balance":10}""");

    return new FamilyCommandResult(
      [
        new FamilyAggregateSnapshot(
          command.FamilyId,
          "savings",
          1,
          2,
          state.RootElement.Clone(),
          command.ActorId,
          DateTimeOffset.Parse("2026-10-29T12:00:00Z")
        ),
      ]
    );
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