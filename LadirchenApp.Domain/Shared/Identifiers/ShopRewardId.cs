using System;
using System.Text.RegularExpressions;

namespace LadirchenApp.Domain.Shared;

public sealed record ShopRewardId : DomainIdentifier
{
  public ShopRewardId(string value) : base(value) { }
}