using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

internal sealed class SavingsFamilyCommandHandler : TypedFamilyCommandAreaHandler
{
  private readonly IFamilyEconomyRepository _economyRepository;
  private readonly IFamilyProgressionRepository _progressionRepository;

  public SavingsFamilyCommandHandler(
    IFamilyEconomyRepository economyRepository,
    IFamilyProgressionRepository progressionRepository,
    ITypedAggregateUnitOfWork unitOfWork,
    TimeProvider timeProvider
  )
    : base(unitOfWork, timeProvider, "savings.")
  {
    _economyRepository = economyRepository ?? throw new ArgumentNullException(nameof(economyRepository));
    _progressionRepository = progressionRepository
      ?? throw new ArgumentNullException(nameof(progressionRepository));
  }

  public override async Task<FamilyCommandResult> HandleAsync(
    FamilyCommand command,
    LoadedAggregate<Family> family,
    CancellationToken cancellationToken
  )
  {
    ArgumentNullException.ThrowIfNull(command);
    ArgumentNullException.ThrowIfNull(family);

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

    var payload = RequiredObject(command.Payload);
    var now = TimeProvider.GetUtcNow();

    if (command.CommandType is "savings.set-currency" or "savings.set-exchange-rate")
    {
      return await UpdateFamilySettingsAsync(
        command,
        family,
        payload,
        now,
        cancellationToken
      );
    }

    var loadedEconomy = await _economyRepository.LoadAsync(family.Root, cancellationToken)
      ?? throw new FamilyCommandException("The family economy has not been initialized.");
    var economy = loadedEconomy.Root;
    var operationId = OperationId(command);
    SavingGoalPresentationState? goalPresentation = null;
    GuardianGiftPresentationState? giftPresentation = null;
    LoadedAggregate<FamilyHouse>? progression = null;

    switch (command.CommandType)
    {
      case "savings.toggle-cheer":
        economy.ToggleGoalCheer(command.ActorId, GoalId(payload), now);
        break;
      case "savings.add-goal":
        goalPresentation = AddSavingsGoal(
          economy,
          family.Root,
          command.ActorId,
          payload,
          operationId,
          now
        );
        break;
      case "savings.update-goal":
        goalPresentation = UpdateSavingsGoal(economy, command.ActorId, payload, now);
        break;
      case "savings.support-goal":
        giftPresentation = SupportSavingsGoal(
          economy,
          family.Root,
          command.ActorId,
          payload,
          operationId,
          now
        );
        break;
      case "savings.gift-to-goal":
        giftPresentation = GiftToSavingsGoal(
          economy,
          family.Root,
          command.ActorId,
          payload,
          operationId,
          now
        );
        break;
      case "savings.gift-to-member":
        giftPresentation = GiftToFamilyMember(
          economy,
          family.Root,
          command.ActorId,
          payload,
          operationId,
          now
        );
        break;
      case "savings.special-gift":
        giftPresentation = GiveSpecialSavingsGift(
          economy,
          family.Root,
          command.ActorId,
          payload,
          operationId,
          now
        );
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
          family.Root,
          economy,
          payload,
          operationId,
          now,
          cancellationToken
        );

        if (progression is null)
        {
          return new FamilyCommandResult(Array.Empty<FamilyAggregateChange>());
        }

        break;
      default:
        throw new FamilyCommandException("The savings command is not supported.");
    }

    var presentation = UpdatePresentation(
      loadedEconomy.Presentation,
      economy,
      goalPresentation,
      giftPresentation
    );
    var guards = progression is null
      ? new[]
      {
        new TypedAggregateRevisionGuard(TypedAggregateKind.Family, family.Revision),
      }
      :
      [
        new TypedAggregateRevisionGuard(TypedAggregateKind.Family, family.Revision),
        new TypedAggregateRevisionGuard(
          TypedAggregateKind.FamilyProgression,
          progression.Revision
        ),
      ];
    var saved = await UnitOfWork.SaveAsync(
      new TypedAggregateSaveBatch(
        command.FamilyId,
        command.ActorId,
        familyEconomy: new PendingFamilyEconomySave(
          economy,
          presentation,
          loadedEconomy.Revision
        ),
        revisionGuards: guards
      ),
      cancellationToken
    );
    var changedEconomy = saved.FamilyEconomy
      ?? throw new InvalidOperationException("The family economy was not saved.");

