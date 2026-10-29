using System;
using System.Linq;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House.Shop;

namespace LadirchenApp.Application.Aggregates;

internal static class HomePlacementReadModel
{
  public static void Ensure(JsonObject home, JsonObject profile)
  {
    var placements = RequireArray(home, "placements");
    var members = RequireArray(profile, "members").OfType<JsonObject>().ToArray();
    var pets = RequireArray(profile, "pets").OfType<JsonObject>().ToArray();
    var policy = new HouseInitialPlacementPolicy();

    AddMissing(placements, "family-ladi", "family-ladi", "ladi", policy.FamilyLadi());

    for (var index = 0; index < members.Length; index++)
    {
      var member = members[index];
      var memberId = RequiredString(member, "id");
      var role = ParseRole(RequiredString(member, "role"));
      AddMissing(
        placements,
        $"member-{memberId}",
        memberId,
        "member",
        policy.FamilyMember(index, role)
      );
    }

    for (var index = 0; index < pets.Length; index++)
    {
      var petId = RequiredString(pets[index], "id");
      AddMissing(
        placements,
        $"pet-{petId}",
        petId,
        "pet",
        policy.FamilyPet(index)
      );
    }
  }

  public static HousePlacement InitialFor(JsonObject profile, JsonObject placement)
  {
    var entityType = RequiredString(placement, "entityType");
    var entityId = RequiredString(placement, "entityId");
    var policy = new HouseInitialPlacementPolicy();

    if (entityType == "furniture")
    {
      return policy.Accessory(entityId);
    }

    if (entityType == "ladi" && entityId == "family-ladi")
    {
      return policy.FamilyLadi();
    }

    if (entityType == "member")
    {
      var members = RequireArray(profile, "members").OfType<JsonObject>().ToArray();
      var index = Array.FindIndex(members, member => StringValue(member, "id") == entityId);

      return index >= 0
        ? policy.FamilyMember(index, ParseRole(RequiredString(members[index], "role")))
        : throw new InvalidOperationException("The family member placement is unknown.");
    }

    if (entityType == "pet")
    {
      var pets = RequireArray(profile, "pets").OfType<JsonObject>().ToArray();
      var index = Array.FindIndex(pets, pet => StringValue(pet, "id") == entityId);

      return index >= 0
        ? policy.FamilyPet(index)
        : throw new InvalidOperationException("The family pet placement is unknown.");
    }

    throw new InvalidOperationException("The house entity is unknown.");
  }

  private static void AddMissing(
    JsonArray placements,
    string id,
    string entityId,
    string entityType,
    HousePlacement initial
  )
  {
    if (placements.OfType<JsonObject>().Any(item => StringValue(item, "id") == id))
    {
      return;
    }

    placements.Add(new JsonObject
    {
      ["id"] = id,
      ["entityId"] = entityId,
      ["entityType"] = entityType,
      ["zoneId"] = initial.Area,
      ["x"] = initial.X,
      ["y"] = initial.Y,
      ["scale"] = initial.Scale,
    });
  }

  private static JsonArray RequireArray(JsonObject source, string property)
  {
    return source[property] as JsonArray
      ?? throw new InvalidOperationException($"The '{property}' collection is required.");
  }

  private static string RequiredString(JsonObject source, string property)
  {
    return StringValue(source, property)
      ?? throw new InvalidOperationException($"The '{property}' value is required.");
  }

  private static string? StringValue(JsonObject source, string property)
  {
    return source[property]?.GetValue<string>();
  }

  private static MemberRole ParseRole(string value)
  {
    return value switch
    {
      "child" => MemberRole.Child,
      "guardian" => MemberRole.Guardian,
      _ => throw new InvalidOperationException("The family-member role is invalid."),
    };
  }
}
