using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class InMemoryFamilyAggregateSnapshotRepository : IFamilyAggregateSnapshotRepository
{
  private readonly ConcurrentDictionary<(Guid FamilyId, string AggregateType), FamilyAggregateSnapshot> _snapshots =
    new();
  private readonly ConcurrentDictionary<(Guid FamilyId, string AggregateType), object> _locks = new();
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
    var key = (familyId.Value, aggregateType);
    var found = _snapshots.TryGetValue(key, out var snapshot);

    return Task.FromResult(found ? Clone(snapshot!) : null);
  }

  public Task<FamilyAggregateSnapshot> SaveAsync(
    SaveFamilyAggregateCommand command,
    int schemaVersion,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();
    var key = (command.FamilyId.Value, command.AggregateType);
    var aggregateLock = _locks.GetOrAdd(key, _ => new object());

    lock (aggregateLock)
    {
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

      return Task.FromResult(Clone(saved));
    }
  }

  private static FamilyAggregateSnapshot Clone(FamilyAggregateSnapshot snapshot)
  {
    return snapshot with { State = snapshot.State.Clone() };
  }
}
