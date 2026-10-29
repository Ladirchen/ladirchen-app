namespace LadirchenApp.Domain.Family.Permissions;

public sealed class FamilyPermissionPolicy
{
  public FamilyPermissions Resolve(FamilyMember? member)
  {
    return member is null
      ? None()
      : Resolve(member.Role, member.Access, member.InvitationPending);
  }

  public FamilyPermissions Resolve(MemberRole role, GuardianAccess access, bool invitationPending)
  {
    if (invitationPending)
    {
      return None();
    }

    if (role == MemberRole.Guardian && access == GuardianAccess.Admin)
    {
      return new(true, true, true, true, true, true, true);
    }

    if (role == MemberRole.Guardian)
    {
      return new(false, false, false, false, true, true, false);
    }

    return new(false, false, false, false, false, true, false);
  }

  private static FamilyPermissions None()
  {
    return new(false, false, false, false, false, false, false);
  }
}
