using System;

namespace LadirchenApp.Domain.Family;

public sealed class FamilyCurrencySettingsPolicy
{
  public const int DefaultLadirchenPerCurrencyUnit = 10;

  public bool TryNormalize(string currency, decimal enteredRate, out FamilyCurrency normalizedCurrency, out ExchangeRate? rate)
  {
    rate = null;

    if (
      !Enum.TryParse(currency, out normalizedCurrency)
      || !Enum.IsDefined(normalizedCurrency)
      || enteredRate <= 0
    )
    {
      return false;
    }

    var rounded = decimal.Round(enteredRate, 0, MidpointRounding.AwayFromZero);

    if (rounded < 1 || rounded > int.MaxValue)
    {
      return false;
    }

    rate = new ExchangeRate(decimal.ToInt32(rounded));

    return true;
  }
}
