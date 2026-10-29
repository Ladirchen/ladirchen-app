using System;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;

namespace LadirchenApp.Application.Aggregates;

internal static class FamilyMemberPermissionResolver
{
  public static FamilyPermissions Resolve(JsonObject member, FamilyPermissionPolicy? policy = null)
  {
    ArgumentNullException.ThrowIfNull(member);
    var role = Enum.Parse<MemberRole>(member["role"]!.GetValue<string>(), true);
    var access = Enum.TryParse<GuardianAccess>(
      member["guardianAccess"]?.GetValue<string>(),
      true,
      out var parsedAccess
    )
      ? parsedAccess
      : GuardianAccess.Supporter;

    return (policy ?? new FamilyPermissionPolicy()).Resolve(
      role,
      access,
      member["invitationPending"]?.GetValue<bool>() ?? false
    );
  }
}
