using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Persistence;

public sealed record ContributionPresentationState(
  ContributionId ContributionId,
  string Description,
  string Icon,
  bool RewardCelebrated
);
