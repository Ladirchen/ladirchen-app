using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Avatar;
using LadirchenApp.Domain.Shared;

using Reqnroll;

namespace LadirchenApp.Domain.Tests.StepDefinitions;

[Binding]
public sealed class AvatarSteps
{
  private readonly AvatarCatalog _catalog = new();
  private readonly FamilyMemberId _member = new("member");
  private readonly FamilyMemberId _other = new("other");
  private AvatarAppearanceService _service;
  private AvatarProfile _profile;
  private AvatarAppearance? _appearance;
  private AvatarAppearance? _secondResolution;
  private string _unknownPart = string.Empty;
  private bool _rejected;
  private bool _authorized = true;

  public AvatarSteps()
  {
    _service = new AvatarAppearanceService(_catalog);
  }

  [Given("a child has no saved avatar appearance")]
  public void ChildWithoutAppearance()
  {
    _service = new AvatarAppearanceService(_catalog);
    _profile = AvatarProfile.Child;
  }

  [When("the child's appearance is resolved by their index among children")]
  public void ResolveChild()
  {
    _appearance = _service.Resolve(_member, _profile, 1);
    _secondResolution = _service.Resolve(_member, _profile, 1);
  }

  [Then("a complete child appearance is returned")]
  public async Task CompleteChildAppearance()
  {
    await Assert.That(_appearance).IsNotNull();
    await Assert.That(_appearance?.Age).IsEqualTo("child");
  }

  [Then("the same child index always receives the same default variant")]
  public async Task DeterministicChildDefault()
  {
    await Assert.That(_secondResolution).IsEqualTo(_appearance);
  }

  [Given("a guardian has no saved avatar appearance")]
  public void GuardianWithoutAppearance()
  {
    _service = new AvatarAppearanceService(_catalog);
    _profile = AvatarProfile.Guardian;
  }

  [When("the guardian's appearance is resolved by their index among guardians")]
  public void ResolveGuardian()
  {
    _appearance = _service.Resolve(_member, _profile, 1);
  }

  [Then("a complete adult or senior guardian appearance is returned")]
  public async Task CompleteGuardianAppearance()
  {
    await Assert.That(_appearance).IsNotNull();
    await Assert.That(new[] { "adult", "senior" }.Contains(_appearance?.Age)).IsTrue();
  }

  [Given("the avatar editor is opened for a {string} profile")]
  public void OpenEditor(string profile)
  {
    _profile = profile == "child" ? AvatarProfile.Child : AvatarProfile.Guardian;
  }

  [When("available avatar parts are listed")]
  public void ListParts()
  {
    _catalog.Options("age", _profile);
  }

  [Then("only parts supporting {string} are available")]
  public async Task OnlySupportedParts(string profile)
  {
    var expectedAge = profile == "child" ? "child" : "adult";
    await Assert.That(_catalog.Options("age", _profile)).Contains(expectedAge);
  }

  [Given("a family member is signed in and their own profile is active")]
  public void OwnActiveProfile()
  {
    _authorized = true;
    _profile = AvatarProfile.Child;
  }

  [When("the member saves a valid avatar appearance")]
  public void SaveValidAppearance()
  {
    _appearance = ValidAppearance(_profile);
    _service.Save(_member, _member, _member, _profile, _appearance);
  }

  [Then("the appearance is stored for the signed-in member")]
  public async Task StoredForMember()
  {
    await Assert.That(_service.SavedFor(_member)).IsEqualTo(_appearance);
  }

  [Then("every other family member's appearance remains unchanged")]
  public async Task OtherAppearancesUnchanged()
  {
    await Assert.That(_service.SavedFor(_other)).IsNull();
  }

  [Given("the signed-in member attempts to save an appearance for {string}")]
  public void UnauthorizedProfile(string profile)
  {
    _authorized = false;
    _profile = AvatarProfile.Child;
  }

  [When("the appearance is submitted")]
  public void SubmitAppearance()
  {
    var target = _authorized ? _member : _other;
    TrySave(target, ValidAppearance(_profile));
  }

  [Then("every avatar appearance remains unchanged")]
  public async Task EveryAppearanceUnchanged()
  {
    await Assert.That(_service.SavedFor(_member)).IsNull();
    await Assert.That(_service.SavedFor(_other)).IsNull();
  }

  [Given("an avatar appearance contains an unknown {string} value")]
  public void UnknownValue(string part)
  {
    _unknownPart = part;
    _profile = AvatarProfile.Child;
    _appearance = ValidAppearance(_profile);
  }

  [When("the family member attempts to save the appearance")]
  public void SaveUnknownAppearance()
  {
    TrySave(_member, WithUnknownValue(_appearance ?? throw new InvalidOperationException(), _unknownPart));
  }

  [Then("the appearance is rejected")]
  public async Task AppearanceRejected()
  {
    await Assert.That(_rejected).IsTrue();
  }

  [Then("the previously stored appearance remains unchanged")]
  public async Task PreviousAppearanceUnchanged()
  {
    await Assert.That(_service.SavedFor(_member)).IsNull();
  }

  private void TrySave(FamilyMemberId target, AvatarAppearance appearance)
  {
    try
    {
      _service.Save(_member, target, _member, _profile, appearance);
    }
    catch (DomainRuleException)
    {
      _rejected = true;
    }
  }

  private static AvatarAppearance ValidAppearance(AvatarProfile profile)
  {
    return new AvatarAppearance(
      profile == AvatarProfile.Child ? "child" : "adult",
      "light",
      "round",
      "smile",
      "short",
      "brown",
      profile == AvatarProfile.Child ? "play" : "casual",
      "blue",
      "none",
      "none",
      "none"
    );
  }

  private static AvatarAppearance WithUnknownValue(AvatarAppearance value, string part)
  {
    return part switch
    {
      "age" => value with { Age = "unknown" },
      "skin tone" => value with { SkinTone = "unknown" },
      "face shape" => value with { FaceShape = "unknown" },
      "face" => value with { Face = "unknown" },
      "hair" => value with { Hair = "unknown" },
      "hair color" => value with { HairColor = "unknown" },
      "outfit" => value with { Outfit = "unknown" },
      "outfit color" => value with { OutfitColor = "unknown" },
      "accessory" => value with { Accessory = "unknown" },
      "fun accessory" => value with { FunAccessory = "unknown" },
      "seasonal accessory" => value with { SeasonalAccessory = "unknown" },
      _ => value,
    };
  }
}
