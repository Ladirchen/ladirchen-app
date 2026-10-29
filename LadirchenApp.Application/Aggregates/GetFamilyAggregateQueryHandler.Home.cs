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
  private static FamilyAggregateSnapshot ProjectHomeCatalog(
    FamilyAggregateSnapshot home,
    FamilyAggregateSnapshot progression,
    FamilyAggregateSnapshot profile,
    FamilyAggregateSnapshot savings,
    FamilyAggregateSnapshot rewardShop
  )
  {
    var state = JsonNode.Parse(home.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The home customization state must be an object.");
    var profileState = JsonNode.Parse(profile.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The family profile state must be an object.");
    HomePlacementReadModel.Ensure(state, profileState);
    var houseLevel = progression.State.GetProperty("houseLevel").GetInt32();
    var accessories = state["accessories"] as JsonArray
      ?? throw new InvalidOperationException("The house accessory collection is required.");
    var editions = state["editions"] as JsonArray
      ?? throw new InvalidOperationException("The house edition collection is required.");
    var roomDesigns = state["roomDesigns"] as JsonArray
      ?? throw new InvalidOperationException("The room design collection is required.");
    var members = profileState["members"] as JsonArray
      ?? throw new InvalidOperationException("The family member collection is required.");
    var balances = JsonNode.Parse(savings.State.GetRawText())?["balances"] as JsonObject
      ?? throw new InvalidOperationException("The savings balance collection is required.");
    var rewards = JsonNode.Parse(rewardShop.State.GetRawText())?["rewards"] as JsonArray
      ?? throw new InvalidOperationException("The reward collection is required.");
    var availableBalances = new JsonObject();

    foreach (var balance in balances)
    {
      var memberId = balance.Key;
      availableBalances[memberId] = AvailableBalance(balance.Value?.GetValue<decimal>() ?? 0, rewards, memberId);
    }
    var ownedAccessoryIds = accessories.OfType<JsonObject>()
      .Where(item => item["owned"]?.GetValue<bool>() ?? false)
      .Select(item => item["id"]!.GetValue<string>())
      .ToHashSet(StringComparer.Ordinal);

    state["capabilities"] = new JsonObject
    {
      ["canArrangeHouse"] = profileState["subscriptionTier"]?.GetValue<string>() == "pro",
    };
    state["catalog"] = new JsonObject
    {
      ["accessories"] = new JsonArray(HouseShopCatalog.Accessories.Select(item => new JsonObject
      {
        ["available"] = houseLevel >= item.Value.MinimumLevel,
        ["catalogArea"] = HouseShopCatalog.CatalogAreaForAccessory(item.Key),
        ["id"] = item.Key,
        ["minimumHouseLevel"] = item.Value.MinimumLevel,
        ["owned"] = ownedAccessoryIds.Contains(item.Key),
        ["price"] = item.Value.Price,
        ["purchasableByMemberIds"] = PurchasableByMemberIds(
          members,
          availableBalances,
          houseLevel >= item.Value.MinimumLevel && !ownedAccessoryIds.Contains(item.Key),
          item.Value.Price
        ),
      }).ToArray<JsonNode?>()),
      ["areas"] = new JsonArray(HouseShopCatalog.AreaMinimumLevels.Select(item => new JsonObject
      {
        ["available"] = houseLevel >= item.Value,
        ["id"] = item.Key,
        ["minimumHouseLevel"] = item.Value,
      }).ToArray<JsonNode?>()),
      ["furnitureSets"] = new JsonArray(HouseShopCatalog.FurnitureSets.Select(item => new JsonObject
      {
        ["accessoryIds"] = new JsonArray(item.Value.AccessoryIds.Select(id => JsonValue.Create(id)).ToArray<JsonNode?>()),
        ["available"] = houseLevel >= item.Value.MinimumLevel,
        ["catalogArea"] = item.Value.AccessoryIds.Any(id => HouseShopCatalog.CatalogAreaForAccessory(id) == "outside")
          ? "outside"
          : "inside",
        ["id"] = item.Key,
        ["minimumHouseLevel"] = item.Value.MinimumLevel,
        ["owned"] = item.Value.AccessoryIds.All(ownedAccessoryIds.Contains),
        ["price"] = item.Value.Price,
        ["purchasableByMemberIds"] = PurchasableByMemberIds(
          members,
          availableBalances,
          houseLevel >= item.Value.MinimumLevel && !item.Value.AccessoryIds.All(ownedAccessoryIds.Contains),
          item.Value.Price
        ),
      }).ToArray<JsonNode?>()),
      ["houseStages"] = new JsonArray(HouseShopCatalog.HouseStages.Select(item => new JsonObject
      {
        ["active"] = houseLevel == item.Level,
        ["id"] = item.Id,
        ["level"] = item.Level,
        ["roomCount"] = item.RoomCount,
      }).ToArray<JsonNode?>()),
      ["roomDesigns"] = new JsonArray(HouseShopCatalog.RoomDesigns.Select(item => new JsonObject
      {
        ["available"] = houseLevel >= item.Value.MinimumLevel,
        ["areaId"] = item.Value.Area,
        ["id"] = item.Key,
        ["minimumHouseLevel"] = item.Value.MinimumLevel,
        ["owned"] = roomDesigns.OfType<JsonObject>().Any(design =>
          design["id"]?.GetValue<string>() == item.Key && (design["owned"]?.GetValue<bool>() ?? false)),
        ["price"] = item.Value.Price,
        ["purchasableByMemberIds"] = PurchasableByMemberIds(
          members,
          availableBalances,
          houseLevel >= item.Value.MinimumLevel && !roomDesigns.OfType<JsonObject>().Any(design =>
            design["id"]?.GetValue<string>() == item.Key && (design["owned"]?.GetValue<bool>() ?? false)),
          item.Value.Price
        ),
        ["themeId"] = item.Value.ThemeId,
      }).ToArray<JsonNode?>()),
      ["themes"] = new JsonArray(HouseShopCatalog.Themes.Select(item => new JsonObject
      {
        ["available"] = true,
        ["id"] = item.Key,
        ["owned"] = editions.OfType<JsonObject>().Any(edition =>
          edition["id"]?.GetValue<string>() == item.Key && (edition["owned"]?.GetValue<bool>() ?? false)),
        ["price"] = item.Value,
        ["purchasableByMemberIds"] = PurchasableByMemberIds(
          members,
          availableBalances,
          !editions.OfType<JsonObject>().Any(edition =>
            edition["id"]?.GetValue<string>() == item.Key && (edition["owned"]?.GetValue<bool>() ?? false)),
          item.Value
        ),
      }).ToArray<JsonNode?>()),
    };

    using var document = JsonDocument.Parse(state.ToJsonString());

    return home with { State = document.RootElement.Clone() };
  }

  private static JsonArray PurchasableByMemberIds(
    IEnumerable<JsonNode?> members,
    JsonObject balances,
    bool available,
    decimal price
  )
  {
    return new JsonArray(members
      .OfType<JsonObject>()
      .Where(member => available && member["role"]?.GetValue<string>() == "child")
      .Where(member => (balances[member["id"]!.GetValue<string>()]?.GetValue<decimal>() ?? 0) >= price)
      .Select(member => JsonValue.Create(member["id"]!.GetValue<string>()))
      .ToArray<JsonNode?>());
  }

  private static decimal AvailableBalance(decimal balance, IEnumerable<JsonNode?> rewards, string memberId)
  {
    var reserved = rewards
      .OfType<JsonObject>()
      .Where(reward => reward["status"]?.GetValue<string>() == "requested")
      .Where(reward => reward["requesterId"]?.GetValue<string>() == memberId)
      .Sum(reward => reward["price"]?.GetValue<decimal>() ?? 0);

    return Math.Max(0, balance - reserved);
  }

}
