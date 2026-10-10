using System;

namespace LadirchenApp.Domain.Shared;

public sealed record StarRating
{
  public StarRating(int value)
  {
    ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 5);
    Value = value;
  }
  public int Value
  {
    get;
  }
}