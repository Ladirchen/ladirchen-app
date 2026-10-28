using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed class FamilyGoalParticipantPolicy
{
  public IReadOnlySet<FamilyMemberId> Select(
    IEnumerable<FamilyMember> activeMembers,
    string participation,
    IEnumerable<FamilyMemberId>? selected = null
  )
  {
    ArgumentNullException.ThrowIfNull(activeMembers);
    var members = activeMembers.Where(item => !item.InvitationPending).ToArray();

    return participation switch
    {
      "the entire family" => members.Select(item => item.Id).ToHashSet(),
      "children only" => members.Where(item => item.Role == MemberRole.Child).Select(item => item.Id).ToHashSet(),
      "selected family members" => (selected ?? Array.Empty<FamilyMemberId>()).ToHashSet(),
      _ => throw new DomainRuleException(DomainError.InvalidValue, "Unknown family-goal participation selection."),
    };
  }
}
