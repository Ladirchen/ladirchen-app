using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using Reqnroll;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class PersonalMemberAccountSteps
{
  private readonly FamilyMemberId _administratorId = new("administrator");
  private readonly FamilyMemberId _activeChildId = new("active-child");
  private readonly DateTimeOffset _now = DateTimeOffset.Parse("2026-10-29T10:00:00Z");
  private readonly List<(
    string Name,
    string EmailAddress,
    MemberRole Role,
    GuardianAccess Access
  )> _invalidInvitations = new();
  private readonly List<DomainError?> _invalidInvitationErrors = new();
  private FamilyAggregate _family = null!;
  private FamilyMemberId _invitedMemberId = null!;
  private FamilyMemberId _rejectedMemberId = null!;
  private DomainRuleException? _invitationError;
  private int _memberCountBeforeRejection;

  [Given("a family can invite personal members")]
  public void FamilyCanInvitePersonalMembers()
  {
    _family = new FamilyAggregate(
      new FamilyId(Guid.Parse("4b69f443-c79f-4dda-ac28-31cfc996863e")),
      _administratorId,
      "Alex",
      new IanaTimeZone("Europe/Zurich")
    );
  }

  [When("the guardian administrator invites the child {string} with email {string}")]
  public void InviteChild(string name, string emailAddress)
  {
    _invitedMemberId = new FamilyMemberId("invited-child");
    _family.InviteMember(
      _administratorId,
      _invitedMemberId,
      name,
      emailAddress,
      MemberRole.Child,
      GuardianAccess.Supporter,
      _now
    );
  }

  [Then("the invited child is pending")]
  public async Task InvitedChildIsPending()
  {
    var member = InvitedMember();
    await Assert.That(member.Role).IsEqualTo(MemberRole.Child);
    await Assert.That(member.InvitationPending).IsTrue();
    await Assert.That(member.EmailAddress).IsEqualTo("sam@example.com");
    await Assert.That(() => _family.ActiveMember(_invitedMemberId)).Throws<DomainRuleException>();
  }

  [Then("the pending child does not participate in the weekly family goal")]
  public async Task PendingChildDoesNotParticipate()
  {
    await Assert.That(InvitedMember().ParticipatesInWeeklyGoal).IsFalse();
    await Assert.That(_family.WeeklyGoalParticipants.Any(member => member.Id == _invitedMemberId)).IsFalse();
  }

  [Then("the pending child is not the active child")]
  public async Task PendingChildIsNotActiveChild()
  {
    await Assert.That(_family.ActiveChildId).IsNull();
  }

  [When("the guardian administrator invites the guardian {string} with email {string}")]
  public void InviteGuardian(string name, string emailAddress)
  {
    _invitedMemberId = new FamilyMemberId("invited-guardian");
    _family.InviteMember(
      _administratorId,
      _invitedMemberId,
      name,
      emailAddress,
      MemberRole.Guardian,
      GuardianAccess.Supporter,
      _now
    );
  }

  [Then("the invited guardian is pending")]
  public async Task InvitedGuardianIsPending()
  {
    var member = InvitedMember();
    await Assert.That(member.Role).IsEqualTo(MemberRole.Guardian);
    await Assert.That(member.InvitationPending).IsTrue();
    await Assert.That(member.EmailAddress).IsEqualTo("taylor@example.com");
    await Assert.That(() => _family.ActiveMember(_invitedMemberId)).Throws<DomainRuleException>();
  }

  [Then("the invited guardian has supporter access")]
  public async Task InvitedGuardianIsSupporter()
  {
    await Assert.That(InvitedMember().Access).IsEqualTo(GuardianAccess.Supporter);
  }

  [Then("the pending guardian does not participate in the weekly family goal")]
  [Then("the accepted guardian does not participate in the weekly family goal")]
  public async Task GuardianDoesNotParticipate()
  {
    await Assert.That(InvitedMember().ParticipatesInWeeklyGoal).IsFalse();
  }

  [Given("a pending personal invitation for a child")]
  public void PendingChildInvitation()
  {
    FamilyCanInvitePersonalMembers();
    InviteChild("Sam", "sam@example.com");
  }

  [Given("a pending personal invitation for a guardian")]
  public void PendingGuardianInvitation()
  {
    FamilyCanInvitePersonalMembers();
    InviteGuardian("Taylor", "taylor@example.com");
  }

  [When("the personal member invitation is accepted")]
  public void AcceptPersonalMemberInvitation()
  {
    _family.AcceptInvitation(_invitedMemberId, _now);
  }

  [Then("the invited child is active")]
  public async Task InvitedChildIsActive()
  {
    var member = _family.ActiveMember(_invitedMemberId);
    await Assert.That(member.Role).IsEqualTo(MemberRole.Child);
    await Assert.That(member.InvitationPending).IsFalse();
  }

  [Then("the accepted child participates in the weekly family goal")]
  public async Task AcceptedChildParticipates()
  {
    await Assert.That(_family.ActiveMember(_invitedMemberId).ParticipatesInWeeklyGoal).IsTrue();
    await Assert.That(_family.WeeklyGoalParticipants.Any(member => member.Id == _invitedMemberId)).IsTrue();
  }

  [Then("the accepted child is the family's first active child")]
  public async Task AcceptedChildIsFirstActiveChild()
  {
    await Assert.That(_family.ActiveChildId).IsEqualTo(_invitedMemberId);
  }

  [Then("family onboarding is complete")]
  public async Task FamilyOnboardingIsComplete()
  {
    await Assert.That(_family.OnboardingCompleted).IsTrue();
  }

  [Then("the invited guardian is active")]
  public async Task InvitedGuardianIsActive()
  {
    var member = _family.ActiveMember(_invitedMemberId);
    await Assert.That(member.Role).IsEqualTo(MemberRole.Guardian);
    await Assert.That(member.InvitationPending).IsFalse();
  }

  [Given("a family has an active child without family administration permission")]
  public void FamilyHasActiveChildWithoutAdministrationPermission()
  {
    FamilyCanInvitePersonalMembers();
    _family.AddMember(
      _administratorId,
      _activeChildId,
      "Sam",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );
  }

  [When("the child attempts to invite another family member")]
  public void ChildAttemptsInvitation()
  {
    _rejectedMemberId = new FamilyMemberId("unauthorized-invitation");
    _memberCountBeforeRejection = _family.Members.Count;
    _invitationError = CaptureDomainError(() => _family.InviteMember(
      _activeChildId,
      _rejectedMemberId,
      "Taylor",
      "taylor@example.com",
      MemberRole.Guardian,
      GuardianAccess.Supporter,
      _now
    ));
  }

  [Then("the personal member invitation is rejected as forbidden")]
  public async Task PersonalMemberInvitationIsForbidden()
  {
    await Assert.That(_invitationError?.Code).IsEqualTo(DomainError.Forbidden);
  }

  [Then("the rejected personal member invitation leaves the family unchanged")]
  public async Task RejectedInvitationLeavesFamilyUnchanged()
  {
    await Assert.That(_family.Members.Count).IsEqualTo(_memberCountBeforeRejection);
    await Assert.That(_family.Members.Any(member => member.Id == _rejectedMemberId)).IsFalse();
  }

  [Given("these invalid personal member invitations:")]
  public void InvalidPersonalMemberInvitations(DataTable table)
  {
    FamilyCanInvitePersonalMembers();
    _invalidInvitations.Clear();

    foreach (var row in table.Rows)
    {
      _invalidInvitations.Add(
        (
          row["name"],
          row["email"],
          ParseRole(row["role"]),
          ParseAccess(row["guardianAccess"])
        )
      );
    }

  }

  [When("the invalid personal member invitations are submitted")]
  public void SubmitInvalidPersonalMemberInvitations()
  {
    _memberCountBeforeRejection = _family.Members.Count;
    _invalidInvitationErrors.Clear();

    for (var index = 0; index < _invalidInvitations.Count; index++)
    {
      var invitation = _invalidInvitations[index];
      var error = CaptureDomainError(() => _family.InviteMember(
        _administratorId,
        new FamilyMemberId($"invalid-invitation-{index}"),
        invitation.Name,
        invitation.EmailAddress,
        invitation.Role,
        invitation.Access,
        _now
      ));
      _invalidInvitationErrors.Add(error?.Code);
    }

  }

  [Then("every invalid personal member invitation is rejected")]
  public async Task EveryInvalidPersonalMemberInvitationIsRejected()
  {
    await Assert.That(_invalidInvitationErrors.Count).IsEqualTo(_invalidInvitations.Count);
    await Assert.That(_invalidInvitationErrors.All(error => error == DomainError.InvalidValue)).IsTrue();
  }

  [Then("no invalid personal member invitation changes the family")]
  public async Task InvalidInvitationsDoNotChangeFamily()
  {
    await Assert.That(_family.Members.Count).IsEqualTo(_memberCountBeforeRejection);
  }

  [Given("a personal member invitation already uses {string}")]
  public void ExistingEmailInvitation(string emailAddress)
  {
    FamilyCanInvitePersonalMembers();
    InviteChild("Sam", emailAddress);
  }

  [When("the guardian administrator invites another member with email {string}")]
  public void InviteAnotherMemberWithSameEmail(string emailAddress)
  {
    _rejectedMemberId = new FamilyMemberId("duplicate-email-invitation");
    _memberCountBeforeRejection = _family.Members.Count;
    _invitationError = CaptureDomainError(() => _family.InviteMember(
      _administratorId,
      _rejectedMemberId,
      "Robin",
      emailAddress,
      MemberRole.Guardian,
      GuardianAccess.Supporter,
      _now
    ));
  }

  [Then("the duplicate personal member invitation is rejected")]
  public async Task DuplicatePersonalMemberInvitationIsRejected()
  {
    await Assert.That(_invitationError?.Code).IsEqualTo(DomainError.Duplicate);
  }

  private FamilyMember InvitedMember()
  {
    return _family.Members.Single(member => member.Id == _invitedMemberId);
  }

  private static DomainRuleException? CaptureDomainError(Action action)
  {
    try
    {
      action();

      return null;
    }
    catch (DomainRuleException exception)
    {
      return exception;
    }
  }

  private static MemberRole ParseRole(string value)
  {
    return value switch
    {
      "child" => MemberRole.Child,
      "guardian" => MemberRole.Guardian,
      _ => (MemberRole)(-1),
    };
  }

  private static GuardianAccess ParseAccess(string value)
  {
    return value switch
    {
      "admin" => GuardianAccess.Admin,
      "supporter" => GuardianAccess.Supporter,
      _ => (GuardianAccess)(-1),
    };
  }
}