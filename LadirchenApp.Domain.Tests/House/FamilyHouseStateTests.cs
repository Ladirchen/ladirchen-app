using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.House;

public sealed class FamilyHouseStateTests
{
  [Test]
  public async Task InitialCreationUsesFamilyIdentityDefaultsAndCurrentParticipants()
  {
    var child = new FamilyMemberId("child");
    var family = CompleteFamily(child);

    var house = FamilyHouse.CreateInitial(family);
    var state = house.CaptureState();

    await Assert.That(house.Id).IsEqualTo(family.Id);
    await Assert.That(house.DomainEvents).IsEmpty();
    await Assert.That(state.CurrentWeekTarget).IsEqualTo(PersonalStreakPolicy.DefaultWeeklyTargetDays);
    await Assert.That(state.MaximumSavingsInterestRate).IsEqualTo(SavingsInterestPolicy.Default.MaximumRate);
    await Assert.That(state.Participants.Select(item => item.MemberId)).Contains(child);
  }

  [Test]
  public async Task ReconstitutionRoundTripsAuthoritativeProgressionState()
  {
    var currentMember = new FamilyMemberId("current-member");
    var formerMember = new FamilyMemberId("former-member");
    var family = CompleteFamily(currentMember);
    var state = CompleteState(family.Id, currentMember, formerMember);

    var house = FamilyHouse.Reconstitute(family, state);
    var captured = house.CaptureState();

    await Assert.That(JsonSerializer.Serialize(captured)).IsEqualTo(JsonSerializer.Serialize(state));
    await Assert.That(house.StoredLevel).IsEqualTo(3);
    await Assert.That(house.CompletedWeeklyStreak).IsEqualTo(4);
    await Assert.That(house.CurrentWeekTarget).IsEqualTo(5);
    await Assert.That(house.MaximumSavingsInterestRate.Value).IsEqualTo(9);
    await Assert.That(house.Id).IsEqualTo(family.Id);
    await Assert.That(house.DomainEvents).IsEmpty();
  }

  [Test]
  public async Task CurrentFamilyContextDoesNotRewriteHistoricalMemberProgress()
  {
    var currentMember = new FamilyMemberId("current-member");
    var formerMember = new FamilyMemberId("former-member");
    var family = CompleteFamily(currentMember);
    var house = FamilyHouse.Reconstitute(
      family,
      CompleteState(family.Id, currentMember, formerMember)
    );

    var succeeded = house.CloseSuccessfulWeek(HouseEnergyPolicy.MinimumSuccessfulWeekEnergy);

    await Assert.That(succeeded).IsTrue();
    await Assert.That(house.StoredWeeklyStreakFor(currentMember)).IsEqualTo(5);
    await Assert.That(house.StoredWeeklyStreakFor(formerMember)).IsEqualTo(2);
    await Assert.That(house.ClosedWeeks.Single().MemberEvaluations.ContainsKey(formerMember)).IsTrue();
  }

  [Test]
  public async Task ReconstitutionAddsANewCurrentParticipantWithoutInventingAStreak()
  {
    var currentMember = new FamilyMemberId("current-member");
    var newMember = new FamilyMemberId("new-member");
    var formerMember = new FamilyMemberId("former-member");
    var family = CompleteFamily(currentMember, newMember);

    var house = FamilyHouse.Reconstitute(
      family,
      CompleteState(family.Id, currentMember, formerMember)
    );

    await Assert.That(house.StoredWeeklyStreakFor(newMember)).IsEqualTo(0);
    await Assert.That(house.CaptureState().Participants.Select(item => item.MemberId)).Contains(newMember);
  }

  [Test]
  public async Task ExplicitSynchronizationPrunesDepartedProgressWithoutRemovingClosedWeekHistory()
  {
    var administrator = new FamilyMemberId("administrator");
    var currentMember = new FamilyMemberId("current-member");
    var newMember = new FamilyMemberId("new-member");
    var formerMember = new FamilyMemberId("former-member");
    var family = CompleteFamily(currentMember);
    var house = FamilyHouse.Reconstitute(
      family,
      CompleteState(family.Id, currentMember, formerMember)
    );
    family.AddMember(
      administrator,
      newMember,
      "New Member",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      DateTimeOffset.Parse("2026-10-15T10:00:00Z")
    );

    house.SynchronizeParticipants(family);

    await Assert.That(house.StoredWeeklyStreakFor(newMember)).IsEqualTo(0);
    await Assert.That(house.CaptureState().Participants.Select(item => item.MemberId)).DoesNotContain(formerMember);
    await Assert.That(house.CaptureState().Participants.Select(item => item.MemberId)).Contains(newMember);
    await Assert.That(house.ClosedWeeks.Single().MemberEvaluations.ContainsKey(formerMember)).IsTrue();

    family.AddMember(
      administrator,
      formerMember,
      "Returning Member",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      DateTimeOffset.Parse("2026-10-16T10:00:00Z")
    );
    house.SynchronizeParticipants(family);

    await Assert.That(house.StoredWeeklyStreakFor(formerMember)).IsEqualTo(0);
  }

  private static FamilyHouseState CompleteState(
    FamilyId familyId,
    FamilyMemberId currentMember,
    FamilyMemberId formerMember
  )
  {
    var period = new IsoWeekPeriod(2026, 41);
    var historicalEvaluation = new PersonalLadiEvaluation(
      formerMember,
      period.Year,
      period.Week,
      new PersonalParticipation(9, 75, 4),
      new ParticipationValue(6, 6),
      LadiStage.AtPosition(5)
    );

    return new FamilyHouseState(
      familyId,
      3,
      4,
      5,
      new Percentage(9),
      [
        new ParticipantProgressState(currentMember, 4),
        new ParticipantProgressState(formerMember, 2),
      ],
      [
        new ClosedFamilyWeekState(
          period,
          FamilyWeekOutcome.Completed,
          DateTimeOffset.Parse("2026-10-12T10:00:00+02:00"),
          [historicalEvaluation]
        ),
      ]
    );
  }

  private static FamilyAggregate CompleteFamily(params FamilyMemberId[] childIds)
  {
    var administrator = new FamilyMemberId("administrator");
    var family = new FamilyAggregate(
      new FamilyId(Guid.Parse("0df009c4-6797-452b-a287-afb924b0f6be")),
      administrator,
      "Administrator",
      new IanaTimeZone("Europe/Zurich")
    );

    foreach (var childId in childIds)
    {
      family.AddMember(
        administrator,
        childId,
        childId.Value,
        MemberRole.Child,
        GuardianAccess.Supporter,
        false,
        DateTimeOffset.Parse("2026-10-12T10:00:00Z")
      );
    }

    family.ClearDomainEvents();

    return family;
  }
}
