using System;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Auth;

public sealed class FamilyAccount
{
  public FamilyAccount(string username, string password, FamilyAggregate family, FamilyMemberId memberId)
  {
    Username = Rules.Text(username);
    Password = Rules.Text(password);
    Family = family ?? throw new ArgumentNullException(nameof(family));
    MemberId = memberId ?? throw new ArgumentNullException(nameof(memberId));
  }

  public string Username { get; }
  internal string Password { get; }
  public FamilyAggregate Family { get; }
  public FamilyMemberId MemberId { get; }
}
