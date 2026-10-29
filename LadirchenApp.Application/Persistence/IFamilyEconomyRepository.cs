using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public interface IFamilyEconomyRepository
{
  Task<LoadedFamilyEconomy?> LoadAsync(
    Family family,
    CancellationToken cancellationToken
  );

  Task<LoadedFamilyEconomy> SaveAsync(
    FamilyEconomy root,
    FamilyEconomyPresentationState presentation,
    long expectedRevision,
    FamilyMemberId updatedBy,
    CancellationToken cancellationToken
  );
}
