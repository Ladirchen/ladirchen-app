using System;
using System.Linq;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.House.Shop;

namespace LadirchenApp.Application.Aggregates;

internal static class HomePlacementReadModel
{
  public static void Ensure(JsonObject home, JsonObject profile)
  {
    var placements = RequireArray(home, "placements");
    var members = RequireArray(profile, "members").OfType<JsonObject>().ToArray();
    var pets = RequireArray(profile, "pets").OfType<JsonObject>().ToArray();

    AddMissing(placements, "family-ladi", "family-ladi", "ladi", new HousePlacement("living-room", 18, 30, 0.78m));

    for (var index = 0; index < members.Length; index++)
    {
      var member = members[index];
      var memberId = RequiredString(member, "id");
      var scale = StringValue(member, "role") == "guardian" ? 0.78m : 0.75m;
      AddMissing(
        placements,
        $"member-{memberId}",
        memberId,
        "member",
        new HousePlacement("living-room", Math.Min(75, 35 + index * 10), 72, scale)
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
        new HousePlacement("living-room", Math.Min(88, 18 + index * 14), 80, 0.72m)
      );
    }
  }

  public static HousePlacement InitialFor(JsonObject profile, JsonObject placement)
  {
    var entityType = RequiredString(placement, "entityType");
    var entityId = RequiredString(placement, "entityId");

    if (entityType == "furniture")
    {
      return HouseShopCatalog.Accessories.TryGetValue(entityId, out var definition)
        ? definition.Initial
        : throw new InvalidOperationException("The house accessory is unknown.");
    }

    var temporaryHome = new JsonObject { ["placements"] = new JsonArray() };
    Ensure(temporaryHome, profile);
    var initial = RequireArray(temporaryHome, "placements")
      .OfType<JsonObject>()
      .FirstOrDefault(item => StringValue(item, "entityType") == entityType && StringValue(item, "entityId") == entityId)
      ?? throw new InvalidOperationException("The house entity is unknown.");

    return new HousePlacement(
      RequiredString(initial, "zoneId"),
      RequiredDecimal(initial, "x"),
      RequiredDecimal(initial, "y"),
      RequiredDecimal(initial, "scale")
    );
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

  private static decimal RequiredDecimal(JsonObject source, string property)
  {
    return source[property]?.GetValue<decimal>()
      ?? throw new InvalidOperationException($"The '{property}' value is required.");
  }
}
