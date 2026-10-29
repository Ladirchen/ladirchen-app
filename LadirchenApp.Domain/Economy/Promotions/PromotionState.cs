using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record PromotionState(
  PromotionId Id,
  ContributionId ContributionId,
  RewardMultiplier Multiplier,
  LadirchenAmount TeamworkBonus,
  PromotionDeadline Deadline,
  bool Active
);
