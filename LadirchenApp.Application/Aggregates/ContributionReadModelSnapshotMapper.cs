using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal static class ContributionReadModelSnapshotMapper
{
  public static ContributionReadModelState Read(
    FamilyAggregateSnapshot contributions,
    FamilyAggregateSnapshot profile
  )
  {
    ArgumentNullException.ThrowIfNull(contributions);
    ArgumentNullException.ThrowIfNull(profile);
    var contributionState = ReadObject(contributions, "The contributions state must be an object.");
    var profileState = ReadObject(profile, "The family profile state must be an object.");
    var family = FamilyProfileSnapshotMapper.Rehydrate(profile.FamilyId, profileState);
    var economy = FamilyEconomySnapshotMapper.RehydrateContributions(family, contributionState);

    return new ContributionReadModelState(family, economy);
  }

  public static FamilyAggregateSnapshot Write(
    FamilyAggregateSnapshot contributions,
    ContributionReadModelProjection projection
  )
  {
    ArgumentNullException.ThrowIfNull(contributions);
    ArgumentNullException.ThrowIfNull(projection);
    var state = ReadObject(contributions, "The contributions state must be an object.");
    state["perfectRatingBonusPercent"] = projection.PerfectRatingBonusPercent;
    state["todayEarnedByMember"] = DecimalMap(projection.TodayEarnedByMember);
    state["currentWeekEarnedByMember"] = DecimalMap(projection.CurrentWeekEarnedByMember);
    state["currentWeekApprovedContributionCountByMember"] = IntegerMap(
      projection.CurrentWeekApprovedContributionCountByMember
    );
    WriteContributions(state, projection.Contributions);
    WritePromotions(state, projection.PromotionAvailability);

    using var document = JsonDocument.Parse(state.ToJsonString());

    return contributions with
    {
      State = document.RootElement.Clone()
    };
  }

  private static void WriteContributions(
    JsonObject state,
    IReadOnlyDictionary<ContributionId, ContributionReadProjection> projections
  )
  {
    foreach (var node in RequiredArray(state, "contributions"))
    {
      var contribution = node as JsonObject
        ?? throw new InvalidOperationException("Every contribution must be an object.");
      var id = new ContributionId(RequiredString(contribution, "id"));
      var projection = projections.TryGetValue(id, out var value)
        ? value
        : throw new InvalidOperationException("The contribution projection is missing.");
      contribution["projectedReward"] = projection.ProjectedReward;
      contribution["partnerableMemberIds"] = IdentifierArray(projection.PartnerableMemberIds);
    }
  }

  private static void WritePromotions(
    JsonObject state,
    IReadOnlyDictionary<PromotionId, bool> availability
  )
  {
    foreach (var node in RequiredArray(state, "promotions"))
    {
      var promotion = node as JsonObject
        ?? throw new InvalidOperationException("Every promotion must be an object.");
      var id = new PromotionId(RequiredString(promotion, "id"));
      promotion["available"] = availability.TryGetValue(id, out var available)
        ? available
        : throw new InvalidOperationException("The promotion projection is missing.");
    }
  }

  private static JsonObject DecimalMap(IReadOnlyDictionary<FamilyMemberId, decimal> values)
  {
    var result = new JsonObject();

    foreach (var item in values)
    {
      result[item.Key.Value] = item.Value;
    }

    return result;
  }

  private static JsonObject IntegerMap(IReadOnlyDictionary<FamilyMemberId, int> values)
  {
    var result = new JsonObject();

    foreach (var item in values)
    {
      result[item.Key.Value] = item.Value;
    }

    return result;
  }

  private static JsonArray IdentifierArray(IEnumerable<FamilyMemberId> identifiers)
  {
    return new JsonArray(
      identifiers.Select(identifier => JsonValue.Create(identifier.Value)).ToArray<JsonNode?>()
    );
  }

  private static JsonObject ReadObject(FamilyAggregateSnapshot snapshot, string message)
  {
    return JsonNode.Parse(snapshot.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException(message);
  }

  private static JsonArray RequiredArray(JsonObject value, string property)
  {
    return value[property] as JsonArray
      ?? throw new InvalidOperationException($"The {property} collection is required.");
  }

  private static string RequiredString(JsonObject value, string property)
  {
    var result = value[property]?.GetValue<string>();

    if (string.IsNullOrWhiteSpace(result))
    {
      throw new InvalidOperationException($"The {property} value is required.");
    }

    return result;
  }
}
