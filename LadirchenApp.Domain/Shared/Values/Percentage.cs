using System;

namespace LadirchenApp.Domain.Shared;

public sealed record Percentage
{
  public Percentage(decimal value)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(value);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 100);
    Value = value;
  }
  public decimal Value
  {
    get;
  }
}