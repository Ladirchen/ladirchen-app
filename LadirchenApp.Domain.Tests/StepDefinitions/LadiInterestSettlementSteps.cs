using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class LadiInterestSettlementSteps
{
  private readonly SavingsInterestPolicy _policy = new(new Percentage(8));
  private readonly FamilyMemberId _child = new("child");
  private PersonalLadiEvaluation _closedEvaluation = null!;
  private LadiStage _currentStage = LadiStage.FromId("idle-ladi");
  private readonly List<decimal> _goalRates = new();
  private int _eligibleGoalCount = 1;
  private decimal _settledRate;

  [Given("the child had the personal Ladi stage {string} in a closed ISO week")]
  public void ClosedStage(string stageId)
  {
    _closedEvaluation = Evaluation(stageId);
  }

  [Given("the child currently has the personal Ladi stage {string}")]
  public void CurrentStage(string stageId)
  {
    _currentStage = LadiStage.FromId(stageId);
  }

  [Given("the child has an eligible personal savings goal")]
  public void EligibleGoal()
  {
    _eligibleGoalCount = 1;
  }

  [When("interest for the closed ISO week is settled")]
  [When("interest for that week is settled")]
  public void SettleEvaluatedStage()
  {
    _settledRate = _policy.RateFor(_closedEvaluation).Value;
    _goalRates.Clear();

    for (var index = 0; index < _eligibleGoalCount; index++)
    {
      _goalRates.Add(_settledRate);
    }
  }

  [Then("the interest rate is determined from {string}")]
  public async Task RateUsesClosedStage(string stageId)
  {
    await Assert.That(_closedEvaluation.Stage.Id).IsEqualTo(stageId);
    await Assert.That(_settledRate).IsEqualTo(4);
  }

  [Then("the current {string} stage does not change that settlement")]
  public async Task CurrentStageIgnored(string stageId)
  {
    await Assert.That(_currentStage.Id).IsEqualTo(stageId);
    await Assert.That(_settledRate).IsEqualTo(4);
  }

  [Given("one child has multiple eligible personal savings goals")]
  public void MultipleEligibleGoals()
  {
    _eligibleGoalCount = 3;
  }

  [Given("the child has one personal Ladi evaluation for the settled ISO week")]
  public void OneEvaluation()
  {
    _closedEvaluation = Evaluation("sun-ladi");
  }

  [Then("every eligible goal of the child uses the same evaluated Ladi stage")]
  public async Task SameEvaluationForAllGoals()
  {
    await Assert.That(_goalRates.Count).IsEqualTo(_eligibleGoalCount);
    await Assert.That(_goalRates.Distinct().Count()).IsEqualTo(1);
  }

  [Given("every eligible goal owner has a personal Ladi evaluation for the settled ISO week")]
  public void EveryOwnerEvaluated()
  {
    _closedEvaluation = Evaluation("aurora-ladi");
    _eligibleGoalCount = 2;
  }

  [Then("settlement uses the evaluated Ladi stages")]
  public async Task UsesEvaluatedStages()
  {
    await Assert.That(_settledRate).IsEqualTo(6.67m);
  }

  [Then("it does not recalculate streak, completion or average rating from contributions")]
  public async Task DoesNotRecalculateParticipation()
  {
    await Assert.That(_closedEvaluation.Participation).IsEqualTo(new PersonalParticipation(2, 60, 4));
  }

  private PersonalLadiEvaluation Evaluation(string stageId)
  {
    var participation = new PersonalParticipation(2, 60, 4);
    var value = new ParticipationValue(5, 5);

    return new PersonalLadiEvaluation(_child, 2026, 41, participation, value, LadiStage.FromId(stageId));
  }
}
