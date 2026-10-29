using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.Economy;

public sealed class FamilyEconomyStateTests
{
  [Test]
  public async Task ReconstitutionRoundTripsTheCompleteAuthoritativeStateWithoutDomainEvents()
  {
    var state = CompleteState();

    var economy = FamilyEconomy.Reconstitute(CompleteFamily(), state);
    var captured = economy.CaptureState();

    await Assert.That(economy.DomainEvents).IsEmpty();
    await Assert.That(JsonSerializer.Serialize(captured)).IsEqualTo(JsonSerializer.Serialize(state));
    await Assert.That(captured.SavingGoals.Single(item => item.Cancelled).InterestEarned).IsEqualTo(2);
    await Assert.That(captured.ShopRewards.Single(item => item.Deleted).Id.Value).IsEqualTo("reward-deleted");
    await Assert.That(captured.RedemptionRequests.Count(item => item.Status != RedemptionStatus.Pending)).IsEqualTo(2);
    await Assert.That(captured.AppliedOperationIds.Select(item => item.Value)).Contains("operation-zero");
  }

  [Test]
  public async Task ReconstitutionRejectsALedgerEntryWithoutItsAppliedOperation()
  {
    var state = CompleteState();
    var invalid = state with
    {
      AppliedOperationIds = [new TransactionId("operation-zero")],
    };

    await Assert.That(() => FamilyEconomy.Reconstitute(CompleteFamily(), invalid)).Throws<DomainRuleException>();
  }

  [Test]
  public async Task ReconstitutionRejectsAReservationThatDoesNotMatchPendingRequests()
  {
    var state = CompleteState();
    var invalid = state with
    {
      Wallets = state.Wallets
        .Select(item => item.MemberId.Value == "child" ? item with { Reserved = 0 } : item)
        .ToArray(),
    };

    await Assert.That(() => FamilyEconomy.Reconstitute(CompleteFamily(), invalid)).Throws<DomainRuleException>();
  }

  [Test]
  public async Task ReconstitutionUsesTheCurrentFamilyMembersAndTimezone()
  {
    var family = CompleteFamily();
    var administrator = new FamilyMemberId("administrator");
    var child = new FamilyMemberId("child");
    family.UpdateSettings(
      administrator,
      new IanaTimeZone("UTC"),
      family.Currency,
      family.ExchangeRate,
      DateTimeOffset.Parse("2026-10-15T10:00:00Z")
    );
    var state = EmptyState(family.Id, administrator, child, new LadirchenAmount(10));
    var newMember = new FamilyMemberId("new-member");
    family.AddMember(
      administrator,
      newMember,
      "New Member",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      DateTimeOffset.Parse("2026-10-15T11:00:00Z")
    );
    var economy = FamilyEconomy.Reconstitute(family, state);
    var now = DateTimeOffset.Parse("2026-10-15T17:30:00Z");
    var rewardId = new ShopRewardId("current-context-reward");

    economy.AddShopReward(
      administrator,
      rewardId,
      new ShopRewardDefinition(
        "Current context reward",
        ShopRewardCategory.Gift,
        new LadirchenAmount(1),
        1,
        true,
        new AvailabilityWindow()
      ),
      now
    );
    economy.RequestShopReward(
      child,
      rewardId,
      new RedemptionRequestId("current-context-request"),
      new TransactionId("current-context-operation"),
      now
    );

    await Assert.That(economy.RedemptionRequests).HasSingleItem();
    await Assert.That(economy.WalletFor(newMember).Balance).IsEqualTo(0);
  }

