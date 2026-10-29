using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public static class ContributionRewardPolicy
{
  public static Percentage DefaultPerfectRatingBonus { get; } = new(5);

  public static ContributionRewardBreakdown Calculate(LadirchenAmount reward, StarRating rating,
    Percentage perfectRatingBonus, RewardMultiplier? multiplier = null, LadirchenAmount? teamworkBonus = null)
  {
    ArgumentNullException.ThrowIfNull(reward);
    ArgumentNullException.ThrowIfNull(rating);
    ArgumentNullException.ThrowIfNull(perfectRatingBonus);
    var factor = multiplier?.Value ?? 1;
    var baseReward = checked(reward.Value * factor + (teamworkBonus?.Value ?? 0));
    var ratingBonus = rating.Value == 5 && perfectRatingBonus.Value > 0
      ? Math.Max(1, Rules.Round(checked(baseReward * (perfectRatingBonus.Value / 100)))) : 0;
    var result = new ContributionRewardBreakdown(baseReward, ratingBonus, factor);
    _ = result.Total;

    return result;
  }
}
