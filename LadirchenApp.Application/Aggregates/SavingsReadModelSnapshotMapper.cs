using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using LadirchenApp.Application.Commands;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal static class SavingsReadModelSnapshotMapper
{
  public static SavingsReadModelState Read(
    FamilyAggregateSnapshot savings,
    FamilyAggregateSnapshot progression,
    FamilyAggregateSnapshot profile,
    FamilyAggregateSnapshot? rewardShop
  )
  {
    ArgumentNullException.ThrowIfNull(savings);
    ArgumentNullException.ThrowIfNull(progression);
    ArgumentNullException.ThrowIfNull(profile);
    var family = RehydrateFamily(profile);
    var economy = RehydrateEconomy(family, savings, rewardShop);
    var progressionState = ReadObject(progression, "The progression state must be an object.");
    var savingsState = ReadObject(savings, "The savings state must be an object.");
    var maximumRate = FamilyProgressionSnapshotMapper.ReadMaximumSavingsInterestRate(progressionState);
    var stages = ReadLadiStages(progressionState);
    var storedBalanceMemberIds = RequiredObject(savingsState, "balances")
      .Select(item => new FamilyMemberId(item.Key))
      .ToHashSet();

    return new SavingsReadModelState(family, economy, maximumRate, stages, storedBalanceMemberIds);
  }

  public static Family RehydrateFamily(FamilyAggregateSnapshot profile)
  {
    ArgumentNullException.ThrowIfNull(profile);
    var state = ReadObject(profile, "The family profile state must be an object.");

    return FamilyProfileSnapshotMapper.Rehydrate(profile.FamilyId, state);
  }

  public static FamilyEconomy RehydrateEconomy(
    Family family,
    FamilyAggregateSnapshot savings,
    FamilyAggregateSnapshot? rewardShop
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(savings);
    var savingsState = ReadObject(savings, "The savings state must be an object.");
    var rewardShopState = rewardShop is null
      ? new JsonObject { ["rewards"] = new JsonArray() }
      : ReadObject(rewardShop, "The reward shop state must be an object.");

    return FamilyEconomySnapshotMapper.RehydrateSavings(family, savingsState, rewardShopState);
  }

  public static Percentage ReadMaximumInterestRate(FamilyAggregateSnapshot progression)
  {
    ArgumentNullException.ThrowIfNull(progression);
    var state = ReadObject(progression, "The progression state must be an object.");

    return FamilyProgressionSnapshotMapper.ReadMaximumSavingsInterestRate(state);
  }

  public static FamilyAggregateSnapshot Write(
    FamilyAggregateSnapshot savings,
    SavingsReadModelProjection projection
  )
  {
    ArgumentNullException.ThrowIfNull(savings);
    ArgumentNullException.ThrowIfNull(projection);
    var state = ReadObject(savings, "The savings state must be an object.");
    state["balances"] = DecimalMap(projection.Balances);
    state["availableBalances"] = DecimalMap(projection.AvailableBalances);
    state["sharedSavedBalances"] = DecimalMap(projection.SharedSavedBalances);
    WriteGoals(state, projection.Goals);
    WriteVouchers(state, projection.Vouchers);
    WritePendingGuardianGifts(state, projection.VisibleBalanceMemberIds);
    state.Remove("appliedOperationIds");
    state.Remove("ledger");

    using var document = JsonDocument.Parse(state.ToJsonString());

    return savings with { State = document.RootElement.Clone() };
  }

  private static void WriteGoals(
    JsonObject state,
    IReadOnlyDictionary<SavingGoalId, SavingsGoalReadProjection> projections
  )
  {
    var goals = RequiredArray(state, "goals");

    foreach (var hiddenGoal in goals
      .OfType<JsonObject>()
      .Where(goal => !projections.ContainsKey(new SavingGoalId(RequiredString(goal, "id"))))
      .ToArray())
    {
      goals.Remove(hiddenGoal);
    }

    foreach (var node in goals)
    {
      var goal = node as JsonObject
        ?? throw new InvalidOperationException("Every savings goal must be an object.");
      var goalId = new SavingGoalId(RequiredString(goal, "id"));
      var projection = projections.TryGetValue(goalId, out var value)
        ? value
        : throw new InvalidOperationException("The savings-goal projection is missing.");
      goal.Remove("refundableContributions");
      goal["projectedInterestCredit"] = projection.ProjectedInterestCredit;
      goal["remainingAmount"] = projection.RemainingAmount;
      goal["withdrawableBalance"] = projection.WithdrawableBalance;
      goal["visibleToMemberIds"] = IdentifierArray(projection.VisibleToMemberIds);
      goal["commandLimits"] = CommandLimitsMap(projection.CommandLimits);

      if (projection.CancellationRequest is null)
      {
        goal.Remove("cancellationRequest");
      }
      else
      {
        goal["cancellationRequest"] = new JsonObject
        {
          ["requestedByMemberId"] = projection.CancellationRequest.RequestedByMemberId.Value,
          ["consentedMemberIds"] = IdentifierArray(projection.CancellationRequest.ConsentedMemberIds),
          ["pendingMemberIds"] = IdentifierArray(projection.CancellationRequest.PendingMemberIds),
        };
      }
    }
  }

  private static void WriteVouchers(
    JsonObject state,
    IReadOnlyDictionary<SavingGoalId, SavingsVoucherReadProjection> projections
  )
  {
    var vouchers = state["vouchers"] as JsonArray ?? new JsonArray();
    state["vouchers"] = vouchers;

    foreach (var hiddenVoucher in vouchers
      .OfType<JsonObject>()
      .Where(voucher => !projections.ContainsKey(new SavingGoalId(RequiredString(voucher, "goalId"))))
      .ToArray())
    {
      vouchers.Remove(hiddenVoucher);
    }

    foreach (var node in vouchers)
    {
      var voucher = node as JsonObject
        ?? throw new InvalidOperationException("Every savings voucher must be an object.");
      var goalId = new SavingGoalId(RequiredString(voucher, "goalId"));
      var projection = projections.TryGetValue(goalId, out var value)
        ? value
        : throw new InvalidOperationException("The savings-voucher projection is missing.");
      voucher["visibleToMemberIds"] = IdentifierArray(projection.VisibleToMemberIds);
      voucher["consentableByMemberIds"] = IdentifierArray(projection.ConsentableByMemberIds);
      voucher["redeemableByMemberIds"] = IdentifierArray(projection.RedeemableByMemberIds);
    }
  }

  private static void WritePendingGuardianGifts(
    JsonObject state,
    IReadOnlySet<FamilyMemberId> visibleMemberIds
  )
  {
    var gifts = state["pendingGuardianGifts"] as JsonArray ?? new JsonArray();
    state["pendingGuardianGifts"] = gifts;

    foreach (var hiddenGift in gifts
      .OfType<JsonObject>()
      .Where(gift => !visibleMemberIds.Contains(new FamilyMemberId(RequiredString(gift, "childId"))))
      .ToArray())
    {
      gifts.Remove(hiddenGift);
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

  private static JsonObject CommandLimitsMap(
    IReadOnlyDictionary<FamilyMemberId, SavingsGoalCommandLimits> limits
  )
  {
    var result = new JsonObject();

    foreach (var item in limits)
    {
      result[item.Key.Value] = new JsonObject
      {
        ["canCancel"] = item.Value.CanCancel,
        ["canRequestCancellation"] = item.Value.CanRequestCancellation,
        ["canApproveCancellation"] = item.Value.CanApproveCancellation,
        ["canRejectCancellation"] = item.Value.CanRejectCancellation,
        ["canWithdraw"] = item.Value.CanWithdraw,
        ["maximumDepositAmount"] = item.Value.MaximumDepositAmount,
        ["maximumSupportAmount"] = item.Value.MaximumSupportAmount,
      };
    }

    return result;
  }

  private static JsonArray IdentifierArray(IEnumerable<FamilyMemberId> identifiers)
  {
    return new JsonArray(
      identifiers.Select(identifier => JsonValue.Create(identifier.Value)).ToArray<JsonNode?>()
    );
  }

  private static IReadOnlyDictionary<FamilyMemberId, LadiStage> ReadLadiStages(JsonObject progression)
  {
    var members = RequiredObject(progression, "memberProgressions");
    var result = new Dictionary<FamilyMemberId, LadiStage>();

    foreach (var item in members)
    {
      var member = item.Value as JsonObject
        ?? throw new InvalidOperationException("Every member progression must be an object.");
      result.Add(
        new FamilyMemberId(item.Key),
        new LadiStage(
          RequiredString(member, "ladiStageId"),
          RequiredInt(member, "ladiStagePosition")
        )
      );
    }

    return result;
  }

  private static JsonObject ReadObject(FamilyAggregateSnapshot snapshot, string message)
  {
    return JsonNode.Parse(snapshot.State.GetRawText()) as JsonObject
      ?? throw new InvalidOperationException(message);
  }

  private static JsonObject RequiredObject(JsonObject value, string property)
  {
    return value[property] as JsonObject
      ?? throw new InvalidOperationException($"The {property} object is required.");
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

  private static int RequiredInt(JsonObject value, string property)
  {
    return value[property]?.GetValue<int>()
      ?? throw new InvalidOperationException($"The {property} value is required.");
  }
}
