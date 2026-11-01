using System.Threading;
using System.Threading.Tasks;

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
