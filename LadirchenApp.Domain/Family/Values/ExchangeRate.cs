using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Family;

public sealed record ExchangeRate
{
  public ExchangeRate(int ladirchenPerCurrencyUnit)
  {
    ArgumentOutOfRangeException.ThrowIfLessThan(ladirchenPerCurrencyUnit, 1);
    LadirchenPerCurrencyUnit = ladirchenPerCurrencyUnit;
  }

  public int LadirchenPerCurrencyUnit
  {
    get;
  }
}