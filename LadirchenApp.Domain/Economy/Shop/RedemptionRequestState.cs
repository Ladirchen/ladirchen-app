using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record RedemptionRequestState(
  RedemptionRequestId Id,
  ShopRewardId RewardId,
  FamilyMemberId RequesterId,
  LadirchenAmount Price,
  DateTimeOffset RequestedAt,
  RedemptionStatus Status,
  DateTimeOffset? DecidedAt
);
