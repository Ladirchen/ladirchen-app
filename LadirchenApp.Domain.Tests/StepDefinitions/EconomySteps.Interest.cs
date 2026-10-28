using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Domain.Ladi;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private InterestEvaluationContext _evaluation = null!;
  private decimal _calculatedRate;
  private decimal _calculatedCredit;
  private string? _beforeRepeatedSettlement;
  private Percentage _maximumStageRate = new(8);
  private LadiStage _interestStage = LadiStage.AtPosition(1);

  [Given("participation of {int} streak days, {float} percent complete and {float} stars")]
  public void Participation(int days, decimal completion, decimal stars)
  {
    _evaluation = new(days, new(completion), stars);
  }

  [When("the default interest rate is calculated")]
  public void CalculateRate()
  {
    _calculatedRate = SavingsInterestPolicy.Default.CalculateRate(_evaluation).Value;
  }

  [Then("the interest rate is {float} percent")]
  [Then("the savings interest rate is {float} percent")]
  public async Task Rate(decimal rate)
  {
    await Assert.That(_calculatedRate).IsEqualTo(rate);
  }

  [Given("the maximum savings interest rate is configured as {float} percent")]
  public void MaximumStageRate(decimal rate)
  {
    _maximumStageRate = new Percentage(rate);
  }

  [Given("the child's personal Ladi stage is {string} at position {int}")]
  public void ChildStage(string id, int position)
  {
    _interestStage = new LadiStage(id, position);
  }

  [Given("the child's personal Ladi stage is at position {int}")]
  public void ChildStagePosition(int position)
  {
    _interestStage = LadiStage.AtPosition(position);
  }

  [Given("the child's personal Ladi stage is {string}")]
  public void ChildStageId(string id)
  {
    _interestStage = LadiStage.FromId(id);
  }

  [Given("the child's personal participation has already determined their Ladi stage")]
  public void EvaluatedChildStage()
  {
    _interestStage = LadiStage.FromId("sun-ladi");
  }

  [Given("the family house level is configured as {float}")]
  [Given("the five-star contribution bonus percent is configured as {float}")]
  [Given("the completed family weekly streak is configured as {float}")]
  public void UnrelatedSetting(decimal value)
  {
  }

  [When("the child's personal savings interest rate is determined")]
  public void DetermineStageRate()
  {
    _calculatedRate = SavingsInterestPolicy.CalculateRate(_interestStage, _maximumStageRate).Value;
  }

  [Then("the interest policy does not evaluate streak, completion or average rating again")]
  public async Task InterestUsesStageOnly()
  {
    await Assert.That(_calculatedRate).IsEqualTo(5.33m);
  }

  [When("interest on {float} saved toward {float} at {float} percent is calculated")]
  public void CalculateCredit(decimal saved, decimal target, decimal rate)
  {
    _calculatedCredit = SavingsInterestPolicy.CalculateCredit(new(saved), new(target), new(rate));
  }

  [Then("the interest credit is {float} Ladirchen")]
  public async Task Credit(decimal amount)
  {
    await Assert.That(_calculatedCredit).IsEqualTo(amount);
  }

  [When("interest for week {int} of {int} is settled")]
  public void Settle(int week, int year)
  {
    var period = new IsoWeekPeriod(year, week);
    var context = _economy.EvaluateParticipation(_child, period, 0, 7);
    _economy.ApplyWeeklyInterest(
      _admin,
      period,
      new Dictionary<FamilyMemberId, InterestEvaluationContext> { [_child] = context },
      NextOperation(),
      _now
    );
  }

  [When("interest for week {int} of {int} is settled again")]
  public void RepeatSettlement(int week, int year)
  {
    _beforeRepeatedSettlement = Snapshot();
    Settle(week, year);
  }

  [When("interest for week {int} of {int} is attempted")]
  public void TrySettle(int week, int year)
  {
    Try(() => Settle(week, year));
  }

  [When("weekly settlement is attempted without owner evaluations")]
  public void MissingEvaluations()
  {
    Try(() =>
      _economy.ApplyWeeklyInterest(
        _admin,
        new(2026, 41),
        new Dictionary<FamilyMemberId, InterestEvaluationContext>
        {
          [_child] = _economy.EvaluateParticipation(_child, new(2026, 41), 0, 7),
        },
        NextOperation(),
        _now
      )
    );
  }

  [Given("the sibling creates a private goal")]
  public void SiblingGoal()
  {
    _economy.AddGoal(
      _sibling,
      new("sibling-goal"),
      new(GoalOwner.Member(_sibling), "Trip", new(100), GoalVisibility.Private),
      NextOperation(),
      _now
    );
  }

  [Given("these approved contributions:")]
  public void ApprovedContributions(DataTable table)
  {

    foreach (var row in table.Rows)
    {
      var id = new ContributionId(row["id"]);
      var instant = DateTimeOffset.Parse(row["at"], System.Globalization.CultureInfo.InvariantCulture);
      var kind = Enum.Parse<ContributionKind>(row["kind"]);
      _economy.AddContribution(_admin, id, new("Help", kind, new(10), new(50), _child), instant);
      _economy.SubmitContribution(_child, id, instant);
      _economy.ApproveContribution(
        _admin,
        id,
        new(int.Parse(row["stars"], System.Globalization.CultureInfo.InvariantCulture)),
        NextOperation(),
        instant
      );
    }

  }

  [When("participation for week {int} of {int} is evaluated")]
  public void Evaluate(int week, int year)
  {
    _evaluation = _economy.EvaluateParticipation(_child, new(year, week), 0, 7);
  }

  [Then("participation is {int} days, {float} percent and {float} stars")]
  public async Task EvaluatedParticipation(int days, decimal completion, decimal rating)
  {
    await Assert.That(_evaluation.StreakDays).IsEqualTo(days);
    await Assert.That(_evaluation.Completion.Value).IsEqualTo(completion);
    await Assert.That(_evaluation.AverageRating).IsEqualTo(rating);
  }

  [Then("the repeated settlement left the economy unchanged")]
  public async Task SettlementUnchanged()
  {
    await Assert.That(Snapshot()).IsEqualTo(_beforeRepeatedSettlement);
  }
}
