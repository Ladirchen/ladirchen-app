using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Family;

public sealed class FamilyMember : Entity<FamilyMemberId>
{
  internal FamilyMember(FamilyMemberId id, string name, MemberRole role, GuardianAccess access, bool pending)
    : base(id)
  {
    Name = Rules.Text(name);
    Role = Rules.Defined(role);
    Access = Rules.Defined(access);
    InvitationPending = pending;
    ParticipatesInWeeklyGoal = role == MemberRole.Child && !pending;
  }

  internal FamilyMember(FamilyMemberState state)
    : base(state?.Id ?? throw new ArgumentNullException(nameof(state)))
  {
    Name = state.Name;
    Role = state.Role;
    Access = state.Access;
    InvitationPending = state.InvitationPending;
    Nickname = state.Nickname;
    EmailAddress = state.EmailAddress;
    ParticipatesInWeeklyGoal = state.ParticipatesInWeeklyGoal;
  }

  public string Name
  {
    get; private set;
  }
  public MemberRole Role
  {
    get;
  }
  public GuardianAccess Access
  {
    get; private set;
  }
  public bool InvitationPending
  {
    get; private set;
  }
  public string? Nickname { get; private set; }
  public string? EmailAddress { get; private set; }
  public bool ParticipatesInWeeklyGoal { get; private set; }
  public string DisplayName
  {
    get
    {
      return Nickname ?? Name;
    }
  }
  public bool IsAdministrator
  {
    get
    {

      return !InvitationPending && Role == MemberRole.Guardian && Access == GuardianAccess.Admin;
    }
  }

  internal void Rename(string name)
  {
    Name = name;
  }

  internal void ChangeAccess(GuardianAccess access)
  {
    Access = access;
  }

  internal void AcceptInvitation()
  {
    InvitationPending = false;
  }

  internal void SetNickname(string? nickname)
  {
    Nickname = nickname;
  }

  internal void SetEmailAddress(string emailAddress)
  {
    EmailAddress = emailAddress;
  }

  internal void SetWeeklyGoalParticipation(bool participates)
  {
    ParticipatesInWeeklyGoal = participates;
  }
}
