using System;

namespace LadirchenApp.Domain.Economy;

public sealed class SpecialGiftPolicy
{
  public const decimal MaximumAmount = 10000;
  public const decimal MinimumAmount = 1;

  public decimal Normalize(decimal enteredAmount)
  {
    return Math.Clamp(
      decimal.Round(enteredAmount, 0, MidpointRounding.AwayFromZero),
      MinimumAmount,
      MaximumAmount
    );
  }
}
