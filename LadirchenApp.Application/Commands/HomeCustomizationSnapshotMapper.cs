using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.House.Shop;

namespace LadirchenApp.Application.Commands;

internal static class HomeCustomizationSnapshotMapper
{
  public static HomeCustomizationState Rehydrate(
    JsonObject home,
    JsonObject profile,
    int houseLevel
  )
  {
    ArgumentNullException.ThrowIfNull(home);
    ArgumentNullException.ThrowIfNull(profile);
    var presentation = home.DeepClone().AsObject();
    HomePlacementReadModel.Ensure(presentation, profile);
    var accessoryStates = ById(RequiredArray(presentation, "accessories"), "house accessory");
    var placementStates = ById(RequiredArray(presentation, "placements"), "house placement");
    var accessories = CreateAccessories(accessoryStates, placementStates);
    var roomDesignStates = ById(RequiredArray(presentation, "roomDesigns"), "room design");
    var selectedDesigns = SelectedDesigns(presentation);
    var roomDesigns = CreateRoomDesigns(roomDesignStates, selectedDesigns);
    var themeStates = ById(RequiredArray(presentation, "editions"), "house theme");
    var themes = CreateThemes(themeStates, roomDesigns);
    var shop = new FamilyHouseShop();
    shop.Restore(
      houseLevel,
      RequiredString(presentation, "selectedEditionId"),
      themes.Values
    );
    var furnitureSets = HouseShopCatalog.FurnitureSets.ToDictionary(
      item => item.Key,
      item => new FurnitureSet(
        item.Key,
        item.Value.Price,
        item.Value.MinimumLevel,
        item.Value.AccessoryIds.Select(id => accessories[id])
      ),
      StringComparer.Ordinal
    );
    var (placements, initialPlacements) = CreatePlacements(
      placementStates,
      profile,
      accessories
    );

    return new HomeCustomizationState(
      presentation,
      shop,
      ParseSubscriptionTier(profile),
      accessories,
      furnitureSets,
      themes,
      roomDesigns,
      placements,
      initialPlacements
    );
  }

  public static JsonObject Write(HomeCustomizationState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    var result = state.Presentation.DeepClone().AsObject();
    var accessories = ById(RequiredArray(result, "accessories"), "house accessory");

    foreach (var item in state.Accessories)
    {
      var target = Required(accessories, item.Id, "house accessory");
      target["owned"] = item.Owned;
      target["equipped"] = item.Equipped;
    }

    var placements = ById(RequiredArray(result, "placements"), "house placement");

    foreach (var entity in state.Placements)
    {
      var target = Required(placements, entity.Id, "house placement");
      target["zoneId"] = entity.Placement.Area;
      target["x"] = entity.Placement.X;
      target["y"] = entity.Placement.Y;
      target["scale"] = entity.Placement.Scale;
    }

    var themes = ById(RequiredArray(result, "editions"), "house theme");

    foreach (var theme in state.Themes)
    {
      Required(themes, theme.Id, "house theme")["owned"] = theme.Owned;
    }

    var roomDesigns = ById(RequiredArray(result, "roomDesigns"), "room design");

    foreach (var design in state.RoomDesigns)
    {
      Required(roomDesigns, design.Id, "room design")["owned"] = design.Owned;
    }

    result["selectedEditionId"] = state.Shop.SelectedThemeId;
    WriteSelectedRoomDesigns(result, state.RoomDesigns);

    return result;
  }

