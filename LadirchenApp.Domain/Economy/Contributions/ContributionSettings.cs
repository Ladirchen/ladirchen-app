using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed record ContributionSettings
{
  public ContributionSettings(Percentage perfectRatingBonus)
  {
    ArgumentNullException.ThrowIfNull(perfectRatingBonus);
    PerfectRatingBonus = perfectRatingBonus;
  }

  public static ContributionSettings Default { get; } = new(
    ContributionRewardPolicy.DefaultPerfectRatingBonus
  );

  public Percentage PerfectRatingBonus { get; }
}
