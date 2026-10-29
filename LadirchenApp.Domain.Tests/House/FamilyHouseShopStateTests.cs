using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Family.Setup;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.Tests.House;

public sealed class FamilyHouseShopStateTests
{
  [Test]
  public async Task InitialCreationUsesCatalogDefaultsAndCompleteFamilyPlacements()
  {
    var family = CompleteFamily();

    var shop = FamilyHouseShop.CreateInitial(family, 0, HouseSubscriptionTier.Pro);
    var state = shop.CaptureState();

    await Assert.That(state.SelectedThemeId).IsEqualTo("sunny-dollhouse");
    await Assert.That(state.SubscriptionTier).IsEqualTo(HouseSubscriptionTier.Pro);
    await Assert.That(state.Id).IsEqualTo(family.Id);
    await Assert.That(shop.CanArrange()).IsTrue();
    await Assert.That(shop.DomainEvents).IsEmpty();
    await Assert.That(state.Accessories.Single(item => item.Id == "cozy-sofa").Owned).IsTrue();
    await Assert.That(state.Accessories.Single(item => item.Id == "bookshelf").Owned).IsFalse();
    await Assert.That(state.Placements.Select(item => item.Id)).Contains("family-ladi");
    await Assert.That(state.Placements.Select(item => item.Id)).Contains("member-administrator");
    await Assert.That(state.Placements.Select(item => item.Id)).Contains("member-child");
    await Assert.That(state.Placements.Select(item => item.Id)).Contains("pet-pet");
  }

  [Test]
  public async Task ReconstitutionRoundTripsHistoricalOwnershipAndPlacementsWithoutPurchasingAgain()
  {
    var family = CompleteFamily();
    var initial = FamilyHouseShop.CreateInitial(
      family,
      0,
      HouseSubscriptionTier.Free
    ).CaptureState();
    var historicalPlacement = new HousePlacement("creative-room", 44.25m, 68.75m, 0.82m);
    var historicalState = new FamilyHouseShopState(
      initial.Id,
      initial.SelectedThemeId,
      HouseSubscriptionTier.Pro,
      initial.Accessories.Select(item =>
        item.Id == "art-desk" ? new HouseItemState(item.Id, true, true) : item
      ),
      initial.Themes,
      initial.RoomDesigns,
      initial.Placements
        .Select(item =>
          item.EntityType == "furniture" && item.EntityId == "art-desk"
            ? new HouseLayoutEntityState(item.Id, item.EntityType, item.EntityId, historicalPlacement)
            : item
        )
        .Append(
          new HouseLayoutEntityState(
            "member-former-member",
            "member",
            "former-member",
            new HousePlacement("living-room", 75, 72, 0.78m)
          )
        )
    );

    var shop = FamilyHouseShop.Reconstitute(family, 0, historicalState);
    var captured = shop.CaptureState();
    var reconstructed = FamilyHouseShop.Reconstitute(family, 0, captured).CaptureState();

    await Assert.That(shop.HouseLevel).IsEqualTo(0);
    await Assert.That(shop.DomainEvents).IsEmpty();
    await Assert.That(captured.Accessories.Single(item => item.Id == "art-desk").Owned).IsTrue();
    await Assert.That(captured.Accessories.Single(item => item.Id == "art-desk").Equipped).IsTrue();
    await Assert.That(
      captured.Placements.Single(item => item.EntityId == "art-desk").Placement
    ).IsEqualTo(historicalPlacement);
    await Assert.That(captured.Placements.Select(item => item.EntityId)).Contains("former-member");
    await Assert.That(JsonSerializer.Serialize(reconstructed)).IsEqualTo(JsonSerializer.Serialize(captured));
  }

  [Test]
  public async Task ReconstitutionRejectsDuplicatePlacementsForOneHouseEntity()
  {
    var family = CompleteFamily();
    var initial = FamilyHouseShop.CreateInitial(
      family,
      0,
      HouseSubscriptionTier.Free
    ).CaptureState();
    var duplicate = new HouseLayoutEntityState(
      "duplicate-family-ladi",
      "ladi",
      "family-ladi",
      new HousePlacement("living-room", 20, 70, 0.78m)
    );
    var invalid = new FamilyHouseShopState(
      initial.Id,
      initial.SelectedThemeId,
      initial.SubscriptionTier,
      initial.Accessories,
      initial.Themes,
      initial.RoomDesigns,
      initial.Placements.Append(duplicate)
    );

    await Assert.That(() => FamilyHouseShop.Reconstitute(family, 0, invalid)).Throws<DomainRuleException>();
  }