  private static FamilyEconomyState CompleteState()
  {
    var administrator = new FamilyMemberId("administrator");
    var child = new FamilyMemberId("child");
    var sibling = new FamilyMemberId("sibling");
    var approvedContributionId = new ContributionId("contribution-approved");
    var pendingContributionId = new ContributionId("contribution-pending");
    var personalGoalId = new SavingGoalId("goal-personal");
    var familyGoalId = new SavingGoalId("goal-family");
    var completedGoalId = new SavingGoalId("goal-completed");
    var cancelledGoalId = new SavingGoalId("goal-cancelled");
    var pendingRewardId = new ShopRewardId("reward-pending");
    var redeemedRewardId = new ShopRewardId("reward-redeemed");
    var deletedRewardId = new ShopRewardId("reward-deleted");
    var pendingRequestId = new RedemptionRequestId("request-pending");
    var approvedRequestId = new RedemptionRequestId("request-approved");
    var cancelledRequestId = new RedemptionRequestId("request-cancelled");
    var ledgerOperationId = new TransactionId("operation-ledger");
    var eventTime = DateTimeOffset.Parse("2026-10-15T10:00:00Z");

    return new FamilyEconomyState(
      new FamilyId(Guid.Parse("d6cac448-af40-47dd-bb1a-adc83acb202b")),
      new ContributionSettings(new Percentage(17)),
      [
        new MemberWalletState(administrator, 0, 0),
        new MemberWalletState(child, 100, 7),
        new MemberWalletState(sibling, 50, 0),
      ],
      [
        new ContributionState(
          approvedContributionId,
          "Approved contribution",
          ContributionKind.Basic,
          new LadirchenAmount(20),
          new Percentage(40),
          child,
          ContributionStatus.Approved,
          new StarRating(5),
          new ContributionRewardBreakdown(20, 3, 1),
          eventTime,
          []
        ),
        new ContributionState(
          pendingContributionId,
          "Pending team contribution",
          ContributionKind.Extra,
          new LadirchenAmount(8),
          new Percentage(20),
          child,
          ContributionStatus.Pending,
          null,
          null,
          null,
          [sibling]
        ),
      ],
      [
        approvedContributionId,
        new ContributionId("contribution-deleted"),
        pendingContributionId,
      ],
      [
        new PromotionState(
          new PromotionId("promotion-one"),
          pendingContributionId,
          new RewardMultiplier(2),
          new LadirchenAmount(3),
          new PromotionDeadline(new TimeOnly(18, 0)),
          true
        ),
      ],
      [ledgerOperationId, new TransactionId("operation-zero")],
      [
        Ledger(
          "1f1c3936-148d-41e5-b21e-2ea86ec93a01",
          ledgerOperationId,
          child,
          null,
          LedgerKind.Credit,
          20,
          LedgerSourceKind.Contribution,
          approvedContributionId.Value,
          "Contribution credit",
          eventTime,
          administrator
        ),
        Ledger(
          "1f1c3936-148d-41e5-b21e-2ea86ec93a02",
          ledgerOperationId,
          child,
          null,
          LedgerKind.Debit,
          2,
          LedgerSourceKind.FamilyMember,
          sibling.Value,
          "Family gift",
          eventTime,
          null
        ),
        Ledger(
          "1f1c3936-148d-41e5-b21e-2ea86ec93a03",
          ledgerOperationId,
          administrator,
          null,
          LedgerKind.Debit,
          5,
          LedgerSourceKind.HouseContent,
          "house-chair",
          "House purchase",
          eventTime,
          null
        ),
        Ledger(
          "1f1c3936-148d-41e5-b21e-2ea86ec93a04",
          ledgerOperationId,
          child,
          null,
          LedgerKind.Reserve,
          7,
          LedgerSourceKind.RedemptionRequest,
          pendingRequestId.Value,
          "Shop reservation",
          eventTime,
          null
        ),
        Ledger(
          "1f1c3936-148d-41e5-b21e-2ea86ec93a05",
          ledgerOperationId,
          child,
          personalGoalId,
          LedgerKind.GoalCredit,
          15,
          LedgerSourceKind.SavingGoal,
          personalGoalId.Value,
          "Savings deposit",
          eventTime,
          null
        ),
      ],
      [
        new SavingGoalState(
          personalGoalId,
          child,
          "Personal goal",
          100,
          GoalVisibility.Private,
          eventTime,
          [],
          22,
          5,
          2,
          [new RefundableContributionState(child, 15)],
          null,
          true,
          false,
          new IsoWeekPeriod(2026, 41),
          new Percentage(4)
        ),
        new SavingGoalState(
          familyGoalId,
          null,
          "Family goal",
          100,
          GoalVisibility.Family,
          eventTime,
          [child, sibling],
          20,
          10,
          0,
          [new RefundableContributionState(child, 10)],
          new GoalCancellationRequestState(child, [child]),
          false,
          false,
          null,
          null
        ),
        new SavingGoalState(
          completedGoalId,
          sibling,
          "Completed goal",
          5,
          GoalVisibility.Private,
          eventTime,
          [],
          5,
          5,
          0,
          [],
          null,
          false,
          false,
          null,
          null
        ),
        new SavingGoalState(
          cancelledGoalId,
          child,
          "Cancelled goal",
          100,
          GoalVisibility.Private,
          eventTime,
          [],
          0,
          5,
          2,
          [],
          null,
          false,
          true,
          new IsoWeekPeriod(2026, 40),
          new Percentage(3)
        ),
      ],
      [
        new SavingsVoucherState(completedGoalId, 5, sibling, [], [], eventTime),
      ],
      [
        new GuardianGoalGiftState(
          new GuardianGiftId("gift-identified"),
          child,
          administrator,
          3,
          personalGoalId,
          GiftDestination.Goal
        ),
        new GuardianGoalGiftState(
          null,
          child,
          administrator,
          2,
          personalGoalId,
          GiftDestination.Goal
        ),
      ],
      [new IsoWeekPeriod(2026, 40), new IsoWeekPeriod(2026, 41)],
      [
        new ShopRewardState(
          pendingRewardId,
          "Pending reward",
          ShopRewardCategory.Gift,
          new LadirchenAmount(7),
          2,
          true,
          new AvailabilityWindow(),
          ShopRewardStatus.Requested,
          false
        ),
        new ShopRewardState(
          redeemedRewardId,
          "Redeemed reward",
          ShopRewardCategory.Activity,
          new LadirchenAmount(4),
          0,
          true,
          new AvailabilityWindow(),
          ShopRewardStatus.Redeemed,
          false
        ),
        new ShopRewardState(
          deletedRewardId,
          "Deleted reward",
          ShopRewardCategory.Privilege,
          new LadirchenAmount(3),
          3,
          false,
          new AvailabilityWindow(),
          ShopRewardStatus.Available,
          true
        ),
      ],
      [
        new RedemptionRequestState(
          pendingRequestId,
          pendingRewardId,
          child,
          new LadirchenAmount(7),
          eventTime,
          RedemptionStatus.Pending,
          null
        ),
        new RedemptionRequestState(
          approvedRequestId,
          redeemedRewardId,
          child,
          new LadirchenAmount(4),
          eventTime.AddDays(-2),
          RedemptionStatus.Approved,
          eventTime.AddDays(-1)
        ),
        new RedemptionRequestState(
          cancelledRequestId,
          deletedRewardId,
          sibling,
          new LadirchenAmount(2),
          eventTime.AddDays(-4),
          RedemptionStatus.Cancelled,
          eventTime.AddDays(-3)
        ),
      ]
    );
  }

