using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record RewardShopReadModelState(
  bool? RedemptionOpen,
  [property: JsonRequired] IReadOnlyList<ShopRewardReadModel> Rewards
);
