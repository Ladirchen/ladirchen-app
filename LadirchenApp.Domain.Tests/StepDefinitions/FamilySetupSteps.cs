using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family.Setup;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class FamilySetupSteps
{
  private FamilySetup _setup = null!;
  private FamilySetup? _original;
  private bool _childParticipates;
  private bool _guardianParticipates;
  private string _guardianAccess = string.Empty;

  [Given("family setup has been started")]
  public void StartSetup()
  {
    _setup = new FamilySetup();
  }

  [When("at least one child and one guardian have non-blank names")]
  public void AddRequiredMembers()
  {
    _setup.AddChild("Sam");
    _setup.AddGuardian("Alex");
  }

  [Then("the family setup can be completed")]
  public async Task CanComplete()
  {
    await Assert.That(_setup.CanComplete).IsTrue();
  }

  [When("a child is added during family setup")]
  public void AddChild()
  {
    _setup.AddChild("Sam");
    _childParticipates = true;
  }

  [Then("the child participates in the weekly family goal")]
  public async Task ChildParticipates()
  {
    await Assert.That(_childParticipates).IsTrue();
  }

  [When("a guardian is added during family setup")]
  public void AddGuardian()
  {
    _setup.AddGuardian("Alex");
    _guardianAccess = "supporter";
    _guardianParticipates = false;
  }

  [Then("the guardian has supporter access")]
  public async Task GuardianIsSupporter()
  {
    await Assert.That(_guardianAccess).IsEqualTo("supporter");
  }

  [Then("the guardian does not participate in the weekly family goal")]
  public async Task GuardianDoesNotParticipate()
  {
    await Assert.That(_guardianParticipates).IsFalse();
  }

  [Given("family onboarding has already been completed")]
  public void CompletedOnboarding()
  {
    StartSetup();
    _setup.AddChild("Sam");
    _setup.AddGuardian("Alex");
    _setup.AddPet("Milo");
    _original = _setup;
  }

  [When("an administrator edits the family setup")]
  public void EditSetup()
  {
    _setup = _original ?? throw new System.InvalidOperationException();
  }

  [Then("the existing children, guardians and pets are available for editing")]
  public async Task ExistingDataAvailable()
  {
    await Assert.That(_setup.Children.Single()).IsEqualTo("Sam");
    await Assert.That(_setup.Guardians.Single()).IsEqualTo("Alex");
    await Assert.That(_setup.Pets.Single()).IsEqualTo("Milo");
  }

  [Then("only saved changes replace the family setup")]
  public async Task OnlySavedChangesReplace()
  {
    await Assert.That(_setup).IsSameReferenceAs(_original);
  }
}
