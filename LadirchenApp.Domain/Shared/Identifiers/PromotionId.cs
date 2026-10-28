using System;
using System.Text.RegularExpressions;

namespace LadirchenApp.Domain.Shared;

public sealed record PromotionId : DomainIdentifier
{
  public PromotionId(string value) : base(value) { }
}