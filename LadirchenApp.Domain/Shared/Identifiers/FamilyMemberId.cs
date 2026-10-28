using System;
using System.Text.RegularExpressions;

namespace LadirchenApp.Domain.Shared;

public sealed record FamilyMemberId : DomainIdentifier
{
  public FamilyMemberId(string value) : base(value) { }
}