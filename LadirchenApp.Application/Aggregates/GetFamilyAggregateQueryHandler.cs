using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
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
}
