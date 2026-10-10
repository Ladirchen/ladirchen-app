using System;
using System.Collections.Generic;

using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Shared;

public sealed class LadirchenAmount : ValueObject
{
  public LadirchenAmount(decimal value)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(value);
    Value = value;
  }

  public decimal Value
  {
    get;
  }

  protected override IEnumerable<object?> GetEqualityComponents()
  {

    yield return Value;
  }
}