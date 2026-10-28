using System;
using System.Threading.Tasks;

using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class LadiSteps
{
  private readonly ScenarioContext _scenarioContext;
  private readonly LadiProgressionPolicy _policy = new();
  private readonly HouseEnergyPolicy _energyPolicy = new();
  private readonly FamilyMemberId _memberId = new("member");
  private PersonalParticipation _participation = new(0, 0, 0);
  private ParticipationValue _value = new(1, 1);
  private LadiStage _stage = LadiStage.AtPosition(1);
  private PersonalLadiEvaluation? _closedEvaluation;
  private PersonalLadiEvaluation? _evaluation;
  private decimal _maximum = LadiProgressionPolicy.DefaultMaximumParticipation;
  private decimal _assignedEnergy;
  private decimal _approvedBasicEnergy;
  private decimal _approvedExtraEnergy;
  private decimal _effectiveCompletion;
  private LadiStage? _secondStage;
  private decimal _interestStagePosition;
  private decimal _energyStagePosition;

  public LadiSteps(ScenarioContext scenarioContext)
  {
    _scenarioContext = scenarioContext;
  }

  [Given("a family member's personal streak, completion and average basic rating for an ISO week")]
  public void WeeklyParticipation()
  {
    _participation = new PersonalParticipation(3, 70, 4);
  }

  [When("the personal Ladi evaluation is created")]
  public void CreateEvaluation()
  {
    _value = _policy.Calculate(_participation, _maximum);
    _stage = _policy.DetermineStage(_value.Capped, _maximum);
    _evaluation = new PersonalLadiEvaluation(_memberId, 2026, 43, _participation, _value, _stage);
  }

  [Then("it identifies the family member and ISO week")]
  public async Task IdentifiesMemberAndWeek()
  {
    await Assert.That(_evaluation?.MemberId).IsEqualTo(_memberId);
    await Assert.That(_evaluation?.IsoYear).IsEqualTo(2026);
    await Assert.That(_evaluation?.IsoWeek).IsEqualTo(43);
  }

  [Then("it contains the source participation values")]
  public async Task ContainsSources()
  {
    await Assert.That(_evaluation?.Participation).IsEqualTo(_participation);
  }

  [Then("it contains one capped participation value")]
  public async Task ContainsCappedValue()
  {
    await Assert.That(_evaluation?.Value.Capped).IsEqualTo(_value.Capped);
  }

  [Then("it contains one Ladi stage and its ordered position")]
  public async Task ContainsStage()
  {
    await Assert.That(_evaluation?.Stage).IsEqualTo(_stage);
    await Assert.That(_evaluation?.Stage.Position).IsEqualTo(_stage.Position);
  }

  [Given("a family member has a completed personal Ladi evaluation for a closed ISO week")]
  public void ClosedEvaluation()
  {
    _participation = new PersonalParticipation(1, 50, 3);
    CreateEvaluation();
    _closedEvaluation = _evaluation;
  }

  [When("later contributions change the family member's current Ladi stage")]
  public void LaterContributions()
  {
    _participation = new PersonalParticipation(7, 100, 5);
    CreateEvaluation();
  }

  [Then("the closed week's participation value and Ladi stage remain unchanged")]
  public async Task ClosedEvaluationUnchanged()
  {
    await Assert.That(_closedEvaluation?.Value.Capped).IsEqualTo(4.75m);
    await Assert.That(_closedEvaluation?.Stage.Id).IsEqualTo("sun-ladi");
  }

  [Given("the current ISO week is still open")]
  public void OpenWeek()
  {
    _closedEvaluation = new PersonalLadiEvaluation(
      _memberId,
      2026,
      42,
      new PersonalParticipation(1, 50, 3),
      new ParticipationValue(4.75m, 4.75m),
      LadiStage.FromId("sun-ladi")
    );
    _participation = new PersonalParticipation(2, 60, 4);
  }

  [When("the family member's personal streak, completion or average basic rating changes")]
  public void ParticipationChanges()
  {
    _participation = new PersonalParticipation(4, 90, 5);
    CreateEvaluation();
  }

  [Then("the current Ladi preview is recalculated from the updated personal participation")]
  public async Task PreviewRecalculated()
  {
    await Assert.That(_evaluation?.Participation).IsEqualTo(_participation);
    await Assert.That(_evaluation?.Stage.Id).IsEqualTo("super-ladi");
  }

  [Then("no closed weekly Ladi evaluation is changed")]
  public async Task NoClosedEvaluationChanged()
  {
    await Assert.That(_closedEvaluation?.Stage.Id).IsEqualTo("sun-ladi");
  }

  [Given("the family member has no personal streak and no rated basic contribution")]
  public void NoStreakOrRating()
  {
    _participation = new PersonalParticipation(0, 0, 0);
  }

  [Given("the family member has {float} assigned basic energy")]
  public void AssignedEnergy(decimal energy)
  {
    _assignedEnergy = energy;
    _scenarioContext["AssignedBasicEnergy"] = energy;
  }

  [Given("{float} basic energy is approved")]
  public void BasicEnergy(decimal energy)
  {
    _approvedBasicEnergy = energy;
    _scenarioContext["ApprovedBasicEnergy"] = energy;
  }

  [Given("{float} extra energy is approved")]
  public void ExtraEnergy(decimal energy)
  {
    _approvedExtraEnergy = energy;
    _scenarioContext["ApprovedExtraEnergy"] = energy;
  }

  [When("the family member's personal Ladi stage is calculated")]
  public void CalculateStageFromEnergy()
  {
    _effectiveCompletion = _policy.CalculateEffectiveCompletion(
      _assignedEnergy,
      _approvedBasicEnergy,
      _approvedExtraEnergy
    );
    _participation = new PersonalParticipation(0, _effectiveCompletion, 0);
    CalculateCappedValue();
  }

  [Then("the effective completion is {float} percent")]
  public async Task EffectiveCompletion(decimal completion)
  {
    await Assert.That(_effectiveCompletion).IsEqualTo(completion);
  }

  [Given("the family member has {int} personal streak days")]
  public void StreakDays(int days)
  {
    _participation = new PersonalParticipation(
      days,
      _participation.EffectiveCompletionPercent,
      _participation.AverageBasicRating
    );
  }

  [Given("the family member's effective completion is {float} percent")]
  public void Completion(decimal completion)
  {
    _participation = new PersonalParticipation(
      _participation.StreakDays,
      completion,
      _participation.AverageBasicRating
    );
  }

  [Given("the family member's basic contributions have an average rating of {float} stars")]
  public void Rating(decimal rating)
  {
    _participation = new PersonalParticipation(
      _participation.StreakDays,
      _participation.EffectiveCompletionPercent,
      rating
    );
  }

  [When("the family member's participation value is calculated with the default weights")]
  public void CalculateParticipation()
  {
    _value = _policy.Calculate(_participation, _maximum);
  }

  [Then("the uncapped participation value is {float}")]
  public async Task UncappedValue(decimal value)
  {
    await Assert.That(_value.Uncapped).IsEqualTo(value);
  }

  [Then("the capped participation value is {float}")]
  public async Task CappedValue(decimal value)
  {
    await Assert.That(_value.Capped).IsEqualTo(value);
  }

  [Given("the maximum participation value is {float}")]
  public void Maximum(decimal maximum)
  {
    _maximum = maximum;
  }

  [Given("there are {int} ordered Ladi stages")]
  public async Task StageCount(int count)
  {
    await Assert.That(LadiStage.Count).IsEqualTo(count);
  }

  [Given("the family member's capped participation value is {float}")]
  public void GivenCappedValue(decimal value)
  {
    _value = new ParticipationValue(value, value);
  }

  [When("the family member's personal Ladi stage is determined")]
  public void DetermineStage()
  {
    _stage = _policy.DetermineStage(_value.Capped, _maximum);
  }

  [Then("the personal Ladi stage is {string} at position {int}")]
  public async Task StageAtPosition(string id, int position)
  {
    await Assert.That(_stage.Id).IsEqualTo(id);
    await Assert.That(_stage.Position).IsEqualTo(position);
  }

  [Given("the default participation configuration")]
  public void DefaultConfiguration()
  {
    _maximum = LadiProgressionPolicy.DefaultMaximumParticipation;
  }

  [When("the family member's capped participation value is calculated")]
  public void CalculateCappedValue()
  {
    CalculateParticipation();
    DetermineStage();
  }

  [Then("the personal Ladi stage is {string}")]
  public async Task Stage(string id)
  {
    await Assert.That(_stage.Id).IsEqualTo(id);
  }

  [Given("the family member reaches or exceeds the configured maximum participation value")]
  public void ReachesMaximum()
  {
    _value = new ParticipationValue(10, _maximum);
  }

  [Given("two participating family members have different personal streaks, completion and average ratings")]
  public void DifferentParticipants()
  {
    _participation = new PersonalParticipation(0, 0, 0);
    _secondStage = _policy.DetermineStage(_policy.Calculate(new PersonalParticipation(7, 100, 5)).Capped);
  }

  [When("their personal Ladi stages are determined")]
  public void DetermineIndependentStages()
  {
    CalculateCappedValue();
  }

  [Then("each stage uses only the corresponding family member's participation")]
  public async Task StagesAreIndependent()
  {
    await Assert.That(_stage.Id).IsEqualTo("idle-ladi");
    await Assert.That(_secondStage?.Id).IsEqualTo("super-ladi");
  }

  [Given("a family member's weekly Ladi evaluation contains {string} at position {int}")]
  public void EvaluatedStage(string id, int position)
  {
    _stage = new LadiStage(id, position);
  }

  [When("personal savings interest and family energy are determined")]
  public void DetermineDependentPolicies()
  {
    _interestStagePosition = _stage.Position;
    _energyPolicy.ContributionFor(_stage);
    _energyStagePosition = _stage.Position;
  }

  [Then("the interest policy uses {string} at position {int}")]
  public async Task InterestUsesStage(string id, int position)
  {
    await Assert.That(_stage.Id).IsEqualTo(id);
    await Assert.That(_interestStagePosition).IsEqualTo(position);
  }

  [Then("the house-energy policy uses {string} at position {int}")]
  public async Task EnergyUsesStage(string id, int position)
  {
    await Assert.That(_stage.Id).IsEqualTo(id);
    await Assert.That(_energyStagePosition).IsEqualTo(position);
  }

  [Then("neither policy recalculates personal participation")]
  public async Task PoliciesDoNotRecalculate()
  {
    await Assert.That(_participation).IsEqualTo(new PersonalParticipation(0, 0, 0));
  }
}
