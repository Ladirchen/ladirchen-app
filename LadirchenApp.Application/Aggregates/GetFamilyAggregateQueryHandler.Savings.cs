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
  private static FamilyAggregateSnapshot ProjectSavingsReadModel(
    FamilyAggregateSnapshot savings,
    FamilyAggregateSnapshot progression,
    FamilyAggregateSnapshot? profile,
    FamilyAggregateSnapshot? rewardShop
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
    var balances = state["balances"] as JsonObject
      ?? throw new InvalidOperationException("The savings balance collection is required.");
    var vouchers = state["vouchers"] as JsonArray ?? new JsonArray();
    state["vouchers"] = vouchers;
    JsonObject[] rewards = rewardShop is null
      ? []
      : (JsonNode.Parse(rewardShop.State.GetRawText())?["rewards"] as JsonArray)?.OfType<JsonObject>().ToArray() ?? [];
    JsonObject[] members = profile is null
      ? []
      : (JsonNode.Parse(profile.State.GetRawText())?["members"] as JsonArray)?.OfType<JsonObject>().ToArray() ?? [];
    var availableBalances = new JsonObject();
    var sharedSavedBalances = new JsonObject();

    foreach (var balance in balances)
    {
      var memberId = balance.Key;
      var amount = balance.Value?.GetValue<decimal>() ?? 0;
      var reserved = rewards
        .Where(reward => reward["status"]?.GetValue<string>() == "requested")
        .Where(reward => reward["requesterId"]?.GetValue<string>() == memberId)
        .Sum(reward => reward["price"]?.GetValue<decimal>() ?? 0);
      availableBalances[memberId] = Math.Max(0, amount - reserved);
    }

    state["availableBalances"] = availableBalances;

    foreach (var goal in goals.OfType<JsonObject>())
    {
      var ownerId = goal["ownerId"]?.GetValue<string>();
      var saved = goal["saved"]?.GetValue<decimal>() ?? 0;
      var target = goal["target"]?.GetValue<decimal>() ?? 0;
      var starterBonus = goal["starterBonus"]?.GetValue<decimal>() ?? 0;
      var rate = ownerId is not null && evaluations[ownerId] is JsonObject evaluation
        ? evaluation["savingsInterestRate"]?.GetValue<decimal>() ?? 0
        : 0;
      goal["projectedInterestCredit"] = SavingsInterestPolicy.CalculateCredit(
        new LadirchenAmount(saved),
        new LadirchenAmount(target),
        new Percentage(rate)
      );
      goal["remainingAmount"] = Math.Max(0, target - saved);
      goal["withdrawableBalance"] = goal["voucherId"] is null
        ? saved >= target
          ? saved
          : Math.Max(0, saved - starterBonus)
        : 0;
      goal["visibleToMemberIds"] = new JsonArray(members
        .Where(member => GoalIsVisibleTo(goal, member))
        .Select(member => JsonValue.Create(member["id"]!.GetValue<string>()))
        .ToArray<JsonNode?>());
      goal["commandLimits"] = ProjectGoalCommandLimits(goal, members, availableBalances);

      if (
        ownerId is not null
        && members.Any(member => member["id"]?.GetValue<string>() == ownerId)
        && goal["visibility"]?.GetValue<string>() != "private"
      )
      {
        sharedSavedBalances[ownerId] = (sharedSavedBalances[ownerId]?.GetValue<decimal>() ?? 0) + saved;
      }
    }

    foreach (var voucher in vouchers.OfType<JsonObject>())
    {
      ProjectVoucherCapabilities(voucher);
    }

    state["sharedSavedBalances"] = sharedSavedBalances;

    using var document = JsonDocument.Parse(state.ToJsonString());

    return savings with { State = document.RootElement.Clone() };
  }

  private static JsonObject ProjectGoalCommandLimits(
    JsonObject goal,
    IEnumerable<JsonObject> members,
    JsonObject availableBalances
  )
  {
    var result = new JsonObject();
    var ownerId = goal["ownerId"]?.GetValue<string>();
    var saved = goal["saved"]?.GetValue<decimal>() ?? 0;
    var target = goal["target"]?.GetValue<decimal>() ?? 0;
    var starterBonus = goal["starterBonus"]?.GetValue<decimal>() ?? 0;
    var remaining = Math.Max(0, target - saved);
    var withdrawable = saved >= target ? saved : Math.Max(0, saved - starterBonus);
    var hasVoucher = goal["voucherId"] is not null;

    foreach (var member in members)
    {
      var memberId = member["id"]!.GetValue<string>();
      var role = member["role"]?.GetValue<string>();
      var available = availableBalances[memberId]?.GetValue<decimal>() ?? 0;
      var ownsGoal = ownerId == memberId;
      var supportsAsGuardian = role == "guardian" && GoalIsVisibleTo(goal, member);
      var supportsAsChild = role == "child"
        && !ownsGoal
        && goal["visibility"]?.GetValue<string>() == "family";
      var permissions = FamilyMemberPermissionResolver.Resolve(member);
      var canCancelFamilyGoal = ownerId == "family" && (role == "child" || permissions.ManageFamily);
      result[memberId] = new JsonObject
      {
        ["canCancel"] = !hasVoucher && (ownsGoal || canCancelFamilyGoal),
        ["canWithdraw"] = !hasVoucher && ownsGoal && withdrawable > 0,
        ["maximumDepositAmount"] = !hasVoucher && ownsGoal ? Math.Min(available, remaining) : 0,
        ["maximumSupportAmount"] = !hasVoucher && supportsAsGuardian
          ? remaining
          : !hasVoucher && supportsAsChild
            ? Math.Min(available, remaining)
            : 0,
      };
    }

    return result;
  }

  private static void ProjectVoucherCapabilities(JsonObject voucher)
  {
    var ownerId = voucher["ownerId"]?.GetValue<string>();
    var participants = (voucher["participantIds"] as JsonArray)?
      .Select(item => item?.GetValue<string>())
      .Where(item => item is not null)
      .Select(item => item!)
      .ToArray() ?? [];
    var consents = (voucher["consentedMemberIds"] as JsonArray)?
      .Select(item => item?.GetValue<string>())
      .Where(item => item is not null)
      .Select(item => item!)
      .ToHashSet(StringComparer.Ordinal) ?? [];
    var status = voucher["status"]?.GetValue<string>();
    voucher["visibleToMemberIds"] = new JsonArray(
      participants
        .Append(ownerId)
        .Where(item => item is not null)
        .Distinct(StringComparer.Ordinal)
        .Select(item => JsonValue.Create(item))
        .ToArray<JsonNode?>()
    );
    voucher["consentableByMemberIds"] = new JsonArray(
      (status == "pending" ? participants.Where(item => !consents.Contains(item)) : [])
        .Select(item => JsonValue.Create(item))
        .ToArray<JsonNode?>()
    );
    voucher["redeemableByMemberIds"] = new JsonArray(
      (status == "redeemable" ? participants.Append(ownerId).Where(item => item is not null).Distinct() : [])
        .Select(item => JsonValue.Create(item))
        .ToArray<JsonNode?>()
    );
  }

  private static bool GoalIsVisibleTo(JsonObject goal, JsonObject member)
  {
    var memberId = member["id"]?.GetValue<string>();
    var ownerId = goal["ownerId"]?.GetValue<string>();
    var visibility = goal["visibility"]?.GetValue<string>();
    var permissions = FamilyMemberPermissionResolver.Resolve(member);

    if (ownerId == memberId)
    {
      return true;
    }

    if (visibility == "family")
    {
      return permissions.ViewFamilyGoals;
    }

    return visibility == "guardians" && permissions.ViewGuardianGoals;
  }

}
