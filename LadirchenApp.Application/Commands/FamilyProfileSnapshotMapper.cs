using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Setup;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Application.Commands;

internal static class FamilyProfileSnapshotMapper
{
  public static FamilyAggregate Rehydrate(FamilyId familyId, JsonObject profile)
  {
    ArgumentNullException.ThrowIfNull(familyId);
    ArgumentNullException.ThrowIfNull(profile);
    var activeChild = OptionalString(profile, "activeChildId");
    var members = ReadMembers(RequiredArray(profile, "members"));
    var pets = ReadPets(RequiredArray(profile, "pets"));
    var state = new FamilyState(
      familyId,
      new IanaTimeZone(RequiredString(profile, "timeZone")),
      ParseCurrency(OptionalString(profile, "familyCurrencyCode")),
      new ExchangeRate(OptionalInt(profile, "ladirchenPerCurrencyUnit") ?? 1),
      activeChild is null ? null : new FamilyMemberId(activeChild),
      OptionalBool(profile, "onboardingCompleted") ?? false,
      members,
      pets
    );

    return FamilyAggregate.Restore(state);
  }

  public static IReadOnlyList<FamilyMemberState> ReadMembers(JsonArray members)
  {
    ArgumentNullException.ThrowIfNull(members);
    var result = new List<FamilyMemberState>(members.Count);

    foreach (var node in members)
    {
      var member = node as JsonObject
        ?? throw new FamilyCommandException("Every family member must be an object.");
      var role = ParseRole(RequiredString(member, "role"));
      var access = role == MemberRole.Guardian
        ? ParseGuardianAccess(RequiredString(member, "guardianAccess"))
        : GuardianAccess.Supporter;
      result.Add(
        new FamilyMemberState(
          new FamilyMemberId(RequiredString(member, "id")),
          RequiredString(member, "name"),
          role,
          access,
          OptionalBool(member, "invitationPending") ?? false,
          OptionalString(member, "nickname"),
          OptionalString(member, "email"),
          OptionalBool(member, "participatesInWeeklyGoal") ?? role == MemberRole.Child
        )
      );
    }

    return result;
  }

  public static IReadOnlyList<FamilyPetState> ReadPets(JsonArray pets)
  {
    ArgumentNullException.ThrowIfNull(pets);
    var result = new List<FamilyPetState>(pets.Count);

    foreach (var node in pets)
    {
      var pet = node as JsonObject
        ?? throw new FamilyCommandException("Every family pet must be an object.");
      result.Add(
        new FamilyPetState(
          new FamilyPetId(RequiredString(pet, "id")),
          RequiredString(pet, "name"),
          ParsePetKind(RequiredString(pet, "kind"))
        )
      );
    }

    return result;
  }

  public static JsonArray CreateSetupMemberPresentations(
    JsonObject profile,
    JsonArray members
  )
  {
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(members);
    var storedStreaks = ReadStoredWeeklyStreaks(profile);
    var presentations = new JsonArray();
    var catalog = new AvatarCatalog();

    foreach (var node in members)
    {
      var member = node as JsonObject
        ?? throw new FamilyCommandException("Every family member must be an object.");
      var id = RequiredString(member, "id");
      var role = ParseRole(RequiredString(member, "role"));
      var presentation = new JsonObject
      {
        ["id"] = id,
        ["avatar"] = RequiredString(member, "avatar"),
        ["color"] = RequiredString(member, "color"),
        ["weeklyStreak"] = storedStreaks.TryGetValue(id, out var storedStreak)
          ? storedStreak
          : 0,
      };

      if (member["appearance"] is JsonObject appearance)
      {
        var value = ReadAvatarAppearance(appearance);
        catalog.Validate(value, role == MemberRole.Child ? AvatarProfile.Child : AvatarProfile.Guardian);
        presentation["appearance"] = WriteAvatarAppearance(value);
      }

      presentations.Add(presentation);
    }

    return presentations;
  }

  public static JsonArray CreateSetupPetPresentations(JsonArray pets)
  {
    ArgumentNullException.ThrowIfNull(pets);
    var presentations = new JsonArray();

    foreach (var node in pets)
    {
      var pet = node as JsonObject
        ?? throw new FamilyCommandException("Every family pet must be an object.");
      presentations.Add(
        new JsonObject
        {
          ["id"] = RequiredString(pet, "id"),
          ["avatar"] = RequiredString(pet, "avatar"),
          ["color"] = RequiredString(pet, "color"),
        }
      );
    }

    return presentations;
  }

