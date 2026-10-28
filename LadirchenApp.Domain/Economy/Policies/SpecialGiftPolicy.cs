using System;

namespace LadirchenApp.Domain.Economy;

public sealed class SpecialGiftPolicy
{
  public decimal Normalize(decimal enteredAmount)
  {
    return Math.Clamp(decimal.Round(enteredAmount, 0, MidpointRounding.AwayFromZero), 1, 10000);
  }
}
