using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record ContributionRewardBreakdown(decimal BaseReward, decimal RatingBonus, decimal Multiplier)
{
  public decimal Total
  {
    get
    {

      return checked(BaseReward + RatingBonus);
    }
  }
}