using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public static class InitialFamilyAggregateFactory
{
  private static readonly HouseEnergyPolicy HouseEnergy = new();
  private static readonly HashSet<string> InitiallyOwnedAccessories =
    ["flower-boxes", "round-rug", "cozy-sofa", "kitchen-counter", "retro-fridge", "family-dining-table"];
  private static readonly HashSet<string> InitiallyEquippedAccessories = new(InitiallyOwnedAccessories);
  private static readonly HashSet<string> InitiallyOwnedRoomDesigns =
    ["garden-ladi-hills", "living-ladi-classic", "kitchen-ladi-classic"];

  public static IReadOnlyList<PendingFamilyAggregateSave> Create(
    FamilyId familyId,
    FamilyMemberId actorId,
    string guardianName
  )
  {
    var profile = new JsonObject
    {
      ["members"] = new JsonArray
      {
        new JsonObject
        {
          ["id"] = actorId.Value,
          ["name"] = guardianName,
          ["avatar"] = "🧑",
          ["color"] = "#58aa82",
          ["role"] = "guardian",
          ["guardianAccess"] = "admin",
          ["participatesInWeeklyGoal"] = false,
          ["weeklyStreak"] = 0,
          ["appearance"] = DefaultGuardianAppearance(),
        },
      },
      ["onboardingCompleted"] = false,
      ["pets"] = new JsonArray(),
      ["subscriptionTier"] = "free",
      ["timeZone"] = "Europe/Zurich",
    };
    var contributions = new JsonObject
    {
      ["contributions"] = new JsonArray(),
      ["perfectRatingBonusPercent"] = ContributionRewardPolicy.DefaultPerfectRatingBonus.Value,
      ["promotions"] = new JsonArray(),
      ["usedContributionIds"] = new JsonArray(),
    };
    var savings = new JsonObject
    {
      ["balances"] = new JsonObject(),
      ["familyCurrencyCode"] = "CHF",
      ["goals"] = new JsonArray(),
      ["ladirchenPerCurrencyUnit"] = FamilyCurrencySettingsPolicy.DefaultLadirchenPerCurrencyUnit,
      ["ledger"] = new JsonArray(),
      ["appliedOperationIds"] = new JsonArray(),
      ["pendingGuardianGifts"] = new JsonArray(),
      ["settledInterestPeriods"] = new JsonArray(),
      ["vouchers"] = new JsonArray(),
    };
    var progression = new JsonObject
    {
      ["completedWeeklyStreak"] = 0,
      ["closedWeekOutcomes"] = new JsonObject(),
      ["currentWeekTarget"] = PersonalStreakPolicy.DefaultWeeklyTargetDays,
      ["familyEnergy"] = HouseEnergyPolicy.DefaultEnergy,
      ["houseLevel"] = 0,
      ["houseVisualLevel"] = HouseEnergy.VisualLevel(HouseEnergyPolicy.DefaultEnergy),
      ["maximumSavingsInterestRate"] = SavingsInterestPolicy.Default.MaximumRate.Value,
      ["memberProgressions"] = new JsonObject(),
      ["weeklyEvaluations"] = new JsonObject(),
    };
    var rewardShop = new JsonObject { ["rewards"] = new JsonArray() };
    var home = CreateInitialHomeCustomization();

    return
    [
      CreateInitialSave(familyId, actorId, "family-profile", profile),
      CreateInitialSave(familyId, actorId, "contributions", contributions),
      CreateInitialSave(familyId, actorId, "savings", savings),
      CreateInitialSave(familyId, actorId, "family-progression", progression),
      CreateInitialSave(familyId, actorId, "reward-shop", rewardShop),
      CreateInitialSave(familyId, actorId, "home-customization", home),
    ];
  }

  private static JsonObject CreateInitialHomeCustomization()
  {
    var accessories = new JsonArray();
    var placements = new JsonArray();

    foreach (var item in HouseShopCatalog.Accessories)
    {
      accessories.Add(
        new JsonObject
        {
          ["id"] = item.Key,
          ["equipped"] = InitiallyEquippedAccessories.Contains(item.Key),
          ["owned"] = InitiallyOwnedAccessories.Contains(item.Key),
        }
      );
      placements.Add(
        new JsonObject
        {
          ["id"] = $"furniture-{item.Key}",
          ["entityId"] = item.Key,
          ["entityType"] = "furniture",
          ["zoneId"] = item.Value.Initial.Area,
          ["x"] = item.Value.Initial.X,
          ["y"] = item.Value.Initial.Y,
          ["scale"] = item.Value.Initial.Scale,
        }
      );
    }

    return new JsonObject
    {
      ["accessories"] = accessories,
      ["editions"] = new JsonArray(
        HouseShopCatalog.Themes.Select(item => new JsonObject { ["id"] = item.Key, ["owned"] = item.Key == "sunny-dollhouse" }).ToArray<JsonNode?>()
      ),
      ["placements"] = placements,
      ["roomDesigns"] = new JsonArray(
        HouseShopCatalog.RoomDesigns.Select(item => new JsonObject { ["id"] = item.Key, ["owned"] = InitiallyOwnedRoomDesigns.Contains(item.Key) }).ToArray<JsonNode?>()
      ),
      ["selectedEditionId"] = "sunny-dollhouse",
      ["selectedRoomDesigns"] = new JsonArray
      {
        new JsonObject { ["designId"] = "garden-ladi-hills", ["zoneId"] = "garden" },
        new JsonObject { ["designId"] = "living-ladi-classic", ["zoneId"] = "living-room" },
        new JsonObject { ["designId"] = "kitchen-ladi-classic", ["zoneId"] = "kitchen" },
      },
    };
  }

  private static PendingFamilyAggregateSave CreateInitialSave(
    FamilyId familyId,
    FamilyMemberId actorId,
    string aggregateType,
    JsonObject state
  )
  {
    using var document = JsonDocument.Parse(state.ToJsonString());
    var command = new SaveFamilyAggregateCommand(familyId, aggregateType, 0, document.RootElement.Clone(), actorId);

    return new PendingFamilyAggregateSave(command, FamilyAggregateSchema.GetVersion(aggregateType));
  }

  private static JsonObject DefaultGuardianAppearance()
  {
    return new JsonObject
    {
      ["age"] = "adult",
      ["skinToneId"] = "skin-medium",
      ["faceShape"] = "soft",
      ["face"] = "happy",
      ["hair"] = "short",
      ["hairColorId"] = "hair-brown",
      ["outfit"] = "shirt",
      ["outfitColorId"] = "outfit-emerald",
      ["accessoryId"] = "none",
      ["funAccessoryId"] = "none",
      ["seasonalAccessoryId"] = "none",
    };
  }
}
