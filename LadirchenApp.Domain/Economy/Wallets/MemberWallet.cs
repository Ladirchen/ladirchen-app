using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class MemberWallet : Entity<FamilyMemberId>
{
  internal MemberWallet(FamilyMemberId id) : base(id) { }
  public decimal Balance
  {
    get; private set;
  }
  public decimal Reserved
  {
    get; private set;
  }
  public decimal Available
  {
    get
    {

      return Balance - Reserved;
    }
  }

  internal void Set(decimal balance, decimal reserved)
  {
    Balance = balance;
    Reserved = reserved;
  }
}