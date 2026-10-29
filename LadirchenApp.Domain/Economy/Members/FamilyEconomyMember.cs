using System;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record FamilyEconomyMember
{
  public FamilyEconomyMember(
    FamilyMemberId id,
    MemberRole role,
    GuardianAccess access,
    bool invitationPending
  )
  {
    Id = id ?? throw new ArgumentNullException(nameof(id));
    Role = Rules.Defined(role);
    Access = Rules.Defined(access);
    InvitationPending = invitationPending;
  }

  public FamilyMemberId Id
  {
    get;
  }
  public MemberRole Role
  {
    get;
  }
  public GuardianAccess Access
  {
    get;
  }
  public bool InvitationPending
  {
    get;
  }
  public bool IsAdministrator
  {
    get
    {
      return !InvitationPending && Role == MemberRole.Guardian && Access == GuardianAccess.Admin;
    }
  }
}