  private static Dictionary<string, HouseItem> CreateAccessories(
    IReadOnlyDictionary<string, JsonObject> states,
    IReadOnlyDictionary<string, JsonObject> placements
  )
  {
    var result = new Dictionary<string, HouseItem>(StringComparer.Ordinal);

    foreach (var catalogItem in HouseShopCatalog.Accessories)
    {
      var definition = catalogItem.Value;
      var state = Required(states, catalogItem.Key, "house accessory");
      var placement = placements.Values.SingleOrDefault(item =>
        RequiredString(item, "entityType") == "furniture"
        && RequiredString(item, "entityId") == catalogItem.Key
      ) ?? throw new FamilyCommandException("The house accessory placement does not exist.");
      var item = new HouseItem(
        catalogItem.Key,
        definition.Price,
        Category(catalogItem.Key),
        definition.Movable ? HouseItemMobility.Movable : HouseItemMobility.Fixed,
        definition.IntendedArea,
        definition.Initial,
        definition.MinimumLevel,
        definition.AssignedArea
      );
      item.Restore(
        RequiredBool(state, "owned"),
        RequiredBool(state, "equipped"),
        ReadPlacement(placement)
      );
      result.Add(catalogItem.Key, item);
    }

    return result;
  }

  private static Dictionary<string, RoomDesign> CreateRoomDesigns(
    IReadOnlyDictionary<string, JsonObject> states,
    IReadOnlyDictionary<string, string> selectedDesigns
  )
  {
    var result = new Dictionary<string, RoomDesign>(StringComparer.Ordinal);

    foreach (var catalogItem in HouseShopCatalog.RoomDesigns)
    {
      var definition = catalogItem.Value;
      var state = Required(states, catalogItem.Key, "room design");
      var design = new RoomDesign(
        catalogItem.Key,
        definition.ThemeId,
        definition.Area,
        definition.Price,
        definition.MinimumLevel
      );
      design.Restore(
        RequiredBool(state, "owned"),
        selectedDesigns.GetValueOrDefault(definition.Area) == catalogItem.Key
      );
      result.Add(catalogItem.Key, design);
    }

    return result;
  }

  private static Dictionary<string, HouseTheme> CreateThemes(
    IReadOnlyDictionary<string, JsonObject> states,
    IReadOnlyDictionary<string, RoomDesign> roomDesigns
  )
  {
    var result = new Dictionary<string, HouseTheme>(StringComparer.Ordinal);

    foreach (var catalogItem in HouseShopCatalog.Themes)
    {
      var theme = new HouseTheme(
        catalogItem.Key,
        catalogItem.Value,
        roomDesigns.Values.Where(design => design.ThemeId == catalogItem.Key)
      );
      theme.Restore(RequiredBool(Required(states, catalogItem.Key, "house theme"), "owned"));
      result.Add(catalogItem.Key, theme);
    }

    return result;
  }

  private static (
    Dictionary<string, HouseLayoutEntity> Placements,
    Dictionary<string, HousePlacement> InitialPlacements
  ) CreatePlacements(
    IReadOnlyDictionary<string, JsonObject> states,
    JsonObject profile,
    IReadOnlyDictionary<string, HouseItem> accessories
  )
  {
    var placements = new Dictionary<string, HouseLayoutEntity>(StringComparer.Ordinal);
    var initialPlacements = new Dictionary<string, HousePlacement>(StringComparer.Ordinal);

    foreach (var state in states.Values)
    {
      var id = RequiredString(state, "id");
      var entityType = RequiredString(state, "entityType");
      var entityId = RequiredString(state, "entityId");
      var active = true;
      HousePlacement initialPlacement;

      if (entityType == "furniture")
      {
        var item = accessories.GetValueOrDefault(entityId)
          ?? throw new FamilyCommandException("The house accessory is unknown.");
        active = item.Owned && item.Equipped;
        initialPlacement = item.InitialPlacement;
      }
      else
      {
        initialPlacement = HomePlacementReadModel.InitialFor(profile, state);
      }

      placements.Add(id, new HouseLayoutEntity(id, entityType, entityId, ReadPlacement(state), active));
      initialPlacements.Add(id, initialPlacement);
    }

    return (placements, initialPlacements);
  }

