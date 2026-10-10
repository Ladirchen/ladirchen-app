using System;
using System.Text.RegularExpressions;

namespace LadirchenApp.Domain.Shared;

public sealed record FamilyId
{
  public FamilyId(Guid value)
  {

    if (value == Guid.Empty)
    {
      throw new ArgumentException("A family ID cannot be empty.", nameof(value));
    }

    Value = value;
  }

  public Guid Value
  {
    get;
  }
}