using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.Economy;

public sealed class FamilyEconomyMembershipTests
{
  private static readonly FamilyId _familyId = new(
    Guid.Parse("bb0f1b51-8243-4485-a2f0-b9c2bb50ea12")
  );
  private static readonly FamilyMemberId _administratorId = new("administrator");
  private static readonly FamilyMemberId _departingChildId = new("departing-child");
  private static readonly FamilyMemberId _remainingChildId = new("remaining-child");
  private static readonly DateTimeOffset _now = DateTimeOffset.Parse("2026-10-15T10:00:00Z");

  [Test]
  public async Task SynchronizationAddsANewWalletAndRemovesAnEmptyDepartedWallet()
  {
    var family = EstablishedFamily();
    var economy = new FamilyEconomy(family);
    var newChildId = new FamilyMemberId("new-child");
    var nextMembers = family.CaptureState().Members
      .Where(item => item.Id != _departingChildId)
      .Append(ChildState(newChildId, "New Child"))
      .ToArray();
    family.CompleteSetup(_administratorId, nextMembers, [], _now);

    var synchronized = economy.SynchronizeMembers(family);

    await Assert.That(synchronized.Wallets.Select(item => item.Id)).DoesNotContain(_departingChildId);
    await Assert.That(synchronized.WalletFor(newChildId).Balance).IsEqualTo(0);
  }

  [Test]
  public async Task SynchronizationRejectsAMemberWithAnActiveSavingsGoal()
  {
    var family = EstablishedFamily();
    var economy = new FamilyEconomy(family);
    economy.AddGoal(
      _departingChildId,
      new SavingGoalId("active-goal"),
      new SavingGoalDefinition(
        GoalOwner.Member(_departingChildId),
        "Active goal",
        new LadirchenAmount(100),
        GoalVisibility.Private
      ),
      new TransactionId("create-active-goal"),
      _now
    );
    RemoveDepartingChild(family);

    await Assert.That(() => economy.SynchronizeMembers(family)).Throws<DomainRuleException>();
  }

  [Test]
  public async Task SynchronizationRejectsAMemberWithAnActiveContribution()
  {
    var family = EstablishedFamily();
    var economy = new FamilyEconomy(family);
    economy.AddContribution(
      _administratorId,
      new ContributionId("active-contribution"),
      new ContributionDefinition(
        "Open task",
        ContributionKind.Basic,
        new LadirchenAmount(0),
        new Percentage(20),
        _departingChildId
      ),
      _now
    );
    RemoveDepartingChild(family);

    await Assert.That(() => economy.SynchronizeMembers(family)).Throws<DomainRuleException>();
  }

  [Test]
  public async Task SynchronizationRetainsTerminalEconomyRecordsAsHistory()
  {
    var family = EstablishedFamily();
    var economy = new FamilyEconomy(family);
    var contributionId = new ContributionId("approved-history");
    var goalId = new SavingGoalId("redeemed-goal-history");
    var rewardId = new ShopRewardId("redeemed-reward-history");
    var requestId = new RedemptionRequestId("approved-request-history");
    economy.AddContribution(
      _administratorId,
      contributionId,
      new ContributionDefinition(
        "Completed task",
        ContributionKind.Basic,
        new LadirchenAmount(0),
        new Percentage(20),
        _departingChildId
      ),
      _now
    );
    economy.SubmitContribution(_departingChildId, contributionId, _now);
    economy.ApproveContribution(
      _administratorId,
      contributionId,
      new StarRating(3),
      new TransactionId("approve-history"),
      _now
    );
    economy.AddGoal(
      _departingChildId,
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Member(_departingChildId),
        "Completed goal",
        new LadirchenAmount(5),
        GoalVisibility.Private
      ),
      new TransactionId("create-completed-goal"),
      _now
    );
    economy.RedeemSavingsVoucher(_departingChildId, goalId, _now);
    economy.AddShopReward(
      _administratorId,
      rewardId,
      new ShopRewardDefinition(
        "Redeemed reward",
        ShopRewardCategory.Gift,
        new LadirchenAmount(10),
        1,
        true,
        new AvailabilityWindow()
      ),
      _now
    );
    economy.GiveSpecialGift(
      _administratorId,
      _departingChildId,
      new LadirchenAmount(10),
      "Reward funds",
      new TransactionId("fund-reward"),
      _now
    );
    economy.RequestShopReward(
      _departingChildId,
      rewardId,
      requestId,
      new TransactionId("request-reward"),
      _now
    );
    economy.ApproveShopReward(
      _administratorId,
      requestId,
      new TransactionId("approve-reward"),
      _now
    );
    RemoveDepartingChild(family);

    var synchronized = economy.SynchronizeMembers(family);
    var restoredHistory = synchronized.Contributions.Single(item => item.Id == contributionId);

    await Assert.That(restoredHistory.Status).IsEqualTo(ContributionStatus.Approved);
    await Assert.That(restoredHistory.AssigneeId).IsEqualTo(_departingChildId);
    await Assert.That(synchronized.SavingsVouchers.Single(item => item.Id == goalId).RedeemedAt).IsNotNull();
    await Assert.That(synchronized.RedemptionRequests.Single(item => item.Id == requestId).Status).IsEqualTo(RedemptionStatus.Approved);
    await Assert.That(synchronized.Wallets.Select(item => item.Id)).DoesNotContain(_departingChildId);
  }

  private static FamilyAggregate EstablishedFamily()
  {
    var family = new FamilyAggregate(
      _familyId,
      _administratorId,
      "Administrator",
      new IanaTimeZone("Europe/Zurich")
    );
    family.AddMember(
      _administratorId,
      _departingChildId,
      "Departing Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );
    family.AddMember(
      _administratorId,
      _remainingChildId,
      "Remaining Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );

    return family;
  }

  private static void RemoveDepartingChild(FamilyAggregate family)
  {
    var retainedMembers = family.CaptureState().Members
      .Where(item => item.Id != _departingChildId)
      .ToArray();
    family.CompleteSetup(_administratorId, retainedMembers, [], _now);
  }

  private static FamilyMemberState ChildState(FamilyMemberId id, string name)
  {
    return new FamilyMemberState(
      id,
      name,
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      null,
      null,
      true
    );
  }
}
