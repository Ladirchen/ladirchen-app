using System;

namespace LadirchenApp.Domain.Shared;

public sealed record RewardMultiplier
{
  public RewardMultiplier(decimal value)
  {
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0);
    Value = value;
  }
  public decimal Value
  {
    get;
  }
}