using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Tests.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Application.Queries;

public sealed class SavingsQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(
    Guid.Parse("9f210fed-fbc0-40a7-9764-ceaefc304485")
  );
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly DateTimeOffset _now = new(2026, 10, 29, 10, 0, 0, TimeSpan.Zero);

  [Test]
  public async Task SavingsReadModelUsesTypedDomainStateAndLadiStage()
  {
    var store = CreateStore();
    var result = await QueryAsync(store, _childId);
    var personalGoal = result.Economy.SavingsGoals.Single(goal => goal.Id.Value == "bicycle");
    var cancellingGoal = result.Economy.SavingsGoals.Single(
      goal => goal.Id.Value == "family-cancellation"
    );
    var personal = result.CalculatedState.Goals[personalGoal.Id];
    var cancellation = result.CalculatedState.Goals[cancellingGoal.Id];
    var voucher = result.CalculatedState.Vouchers[new SavingGoalId("family-trip")];

    await Assert.That(result.CalculatedState.AvailableBalances[_childId]).IsEqualTo(20);
    await Assert.That(personal.ProjectedInterestCredit).IsEqualTo(3);
    await Assert.That(personal.RemainingAmount).IsEqualTo(50);
    await Assert.That(personal.WithdrawableBalance).IsEqualTo(45);
    await Assert.That(personal.VisibleToMemberIds).IsEquivalentTo([_childId]);
    await Assert.That(personal.CommandLimits[_childId].MaximumDepositAmount).IsEqualTo(20);
    await Assert.That(personal.CommandLimits.ContainsKey(_guardianId)).IsFalse();
    await Assert.That(result.CalculatedState.Balances.ContainsKey(_guardianId)).IsFalse();
    await Assert.That(result.CalculatedState.Goals.Keys.Any(id => id.Value == "guardian-private"))
      .IsFalse();
    await Assert.That(result.CalculatedState.SharedSavedBalances[_childId]).IsEqualTo(50);
    await Assert.That(voucher.ConsentableByMemberIds).IsEmpty();
    await Assert.That(voucher.RedeemableByMemberIds).IsEmpty();
    await Assert.That(cancellation.CancellationRequest).IsNotNull();
    await Assert.That(cancellation.CancellationRequest!.RequestedByMemberId).IsEqualTo(_childId);
    await Assert.That(cancellation.CancellationRequest.ConsentedMemberIds)
      .IsEquivalentTo([_childId]);
    await Assert.That(cancellation.CancellationRequest.PendingMemberIds)
      .IsEquivalentTo([_guardianId]);
    await Assert.That(cancellation.CommandLimits[_childId].CanRequestCancellation).IsFalse();
    await Assert.That(cancellation.CommandLimits[_childId].CanApproveCancellation).IsFalse();
    await Assert.That(cancellation.CommandLimits[_childId].CanRejectCancellation).IsFalse();
  }

  [Test]
  public async Task GuardianAdministratorCannotReadChildPrivateGoalThroughViewerSelection()
  {
    var store = CreateStore();
    var handler = CreateHandler(store);
    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "savings", _guardianId, _childId),
      CancellationToken.None
    ) as SavingsFamilyReadModelProjection
      ?? throw new InvalidOperationException("The savings read model is missing.");

    await Assert.That(result.CalculatedState.Goals.Keys.Any(id => id.Value == "child-private"))
      .IsFalse();
    await Assert.That(result.CalculatedState.Goals.Keys.Any(id => id.Value == "guardian-private"))
      .IsTrue();
    var familyGoal = result.CalculatedState.Goals[new SavingGoalId("family-cancellation")];
    var limits = familyGoal.CommandLimits[_guardianId];
    await Assert.That(limits.CanRequestCancellation).IsFalse();
    await Assert.That(limits.CanApproveCancellation).IsTrue();
    await Assert.That(limits.CanRejectCancellation).IsTrue();
  }

  [Test]
  public async Task ChildCannotSelectGuardianAsReadModelViewer()
  {
    var handler = CreateHandler(CreateStore());

    await Assert.That(async () =>
      await handler.HandleAsync(
        new GetFamilyAggregateQuery(_familyId, "savings", _childId, _guardianId),
        CancellationToken.None
      )
    ).Throws<DomainRuleException>();
  }

  [Test]
  public async Task MissingProjectionDependencyNeverReturnsPartialSavingsState()
  {
    var complete = CreateStore();
    var family = complete.RequiredFamily();
    var incomplete = new TestTypedAggregateStore(new FixedTimeProvider(_now));
    incomplete.Seed(
      family,
      complete.RequiredEconomy(),
      complete.EconomyPresentation
    );
    var handler = CreateHandler(incomplete);

    await Assert.That(async () =>
      await handler.HandleAsync(
        new GetFamilyAggregateQuery(_familyId, "savings", _childId),
        CancellationToken.None
      )
    ).Throws<InvalidOperationException>();
  }

  private static async Task<SavingsFamilyReadModelProjection> QueryAsync(
    TestTypedAggregateStore store,
    FamilyMemberId actorId
  )
  {
    return await CreateHandler(store).HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "savings", actorId),
      CancellationToken.None
    ) as SavingsFamilyReadModelProjection
      ?? throw new InvalidOperationException("The savings read model is missing.");
  }

  private static GetFamilyAggregateQueryHandler CreateHandler(TestTypedAggregateStore store)
  {
    return new GetFamilyAggregateQueryHandler(
      store,
      store,
      store,
      store,
      new FixedTimeProvider(_now)
    );
  }

  private static TestTypedAggregateStore CreateStore()
  {
    var family = Family.Reconstitute(new FamilyState(
      _familyId,
      new IanaTimeZone("Europe/Zurich"),
      FamilyCurrency.CHF,
      new ExchangeRate(10),
      _childId,
      false,
      [
        Member(_guardianId, "Guardian", MemberRole.Guardian, GuardianAccess.Admin),
        Member(_childId, "Child", MemberRole.Child, GuardianAccess.Supporter),
      ],
      []
    ));
    var contributionId = new ContributionId("completed-basic");
    var goals = Goals();
    var rewardId = new ShopRewardId("reserved-reward");
    var economy = FamilyEconomy.Reconstitute(family, new FamilyEconomyState(
      _familyId,
      ContributionSettings.Default,
      [
        new MemberWalletState(_guardianId, 0, 0),
        new MemberWalletState(_childId, 100, 80),
      ],
      [
        new ContributionState(
          contributionId,
          "Completed basic",
          ContributionKind.Basic,
          new LadirchenAmount(1),
          new Percentage(10),
          _childId,
          ContributionStatus.Approved,
          new StarRating(1),
          new ContributionRewardBreakdown(1, 0, 1),
          new DateTimeOffset(2026, 9, 1, 10, 0, 0, TimeSpan.Zero),
          []
        ),
      ],
      [contributionId],
      [],
      [],
      [],
      goals,
      [
        new SavingsVoucherState(
          new SavingGoalId("family-trip"),
          20,
          null,
          [_childId, _guardianId],
          [_childId],
          null
        ),
        new SavingsVoucherState(
          new SavingGoalId("guardian-private"),
          5,
          _guardianId,
          [],
          [],
          null
        ),
      ],
      [],
      [],
      [
        new ShopRewardState(
          rewardId,
          "Reserved reward",
          ShopRewardCategory.Gift,
          new LadirchenAmount(80),
          1,
          true,
          new AvailabilityWindow(null, null),
          ShopRewardStatus.Requested,
          false
        ),
      ],
      [
        new RedemptionRequestState(
          new RedemptionRequestId("reserved-request"),
          rewardId,
          _childId,
          new LadirchenAmount(80),
          _now.AddDays(-1),
          RedemptionStatus.Pending,
          null
        ),
      ]
    ));
    var presentation = new FamilyEconomyPresentationState(
      [new ContributionPresentationState(contributionId, "", "", false)],
      goals.Select(goal => new SavingGoalPresentationState(goal.Id, $"{goal.Id.Value}-icon")),
      [new ShopRewardPresentationState(rewardId, "", "", "")],
      []
    );
    var progression = FamilyHouse.Reconstitute(
      family,
      new FamilyHouseState(
        _familyId,
        0,
        0,
        7,
        new Percentage(8),
        [new ParticipantProgressState(_childId, 0)],
        []
      )
    );
    var store = new TestTypedAggregateStore(new FixedTimeProvider(_now));
    store.Seed(family, economy, presentation, progression);

    return store;
  }

  private static SavingGoalState[] Goals()
  {
    var createdAt = new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

    return
    [
      Goal("bicycle", _childId, "Bicycle", 100, 50, GoalVisibility.Family, createdAt,
        refundable: [new RefundableContributionState(_childId, 45)]),
      Goal("child-private", _childId, "Private child goal", 10, 5, GoalVisibility.Private, createdAt),
      Goal("guardian-private", _guardianId, "Private guardian goal", 5, 5, GoalVisibility.Private, createdAt),
      Goal(
        "family-cancellation",
        null,
        "Family cancellation",
        100,
        10,
        GoalVisibility.Family,
        createdAt,
        [_childId, _guardianId],
        new GoalCancellationRequestState(_childId, [_childId])
      ),
      Goal(
        "family-trip",
        null,
        "Family trip",
        20,
        20,
        GoalVisibility.Family,
        createdAt,
        [_childId, _guardianId]
      ),
    ];
  }

  private static SavingGoalState Goal(
    string id,
    FamilyMemberId? ownerId,
    string title,
    decimal target,
    decimal saved,
    GoalVisibility visibility,
    DateTimeOffset createdAt,
    FamilyMemberId[]? participants = null,
    GoalCancellationRequestState? cancellation = null,
    RefundableContributionState[]? refundable = null
  )
  {
    var participantIds = participants ?? [];
    var starterBonus = ownerId is null ? participantIds.Length * 5 : 5;

    return new SavingGoalState(
      new SavingGoalId(id),
      ownerId,
      title,
      target,
      visibility,
      createdAt,
      participantIds,
      saved,
      starterBonus,
      0,
      refundable ?? [],
      cancellation,
      false,
      false,
      null,
      null
    );
  }

  private static FamilyMemberState Member(
    FamilyMemberId id,
    string name,
    MemberRole role,
    GuardianAccess access
  )
  {
    return new FamilyMemberState(
      id,
      name,
      role,
      access,
      false,
      null,
      null,
      role == MemberRole.Child
    );
  }
}
