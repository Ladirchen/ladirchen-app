using System.Threading.Tasks;

using LadirchenApp.Domain.Avatar;

namespace LadirchenApp.Domain.Tests.Avatar;

public sealed class AvatarCatalogTests
{
  private readonly AvatarCatalog _catalog = new();

  [Test]
  public async Task ChildCatalogContainsTheNewCollectionItems()
  {
    await Assert.That(_catalog.Options("accessory", AvatarProfile.Child)).Contains("blue-ocarina");
    await Assert.That(_catalog.Options("accessory", AvatarProfile.Child)).Contains("gemstone-earrings");
    await Assert.That(_catalog.Options("accessory", AvatarProfile.Child)).Contains("headset-microphone");
    await Assert.That(_catalog.Options("accessory", AvatarProfile.Child)).Contains("fantasy-ears");
    await Assert.That(_catalog.Options("hair", AvatarProfile.Child)).Contains("popstar-space-buns");
    await Assert.That(_catalog.Options("hair", AvatarProfile.Child)).Contains("forest-braid");
    await Assert.That(_catalog.Options("outfit", AvatarProfile.Child)).Contains("glitter-stage-jacket");
    await Assert.That(_catalog.Options("outfit", AvatarProfile.Child)).Contains("forest-adventurer");
  }

  [Test]
  public async Task GuardianCatalogDoesNotContainChildCollectionItems()
  {
    await Assert.That(_catalog.Options("accessory", AvatarProfile.Guardian)).DoesNotContain("blue-ocarina");
    await Assert.That(_catalog.Options("accessory", AvatarProfile.Guardian)).DoesNotContain("gemstone-earrings");
    await Assert.That(_catalog.Options("accessory", AvatarProfile.Guardian)).DoesNotContain("headset-microphone");
    await Assert.That(_catalog.Options("accessory", AvatarProfile.Guardian)).DoesNotContain("fantasy-ears");
    await Assert.That(_catalog.Options("hair", AvatarProfile.Guardian)).DoesNotContain("popstar-space-buns");
    await Assert.That(_catalog.Options("hair", AvatarProfile.Guardian)).DoesNotContain("forest-braid");
    await Assert.That(_catalog.Options("outfit", AvatarProfile.Guardian)).DoesNotContain("glitter-stage-jacket");
    await Assert.That(_catalog.Options("outfit", AvatarProfile.Guardian)).DoesNotContain("forest-adventurer");
  }

  [Test]
  public async Task ChildCollectionAppearancePassesCatalogValidation()
  {
    var appearance = new AvatarAppearance(
      "child",
      "skin-light",
      "soft",
      "happy",
      "popstar-space-buns",
      "hair-brown",
      "glitter-stage-jacket",
      "outfit-blue",
      "blue-ocarina",
      "none",
      "none"
    );

    await Assert.That(() => _catalog.Validate(appearance, AvatarProfile.Child)).ThrowsNothing();
  }
}