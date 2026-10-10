using System;
using System.Text.RegularExpressions;

namespace LadirchenApp.Domain.Shared;

public abstract record DomainIdentifier
{
  private protected DomainIdentifier(string value)
  {
    ArgumentNullException.ThrowIfNull(value);

    if (!Regex.IsMatch(value, @"^[A-Za-z0-9_][A-Za-z0-9_.:-]{0,127}$", RegexOptions.CultureInvariant))
    {
      throw new ArgumentException("An ID must contain 1-128 identifier characters.", nameof(value));
    }

    Value = value;
  }

  public string Value
  {
    get;
  }
}