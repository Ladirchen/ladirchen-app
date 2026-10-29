using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed class GoalCancellationRequest
{
  private readonly HashSet<FamilyMemberId> _consentedMemberIds;

  internal GoalCancellationRequest(
    FamilyMemberId requestedByMemberId,
    IEnumerable<FamilyMemberId>? consentedMemberIds = null
  )
  {
    ArgumentNullException.ThrowIfNull(requestedByMemberId);
    RequestedByMemberId = requestedByMemberId;
    _consentedMemberIds = new HashSet<FamilyMemberId>(consentedMemberIds ?? []);
    _consentedMemberIds.Add(requestedByMemberId);
  }

  public FamilyMemberId RequestedByMemberId
  {
    get;
  }

  public IReadOnlySet<FamilyMemberId> ConsentedMemberIds
  {
    get
    {
      return _consentedMemberIds.ToHashSet();
    }
  }

  internal bool HasConsented(FamilyMemberId memberId)
  {
    return _consentedMemberIds.Contains(memberId);
  }

  internal void Approve(FamilyMemberId memberId)
  {
    Rules.Require(
      _consentedMemberIds.Add(memberId),
      DomainError.Duplicate,
      "This family member already consented to the goal cancellation."
    );
  }

  internal bool IsUnanimous(IReadOnlySet<FamilyMemberId> participants)
  {
    ArgumentNullException.ThrowIfNull(participants);

    return _consentedMemberIds.SetEquals(participants);
  }
}
