using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
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

          return Task.FromResult(
            new FamilyCommandResult(Array.Empty<FamilyAggregateChange>())
          );
        },
        CancellationToken.None
      );

      await Assert.That(replayedResult.ChangedAggregates.Count).IsEqualTo(1);
      await Assert.That(replayedResult.ChangedAggregates[0].Revision).IsEqualTo(
        firstResult.ChangedAggregates[0].Revision
      );
      await Assert.That(replayedResult.ChangedAggregates[0].AggregateType).IsEqualTo("savings");
      await Assert.That(replayedResult.ChangedAggregates[0].UpdatedBy).IsEqualTo(command.ActorId);
      await Assert.That(executions).IsEqualTo(1);
      await Assert.That(await restartedDatabase.FamilyCommandReceipts.CountAsync()).IsEqualTo(1);
      await Assert.That(await restartedDatabase.FamilyAggregateOutboxMessages.CountAsync()).IsEqualTo(1);
      var outboxMessage = await restartedDatabase.FamilyAggregateOutboxMessages
        .AsNoTracking()
        .SingleAsync();
      await Assert.That(outboxMessage.CommandId).IsEqualTo(command.CommandId);
    }
  }

  [Test]
  public async Task DifferentChangesMayPublishTheSameReadModelRevision()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var command = CreateCommand();

    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    database.FamilyAggregateOutboxMessages.Add(
      new FamilyAggregateOutboxMessageEntity
      {
        AggregateType = "savings",
        FamilyId = command.FamilyId.Value,
        MessageId = Guid.NewGuid(),
        OccurredAt = DateTimeOffset.UtcNow,
        Revision = 2,
        UpdatedAt = DateTimeOffset.UtcNow,
        UpdatedBy = command.ActorId.Value,
      }
    );
    await database.SaveChangesAsync();
    var coordinator = new PostgreSqlFamilyCommandExecutionCoordinator(
      database,
      TimeProvider.System
    );

    var result = await coordinator.ExecuteOnceAsync(
      command,
      _ => Task.FromResult(CreateResult(command)),
      CancellationToken.None
    );

    await Assert.That(result.ChangedAggregates.Count).IsEqualTo(1);
    await Assert.That(
      await database.FamilyAggregateOutboxMessages.CountAsync(message =>
        message.FamilyId == command.FamilyId.Value
        && message.AggregateType == "savings"
        && message.Revision == 2
      )
    ).IsEqualTo(2);
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
      var unitOfWork = new DatabaseTypedAggregateUnitOfWork(database, TimeProvider.System);

      await Assert.That(async () =>
        await coordinator.ExecuteOnceAsync(
          command,
          async cancellationToken =>
          {
            await unitOfWork.SaveAsync(
              InitialFamilyAggregateFactory.Create(
                command.FamilyId,
                command.ActorId,
                "Guardian"
              ),
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
      var unitOfWork = new DatabaseTypedAggregateUnitOfWork(
        restartedDatabase,
        TimeProvider.System
      );
      var familyRepository = new DatabaseFamilyRepository(restartedDatabase, unitOfWork);
      await Assert.That(
        await familyRepository.LoadAsync(command.FamilyId, CancellationToken.None)
      ).IsNull();
      var result = await coordinator.ExecuteOnceAsync(
        command,
        async cancellationToken =>
        {
          var saved = await unitOfWork.SaveAsync(
            InitialFamilyAggregateFactory.Create(
              command.FamilyId,
              command.ActorId,
              "Guardian"
            ),
            cancellationToken
          );
          var family = saved.Family
            ?? throw new InvalidOperationException("The family aggregate was not saved.");

          return new FamilyCommandResult(
            [CreateChange(command, "family-profile", family)]
          );
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
        _ => Task.FromResult(
          new FamilyCommandResult(Array.Empty<FamilyAggregateChange>())
        ),
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
      var unitOfWork = new DatabaseTypedAggregateUnitOfWork(
        setupDatabase,
        TimeProvider.System
      );
      await unitOfWork.SaveAsync(
        InitialFamilyAggregateFactory.Create(familyId, memberId, "Guardian"),
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
    var firstUnitOfWork = new DatabaseTypedAggregateUnitOfWork(firstDatabase, TimeProvider.System);
    var secondUnitOfWork = new DatabaseTypedAggregateUnitOfWork(secondDatabase, TimeProvider.System);
    var firstFamilyRepository = new DatabaseFamilyRepository(firstDatabase, firstUnitOfWork);
    var secondFamilyRepository = new DatabaseFamilyRepository(secondDatabase, secondUnitOfWork);
    var firstEconomyRepository = new DatabaseFamilyEconomyRepository(firstDatabase, firstUnitOfWork);
    var secondEconomyRepository = new DatabaseFamilyEconomyRepository(secondDatabase, secondUnitOfWork);
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
        var family = await firstFamilyRepository.LoadAsync(familyId, cancellationToken)
          ?? throw new InvalidOperationException("The family aggregate was not found.");
        var economy = await firstEconomyRepository.LoadAsync(family.Root, cancellationToken)
          ?? throw new InvalidOperationException("The family economy was not found.");
        firstReadCompleted.SetResult();
        await secondReadCompleted.Task.WaitAsync(cancellationToken);
        economy.Root.UpdateContributionSettings(
          memberId,
          new ContributionSettings(new Percentage(15)),
          DateTimeOffset.Parse("2026-10-29T12:00:00Z")
        );
        var changed = await firstEconomyRepository.SaveAsync(
          economy.Root,
          economy.Presentation,
          economy.Revision,
          memberId,
          cancellationToken
        );

        return new FamilyCommandResult(
          [CreateChange(firstCommand, "contributions", changed)]
        );
      },
      CancellationToken.None
    );
    var secondExecution = secondCoordinator.ExecuteOnceAsync(
      secondCommand,
      async cancellationToken =>
      {
        var family = await secondFamilyRepository.LoadAsync(familyId, cancellationToken)
          ?? throw new InvalidOperationException("The family aggregate was not found.");
        _ = await secondEconomyRepository.LoadAsync(family.Root, cancellationToken)
          ?? throw new InvalidOperationException("The family economy was not found.");
        secondReadCompleted.SetResult();
        await firstReadCompleted.Task.WaitAsync(cancellationToken);
        family.Root.RenameMember(
          memberId,
          memberId,
          "Renamed guardian",
          DateTimeOffset.Parse("2026-10-29T12:00:00Z")
        );
        var changed = await secondFamilyRepository.SaveAsync(
          family.Root,
          family.Revision,
          memberId,
          cancellationToken
        );

        return new FamilyCommandResult(
          [CreateChange(secondCommand, "family-profile", changed)]
        );
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
    return new FamilyCommandResult(
      [
        new FamilyAggregateChange(
          command.FamilyId,
          "savings",
          2,
          command.ActorId,
          DateTimeOffset.Parse("2026-10-29T12:00:00Z")
        ),
      ]
    );
  }

  private static FamilyAggregateChange CreateChange<TAggregate>(
    FamilyCommand command,
    string aggregateType,
    LoadedAggregate<TAggregate> saved
  )
  {
    return new FamilyAggregateChange(
      command.FamilyId,
      aggregateType,
      saved.Revision,
      saved.Audit.UpdatedBy,
      saved.Audit.UpdatedAt
    );
  }

  private static FamilyAggregateChange CreateChange(
    FamilyCommand command,
    string aggregateType,
    LoadedFamilyEconomy saved
  )
  {
    return new FamilyAggregateChange(
      command.FamilyId,
      aggregateType,
      saved.Revision,
      saved.Audit.UpdatedBy,
      saved.Audit.UpdatedAt
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
