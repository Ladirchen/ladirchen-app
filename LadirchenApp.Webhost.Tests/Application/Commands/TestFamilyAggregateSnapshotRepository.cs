using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

internal sealed class TestFamilyAggregateSnapshotRepository : IFamilyAggregateSnapshotRepository
{
  private readonly Dictionary<(Guid FamilyId, string AggregateType), FamilyAggregateSnapshot> _snapshots = new();

  public IReadOnlyList<FamilyAggregateRevisionGuard> LastRevisionGuards { get; private set; } = [];

  public void Seed(FamilyAggregateSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    _snapshots[(snapshot.FamilyId.Value, snapshot.AggregateType)] = snapshot;
  }

  public FamilyAggregateSnapshot Required(FamilyId familyId, string aggregateType)
  {
    return _snapshots[(familyId.Value, aggregateType)];
  }

  public Task<IReadOnlyList<FamilyId>> ListFamilyIdsAsync(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    IReadOnlyList<FamilyId> result = _snapshots.Keys
      .Select(item => item.FamilyId)
      .Distinct()
      .Select(item => new FamilyId(item))
      .ToArray();

    return Task.FromResult(result);
  }

  public Task<FamilyAggregateSnapshot?> GetAsync(
    FamilyId familyId,
    string aggregateType,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();
    _snapshots.TryGetValue((familyId.Value, aggregateType), out var snapshot);

    return Task.FromResult(snapshot);
  }

  public Task<IReadOnlyDictionary<string, FamilyAggregateSnapshot>> GetManyAsync(
    FamilyId familyId,
    IReadOnlyCollection<string> aggregateTypes,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();
    var requested = aggregateTypes.ToHashSet(StringComparer.Ordinal);
    IReadOnlyDictionary<string, FamilyAggregateSnapshot> result = _snapshots.Values
      .Where(item => item.FamilyId == familyId && requested.Contains(item.AggregateType))
      .ToDictionary(item => item.AggregateType, StringComparer.Ordinal);

    return Task.FromResult(result);
  }

  public async Task<FamilyAggregateSnapshot> SaveAsync(
    SaveFamilyAggregateCommand command,
    int schemaVersion,
    CancellationToken cancellationToken
  )
  {
    var saved = await SaveBatchAsync(
      [new PendingFamilyAggregateSave(command, schemaVersion)],
      cancellationToken
    );

    return saved[0];
  }

  public Task<IReadOnlyList<FamilyAggregateSnapshot>> SaveBatchAsync(
    IReadOnlyList<PendingFamilyAggregateSave> saves,
    CancellationToken cancellationToken
  )
  {
    return SaveBatchAsync(saves, [], cancellationToken);
  }

  public Task<IReadOnlyList<FamilyAggregateSnapshot>> SaveBatchAsync(
    IReadOnlyList<PendingFamilyAggregateSave> saves,
    IReadOnlyList<FamilyAggregateRevisionGuard> revisionGuards,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();
    LastRevisionGuards = revisionGuards.ToArray();

    foreach (var guard in revisionGuards)
    {
      var actualRevision = CurrentRevision(guard.FamilyId, guard.AggregateType);

      if (actualRevision != guard.ExpectedRevision)
      {
        throw new FamilyAggregateConflictException(
          guard.AggregateType,
          guard.ExpectedRevision,
          actualRevision
        );
      }
    }

    foreach (var save in saves)
    {
      var command = save.Command;
      var actualRevision = CurrentRevision(command.FamilyId, command.AggregateType);

      if (actualRevision != command.ExpectedRevision)
      {
        throw new FamilyAggregateConflictException(
          command.AggregateType,
          command.ExpectedRevision,
          actualRevision
        );
      }
    }

    var changed = saves.Select(save =>
    {
      var command = save.Command;
      var snapshot = new FamilyAggregateSnapshot(
        command.FamilyId,
        command.AggregateType,
        save.SchemaVersion,
        command.ExpectedRevision + 1,
        command.State.Clone(),
        command.UpdatedBy,
        DateTimeOffset.UtcNow
      );
      _snapshots[(command.FamilyId.Value, command.AggregateType)] = snapshot;

      return snapshot;
    }).ToArray();

    return Task.FromResult<IReadOnlyList<FamilyAggregateSnapshot>>(changed);
  }

  private long CurrentRevision(FamilyId familyId, string aggregateType)
  {
    return _snapshots.TryGetValue((familyId.Value, aggregateType), out var snapshot)
      ? snapshot.Revision
      : 0;
  }
}
