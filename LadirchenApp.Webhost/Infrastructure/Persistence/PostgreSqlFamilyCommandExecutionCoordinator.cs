using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class PostgreSqlFamilyCommandExecutionCoordinator : IFamilyCommandExecutionCoordinator
{
  private readonly LadirchenDbContext _database;
  private readonly TimeProvider _timeProvider;

  public PostgreSqlFamilyCommandExecutionCoordinator(
    LadirchenDbContext database,
    TimeProvider timeProvider
  )
  {
    _database = database;
    _timeProvider = timeProvider;
  }

  public async Task<FamilyCommandResult> ExecuteOnceAsync(
    FamilyCommand command,
    Func<CancellationToken, Task<FamilyCommandResult>> execute,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(command);
    ArgumentNullException.ThrowIfNull(execute);

    var fingerprint = CreateFingerprint(command);
    var completed = await FindCompletedAsync(command.CommandId, cancellationToken);

    if (completed is not null)
    {
      return Replay(command, fingerprint, completed);
    }

    await using var transaction = await _database.Database.BeginTransactionAsync(
      IsolationLevel.Serializable,
      cancellationToken
    );
    var now = _timeProvider.GetUtcNow();
    var receipt = new FamilyCommandReceiptEntity
    {
      ActorId = command.ActorId.Value,
      CommandId = command.CommandId,
      CommandType = command.CommandType,
      CreatedAt = now,
      FamilyId = command.FamilyId.Value,
      RequestFingerprint = fingerprint,
    };
    _database.FamilyCommandReceipts.Add(receipt);

    try
    {
      await _database.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
    {
      await RollbackSafelyAsync(transaction);
      _database.ChangeTracker.Clear();
      var duplicate = await FindCompletedAsync(command.CommandId, cancellationToken)
        ?? throw new InvalidOperationException(
          "The duplicate command receipt was not available after the conflicting insert.",
          exception
        );

      return Replay(command, fingerprint, duplicate);
    }
    catch (Exception exception) when (
      PostgreSqlConcurrencyExceptionClassifier.IsSerializableConflict(exception)
    )
    {
      await RollbackSafelyAsync(transaction);
      _database.ChangeTracker.Clear();

      throw FamilyAggregateConflictException.ConcurrentChange(exception);
    }

    try
    {
      var result = await execute(cancellationToken);
      receipt.ResultJson = Serialize(result);
      receipt.CompletedAt = _timeProvider.GetUtcNow();

      foreach (var change in result.ChangedAggregates)
      {
        _database.FamilyAggregateOutboxMessages.Add(
          new FamilyAggregateOutboxMessageEntity
          {
            AggregateType = change.AggregateType,
            CommandId = command.CommandId,
            FamilyId = change.FamilyId.Value,
            MessageId = Guid.NewGuid(),
            OccurredAt = receipt.CompletedAt.Value,
            Revision = change.Revision,
            UpdatedAt = change.UpdatedAt,
            UpdatedBy = change.UpdatedBy.Value,
          }
        );
      }

      await _database.SaveChangesAsync(cancellationToken);
      await transaction.CommitAsync(cancellationToken);

      return result;
    }
    catch (FamilyAggregateConflictException)
    {
      await RollbackSafelyAsync(transaction);
      throw;
    }
    catch (Exception exception) when (
      PostgreSqlConcurrencyExceptionClassifier.IsSerializableConflict(exception)
    )
    {
      await RollbackSafelyAsync(transaction);
      _database.ChangeTracker.Clear();

      throw FamilyAggregateConflictException.ConcurrentChange(exception);
    }
    catch
    {
      await RollbackSafelyAsync(transaction);
      throw;
    }
  }

  private async Task<FamilyCommandReceiptEntity?> FindCompletedAsync(
    Guid commandId,
    CancellationToken cancellationToken
  )
  {
    return await _database.FamilyCommandReceipts
      .AsNoTracking()
      .SingleOrDefaultAsync(
        item => item.CommandId == commandId && item.CompletedAt != null,
        cancellationToken
      );
  }

  private static FamilyCommandResult Replay(
    FamilyCommand command,
    string fingerprint,
    FamilyCommandReceiptEntity receipt
  )
  {
    if (
      receipt.FamilyId != command.FamilyId.Value
      || !string.Equals(receipt.ActorId, command.ActorId.Value, StringComparison.Ordinal)
      || !string.Equals(receipt.CommandType, command.CommandType, StringComparison.Ordinal)
      || !string.Equals(receipt.RequestFingerprint, fingerprint, StringComparison.Ordinal)
    )
    {
      throw new FamilyCommandException("The command ID has already been used for a different command.");
    }

    if (receipt.ResultJson is null)
    {
      throw new InvalidOperationException("The completed command receipt has no stored result.");
    }

    return Deserialize(receipt.ResultJson);
  }

  private static string CreateFingerprint(FamilyCommand command)
  {
    var request = string.Join(
      '\n',
      command.FamilyId.Value.ToString("D"),
      (command.AuthenticatedMemberId ?? command.ActorId).Value,
      command.ActorId.Value,
      command.CommandType,
      command.Payload.GetRawText()
    );

    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request)));
  }

  private static string Serialize(FamilyCommandResult result)
  {
    var stored = new StoredFamilyCommandResult(
      result.ChangedAggregates
        .Select(change => new StoredFamilyAggregateChange(
          change.AggregateType,
          change.FamilyId.Value,
          change.Revision,
          change.UpdatedAt,
          change.UpdatedBy.Value
        ))
        .ToArray()
    );

    return JsonSerializer.Serialize(stored);
  }

  private static FamilyCommandResult Deserialize(string json)
  {
    var stored = JsonSerializer.Deserialize<StoredFamilyCommandResult>(json)
      ?? throw new InvalidOperationException("The stored command result is invalid.");
    IReadOnlyList<FamilyAggregateChange> changes = stored.ChangedAggregates
      .Select(change => new FamilyAggregateChange(
        new FamilyId(change.FamilyId),
        change.AggregateType,
        change.Revision,
        new FamilyMemberId(change.UpdatedBy),
        change.UpdatedAt
      ))
      .ToArray();

    return new FamilyCommandResult(changes);
  }

  private static bool IsUniqueConstraintViolation(DbUpdateException exception)
  {
    return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
  }

  private static async Task RollbackSafelyAsync(IDbContextTransaction transaction)
  {
    try
    {
      await transaction.RollbackAsync(CancellationToken.None);
    }
    catch
    {
      // The original failure is more useful than a secondary rollback failure.
    }
  }

  private sealed record StoredFamilyCommandResult(
    IReadOnlyList<StoredFamilyAggregateChange> ChangedAggregates
  );

  private sealed record StoredFamilyAggregateChange(
    string AggregateType,
    Guid FamilyId,
    long Revision,
    DateTimeOffset UpdatedAt,
    string UpdatedBy
  );
}
