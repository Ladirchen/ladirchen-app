using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Family;
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
    var profileState = ParseHomeObject(profile, "family profile");
    var savingsState = ParseHomeObject(savings, "savings");
    var rewardShopState = ParseHomeObject(rewardShop, "reward shop");
    var family = FamilyProfileSnapshotMapper.Rehydrate(profile.FamilyId, profileState);
    var economy = FamilyEconomySnapshotMapper.RehydrateRewardShop(
      family,
      rewardShopState,
      savingsState
    );
    var houseLevel = FamilyProgressionSnapshotMapper.ReadHouseLevel(
      ParseHomeObject(progression, "family progression")
    );
    var customization = HomeCustomizationSnapshotMapper.Rehydrate(
      ParseHomeObject(home, "home customization"),
      profileState,
      0,
      houseLevel
    );
    var state = customization.Presentation;
    var activeMembers = family.Members.Where(member => !member.InvitationPending).ToArray();
    var availableBalances = economy.Wallets.ToDictionary(
      wallet => wallet.Id,
      wallet => Math.Max(0, wallet.Available)
    );

    state["capabilities"] = new JsonObject
    {
      ["canArrangeHouse"] = customization.Shop.CanArrange(customization.SubscriptionTier),
    };
    state["catalog"] = new JsonObject
    {
      ["accessories"] = new JsonArray(HouseShopCatalog.Accessories.Select(entry =>
      {
        var item = customization.Accessory(entry.Key);

        return new JsonObject
        {
          ["available"] = customization.Shop.IsAvailableAtCurrentLevel(item.MinimumLevel),
          ["catalogArea"] = HouseShopCatalog.CatalogAreaForAccessory(item.Id),
          ["id"] = item.Id,
          ["minimumHouseLevel"] = item.MinimumLevel,
          ["owned"] = item.Owned,
          ["price"] = item.Price,
          ["purchasableByMemberIds"] = HomePurchasableByMemberIds(
            activeMembers,
            availableBalances,
            (member, availableBalance) => customization.Shop.CanPurchaseAccessory(
              item,
              member.Role == MemberRole.Child,
              availableBalance
            )
          ),
        };
      }).ToArray<JsonNode?>()),
      ["areas"] = new JsonArray(HouseShopCatalog.AreaMinimumLevels.Select(entry => new JsonObject
      {
        ["available"] = customization.Shop.IsAvailableAtCurrentLevel(entry.Value),
        ["id"] = entry.Key,
        ["minimumHouseLevel"] = entry.Value,
      }).ToArray<JsonNode?>()),
      ["furnitureSets"] = new JsonArray(HouseShopCatalog.FurnitureSets.Select(entry =>
      {
        var set = customization.FurnitureSet(entry.Key);

        return new JsonObject
        {
          ["accessoryIds"] = new JsonArray(
            set.Items.Select(item => JsonValue.Create(item.Id)).ToArray<JsonNode?>()
          ),
          ["available"] = customization.Shop.IsAvailableAtCurrentLevel(set.MinimumLevel),
          ["catalogArea"] = set.Items.Any(item =>
            HouseShopCatalog.CatalogAreaForAccessory(item.Id) == "outside"
          )
            ? "outside"
            : "inside",
          ["id"] = set.Id,
          ["minimumHouseLevel"] = set.MinimumLevel,
          ["owned"] = set.Owned,
          ["price"] = set.Price,
          ["purchasableByMemberIds"] = HomePurchasableByMemberIds(
            activeMembers,
            availableBalances,
            (member, availableBalance) => customization.Shop.CanPurchaseSet(
              set,
              member.Role == MemberRole.Child,
              availableBalance
            )
          ),
        };
      }).ToArray<JsonNode?>()),
      ["houseStages"] = new JsonArray(HouseShopCatalog.HouseStages.Select(stage => new JsonObject
      {
        ["active"] = houseLevel == stage.Level,
        ["id"] = stage.Id,
        ["level"] = stage.Level,
        ["roomCount"] = stage.RoomCount,
      }).ToArray<JsonNode?>()),
      ["roomDesigns"] = new JsonArray(HouseShopCatalog.RoomDesigns.Select(entry =>
      {
        var design = customization.RoomDesign(entry.Key);
        var theme = customization.ThemeFor(design);

        return new JsonObject
        {
          ["available"] = customization.Shop.IsAvailableAtCurrentLevel(design.MinimumLevel),
          ["areaId"] = design.Room,
          ["id"] = design.Id,
          ["minimumHouseLevel"] = design.MinimumLevel,
          ["owned"] = design.Owned,
          ["price"] = design.Price,
          ["purchasableByMemberIds"] = HomePurchasableByMemberIds(
            activeMembers,
            availableBalances,
            (member, availableBalance) => customization.Shop.CanPurchaseDesign(
              design,
              theme,
              member.Role == MemberRole.Child,
              availableBalance
            )
          ),
          ["themeId"] = design.ThemeId,
        };
      }).ToArray<JsonNode?>()),
      ["themes"] = new JsonArray(HouseShopCatalog.Themes.Select(entry =>
      {
        var theme = customization.Theme(entry.Key);

        return new JsonObject
        {
          ["available"] = true,
          ["id"] = theme.Id,
          ["owned"] = theme.Owned,
          ["price"] = theme.Price,
          ["purchasableByMemberIds"] = HomePurchasableByMemberIds(
            activeMembers,
            availableBalances,
            (member, availableBalance) => customization.Shop.CanPurchaseTheme(
              theme,
              member.Role == MemberRole.Child,
              availableBalance
            )
          ),
        };
      }).ToArray<JsonNode?>()),
    };

    using var document = JsonDocument.Parse(state.ToJsonString());

    return home with { State = document.RootElement.Clone() };
  }

  private static JsonArray HomePurchasableByMemberIds(
    IEnumerable<FamilyMember> members,
    IReadOnlyDictionary<FamilyMemberId, decimal> availableBalances,
    Func<FamilyMember, decimal, bool> canPurchase
  )
  {
    return new JsonArray(members
      .Where(member => canPurchase(member, availableBalances.GetValueOrDefault(member.Id)))
      .Select(member => JsonValue.Create(member.Id.Value))
      .ToArray<JsonNode?>());
  }

  private static JsonObject ParseHomeObject(FamilyAggregateSnapshot snapshot, string name)
  {
    return JsonNode.Parse(snapshot.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException($"The {name} state must be an object.");
  }
}
