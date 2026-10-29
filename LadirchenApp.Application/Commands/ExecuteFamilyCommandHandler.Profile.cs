using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed partial class ExecuteFamilyCommandHandler
{
  private async Task<FamilyCommandResult> ExecuteProfileAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profile,
    JsonObject profileState,
    JsonArray members,
    JsonObject actor,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var refreshProgression = false;

    switch (command.CommandType)
    {
      case "profile.set-weekly-participation":
        SetWeeklyParticipation(members, actor, payload);
        refreshProgression = true;
        break;
      case "profile.save-own-appearance":
        SaveOwnAppearance(actor, payload);
        break;
      case "profile.set-own-nickname":
        SetOwnNickname(actor, payload);
        break;
      case "profile.invite-guardian":
        InviteGuardian(members, actor, payload);
        break;
      case "profile.set-guardian-access":
        SetGuardianAccess(members, actor, payload);
        break;
      case "profile.complete-setup":
        CompleteFamilySetup(profileState, actor, payload);
        refreshProgression = true;
        break;
      default:
        throw new FamilyCommandException("The family-profile command is not supported.");
    }

    var saves = new List<PendingFamilyAggregateSave> { CreateSave(profile, profileState, command.ActorId) };

    if (refreshProgression)
    {
      var contributions = await RequireSnapshotAsync(command.FamilyId, "contributions", cancellationToken);
      saves.Add(
        await CreateProgressionSaveAsync(
          command.FamilyId,
          profileState,
          RequireObject(contributions.State),
          command.ActorId,
          cancellationToken
        )
      );
    }

    var changed = await _repository.SaveBatchAsync(saves, cancellationToken);

    foreach (var snapshot in changed)
    {
      await _publisher.PublishAsync(snapshot, cancellationToken);
    }

    return new FamilyCommandResult(changed);
  }

  private static void SetWeeklyParticipation(JsonArray members, JsonObject actor, JsonObject payload)
  {
    var member = RequireMember(members, RequiredString(payload, "memberId"));
    Require(StringValue(member, "role") == "guardian", "Only guardians may opt into the weekly family goal.");
    Require(
      IsAdministrator(actor) || StringValue(actor, "id") == StringValue(member, "id"),
      "Only an administrator or the affected guardian may change weekly participation."
    );
    member["participatesInWeeklyGoal"] = RequiredBool(payload, "participates");
  }

  private static void SaveOwnAppearance(JsonObject actor, JsonObject payload)
  {
    var appearance = RequireObject(payload, "appearance");
    ValidateAvatarAppearance(appearance, RequiredString(actor, "role"));
    actor["appearance"] = appearance.DeepClone();
  }

  private static void SetOwnNickname(JsonObject actor, JsonObject payload)
  {
    Require(StringValue(actor, "role") == "child", "Only children may set a nickname.");
    var nickname = StringValue(payload, "nickname")?.Trim() ?? string.Empty;

    if (nickname.Length > Family.MaximumNicknameLength)
    {
      nickname = nickname[..Family.MaximumNicknameLength];
    }

    if (nickname.Length == 0)
    {
      actor.Remove("nickname");
    }
    else
    {
      actor["nickname"] = nickname;
    }
  }

  private static void InviteGuardian(JsonArray members, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    var id = RequiredString(payload, "id");
    Require(FindById(members, id) is null, "The invited family member already exists.");
    var name = RequiredString(payload, "name").Trim();
    var email = RequiredString(payload, "email").Trim();
    var access = RequiredString(payload, "guardianAccess");
    Require(access is "admin" or "supporter", "The guardian access level is invalid.");
    var normalizedName = name.ToLowerInvariant();
    var preset = normalizedName.Contains("oma", StringComparison.Ordinal)
      ? "grandma"
      : normalizedName.Contains("opa", StringComparison.Ordinal)
        ? "grandpa"
        : "adult";
    members.Add(
      new JsonObject
      {
        ["id"] = id,
        ["name"] = name,
        ["email"] = email,
        ["avatar"] = "🧑",
        ["color"] = "#7e8db8",
        ["role"] = "guardian",
        ["guardianAccess"] = access,
        ["participatesInWeeklyGoal"] = false,
        ["weeklyStreak"] = 0,
        ["invitationPending"] = true,
        ["appearance"] = GuardianAppearance(preset),
      }
    );
  }

  private static void SetGuardianAccess(JsonArray members, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    var member = RequireMember(members, RequiredString(payload, "memberId"));
    Require(StringValue(member, "role") == "guardian", "The affected member must be a guardian.");
    Require(StringValue(member, "id") != StringValue(actor, "id"), "A guardian cannot change their own access.");
    var access = RequiredString(payload, "guardianAccess");
    Require(access is "admin" or "supporter", "The guardian access level is invalid.");
    member["guardianAccess"] = access;
  }

  private static void CompleteFamilySetup(JsonObject profile, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    var members = RequireArray(payload, "members");
    var pets = RequireArray(payload, "pets");
    Require(members.Count > 0, "A family requires at least one member.");
    var memberIds = new HashSet<string>(StringComparer.Ordinal);

    foreach (var member in members.OfType<JsonObject>())
    {
      ValidateFamilyMember(member);
      Require(memberIds.Add(RequiredString(member, "id")), "Family member identifiers must be unique.");
    }

    Require(memberIds.Count == members.Count, "Every family member must be an object.");
    Require(memberIds.Contains(RequiredString(actor, "id")), "The active administrator must remain in the family.");
    var retainedActor = FindById(members, RequiredString(actor, "id"))!;
    Require(IsAdministrator(retainedActor), "The active administrator must retain administrator access.");
    var petIds = new HashSet<string>(StringComparer.Ordinal);

    foreach (var pet in pets.OfType<JsonObject>())
    {
      ValidateFamilyPet(pet);
      Require(petIds.Add(RequiredString(pet, "id")), "Family pet identifiers must be unique.");
    }

    Require(petIds.Count == pets.Count, "Every family pet must be an object.");
    profile["members"] = members.DeepClone();
    profile["pets"] = pets.DeepClone();
    profile["onboardingCompleted"] = true;
  }

  private static void ValidateFamilyMember(JsonObject member)
  {
    _ = RequiredString(member, "id");
    _ = RequiredString(member, "name");
    _ = RequiredString(member, "avatar");
    _ = RequiredString(member, "color");
    var role = RequiredString(member, "role");
    Require(role is "child" or "guardian", "The family member role is invalid.");
    Require(RequiredInt(member, "weeklyStreak") >= 0, "A weekly streak cannot be negative.");

    if (role == "guardian")
    {
      Require(StringValue(member, "guardianAccess") is "admin" or "supporter", "The guardian access level is invalid.");
    }

    if (member["appearance"] is JsonObject appearance)
    {
      ValidateAvatarAppearance(appearance, role);
    }
  }

  private static void ValidateFamilyPet(JsonObject pet)
  {
    _ = RequiredString(pet, "id");
    _ = RequiredString(pet, "name");
    _ = RequiredString(pet, "avatar");
    _ = RequiredString(pet, "color");
    Require(
      RequiredString(pet, "kind") is "cat" or "dog" or "rabbit" or "bird" or "other",
      "The family pet kind is invalid."
    );
  }

  private static void ValidateAvatarAppearance(JsonObject appearance, string role)
  {
    var avatar = new AvatarAppearance(
      RequiredString(appearance, "age"),
      RequiredString(appearance, "skinToneId"),
      RequiredString(appearance, "faceShape"),
      RequiredString(appearance, "face"),
      RequiredString(appearance, "hair"),
      RequiredString(appearance, "hairColorId"),
      RequiredString(appearance, "outfit"),
      RequiredString(appearance, "outfitColorId"),
      RequiredString(appearance, "accessoryId"),
      RequiredString(appearance, "funAccessoryId"),
      RequiredString(appearance, "seasonalAccessoryId")
    );

    try
    {
      new AvatarCatalog().Validate(avatar, role == "child" ? AvatarProfile.Child : AvatarProfile.Guardian);
    }
    catch (DomainRuleException exception)
    {
      throw new FamilyCommandException(exception.Message);
    }
  }

  private static JsonObject GuardianAppearance(string preset)
  {
    var appearance = new JsonObject
    {
      ["age"] = preset == "adult" ? "adult" : "senior",
      ["skinToneId"] = "skin-medium",
      ["faceShape"] = preset == "grandpa" ? "angular" : "soft",
      ["face"] = preset == "adult" ? "happy" : "confident",
      ["hair"] = preset == "grandma" ? "bun" : "short",
      ["hairColorId"] = preset switch
      {
        "grandma" => "hair-white",
        "grandpa" => "hair-silver",
        _ => "hair-brown",
      },
      ["outfit"] = preset switch
      {
        "grandma" => "cardigan",
        "grandpa" => "blazer",
        _ => "shirt",
      },
      ["outfitColorId"] = preset switch
      {
        "grandma" => "outfit-lavender",
        "grandpa" => "outfit-slate",
        _ => "outfit-emerald",
      },
      ["accessoryId"] = preset == "adult" ? "none" : "glasses",
      ["funAccessoryId"] = "none",
      ["seasonalAccessoryId"] = "none",
    };

    return appearance;
  }

}
