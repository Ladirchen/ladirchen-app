using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Application.Queries;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class SettleAvailableWeeklyInterestCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("aa71a979-52c4-4963-99dc-0298d735fbe7"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly DateTimeOffset _now = new(2026, 10, 12, 10, 0, 0, TimeSpan.Zero);

  [Test]
  public async Task SettlesEachClosedWeekOnlyOnceAcrossRepeatedRuns()
  {
    var store = CreateStore(hasClosedWeek: true);
    var handler = CreateHandler(store);

    var first = await handler.HandleAsync(
      new SettleAvailableWeeklyInterestCommand(),
      CancellationToken.None
    );
    var revisionAfterFirstRun = store.Revision(TypedAggregateKind.FamilyEconomy);
    var second = await handler.HandleAsync(
      new SettleAvailableWeeklyInterestCommand(),
      CancellationToken.None
    );

    await Assert.That(first.AttemptedPeriods).IsEqualTo(1);
    await Assert.That(first.SettledPeriods).IsEqualTo(1);
    await Assert.That(first.DeferredPeriods).IsEqualTo(0);
    await Assert.That(first.FailedFamilies).IsEqualTo(0);
    await Assert.That(second.AttemptedPeriods).IsEqualTo(0);
    await Assert.That(second.SettledPeriods).IsEqualTo(0);
    await Assert.That(second.DeferredPeriods).IsEqualTo(0);
    await Assert.That(second.FailedFamilies).IsEqualTo(0);

    var economy = store.RequiredEconomy();
    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy))
      .IsEqualTo(revisionAfterFirstRun);
    await Assert.That(economy.SavingsGoals.Single().Saved).IsEqualTo(53m);
    await Assert.That(economy.SettledInterestPeriods.Select(period => period.Key))
      .IsEquivalentTo(["2026-W41"]);
  }

  [Test]
  public async Task DoesNothingWhenNoFamilyHasAnEligibleClosedWeek()
  {
    var store = CreateStore(hasClosedWeek: false);
    var handler = CreateHandler(store);

    var result = await handler.HandleAsync(
      new SettleAvailableWeeklyInterestCommand(),
      CancellationToken.None
    );

    await Assert.That(result.AttemptedPeriods).IsEqualTo(0);
    await Assert.That(result.SettledPeriods).IsEqualTo(0);
    await Assert.That(result.DeferredPeriods).IsEqualTo(0);
    await Assert.That(result.FailedFamilies).IsEqualTo(0);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy)).IsEqualTo(1);
  }

  private static SettleAvailableWeeklyInterestCommandHandler CreateHandler(
    TestTypedAggregateStore store
  )
  {
    var planner = new GetWeeklyInterestSettlementPlanQueryHandler(store, store, store);

    return new SettleAvailableWeeklyInterestCommandHandler(
      store,
      planner,
      store.CreateCommandHandler()
    );
  }

  private static TestTypedAggregateStore CreateStore(bool hasClosedWeek)
  {
    var family = new Family(
      _familyId,
      _guardianId,
      "Guardian",
      new IanaTimeZone("Europe/Zurich")
    );
    family.AddMember(
      _guardianId,
      _childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );
    var economy = new FamilyEconomy(family);
    economy.GiveSpecialGift(
      _guardianId,
      _childId,
      new LadirchenAmount(45),
      "Test balance",
      new TransactionId("initial-balance"),
      _now
    );
    var goalId = new SavingGoalId("bicycle");
    economy.AddGoal(
      _childId,
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Member(_childId),
        "Bicycle",
        new LadirchenAmount(100),
        GoalVisibility.Family
      ),
      new TransactionId("add-bicycle-goal"),
      _now.AddDays(-11)
    );
    economy.DepositToGoal(
      _childId,
      goalId,
      new LadirchenAmount(45),
      new TransactionId("deposit-bicycle-goal"),
      _now.AddDays(-11)
    );
    var progression = FamilyHouse.CreateInitial(family);

    if (hasClosedWeek)
    {
      var period = new IsoWeekPeriod(2026, 41);
      var participation = new PersonalParticipation(0, 60, 2);
      var policy = new LadiProgressionPolicy();
      var value = policy.Calculate(participation);
      var evaluation = new PersonalLadiEvaluation(
        _childId,
        period.Year,
        period.Week,
        participation,
        value,
        policy.DetermineStage(value.Capped)
      );
      progression.CloseSuccessfulWeek(
        period,
        HouseEnergyPolicy.MinimumSuccessfulWeekEnergy,
        [evaluation],
        _now
      );
    }

    var store = new TestTypedAggregateStore(new FixedTimeProvider(_now));
    store.Seed(
      family,
      economy,
      new FamilyEconomyPresentationState(
        [],
        [new SavingGoalPresentationState(goalId, "bicycle-icon")],
        [],
        []
      ),
      progression
    );

    return store;
  }
}
