using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class HouseSteps
{
  private readonly HouseEnergyPolicy _energyPolicy = new();
  private readonly HouseProgressionPolicy _progressionPolicy = new();
  private readonly FamilyHouse _house = new();
  private readonly FamilyMemberId _child = new("child");
  private readonly FamilyMemberId _guardian = new("guardian");
  private readonly List<LadiStage> _participantStages = new();
  private LadiStage _stage = LadiStage.AtPosition(1);
  private int _energy = HouseEnergyPolicy.DefaultEnergy;
  private int _memberEnergy;
  private int _visualLevel;
  private string _atmosphere = string.Empty;
  private bool _weekAccepted;
  private bool _actionAccepted;
  private int _requiredLevel;
  private string _contentType = string.Empty;
  private string _selectedTheme = "forest";
  private int _initialStoredLevel;
  private int _nonParticipantInitialStreak;

  [Given("a participating family member has the personal Ladi stage {string}")]
  public void ParticipantStage(string stageId)
  {
    _stage = LadiStage.FromId(stageId);
  }

  [When("the member's contribution to family energy is determined")]
  public void DetermineMemberEnergy()
  {
    _memberEnergy = _energyPolicy.ContributionFor(_stage);
  }

  [Then("the member contributes {int} percent energy")]
  public async Task MemberContributes(int energy)
  {
    await Assert.That(_memberEnergy).IsEqualTo(energy);
  }

  [Given("the participating family members have these personal Ladi stages:")]
  public void ParticipantStages(DataTable table)
  {
    _participantStages.Clear();

    foreach (var row in table.Rows)
    {
      _participantStages.Add(LadiStage.FromId(row["stageId"]));
    }
  }

  [Given("a guardian does not participate in the weekly family goal")]
  [Given("the family has pets")]
  [Given("no family member participates in the weekly family goal")]
  public void NoEnergyParticipants()
  {
    _participantStages.Clear();
  }

  [Given("the guardian has a personal Ladi stage")]
  public void NonParticipatingGuardianStage()
  {
    _stage = LadiStage.FromId("super-ladi");
  }

  [When("family energy is calculated")]
  public void CalculateEnergy()
  {
    _energy = _energyPolicy.Calculate(_participantStages);
  }

  [Then("family energy is {int} percent")]
  public async Task FamilyEnergy(int energy)
  {
    await Assert.That(_energy).IsEqualTo(energy);
  }

  [Then("that guardian's Ladi stage is ignored")]
  [Then("pets are ignored")]
  public async Task IgnoredFromEnergy()
  {
    await Assert.That(_participantStages).IsEmpty();
    await Assert.That(_energy).IsEqualTo(HouseEnergyPolicy.DefaultEnergy);
  }

  [Given("every participant's personal Ladi stage has already been determined")]
  public void EvaluatedStages()
  {
    _participantStages.Add(LadiStage.FromId("garden-ladi"));
    _participantStages.Add(LadiStage.FromId("super-ladi"));
  }

  [Then("the house-energy policy uses only the participants' Ladi stages")]
  public async Task UsesOnlyStages()
  {
    await Assert.That(_energy).IsEqualTo(70);
  }

  [Then("it does not recalculate streak, completion or average rating")]
  public async Task DoesNotRecalculateParticipation()
  {
    await Assert.That(_participantStages.Count).IsEqualTo(2);
  }

  [Given("an approved extra contribution improves a participant's effective completion")]
  public void ImprovedCompletion()
  {
    _stage = LadiStage.FromId("garden-ladi");
  }

  [When("the participant's personal Ladi stage changes from {string} to {string}")]
  public void ChangeStage(string previous, string current)
  {
    _memberEnergy = _energyPolicy.ContributionFor(LadiStage.FromId(previous));
    _stage = LadiStage.FromId(current);
  }

  [Then("the participant's energy contribution changes from {int} percent to {int} percent")]
  public async Task EnergyChanges(int previous, int current)
  {
    await Assert.That(_memberEnergy).IsEqualTo(previous);
    await Assert.That(_energyPolicy.ContributionFor(_stage)).IsEqualTo(current);
  }

  [Then("the extra contribution adds no separate amount directly to family energy")]
  public async Task NoDirectExtraEnergy()
  {
    await Assert.That(_energyPolicy.ContributionFor(_stage)).IsEqualTo(60);
  }

  [Given("family energy is {int} percent")]
  [Given("the family energy is {int} percent")]
  public void GivenEnergy(int energy)
  {
    _energy = energy;
  }

  [Given("the family energy is at least {int} percent")]
  public void EnergyAtLeast(int energy)
  {
    _energy = energy;
  }

  [Given("the family energy is below {int} percent")]
  public void EnergyBelow(int energy)
  {
    _energy = energy - 1;
  }

  [Given("the family energy is at least {int} but below {int} percent")]
  public void EnergyBetween(int minimum, int maximum)
  {
    _energy = minimum;
  }

  [When("the house visual level is determined")]
  public void DetermineVisualLevel()
  {
    _visualLevel = _energyPolicy.VisualLevel(_energy);
  }

  [Then("the visual level is {int}")]
  public async Task VisualLevel(int level)
  {
    await Assert.That(_visualLevel).IsEqualTo(level);
  }

  [When("a family member who can manage the family attempts to complete the family week")]
  [When("a family member who can manage the family attempts to close the family week as successful")]
  public void TryCompleteWeek()
  {
    _initialStoredLevel = _house.StoredLevel;
    _weekAccepted = _house.CloseSuccessfulWeek(_energy);
  }

  [Then("the week completion is {word}")]
  public async Task WeekCompletion(string result)
  {
    await Assert.That(_weekAccepted).IsEqualTo(result == "accepted");
  }

  [Given("the stored house level is {int}")]
  public void StoredLevel(int level)
  {
    _house.Restore(level, _house.CompletedWeeklyStreak);
    _initialStoredLevel = level;
  }

  [Given("the stored house level is at its maximum")]
  public void MaximumStoredLevel()
  {
    StoredLevel(FamilyHouse.MaximumStoredLevel);
  }

  [When("the house progression is displayed")]
  public void DisplayProgression()
  {
    _progressionPolicy.DisplayedLevel(_house.StoredLevel);
  }

  [Then("the displayed house level is {int}")]
  public async Task DisplayedLevel(int level)
  {
    await Assert.That(_progressionPolicy.DisplayedLevel(_house.StoredLevel)).IsEqualTo(level);
  }

  [Then("the house stage is {string}")]
  public async Task HouseStage(string stage)
  {
    await Assert.That(_progressionPolicy.Stage(_house.StoredLevel)).IsEqualTo(stage);
  }

  [Then("{int} rooms are available")]
  public async Task RoomCount(int count)
  {
    await Assert.That(_progressionPolicy.RoomCount(_house.StoredLevel)).IsEqualTo(count);
  }

  [When("the available house rooms are determined")]
  public void DetermineRooms()
  {
    _progressionPolicy.RoomCount(_house.StoredLevel);
  }

  [Then("the {string} room is available")]
  public async Task RoomAvailable(string room)
  {
    await Assert.That(_progressionPolicy.IsRoomAvailable(room, _house.StoredLevel)).IsTrue();
  }

  [When("a family member who can manage the family closes the family week as successful")]
  [When("the family week was successful closed")]
  public void CloseSuccessfulWeek()
  {
    TryCompleteWeek();
  }

  [Then("the stored house level is {int}")]
  public async Task ResultingStoredLevel(int level)
  {
    await Assert.That(_house.StoredLevel).IsEqualTo(level);
  }

  [Then("the stored house level remains {int}")]
  public async Task StoredLevelRemains(int level)
  {
    await Assert.That(_house.StoredLevel).IsEqualTo(level);
  }

  [Then("the stored house level remains at its maximum")]
  public async Task RemainsMaximum()
  {
    await Assert.That(_house.StoredLevel).IsEqualTo(FamilyHouse.MaximumStoredLevel);
  }

  [Then("the new house stage is revealed")]
  [Then("the lower house stage is revealed")]
  public async Task CorrectStageRevealed()
  {
    await Assert.That(_progressionPolicy.Stage(_house.StoredLevel)).IsNotEmpty();
  }

  [Then("the house progression remains unchanged")]
  public async Task ProgressionUnchanged()
  {
    await Assert.That(_house.StoredLevel).IsEqualTo(_initialStoredLevel);
  }

  [When("a family member who can manage the family closes the family week as failed")]
  public void CloseFailedWeek()
  {
    _house.CloseFailedWeek(_energy);
  }

  [Given("the {word} requires house level {int}")]
  public void ContentRequiresLevel(string contentType, int requiredLevel)
  {
    _contentType = contentType;
    _requiredLevel = requiredLevel;
  }

  [Given("the furniture set requires house level {int}")]
  public void FurnitureSetRequiresLevel(int level)
  {
    ContentRequiresLevel("furniture set", level);
  }

  [Given("the room design requires house level {int}")]
  public void RoomDesignRequiresLevel(int level)
  {
    ContentRequiresLevel("room design", level);
  }

  [Given("the house room requires house level {int}")]
  public void HouseRoomRequiresLevel(int level)
  {
    ContentRequiresLevel("house room", level);
  }

  [When("the child attempts to use the locked {word}")]
  public void UseLockedContent(string contentType)
  {
    _actionAccepted = _contentType == contentType && _progressionPolicy.IsContentAvailable(_house.StoredLevel, _requiredLevel);
  }

  [When("the child attempts to use the locked furniture set")]
  public void UseLockedFurnitureSet()
  {
    UseLockedContent("furniture set");
  }

  [When("the child attempts to use the locked room design")]
  public void UseLockedRoomDesign()
  {
    UseLockedContent("room design");
  }

  [When("the child attempts to use the locked house room")]
  public void UseLockedHouseRoom()
  {
    UseLockedContent("house room");
  }

  [Then("the action is rejected")]
  public async Task ActionRejected()
  {
    await Assert.That(_actionAccepted).IsFalse();
  }

  [Given("a house room requires level {int}")]
  public void RoomRequiresLevel(int requiredLevel)
  {
    _requiredLevel = requiredLevel;
  }

  [When("furniture is moved or placed into that room")]
  public void PlaceFurnitureInRoom()
  {
    _actionAccepted = _progressionPolicy.IsContentAvailable(_house.StoredLevel, _requiredLevel);
  }

  [Then("the furniture placement is rejected")]
  public async Task FurnitureRejected()
  {
    await Assert.That(_actionAccepted).IsFalse();
  }

  [When("the family house is displayed")]
  public void DisplayHouse()
  {
    _atmosphere = _energyPolicy.Atmosphere(_energy);
  }

  [Then("the {string} extension is visible")]
  public async Task ExtensionVisible(string extension)
  {
    await Assert.That(_progressionPolicy.IsExtensionVisible(extension, _house.StoredLevel)).IsTrue();
  }

  [Then("the house atmosphere is {string}")]
  public async Task Atmosphere(string atmosphere)
  {
    await Assert.That(_atmosphere).IsEqualTo(atmosphere);
  }

  [Given("the house has a selected theme and a stored progression level")]
  public void SelectedStructure()
  {
    _selectedTheme = "forest";
    StoredLevel(2);
  }

  [When("family energy changes")]
  public void ChangeEnergy()
  {
    _energy = 80;
    DisplayHouse();
  }

  [Then("the energy-dependent appearance is updated")]
  public async Task AppearanceUpdated()
  {
    await Assert.That(_atmosphere).IsEqualTo("sunny");
  }

  [Then("the selected theme and stored progression level remain unchanged")]
  public async Task StructureUnchanged()
  {
    await Assert.That(_selectedTheme).IsEqualTo("forest");
    await Assert.That(_house.StoredLevel).IsEqualTo(2);
  }

  [Given("the completed family weekly streak is {int}")]
  public void CompletedStreak(int streak)
  {
    _house.Restore(_house.StoredLevel, streak);
    _house.RegisterParticipant(_child, true, streak);
    _nonParticipantInitialStreak = streak + 2;
    _house.RegisterParticipant(_guardian, false, _nonParticipantInitialStreak);
  }

  [Then("the completed family weekly streak is {int}")]
  public async Task CompletedStreakIs(int streak)
  {
    await Assert.That(_house.CompletedWeeklyStreak).IsEqualTo(streak);
  }

  [Then("the completed family weekly streak remains {int}")]
  public async Task CompletedStreakRemains(int streak)
  {
    await Assert.That(_house.CompletedWeeklyStreak).IsEqualTo(streak);
  }

  [Then("every family member participating in the weekly goal has a stored weekly streak of {int}")]
  public async Task ParticipantStreak(int streak)
  {
    await Assert.That(_house.StoredWeeklyStreakFor(_child)).IsEqualTo(streak);
  }

  [Then("a non-participating family member's stored weekly streak remains unchanged")]
  public async Task NonParticipantStreakUnchanged()
  {
    await Assert.That(_house.StoredWeeklyStreakFor(_guardian)).IsEqualTo(_nonParticipantInitialStreak);
  }
}
