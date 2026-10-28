using System;
using System.Text.RegularExpressions;

namespace LadirchenApp.Domain.Shared;

public sealed record RedemptionRequestId : DomainIdentifier
{
  public RedemptionRequestId(string value) : base(value) { }
}