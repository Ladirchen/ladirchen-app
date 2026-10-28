using System;

namespace LadirchenApp.Domain.Family;

public sealed class FamilyCurrencySettingsPolicy
{
  public bool TryNormalize(string currency, decimal enteredRate, out FamilyCurrency normalizedCurrency, out ExchangeRate? rate)
  {
    rate = null;

    if (!Enum.TryParse(currency, out normalizedCurrency) || enteredRate <= 0)
    {
      return false;
    }

    rate = new ExchangeRate((int)decimal.Round(enteredRate, 0, MidpointRounding.AwayFromZero));

    return true;
  }
}
