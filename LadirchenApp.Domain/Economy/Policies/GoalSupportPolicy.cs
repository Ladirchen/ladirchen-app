using System;

namespace LadirchenApp.Domain.Economy;

public sealed class GoalSupportPolicy
{
  public decimal Normalize(decimal entered, decimal remaining)
  {
    if (entered <= 0 || remaining <= 0)
    {
      return 0;
    }

    return Math.Min(decimal.Round(entered, 0, MidpointRounding.AwayFromZero), remaining);
  }
}
