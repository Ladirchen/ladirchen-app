using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

public sealed partial class EconomySteps
{
  private FamilyMemberId _contributionActor = null!;
  private FamilyMemberId? _contributionAssignee;
  private string _contributionTitle = "Help";
  private ContributionKind _contributionKind = ContributionKind.Basic;
  private decimal _contributionEnergy = 10;
  private int _contributionCountBefore;
  private FamilyMemberId? _assigneeBefore;
  private decimal _walletBeforeDelete;
  private FamilyMemberId _partner = null!;

  [Given("an available contribution assigned to the child")]
  public void AvailableAssignedContribution()
  {
    CreateEconomy();
    AddAvailableContribution(_child, ContributionKind.Basic, 10);
  }

  [When("another family member attempts to submit it")]
  public void OtherMemberSubmits()
  {
    Try(() => _economy.SubmitContribution(_sibling, _contributionId, _now));
  }

  [Then("the contribution remains available")]
  public async Task ContributionRemainsAvailable()
  {
    await Assert.That(_economy.Contributions.Single().Status).IsEqualTo(ContributionStatus.Available);
  }

  [Given("an available extra contribution has invited partners")]
  public void ExtraContributionWithPartners()
  {
    CreateEconomy();
    AddAvailableContribution(_child, ContributionKind.Extra, 20);
    _economy.SetContributionPartners(_child, _contributionId, [_sibling], _now);
  }

  [When("a guardian administrator assigns the contribution to another member")]
  public void AssignToOtherMember()
  {
    _economy.AssignContribution(_admin, _contributionId, _sibling, _now);
  }

  [Then("the contribution has no invited partners")]
  [Then("previous partner invitations are removed")]
  public async Task NoPartners()
  {
    await Assert.That(_economy.Contributions.Single().InvitedPartners).IsEmpty();
  }

  [Given("a guardian administrator creates an extra contribution with {float} energy")]
  public void AdministratorCreatesExtra(decimal energy)
  {
    CreateEconomy();
    _contributionKind = ContributionKind.Extra;
    _contributionEnergy = energy;
    _contributionActor = _admin;
    _contributionAssignee = _child;
  }

  [When("the contribution is added")]
  public void AddPreparedContribution()
  {
    AddPreparedContributionCore();
  }

  [Then("its stored energy is {float}")]
  public async Task StoredContributionEnergy(decimal energy)
  {
    await Assert.That(_economy.Contributions.Single().Energy.Value).IsEqualTo(energy);
  }

  [Given("an available extra contribution assigned to a {word}")]
  public void TeamContribution(string role)
  {
    CreateEconomy();
    var assignee = role == "child" ? _child : _supporter;
    AddAvailableContribution(assignee, ContributionKind.Extra, 20);
  }

  [When("an active {word} family member is invited as a partner")]
  public void InvitePartner(string role)
  {
    _partner = role == "child" ? _child : _supporter;
    _economy.SetContributionPartners(_admin, _contributionId, [_partner], _now);
  }

  [Then("the {word} family member belongs to the contribution team")]
  public async Task PartnerBelongsToTeam(string role)
  {
    await Assert.That(_economy.Contributions.Single().InvitedPartners).Contains(_partner);
  }

  [Then("the assignee cannot be invited as their own partner")]
  public async Task AssigneeNotPartner()
  {
    var contribution = _economy.Contributions.Single();
    await Assert.That(contribution.InvitedPartners).DoesNotContain(contribution.AssigneeId!);
  }

  [Given("a family member has content-management permission")]
  public void ContentManagerCreates()
  {
    CreateEconomy();
    _contributionActor = _admin;
  }

  [Given("valid contribution data names an existing family member as assignee")]
  public void ExistingAssignee()
  {
    _contributionAssignee = _child;
  }

  [When("the family member creates the contribution")]
  public void CreatePreparedContribution()
  {
    AddPreparedContributionCore();
  }

  [Then("the contribution is available to its assignee")]
  public async Task ContributionAvailableToAssignee()
  {
    var contribution = _economy.Contributions.Single();
    await Assert.That(contribution.Status).IsEqualTo(ContributionStatus.Available);
    await Assert.That(contribution.AssigneeId).IsEqualTo(_child);
  }

  [Given("a guardian does not have content-management permission")]
  public void GuardianWithoutContentPermission()
  {
    CreateEconomy();
    _contributionActor = _supporter;
  }

  [When("the guardian creates a valid contribution assigned to themselves")]
  public void GuardianCreatesOwnContribution()
  {
    _contributionAssignee = _supporter;
    AddPreparedContributionCore();
  }

  [Then("the contribution is available to that guardian")]
  public async Task ContributionAvailableToGuardian()
  {
    await Assert.That(_economy.Contributions.Single().AssigneeId).IsEqualTo(_supporter);
  }

  [Given("a family member enters valid contribution data assigned to {string}")]
  public void UnauthorizedContributionData(string assignee)
  {
    CreateEconomy();
    _contributionActor = _supporter;
    _contributionAssignee = assignee == "nobody" ? null : _child;
    _contributionCountBefore = _economy.Contributions.Count;
  }

  [Given("the family member cannot manage content")]
  [Given("a family member cannot manage content")]
  public void CannotManageContent()
  {
    if (_economy is null)
    {
      CreateEconomy();
    }

    _contributionActor = _supporter;
  }