  private static FamilyAggregate CompleteFamily()
  {
    var administrator = new FamilyMemberId("administrator");
    var family = new FamilyAggregate(
      new FamilyId(Guid.Parse("d6cac448-af40-47dd-bb1a-adc83acb202b")),
      administrator,
      "Administrator",
      new IanaTimeZone("Europe/Zurich")
    );
    var now = DateTimeOffset.Parse("2026-10-15T10:00:00Z");
    family.AddMember(
      administrator,
      new FamilyMemberId("child"),
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      now
    );
    family.AddMember(
      administrator,
      new FamilyMemberId("sibling"),
      "Sibling",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      now
    );
    family.AddMember(
      administrator,
      new FamilyMemberId("invited-guardian"),
      "Invited Guardian",
      MemberRole.Guardian,
      GuardianAccess.Supporter,
      true,
      now
    );

    return family;
  }

  private static FamilyEconomyState EmptyState(
    FamilyId familyId,
    FamilyMemberId administrator,
    FamilyMemberId child,
    LadirchenAmount childBalance
  )
  {
    return new FamilyEconomyState(
      familyId,
      ContributionSettings.Default,
      [
        new MemberWalletState(administrator, 0, 0),
        new MemberWalletState(child, childBalance.Value, 0),
        new MemberWalletState(new FamilyMemberId("sibling"), 0, 0),
      ],
      [],
      [],
      [],
      [],
      [],
      [],
      [],
      [],
      [],
      [],
      []
    );
  }

  private static LedgerEntryState Ledger(
    string entryId,
    TransactionId operationId,
    FamilyMemberId memberId,
    SavingGoalId? goalId,
    LedgerKind kind,
    decimal amount,
    LedgerSourceKind sourceKind,
    string sourceId,
    string reason,
    DateTimeOffset occurredAtUtc,
    FamilyMemberId? initiatorId
  )
  {
    return new LedgerEntryState(
      Guid.Parse(entryId),
      operationId,
      memberId,
      goalId,
      kind,
      amount,
      new LedgerSourceState(sourceKind, sourceId),
      reason,
      occurredAtUtc,
      initiatorId
    );
  }
}
