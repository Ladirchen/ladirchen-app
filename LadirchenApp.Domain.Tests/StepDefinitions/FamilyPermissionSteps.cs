using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Permissions;
using LadirchenApp.Domain.Shared;

using Reqnroll;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class FamilyPermissionSteps
{
  private readonly FamilyPermissionPolicy _policy = new();
  private FamilyAggregate _family = null!;
  private FamilyMember? _member;
  private FamilyPermissions _permissions = null!;

  [Given("the signed-in member is a {string}")]
  public void SignedInRole(string role)
  {
    var adminId = new FamilyMemberId("admin");
    _family = new FamilyAggregate(new FamilyId(Guid.NewGuid()), adminId, "Admin", new IanaTimeZone("Europe/Zurich"));

    if (role == "guardian admin")
    {
      _member = _family.ActiveMember(adminId);

      return;
    }

    var memberId = new FamilyMemberId("member");
    var memberRole = role == "child" ? MemberRole.Child : MemberRole.Guardian;
    _family.AddMember(adminId, memberId, "Member", memberRole, GuardianAccess.Supporter, false, DateTimeOffset.UtcNow);
    _member = _family.ActiveMember(memberId);
  }

  [Given("no signed-in family member can be resolved")]
  public void MissingMember()
  {
    _member = null;
  }

  [When("family permissions are resolved")]
  public void ResolvePermissions()
  {
    _permissions = _policy.Resolve(_member);
  }

  [Then("member invitation is {word}")]
  public Task Invitation(string expected)
  {
    return AssertPermission(_permissions.InviteMembers, expected);
  }

  [Then("goal management is {word}")]
  public Task GoalManagement(string expected)
  {
    return AssertPermission(_permissions.ManageGoals, expected);
  }

  [Then("content management is {word}")]
  public Task ContentManagement(string expected)
  {
    return AssertPermission(_permissions.ManageContent, expected);
  }

  [Then("family management is {word}")]
  public Task FamilyManagement(string expected)
  {
    return AssertPermission(_permissions.ManageFamily, expected);
  }

  [Then("child-goal support is {word}")]
  public Task GoalSupport(string expected)
  {
    return AssertPermission(_permissions.SupportGoals, expected);
  }

  [Then("family-goal visibility is {word}")]
  public Task FamilyGoalVisibility(string expected)
  {
    return AssertPermission(_permissions.ViewFamilyGoals, expected);
  }

  [Then("guardian-goal visibility is {word}")]
  public Task GuardianGoalVisibility(string expected)
  {
    return AssertPermission(_permissions.ViewGuardianGoals, expected);
  }

  [Then("every family capability is denied")]
  public async Task AllDenied()
  {
    await Assert.That(_permissions.GetType().GetProperties().All(item => item.GetValue(_permissions) is false)).IsTrue();
  }

  private static async Task AssertPermission(bool actual, string expected)
  {
    await Assert.That(actual).IsEqualTo(expected == "allowed");
  }
}
