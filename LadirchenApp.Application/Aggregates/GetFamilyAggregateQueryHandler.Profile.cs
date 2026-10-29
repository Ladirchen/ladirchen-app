using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private static FamilyAggregateSnapshot ProjectFamilyPermissions(
    FamilyAggregateSnapshot profile,
    FamilyMemberId authenticatedMemberId,
    FamilyMemberId viewerId
  )
  {
    var state = ReadProfileState(profile);
    var members = state["members"] as JsonArray
      ?? throw new InvalidOperationException("The family member collection is required.");
    var family = FamilyProfileSnapshotMapper.Rehydrate(profile.FamilyId, state);
    var permissionPolicy = new FamilyPermissionPolicy();
    var authenticatedPermissions = permissionPolicy.Resolve(family.ActiveMember(authenticatedMemberId));
    var viewerPermissions = permissionPolicy.Resolve(family.ActiveMember(viewerId));
    var canManageInvitations = (
      authenticatedPermissions.InviteMembers && viewerPermissions.InviteMembers
    ) || (
      authenticatedPermissions.ManageFamily && viewerPermissions.ManageFamily
    );

    if (!canManageInvitations)
    {
      foreach (var pendingInvitation in members
        .OfType<JsonObject>()
        .Where(member => member["invitationPending"]?.GetValue<bool>() == true)
        .ToArray())
      {
        members.Remove(pendingInvitation);
      }
    }

    var presentations = members
      .OfType<JsonObject>()
      .ToDictionary(
        member => member["id"]?.GetValue<string>()
          ?? throw new InvalidOperationException("A family member identity is required."),
        StringComparer.Ordinal
      );
    foreach (var member in family.Members.Where(member => presentations.ContainsKey(member.Id.Value)))
    {
      var presentation = presentations[member.Id.Value];

      if (!canManageInvitations && member.Id != viewerId)
      {
        presentation.Remove("email");
      }

      if (member.Id != viewerId)
      {
        presentation.Remove("permissions");

        continue;
      }

      var permissions = permissionPolicy.Resolve(member);
      presentation["permissions"] = new JsonObject
      {
        ["canInviteMembers"] = permissions.InviteMembers && authenticatedPermissions.InviteMembers,
        ["canManageContent"] = permissions.ManageContent && authenticatedPermissions.ManageContent,
        ["canManageFamily"] = permissions.ManageFamily && authenticatedPermissions.ManageFamily,
        ["canManageGoals"] = permissions.ManageGoals && authenticatedPermissions.ManageGoals,
        ["canSupportChildGoals"] = permissions.SupportGoals && authenticatedPermissions.SupportGoals,
        ["canViewFamilyGoals"] = permissions.ViewFamilyGoals && authenticatedPermissions.ViewFamilyGoals,
        ["canViewGuardianGoals"] = permissions.ViewGuardianGoals && authenticatedPermissions.ViewGuardianGoals,
      };
    }

    state["avatarCatalog"] = ProjectAvatarCatalog();

    using var document = JsonDocument.Parse(state.ToJsonString());

    return profile with { State = document.RootElement.Clone() };
  }

  private static JsonObject ReadProfileState(FamilyAggregateSnapshot profile)
  {
    return JsonNode.Parse(profile.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The family profile state must be an object.");
  }

  private static JsonObject ProjectAvatarCatalog()
  {
    var catalog = new AvatarCatalog();

    return new JsonObject
    {
      ["child"] = AvatarOptions(catalog, AvatarProfile.Child),
      ["guardian"] = AvatarOptions(catalog, AvatarProfile.Guardian),
    };
  }

  private static JsonObject AvatarOptions(AvatarCatalog catalog, AvatarProfile profile)
  {
    return new JsonObject
    {
      ["accessoryIds"] = StringArray(catalog.Options("accessory", profile)),
      ["ages"] = StringArray(catalog.Options("age", profile)),
      ["faceIds"] = StringArray(catalog.Options("face", profile)),
      ["faceShapeIds"] = StringArray(catalog.Options("face shape", profile)),
      ["funAccessoryIds"] = StringArray(catalog.Options("fun accessory", profile)),
      ["hairColorIds"] = StringArray(catalog.Options("hair color", profile)),
      ["hairIds"] = StringArray(catalog.Options("hair", profile)),
      ["outfitColorIds"] = StringArray(catalog.Options("outfit color", profile)),
      ["outfitIds"] = StringArray(catalog.Options("outfit", profile)),
      ["seasonalAccessoryIds"] = StringArray(catalog.Options("seasonal accessory", profile)),
      ["skinToneIds"] = StringArray(catalog.Options("skin tone", profile)),
    };
  }

  private static JsonArray StringArray(IEnumerable<string> values)
  {
    return new JsonArray(values.Select(value => JsonValue.Create(value)).ToArray<JsonNode?>());
  }

}
