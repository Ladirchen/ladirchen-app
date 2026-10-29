using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using Reqnroll;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class FamilyProfileSteps
{
  private readonly FamilyMemberId _admin = new("admin");
  private readonly FamilyMemberId _child = new("child");
  private readonly FamilyMemberId _secondChild = new("second-child");
  private readonly FamilyMemberId _guardian = new("guardian");
  private readonly FamilyMemberId _otherGuardian = new("other-guardian");
  private readonly DateTimeOffset _now = DateTimeOffset.Parse("2026-10-28T10:00:00Z");
  private FamilyAggregate _family = null!;
  private FamilyMemberId _actor = null!;
  private FamilyMemberId _target = null!;
  private string _invitationName = "Taylor";
  private string _invitationEmail = "taylor@example.com";
  private GuardianAccess _invitationAccess;
  private int _memberCountBefore;
  private bool _initialParticipation;

  [Given("the signed-in child is the active child")]
  public void ActiveSignedInChild()
  {
    CreateFamilyWithChild();
    _actor = _child;
  }

  [When("the child saves the nickname {string}")]
  public void SaveNickname(string nickname)
  {
    _family.SaveNickname(_actor, _child, nickname, _now);
  }

  [Then("the stored nickname is {string}")]
  public async Task StoredNickname(string nickname)
  {
    await Assert.That(_family.ActiveMember(_child).Nickname ?? string.Empty).IsEqualTo(nickname);
  }

  [Given("a nickname belongs to a child")]
  public void ExistingNickname()
  {
    ActiveSignedInChild();
    SaveNickname("Sunny");
  }

  [When("a guardian or another child attempts to change that nickname")]
  public void UnauthorizedNicknameChange()
  {
    Try(() => _family.SaveNickname(_admin, _child, "Changed", _now));
  }

  [Then("the nickname remains unchanged")]
  public async Task NicknameUnchanged()
  {
    await Assert.That(_family.ActiveMember(_child).Nickname).IsEqualTo("Sunny");
  }

  [Given("the signed-in child has a nickname")]
  public void ChildHasNickname()
  {
    ExistingNickname();
  }

  [When("the child saves a blank nickname")]
  public void SaveBlankNickname()
  {
    SaveNickname("   ");
  }

  [Then("the nickname is removed")]
  public async Task NicknameRemoved()
  {
    await Assert.That(_family.ActiveMember(_child).Nickname).IsNull();
  }

  [Then("the child's regular name is used for display")]
  public async Task RegularNameDisplayed()
  {
    await Assert.That(_family.ActiveMember(_child).DisplayName).IsEqualTo("Sam");
  }

  [Given("a guardian administrator enters a valid name and email address")]
  public void ValidInvitation()
  {
    CreateFamily();
    _actor = _admin;
    _invitationName = "Taylor";
    _invitationEmail = "taylor@example.com";
  }

  [Given("selects {string} guardian access")]
  public void SelectAccess(string access)
  {
    _invitationAccess = access == "admin" ? GuardianAccess.Admin : GuardianAccess.Supporter;
  }

  [When("the guardian administrator invites the guardian")]
  public void InviteGuardian()
  {
    _family.InviteGuardian(
      _actor,
      _guardian,
      _invitationName,
      _invitationEmail,
      _invitationAccess,
      _now
    );
  }

  [Then("the guardian is added with {string} access")]
  public async Task GuardianAdded(string access)
  {
    var expected = access == "admin" ? GuardianAccess.Admin : GuardianAccess.Supporter;
    await Assert.That(_family.Members.Single(item => item.Id == _guardian).Access).IsEqualTo(expected);
  }

  [Then("the invitation is pending")]
  public async Task InvitationPending()
  {
    await Assert.That(_family.Members.Single(item => item.Id == _guardian).InvitationPending).IsTrue();
  }

  [Then("the guardian does not participate in the weekly family goal")]
  public async Task InvitedGuardianDoesNotParticipate()
  {
    await Assert.That(_family.Members.Single(item => item.Id == _guardian).ParticipatesInWeeklyGoal)
      .IsFalse();
  }

  [Given("a family member attempts a guardian invitation with {string}")]
  public void InvalidInvitation(string invalidInput)
  {
    CreateFamilyWithChild();
    _actor = _admin;

    if (invalidInput == "no member-invitation permission")
    {
      _actor = _child;
    }
    else if (invalidInput == "a name shorter than two characters")
    {
      _invitationName = "T";
    }
    else
    {
      _invitationEmail = "invalid";
    }

    _memberCountBefore = _family.Members.Count;
  }

  [When("the invitation is submitted")]
  public void SubmitInvitation()
  {
    Try(InviteGuardian);
  }

  [Then("no guardian invitation is created")]
  public async Task NoInvitationCreated()
  {
    await Assert.That(_family.Members.Count).IsEqualTo(_memberCountBefore);
  }

  [Given("the signed-in guardian can manage the family")]
  public void ManagingGuardian()
  {
    CreateFamily();
    _actor = _admin;
  }

  [Given("another guardian belongs to the family")]
  public void AddOtherGuardian()
  {
    _family.AddMember(_admin, _otherGuardian, "Other", MemberRole.Guardian, GuardianAccess.Supporter, false, _now);
  }

  [When("the guardian administrator changes that guardian from supporter to guardian administrator")]
  public void PromoteOtherGuardian()
  {
    _family.ChangeGuardianAccess(_actor, _otherGuardian, GuardianAccess.Admin, _now);
  }

  [Then("the other guardian has guardian administrator access")]
  public async Task OtherGuardianIsAdmin()
  {
    await Assert.That(_family.ActiveMember(_otherGuardian).Access).IsEqualTo(GuardianAccess.Admin);
  }

  [Given("a guardian is signed in")]
  public void GuardianSignedIn()
  {
    CreateFamily();
    _actor = _admin;
  }

  [When("that guardian attempts to change their own access")]
  public void ChangeOwnAccess()
  {
    Try(() => _family.ChangeGuardianAccess(_actor, _actor, GuardianAccess.Supporter, _now));
  }

  [Then("their guardian access remains unchanged")]
  public async Task OwnAccessUnchanged()
  {
    await Assert.That(_family.ActiveMember(_actor).Access).IsEqualTo(GuardianAccess.Admin);
  }

  [Given("a guardian currently does not participate in the weekly family goal")]
  public void NonParticipatingGuardian()
  {
    CreateFamily();
    _family.AddMember(_admin, _guardian, "Taylor", MemberRole.Guardian, GuardianAccess.Supporter, false, _now);
    _target = _guardian;
  }

  [Given("the participation change is requested by {string}")]
  public void ParticipationActor(string actor)
  {
    _actor = actor == "that guardian" ? _guardian : _admin;
  }

  [When("participation is enabled")]
  public void EnableParticipation()
  {
    _family.SetGuardianWeeklyGoalParticipation(_actor, _target, true, _now);
  }

  [Then("the guardian participates in the weekly family goal")]
  public async Task GuardianParticipates()
  {
    await Assert.That(_family.ActiveMember(_target).ParticipatesInWeeklyGoal).IsTrue();
  }

  [Given("a family member attempts to change weekly-goal participation for {string}")]
  public void ParticipationTarget(string target)
  {
    CreateFamilyWithChild();
    _family.AddMember(_admin, _guardian, "Taylor", MemberRole.Guardian, GuardianAccess.Supporter, false, _now);
    _family.AddMember(_admin, _otherGuardian, "Other", MemberRole.Guardian, GuardianAccess.Supporter, false, _now);
    _target = target == "a child" ? _child : _otherGuardian;
    _initialParticipation = _family.ActiveMember(_target).ParticipatesInWeeklyGoal;
  }

  [Given("the actor is {string}")]
  public void UnauthorizedParticipationActor(string actor)
  {
    _actor = actor == "guardian administrator" ? _admin : _guardian;
  }

  [When("participation is changed")]
  public void ChangeParticipation()
  {
    Try(() => _family.SetGuardianWeeklyGoalParticipation(_actor, _target, true, _now));
  }

  [Then("weekly-goal participation remains unchanged")]
  public async Task ParticipationUnchanged()
  {
    await Assert.That(_family.ActiveMember(_target).ParticipatesInWeeklyGoal).IsEqualTo(_initialParticipation);
  }

  [Given("the family has two children")]
  public void TwoChildren()
  {
    CreateFamilyWithChild();
    _family.AddMember(_admin, _secondChild, "Robin", MemberRole.Child, GuardianAccess.Supporter, false, _now);
  }

  [When("a guardian selects the second child")]
  public void SelectSecondChild()
  {
    _family.SelectActiveChild(_admin, _secondChild, _now);
  }

  [Then("the second child becomes the active child")]
  public async Task SecondChildActive()
  {
    await Assert.That(_family.ActiveChildId).IsEqualTo(_secondChild);
  }

  [When("a child attempts to select another child")]
  public void ChildSelectsChild()
  {
    Try(() => _family.SelectActiveChild(_child, _child, _now));
  }

  [Then("the active child remains unchanged")]
  public async Task ActiveChildUnchanged()
  {
    await Assert.That(_family.ActiveChildId).IsEqualTo(_secondChild);
  }

  private void CreateFamily()
  {
    _family = new FamilyAggregate(new FamilyId(Guid.NewGuid()), _admin, "Alex", new IanaTimeZone("Europe/Zurich"));
  }

  private void CreateFamilyWithChild()
  {
    CreateFamily();
    _family.AddMember(_admin, _child, "Sam", MemberRole.Child, GuardianAccess.Supporter, false, _now);
  }

  private static void Try(Action action)
  {
    try
    {
      action();
    }
    catch (DomainRuleException)
    {
    }
  }
}
