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
  private FamilyAggregateSnapshot ProjectContributionRewards(
    FamilyAggregateSnapshot contributions,
    FamilyAggregateSnapshot profile
  )
  {
    var state = JsonNode.Parse(contributions.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The contributions state must be an object.");
    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(profile.State.GetProperty("timeZone").GetString()!);
    var familyZone = new IanaTimeZone(profile.State.GetProperty("timeZone").GetString()!);
    var localNow = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), timeZone);
    var items = state["contributions"] as JsonArray
      ?? throw new InvalidOperationException("The contributions collection is required.");
    var promotions = state["promotions"] as JsonArray
      ?? throw new InvalidOperationException("The promotions collection is required.");
    state["perfectRatingBonusPercent"] ??= ContributionRewardPolicy.DefaultPerfectRatingBonus.Value;

    foreach (var promotion in promotions.OfType<JsonObject>())
    {
      promotion["available"] = PromotionIsAvailable(promotion, _timeProvider.GetUtcNow(), familyZone);
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

    var todayEarnedByMember = new JsonObject();

    foreach (var contribution in items
      .OfType<JsonObject>()
      .Where(item => item["status"]?.GetValue<string>() == "approved")
      .Where(item => ApprovedOnLocalDate(item, localNow, timeZone)))
    {
      var memberId = contribution["assigneeId"]?.GetValue<string>();

      if (memberId is null)
      {
        continue;
      }

      var earned = contribution["earnedReward"]?.GetValue<decimal>()
        ?? contribution["reward"]?.GetValue<decimal>()
        ?? 0;
      var bonus = contribution["earnedRatingBonus"]?.GetValue<decimal>() ?? 0;
      todayEarnedByMember[memberId] = (todayEarnedByMember[memberId]?.GetValue<decimal>() ?? 0) + earned + bonus;
    }

    state["todayEarnedByMember"] = todayEarnedByMember;

    using var document = JsonDocument.Parse(state.ToJsonString());

    return contributions with { State = document.RootElement.Clone() };
  }

  private static bool ApprovedOnLocalDate(JsonObject contribution, DateTimeOffset localNow, TimeZoneInfo timeZone)
  {
    return DateTimeOffset.TryParse(
      contribution["approvedAt"]?.GetValue<string>(),
      CultureInfo.InvariantCulture,
      DateTimeStyles.AssumeUniversal,
      out var approvedAt
    ) && TimeZoneInfo.ConvertTime(approvedAt, timeZone).Date == localNow.Date;
  }

  private static bool PromotionIsAvailable(JsonObject promotion, DateTimeOffset now, IanaTimeZone zone)
  {
    return (promotion["active"]?.GetValue<bool>() ?? false)
      && PromotionDeadline.TryParse(promotion["deadline"]?.GetValue<string>(), out var deadline)
      && deadline!.IsOpen(now, zone);
  }

}
