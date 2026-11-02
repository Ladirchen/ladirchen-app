using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private async Task<RewardShopReadModelProjection?> ProjectRewardShopAsync(
    Family family,
    FamilyMemberId authenticatedMemberId,
    FamilyMemberId viewerId,
    CancellationToken cancellationToken
  )
  {
    var loadedEconomy = await _economyRepository.LoadAsync(family, cancellationToken);

    if (loadedEconomy is null)
    {
      return null;
    }

    var now = _timeProvider.GetUtcNow();
    var authenticatedMember = family.ActiveMember(authenticatedMemberId);
    var viewer = family.ActiveMember(viewerId);
    var mayManageShop = authenticatedMember.Role == MemberRole.Guardian
      && viewer.Role == MemberRole.Guardian;
    var availability = loadedEconomy.Root.ShopRewards.ToDictionary(
      reward => reward.Id,
      reward => loadedEconomy.Root.GetShopRewardAvailability(reward.Id, now)
    );
    var visibleRewardIds = loadedEconomy.Root.ShopRewards
      .Where(reward => mayManageShop
        || availability[reward.Id].PublicationStatus == ShopRewardPublicationStatus.Visible)
      .Select(reward => reward.Id)
      .ToHashSet();

    return new RewardShopReadModelProjection(
      loadedEconomy.Revision,
      loadedEconomy.Root,
      loadedEconomy.Presentation,
      ShopAvailabilityPolicy.RedemptionIsOpen(now, family.TimeZone),
      family.TimeZone,
      viewerId,
      availability,
      visibleRewardIds
    );
  }
}
