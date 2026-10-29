using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private readonly SavingGoalId _goalId = new("goal");

  [Given("the child has {float} Ladirchen")]
  public void FundChild(decimal amount)
  {
    _economy.GiveSpecialGift(_admin, _child, new(amount), "Birthday", NextOperation(), _now);
  }

  [When("the child creates a {string} goal targeting {float} Ladirchen")]
  [Given("the child creates a {string} goal targeting {float} Ladirchen")]
  public void CreateGoal(string visibility, decimal target)
  {
    _economy.AddGoal(
      _child,
      _goalId,
      new(GoalOwner.Member(_child), "Bicycle", new(target), Enum.Parse<GoalVisibility>(visibility)),
      NextOperation(),
      _now
    );
  }

  [When("the child deposits {float} Ladirchen")]
  [Given("the child deposits {float} Ladirchen")]
  public void Deposit(decimal amount)
  {
    _economy.DepositToGoal(_child, _goalId, new(amount), NextOperation(), _now);
  }

  [When("the child withdraws {float} Ladirchen")]
  public void Withdraw(decimal amount)
  {
    _economy.WithdrawFromGoal(_child, _goalId, new(amount), NextOperation(), _now);
  }

  [When("the child cancels the goal")]
  public void CancelGoal()
  {
    _economy.CancelGoal(_child, _goalId, NextOperation(), _now);
  }

  [When("the guardian administrator tries to support it")]
  public void AdminSupport()
  {
    Try(() => _economy.SupportGoal(_admin, _goalId, new(10), NextOperation(), _now));
  }

  [When("the supporter tries to support it")]
  public void SupporterSupport()
  {
    Try(() => _economy.SupportGoal(_supporter, _goalId, new(10), NextOperation(), _now));
  }

  [When("the child gifts {float} Ladirchen to the sibling")]
  public void GiftSibling(decimal amount)
  {
    _economy.GiftToMember(_child, _sibling, new(amount), NextOperation(), _now);
  }

  [When("the child tries to reduce the goal target to {float} Ladirchen")]
  public void ReduceTarget(decimal target)
  {
    Try(() => _economy.UpdateGoal(_child, _goalId, "Bicycle", new(target), GoalVisibility.Family, _now));
  }

  [Then("the goal contains {float} Ladirchen")]
  public async Task Saved(decimal amount)
  {
    await Assert.That(_economy.GoalFor(_child, _goalId).Saved).IsEqualTo(amount);
  }

  [Then("the goal allows withdrawing {float} Ladirchen")]
  public async Task Withdrawable(decimal amount)
  {
    await Assert.That(_economy.GoalFor(_child, _goalId).Withdrawable).IsEqualTo(amount);
  }

  [Then("the cancelled goal is no longer visible")]
  public async Task GoalRemoved()
  {
    await Assert.That(_economy.GoalsFor(_child).Count).IsEqualTo(0);
  }

  [Then("the ledger records {float} forfeited Ladirchen")]
  public async Task Forfeiture(decimal amount)
  {
    await Assert.That(_economy.Ledger.Single(item => item.Kind == LedgerKind.BonusForfeited).Amount).IsEqualTo(amount);
  }

  [Then("the sibling wallet contains {float} Ladirchen")]
  public async Task SiblingBalance(decimal amount)
  {
    await Assert.That(_economy.WalletFor(_sibling).Balance).IsEqualTo(amount);
  }

  [Then("the transfer ledger has matching debit and credit")]
  public async Task TransferEntries()
  {
    var transfer = _economy.DomainEvents.OfType<LadirchenTransferred>().Single();
    var entries = _economy.Ledger.Where(item => item.OperationId == transfer.OperationId).ToArray();
    await Assert.That(entries.Length).IsEqualTo(2);
    await Assert.That(entries.Single(item => item.Kind == LedgerKind.Debit).Amount).IsEqualTo(transfer.Amount);
    await Assert.That(entries.Single(item => item.Kind == LedgerKind.Credit).Amount).IsEqualTo(transfer.Amount);
  }
}
