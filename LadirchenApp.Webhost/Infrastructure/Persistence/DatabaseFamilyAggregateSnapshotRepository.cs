using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class DatabaseFamilyAggregateSnapshotRepository : IFamilyAggregateSnapshotRepository
{
  private readonly LadirchenDbContext _database;
  private readonly TimeProvider _timeProvider;

  public DatabaseFamilyAggregateSnapshotRepository(LadirchenDbContext database, TimeProvider timeProvider)
  {
    _database = database;
    _timeProvider = timeProvider;
  }

  public async Task<IReadOnlyList<FamilyId>> ListFamilyIdsAsync(CancellationToken cancellationToken)
  {
    var familyIds = await _database.FamilyAggregateSnapshots
      .AsNoTracking()
      .Select(item => item.FamilyId)
      .Distinct()
      .ToArrayAsync(cancellationToken);

    return familyIds.Select(value => new FamilyId(value)).ToArray();
  }

  public async Task<FamilyAggregateSnapshot?> GetAsync(
    FamilyId familyId,
    string aggregateType,
    CancellationToken cancellationToken
  )
  {
    var snapshots = await GetManyAsync(familyId, [aggregateType], cancellationToken);
    snapshots.TryGetValue(aggregateType, out var snapshot);

    return snapshot;
  }

  public async Task<IReadOnlyDictionary<string, FamilyAggregateSnapshot>> GetManyAsync(
    FamilyId familyId,
    IReadOnlyCollection<string> aggregateTypes,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(aggregateTypes);

    if (aggregateTypes.Count == 0)
    {
      return new Dictionary<string, FamilyAggregateSnapshot>(StringComparer.Ordinal);
    }

    var requestedTypes = aggregateTypes.Distinct(StringComparer.Ordinal).ToArray();

    if (requestedTypes.Any(string.IsNullOrWhiteSpace))
    {
      throw new ArgumentException("Aggregate types must not be empty.", nameof(aggregateTypes));
    }

    var entities = await _database.FamilyAggregateSnapshots
      .AsNoTracking()
      .Where(item => item.FamilyId == familyId.Value && requestedTypes.Contains(item.AggregateType))
      .ToArrayAsync(cancellationToken);

    return entities.ToDictionary(
      item => item.AggregateType,
      ToSnapshot,
      StringComparer.Ordinal
    );
  }

  public async Task<FamilyAggregateSnapshot> SaveAsync(
    SaveFamilyAggregateCommand command,
    int schemaVersion,
    CancellationToken cancellationToken
  )
  {
    var results = await SaveBatchAsync([new PendingFamilyAggregateSave(command, schemaVersion)], cancellationToken);

    return results[0];
  }

  public async Task<IReadOnlyList<FamilyAggregateSnapshot>> SaveBatchAsync(
    IReadOnlyList<PendingFamilyAggregateSave> saves,
    CancellationToken cancellationToken
  )
  {
    return await SaveBatchAsync(saves, [], cancellationToken);
  }

  public async Task<IReadOnlyList<FamilyAggregateSnapshot>> SaveBatchAsync(
    IReadOnlyList<PendingFamilyAggregateSave> saves,
    IReadOnlyList<FamilyAggregateRevisionGuard> revisionGuards,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(saves);
    ArgumentNullException.ThrowIfNull(revisionGuards);

    if (saves.Count == 0 && revisionGuards.Count == 0)
    {
      return [];
    }

    var saveKeys = saves
      .Select(item => (item.Command.FamilyId.Value, item.Command.AggregateType))
      .ToHashSet();

    if (saveKeys.Count != saves.Count)
    {
      throw new ArgumentException("An aggregate may only be saved once per transaction.", nameof(saves));
    }

    var guardKeys = revisionGuards
      .Select(item => (item.FamilyId.Value, item.AggregateType))
      .ToHashSet();

    if (guardKeys.Count != revisionGuards.Count)
    {
      throw new ArgumentException(
        "An aggregate may only be guarded once per transaction.",
        nameof(revisionGuards)
      );
    }

    if (guardKeys.Overlaps(saveKeys))
    {
      throw new ArgumentException(
        "A revision guard must reference an aggregate that is not included in the writes.",
        nameof(revisionGuards)
      );
    }

    if (revisionGuards.Any(item => string.IsNullOrWhiteSpace(item.AggregateType)))
    {
      throw new ArgumentException("Aggregate types must not be empty.", nameof(revisionGuards));
    }

    if (revisionGuards.Any(item => item.ExpectedRevision < 0))
    {
      throw new ArgumentOutOfRangeException(
        nameof(revisionGuards),
        "Expected revisions must not be negative."
      );
    }

    IDbContextTransaction? transaction = null;

    try
    {
      if (_database.Database.CurrentTransaction is null)
      {
        transaction = await _database.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
      }
      else if (
        revisionGuards.Count > 0
        && _database.Database.CurrentTransaction.GetDbTransaction().IsolationLevel
          != IsolationLevel.Serializable
      )
      {
        throw new InvalidOperationException(
          "Revision guards require a serializable database transaction."
        );
      }

      var targetKeys = saveKeys.Concat(guardKeys).ToHashSet();
      var familyIds = targetKeys.Select(item => item.Value).Distinct().ToArray();
      var aggregateTypes = targetKeys.Select(item => item.AggregateType).Distinct().ToArray();

      foreach (
        var entry in _database.ChangeTracker
          .Entries<FamilyAggregateSnapshotEntity>()
          .Where(item => targetKeys.Contains((item.Entity.FamilyId, item.Entity.AggregateType)))
      )
      {
        entry.State = EntityState.Detached;
      }

      var loadedEntities = await _database.FamilyAggregateSnapshots
        .Where(
          item => familyIds.Contains(item.FamilyId) && aggregateTypes.Contains(item.AggregateType)
        )
        .ToArrayAsync(cancellationToken);
      var entitiesByKey = loadedEntities
        .Where(item => targetKeys.Contains((item.FamilyId, item.AggregateType)))
        .ToDictionary(item => (item.FamilyId, item.AggregateType));

      foreach (var guard in revisionGuards)
      {
        entitiesByKey.TryGetValue((guard.FamilyId.Value, guard.AggregateType), out var guardedEntity);
        var actualRevision = guardedEntity?.Revision ?? 0;

        if (actualRevision != guard.ExpectedRevision)
        {
          throw new FamilyAggregateConflictException(
            guard.AggregateType,
            guard.ExpectedRevision,
            actualRevision
          );
        }
      }

      var validated = new List<(PendingFamilyAggregateSave Save, FamilyAggregateSnapshotEntity? Entity)>(
        saves.Count
      );

      foreach (var save in saves)
      {
        var command = save.Command;
        entitiesByKey.TryGetValue((command.FamilyId.Value, command.AggregateType), out var entity);
        var actualRevision = entity?.Revision ?? 0;

        if (actualRevision != command.ExpectedRevision)
        {
          throw new FamilyAggregateConflictException(command.AggregateType, command.ExpectedRevision, actualRevision);
        }

        validated.Add((save, entity));
      }

      var entities = new List<FamilyAggregateSnapshotEntity>(saves.Count);

      foreach (var (save, existingEntity) in validated)
      {
        var command = save.Command;
        var entity = existingEntity;

        if (entity is null)
        {
          entity = new FamilyAggregateSnapshotEntity
          {
            FamilyId = command.FamilyId.Value,
            AggregateType = command.AggregateType,
          };
          _database.FamilyAggregateSnapshots.Add(entity);
        }
        else
        {
          _database.Entry(entity).Property(item => item.Revision).OriginalValue = command.ExpectedRevision;
        }

        entity.SchemaVersion = save.SchemaVersion;
        entity.Revision = checked(command.ExpectedRevision + 1);
        entity.StateJson = command.State.GetRawText();
        entity.UpdatedBy = command.UpdatedBy.Value;
        entity.UpdatedAt = _timeProvider.GetUtcNow();
        entities.Add(entity);
      }

      await _database.SaveChangesAsync(cancellationToken);

      if (transaction is not null)
      {
        await transaction.CommitAsync(cancellationToken);
      }

      return entities.Select(ToSnapshot).ToArray();
    }
    catch (DbUpdateConcurrencyException exception)
    {
      var canReadActualRevision = await RollbackAndDisposeOwnedTransactionAsync(transaction);
      transaction = null;

      throw await CreateConflictAsync(
        saves,
        exception,
        canReadActualRevision,
        cancellationToken
      );
    }
    catch (DbUpdateException exception) when (
      PostgreSqlConcurrencyExceptionClassifier.IsAggregateSnapshotInsertConflict(exception)
    )
    {
      var canReadActualRevision = await RollbackAndDisposeOwnedTransactionAsync(transaction);
      transaction = null;

      throw await CreateConflictAsync(
        saves,
        exception,
        canReadActualRevision,
        cancellationToken
      );
    }
    catch (Exception exception) when (
      PostgreSqlConcurrencyExceptionClassifier.IsSerializableConflict(exception)
    )
    {
      await RollbackAndDisposeOwnedTransactionAsync(transaction);
      transaction = null;
      _database.ChangeTracker.Clear();

      throw FamilyAggregateConflictException.ConcurrentChange(exception);
    }
    finally
    {
      if (transaction is not null)
      {
        await transaction.DisposeAsync();
      }
    }
  }

  private async Task<FamilyAggregateConflictException> CreateConflictAsync(
    IReadOnlyList<PendingFamilyAggregateSave> saves,
    Exception exception,
    bool canReadActualRevision,
    CancellationToken cancellationToken
  )
  {
    _database.ChangeTracker.Clear();

    if (saves.Count == 0)
    {
      return FamilyAggregateConflictException.ConcurrentChange(exception);
    }

    var first = saves[0].Command;

    if (!canReadActualRevision)
    {
      return FamilyAggregateConflictException.UnknownActualRevision(
        first.AggregateType,
        first.ExpectedRevision,
        exception
      );
    }

    long actualRevision;

    try
    {
      actualRevision = await _database.FamilyAggregateSnapshots
        .AsNoTracking()
        .Where(item => item.FamilyId == first.FamilyId.Value && item.AggregateType == first.AggregateType)
        .Select(item => (long?)item.Revision)
        .SingleOrDefaultAsync(cancellationToken) ?? 0;
    }
    catch (Exception) when (!cancellationToken.IsCancellationRequested)
    {
      return FamilyAggregateConflictException.UnknownActualRevision(
        first.AggregateType,
        first.ExpectedRevision,
        exception
      );
    }

    return new FamilyAggregateConflictException(
      first.AggregateType,
      first.ExpectedRevision,
      actualRevision,
      exception
    );
  }

  private static async Task<bool> RollbackAndDisposeOwnedTransactionAsync(
    IDbContextTransaction? transaction
  )
  {
    if (transaction is null)
    {
      return false;
    }

    var rolledBack = false;

    try
    {
      await transaction.RollbackAsync(CancellationToken.None);
      rolledBack = true;
    }
    catch
    {
      // Preserve the classified concurrency failure if rollback also fails.
    }

    try
    {
      await transaction.DisposeAsync();
    }
    catch
    {
      // Preserve the classified concurrency failure if disposal also fails.
      rolledBack = false;
    }

    return rolledBack;
  }

  private static FamilyAggregateSnapshot ToSnapshot(FamilyAggregateSnapshotEntity entity)
  {
    using var document = JsonDocument.Parse(entity.StateJson);

    return new FamilyAggregateSnapshot(
      new FamilyId(entity.FamilyId),
      entity.AggregateType,
      entity.SchemaVersion,
      entity.Revision,
      document.RootElement.Clone(),
      new FamilyMemberId(entity.UpdatedBy),
      entity.UpdatedAt
    );
  }
}