using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record RewardShopReadModelState(
  bool? RedemptionOpen,
  [property: JsonRequired] IReadOnlyList<string> ShopTaskGateOverrideMemberIds,
  [property: JsonRequired] IReadOnlyList<ShopRewardReadModel> Rewards,
  [property: JsonRequired] IReadOnlyList<ShopTimeRewardReadModel> TimeRewards
);
