using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.Economy;

public sealed class FamilyEconomyGoalLifecycleTests
{
  private static readonly FamilyId _familyId = new(
    Guid.Parse("0d4bf4a6-1f48-4f12-894d-1505911f44c2")
  );
  private static readonly FamilyMemberId _administratorId = new("administrator");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly FamilyMemberId _siblingId = new("sibling");
  private static readonly DateTimeOffset _now = DateTimeOffset.Parse("2026-11-13T10:00:00Z");

  [Test]
  public async Task RedeemingAVoucherRemovesTheVoucherAndItsCompletedGoal()
  {
    var economy = CreateEconomy();
    var goalId = new SavingGoalId("completed-personal-goal");
    economy.AddGoal(
      _childId,
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Member(_childId),
        "Completed goal",
        new LadirchenAmount(5),
        GoalVisibility.Private
      ),
      new TransactionId("create-personal-goal"),
      _now
    );

    economy.RedeemSavingsVoucher(
      _childId,
      goalId,
      new TransactionId("redeem-personal-goal"),
      _now
    );

    await Assert.That(economy.SavingsVouchers).IsEmpty();
    await Assert.That(economy.SavingsGoals.Select(goal => goal.Id)).DoesNotContain(goalId);
  }

  [Test]
  public async Task AdministratorCanDissolveAFamilyGoalAwaitingVoucherConsentAndRefundContributors()
  {
    var economy = CreateEconomy();
    var goalId = new SavingGoalId("family-goal");
    GiveBalance(economy, _childId, "fund-child");
    GiveBalance(economy, _siblingId, "fund-sibling");
    economy.AddGoal(
      _administratorId,
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Family,
        "Family goal",
        new LadirchenAmount(20),
        GoalVisibility.Family,
        [_childId, _siblingId]
      ),
      new TransactionId("create-family-goal"),
      _now
    );
    economy.GiftToGoal(
      _childId,
      goalId,
      new LadirchenAmount(5),
      new TransactionId("child-contribution"),
      _now
    );
    economy.GiftToGoal(
      _siblingId,
      goalId,
      new LadirchenAmount(5),
      new TransactionId("sibling-contribution"),
      _now
    );

    await Assert.That(() => economy.DissolveGoal(
      _childId,
      goalId,
      new TransactionId("unauthorized-dissolution"),
      _now
    )).Throws<DomainRuleException>();

    var refunded = economy.DissolveGoal(
      _administratorId,
      goalId,
      new TransactionId("dissolve-family-goal"),
      _now
    );

    await Assert.That(refunded).IsEqualTo(10);
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(20);
    await Assert.That(economy.WalletFor(_siblingId).Balance).IsEqualTo(20);
    await Assert.That(economy.GoalsFor(_administratorId).Select(goal => goal.Id)).DoesNotContain(goalId);
    await Assert.That(economy.SavingsVouchers).IsEmpty();
  }

  [Test]
  public async Task PersonalVoucherCanBeConvertedBackToTheOwnersLadirchenBalance()
  {
    var economy = CreateEconomy();
    var goalId = new SavingGoalId("convertible-personal-goal");
    GiveBalance(economy, _childId, "fund-convertible-goal");
    economy.AddGoal(
      _childId,
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Member(_childId),
        "Convertible goal",
        new LadirchenAmount(10),
        GoalVisibility.Private
      ),
      new TransactionId("create-convertible-goal"),
      _now
    );
    economy.DepositToGoal(
      _childId,
      goalId,
      new LadirchenAmount(5),
      new TransactionId("complete-convertible-goal"),
      _now
    );

    var returned = economy.ConvertSavingsVoucherToLadirchen(
      _childId,
      goalId,
      new TransactionId("convert-personal-voucher"),
      _now
    );

    await Assert.That(returned).IsEqualTo(10);
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(25);
    await Assert.That(economy.SavingsVouchers).IsEmpty();
    await Assert.That(economy.SavingsGoals.Select(goal => goal.Id)).DoesNotContain(goalId);
  }

  [Test]
  public async Task FamilyVoucherConversionReturnsEachWalletFundedContributionAfterConsent()
  {
    var economy = CreateEconomy();
    var goalId = new SavingGoalId("convertible-family-goal");
    GiveBalance(economy, _childId, "fund-family-conversion-child");
    GiveBalance(economy, _siblingId, "fund-family-conversion-sibling");
    economy.AddGoal(
      _administratorId,
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Family,
        "Convertible family goal",
        new LadirchenAmount(20),
        GoalVisibility.Family,
        [_childId, _siblingId]
      ),
      new TransactionId("create-convertible-family-goal"),
      _now
    );
    economy.GiftToGoal(
      _childId,
      goalId,
      new LadirchenAmount(5),
      new TransactionId("convertible-child-contribution"),
      _now
    );
    economy.GiftToGoal(
      _siblingId,
      goalId,
      new LadirchenAmount(5),
      new TransactionId("convertible-sibling-contribution"),
      _now
    );
    _ = economy.ConsentSavingsVoucher(_childId, goalId, _now);
    _ = economy.ConsentSavingsVoucher(_siblingId, goalId, _now);

    var returned = economy.ConvertSavingsVoucherToLadirchen(
      _childId,
      goalId,
      new TransactionId("convert-family-voucher"),
      _now
    );

    await Assert.That(returned).IsEqualTo(10);
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(20);
    await Assert.That(economy.WalletFor(_siblingId).Balance).IsEqualTo(20);
    await Assert.That(economy.SavingsVouchers).IsEmpty();
    await Assert.That(economy.SavingsGoals.Select(goal => goal.Id)).DoesNotContain(goalId);
  }

  [Test]
  public async Task AdministratorCanDissolveAPrivateGoalWithoutExposingItForOtherActions()
  {
    var economy = CreateEconomy();
    var goalId = new SavingGoalId("private-child-goal");
    GiveBalance(economy, _childId, "fund-private-child-goal");
    economy.AddGoal(
      _childId,
      goalId,
      new SavingGoalDefinition(
        GoalOwner.Member(_childId),
        "Private child goal",
        new LadirchenAmount(100),
        GoalVisibility.Private
      ),
      new TransactionId("create-private-child-goal"),
      _now
    );
    economy.DepositToGoal(
      _childId,
      goalId,
      new LadirchenAmount(5),
      new TransactionId("deposit-private-child-goal"),
      _now
    );

    await Assert.That(economy.GoalsFor(_administratorId).Select(goal => goal.Id)).DoesNotContain(goalId);

    var refunded = economy.DissolveGoal(
      _administratorId,
      goalId,
      new TransactionId("dissolve-private-child-goal"),
      _now
    );

    await Assert.That(refunded).IsEqualTo(5);
    await Assert.That(economy.WalletFor(_childId).Balance).IsEqualTo(20);
  }

  private static FamilyEconomy CreateEconomy()
  {
    var family = new FamilyAggregate(
      _familyId,
      _administratorId,
      "Administrator",
      new IanaTimeZone("Europe/Zurich")
    );
    family.AddMember(
      _administratorId,
      _childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );
    family.AddMember(
      _administratorId,
      _siblingId,
      "Sibling",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );

    return new FamilyEconomy(family);
  }

  private static void GiveBalance(
    FamilyEconomy economy,
    FamilyMemberId memberId,
    string transactionId
  )
  {
    economy.GiveSpecialGift(
      _administratorId,
      memberId,
      new LadirchenAmount(20),
      "Goal balance",
      new TransactionId(transactionId),
      _now
    );
  }
}
