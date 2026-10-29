using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed partial class ExecuteFamilyCommandHandler
{
  private async Task<FamilyCommandResult> ExecuteSavingsAsync(
    FamilyCommand command,
    JsonObject profileState,
    JsonArray members,
    JsonObject actor,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    var savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
    var state = RequireObject(savings.State);
    var goals = RequireArray(state, "goals");
    var vouchers = state["vouchers"] as JsonArray ?? new JsonArray();
    state["vouchers"] = vouchers;
    var settledPeriods = state["settledInterestPeriods"] as JsonArray ?? new JsonArray();
    state["settledInterestPeriods"] = settledPeriods;
    var rewardShop = await RequireSnapshotAsync(command.FamilyId, "reward-shop", cancellationToken);
    var rewardItems = RequireArray(RequireObject(rewardShop.State), "rewards");
    switch (command.CommandType)
    {
      case "savings.set-currency":
        RequireAdministrator(actor);
        var currency = RequiredString(payload, "currency");
        Require(currency is "CHF" or "EUR" or "HUF", "The family currency is invalid.");
        state["familyCurrencyCode"] = currency;
        break;
      case "savings.set-exchange-rate":
        RequireAdministrator(actor);
        var rate = RequiredDecimal(payload, "rate");
        Require(rate >= 1, "The exchange rate must be at least one.");
        state["ladirchenPerCurrencyUnit"] = Round(rate);
        break;
      case "savings.toggle-cheer":
        ToggleCheer(goals, actor, payload);
        break;
      case "savings.add-goal":
        AddGoal(state, goals, vouchers, members, actor, payload, _timeProvider.GetUtcNow());
        break;
      case "savings.update-goal":
        UpdateGoal(goals, actor, payload);
        break;
      case "savings.support-goal":
        SupportGoal(state, goals, vouchers, actor, payload);
        break;
      case "savings.gift-to-goal":
        GiftToGoal(state, goals, vouchers, rewardItems, members, actor, payload);
        break;
      case "savings.gift-to-member":
        GiftToMember(state, rewardItems, members, actor, payload);
        break;
      case "savings.special-gift":
        SpecialGift(state, members, actor, payload);
        break;
      case "savings.consume-guardian-gift":
        ConsumeGuardianGift(state, actor, payload);
        break;
      case "savings.deposit":
        Deposit(state, goals, vouchers, rewardItems, actor, payload);
        break;
      case "savings.withdraw":
        Withdraw(state, goals, actor, payload);
        break;
      case "savings.cancel-goal":
        CancelGoal(state, goals, actor, payload);
        break;
      case "savings.consent-voucher":
        ConsentVoucher(vouchers, actor, payload);
        break;
      case "savings.redeem-voucher":
        RedeemVoucher(vouchers, actor, payload, _timeProvider.GetUtcNow());
        break;
      case "savings.settle-weekly-interest":
        var settlement = await SettleWeeklyInterestAsync(
          command,
          profileState,
          state,
          goals,
          vouchers,
          settledPeriods,
          actor,
          payload,
          cancellationToken
        );

        if (!settlement)
        {
          return new FamilyCommandResult([]);
        }
        break;
      default:
        throw new FamilyCommandException("The savings command is not supported.");
    }

    var changed = await _repository.SaveBatchAsync(
      new[] { CreateSavingsSave(savings, state, command.ActorId) },
      cancellationToken
    );

    foreach (var snapshot in changed)
    {
      await _publisher.PublishAsync(snapshot, cancellationToken);
    }

    return new FamilyCommandResult(changed);
  }

  private static void ToggleCheer(JsonArray goals, JsonObject actor, JsonObject payload)
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    Require(CanViewGoal(actor, goal), "The savings goal is not visible to this member.");
    goal["cheered"] = !BoolValue(goal, "cheered");
  }

  private static void AddGoal(
    JsonObject state,
    JsonArray goals,
    JsonArray vouchers,
    JsonArray members,
    JsonObject actor,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    var id = RequiredString(payload, "id");
    Require(FindById(goals, id) is null, "The savings goal already exists.");
    var ownerId = RequiredString(payload, "ownerId");
    var actorId = RequiredString(actor, "id");
    var ownerIsFamily = ownerId == "family";
    JsonObject? owner = null;

    if (!ownerIsFamily)
    {
      owner = RequireMember(members, ownerId);
    }

    Require(
      ownerIsFamily && (IsAdministrator(actor) || StringValue(actor, "role") == "child")
        || ownerId == actorId
        || IsAdministrator(actor) && StringValue(owner!, "role") == "child",
      "The member may not create this savings goal."
    );
    var target = RequiredDecimal(payload, "target");
    Require(target > 0, "The savings target must be positive.");
    var participantIds = ownerIsFamily ? ResolveFamilyGoalParticipants(members, payload) : [];
    var starterBonus = Math.Min(
      ownerIsFamily
        ? SavingGoal.StarterBonusPerMember * participantIds.Length
        : SavingGoal.StarterBonusPerMember,
      target
    );
    var visibility = ownerIsFamily ? "family" : RequiredString(payload, "visibility");
    Require(visibility is "family" or "guardians" or "private", "The savings visibility is invalid.");
    var goal = new JsonObject
    {
      ["id"] = id,
      ["title"] = RequiredString(payload, "title"),
      ["icon"] = RequiredString(payload, "icon"),
      ["ownerId"] = ownerId,
      ["target"] = target,
      ["saved"] = starterBonus,
      ["starterBonus"] = starterBonus,
      ["interestEarned"] = 0,
      ["shared"] = ownerIsFamily,
      ["visibility"] = visibility,
      ["cheered"] = false,
      ["createdAt"] = now.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
      ["participantIds"] = new JsonArray(participantIds.Select(value => JsonValue.Create(value)).ToArray<JsonNode?>()),
    };
    goals.Add(goal);
    EnsureVoucher(vouchers, goal);
  }

  private static void UpdateGoal(JsonArray goals, JsonObject actor, JsonObject payload)
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    Require(StringValue(goal, "voucherId") is null, "A completed goal with a voucher cannot be edited.");
    var actorId = RequiredString(actor, "id");
    Require(
      StringValue(goal, "ownerId") == actorId || IsAdministrator(actor),
      "Only the owner or an administrator may update this savings goal."
    );
    var target = RequiredDecimal(payload, "target");
    Require(target >= RequiredDecimal(goal, "saved"), "The target cannot be lower than the saved amount.");
    var visibility = StringValue(goal, "ownerId") == "family" ? "family" : RequiredString(payload, "visibility");
    Require(visibility is "family" or "guardians" or "private", "The savings visibility is invalid.");
    goal["title"] = RequiredString(payload, "title");
    goal["icon"] = RequiredString(payload, "icon");
    goal["target"] = target;
    goal["visibility"] = visibility;
  }

  private static void SupportGoal(
    JsonObject state,
    JsonArray goals,
    JsonArray vouchers,
    JsonObject actor,
    JsonObject payload
  )
  {
    Require(StringValue(actor, "role") == "guardian", "Only guardians may support a savings goal.");
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    Require(CanViewGoal(actor, goal), "The savings goal is not visible to this member.");
    var amount = LimitedGoalAmount(goal, RequiredDecimal(payload, "amount"));
    goal["saved"] = RequiredDecimal(goal, "saved") + amount;
    EnsureVoucher(vouchers, goal);

    if (StringValue(goal, "ownerId") != "family")
    {
      AddGuardianGift(state, payload, RequiredString(goal, "ownerId"), RequiredString(goal, "title"), "goal", amount);
    }
  }

  private static void GiftToGoal(
    JsonObject state,
    JsonArray goals,
    JsonArray vouchers,
    JsonArray rewards,
    JsonArray members,
    JsonObject actor,
    JsonObject payload
  )
  {
    Require(StringValue(actor, "role") == "child", "Only children may transfer Ladirchen to a savings goal.");
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    var actorId = RequiredString(actor, "id");
    var ownerId = RequiredString(goal, "ownerId");
    Require(ownerId != actorId && StringValue(goal, "visibility") == "family", "The goal cannot receive this gift.");

    if (ownerId != "family")
    {
      var owner = RequireMember(members, ownerId);
      Require(StringValue(owner, "role") == "child", "A personal gift requires a child recipient.");
    }

    var amount = LimitedGoalAmount(goal, RequiredDecimal(payload, "amount"));
    DebitAvailableBalance(state, rewards, actorId, amount);
    goal["saved"] = RequiredDecimal(goal, "saved") + amount;
    EnsureVoucher(vouchers, goal);

    if (ownerId != "family")
    {
      AddGuardianGift(state, payload, ownerId, RequiredString(goal, "title"), "goal", amount);
    }
  }

  private static void GiftToMember(
    JsonObject state,
    JsonArray rewards,
    JsonArray members,
    JsonObject actor,
    JsonObject payload
  )
  {
    Require(StringValue(actor, "role") == "child", "Only children may transfer Ladirchen to a member.");
    var actorId = RequiredString(actor, "id");
    var recipientId = RequiredString(payload, "recipientId");
    var recipient = RequireMember(members, recipientId);
    Require(StringValue(recipient, "role") == "child" && recipientId != actorId, "A gift requires another child.");
    var amount = PositiveRoundedAmount(payload);
    DebitAvailableBalance(state, rewards, actorId, amount);
    CreditBalance(state, recipientId, amount);
    AddGuardianGift(state, payload, recipientId, string.Empty, "balance", amount);
  }

  private static void SpecialGift(JsonObject state, JsonArray members, JsonObject actor, JsonObject payload)
  {
    RequireAdministrator(actor);
    var childId = RequiredString(payload, "childId");
    var child = RequireMember(members, childId);
    Require(StringValue(child, "role") == "child", "A special gift requires a child recipient.");
    var amount = PositiveRoundedAmount(payload);
    Require(
      amount <= SpecialGiftPolicy.MaximumAmount,
      $"A special gift must not exceed {SpecialGiftPolicy.MaximumAmount} Ladirchen."
    );
    _ = RequiredString(payload, "reason");
    CreditBalance(state, childId, amount);
    AddGuardianGift(state, payload, childId, RequiredString(payload, "reason"), "balance", amount);
  }

  private static void ConsumeGuardianGift(JsonObject state, JsonObject actor, JsonObject payload)
  {
    var gifts = RequireArray(state, "pendingGuardianGifts");
    var gift = FindById(gifts, RequiredString(payload, "id"))
      ?? throw new FamilyCommandException("The guardian gift does not exist.");
    Require(StringValue(gift, "childId") == StringValue(actor, "id"), "Only the recipient may acknowledge this gift.");
    gifts.Remove(gift);
  }

  private static void Deposit(
    JsonObject state,
    JsonArray goals,
    JsonArray vouchers,
    JsonArray rewards,
    JsonObject actor,
    JsonObject payload
  )
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    var actorId = RequiredString(actor, "id");
    Require(StringValue(goal, "ownerId") == actorId, "Only the owner may deposit into this savings goal.");
    var amount = LimitedGoalAmount(goal, RequiredDecimal(payload, "amount"));
    DebitAvailableBalance(state, rewards, actorId, amount);
    goal["saved"] = RequiredDecimal(goal, "saved") + amount;
    EnsureVoucher(vouchers, goal);
  }

  private static void Withdraw(JsonObject state, JsonArray goals, JsonObject actor, JsonObject payload)
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    Require(StringValue(goal, "voucherId") is null, "A completed goal voucher must be redeemed instead.");
    var actorId = RequiredString(actor, "id");
    Require(StringValue(goal, "ownerId") == actorId, "Only the owner may withdraw from this savings goal.");
    var saved = RequiredDecimal(goal, "saved");
    var completed = saved >= RequiredDecimal(goal, "target");
    var withdrawable = completed ? saved : Math.Max(0, saved - (DecimalValue(goal, "starterBonus") ?? 0));
    var requested = RequiredDecimal(payload, "amount");
    Require(requested > 0, "The withdrawal must be positive.");
    var amount = Math.Min(requested, withdrawable);
    Require(amount > 0, "The starter bonus is locked until completion.");
    goal["saved"] = saved - amount;
    CreditBalance(state, actorId, amount);
  }

  private static void CancelGoal(JsonObject state, JsonArray goals, JsonObject actor, JsonObject payload)
  {
    var goal = RequireGoal(goals, RequiredString(payload, "id"));
    Require(StringValue(goal, "voucherId") is null, "A completed goal voucher must be redeemed instead.");
    var actorId = RequiredString(actor, "id");
    var ownerId = RequiredString(goal, "ownerId");
    Require(
      ownerId == actorId || ownerId == "family" && (IsAdministrator(actor) || StringValue(actor, "role") == "child"),
      "The member may not cancel this savings goal."
    );
    var saved = RequiredDecimal(goal, "saved");
    var completed = saved >= RequiredDecimal(goal, "target");
    var returned = completed ? saved : Math.Max(0, saved - (DecimalValue(goal, "starterBonus") ?? 0));
    CreditBalance(state, actorId, returned);
    goals.Remove(goal);
  }

  private static JsonObject RequireGoal(JsonArray goals, string id)
  {
    return FindById(goals, id) ?? throw new FamilyCommandException("The savings goal does not exist.");
  }

  private static bool CanViewGoal(JsonObject actor, JsonObject goal)
  {
    var permissions = FamilyMemberPermissionResolver.Resolve(actor);

    return StringValue(goal, "ownerId") == StringValue(actor, "id")
      || StringValue(goal, "visibility") == "family" && permissions.ViewFamilyGoals
      || StringValue(goal, "visibility") == "guardians" && permissions.ViewGuardianGoals;
  }

  private static decimal LimitedGoalAmount(JsonObject goal, decimal requested)
  {
    Require(requested > 0, "The amount must be positive.");
    var amount = Math.Min(Round(requested), RequiredDecimal(goal, "target") - RequiredDecimal(goal, "saved"));
    Require(amount > 0, "The savings goal is already complete.");

    return amount;
  }

  private static decimal PositiveRoundedAmount(JsonObject payload)
  {
    var amount = Round(RequiredDecimal(payload, "amount"));
    Require(amount > 0, "The rounded amount must be positive.");

    return amount;
  }

  private static void DebitAvailableBalance(
    JsonObject state,
    JsonArray rewards,
    string memberId,
    decimal amount,
    string? excludedRewardId = null
  )
  {
    RequireAvailableBalance(state, rewards, memberId, amount, excludedRewardId);
    var balances = RequireObject(state, "balances");
    var balance = DecimalValue(balances, memberId) ?? 0;
    balances[memberId] = balance - amount;
  }

  private static void RequireAvailableBalance(
    JsonObject state,
    JsonArray rewards,
    string memberId,
    decimal amount,
    string? excludedRewardId = null
  )
  {
    var balance = DecimalValue(RequireObject(state, "balances"), memberId) ?? 0;
    var reserved = rewards
      .OfType<JsonObject>()
      .Where(reward => StringValue(reward, "status") == "requested")
      .Where(reward => StringValue(reward, "requesterId") == memberId)
      .Where(reward => excludedRewardId is null || StringValue(reward, "id") != excludedRewardId)
      .Sum(reward => DecimalValue(reward, "price") ?? 0);
    Require(balance - reserved >= amount, "The transfer exceeds the available balance.");
  }

  private static void CreditBalance(JsonObject state, string memberId, decimal amount)
  {
    var balances = RequireObject(state, "balances");
    balances[memberId] = (DecimalValue(balances, memberId) ?? 0) + amount;
  }

  private static void AddGuardianGift(
    JsonObject state,
    JsonObject payload,
    string childId,
    string goalTitle,
    string destination,
    decimal amount
  )
  {
    RequireArray(state, "pendingGuardianGifts").Add(
      new JsonObject
      {
        ["id"] = RequiredString(payload, "giftId"),
        ["childId"] = childId,
        ["guardianName"] = RequiredString(payload, "senderName"),
        ["goalTitle"] = goalTitle,
        ["destination"] = destination,
        ["amount"] = amount,
      }
    );
  }

  private static string[] ResolveFamilyGoalParticipants(JsonArray members, JsonObject payload)
  {
    var eligible = members
      .OfType<JsonObject>()
      .Where(member => !BoolValue(member, "invitationPending"))
      .Where(member => StringValue(member, "role") is "child" or "guardian")
      .ToArray();
    var scope = StringValue(payload, "participantScope") ?? "family";
    Require(scope is "family" or "children" or "selected", "The family-goal participant scope is invalid.");
    string[] selected;

    if (scope == "family")
    {
      selected = eligible.Select(member => RequiredString(member, "id")).ToArray();
    }
    else if (scope == "children")
    {
      selected = eligible
        .Where(member => StringValue(member, "role") == "child")
        .Select(member => RequiredString(member, "id"))
        .ToArray();
    }
    else
    {
      var requested = payload["participantIds"] as JsonArray
        ?? throw new FamilyCommandException("Selected family goals require participant identifiers.");
      selected = requested
        .Select(RequiredString)
        .Distinct(StringComparer.Ordinal)
        .ToArray();
      var eligibleIds = eligible.Select(member => RequiredString(member, "id")).ToHashSet(StringComparer.Ordinal);
      Require(selected.All(eligibleIds.Contains), "A selected family-goal participant is not an active family member.");
    }

    Require(selected.Length > 0, "A family savings goal requires at least one participant.");

    return selected;
  }

  private static void EnsureVoucher(JsonArray vouchers, JsonObject goal)
  {
    if (
      RequiredDecimal(goal, "saved") < RequiredDecimal(goal, "target")
      || StringValue(goal, "voucherId") is not null
    )
    {
      return;
    }

    var goalId = RequiredString(goal, "id");
    var ownerValue = RequiredString(goal, "ownerId");
    var ownerId = ownerValue == "family" ? null : new FamilyMemberId(ownerValue);
    var participantIds = (goal["participantIds"] as JsonArray ?? new JsonArray())
      .Select(RequiredString)
      .Select(value => new FamilyMemberId(value))
      .ToArray();
    var domainVoucher = new SavingsVoucher(RequiredDecimal(goal, "saved"), ownerId, participantIds);
    var voucher = new JsonObject
    {
      ["id"] = goalId,
      ["goalId"] = goalId,
      ["amount"] = domainVoucher.Amount,
      ["ownerId"] = domainVoucher.OwnerId?.Value,
      ["participantIds"] = new JsonArray(domainVoucher.Participants.Select(item => JsonValue.Create(item.Value)).ToArray<JsonNode?>()),
      ["consentedMemberIds"] = new JsonArray(),
      ["status"] = domainVoucher.Redeemable ? "redeemable" : "pending",
    };
    vouchers.Add(voucher);
    goal["voucherId"] = goalId;
  }

  private static void ConsentVoucher(JsonArray vouchers, JsonObject actor, JsonObject payload)
  {
    var voucher = FindById(vouchers, RequiredString(payload, "id"))
      ?? throw new FamilyCommandException("The savings voucher does not exist.");
    Require(StringValue(voucher, "ownerId") is null, "A personal voucher does not require family consent.");
    Require(StringValue(voucher, "status") != "redeemed", "A redeemed voucher cannot receive consent.");
    var actorId = new FamilyMemberId(RequiredString(actor, "id"));
    var domainVoucher = RehydrateVoucher(voucher);
    Require(domainVoucher.Participants.Contains(actorId), "Only a voucher participant may consent.");
    _ = domainVoucher.Consent(actorId);
    WriteVoucherState(voucher, domainVoucher);
  }

  private static void RedeemVoucher(
    JsonArray vouchers,
    JsonObject actor,
    JsonObject payload,
    DateTimeOffset redeemedAt
  )
  {
    var voucher = FindById(vouchers, RequiredString(payload, "id"))
      ?? throw new FamilyCommandException("The savings voucher does not exist.");
    var actorId = new FamilyMemberId(RequiredString(actor, "id"));
    var domainVoucher = RehydrateVoucher(voucher);
    Require(
      domainVoucher.OwnerId == actorId || domainVoucher.Participants.Contains(actorId),
      "Only the voucher owner or a family-goal participant may redeem it."
    );
    Require(domainVoucher.Redeem(), "The savings voucher is not redeemable or was already redeemed.");
    WriteVoucherState(voucher, domainVoucher);
    voucher["redeemedAt"] = redeemedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
  }

  private static SavingsVoucher RehydrateVoucher(JsonObject voucher)
  {
    var owner = StringValue(voucher, "ownerId");
    var participants = RequireArray(voucher, "participantIds")
      .Select(RequiredString)
      .Select(value => new FamilyMemberId(value))
      .ToArray();
    var domainVoucher = new SavingsVoucher(
      RequiredDecimal(voucher, "amount"),
      owner is null ? null : new FamilyMemberId(owner),
      participants
    );

    foreach (var consent in RequireArray(voucher, "consentedMemberIds").Select(RequiredString))
    {
      _ = domainVoucher.Consent(new FamilyMemberId(consent));
    }

    if (StringValue(voucher, "status") == "redeemed")
    {
      Require(domainVoucher.Redeem(), "The stored savings voucher is inconsistent.");
    }

    return domainVoucher;
  }

  private static void WriteVoucherState(JsonObject voucher, SavingsVoucher domainVoucher)
  {
    voucher["consentedMemberIds"] = new JsonArray(
      domainVoucher.Consents.Select(item => JsonValue.Create(item.Value)).ToArray<JsonNode?>()
    );
    voucher["status"] = domainVoucher.RedemptionCount > 0
      ? "redeemed"
      : domainVoucher.Redeemable
        ? "redeemable"
        : "pending";
  }

  private async Task<bool> SettleWeeklyInterestAsync(
    FamilyCommand command,
    JsonObject profile,
    JsonObject savings,
    JsonArray goals,
    JsonArray vouchers,
    JsonArray settledPeriods,
    JsonObject actor,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    RequireAdministrator(actor);
    var period = new IsoWeekPeriod(RequiredInt(payload, "year"), RequiredInt(payload, "week"));

    if (settledPeriods.Any(item => item?.GetValue<string>() == period.Key))
    {
      return false;
    }

    var zone = new IanaTimeZone(RequiredString(profile, "timeZone"));
    var currentPeriod = IsoWeekPeriod.For(zone.LocalDate(_timeProvider.GetUtcNow()));
    Require(period.Start < currentPeriod.Start, "Only a closed ISO week can be settled.");
    var progression = await RequireSnapshotAsync(command.FamilyId, "family-progression", cancellationToken);
    var progressionState = RequireObject(progression.State);
    var weeklyEvaluations = progressionState["weeklyEvaluations"] as JsonObject
      ?? throw new FamilyCommandException("No closed-week evaluations are available.");
    var week = weeklyEvaluations[period.Key] as JsonObject
      ?? throw new FamilyCommandException("The requested week has no immutable Ladi evaluation.");
    var evaluations = RequireObject(week, "members");
    var maximumRate = new Percentage(
      DecimalValue(progressionState, "maximumSavingsInterestRate") ?? SavingsInterestPolicy.Default.MaximumRate.Value
    );
    var interestPolicy = new WeeklyLadiInterestPolicy();

    foreach (
      var goal in goals
        .OfType<JsonObject>()
        .Where(goal => StringValue(goal, "ownerId") != "family")
        .Where(goal => StringValue(goal, "voucherId") is null)
        .Where(goal => RequiredDecimal(goal, "saved") > 0)
        .Where(goal => RequiredDecimal(goal, "saved") < RequiredDecimal(goal, "target"))
    )
    {
      var createdAt = ParseOptionalInstant(goal, "createdAt");

      if (createdAt is not null && zone.LocalDate(createdAt.Value) > period.Start.AddDays(6))
      {
        continue;
      }

      var ownerId = RequiredString(goal, "ownerId");
      var evaluation = evaluations[ownerId] as JsonObject
        ?? throw new FamilyCommandException("Every eligible savings-goal owner requires a closed-week evaluation.");
      var stage = new LadiStage(RequiredString(evaluation, "ladiStageId"), RequiredInt(evaluation, "ladiStagePosition"));
      var participation = new PersonalParticipation(
        RequiredInt(evaluation, "streakDays"),
        RequiredDecimal(evaluation, "completionPercent"),
        RequiredDecimal(evaluation, "averageBasicRating")
      );
      var participationValue = RequiredDecimal(evaluation, "participationValue");
      var domainEvaluation = new PersonalLadiEvaluation(
        new FamilyMemberId(ownerId),
        period.Year,
        period.Week,
        participation,
        new ParticipationValue(participationValue, participationValue),
        stage
      );
      var rate = interestPolicy.RateFor(domainEvaluation, maximumRate);
      var credit = SavingsInterestPolicy.CalculateCredit(
        new LadirchenAmount(RequiredDecimal(goal, "saved")),
        new LadirchenAmount(RequiredDecimal(goal, "target")),
        rate
      );

      if (credit <= 0)
      {
        continue;
      }

      goal["saved"] = RequiredDecimal(goal, "saved") + credit;
      goal["interestEarned"] = (DecimalValue(goal, "interestEarned") ?? 0) + credit;
      goal["lastInterestPeriod"] = period.Key;
      goal["lastInterestRate"] = rate.Value;
      EnsureVoucher(vouchers, goal);
    }

    settledPeriods.Add(period.Key);
    savings["lastSettledInterestPeriod"] = period.Key;

    return true;
  }

  private static PendingFamilyAggregateSave CreateSavingsSave(
    FamilyAggregateSnapshot current,
    JsonObject state,
    FamilyMemberId actorId
  )
  {
    using var document = JsonDocument.Parse(state.ToJsonString());
    var command = new SaveFamilyAggregateCommand(
      current.FamilyId,
      current.AggregateType,
      current.Revision,
      document.RootElement.Clone(),
      actorId
    );

    return new PendingFamilyAggregateSave(command, 2);
  }

}
