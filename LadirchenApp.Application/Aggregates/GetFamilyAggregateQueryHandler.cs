using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed class GetFamilyAggregateQueryHandler
{
  private readonly IFamilyAggregateSnapshotRepository _repository;
  private readonly TimeProvider _timeProvider;

  public GetFamilyAggregateQueryHandler(IFamilyAggregateSnapshotRepository repository, TimeProvider timeProvider)
  {
    _repository = repository;
    _timeProvider = timeProvider;
  }

  public async Task<FamilyAggregateSnapshot?> HandleAsync(
    GetFamilyAggregateQuery query,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!FamilyAggregateSchema.TryGetVersion(query.AggregateType, out _))
    {
      throw new ArgumentException("The aggregate type is not supported.", nameof(query));
    }

    var snapshot = await _repository.GetAsync(query.FamilyId, query.AggregateType, cancellationToken);

    if (snapshot is null)
    {
      return snapshot;
    }

    if (query.AggregateType == "contributions")
    {
      var profile = await _repository.GetAsync(query.FamilyId, "family-profile", cancellationToken);

      return profile is null ? snapshot : ProjectContributionRewards(snapshot, profile);
    }

    if (query.AggregateType == "savings")
    {
      var progression = await _repository.GetAsync(query.FamilyId, "family-progression", cancellationToken);

      return progression is null ? snapshot : ProjectSavingsInterest(snapshot, progression);
    }

    if (query.AggregateType == "reward-shop")
    {
      var profile = await _repository.GetAsync(query.FamilyId, "family-profile", cancellationToken);

      return profile is null ? snapshot : ProjectRewardShop(snapshot, profile);
    }

    if (query.AggregateType == "home-customization")
    {
      var progression = await _repository.GetAsync(query.FamilyId, "family-progression", cancellationToken);

      return progression is null ? snapshot : ProjectHomeCatalog(snapshot, progression);
    }

    return snapshot;
  }

  private FamilyAggregateSnapshot ProjectContributionRewards(
    FamilyAggregateSnapshot contributions,
    FamilyAggregateSnapshot profile
  )
  {
    var state = JsonNode.Parse(contributions.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The contributions state must be an object.");
    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(profile.State.GetProperty("timeZone").GetString()!);
    var localNow = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), timeZone);
    var items = state["contributions"] as JsonArray
      ?? throw new InvalidOperationException("The contributions collection is required.");
    var promotions = state["promotions"] as JsonArray
      ?? throw new InvalidOperationException("The promotions collection is required.");

    foreach (var promotion in promotions.OfType<JsonObject>())
    {
      promotion["available"] = PromotionIsAvailable(promotion, localNow);
    }

    foreach (var contribution in items.OfType<JsonObject>())
    {
      var id = contribution["id"]?.GetValue<string>();
      var promotion = promotions.OfType<JsonObject>().FirstOrDefault(item =>
        item["contributionId"]?.GetValue<string>() == id
        && (item["available"]?.GetValue<bool>() ?? false)
      );
      var reward = contribution["reward"]?.GetValue<decimal>() ?? 0;
      var multiplier = promotion?["multiplier"]?.GetValue<decimal>() ?? 1;
      var teamworkBonus = promotion?["teamworkBonus"]?.GetValue<decimal>() ?? 0;
      contribution["projectedReward"] = reward * multiplier + teamworkBonus;
    }

    using var document = JsonDocument.Parse(state.ToJsonString());

    return contributions with { State = document.RootElement.Clone() };
  }

  private static bool PromotionIsAvailable(JsonObject promotion, DateTimeOffset localNow)
  {
    return (promotion["active"]?.GetValue<bool>() ?? false)
      && TimeOnly.TryParseExact(
        promotion["deadline"]?.GetValue<string>(),
        "HH:mm",
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out var deadline
      )
      && TimeOnly.FromDateTime(localNow.DateTime) < deadline;
  }

  private static FamilyAggregateSnapshot ProjectSavingsInterest(
    FamilyAggregateSnapshot savings,
    FamilyAggregateSnapshot progression
  )
  {
    var state = JsonNode.Parse(savings.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The savings state must be an object.");
    var progressionState = JsonNode.Parse(progression.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The progression state must be an object.");
    var evaluations = progressionState["memberProgressions"] as JsonObject
      ?? throw new InvalidOperationException("The member progression read model is required.");
    var goals = state["goals"] as JsonArray
      ?? throw new InvalidOperationException("The savings goals collection is required.");

    foreach (var goal in goals.OfType<JsonObject>())
    {
      var ownerId = goal["ownerId"]?.GetValue<string>();
      var rate = ownerId is not null && evaluations[ownerId] is JsonObject evaluation
        ? evaluation["savingsInterestRate"]?.GetValue<decimal>() ?? 0
        : 0;
      goal["projectedInterestCredit"] = SavingsInterestPolicy.CalculateCredit(
        new LadirchenAmount(goal["saved"]?.GetValue<decimal>() ?? 0),
        new LadirchenAmount(goal["target"]?.GetValue<decimal>() ?? 0),
        new Percentage(rate)
      );
    }

    using var document = JsonDocument.Parse(state.ToJsonString());

    return savings with { State = document.RootElement.Clone() };
  }

  private FamilyAggregateSnapshot ProjectRewardShop(
    FamilyAggregateSnapshot rewardShop,
    FamilyAggregateSnapshot profile
  )
  {
    var state = JsonNode.Parse(rewardShop.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The reward shop state must be an object.");
    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(profile.State.GetProperty("timeZone").GetString()!);
    var localNow = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), timeZone);
    var localDate = DateOnly.FromDateTime(localNow.DateTime);
    var rewards = state["rewards"] as JsonArray
      ?? throw new InvalidOperationException("The reward collection is required.");

    state["redemptionOpen"] = localNow.Hour < 18;

    foreach (var reward in rewards.OfType<JsonObject>())
    {
      var visible = reward["isVisible"]?.GetValue<bool>() ?? true;
      var availableFrom = ParseOptionalCalendarDate(reward, "availableFrom");
      var availableUntil = ParseOptionalCalendarDate(reward, "availableUntil");
      reward["published"] = visible
        && (availableFrom is null || localDate >= availableFrom)
        && (availableUntil is null || localDate <= availableUntil);
    }

    using var document = JsonDocument.Parse(state.ToJsonString());

    return rewardShop with { State = document.RootElement.Clone() };
  }

  private static DateOnly? ParseOptionalCalendarDate(JsonObject source, string property)
  {
    var value = source[property]?.GetValue<string>();

    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    return DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
  }

  private static FamilyAggregateSnapshot ProjectHomeCatalog(
    FamilyAggregateSnapshot home,
    FamilyAggregateSnapshot progression
  )
  {
    var state = JsonNode.Parse(home.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The home customization state must be an object.");
    var houseLevel = progression.State.GetProperty("houseLevel").GetInt32();
    var accessories = state["accessories"] as JsonArray
      ?? throw new InvalidOperationException("The house accessory collection is required.");
    var editions = state["editions"] as JsonArray
      ?? throw new InvalidOperationException("The house edition collection is required.");
    var roomDesigns = state["roomDesigns"] as JsonArray
      ?? throw new InvalidOperationException("The room design collection is required.");
    var ownedAccessoryIds = accessories.OfType<JsonObject>()
      .Where(item => item["owned"]?.GetValue<bool>() ?? false)
      .Select(item => item["id"]!.GetValue<string>())
      .ToHashSet(StringComparer.Ordinal);

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
        ["themeId"] = item.Value.ThemeId,
      }).ToArray<JsonNode?>()),
      ["themes"] = new JsonArray(HouseShopCatalog.Themes.Select(item => new JsonObject
      {
        ["available"] = true,
        ["id"] = item.Key,
        ["owned"] = editions.OfType<JsonObject>().Any(edition =>
          edition["id"]?.GetValue<string>() == item.Key && (edition["owned"]?.GetValue<bool>() ?? false)),
        ["price"] = item.Value,
      }).ToArray<JsonNode?>()),
    };

    using var document = JsonDocument.Parse(state.ToJsonString());

    return home with { State = document.RootElement.Clone() };
  }
}
