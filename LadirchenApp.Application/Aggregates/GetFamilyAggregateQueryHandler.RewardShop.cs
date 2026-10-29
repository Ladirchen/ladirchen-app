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
  private FamilyAggregateSnapshot ProjectRewardShop(
    FamilyAggregateSnapshot rewardShop,
    FamilyAggregateSnapshot profile,
    FamilyAggregateSnapshot savings
  )
  {
    var state = JsonNode.Parse(rewardShop.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException("The reward shop state must be an object.");
    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(profile.State.GetProperty("timeZone").GetString()!);
    var localNow = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), timeZone);
    var localDate = DateOnly.FromDateTime(localNow.DateTime);
    var rewards = state["rewards"] as JsonArray
      ?? throw new InvalidOperationException("The reward collection is required.");
    var members = JsonNode.Parse(profile.State.GetRawText())?["members"] as JsonArray
      ?? throw new InvalidOperationException("The family member collection is required.");
    var balances = JsonNode.Parse(savings.State.GetRawText())?["balances"] as JsonObject
      ?? throw new InvalidOperationException("The savings balance collection is required.");

    var redemptionOpen = ShopAvailabilityPolicy.RedemptionIsOpen(
      _timeProvider.GetUtcNow(),
      new IanaTimeZone(profile.State.GetProperty("timeZone").GetString()!)
    );
    state["redemptionOpen"] = redemptionOpen;

    foreach (var reward in rewards.OfType<JsonObject>())
    {
      var visible = reward["isVisible"]?.GetValue<bool>() ?? true;
      var availableFrom = ParseOptionalCalendarDate(reward, "availableFrom");
      var availableUntil = ParseOptionalCalendarDate(reward, "availableUntil");
      var publicationStatus = !visible
        ? "hidden"
        : availableFrom is not null && localDate < availableFrom
          ? "scheduled"
          : availableUntil is not null && localDate > availableUntil
            ? "expired"
            : "visible";
      var published = publicationStatus == "visible";
      var requestableByMemberIds = members
        .OfType<JsonObject>()
        .Where(member => member["role"]?.GetValue<string>() == "child")
        .Where(member =>
        {
          var memberId = member["id"]!.GetValue<string>();
          var balance = balances[memberId]?.GetValue<decimal>() ?? 0;
          var available = AvailableBalance(balance, rewards, memberId);

          return redemptionOpen
            && published
            && reward["status"]?.GetValue<string>() == "available"
            && (reward["quantity"]?.GetValue<int>() ?? 0) > 0
            && available >= (reward["price"]?.GetValue<decimal>() ?? 0);
        })
        .Select(member => JsonValue.Create(member["id"]!.GetValue<string>()))
        .ToArray<JsonNode?>();
      var requesterId = reward["requesterId"]?.GetValue<string>();
      var requested = reward["status"]?.GetValue<string>() == "requested";
      reward["published"] = published;
      reward["publicationStatus"] = publicationStatus;
      var nextStatusChangeAt = NextPublicationChangeAt(availableFrom, availableUntil, localDate, timeZone);

      if (nextStatusChangeAt is null)
      {
        reward.Remove("nextStatusChangeAt");
      }
      else
      {
        reward["nextStatusChangeAt"] = nextStatusChangeAt;
      }

      reward["requestableByMemberIds"] = new JsonArray(requestableByMemberIds);
      reward["cancellableByMemberIds"] = requested && requesterId is not null
        ? new JsonArray(JsonValue.Create(requesterId))
        : new JsonArray();
      reward["approvableByMemberIds"] = new JsonArray(members
        .OfType<JsonObject>()
        .Where(member => requested && redemptionOpen && FamilyMemberPermissionResolver.Resolve(member).ManageContent)
        .Select(member => JsonValue.Create(member["id"]!.GetValue<string>()))
        .ToArray<JsonNode?>());
    }

    using var document = JsonDocument.Parse(state.ToJsonString());

    return rewardShop with { State = document.RootElement.Clone() };
  }

  private static string? NextPublicationChangeAt(
    DateOnly? availableFrom,
    DateOnly? availableUntil,
    DateOnly localDate,
    TimeZoneInfo timeZone
  )
  {
    DateOnly? transitionDate = availableFrom is not null && localDate < availableFrom
      ? availableFrom
      : availableUntil is not null && localDate <= availableUntil
        ? availableUntil.Value.AddDays(1)
        : null;

    if (transitionDate is null)
    {
      return null;
    }

    var localTransition = DateTime.SpecifyKind(transitionDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
    var utcTransition = TimeZoneInfo.ConvertTimeToUtc(localTransition, timeZone);

    return new DateTimeOffset(utcTransition, TimeSpan.Zero).ToString("O", CultureInfo.InvariantCulture);
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

}