    return EconomyResult(command.FamilyId, changedEconomy);
  }

  private async Task<FamilyCommandResult> UpdateFamilySettingsAsync(
    FamilyCommand command,
    LoadedAggregate<Family> family,
    JsonElement payload,
    DateTimeOffset now,
    CancellationToken cancellationToken
  )
  {
    if (command.CommandType == "savings.set-currency")
    {
      family.Root.UpdateSettings(
        command.ActorId,
        family.Root.TimeZone,
        ParseSavingsCurrency(RequiredString(payload, "currency")),
        family.Root.ExchangeRate,
        now
      );
    }
    else
    {
      family.Root.UpdateSettings(
        command.ActorId,
        family.Root.TimeZone,
        family.Root.Currency,
        ParseSavingsExchangeRate(RequiredDecimal(payload, "rate")),
        now
      );
    }

    var saved = await UnitOfWork.SaveAsync(
      new TypedAggregateSaveBatch(
        command.FamilyId,
        command.ActorId,
        family: new PendingFamilySave(family.Root, family.Revision)
      ),
      cancellationToken
    );
    var changedFamily = saved.Family
      ?? throw new InvalidOperationException("The family was not saved.");

    return new FamilyCommandResult(
    [
      Change(command.FamilyId, "family-profile", changedFamily.Revision, changedFamily.Audit),
      Change(command.FamilyId, "savings", changedFamily.Revision, changedFamily.Audit),
    ]);
  }

  private static SavingGoalPresentationState AddSavingsGoal(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonElement payload,
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
        owner.IsFamily
          ? GoalVisibility.Family
          : ParseGoalVisibility(RequiredString(payload, "visibility")),
        participants
      ),
      operationId,
      now
    );

    return new SavingGoalPresentationState(id, RequiredString(payload, "icon"));
  }

  private static SavingGoalPresentationState UpdateSavingsGoal(
    FamilyEconomy economy,
    FamilyMemberId actorId,
    JsonElement payload,
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

    return new SavingGoalPresentationState(id, RequiredString(payload, "icon"));
  }

  private static GuardianGiftPresentationState? SupportSavingsGoal(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonElement payload,
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
      : new GuardianGiftPresentationState(
        giftId,
        family.ActiveMember(actorId).Name,
        goal.Title
      );
  }

  private static GuardianGiftPresentationState? GiftToSavingsGoal(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonElement payload,
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
      : new GuardianGiftPresentationState(
        giftId,
        family.ActiveMember(actorId).Name,
        goal.Title
      );
  }

  private static GuardianGiftPresentationState GiftToFamilyMember(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonElement payload,
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

    return new GuardianGiftPresentationState(
      giftId,
      family.ActiveMember(actorId).Name,
      string.Empty
    );
  }

  private static GuardianGiftPresentationState GiveSpecialSavingsGift(
    FamilyEconomy economy,
    Family family,
    FamilyMemberId actorId,
    JsonElement payload,
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

    return new GuardianGiftPresentationState(
      giftId,
      family.ActiveMember(actorId).Name,
      reason
    );
  }

  private async Task<LoadedAggregate<FamilyHouse>?> SettleWeeklyInterestAsync(
    FamilyCommand command,
    Family family,
    FamilyEconomy economy,
    JsonElement payload,
    TransactionId operationId,
    DateTimeOffset now,
    CancellationToken cancellationToken
  )
  {
    var period = new IsoWeekPeriod(
      RequiredInt(payload, "year"),
      RequiredInt(payload, "week")
    );

    if (economy.SettledInterestPeriods.Contains(period))
    {
      return null;
    }

    var progression = await _progressionRepository.LoadAsync(family, cancellationToken)
      ?? throw new FamilyCommandException("The family progression has not been initialized.");
    var closedWeek = progression.Root.ClosedWeekFor(period);
    _ = economy.ApplyWeeklyInterest(
      command.ActorId,
      period,
      closedWeek.MemberEvaluations,
      new SavingsInterestPolicy(progression.Root.MaximumSavingsInterestRate),
      operationId,
      now
    );

    return progression;
  }

  private static FamilyEconomyPresentationState UpdatePresentation(
    FamilyEconomyPresentationState presentation,
    FamilyEconomy economy,
    SavingGoalPresentationState? changedGoal,
    GuardianGiftPresentationState? addedGift
  )
  {
    var liveGoalIds = economy.SavingsGoals
      .Where(item => !item.Cancelled)
      .Select(item => item.Id)
      .ToHashSet();
    var goals = presentation.SavingGoals
      .Where(item => liveGoalIds.Contains(item.GoalId))
      .Where(item => item.GoalId != changedGoal?.GoalId)
      .ToList();

    if (changedGoal is not null)
    {
      goals.Add(changedGoal);
    }

    Require(
      liveGoalIds.All(id => goals.Any(item => item.GoalId == id)),
      "Presentation data for a savings goal is missing."
    );
    var liveGiftIds = economy.GuardianGoalGifts
      .Select(item => item.Id)
      .OfType<GuardianGiftId>()
      .ToHashSet();
    var gifts = presentation.GuardianGifts
      .Where(item => liveGiftIds.Contains(item.GiftId))
      .ToList();

    if (addedGift is not null)
    {
      Require(
        gifts.All(item => item.GiftId != addedGift.GiftId),
        "Presentation data for the guardian gift already exists."
      );
      gifts.Add(addedGift);
    }

    Require(
      liveGiftIds.All(id => gifts.Any(item => item.GiftId == id)),
      "Presentation data for a guardian gift is missing."
    );

    return new FamilyEconomyPresentationState(
      presentation.Contributions,
      goals,
      presentation.ShopRewards,
      gifts
    );
  }

  private static FamilyCommandResult EconomyResult(
    FamilyId familyId,
    LoadedFamilyEconomy changedEconomy
  )
  {
    return new FamilyCommandResult(
    [
      Change(familyId, "savings", changedEconomy.Revision, changedEconomy.Audit),
      Change(familyId, "reward-shop", changedEconomy.Revision, changedEconomy.Audit),
      Change(familyId, "home-customization", changedEconomy.Revision, changedEconomy.Audit),
    ]);
  }

  private static FamilyMemberId[] ResolveFamilyGoalParticipants(
    Family family,
    JsonElement payload
  )
  {
    var scope = OptionalString(payload, "participantScope") ?? "family";
    var selection = scope switch
    {
      "family" => FamilyGoalParticipation.EntireFamily,
      "children" => FamilyGoalParticipation.ChildrenOnly,
      "selected" => FamilyGoalParticipation.SelectedMembers,
      _ => throw new FamilyCommandException("The family-goal participant scope is invalid."),
    };
    var selected = selection == FamilyGoalParticipation.SelectedMembers
      ? RequiredArray(payload, "participantIds")
        .Select(RequiredString)
        .Select(value => new FamilyMemberId(value))
      : null;

    return new FamilyGoalParticipantPolicy().Select(family.Members, selection, selected).ToArray();
  }

  private static SavingGoalId GoalId(JsonElement payload)
  {
    return new SavingGoalId(RequiredString(payload, "id"));
  }

  private static GuardianGiftId GiftId(JsonElement payload)
  {
    return new GuardianGiftId(RequiredString(payload, "giftId"));
  }

  private static TransactionId OperationId(FamilyCommand command)
  {
    return new TransactionId(command.CommandId.ToString("N", CultureInfo.InvariantCulture));
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
