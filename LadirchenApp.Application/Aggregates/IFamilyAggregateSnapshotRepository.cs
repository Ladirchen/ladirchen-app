using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public interface IFamilyAggregateSnapshotRepository
{
  Task<IReadOnlyList<FamilyId>> ListFamilyIdsAsync(CancellationToken cancellationToken);

  Task<FamilyAggregateSnapshot?> GetAsync(
    FamilyId familyId,
    string aggregateType,
    CancellationToken cancellationToken
  );

  Task<IReadOnlyDictionary<string, FamilyAggregateSnapshot>> GetManyAsync(
    FamilyId familyId,
    IReadOnlyCollection<string> aggregateTypes,
    CancellationToken cancellationToken
  );

  Task<FamilyAggregateSnapshot> SaveAsync(
    SaveFamilyAggregateCommand command,
    int schemaVersion,
    CancellationToken cancellationToken
  );

  Task<IReadOnlyList<FamilyAggregateSnapshot>> SaveBatchAsync(
    IReadOnlyList<PendingFamilyAggregateSave> saves,
    CancellationToken cancellationToken
  );

  Task<IReadOnlyList<FamilyAggregateSnapshot>> SaveBatchAsync(
    IReadOnlyList<PendingFamilyAggregateSave> saves,
    IReadOnlyList<FamilyAggregateRevisionGuard> revisionGuards,
    CancellationToken cancellationToken
  );
}