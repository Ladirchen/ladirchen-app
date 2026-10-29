using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public interface IFamilyProgressionRepository
{
  Task<LoadedAggregate<FamilyHouse>?> LoadAsync(Family family, CancellationToken cancellationToken);

  Task<LoadedAggregate<FamilyHouse>> SaveAsync(
    FamilyHouse root,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  );
}
