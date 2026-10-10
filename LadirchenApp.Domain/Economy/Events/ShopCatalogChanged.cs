using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ShopCatalogChanged(FamilyId FamilyId, ShopRewardId RewardId, string Change, DateTimeOffset At)
  : BusinessEvent(At.ToUniversalTime());