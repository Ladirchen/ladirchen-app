using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Setup;
using LadirchenApp.Domain.Shared;

using Reqnroll;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class FamilySetupSteps
{
  private readonly FamilyMemberId _administratorId = new("admin");
  private readonly FamilyMemberId _childId = new("child");
  private readonly FamilyMemberId _supporterId = new("supporter");
  private FamilyAggregate _family = null!;
  private IReadOnlyList<FamilyMemberState> _draftMembers = [];
  private IReadOnlyList<FamilyPetState> _draftPets = [];
  private DomainRuleException? _setupError;
  private int _memberCountBeforeAttempt;

  [Given("family setup has been started")]
  public void StartSetup()
  {
    EditAggregateSetup();
  }

  [When("at least one child and one guardian have non-blank names")]
  public void AddRequiredMembers()
  {
    _family.AddMember(
      _administratorId,
      _childId,
      "Sam",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      Now()
    );
  }

  [Then("the family setup can be completed")]
  public async Task CanComplete()
  {
    _family.CompleteSetup(
      _administratorId,
      _family.Members.Select(ToState),
      [],
      Now()
    );
    await Assert.That(_family.Members.Any(member => member.Role == MemberRole.Child)).IsTrue();
    await Assert.That(_family.Members.Any(member => member.Role == MemberRole.Guardian)).IsTrue();
  }

  [When("a child is added during family setup")]
  public void AddChild()
  {
    AddRequiredMembers();
  }

  [Then("the child participates in the weekly family goal")]
  public async Task ChildParticipates()
  {
    await Assert.That(_family.ActiveMember(_childId).ParticipatesInWeeklyGoal).IsTrue();
  }

  [When("a guardian is added during family setup")]
  public void AddGuardian()
  {
    _family.AddMember(
      _administratorId,
      _supporterId,
      "Taylor",
      MemberRole.Guardian,
      GuardianAccess.Supporter,
      false,
      Now()
    );
  }

  [Then("the guardian has supporter access")]
  public async Task GuardianIsSupporter()
  {
    await Assert.That(_family.ActiveMember(_supporterId).Access).IsEqualTo(GuardianAccess.Supporter);
  }

  [Then("the new guardian does not participate in the weekly family goal")]
  public async Task GuardianDoesNotParticipate()
  {
    await Assert.That(_family.ActiveMember(_supporterId).ParticipatesInWeeklyGoal).IsFalse();
  }

  [Given("family onboarding has already been completed")]
  public void CompletedOnboarding()
  {
    EditAggregateSetup();
    _family.CompleteSetup(
      _administratorId,
      [
        MemberState(_administratorId, "Alex", MemberRole.Guardian, GuardianAccess.Admin, false),
        MemberState(_childId, "Sam", MemberRole.Child, GuardianAccess.Supporter, true),
      ],
      [new FamilyPetState(new FamilyPetId("pet"), "Milo", FamilyPetKind.Cat)],
      Now()
    );
  }

  [When("a guardian administrator edits the family setup")]
  public void EditSetup()
  {
    _draftMembers = _family.Members.Select(ToState).ToArray();
    _draftPets = _family.Pets.ToArray();
  }

  [Then("the existing children, guardians and pets are available for editing")]
  public async Task ExistingDataAvailable()
  {
    await Assert.That(_draftMembers.Single(member => member.Role == MemberRole.Child).Name).IsEqualTo("Sam");
    await Assert.That(_draftMembers.Single(member => member.Role == MemberRole.Guardian).Name).IsEqualTo("Alex");
    await Assert.That(_draftPets.Single().Name).IsEqualTo("Milo");
  }

  [Then("only saved changes replace the family setup")]
  public async Task OnlySavedChangesReplace()
  {
    var changedMembers = _draftMembers
      .Select(member => member.Id == _childId
        ? MemberState(_childId, "Samuel", MemberRole.Child, GuardianAccess.Supporter, true)
        : member)
      .ToArray();
    await Assert.That(_family.ActiveMember(_childId).Name).IsEqualTo("Sam");
    _family.CompleteSetup(_administratorId, changedMembers, _draftPets, Now());
    await Assert.That(_family.ActiveMember(_childId).Name).IsEqualTo("Samuel");
    await Assert.That(_family.Pets.Single().Name).IsEqualTo("Milo");
  }

  [Given("a guardian administrator is editing the aggregate family setup")]
  public void EditAggregateSetup()
  {
    _family = new FamilyAggregate(
      new FamilyId(Guid.Parse("94bac411-e603-40bd-ae9f-19a46c9d752f")),
      _administratorId,
      "Alex",
      new IanaTimeZone("Europe/Zurich")
    );
  }

  [When("setup is completed with an active guardian, an active child and an optional pet")]
  public void CompleteAggregateSetup()
  {
    _family.CompleteSetup(
      _administratorId,
      [
        MemberState(_administratorId, "Alex", MemberRole.Guardian, GuardianAccess.Admin, false),
        MemberState(_childId, "Sam", MemberRole.Child, GuardianAccess.Supporter, true),
      ],
      [new FamilyPetState(new FamilyPetId("pet"), "Milo", FamilyPetKind.Cat)],
      DateTimeOffset.Parse("2026-10-28T10:00:00Z")
    );
  }

  [Then("the aggregate contains the configured guardian and child")]
  public async Task AggregateContainsMembers()
  {
    await Assert.That(_family.Members.Any(item => item.Id == _administratorId && item.IsAdministrator)).IsTrue();
    await Assert.That(_family.Members.Any(item => item.Id == _childId && item.ParticipatesInWeeklyGoal)).IsTrue();
  }

  [Then("the aggregate contains the configured pet")]
  public async Task AggregateContainsPet()
  {
    await Assert.That(_family.Pets.Single().Name).IsEqualTo("Milo");
  }

  [When("setup completion is attempted with only the active guardian")]
  public void CompleteWithoutChild()
  {
    _memberCountBeforeAttempt = _family.Members.Count;

    try
    {
      _family.CompleteSetup(
        _administratorId,
        [MemberState(_administratorId, "Alex", MemberRole.Guardian, GuardianAccess.Admin, false)],
        [],
        DateTimeOffset.Parse("2026-10-28T10:00:00Z")
      );
    }
    catch (DomainRuleException exception)
    {
      _setupError = exception;
    }
  }

  [When("setup completion is attempted with only the active child")]
  public void CompleteWithoutGuardian()
  {
    _memberCountBeforeAttempt = _family.Members.Count;

    try
    {
      _family.CompleteSetup(
        _administratorId,
        [MemberState(_childId, "Sam", MemberRole.Child, GuardianAccess.Supporter, true)],
        [],
        DateTimeOffset.Parse("2026-10-28T10:00:00Z")
      );
    }
    catch (DomainRuleException exception)
    {
      _setupError = exception;
    }
  }

  [Then("aggregate setup completion is rejected")]
  public async Task AggregateSetupRejected()
  {
    await Assert.That(_setupError?.Code).IsEqualTo(DomainError.InvalidValue);
  }

  [Then("the aggregate family state remains unchanged")]
  public async Task AggregateStateUnchanged()
  {
    await Assert.That(_family.Members.Count).IsEqualTo(_memberCountBeforeAttempt);
    await Assert.That(_family.Pets).IsEmpty();
  }

  private static FamilyMemberState MemberState(
    FamilyMemberId id,
    string name,
    MemberRole role,
    GuardianAccess access,
    bool participates
  )
  {
    return new FamilyMemberState(id, name, role, access, false, null, null, participates);
  }

  private static FamilyMemberState ToState(FamilyMember member)
  {
    return new FamilyMemberState(
      member.Id,
      member.Name,
      member.Role,
      member.Access,
      member.InvitationPending,
      member.Nickname,
      member.EmailAddress,
      member.ParticipatesInWeeklyGoal
    );
  }

  private static DateTimeOffset Now()
  {
    return DateTimeOffset.Parse("2026-10-28T10:00:00Z");
  }
}
