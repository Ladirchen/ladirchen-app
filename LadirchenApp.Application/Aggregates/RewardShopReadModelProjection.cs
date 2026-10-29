using System.Collections.Generic;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record RewardShopReadModelProjection(
  long Revision,
  FamilyEconomy Economy,
  FamilyEconomyPresentationState Presentation,
  bool RedemptionOpen,
  IanaTimeZone TimeZone,
  FamilyMemberId ViewerId,
  IReadOnlyDictionary<ShopRewardId, ShopRewardAvailability> Availability,
  IReadOnlySet<ShopRewardId> VisibleRewardIds
) : IFamilyReadModelProjection
{
  public string ReadModelType
  {
    get
    {
      return "reward-shop";
    }
  }
}
