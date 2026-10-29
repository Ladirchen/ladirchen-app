using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed class SavingsFamilyCommandHandler : FamilyCommandAreaHandler
{
  public SavingsFamilyCommandHandler(
    IFamilyAggregateSnapshotRepository repository,
    TimeProvider timeProvider
  )
    : base(repository, timeProvider, "savings.")
  {
  }

  public override async Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    FamilyAggregateSnapshot profileSnapshot,
    JsonObject profileState,
    JsonObject payload,
    CancellationToken cancellationToken
  )
  {
    if (
      command.AuthenticatedMemberId is not null
      && command.AuthenticatedMemberId != command.ActorId
    )
    {
      throw new DomainRuleException(
        DomainError.Forbidden,
        "Savings commands must be executed as the authenticated family member."
      );
    }

    var family = FamilyProfileSnapshotMapper.Rehydrate(command.FamilyId, profileState);
    var savings = await RequireSnapshotAsync(command.FamilyId, "savings", cancellationToken);
    var savingsState = RequireObject(savings.State);
    var rewardShop = await RequireSnapshotAsync(command.FamilyId, "reward-shop", cancellationToken);
    var rewardShopState = RequireObject(rewardShop.State);
    var economy = FamilyEconomySnapshotMapper.RehydrateSavings(family, savingsState, rewardShopState);
    var now = _timeProvider.GetUtcNow();
    var operationId = new TransactionId(command.CommandId.ToString("N", CultureInfo.InvariantCulture));
    SavingGoalPresentation? goalPresentation = null;
    GuardianGiftPresentation? giftPresentation = null;
    FamilyAggregateSnapshot? progression = null;

    switch (command.CommandType)
    {
      case "savings.set-currency":
        family.UpdateSettings(
          command.ActorId,
          family.TimeZone,
          ParseSavingsCurrency(RequiredString(payload, "currency")),
          family.ExchangeRate,
          now
        );
        break;
      case "savings.set-exchange-rate":
        family.UpdateSettings(
          command.ActorId,
          family.TimeZone,
          family.Currency,
          ParseSavingsExchangeRate(RequiredDecimal(payload, "rate")),
          now
        );
        break;
      case "savings.toggle-cheer":
        economy.ToggleGoalCheer(command.ActorId, GoalId(payload), now);
        break;
      case "savings.add-goal":
        goalPresentation = AddSavingsGoal(economy, family, command.ActorId, payload, operationId, now);
        break;
      case "savings.update-goal":
        goalPresentation = UpdateSavingsGoal(economy, command.ActorId, payload, now);
        break;
      case "savings.support-goal":
        giftPresentation = SupportSavingsGoal(economy, family, command.ActorId, payload, operationId, now);
        break;
      case "savings.gift-to-goal":
        giftPresentation = GiftToSavingsGoal(economy, family, command.ActorId, payload, operationId, now);
        break;
      case "savings.gift-to-member":
        giftPresentation = GiftToFamilyMember(economy, family, command.ActorId, payload, operationId, now);
        break;
      case "savings.special-gift":
        giftPresentation = GiveSpecialSavingsGift(economy, family, command.ActorId, payload, operationId, now);
        break;
      case "savings.consume-guardian-gift":
        economy.ConsumeGuardianGift(command.ActorId, GiftId(payload));
        break;
      case "savings.deposit":
        _ = economy.DepositToGoal(
          command.ActorId,
          GoalId(payload),
          new LadirchenAmount(RequiredDecimal(payload, "amount")),
          operationId,
          now
        );
        break;
      case "savings.withdraw":
        _ = economy.WithdrawFromGoal(
          command.ActorId,
          GoalId(payload),
          new LadirchenAmount(RequiredDecimal(payload, "amount")),
          operationId,
          now
        );
        break;
      case "savings.cancel-goal":
        _ = economy.CancelGoal(command.ActorId, GoalId(payload), operationId, now);
        break;
      case "savings.request-goal-cancellation":
        economy.RequestGoalCancellation(command.ActorId, GoalId(payload), operationId, now);
        break;
      case "savings.decide-goal-cancellation":
        _ = economy.DecideGoalCancellation(
          command.ActorId,
          GoalId(payload),
          RequiredBool(payload, "approved"),
          operationId,
          now
        );
        break;
      case "savings.consent-voucher":
        _ = economy.ConsentSavingsVoucher(command.ActorId, GoalId(payload), now);
        break;
      case "savings.redeem-voucher":
        economy.RedeemSavingsVoucher(command.ActorId, GoalId(payload), now);
        break;
      case "savings.settle-weekly-interest":
        progression = await SettleWeeklyInterestAsync(
          command,
          family,
          economy,
          payload,
          operationId,
          now,
          cancellationToken
        );

        if (progression is null)
        {
          return new FamilyCommandResult([]);
        }

        break;
      default:
        throw new FamilyCommandException("The savings command is not supported.");
    }

    var updatedState = FamilyEconomySnapshotMapper.WriteSavings(
      savingsState,
      family,
      economy,
      goalPresentation,
      giftPresentation
    );
    var guards = new List<FamilyAggregateRevisionGuard>
    {
      FamilyAggregateRevisionGuard.FromSnapshot(profileSnapshot),
      FamilyAggregateRevisionGuard.FromSnapshot(rewardShop),
    };

    if (progression is not null)
    {
      guards.Add(FamilyAggregateRevisionGuard.FromSnapshot(progression));
    }

    var changed = await _repository.SaveBatchAsync(
      [CreateSave(savings, updatedState, command.ActorId)],
      guards,
      cancellationToken
    );

    return new FamilyCommandResult(changed);
  }

  private static SavingGoalPresentation AddSavingsGoal(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonObject payload,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var id = GoalId(payload);
    var ownerValue = RequiredString(payload, "ownerId");
    var owner = ownerValue == "family"
      ? GoalOwner.Family
      : GoalOwner.Member(new FamilyMemberId(ownerValue));
    var participants = owner.IsFamily
      ? ResolveFamilyGoalParticipants(family, payload)
      : Array.Empty<FamilyMemberId>();
    economy.AddGoal(
      actorId,
      id,
      new SavingGoalDefinition(
        owner,
        RequiredString(payload, "title"),
        new LadirchenAmount(RequiredDecimal(payload, "target")),
        owner.IsFamily ? GoalVisibility.Family : ParseGoalVisibility(RequiredString(payload, "visibility")),
        participants
      ),
      operationId,
      now
    );

    return new SavingGoalPresentation(id, RequiredString(payload, "icon"));
  }

  private static SavingGoalPresentation UpdateSavingsGoal(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonObject payload,
    DateTimeOffset now
  )
  {
    var id = GoalId(payload);
    var goal = economy.GoalFor(actorId, id);
    economy.UpdateGoal(
      actorId,
      id,
      RequiredString(payload, "title"),
      new LadirchenAmount(RequiredDecimal(payload, "target")),
      goal.Owner.IsFamily
        ? GoalVisibility.Family
        : ParseGoalVisibility(RequiredString(payload, "visibility")),
      now
    );

    return new SavingGoalPresentation(id, RequiredString(payload, "icon"));
  }

  private static GuardianGiftPresentation? SupportSavingsGoal(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonObject payload,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var goal = economy.GoalFor(actorId, GoalId(payload));
    var giftId = goal.Owner.IsFamily ? null : GiftId(payload);
    _ = economy.SupportGoal(
      actorId,
      goal.Id,
      new LadirchenAmount(RequiredDecimal(payload, "amount")),
      operationId,
      now,
      giftId
    );

    return giftId is null
      ? null
      : new GuardianGiftPresentation(giftId, family.ActiveMember(actorId).Name, goal.Title);
  }

  private static GuardianGiftPresentation? GiftToSavingsGoal(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonObject payload,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var goal = economy.GoalFor(actorId, GoalId(payload));
    var giftId = goal.Owner.IsFamily ? null : GiftId(payload);
    _ = economy.GiftToGoal(
      actorId,
      goal.Id,
      new LadirchenAmount(RequiredDecimal(payload, "amount")),
      operationId,
      now,
      giftId
    );

    return giftId is null
      ? null
      : new GuardianGiftPresentation(giftId, family.ActiveMember(actorId).Name, goal.Title);
  }

  private static GuardianGiftPresentation GiftToFamilyMember(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonObject payload,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var giftId = GiftId(payload);
    economy.GiftToMember(
      actorId,
      new FamilyMemberId(RequiredString(payload, "recipientId")),
      new LadirchenAmount(RequiredDecimal(payload, "amount")),
      operationId,
      now,
      giftId
    );

    return new GuardianGiftPresentation(giftId, family.ActiveMember(actorId).Name, string.Empty);
  }

  private static GuardianGiftPresentation GiveSpecialSavingsGift(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonObject payload,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var giftId = GiftId(payload);
    var reason = RequiredString(payload, "reason");
    economy.GiveSpecialGift(
      actorId,
      new FamilyMemberId(RequiredString(payload, "childId")),
      new LadirchenAmount(RequiredDecimal(payload, "amount")),
      reason,
      operationId,
      now,
      giftId
    );

    return new GuardianGiftPresentation(giftId, family.ActiveMember(actorId).Name, reason);
  }

  private async Task<FamilyAggregateSnapshot?> SettleWeeklyInterestAsync(
    FamilyCommand command,
    Family family,
    FamilyEconomy economy,
    JsonObject payload,
    TransactionId operationId,
    DateTimeOffset now,
    CancellationToken cancellationToken
  )
  {
    var period = new IsoWeekPeriod(RequiredInt(payload, "year"), RequiredInt(payload, "week"));

    if (economy.SettledInterestPeriods.Contains(period))
    {
      return null;
    }

    var progression = await RequireSnapshotAsync(command.FamilyId, "family-progression", cancellationToken);
    var progressionState = RequireObject(progression.State);
    var closedWeek = FamilyProgressionSnapshotMapper.ReadClosedWeek(progressionState, period);
    var maximumRate = FamilyProgressionSnapshotMapper.ReadMaximumSavingsInterestRate(progressionState);
    _ = economy.ApplyWeeklyInterest(
      command.ActorId,
      period,
      closedWeek.MemberEvaluations,
      new SavingsInterestPolicy(maximumRate),
      operationId,
      now
    );

    return progression;
  }

  private static FamilyMemberId[] ResolveFamilyGoalParticipants(Family family, JsonObject payload)
  {
    var scope = StringValue(payload, "participantScope") ?? "family";
    var selection = scope switch
    {
      "family" => FamilyGoalParticipation.EntireFamily,
      "children" => FamilyGoalParticipation.ChildrenOnly,
      "selected" => FamilyGoalParticipation.SelectedMembers,
      _ => throw new FamilyCommandException("The family-goal participant scope is invalid."),
    };
    var selected = selection == FamilyGoalParticipation.SelectedMembers
      ? (payload["participantIds"] as JsonArray
          ?? throw new FamilyCommandException("Selected family goals require participant identifiers."))
        .Select(RequiredString)
        .Select(value => new FamilyMemberId(value))
      : null;

    return new FamilyGoalParticipantPolicy().Select(family.Members, selection, selected).ToArray();
  }

  private static SavingGoalId GoalId(JsonObject payload)
  {
    return new SavingGoalId(RequiredString(payload, "id"));
  }

  private static GuardianGiftId GiftId(JsonObject payload)
  {
    return new GuardianGiftId(RequiredString(payload, "giftId"));
  }

  private static GoalVisibility ParseGoalVisibility(string value)
  {
    return value switch
    {
      "family" => GoalVisibility.Family,
      "guardians" => GoalVisibility.Guardians,
      "private" => GoalVisibility.Private,
      _ => throw new FamilyCommandException("The savings-goal visibility is invalid."),
    };
  }

  private static FamilyCurrency ParseSavingsCurrency(string value)
  {
    return value switch
    {
      "CHF" => FamilyCurrency.CHF,
      "EUR" => FamilyCurrency.EUR,
      "HUF" => FamilyCurrency.HUF,
      _ => throw new FamilyCommandException("The family currency is invalid."),
    };
  }

  private static ExchangeRate ParseSavingsExchangeRate(decimal value)
  {
    if (
      !new FamilyCurrencySettingsPolicy().TryNormalize(
        FamilyCurrency.CHF.ToString(),
        value,
        out _,
        out var rate
      )
    )
    {
      throw new FamilyCommandException("The exchange rate must be a valid positive value.");
    }

    return rate!;
  }
}
