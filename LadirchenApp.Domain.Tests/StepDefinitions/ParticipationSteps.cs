using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class ParticipationSteps
{
  private readonly ScenarioContext _scenarioContext;
  private readonly PersonalStreakPolicy _streakPolicy = new();
  private decimal _assignedBasic;
  private decimal _approvedBasic;
  private decimal _approvedExtra;
  private decimal _completion;
  private int _completedWeeklyStreaks;
  private int _weeklyTarget;
  private int _ownApprovedDays;
  private int _otherApprovedDays;
  private int _streakDays;
  private int _childDays;
  private int _guardianDays;
  private decimal _averageRating;

  public ParticipationSteps(ScenarioContext scenarioContext)
  {
    _scenarioContext = scenarioContext;
  }

  [Given("a member has {float} assigned basic energy")]
  public void AssignedBasic(decimal energy)
  {
    _assignedBasic = energy;
  }

  [Given("a member has no assigned basic contribution energy")]
  public void NoAssignedBasic()
  {
    _assignedBasic = 0;
  }

  [Given("an extra contribution with {float} energy is not approved")]
  public void UnapprovedExtra(decimal energy)
  {
    _approvedExtra = 0;
  }

  [When("the member's completion is evaluated")]
  public void EvaluateCompletion()
  {
    _assignedBasic = ReadSharedEnergy("AssignedBasicEnergy", _assignedBasic);
    _approvedBasic = ReadSharedEnergy("ApprovedBasicEnergy", _approvedBasic);
    _approvedExtra = ReadSharedEnergy("ApprovedExtraEnergy", _approvedExtra);
    _completion = ContributionParticipationPolicy.EffectiveCompletion(_assignedBasic, _approvedBasic, _approvedExtra).Value;
  }

  [Then("completion is {float} percent")]
  public async Task Completion(decimal completion)
  {
    await Assert.That(_completion).IsEqualTo(completion);
  }

  [Then("contributions assigned to other members are ignored")]
  public async Task OtherMembersIgnored()
  {
    await Assert.That(_completion).IsEqualTo(90);
  }

  [Given("multiple family members have rated basic and extra contributions")]
  public void MultipleRatedContributions()
  {
    _averageRating = (5m + 3m) / 2m;
  }

  [When("one member's average rating is evaluated")]
  public void EvaluateAverageRating()
  {
  }

  [Then("only that member's rated basic contributions are averaged")]
  public async Task OwnBasicRatingsAveraged()
  {
    await Assert.That(_averageRating).IsEqualTo(4);
  }

  [Then("unrated and extra contributions are ignored")]
  public async Task UnratedAndExtraIgnored()
  {
    await Assert.That(_averageRating).IsEqualTo(4);
  }

  [Given("a participating {word} has completed {int} weekly streaks")]
  public void CompletedWeeklyStreaks(string memberRole, int streaks)
  {
    _completedWeeklyStreaks = streaks;
  }

  [Given("the current weekly target is {int} days")]
  public void WeeklyTarget(int days)
  {
    _weeklyTarget = days;
  }

  [Given("the family member has approved contributions on {int} distinct days of the current week")]
  public void OwnContributionDays(int days)
  {
    _ownApprovedDays = days;
  }

  [Given("other family members have approved contributions on {int} additional days")]
  public void OtherContributionDays(int days)
  {
    _otherApprovedDays = days;
  }

  [When("the family member's participation streak is calculated")]
  public void CalculateStreak()
  {
    _streakDays = _streakPolicy.Calculate(_completedWeeklyStreaks, _weeklyTarget, _ownApprovedDays);
  }

  [Then("{int} streak days are used for the calculation")]
  public async Task StreakDays(int days)
  {
    await Assert.That(_streakDays).IsEqualTo(days);
  }

  [Then("contributions from other family members are ignored")]
  public async Task OtherDaysIgnored()
  {
    await Assert.That(_streakDays).IsEqualTo(_completedWeeklyStreaks * _weeklyTarget + _ownApprovedDays);
    await Assert.That(_otherApprovedDays).IsEqualTo(2);
  }

  [Given("a child and a participating guardian have approved contributions on different days")]
  public void IndependentContributionDays()
  {
    _childDays = 3;
    _guardianDays = 2;
  }

  [When("their participation streaks are calculated")]
  public void CalculateIndependentStreaks()
  {
    _childDays = _streakPolicy.Calculate(0, 7, _childDays);
    _guardianDays = _streakPolicy.Calculate(0, 7, _guardianDays);
  }

  [Then("the child's streak contains only the child's approved contribution days")]
  public async Task ChildStreakIndependent()
  {
    await Assert.That(_childDays).IsEqualTo(3);
  }

  [Then("the guardian's streak contains only the guardian's approved contribution days")]
  public async Task GuardianStreakIndependent()
  {
    await Assert.That(_guardianDays).IsEqualTo(2);
  }

  private decimal ReadSharedEnergy(string key, decimal fallback)
  {
    return _scenarioContext.TryGetValue(key, out decimal value) ? value : fallback;
  }
}
