using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private FamilyAggregateSnapshot ProjectRewardShop(
    FamilyAggregateSnapshot rewardShop,
    FamilyAggregateSnapshot profile,
    FamilyAggregateSnapshot savings,
    FamilyMemberId authenticatedMemberId,
    FamilyMemberId viewerId
  )
  {
    var state = ParseRewardShopState(rewardShop.State, "reward shop");
    var profileState = ParseRewardShopState(profile.State, "family profile");
    var savingsState = ParseRewardShopState(savings.State, "savings");
    var family = FamilyProfileSnapshotMapper.Rehydrate(rewardShop.FamilyId, profileState);
    var economy = FamilyEconomySnapshotMapper.RehydrateRewardShop(
      family,
      state,
      savingsState
    );
    var presentations = RequiredRewardPresentations(state);
    var now = _timeProvider.GetUtcNow();
    var permissionPolicy = new FamilyPermissionPolicy();
    var authenticatedPermissions = permissionPolicy.Resolve(family.ActiveMember(authenticatedMemberId));
    var viewerPermissions = permissionPolicy.Resolve(family.ActiveMember(viewerId));
    var mayManageContent = authenticatedPermissions.ManageContent && viewerPermissions.ManageContent;
    state["redemptionOpen"] = ShopAvailabilityPolicy.RedemptionIsOpen(now, family.TimeZone);

    foreach (var reward in economy.ShopRewards)
    {
      if (!presentations.TryGetValue(reward.Id, out var presentation))
      {
        throw new InvalidOperationException("The reward presentation is missing.");
      }

      var availability = economy.GetShopRewardAvailability(reward.Id, now);

      if (!mayManageContent && availability.PublicationStatus != ShopRewardPublicationStatus.Visible)
      {
        (state["rewards"] as JsonArray)?.Remove(presentation);

        continue;
      }

      WriteRewardAvailability(presentation, availability, family.TimeZone, viewerId);
    }

    using var document = JsonDocument.Parse(state.ToJsonString());

    return rewardShop with
    {
      State = document.RootElement.Clone()
    };
  }

  private static void WriteRewardAvailability(
    JsonObject target,
    ShopRewardAvailability availability,
    IanaTimeZone timeZone,
    FamilyMemberId viewerId
  )
  {
    target["published"] = availability.Published;
    target["publicationStatus"] = FormatPublicationStatus(availability.PublicationStatus);
    var nextStatusChangeAt = FormatNextPublicationChange(
      availability.NextPublicationChangeDate,
      timeZone
    );

    if (nextStatusChangeAt is null)
    {
      target.Remove("nextStatusChangeAt");
    }
    else
    {
      target["nextStatusChangeAt"] = nextStatusChangeAt;
    }

    target["requestableByMemberIds"] = ViewerCapability(availability.RequestableByMemberIds, viewerId);
    target["cancellableByMemberIds"] = ViewerCapability(availability.CancellableByMemberIds, viewerId);
    target["approvableByMemberIds"] = ViewerCapability(availability.ApprovableByMemberIds, viewerId);
  }

  private static JsonArray ViewerCapability(
    IReadOnlyCollection<FamilyMemberId> capableMemberIds,
    FamilyMemberId viewerId
  )
  {
    return capableMemberIds.Contains(viewerId) ? MemberIdArray([viewerId]) : new JsonArray();
  }

  private static IReadOnlyDictionary<ShopRewardId, JsonObject> RequiredRewardPresentations(
    JsonObject state
  )
  {
    var rewards = state["rewards"] as JsonArray
      ?? throw new InvalidOperationException("The reward collection is required.");

    return rewards
      .OfType<JsonObject>()
      .ToDictionary(
        item => new ShopRewardId(
          item["id"]?.GetValue<string>()
            ?? throw new InvalidOperationException("A reward identifier is required.")
        )
      );
  }

  private static JsonObject ParseRewardShopState(JsonElement state, string aggregateName)
  {
    return JsonNode.Parse(state.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException($"The {aggregateName} state must be an object.");
  }

  private static JsonArray MemberIdArray(IEnumerable<FamilyMemberId> memberIds)
  {
    return new JsonArray(
      memberIds.Select(memberId => JsonValue.Create(memberId.Value)).ToArray<JsonNode?>()
    );
  }

  private static string FormatPublicationStatus(ShopRewardPublicationStatus status)
  {
    return status switch
    {
      ShopRewardPublicationStatus.Hidden => "hidden",
      ShopRewardPublicationStatus.Scheduled => "scheduled",
      ShopRewardPublicationStatus.Expired => "expired",
      ShopRewardPublicationStatus.Visible => "visible",
      _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
  }

  private static string? FormatNextPublicationChange(
    DateOnly? transitionDate,
    IanaTimeZone timeZone
  )
  {
    if (transitionDate is null)
    {
      return null;
    }

    var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZone.Value);
    var localTransition = DateTime.SpecifyKind(
      transitionDate.Value.ToDateTime(TimeOnly.MinValue),
      DateTimeKind.Unspecified
    );
    var utcTransition = TimeZoneInfo.ConvertTimeToUtc(localTransition, zone);

    return new DateTimeOffset(utcTransition, TimeSpan.Zero).ToString("O", CultureInfo.InvariantCulture);
  }
}
