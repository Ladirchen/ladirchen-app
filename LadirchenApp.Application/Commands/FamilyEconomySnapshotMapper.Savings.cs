using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal static partial class FamilyEconomySnapshotMapper
{
  public static FamilyEconomy RehydrateSavings(
    Family family,
    JsonObject savingsState,
    JsonObject rewardShopState
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(savingsState);
    ArgumentNullException.ThrowIfNull(rewardShopState);
    family.RestoreEconomySettings(
      ParseSavingsCurrency(RequiredString(savingsState, "familyCurrencyCode")),
      ParseStoredExchangeRate(RequiredDecimal(savingsState, "ladirchenPerCurrencyUnit"))
    );
    var economy = RehydrateRewardShop(family, rewardShopState, savingsState);

    foreach (var item in RequiredArray(savingsState, "goals").OfType<JsonObject>())
    {
      var ownerValue = RequiredString(item, "ownerId");
      var owner = ownerValue == "family"
        ? GoalOwner.Family
        : GoalOwner.Member(new FamilyMemberId(ownerValue));
      var participants = RequiredArray(item, "participantIds")
        .Select(RequiredString)
        .Select(value => new FamilyMemberId(value))
        .ToArray();
      var saved = RequiredDecimal(item, "saved");
      var starterBonus = RequiredDecimal(item, "starterBonus");
      var interestEarned = OptionalDecimal(item, "interestEarned") ?? 0;
      var refundableContributions = ParseRefundableContributions(
        item,
        saved,
        starterBonus,
        interestEarned
      );
      var cancellationRequest = item["cancellationRequest"] as JsonObject;
      var cancellationRequestedByMemberId = cancellationRequest is null
        ? null
        : new FamilyMemberId(RequiredString(cancellationRequest, "requestedByMemberId"));
      var cancellationConsentedMemberIds = cancellationRequest is null
        ? Array.Empty<FamilyMemberId>()
        : RequiredArray(cancellationRequest, "consentedMemberIds")
          .Select(RequiredString)
          .Select(value => new FamilyMemberId(value))
          .ToArray();

      var lastPeriod = ParseOptionalPeriod(item, "lastInterestPeriod");
      economy.RestoreSavingGoal(
        new SavingGoalId(RequiredString(item, "id")),
        new SavingGoalDefinition(
          owner,
          RequiredString(item, "title"),
          new LadirchenAmount(RequiredDecimal(item, "target")),
          ParseGoalVisibility(RequiredString(item, "visibility")),
          participants
        ),
        OptionalInstant(item, "createdAt") ?? DateTimeOffset.UnixEpoch,
        saved,
        starterBonus,
        interestEarned,
        refundableContributions,
        cancellationRequestedByMemberId,
        cancellationConsentedMemberIds,
        OptionalBool(item, "cheered", false),
        false,
        lastPeriod,
        OptionalDecimal(item, "lastInterestRate") is { } rate ? new Percentage(rate) : null
      );
    }

    foreach (var item in OptionalArray(savingsState, "vouchers").OfType<JsonObject>())
    {
      var owner = OptionalString(item, "ownerId");
      var participants = RequiredArray(item, "participantIds")
        .Select(RequiredString)
        .Select(value => new FamilyMemberId(value))
        .ToArray();
      var consents = RequiredArray(item, "consentedMemberIds")
        .Select(RequiredString)
        .Select(value => new FamilyMemberId(value))
        .ToArray();
      var status = RequiredString(item, "status");

      if (status is not ("pending" or "redeemable" or "redeemed"))
      {
        throw new FamilyCommandException("The stored savings-voucher status is invalid.");
      }

      economy.RestoreSavingsVoucher(
        new SavingGoalId(RequiredString(item, "goalId")),
        RequiredDecimal(item, "amount"),
        owner is null ? null : new FamilyMemberId(owner),
        participants,
        consents,
        status == "redeemed"
          ? OptionalInstant(item, "redeemedAt")
            ?? throw new FamilyCommandException("A redeemed savings voucher requires its redemption time.")
          : null
      );
    }

    foreach (var item in OptionalArray(savingsState, "pendingGuardianGifts").OfType<JsonObject>())
    {
      var sender = OptionalString(item, "senderId");
      var goal = OptionalString(item, "goalId");
      economy.RestorePendingGuardianGift(
        new GuardianGiftId(RequiredString(item, "id")),
        new FamilyMemberId(RequiredString(item, "childId")),
        sender is null ? null : new FamilyMemberId(sender),
        RequiredDecimal(item, "amount"),
        goal is null ? null : new SavingGoalId(goal),
        ParseGiftDestination(RequiredString(item, "destination"))
      );
    }

    foreach (var node in OptionalArray(savingsState, "settledInterestPeriods"))
    {
      var value = RequiredString(node);

      if (!IsoWeekPeriod.TryParse(value, out var period))
      {
        throw new FamilyCommandException("A stored settled-interest period is invalid.");
      }

      economy.RestoreSettledInterestPeriod(period!);
    }

    return economy;
  }

  public static JsonObject WriteSavings(
    JsonObject original,
    Family family,
    FamilyEconomy economy,
    SavingGoalPresentation? addedGoal = null,
    GuardianGiftPresentation? addedGift = null
  )
  {
    ArgumentNullException.ThrowIfNull(original);
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(economy);
    var result = original.DeepClone().AsObject();
    result["familyCurrencyCode"] = family.Currency.ToString();
    result["ladirchenPerCurrencyUnit"] = family.ExchangeRate.LadirchenPerCurrencyUnit;
    var balances = RequiredObject(result, "balances");

    foreach (var wallet in economy.Wallets)
    {
      balances[wallet.Id.Value] = wallet.Balance;
    }

    WriteSavingsGoals(result, economy, addedGoal);
    WriteSavingsVouchers(result, economy);
    WriteGuardianGifts(result, economy, addedGift);
    WriteSettledPeriods(result, economy);
    WriteEconomyHistory(result, economy);

    return result;
  }

  private static void WriteSavingsGoals(
    JsonObject state,
    FamilyEconomy economy,
    SavingGoalPresentation? addedGoal
  )
  {
    var stored = RequiredArray(state, "goals");
    var byId = stored
      .OfType<JsonObject>()
      .ToDictionary(item => RequiredString(item, "id"), StringComparer.Ordinal);
    var active = economy.SavingsGoals.Where(item => !item.Cancelled).ToArray();
    var activeIds = active.Select(item => item.Id.Value).ToHashSet(StringComparer.Ordinal);

    foreach (var removed in byId.Where(item => !activeIds.Contains(item.Key)).Select(item => item.Value).ToArray())
    {
      stored.Remove(removed);
    }

    var voucherIds = economy.SavingsVouchers.Select(item => item.Id).ToHashSet();

    foreach (var goal in active)
    {
      if (!byId.TryGetValue(goal.Id.Value, out var item))
      {
        if (addedGoal is null || addedGoal.Id != goal.Id)
        {
          throw new FamilyCommandException("Presentation data for the new savings goal is missing.");
        }

        item = new JsonObject
        {
          ["id"] = goal.Id.Value,
          ["icon"] = addedGoal.Icon,
        };
        stored.Insert(0, item);
      }

      if (addedGoal is not null && addedGoal.Id == goal.Id)
      {
        item["icon"] = addedGoal.Icon;
      }

      item["title"] = goal.Title;
      item["ownerId"] = goal.Owner.IsFamily ? "family" : goal.Owner.MemberId!.Value;
      item["target"] = goal.Target;
      item["saved"] = goal.Saved;
      item["starterBonus"] = goal.StarterBonus;
      item["interestEarned"] = goal.InterestEarned;
      item["shared"] = goal.Owner.IsFamily;
      item["visibility"] = FormatGoalVisibility(goal.Visibility);
      item["cheered"] = goal.Cheered;
      item["createdAt"] = goal.CreatedAt.ToString("O", CultureInfo.InvariantCulture);
      item["participantIds"] = IdentifierArray(goal.Participants);
      item["refundableContributions"] = new JsonArray(
        goal.RefundableContributions
          .OrderBy(contribution => contribution.Key.Value, StringComparer.Ordinal)
          .Select(contribution => new JsonObject
          {
            ["memberId"] = contribution.Key.Value,
            ["amount"] = contribution.Value,
          })
          .ToArray<JsonNode?>()
      );

      if (goal.CancellationRequest is null)
      {
        item.Remove("cancellationRequest");
      }
      else
      {
        item["cancellationRequest"] = new JsonObject
        {
          ["requestedByMemberId"] = goal.CancellationRequest.RequestedByMemberId.Value,
          ["consentedMemberIds"] = IdentifierArray(goal.CancellationRequest.ConsentedMemberIds),
        };
      }

      SetOptional(item, "voucherId", voucherIds.Contains(goal.Id) ? goal.Id.Value : null);
      SetOptional(item, "lastInterestPeriod", goal.LastInterestPeriod?.Key);

      if (goal.LastInterestRate is null)
      {
        item.Remove("lastInterestRate");
      }
      else
      {
        item["lastInterestRate"] = goal.LastInterestRate.Value;
      }
    }
  }

  private static IReadOnlyDictionary<FamilyMemberId, decimal> ParseRefundableContributions(
    JsonObject goal,
    decimal saved,
    decimal starterBonus,
    decimal interestEarned
  )
  {
    if (!goal.TryGetPropertyValue("refundableContributions", out var stored) || stored is null)
    {
      if (saved > checked(starterBonus + interestEarned))
      {
        throw new FamilyCommandException(
          "The stored savings goal requires explicit refundable-contribution provenance."
        );
      }

      return new Dictionary<FamilyMemberId, decimal>();
    }

    if (stored is not JsonArray contributions)
    {
      throw new FamilyCommandException("Stored refundable contributions must be an array.");
    }

    var result = new Dictionary<FamilyMemberId, decimal>();

    foreach (var node in contributions)
    {
      var contribution = node as JsonObject
        ?? throw new FamilyCommandException("Every stored refundable contribution must be an object.");
      var memberId = new FamilyMemberId(RequiredString(contribution, "memberId"));

      if (!result.TryAdd(memberId, RequiredDecimal(contribution, "amount")))
      {
        throw new FamilyCommandException("A refundable contributor is stored more than once.");
      }
    }

    return result;
  }

  private static void WriteSavingsVouchers(JsonObject state, FamilyEconomy economy)
  {
    var vouchers = new JsonArray();

    foreach (var voucher in economy.SavingsVouchers)
    {
      var item = new JsonObject
      {
        ["id"] = voucher.Id.Value,
        ["goalId"] = voucher.Id.Value,
        ["amount"] = voucher.Amount,
        ["ownerId"] = voucher.OwnerId?.Value,
        ["participantIds"] = IdentifierArray(voucher.Participants),
        ["consentedMemberIds"] = IdentifierArray(voucher.Consents),
        ["status"] = voucher.RedemptionCount > 0
          ? "redeemed"
          : voucher.Redeemable
            ? "redeemable"
            : "pending",
      };
      SetOptional(item, "redeemedAt", voucher.RedeemedAt?.ToString("O", CultureInfo.InvariantCulture));
      vouchers.Add(item);
    }

    state["vouchers"] = vouchers;
  }

  private static void WriteGuardianGifts(
    JsonObject state,
    FamilyEconomy economy,
    GuardianGiftPresentation? addedGift
  )
  {
    var stored = RequiredArray(state, "pendingGuardianGifts");
    var byId = stored
      .OfType<JsonObject>()
      .ToDictionary(item => RequiredString(item, "id"), StringComparer.Ordinal);
    var gifts = economy.GuardianGoalGifts.Where(item => item.Id is not null).ToArray();
    var liveIds = gifts.Select(item => item.Id!.Value).ToHashSet(StringComparer.Ordinal);

    foreach (var removed in byId.Where(item => !liveIds.Contains(item.Key)).Select(item => item.Value).ToArray())
    {
      stored.Remove(removed);
    }

    foreach (var gift in gifts)
    {
      if (!byId.TryGetValue(gift.Id!.Value, out var item))
      {
        if (addedGift is null || addedGift.Id != gift.Id)
        {
          throw new FamilyCommandException("Presentation data for the new pending gift is missing.");
        }

        item = new JsonObject
        {
          ["id"] = gift.Id.Value,
          ["guardianName"] = addedGift.SenderName,
          ["goalTitle"] = addedGift.GoalTitle,
        };
        stored.Insert(0, item);
      }

      item["childId"] = gift.OwnerId.Value;
      SetOptional(item, "senderId", gift.GuardianId?.Value);
      SetOptional(item, "goalId", gift.GoalId?.Value);
      item["destination"] = gift.Destination == GiftDestination.Goal ? "goal" : "balance";
      item["amount"] = gift.Amount;
    }
  }

  private static void WriteSettledPeriods(JsonObject state, FamilyEconomy economy)
  {
    var periods = economy.SettledInterestPeriods.OrderBy(item => item.Start).ToArray();
    state["settledInterestPeriods"] = new JsonArray(
      periods.Select(item => JsonValue.Create(item.Key)).ToArray<JsonNode?>()
    );
    SetOptional(state, "lastSettledInterestPeriod", periods.LastOrDefault()?.Key);
  }

  private static JsonArray IdentifierArray(IEnumerable<DomainIdentifier> identifiers)
  {
    return new JsonArray(
      identifiers.Select(item => JsonValue.Create(item.Value)).ToArray<JsonNode?>()
    );
  }

  private static IsoWeekPeriod? ParseOptionalPeriod(JsonObject item, string property)
  {
    var value = OptionalString(item, property);

    if (value is null)
    {
      return null;
    }

    if (!IsoWeekPeriod.TryParse(value, out var period))
    {
      throw new FamilyCommandException($"The stored {property} value is invalid.");
    }

    return period;
  }

  private static GoalVisibility ParseGoalVisibility(string value)
  {
    return value switch
    {
      "family" => GoalVisibility.Family,
      "guardians" => GoalVisibility.Guardians,
      "private" => GoalVisibility.Private,
      _ => throw new FamilyCommandException("The stored savings-goal visibility is invalid."),
    };
  }

  private static string FormatGoalVisibility(GoalVisibility value)
  {
    return value switch
    {
      GoalVisibility.Family => "family",
      GoalVisibility.Guardians => "guardians",
      GoalVisibility.Private => "private",
      _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };
  }

  private static GiftDestination ParseGiftDestination(string value)
  {
    return value switch
    {
      "balance" => GiftDestination.Balance,
      "goal" => GiftDestination.Goal,
      _ => throw new FamilyCommandException("The stored gift destination is invalid."),
    };
  }

  private static FamilyCurrency ParseSavingsCurrency(string value)
  {
    return value switch
    {
      "CHF" => FamilyCurrency.CHF,
      "EUR" => FamilyCurrency.EUR,
      "HUF" => FamilyCurrency.HUF,
      _ => throw new FamilyCommandException("The stored family currency is invalid."),
    };
  }

  private static ExchangeRate ParseStoredExchangeRate(decimal value)
  {
    if (value < 1 || value > int.MaxValue || decimal.Truncate(value) != value)
    {
      throw new FamilyCommandException("The stored exchange rate is invalid.");
    }

    return new ExchangeRate(decimal.ToInt32(value));
  }
}
