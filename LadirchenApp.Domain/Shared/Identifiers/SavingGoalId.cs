using System;
using System.Text.RegularExpressions;

namespace LadirchenApp.Domain.Shared;

public sealed record SavingGoalId : DomainIdentifier
{
  public SavingGoalId(string value) : base(value) { }
}