  [Test]
  public async Task RootOperationsUseOwnedCatalogStateAndStoredSubscriptionTier()
  {
    var shop = FamilyHouseShop.CreateInitial(
      CompleteFamily(),
      0,
      HouseSubscriptionTier.Pro
    );

    var purchased = shop.PurchaseAccessory("floor-lamp", true, 90);
    var placed = shop.PlaceAccessory("floor-lamp", "living-room");
    var themePurchased = shop.PurchaseTheme("halloween-night", true, 320);
    var arranged = shop.ArrangeEntity("member-child", "living-room", 40, 90);
    var reset = shop.ResetEntity("member-child");
    var state = shop.CaptureState();

    await Assert.That(purchased).IsTrue();
    await Assert.That(placed).IsTrue();
    await Assert.That(themePurchased).IsTrue();
    await Assert.That(arranged).IsNotNull();
    await Assert.That(reset).IsNotNull();
    await Assert.That(state.Accessories.Single(item => item.Id == "floor-lamp").Equipped).IsTrue();
    await Assert.That(state.SelectedThemeId).IsEqualTo("halloween-night");
    await Assert.That(
      state.Placements.Single(item => item.Id == "member-child").Placement
    ).IsEqualTo(new HouseInitialPlacementPolicy().FamilyMember(1, MemberRole.Child));
  }

  [Test]
  public async Task ExplicitFamilySynchronizationReplacesMemberAndPetPlacements()
  {
    var administrator = new FamilyMemberId("administrator");
    var family = CompleteFamily();
    var shop = FamilyHouseShop.CreateInitial(family, 0, HouseSubscriptionTier.Pro);
    var retainedAdministrator = family.CaptureState().Members.Single(item => item.Id == administrator);
    var newChild = new FamilyMemberState(
      new FamilyMemberId("new-child"),
      "New Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      null,
      "new-child@example.com",
      true
    );
    var newPet = new FamilyPetState(
      new FamilyPetId("new-pet"),
      "Luna",
      FamilyPetKind.Dog
    );
    family.CompleteSetup(
      administrator,
      [retainedAdministrator, newChild],
      [newPet],
      DateTimeOffset.Parse("2026-10-15T10:00:00Z")
    );

    shop.SynchronizeFamilyEntities(family);
    var placementEntityIds = shop.CaptureState().Placements
      .Where(item => item.EntityType is "member" or "pet")
      .Select(item => item.EntityId)
      .ToArray();

    await Assert.That(placementEntityIds).DoesNotContain("child");
    await Assert.That(placementEntityIds).DoesNotContain("pet");
    await Assert.That(placementEntityIds).Contains("new-child");
    await Assert.That(placementEntityIds).Contains("new-pet");
  }

  private static FamilyAggregate CompleteFamily()
  {
    var administrator = new FamilyMemberId("administrator");
    var child = new FamilyMemberId("child");

    return FamilyAggregate.Reconstitute(
      new FamilyState(
        new FamilyId(Guid.Parse("4c221281-dd05-4ed6-9096-b6331c0d678c")),
        new IanaTimeZone("Europe/Zurich"),
        FamilyCurrency.CHF,
        new ExchangeRate(1),
        child,
        true,
        [
          new FamilyMemberState(
            administrator,
            "Administrator",
            MemberRole.Guardian,
            GuardianAccess.Admin,
            false,
            null,
            "administrator@example.com",
            true
          ),
          new FamilyMemberState(
            child,
            "Child",
            MemberRole.Child,
            GuardianAccess.Supporter,
            false,
            null,
            "child@example.com",
            true
          ),
        ],
        [
          new FamilyPetState(
            new FamilyPetId("pet"),
            "Milo",
            FamilyPetKind.Cat
          ),
        ]
      )
    );
  }
}
