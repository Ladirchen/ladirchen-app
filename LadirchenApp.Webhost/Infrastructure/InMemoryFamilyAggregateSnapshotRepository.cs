using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class InMemoryFamilyAggregateSnapshotRepository : IFamilyAggregateSnapshotRepository
{
  private readonly ConcurrentDictionary<(Guid FamilyId, string AggregateType), FamilyAggregateSnapshot> _snapshots =
    new();
  private readonly object _transactionLock = new();
  private readonly TimeProvider _timeProvider;

  public InMemoryFamilyAggregateSnapshotRepository(TimeProvider timeProvider)
  {
    _timeProvider = timeProvider;
  }

  public Task<FamilyAggregateSnapshot?> GetAsync(
    FamilyId familyId,
    string aggregateType,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();
    lock (_transactionLock)
    {
      var key = (familyId.Value, aggregateType);
      var found = _snapshots.TryGetValue(key, out var snapshot);

      return Task.FromResult(found ? Clone(snapshot!) : null);
    }
  }

  public Task<FamilyAggregateSnapshot> SaveAsync(
    SaveFamilyAggregateCommand command,
    int schemaVersion,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();
    lock (_transactionLock)
    {
      return Task.FromResult(SaveLocked(command, schemaVersion));
    }
  }

  public Task<IReadOnlyList<FamilyAggregateSnapshot>> SaveBatchAsync(
    IReadOnlyList<PendingFamilyAggregateSave> saves,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(saves);
    cancellationToken.ThrowIfCancellationRequested();

    lock (_transactionLock)
    {
      foreach (var save in saves)
      {
        ValidateRevision(save.Command);
      }

      var results = saves.Select(save => SaveLocked(save.Command, save.SchemaVersion)).ToArray();

      return Task.FromResult<IReadOnlyList<FamilyAggregateSnapshot>>(results);
    }
  }

  private FamilyAggregateSnapshot SaveLocked(SaveFamilyAggregateCommand command, int schemaVersion)
  {
    var currentRevision = ValidateRevision(command);
    var key = (command.FamilyId.Value, command.AggregateType);
    var saved = new FamilyAggregateSnapshot(
      command.FamilyId,
      command.AggregateType,
      schemaVersion,
      checked(currentRevision + 1),
      command.State.Clone(),
      command.UpdatedBy,
      _timeProvider.GetUtcNow()
    );
    _snapshots[key] = saved;

    return Clone(saved);
  }

  private long ValidateRevision(SaveFamilyAggregateCommand command)
  {
    var key = (command.FamilyId.Value, command.AggregateType);
    var hasCurrent = _snapshots.TryGetValue(key, out var current);
    var currentRevision = hasCurrent ? current!.Revision : 0;

    if (currentRevision != command.ExpectedRevision)
    {
      throw new FamilyAggregateConflictException(
        command.AggregateType,
        command.ExpectedRevision,
        currentRevision
      );
    }

    return currentRevision;
  }

  private static FamilyAggregateSnapshot Clone(FamilyAggregateSnapshot snapshot)
  {
    return snapshot with { State = snapshot.State.Clone() };
  }
}
