namespace LadirchenApp.Domain.Family.Permissions;

public sealed class FamilyPermissionPolicy
{
  public FamilyPermissions Resolve(FamilyMember? member)
  {
    if (member is null || member.InvitationPending)
    {
      return new(false, false, false, false, false, false, false);
    }

    if (member.IsAdministrator)
    {
      return new(true, true, true, true, true, true, true);
    }

    if (member.Role == MemberRole.Guardian)
    {
      return new(false, false, false, false, true, true, false);
    }

    return new(false, false, false, false, false, true, false);
  }
}
