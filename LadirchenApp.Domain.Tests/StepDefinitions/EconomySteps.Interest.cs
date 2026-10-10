using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private InterestEvaluationContext _evaluation = null!;
  private decimal _calculatedRate;
  private decimal _calculatedCredit;
  private string? _beforeRepeatedSettlement;

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
  public async Task Rate(decimal rate)
  {
    await Assert.That(_calculatedRate).IsEqualTo(rate);
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