using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed record GoalOwner
{
  private GoalOwner(FamilyMemberId? memberId)
  {
    MemberId = memberId;
  }

  public FamilyMemberId? MemberId
  {
    get;
  }
  public bool IsFamily
  {
    get
    {

      return MemberId is null;
    }
  }

  public static GoalOwner Family { get; } = new((FamilyMemberId?)null);
  public static GoalOwner Member(FamilyMemberId memberId)
  {
    ArgumentNullException.ThrowIfNull(memberId);

    return new(memberId);
  }
}