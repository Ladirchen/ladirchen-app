using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public interface IFamilyAggregateSnapshotRepository
{
  Task<FamilyAggregateSnapshot?> GetAsync(
    FamilyId familyId,
    string aggregateType,
    CancellationToken cancellationToken
  );

  Task<FamilyAggregateSnapshot> SaveAsync(
    SaveFamilyAggregateCommand command,
    int schemaVersion,
    CancellationToken cancellationToken
  );
}
