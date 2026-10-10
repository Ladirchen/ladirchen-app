using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ShopRewardRequested(FamilyId FamilyId, RedemptionRequestId RequestId, FamilyMemberId ChildId,
  ShopRewardId RewardId, decimal Price, DateTimeOffset At) : BusinessEvent(At.ToUniversalTime());