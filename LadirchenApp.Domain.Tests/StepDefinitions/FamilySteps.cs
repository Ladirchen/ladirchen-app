using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using Reqnroll;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class FamilySteps
{
  private readonly FamilyMemberId _administrator = new("admin");
  private readonly FamilyMemberId _child = new("child");
  private readonly DateTimeOffset _now = DateTimeOffset.Parse("2026-10-10T10:00:00Z");
  private FamilyAggregate _family = null!;
  private DomainRuleException? _error;

  [Given("a family with an administrator")]
  public void CreateFamily()
  {
    _family = new(
      new FamilyId(Guid.Parse("94bac411-e603-40bd-ae9f-19a46c9d752f")),
      _administrator,
      "Alex",
      new("Europe/Zurich")
    );
  }

  [When("the administrator adds a child")]
  [Given("the administrator adds a child")]
  public void AddChild()
  {
    _family.AddMember(_administrator, _child, "Sam", MemberRole.Child, GuardianAccess.Supporter, false, _now);
  }

  [When("a second administrator is added")]
  public void AddAdministrator()
  {
    _family.AddMember(_administrator, new("admin2"), "Pat", MemberRole.Guardian, GuardianAccess.Admin, false, _now);
  }

  [When("the child tries to add a member")]
  public void ChildAddsMember()
  {
    Try(() => _family.AddMember(_child, new("other"), "Pat", MemberRole.Child, GuardianAccess.Supporter, false, _now));
  }

  [When("the administrator tries to become a supporter")]
  public void Demote()
  {
    Try(() => _family.ChangeGuardianAccess(_administrator, _administrator, GuardianAccess.Supporter, _now));
  }

  [Then("the family command fails with {string}")]
  public async Task Error(string code)
  {
    await Assert.That(_error?.Code.ToString()).IsEqualTo(code);
  }

  [Then("the family has {int} active members")]
  public async Task MemberCount(int count)
  {
    await Assert.That(_family.Members.Count(item => !item.InvitationPending)).IsEqualTo(count);
  }

  [Then("the family has {int} administrators")]
  public async Task AdminCount(int count)
  {
    await Assert.That(_family.Members.Count(item => item.IsAdministrator)).IsEqualTo(count);
  }

  private void Try(Action action)
  {
    try
    {
      action();
    }
    catch (DomainRuleException exception)
    {
      _error = exception;
    }
  }
}
