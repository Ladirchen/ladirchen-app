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
    return Select(
      activeMembers,
      participation switch
      {
        "the entire family" => FamilyGoalParticipation.EntireFamily,
        "children only" => FamilyGoalParticipation.ChildrenOnly,
        "selected family members" => FamilyGoalParticipation.SelectedMembers,
        _ => throw new DomainRuleException(DomainError.InvalidValue, "Unknown family-goal participation selection."),
      },
      selected
    );
  }

  public IReadOnlySet<FamilyMemberId> Select(
    IEnumerable<FamilyMember> activeMembers,
    FamilyGoalParticipation participation,
    IEnumerable<FamilyMemberId>? selected = null
  )
  {
    ArgumentNullException.ThrowIfNull(activeMembers);
    var members = activeMembers
      .Where(item => !item.InvitationPending)
      .Where(item => item.Role is MemberRole.Child or MemberRole.Guardian)
      .ToArray();
    var selectedIds = (selected ?? Array.Empty<FamilyMemberId>()).ToHashSet();
    var result = participation switch
    {
      FamilyGoalParticipation.EntireFamily => members.Select(item => item.Id).ToHashSet(),
      FamilyGoalParticipation.ChildrenOnly => members
        .Where(item => item.Role == MemberRole.Child)
        .Select(item => item.Id)
        .ToHashSet(),
      FamilyGoalParticipation.SelectedMembers => selectedIds,
      _ => throw new ArgumentOutOfRangeException(nameof(participation)),
    };
    var eligible = members.Select(item => item.Id).ToHashSet();
    Rules.Require(
      result.Count > 0,
      DomainError.InvalidValue,
      "A family savings goal requires at least one participant."
    );
    Rules.Require(
      result.All(eligible.Contains),
      DomainError.InvalidValue,
      "Every selected participant must be an active child or guardian."
    );

    return result;
  }
}
