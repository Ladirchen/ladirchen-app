using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private static FamilyAggregateSnapshot ProjectFamilyPermissions(FamilyAggregateSnapshot profile)
  {
    var state = JsonNode.Parse(profile.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The family profile state must be an object.");
    var members = state["members"] as JsonArray
      ?? throw new InvalidOperationException("The family member collection is required.");
    var permissionPolicy = new FamilyPermissionPolicy();

    foreach (var member in members.OfType<JsonObject>())
    {
      var permissions = FamilyMemberPermissionResolver.Resolve(member, permissionPolicy);
      member["permissions"] = new JsonObject
      {
        ["canInviteMembers"] = permissions.InviteMembers,
        ["canManageContent"] = permissions.ManageContent,
        ["canManageFamily"] = permissions.ManageFamily,
        ["canManageGoals"] = permissions.ManageGoals,
        ["canSupportChildGoals"] = permissions.SupportGoals,
        ["canViewFamilyGoals"] = permissions.ViewFamilyGoals,
        ["canViewGuardianGoals"] = permissions.ViewGuardianGoals,
      };
    }

    state["avatarCatalog"] = ProjectAvatarCatalog();

    using var document = JsonDocument.Parse(state.ToJsonString());

    return profile with { State = document.RootElement.Clone() };
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
