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
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed partial class ExecuteFamilyCommandHandler
{
  private async Task<FamilyCommandResult> ExecuteHomeAsync(
    FamilyCommand command,
    JsonObject profile,
    JsonObject actor,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var home = await RequireSnapshotAsync(command.FamilyId, "home-customization", cancellationToken);
    var homeState = RequireObject(home.State);
    HomePlacementReadModel.Ensure(homeState, profile);
    var progression = await RequireSnapshotAsync(command.FamilyId, "family-progression", cancellationToken);
    var houseLevel = RequiredInt(RequireObject(progression.State), "houseLevel");
    FamilyAggregateSnapshot? savings = null;
    JsonObject? savingsState = null;
    JsonArray? rewardItems = null;

    if (command.CommandType.StartsWith("home.purchase-", StringComparison.Ordinal))
    {
      savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
      savingsState = RequireObject(savings.State);
      var rewardShop = await RequireSnapshotAsync(command.FamilyId, "reward-shop", cancellationToken);
      rewardItems = RequireArray(RequireObject(rewardShop.State), "rewards");
    }

    switch (command.CommandType)
    {
      case "home.purchase-accessory":
        PurchaseAccessory(homeState, savingsState!, rewardItems!, actor, payload);
        break;
      case "home.purchase-furniture-set":
        PurchaseFurnitureSet(homeState, savingsState!, rewardItems!, actor, payload, houseLevel);
        break;
      case "home.toggle-accessory":
        ToggleAccessory(homeState, payload);
        break;
      case "home.store-accessory":
        StoreAccessory(homeState, payload);
        break;
      case "home.place-accessory":
        PlaceAccessory(homeState, payload, houseLevel);
        break;
      case "home.purchase-theme":
        PurchaseTheme(homeState, savingsState!, rewardItems!, actor, payload);
        break;
      case "home.select-theme":
        SelectTheme(homeState, payload);
        break;
      case "home.purchase-room-design":
        PurchaseRoomDesign(homeState, savingsState!, rewardItems!, actor, payload, houseLevel);
        break;
      case "home.select-room-design":
        SelectRoomDesign(homeState, payload, houseLevel);
        break;
      case "home.move-entity":
        MoveHouseEntity(profile, homeState, payload, houseLevel);
        break;
      case "home.reset-entity":
        ResetHouseEntity(profile, homeState, payload, houseLevel);
        break;
      default:
        throw new FamilyCommandException("The home-customization command is not supported.");
    }

    var saves = new List<PendingFamilyAggregateSave> { CreateSave(home, homeState, command.ActorId) };

    if (savings is not null && savingsState is not null)
    {
      saves.Add(CreateSave(savings, savingsState, command.ActorId));
    }

    var changed = await _repository.SaveBatchAsync(saves, cancellationToken);

    foreach (var snapshot in changed)
    {
      await _publisher.PublishAsync(snapshot, cancellationToken);
    }

    return new FamilyCommandResult(changed);
  }

  private static void PurchaseAccessory(
    JsonObject home,
    JsonObject savings,
    JsonArray rewards,
    JsonObject actor,
    JsonObject payload
  )
  {
    Require(StringValue(actor, "role") == "child", "Only children may purchase house items.");
    var id = RequiredString(payload, "id");
    Require(HouseShopCatalog.Accessories.TryGetValue(id, out var definition), "The house accessory is unknown.");
    var accessory = RequireCatalogItem(home, "accessories", id);
    Require(!RequiredBool(accessory, "owned"), "The house accessory is already owned.");
    DebitAvailableBalance(savings, rewards, RequiredString(actor, "id"), definition.Price);
    accessory["owned"] = true;
    accessory["equipped"] = false;
  }

  private static void PurchaseFurnitureSet(
    JsonObject home,
    JsonObject savings,
    JsonArray rewards,
    JsonObject actor,
    JsonObject payload,
    int houseLevel
  )
  {
    Require(StringValue(actor, "role") == "child", "Only children may purchase furniture sets.");
    var id = RequiredString(payload, "id");
    Require(HouseShopCatalog.FurnitureSets.TryGetValue(id, out var definition), "The furniture set is unknown.");
    Require(houseLevel >= definition.MinimumLevel, "The house level is too low for this furniture set.");
    var items = definition.AccessoryIds.Select(itemId => RequireCatalogItem(home, "accessories", itemId)).ToArray();
    Require(items.Any(item => !RequiredBool(item, "owned")), "The furniture set is already owned.");
    DebitAvailableBalance(savings, rewards, RequiredString(actor, "id"), definition.Price);

    foreach (var item in items)
    {
      item["owned"] = true;
      item["equipped"] = false;
    }
  }

  private static void ToggleAccessory(JsonObject home, JsonObject payload)
  {
    var id = RequiredString(payload, "id");
    Require(HouseShopCatalog.Accessories.TryGetValue(id, out var definition), "The house accessory is unknown.");
    var accessory = RequireCatalogItem(home, "accessories", id);
    Require(RequiredBool(accessory, "owned"), "The house accessory is not owned.");
    Require(definition.Movable, "A fixed house accessory cannot be toggled.");
    accessory["equipped"] = !RequiredBool(accessory, "equipped");
  }

  private static void StoreAccessory(JsonObject home, JsonObject payload)
  {
    var id = RequiredString(payload, "id");
    Require(HouseShopCatalog.Accessories.TryGetValue(id, out var definition), "The house accessory is unknown.");
    var accessory = RequireCatalogItem(home, "accessories", id);
    Require(RequiredBool(accessory, "owned"), "The house accessory is not owned.");
    Require(RequiredBool(accessory, "equipped"), "The house accessory is already stored.");
    Require(definition.Movable, "A fixed house accessory cannot be stored.");
    accessory["equipped"] = false;
  }

  private static void PlaceAccessory(JsonObject home, JsonObject payload, int houseLevel)
  {
    var id = RequiredString(payload, "id");
    Require(HouseShopCatalog.Accessories.TryGetValue(id, out var definition), "The house accessory is unknown.");
    var accessory = RequireCatalogItem(home, "accessories", id);
    Require(RequiredBool(accessory, "owned"), "The house accessory is not owned.");
    Require(definition.Movable, "A fixed house accessory cannot be placed.");
    var requestedArea = RequiredString(payload, "zoneId");
    var targetArea = definition.IntendedArea == "outside"
      ? "garden"
      : requestedArea == "garden"
        ? definition.AssignedArea
        : requestedArea;
    RequireAreaAvailable(targetArea, houseLevel);
    var placement = RequireFurniturePlacement(home, id);

    if (StringValue(placement, "zoneId") != targetArea)
    {
      placement["zoneId"] = targetArea;
      placement["x"] = 50;
      placement["y"] = targetArea == "garden" ? 68 : 66;
    }

    accessory["equipped"] = true;
  }

  private static void PurchaseTheme(
    JsonObject home,
    JsonObject savings,
    JsonArray rewards,
    JsonObject actor,
    JsonObject payload
  )
  {
    Require(StringValue(actor, "role") == "child", "Only children may purchase house themes.");
    var id = RequiredString(payload, "id");
    Require(HouseShopCatalog.Themes.TryGetValue(id, out var price), "The house theme is unknown.");
    var theme = RequireCatalogItem(home, "editions", id);
    Require(!RequiredBool(theme, "owned"), "The house theme is already owned.");
    DebitAvailableBalance(savings, rewards, RequiredString(actor, "id"), price);
    theme["owned"] = true;
    ApplyTheme(home, id);
  }

  private static void SelectTheme(JsonObject home, JsonObject payload)
  {
    var id = RequiredString(payload, "id");
    Require(HouseShopCatalog.Themes.ContainsKey(id), "The house theme is unknown.");
    Require(RequiredBool(RequireCatalogItem(home, "editions", id), "owned"), "The house theme is not owned.");
    ApplyTheme(home, id);
  }

  private static void PurchaseRoomDesign(
    JsonObject home,
    JsonObject savings,
    JsonArray rewards,
    JsonObject actor,
    JsonObject payload,
    int houseLevel
  )
  {
    Require(StringValue(actor, "role") == "child", "Only children may purchase room designs.");
    var id = RequiredString(payload, "id");
    Require(HouseShopCatalog.RoomDesigns.TryGetValue(id, out var definition), "The room design is unknown.");
    Require(houseLevel >= definition.MinimumLevel, "The house level is too low for this room design.");
    Require(
      RequiredBool(RequireCatalogItem(home, "editions", definition.ThemeId), "owned"),
      "The matching house theme is not owned."
    );
    var design = RequireCatalogItem(home, "roomDesigns", id);
    Require(!RequiredBool(design, "owned"), "The room design is already owned.");
    DebitAvailableBalance(savings, rewards, RequiredString(actor, "id"), definition.Price);
    design["owned"] = true;
    home["selectedEditionId"] = definition.ThemeId;
    SetSelectedRoomDesign(home, definition.Area, id);
  }

  private static void SelectRoomDesign(JsonObject home, JsonObject payload, int houseLevel)
  {
    var id = RequiredString(payload, "id");
    Require(HouseShopCatalog.RoomDesigns.TryGetValue(id, out var definition), "The room design is unknown.");
    Require(houseLevel >= definition.MinimumLevel, "The house level is too low for this room design.");
    Require(RequiredBool(RequireCatalogItem(home, "roomDesigns", id), "owned"), "The room design is not owned.");
    Require(
      RequiredBool(RequireCatalogItem(home, "editions", definition.ThemeId), "owned"),
      "The matching house theme is not owned."
    );
    home["selectedEditionId"] = definition.ThemeId;
    SetSelectedRoomDesign(home, definition.Area, id);
  }

  private static void ApplyTheme(JsonObject home, string themeId)
  {
    home["selectedEditionId"] = themeId;

    foreach (var item in HouseShopCatalog.RoomDesigns.Where(item => item.Value.ThemeId == themeId))
    {
      RequireCatalogItem(home, "roomDesigns", item.Key)["owned"] = true;
      SetSelectedRoomDesign(home, item.Value.Area, item.Key);
    }
  }

  private static void SetSelectedRoomDesign(JsonObject home, string area, string designId)
  {
    var selected = RequireArray(home, "selectedRoomDesigns");
    var existing = selected.OfType<JsonObject>().FirstOrDefault(item => StringValue(item, "zoneId") == area);

    if (existing is null)
    {
      selected.Add(new JsonObject { ["designId"] = designId, ["zoneId"] = area });
    }
    else
    {
      existing["designId"] = designId;
    }
  }

  private static void MoveHouseEntity(
    JsonObject profile,
    JsonObject home,
    JsonObject payload,
    int houseLevel
  )
  {
    Require(StringValue(profile, "subscriptionTier") == "pro", "House arrangement requires a pro subscription.");
    var placement = RequireCatalogItem(home, "placements", RequiredString(payload, "placementId"));
    var area = RequiredString(payload, "zoneId");
    RequireAreaAvailable(area, houseLevel);
    RequireMovablePlacement(placement);
    var entityType = RequiredString(placement, "entityType");
    var entityId = RequiredString(placement, "entityId");
    var normalized = new HouseLayoutPolicy().Normalize(
      entityType,
      entityId,
      area,
      RequiredDecimal(payload, "x"),
      RequiredDecimal(payload, "y"),
      RequiredDecimal(placement, "scale")
    );
    var moved = new HouseLayoutEntity(
      RequiredString(placement, "id"),
      entityType,
      entityId,
      normalized,
      true
    );
    Require(
      !new HouseLayoutPolicy().CollidesWithFurniture(moved, HouseLayoutEntities(home)),
      "A family member or pet cannot overlap equipped furniture."
    );
    ApplyPlacement(placement, normalized);
  }

  private static void ResetHouseEntity(
    JsonObject profile,
    JsonObject home,
    JsonObject payload,
    int houseLevel
  )
  {
    Require(StringValue(profile, "subscriptionTier") == "pro", "House arrangement requires a pro subscription.");
    var placement = RequireCatalogItem(home, "placements", RequiredString(payload, "placementId"));
    RequireMovablePlacement(placement);
    HousePlacement initial;

    if (StringValue(placement, "entityType") == "furniture")
    {
      var furnitureId = RequiredString(placement, "entityId");
      Require(HouseShopCatalog.Accessories.TryGetValue(furnitureId, out var definition), "The house accessory is unknown.");
      initial = definition.Initial;
    }
    else
    {
      initial = HomePlacementReadModel.InitialFor(profile, placement);
    }

    RequireAreaAvailable(initial.Area, houseLevel);
    var entityType = RequiredString(placement, "entityType");
    var entityId = RequiredString(placement, "entityId");
    var layoutPolicy = new HouseLayoutPolicy();
    var normalized = layoutPolicy.Normalize(
      entityType,
      entityId,
      initial.Area,
      initial.X,
      initial.Y,
      initial.Scale
    );
    var reset = new HouseLayoutEntity(
      RequiredString(placement, "id"),
      entityType,
      entityId,
      normalized,
      true
    );
    Require(
      !layoutPolicy.CollidesWithFurniture(reset, HouseLayoutEntities(home)),
      "A family member or pet cannot overlap equipped furniture."
    );
    ApplyPlacement(placement, normalized);
  }

  private static void ApplyPlacement(JsonObject placement, HousePlacement value)
  {
    placement["zoneId"] = value.Area;
    placement["x"] = value.X;
    placement["y"] = value.Y;
    placement["scale"] = value.Scale;
  }

  private static IEnumerable<HouseLayoutEntity> HouseLayoutEntities(JsonObject home)
  {
    var accessoryStates = RequireArray(home, "accessories")
      .OfType<JsonObject>()
      .ToDictionary(item => RequiredString(item, "id"), StringComparer.Ordinal);

    foreach (var placement in RequireArray(home, "placements").OfType<JsonObject>())
    {
      var entityType = RequiredString(placement, "entityType");
      var entityId = RequiredString(placement, "entityId");
      var active = true;

      if (entityType == "furniture")
      {
        active = accessoryStates.TryGetValue(entityId, out var accessory)
          && RequiredBool(accessory, "owned")
          && RequiredBool(accessory, "equipped");
      }

      yield return new HouseLayoutEntity(
        RequiredString(placement, "id"),
        entityType,
        entityId,
        new HousePlacement(
          RequiredString(placement, "zoneId"),
          RequiredDecimal(placement, "x"),
          RequiredDecimal(placement, "y"),
          RequiredDecimal(placement, "scale")
        ),
        active
      );
    }
  }

  private static void RequireMovablePlacement(JsonObject placement)
  {
    if (StringValue(placement, "entityType") != "furniture")
    {
      return;
    }

    var entityId = RequiredString(placement, "entityId");
    Require(HouseShopCatalog.Accessories.TryGetValue(entityId, out var definition), "The house accessory is unknown.");
    Require(definition.Movable, "A fixed house accessory cannot be moved.");
  }

  private static void RequireAreaAvailable(string area, int houseLevel)
  {
    Require(HouseShopCatalog.AreaMinimumLevels.TryGetValue(area, out var minimumLevel), "The house area is unknown.");
    Require(houseLevel >= minimumLevel, "The house area is not unlocked.");
  }

  private static JsonObject RequireFurniturePlacement(JsonObject home, string accessoryId)
  {
    return RequireArray(home, "placements")
      .OfType<JsonObject>()
      .FirstOrDefault(item => StringValue(item, "entityType") == "furniture" && StringValue(item, "entityId") == accessoryId)
      ?? throw new FamilyCommandException("The house accessory placement does not exist.");
  }

  private static JsonObject RequireCatalogItem(JsonObject state, string collection, string id)
  {
    return FindById(RequireArray(state, collection), id)
      ?? throw new FamilyCommandException($"The {collection} item does not exist.");
  }

}