  private static IReadOnlyDictionary<string, string> SelectedDesigns(JsonObject state)
  {
    var result = new Dictionary<string, string>(StringComparer.Ordinal);

    foreach (var item in RequiredArray(state, "selectedRoomDesigns").OfType<JsonObject>())
    {
      if (!result.TryAdd(RequiredString(item, "zoneId"), RequiredString(item, "designId")))
      {
        throw new FamilyCommandException("A room can have only one selected design.");
      }
    }

    foreach (var selected in result)
    {
      if (
        !HouseShopCatalog.RoomDesigns.TryGetValue(selected.Value, out var definition)
        || definition.Area != selected.Key
      )
      {
        throw new FamilyCommandException("The selected room design is invalid.");
      }
    }

    return result;
  }

  private static void WriteSelectedRoomDesigns(
    JsonObject state,
    IEnumerable<RoomDesign> roomDesigns
  )
  {
    var selected = roomDesigns.Where(item => item.Selected).ToArray();
    var selectedByRoom = selected.GroupBy(item => item.Room).ToArray();

    if (selectedByRoom.Any(group => group.Count() != 1))
    {
      throw new FamilyCommandException("A room can have only one selected design.");
    }

    var stored = RequiredArray(state, "selectedRoomDesigns");
    var byRoom = stored
      .OfType<JsonObject>()
      .GroupBy(item => RequiredString(item, "zoneId"), StringComparer.Ordinal)
      .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    var selectedRooms = selectedByRoom.Select(group => group.Key).ToHashSet(StringComparer.Ordinal);

    foreach (var obsolete in byRoom.Where(item => !selectedRooms.Contains(item.Key)).Select(item => item.Value).ToArray())
    {
      stored.Remove(obsolete);
    }

    foreach (var group in selectedByRoom)
    {
      if (!byRoom.TryGetValue(group.Key, out var target))
      {
        target = new JsonObject { ["zoneId"] = group.Key };
        stored.Add(target);
      }

      target["designId"] = group.Single().Id;
    }
  }

  private static Dictionary<string, JsonObject> ById(JsonArray items, string description)
  {
    var result = new Dictionary<string, JsonObject>(StringComparer.Ordinal);

    foreach (var item in items)
    {
      var value = item as JsonObject
        ?? throw new FamilyCommandException($"Every {description} must be an object.");
      var id = RequiredString(value, "id");

      if (!result.TryAdd(id, value))
      {
        throw new FamilyCommandException($"The stored {description} identity is duplicated.");
      }
    }

    return result;
  }

  private static JsonObject Required(
    IReadOnlyDictionary<string, JsonObject> values,
    string id,
    string description
  )
  {
    return values.GetValueOrDefault(id)
      ?? throw new FamilyCommandException($"The {description} does not exist.");
  }

  private static HousePlacement ReadPlacement(JsonObject value)
  {
    return new HousePlacement(
      RequiredString(value, "zoneId"),
      RequiredDecimal(value, "x"),
      RequiredDecimal(value, "y"),
      RequiredDecimal(value, "scale")
    );
  }

  private static HouseItemCategory Category(string id)
  {
    return HouseShopCatalog.CatalogAreaForAccessory(id) switch
    {
      "outside" => HouseItemCategory.Garden,
      "special" => HouseItemCategory.Special,
      _ => HouseItemCategory.Furniture,
    };
  }

  private static HouseSubscriptionTier ParseSubscriptionTier(JsonObject profile)
  {
    return RequiredString(profile, "subscriptionTier") switch
    {
      "free" => HouseSubscriptionTier.Free,
      "pro" => HouseSubscriptionTier.Pro,
      _ => throw new FamilyCommandException("The family subscription tier is invalid."),
    };
  }

  private static JsonArray RequiredArray(JsonObject value, string property)
  {
    return value[property] as JsonArray
      ?? throw new FamilyCommandException($"The {property} collection is required.");
  }

  private static string RequiredString(JsonObject value, string property)
  {
    return value[property]?.GetValue<string>()
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  private static bool RequiredBool(JsonObject value, string property)
  {
    return value[property]?.GetValue<bool>()
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }

  private static decimal RequiredDecimal(JsonObject value, string property)
  {
    return value[property]?.GetValue<decimal>()
      ?? throw new FamilyCommandException($"The {property} value is required.");
  }
}