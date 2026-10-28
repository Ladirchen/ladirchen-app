using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private string _eventAction = string.Empty;
  private string _eventSubject = string.Empty;

  [Given("a contribution is in the state required for {string}")]
  public void ContributionEventState(string action)
  {
    CreateEconomy();
    _eventAction = action;
    _eventSubject = "contribution";

    if (action != "create")
    {
      AddAvailableContribution(_child, ContributionKind.Basic, 10);
    }

    if (action is "return" or "approve")
    {
      _economy.SubmitContribution(_child, _contributionId, _now);
    }

    _economy.ClearDomainEvents();
  }

  [Given("a saving goal is in the state required for {string}")]
  public void GoalEventState(string action)
  {
    CreateEconomy();
    _eventAction = action;
    _eventSubject = "goal";

    if (action != "create")
    {
      FundChild(100);
      var target = action == "complete" ? 6 : 100;
      CreateGoal("Private", target);

      if (action is "withdraw" or "cancel")
      {
        Deposit(10);
      }
    }

    _economy.ClearDomainEvents();
  }

  [Given("a shop reward is in the state required for {string}")]
  public void ShopEventState(string action)
  {
    CreateEconomy();
    _eventAction = action;
    _eventSubject = "shop";

    if (action != "add")
    {
      FundChild(100);
      CreateReward(20, 2);
    }

    if (action is "approve" or "reject" or "cancel")
    {
      RequestReward();
    }

    _economy.ClearDomainEvents();
  }

  [When("an authorized member performs {string}")]
  public void PerformEventAction(string action)
  {
    if (_eventAction != action)
    {
      throw new InvalidOperationException("The prepared event action does not match the performed action.");
    }

    var performed = _eventSubject switch
    {
      "contribution" => PerformContributionEventAction(action),
      "goal" => PerformGoalEventAction(action),
      "shop" => PerformShopEventAction(action),
      _ => false,
    };

    if (performed)
    {
      return;
    }

    throw new InvalidOperationException("Unknown event action.");
  }

  [Then("the economy publishes {string}")]
  public async Task EconomyPublishes(string eventName)
  {
    await Assert.That(_economy.DomainEvents.Any(item => item.GetType().Name == eventName)).IsTrue();
  }

  [Given("an accepted operation creates monetary ledger entries")]
  public void AcceptedMonetaryOperation()
  {
    CreateEconomy();
    _economy.ClearDomainEvents();
    FundChild(25);
  }

  [When("the operation is recorded")]
  public void OperationRecorded()
  {
  }

  [Then("each ledger entry publishes a {string} event")]
  public async Task EachLedgerEntryPublishes(string eventName)
  {
    var eventCount = _economy.DomainEvents.Count(item => item.GetType().Name == eventName);
    await Assert.That(eventCount).IsEqualTo(_economy.Ledger.Count);
  }

  [Then("every entry contains the operation, member, source, amount, reason and UTC occurrence time")]
  public async Task CompleteLedgerEntries()
  {
    var complete = _economy.Ledger.All(item =>
      item.OperationId is not null
      && item.MemberId is not null
      && item.SourceId is not null
      && item.Amount > 0
      && !string.IsNullOrWhiteSpace(item.Reason)
      && item.OccurredAtUtc.Offset == TimeSpan.Zero
    );
    await Assert.That(complete).IsTrue();
  }

  [Given("an eligible personal saving goal receives weekly interest")]
  public void EligibleGoalForInterestEvent()
  {
    CreateEconomy();
    LocalTime("2026-10-01T10:00:00");
    CreateGoal("Private", 100);
    LocalTime("2026-10-12T10:00:00");
    _economy.ClearDomainEvents();
  }

  [When("the closed week is settled")]
  public void SettleClosedWeek()
  {
    Settle(41, 2026);
  }

  [Then("the economy publishes a {string} event for the goal")]
  public async Task GoalInterestEvent(string eventName)
  {
    await EconomyPublishes(eventName);
  }

  [Then("the economy publishes one {string} event for the settlement")]
  public async Task WeeklyInterestEvent(string eventName)
  {
    await Assert.That(_economy.DomainEvents.Count(item => item.GetType().Name == eventName)).IsEqualTo(1);
  }

  [Given("an economy command violates a domain rule")]
  public void ViolatingCommand()
  {
    CreateEconomy();
    _economy.ClearDomainEvents();
  }

  [When("the command is rejected")]
  public void RejectCommand()
  {
    Try(() => _economy.GiveSpecialGift(_supporter, _child, new LadirchenAmount(10), "Invalid", NextOperation(), _now));
  }

  [Then("no ledger entries are recorded")]
  public async Task NoLedgerEntries()
  {
    await Assert.That(_economy.Ledger).IsEmpty();
  }

  [Then("no domain events are published")]
  public async Task NoDomainEvents()
  {
    await Assert.That(_economy.DomainEvents).IsEmpty();
  }

  private bool PerformContributionEventAction(string action)
  {
    switch (action)
    {
      case "create" when _economy.Contributions.Count == 0:
        AddAvailableContribution(_child, ContributionKind.Basic, 10);
        return true;
      case "submit" when _economy.Contributions.Count > 0:
        _economy.SubmitContribution(_child, _contributionId, _now);
        return true;
      case "return" when _economy.Contributions.Count > 0:
        _economy.ReturnContribution(_admin, _contributionId, _now);
        return true;
      case "approve" when _economy.Contributions.Count > 0:
        _economy.ApproveContribution(_admin, _contributionId, new StarRating(5), NextOperation(), _now);
        return true;
      case "assign" when _economy.Contributions.Count > 0:
        _economy.AssignContribution(_admin, _contributionId, _sibling, _now);
        return true;
      case "delete" when _economy.Contributions.Count > 0:
        _economy.DeleteContribution(_admin, _contributionId, _now);
        return true;
      default:
        return false;
    }
  }

  private bool PerformGoalEventAction(string action)
  {
    if (_economy.GoalsFor(_child).Count == 0 && action != "create")
    {
      return false;
    }

    switch (action)
    {
      case "create":
        CreateGoal("Private", 100);
        return true;
      case "update":
        _economy.UpdateGoal(_child, _goalId, "Changed", new LadirchenAmount(120), GoalVisibility.Private, _now);
        return true;
      case "deposit":
        Deposit(10);
        return true;
      case "withdraw":
        Withdraw(10);
        return true;
      case "cancel":
        CancelGoal();
        return true;
      case "complete":
        Deposit(1);
        return true;
      default:
        return false;
    }
  }

  private bool PerformShopEventAction(string action)
  {
    switch (action)
    {
      case "add" when _economy.ShopRewards.Count == 0:
        CreateReward(20, 2);
        return true;
      case "update" when _economy.ShopRewards.Count > 0:
        _economy.UpdateShopReward(_admin, _rewardId, "Changed", new LadirchenAmount(25), 2, new AvailabilityWindow(), _now);
        return true;
      case "hide" when _economy.ShopRewards.Count > 0:
        _economy.SetShopRewardVisibility(_admin, _rewardId, false, _now);
        return true;
      case "request" when _economy.ShopRewards.Count > 0:
        RequestReward();
        return true;
      case "approve" when _economy.ShopRewards.Count > 0:
        ApproveReward();
        return true;
      case "reject" when _economy.ShopRewards.Count > 0:
        _economy.RejectShopReward(_admin, _requestId, NextOperation(), _now);
        return true;
      case "cancel" when _economy.ShopRewards.Count > 0:
        _economy.CancelShopRewardRequest(_child, _requestId, NextOperation(), _now);
        return true;
      default:
        return false;
    }
  }
}
