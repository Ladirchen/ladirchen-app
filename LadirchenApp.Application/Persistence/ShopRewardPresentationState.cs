using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public sealed record ShopRewardPresentationState(
  ShopRewardId RewardId,
  string Description,
  string Icon,
  string Conditions
);
