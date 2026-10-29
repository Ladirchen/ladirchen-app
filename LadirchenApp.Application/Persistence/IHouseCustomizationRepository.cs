using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public interface IHouseCustomizationRepository
{
  Task<LoadedAggregate<FamilyHouseShop>?> LoadAsync(
    Family family,
    int currentHouseLevel,
    CancellationToken cancellationToken
  );

  Task<LoadedAggregate<FamilyHouseShop>> SaveAsync(
    FamilyHouseShop root,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  );
}
