using System;
using System.Text.RegularExpressions;

namespace LadirchenApp.Domain.Shared;

public sealed record ContributionId : DomainIdentifier
{
  public ContributionId(string value) : base(value) { }
}