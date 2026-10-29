using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class SavingsCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("92e1f86d-7009-4c5f-9309-2d5d43cfdfb1"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly DateTimeOffset _now = new(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);

  [Test]
  public async Task DepositUsesAvailableFundsAndIssuesPersonalVoucher()
  {
    var store = CreateStoreWithReservation(100, 80);
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "bike",
      ownerId = _childId.Value,
      title = "Bicycle",
      icon = "bike-icon",
      target = 20,
      visibility = "private",
    });
    await ExecuteAsync(handler, _childId, "savings.deposit", new
    {
      id = "bike",
      amount = 15,
    });

    var economy = store.RequiredEconomy();
    var goal = economy.SavingsGoals.Single(item => item.Id == new SavingGoalId("bike"));
    var voucher = economy.SavingsVouchers.Single();
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(85);
    await Assert.That(economy.WalletFor(_childId).Available).IsEqualTo(5);
    await Assert.That(goal.Saved).IsEqualTo(20);
    await Assert.That(store.EconomyPresentation.SavingGoals.Single().Icon)
      .IsEqualTo("bike-icon");
    await Assert.That(voucher.Amount).IsEqualTo(20);
    await Assert.That(voucher.OwnerId).IsEqualTo(_childId);
    await Assert.That(voucher.Redeemable).IsTrue();

    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "camera",
      ownerId = _childId.Value,
      title = "Camera",
      icon = "camera-icon",
      target = 20,
      visibility = "private",
    });
    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "savings.deposit", new
      {
        id = "camera",
        amount = 6,
      })
    ).Throws<DomainRuleException>();
    await Assert.That(store.RequiredEconomy().WalletFor(_childId).Balance).IsEqualTo(85);
  }

  [Test]
  public async Task GuardianCannotExecuteOwnerPrivateSavingsCommandAsChild()
  {
    var store = CreateStore(20);
    var handler = store.CreateCommandHandler();
    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "private-bike",
      ownerId = _childId.Value,
      title = "Private bicycle",
      icon = "bike-icon",
      target = 20,
      visibility = "private",
    });

    await Assert.That(async () =>
      await ExecuteAsync(
        handler,
        _childId,
        "savings.deposit",
        new { id = "private-bike", amount = 5 },
        _guardianId
      )
    ).Throws<DomainRuleException>();

    var economy = store.RequiredEconomy();
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(20);
    await Assert.That(economy.SavingsGoals.Single().Saved).IsEqualTo(5);
  }

  [Test]
  public async Task FamilyVoucherRequiresEverySelectedParticipantAndCanBeRedeemedOnce()
  {
    var store = CreateStore(0);
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "savings.add-goal", new
    {
      id = "family-trip",
      ownerId = "family",
      title = "Family trip",
      icon = "trip-icon",
      target = 20,
      visibility = "family",
      participantScope = "selected",
      participantIds = new[] { _childId.Value, _guardianId.Value },
    });
    await ExecuteAsync(handler, _guardianId, "savings.support-goal", new
    {
      id = "family-trip",
      amount = 10,
    });
    await ExecuteAsync(handler, _childId, "savings.consent-voucher", new
    {
      id = "family-trip",
    });

    var voucher = store.RequiredEconomy().SavingsVouchers.Single();
    await Assert.That(voucher.Redeemable).IsFalse();
    await ExecuteAsync(handler, _guardianId, "savings.consent-voucher", new
    {
      id = "family-trip",
    });
    voucher = store.RequiredEconomy().SavingsVouchers.Single();
    await Assert.That(voucher.Redeemable).IsTrue();
    await ExecuteAsync(handler, _childId, "savings.redeem-voucher", new
    {
      id = "family-trip",
    });
    voucher = store.RequiredEconomy().SavingsVouchers.Single();
    await Assert.That(voucher.RedemptionCount).IsEqualTo(1);
    await Assert.That(voucher.RedeemedAt).IsNotNull();

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "savings.redeem-voucher", new
      {
        id = "family-trip",
      })
    ).Throws<DomainRuleException>();
  }

  [Test]
  public async Task WeeklySettlementUsesStoredLadiStageAndConfiguredMaximumRate()
  {
    var store = CreateWeeklyInterestStore();
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "savings.settle-weekly-interest", new
    {
      year = 2026,
      week = 41,
    });

    var goal = store.RequiredEconomy().SavingsGoals.Single();
    await Assert.That(goal.Saved).IsEqualTo(53);
    await Assert.That(goal.LastInterestRate?.Value).IsEqualTo(5.33m);
    await Assert.That(goal.LastInterestPeriod).IsEqualTo(new IsoWeekPeriod(2026, 41));
    await Assert.That(store.LastRevisionGuards.Select(item => item.AggregateKind))
      .IsEquivalentTo([TypedAggregateKind.Family, TypedAggregateKind.FamilyProgression]);
  }

  [Test]
  public async Task RefundableDepositProvenanceSurvivesReloadAndCancellation()
  {
    var store = CreateStore(100);
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "refundable-goal",
      ownerId = _childId.Value,
      title = "Refundable goal",
      icon = "goal-icon",
      target = 100,
      visibility = "private",
    });
    await ExecuteAsync(handler, _childId, "savings.deposit", new
    {
      id = "refundable-goal",
      amount = 30,
    });

    var goal = store.RequiredEconomy().SavingsGoals.Single();
    await Assert.That(goal.RefundableContributions[_childId]).IsEqualTo(30);

    await ExecuteAsync(handler, _childId, "savings.cancel-goal", new
    {
      id = "refundable-goal",
    });

    var economy = store.RequiredEconomy();
    var refund = economy.Ledger.Single(item => item.Reason == "Goal cancellation refund");
    var forfeited = economy.Ledger.Single(item => item.Kind == LedgerKind.BonusForfeited);
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(100);
    await Assert.That(economy.SavingsGoals.Single().Cancelled).IsTrue();
    await Assert.That(store.EconomyPresentation.SavingGoals).IsEmpty();
    await Assert.That(refund.MemberId).IsEqualTo(_childId);
    await Assert.That(refund.Amount).IsEqualTo(30);
    await Assert.That(forfeited.Amount).IsEqualTo(5);
  }

  [Test]
  public async Task FamilyGoalCancellationRequestPersistsAndRequiresUnanimousApproval()
  {
    var store = CreateStore(100);
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _guardianId, "savings.add-goal", new
    {
      id = "family-goal",
      ownerId = "family",
      title = "Family goal",
      icon = "goal-icon",
      target = 100,
      visibility = "family",
      participantScope = "selected",
      participantIds = new[] { _childId.Value, _guardianId.Value },
    });
    await ExecuteAsync(handler, _childId, "savings.gift-to-goal", new
    {
      id = "family-goal",
      amount = 30,
    });

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "savings.cancel-goal", new
      {
        id = "family-goal",
      })
    ).Throws<DomainRuleException>();

    await ExecuteAsync(handler, _childId, "savings.request-goal-cancellation", new
    {
      id = "family-goal",
    });

    var requestedGoal = store.RequiredEconomy().SavingsGoals.Single();
    await Assert.That(requestedGoal.CancellationRequest?.RequestedByMemberId)
      .IsEqualTo(_childId);
    await Assert.That(requestedGoal.CancellationRequest?.ConsentedMemberIds)
      .IsEquivalentTo([_childId]);

    await ExecuteAsync(handler, _guardianId, "savings.decide-goal-cancellation", new
    {
      id = "family-goal",
      approved = false,
    });

    var rejectedGoal = store.RequiredEconomy().SavingsGoals.Single();
    await Assert.That(rejectedGoal.CancellationRequest).IsNull();
    await Assert.That(rejectedGoal.Saved).IsEqualTo(40);

    await ExecuteAsync(handler, _childId, "savings.request-goal-cancellation", new
    {
      id = "family-goal",
    });
    await ExecuteAsync(handler, _guardianId, "savings.decide-goal-cancellation", new
    {
      id = "family-goal",
      approved = true,
    });

    var economy = store.RequiredEconomy();
    var cancelledGoal = economy.SavingsGoals.Single();
    var refund = economy.Ledger.Single(item => item.Reason == "Goal cancellation refund");
    var forfeited = economy.Ledger.Single(item => item.Kind == LedgerKind.BonusForfeited);
    await Assert.That(cancelledGoal.Cancelled).IsTrue();
    await Assert.That(store.EconomyPresentation.SavingGoals).IsEmpty();
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(100);
    await Assert.That(refund.MemberId).IsEqualTo(_childId);
    await Assert.That(refund.Amount).IsEqualTo(30);
    await Assert.That(forfeited.Amount).IsEqualTo(10);
  }

  [Test]
  public async Task LedgerAndAppliedOperationsSurviveReloadAndRejectAReplayedCommand()
  {
    var store = CreateStore(100);
    var handler = store.CreateCommandHandler();
    var addGoalCommandId = Guid.Parse("d2993c54-2f08-46dc-97d4-2f37ce32d99c");
    var depositCommandId = Guid.Parse("a3285dcb-967b-40ca-a798-79182e8854a2");

    await ExecuteAsync(handler, _childId, "savings.add-goal", new
    {
      id = "durable-goal",
      ownerId = _childId.Value,
      title = "Durable goal",
      icon = "goal-icon",
      target = 100,
      visibility = "private",
    }, commandId: addGoalCommandId);
    await ExecuteAsync(handler, _childId, "savings.deposit", new
    {
      id = "durable-goal",
      amount = 10,
    }, commandId: depositCommandId);

    var economyAfterDeposit = store.RequiredEconomy();
    var entryIds = economyAfterDeposit.Ledger.Select(item => item.EntryId).ToArray();
    var operationIds = economyAfterDeposit.AppliedOperationIds.Select(item => item.Value).ToArray();
    await Assert.That(entryIds.Length).IsEqualTo(3);
    await Assert.That(entryIds.Distinct().Count()).IsEqualTo(3);
    await Assert.That(operationIds).IsEquivalentTo(
      [addGoalCommandId.ToString("N"), depositCommandId.ToString("N")]
    );

    await ExecuteAsync(handler, _childId, "savings.toggle-cheer", new
    {
      id = "durable-goal",
    });

    var reloadedEconomy = store.RequiredEconomy();
    await Assert.That(reloadedEconomy.Ledger.Select(item => item.EntryId))
      .IsEquivalentTo(entryIds);
    await Assert.That(reloadedEconomy.AppliedOperationIds.Select(item => item.Value))
      .IsEquivalentTo(operationIds);
    var revisionBeforeReplay = store.Revision(TypedAggregateKind.FamilyEconomy);

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "savings.deposit", new
      {
        id = "durable-goal",
        amount = 10,
      }, commandId: depositCommandId)
    ).Throws<DomainRuleException>();

    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy))
      .IsEqualTo(revisionBeforeReplay);
    await Assert.That(store.RequiredEconomy().Ledger.Count).IsEqualTo(3);
  }

  [Test]
  public async Task SupportedLedgerSourceIdentifiersRoundTripWithoutChangingTheirTypes()
  {
    var store = CreateLedgerSourceStore();
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _childId, "savings.toggle-cheer", new
    {
      id = "stored-goal",
    });

    var sourceTypes = store.RequiredEconomy().Ledger
      .ToDictionary(item => item.SourceId.Value, item => item.SourceId.GetType());
    await Assert.That(sourceTypes["daily-basic"]).IsEqualTo(typeof(ContributionId));
    await Assert.That(sourceTypes[_guardianId.Value]).IsEqualTo(typeof(FamilyMemberId));
    await Assert.That(sourceTypes["cozy-sofa"]).IsEqualTo(typeof(HouseContentId));
    await Assert.That(sourceTypes["request-1"]).IsEqualTo(typeof(RedemptionRequestId));
    await Assert.That(sourceTypes["stored-goal"]).IsEqualTo(typeof(SavingGoalId));
  }

  private static TestTypedAggregateStore CreateStore(decimal childBalance)
  {
    var family = CreateFamily();
    var economy = new FamilyEconomy(family);
    economy.RestoreWallet(_childId, childBalance, 0);

    return CreateStore(family, economy, FamilyEconomyPresentationState.Empty, FamilyHouse.CreateInitial(family));
  }

  private static TestTypedAggregateStore CreateStoreWithReservation(
    decimal childBalance,
    decimal reserved
  )
  {
    var family = CreateFamily();
    var economy = new FamilyEconomy(family);
    economy.RestoreWallet(_childId, childBalance, 0);
    var rewardId = new ShopRewardId("reserved-reward");
    economy.AddShopReward(
      _guardianId,
      rewardId,
      new ShopRewardDefinition(
        "Reserved reward",
        ShopRewardCategory.Gift,
        new LadirchenAmount(reserved),
        1,
        true,
        new AvailabilityWindow()
      ),
      _now
    );
    economy.RequestShopReward(
      _childId,
      rewardId,
      new RedemptionRequestId("reserved-request"),
      new TransactionId("reserved-operation"),
      _now
    );
    var presentation = new FamilyEconomyPresentationState(
      [],
      [],
      [new ShopRewardPresentationState(rewardId, "Description", "gift", "Conditions")],
      []
    );

    return CreateStore(family, economy, presentation, FamilyHouse.CreateInitial(family));
  }

  private static TestTypedAggregateStore CreateWeeklyInterestStore()
  {
    var family = CreateFamily();
    var economy = new FamilyEconomy(family);
    var goalId = new SavingGoalId("bicycle");
    economy.RestoreSavingGoal(
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Member(_childId),
        "Bicycle",
        new LadirchenAmount(100),
        GoalVisibility.Private,
        []
      ),
      new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.Zero),
      50,
      5,
      0,
      new Dictionary<FamilyMemberId, decimal> { [_childId] = 45 },
      null,
      [],
      false,
      false,
      null,
      null
    );
    var progression = FamilyHouse.CreateInitial(family);
    var period = new IsoWeekPeriod(2026, 41);
    var evaluation = new PersonalLadiEvaluation(
      _childId,
      period.Year,
      period.Week,
      new PersonalParticipation(0, 0, 0),
      new ParticipationValue(1, 1),
      LadiStage.AtPosition(4)
    );
    _ = progression.CloseSuccessfulWeek(
      period,
      HouseEnergyPolicy.MinimumSuccessfulWeekEnergy,
      [evaluation],
      _now
    );
    var presentation = new FamilyEconomyPresentationState(
      [],
      [new SavingGoalPresentationState(goalId, "bike-icon")],
      [],
      []
    );

    return CreateStore(family, economy, presentation, progression);
  }

  private static TestTypedAggregateStore CreateLedgerSourceStore()
  {
    var family = CreateFamily();
    var economy = new FamilyEconomy(family);
    economy.RestoreWallet(_childId, 20, 0);
    var goalId = new SavingGoalId("stored-goal");
    economy.RestoreSavingGoal(
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Member(_childId),
        "Stored goal",
        new LadirchenAmount(100),
        GoalVisibility.Private,
        []
      ),
      new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.Zero),
      5,
      5,
      0,
      new Dictionary<FamilyMemberId, decimal>(),
      null,
      [],
      false,
      false,
      null,
      null
    );
    DomainIdentifier[] sources =
    [
      new ContributionId("daily-basic"),
      _guardianId,
      new HouseContentId("cozy-sofa"),
      new RedemptionRequestId("request-1"),
      goalId,
    ];

    for (var index = 0; index < sources.Length; index++)
    {
      economy.RestoreLedgerEntry(new LedgerEntry(
        new TransactionId($"operation-{index + 1}"),
        _childId,
        null,
        LedgerKind.Credit,
        index + 1,
        sources[index],
        "Stored history",
        _now
      ));
    }

    var presentation = new FamilyEconomyPresentationState(
      [],
      [new SavingGoalPresentationState(goalId, "goal-icon")],
      [],
      []
    );

    return CreateStore(
      family,
      economy,
      presentation,
      FamilyHouse.CreateInitial(family)
    );
  }

  private static TestTypedAggregateStore CreateStore(
    Family family,
    FamilyEconomy economy,
    FamilyEconomyPresentationState presentation,
    FamilyHouse progression
  )
  {
    var store = new TestTypedAggregateStore(new FixedTimeProvider(_now));
    store.Seed(family, economy, presentation, progression);

    return store;
  }

  private static Family CreateFamily()
  {
    var family = new Family(_familyId, _guardianId, "Guardian", new IanaTimeZone("Europe/Zurich"));
    family.AddMember(
      _guardianId,
      _childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );

    return family;
  }

  private static async Task ExecuteAsync(
    ExecuteFamilyCommandHandler handler,
    FamilyMemberId actorId,
    string commandType,
    object payload,
    FamilyMemberId? authenticatedMemberId = null,
    Guid? commandId = null
  )
  {
    await handler.HandleAsync(
      new FamilyCommand(
        commandId ?? Guid.NewGuid(),
        _familyId,
        actorId,
        commandType,
        JsonSerializer.SerializeToElement(payload),
        authenticatedMemberId
      ),
      CancellationToken.None
    );
  }
}
