using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public interface IFamilyRepository
{
  Task<IReadOnlyList<FamilyId>> ListIdsAsync(CancellationToken cancellationToken);

  Task<LoadedAggregate<Family>?> LoadAsync(FamilyId familyId, CancellationToken cancellationToken);

  Task<LoadedAggregate<Family>> SaveAsync(
    Family root,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  );
}