  public static AvatarAppearance ReadAvatarAppearance(JsonObject appearance)
  {
    ArgumentNullException.ThrowIfNull(appearance);

    return new AvatarAppearance(
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
  }

  public static GuardianAccess ParseGuardianAccess(string value)
  {
    return value switch
    {
      "admin" => GuardianAccess.Admin,
      "supporter" => GuardianAccess.Supporter,
      _ => throw new FamilyCommandException("The guardian access level is invalid."),
    };
  }

  public static JsonObject MemberPresentation(JsonObject profile, FamilyMemberId memberId)
  {
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(memberId);

    return RequiredArray(profile, "members")
      .OfType<JsonObject>()
      .SingleOrDefault(item => OptionalString(item, "id") == memberId.Value)
      ?? throw new FamilyCommandException("The family member presentation does not exist.");
  }

  public static void WriteTo(JsonObject profile, FamilyAggregate family)
  {
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(family);
    var currentMembers = RequiredArray(profile, "members")
      .OfType<JsonObject>()
      .Where(item => OptionalString(item, "id") is not null)
      .GroupBy(item => RequiredString(item, "id"), StringComparer.Ordinal)
      .ToDictionary(item => item.Key, item => item.First(), StringComparer.Ordinal);
    var serializedMembers = new JsonArray();

    foreach (var member in family.Members)
    {
      var serialized = currentMembers.TryGetValue(member.Id.Value, out var current)
        ? (JsonObject)current.DeepClone()
        : CreateMemberPresentation(member);
      WriteMember(serialized, member);
      serializedMembers.Add(serialized);
    }

    profile["members"] = serializedMembers;
    profile["pets"] = WritePets(profile, family.Pets);
    profile["timeZone"] = family.TimeZone.Value;
    profile["onboardingCompleted"] = family.OnboardingCompleted;

    if (family.ActiveChildId is null)
    {
      profile.Remove("activeChildId");
    }
    else
    {
      profile["activeChildId"] = family.ActiveChildId.Value;
    }
  }

  private static JsonArray WritePets(JsonObject profile, IReadOnlyList<FamilyPetState> pets)
  {
    var currentPets = RequiredArray(profile, "pets")
      .OfType<JsonObject>()
      .Where(item => OptionalString(item, "id") is not null)
      .GroupBy(item => RequiredString(item, "id"), StringComparer.Ordinal)
      .ToDictionary(item => item.Key, item => item.First(), StringComparer.Ordinal);
    var serializedPets = new JsonArray();

    foreach (var pet in pets)
    {
      var serialized = currentPets.TryGetValue(pet.Id.Value, out var current)
        ? (JsonObject)current.DeepClone()
        : new JsonObject { ["avatar"] = "🐾", ["color"] = "#7e8db8" };
      serialized["id"] = pet.Id.Value;
      serialized["name"] = pet.Name;
      serialized["kind"] = PetKind(pet.Kind);
      serializedPets.Add(serialized);
    }

    return serializedPets;
  }

  private static void WriteMember(JsonObject target, FamilyMember member)
  {
    target["id"] = member.Id.Value;
    target["name"] = member.Name;
    target["role"] = member.Role == MemberRole.Child ? "child" : "guardian";
    target["participatesInWeeklyGoal"] = member.ParticipatesInWeeklyGoal;
    target["invitationPending"] = member.InvitationPending;
    WriteOptionalString(target, "nickname", member.Nickname);
    WriteOptionalString(target, "email", member.EmailAddress);

    if (member.Role == MemberRole.Guardian)
    {
      target["guardianAccess"] = member.Access == GuardianAccess.Admin ? "admin" : "supporter";
    }
    else
    {
      target.Remove("guardianAccess");
    }
  }

  private static JsonObject CreateMemberPresentation(FamilyMember member)
  {
    if (member.Role == MemberRole.Guardian)
    {
      var preset = GuardianPreset(member.Name);

      return new JsonObject
      {
        ["avatar"] = "🧑",
        ["color"] = "#7e8db8",
        ["weeklyStreak"] = 0,
        ["appearance"] = GuardianAppearance(preset),
      };
    }

    return new JsonObject
    {
      ["avatar"] = "🧒",
      ["color"] = "#58aa82",
      ["weeklyStreak"] = 0,
    };
  }

  private static string GuardianPreset(string name)
  {
    var normalized = name.ToLowerInvariant();

    if (normalized.Contains("oma", StringComparison.Ordinal))
    {
      return "grandma";
    }

    return normalized.Contains("opa", StringComparison.Ordinal) ? "grandpa" : "adult";
  }

  private static JsonObject GuardianAppearance(string preset)
  {
    return new JsonObject
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
  }

  private static IReadOnlyDictionary<string, int> ReadStoredWeeklyStreaks(JsonObject profile)
  {
    return RequiredArray(profile, "members")
      .OfType<JsonObject>()
      .Where(item => OptionalString(item, "id") is not null)
      .GroupBy(item => RequiredString(item, "id"), StringComparer.Ordinal)
      .ToDictionary(
        item => item.Key,
        item => RequiredNonNegativeInt(item.First(), "weeklyStreak"),
        StringComparer.Ordinal
      );
  }

  private static JsonObject WriteAvatarAppearance(AvatarAppearance appearance)
  {
    return new JsonObject
    {
      ["age"] = appearance.Age,
      ["skinToneId"] = appearance.SkinTone,
      ["faceShape"] = appearance.FaceShape,
      ["face"] = appearance.Face,
      ["hair"] = appearance.Hair,
      ["hairColorId"] = appearance.HairColor,
      ["outfit"] = appearance.Outfit,
      ["outfitColorId"] = appearance.OutfitColor,
      ["accessoryId"] = appearance.Accessory,
      ["funAccessoryId"] = appearance.FunAccessory,
      ["seasonalAccessoryId"] = appearance.SeasonalAccessory,
    };
  }

  private static FamilyCurrency ParseCurrency(string? value)
  {
    return value switch
    {
      null or "CHF" => FamilyCurrency.CHF,
      "EUR" => FamilyCurrency.EUR,
      "HUF" => FamilyCurrency.HUF,
      _ => throw new FamilyCommandException("The family currency is invalid."),
    };
  }

  private static FamilyPetKind ParsePetKind(string value)
  {
    return value switch
    {
      "cat" => FamilyPetKind.Cat,
      "dog" => FamilyPetKind.Dog,
      "rabbit" => FamilyPetKind.Rabbit,
      "bird" => FamilyPetKind.Bird,
      "other" => FamilyPetKind.Other,
      _ => throw new FamilyCommandException("The family pet kind is invalid."),
    };
  }

  private static string PetKind(FamilyPetKind kind)
  {
    return kind switch
    {
      FamilyPetKind.Cat => "cat",
      FamilyPetKind.Dog => "dog",
      FamilyPetKind.Rabbit => "rabbit",
      FamilyPetKind.Bird => "bird",
      FamilyPetKind.Other => "other",
      _ => throw new FamilyCommandException("The family pet kind is invalid."),
    };
  }

  public static MemberRole ParseRole(string value)
  {
    return value switch
    {
      "child" => MemberRole.Child,
      "guardian" => MemberRole.Guardian,
      _ => throw new FamilyCommandException("The family member role is invalid."),
    };
  }

  private static JsonArray RequiredArray(JsonObject value, string property)
  {
    return value[property] as JsonArray
      ?? throw new FamilyCommandException($"The {property} collection is required.");
  }

  private static string RequiredString(JsonObject value, string property)
  {
    var result = OptionalString(value, property);

    if (string.IsNullOrWhiteSpace(result))
    {
      throw new FamilyCommandException($"The {property} value is required.");
    }

    return result;
  }

  private static string? OptionalString(JsonObject value, string property)
  {
    return value[property] is JsonValue json && json.TryGetValue<string>(out var result) ? result : null;
  }

  private static bool? OptionalBool(JsonObject value, string property)
  {
    return value[property] is JsonValue json && json.TryGetValue<bool>(out var result) ? result : null;
  }

  private static int? OptionalInt(JsonObject value, string property)
  {
    return value[property] is JsonValue json && json.TryGetValue<int>(out var result) ? result : null;
  }

  private static int RequiredNonNegativeInt(JsonObject value, string property)
  {
    var result = OptionalInt(value, property);

    if (result is null || result < 0)
    {
      throw new FamilyCommandException($"The stored {property} value is invalid.");
    }

    return result.Value;
  }

  private static void WriteOptionalString(JsonObject value, string property, string? text)
  {
    if (text is null)
    {
      value.Remove(property);
    }
    else
    {
      value[property] = text;
    }
  }
}
