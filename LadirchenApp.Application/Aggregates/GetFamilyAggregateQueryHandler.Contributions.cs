using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private async Task<ContributionsFamilyReadModelProjection?> ProjectContributionsAsync(
    Family family,
    FamilyMemberId viewerId,
    CancellationToken cancellationToken
  )
  {
    var loadedEconomy = await _economyRepository.LoadAsync(family, cancellationToken);

    if (loadedEconomy is null)
    {
      return null;
    }

    return ProjectContributions(family, loadedEconomy, viewerId);
  }

  private ContributionsFamilyReadModelProjection ProjectContributions(
    Family family,
    LoadedFamilyEconomy loadedEconomy,
    FamilyMemberId viewerId
  )
  {
    var calculatedState = ContributionReadModelProjector.Project(
      new ContributionReadModelState(family, loadedEconomy.Root),
      viewerId,
      _timeProvider.GetUtcNow()
    );

    return new ContributionsFamilyReadModelProjection(
      loadedEconomy.Revision,
      loadedEconomy.Root,
      loadedEconomy.Presentation,
      calculatedState
    );
  }
}
