using System;

namespace LadirchenApp.Domain.Shared;

internal static class Rules
{
  internal static void Require(bool condition, DomainError code, string message)
  {

    if (!condition)
    {
      throw new DomainRuleException(code, message);
    }

  }

  internal static string Text(string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    return value.Trim();
  }

  internal static decimal Positive(LadirchenAmount amount)
  {
    ArgumentNullException.ThrowIfNull(amount);
    Require(amount.Value > 0, DomainError.InvalidValue, "The amount must be positive.");

    return amount.Value;
  }

  internal static T Defined<T>(T value) where T : struct, Enum
  {
    Require(Enum.IsDefined(value), DomainError.InvalidValue, "The enumeration value is invalid.");

    return value;
  }

  internal static decimal Round(decimal value)
  {

    return decimal.Round(value, 0, MidpointRounding.AwayFromZero);
  }
}