  [When("the family member attempts to create the contribution")]
  public void AttemptContributionCreation()
  {
    TryAny(AddPreparedContributionCore);
  }

  [Then("no contribution is created")]
  public async Task NoContributionCreated()
  {
    await Assert.That(_economy.Contributions.Count).IsEqualTo(_contributionCountBefore);
  }

  [Given("a family member is otherwise allowed to create a contribution")]
  public void AllowedContributionCreator()
  {
    CreateEconomy();
    _contributionActor = _admin;
    _contributionAssignee = _child;
    _contributionCountBefore = 0;
  }

  [When("the contribution is submitted with a blank title")]
  public void BlankContributionTitle()
  {
    _contributionTitle = "   ";
    TryAny(AddPreparedContributionCore);
  }

  [Given("an available contribution")]
  public void AvailableContribution()
  {
    CreateEconomy();
    AddAvailableContribution(_child, ContributionKind.Basic, 10);
    _assigneeBefore = _child;
  }

  [When("a family member without content-management permission attempts to assign it")]
  public void UnauthorizedAssignment()
  {
    Try(() => _economy.AssignContribution(_supporter, _contributionId, _sibling, _now));
  }

  [Then("the contribution assignment remains unchanged")]
  public async Task AssignmentUnchanged()
  {
    await Assert.That(_economy.Contributions.Single().AssigneeId).IsEqualTo(_assigneeBefore);
  }

  [Given("a family member who can manage content attempts to assign a contribution that is {string}")]
  public void InvalidAssignmentState(string state)
  {
    CreateEconomy();
    AddAvailableContribution(_child, ContributionKind.Basic, 10);
    _assigneeBefore = _child;

    if (state == "pending")
    {
      _economy.SubmitContribution(_child, _contributionId, _now);
    }
    else if (state == "approved")
    {
      _economy.SubmitContribution(_child, _contributionId, _now);
      _economy.ApproveContribution(_admin, _contributionId, new StarRating(4), NextOperation(), _now);
    }

    _contributionAssignee = state == "assigned to a missing family member" ? new FamilyMemberId("missing") : _sibling;
  }

  [When("the assignment is submitted")]
  public void SubmitAssignment()
  {
    Try(() => _economy.AssignContribution(_admin, _contributionId, _contributionAssignee, _now));
  }

  [Given("an available contribution is assigned to one family member")]
  public void ContributionAssignedToOne()
  {
    ExtraContributionWithPartners();
    _assigneeBefore = _child;
  }

  [Given("a different family member exists")]
  public void DifferentMemberExists()
  {
    _contributionAssignee = _sibling;
  }

  [When("a family member who can manage content assigns the contribution to the different family member")]
  public void ReassignContribution()
  {
    _economy.AssignContribution(_admin, _contributionId, _contributionAssignee, _now);
  }

  [Then("the different family member is the contribution assignee")]
  public async Task DifferentAssignee()
  {
    await Assert.That(_economy.Contributions.Single().AssigneeId).IsEqualTo(_sibling);
  }

  [Given("a contribution exists")]
  [Given("an existing contribution")]
  public void ExistingContribution()
  {
    CreateEconomy();
    AddAvailableContribution(_child, ContributionKind.Basic, 10);
  }

  [When("a family member without content-management permission attempts to delete it")]
  public void UnauthorizedDeleteContribution()
  {
    Try(() => _economy.DeleteContribution(_supporter, _contributionId, _now));
  }

  [Then("the contribution remains unchanged")]
  public async Task ContributionUnchanged()
  {
    await Assert.That(_economy.Contributions.Count).IsEqualTo(1);
  }

  [Given("an approved contribution has credited its assignee's wallet")]
  public void ApprovedCreditedContribution()
  {
    CreateEconomy();
    SubmittedContribution(_child, _admin, 10);
    Approve(5);
    _walletBeforeDelete = _economy.WalletFor(_child).Balance;
  }

  [When("a family member who can manage content deletes the contribution")]
  [When("a guardian administrator deletes the contribution")]
  public void DeleteContribution()
  {
    _economy.DeleteContribution(_admin, _contributionId, _now);
  }

  [Then("the contribution no longer exists")]
  public async Task ContributionDeleted()
  {
    await Assert.That(_economy.Contributions).IsEmpty();
  }

  [Then("the previously credited wallet balance remains unchanged")]
  public async Task CreditedBalanceUnchanged()
  {
    await Assert.That(_economy.WalletFor(_child).Balance).IsEqualTo(_walletBeforeDelete);
  }

  private void AddAvailableContribution(FamilyMemberId assignee, ContributionKind kind, decimal energy)
  {
    _economy.AddContribution(
      _admin,
      _contributionId,
      new ContributionDefinition("Help", kind, new LadirchenAmount(10), new Percentage(energy), assignee),
      _now
    );
  }

  private void AddPreparedContributionCore()
  {
    _economy.AddContribution(
      _contributionActor,
      _contributionId,
      new ContributionDefinition(
        _contributionTitle,
        _contributionKind,
        new LadirchenAmount(10),
        new Percentage(_contributionEnergy),
        _contributionAssignee
      ),
      _now
    );
  }

  private void TryAny(Action action)
  {
    _beforeRejectedCommand = Snapshot();

    try
    {
      action();
    }
    catch (DomainRuleException exception)
    {
      _error = exception;
    }
    catch (ArgumentException)
    {
    }
  }
}
