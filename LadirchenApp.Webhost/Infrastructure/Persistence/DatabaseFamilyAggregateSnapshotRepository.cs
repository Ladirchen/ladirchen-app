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
    var entity = await _database.FamilyAggregateSnapshots
      .AsNoTracking()
      .SingleOrDefaultAsync(
        item => item.FamilyId == familyId.Value && item.AggregateType == aggregateType,
        cancellationToken
      );

    return entity is null ? null : ToSnapshot(entity);
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
    ArgumentNullException.ThrowIfNull(saves);

    if (saves.Count == 0)
    {
      return [];
    }

    var duplicate = saves
      .GroupBy(item => (item.Command.FamilyId.Value, item.Command.AggregateType))
      .FirstOrDefault(group => group.Count() > 1);

    if (duplicate is not null)
    {
      throw new ArgumentException("An aggregate may only be saved once per transaction.", nameof(saves));
    }

    IDbContextTransaction? transaction = null;

    try
    {
      if (_database.Database.CurrentTransaction is null)
      {
        transaction = await _database.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
      }

      var validated = new List<(PendingFamilyAggregateSave Save, FamilyAggregateSnapshotEntity? Entity)>(
        saves.Count
      );

      foreach (var save in saves)
      {
        var command = save.Command;
        var entity = await _database.FamilyAggregateSnapshots.SingleOrDefaultAsync(
          item => item.FamilyId == command.FamilyId.Value && item.AggregateType == command.AggregateType,
          cancellationToken
        );
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
      throw await CreateConflictAsync(saves, exception, cancellationToken);
    }
    catch (DbUpdateException exception)
    {
      throw await CreateConflictAsync(saves, exception, cancellationToken);
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
    CancellationToken cancellationToken
  )
  {
    _database.ChangeTracker.Clear();
    var first = saves[0].Command;
    var actualRevision = await _database.FamilyAggregateSnapshots
      .AsNoTracking()
      .Where(item => item.FamilyId == first.FamilyId.Value && item.AggregateType == first.AggregateType)
      .Select(item => (long?)item.Revision)
      .SingleOrDefaultAsync(cancellationToken) ?? 0;

    return new FamilyAggregateConflictException(
      first.AggregateType,
      first.ExpectedRevision,
      actualRevision,
      exception
    );
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
