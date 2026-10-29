using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class HomeCommandHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("cbe09dd1-2036-4e47-865b-da4cf973392a"));
  private static readonly FamilyMemberId _administratorId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly DateTimeOffset _now = new(2026, 10, 10, 15, 0, 0, TimeSpan.Zero);

  [Test]
  public async Task PurchaseUsesAvailableBalanceAndAtomicallyPersistsTypedRoots()
  {
    var store = CreateStore(100, 50, HouseSubscriptionTier.Pro);
    var handler = store.CreateCommandHandler();

    var result = await ExecuteAsync(handler, _childId, "home.purchase-accessory", new
    {
      id = "wall-art",
    });

    var wallArt = Accessory(store, "wall-art");
    await Assert.That(wallArt.Owned).IsTrue();
    await Assert.That(wallArt.Equipped).IsFalse();
    await Assert.That(store.RequiredEconomy().WalletFor(_childId).Balance).IsEqualTo(55);
    await Assert.That(store.RequiredEconomy().WalletFor(_childId).Available).IsEqualTo(5);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy)).IsEqualTo(2);
    await Assert.That(store.Revision(TypedAggregateKind.HouseCustomization)).IsEqualTo(2);
    await Assert.That(store.LastRevisionGuards.Select(item => item.AggregateKind))
      .IsEquivalentTo([TypedAggregateKind.Family, TypedAggregateKind.FamilyProgression]);
    var changedAggregateTypes = result.ChangedAggregates
      .Select(item => item.AggregateType)
      .ToArray();
    await Assert.That(changedAggregateTypes).Contains("home-customization");
    await Assert.That(changedAggregateTypes).Contains("savings");
    await Assert.That(changedAggregateTypes).Contains("reward-shop");

    var homeRevision = store.Revision(TypedAggregateKind.HouseCustomization);
    var economyRevision = store.Revision(TypedAggregateKind.FamilyEconomy);

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "home.purchase-accessory", new
      {
        id = "floor-lamp",
      })
    ).Throws<FamilyCommandException>();
    await Assert.That(store.Revision(TypedAggregateKind.HouseCustomization))
      .IsEqualTo(homeRevision);
    await Assert.That(store.Revision(TypedAggregateKind.FamilyEconomy))
      .IsEqualTo(economyRevision);
    await Assert.That(store.RequiredEconomy().WalletFor(_childId).Balance).IsEqualTo(55);
  }

  [Test]
  public async Task LayoutUsesDomainCompatibilityBoundsCollisionsAndRevisionGuards()
  {
    var store = CreateStore(500, null, HouseSubscriptionTier.Pro);
    var handler = store.CreateCommandHandler();

    await ExecuteAsync(handler, _childId, "home.move-entity", new
    {
      placementId = "furniture-flower-boxes",
      zoneId = "living-room",
      x = 120,
      y = 110,
    });
    var flowerBoxes = Placement(store, "furniture-flower-boxes");
    await Assert.That(flowerBoxes.Placement.Area).IsEqualTo("garden");
    await Assert.That(flowerBoxes.Placement.X).IsEqualTo(97);
    await Assert.That(flowerBoxes.Placement.Y).IsEqualTo(94);
    await Assert.That(store.LastRevisionGuards.Select(item => item.AggregateKind))
      .IsEquivalentTo([TypedAggregateKind.Family, TypedAggregateKind.FamilyProgression]);

    var revisionBeforeCollision = store.Revision(TypedAggregateKind.HouseCustomization);

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "home.move-entity", new
      {
        placementId = "member-child",
        zoneId = "living-room",
        x = 70,
        y = 62,
      })
    ).Throws<FamilyCommandException>();
    await Assert.That(store.Revision(TypedAggregateKind.HouseCustomization))
      .IsEqualTo(revisionBeforeCollision);

    await ExecuteAsync(handler, _childId, "home.move-entity", new
    {
      placementId = "member-child",
      zoneId = "living-room",
      x = -10,
      y = 2,
    });
    var child = Placement(store, "member-child");
    await Assert.That(child.Placement.X).IsEqualTo(7);
    await Assert.That(child.Placement.Y).IsEqualTo(62);
  }

  [Test]
  public async Task PurchasesAndArrangementEnforceRoleLevelAndSubscriptionInTheDomain()
  {
    var store = CreateStore(500, null, HouseSubscriptionTier.Free);
    var handler = store.CreateCommandHandler();

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "home.move-entity", new
      {
        placementId = "member-child",
        zoneId = "living-room",
        x = 40,
        y = 70,
      })
    ).Throws<FamilyCommandException>();

    await Assert.That(async () =>
      await ExecuteAsync(handler, _childId, "home.purchase-accessory", new
      {
        id = "bookshelf",
      })
    ).Throws<FamilyCommandException>();

    await Assert.That(async () =>
      await ExecuteAsync(handler, _administratorId, "home.purchase-accessory", new
      {
        id = "wall-art",
      })
    ).Throws<FamilyCommandException>();
  }

  private static TestTypedAggregateStore CreateStore(
    decimal balance,
    decimal? reservedRewardPrice,
    HouseSubscriptionTier subscriptionTier
  )
  {
    var timeProvider = new FixedTimeProvider(_now);
    var family = new Family(
      _familyId,
      _administratorId,
      "Guardian",
      new IanaTimeZone("Europe/Zurich")
    );
    family.AddMember(
      _administratorId,
      _childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );
    var economy = new FamilyEconomy(family);
    economy.GiveSpecialGift(
      _administratorId,
      _childId,
      new LadirchenAmount(balance),
      "Test balance",
      new TransactionId("initial-balance"),
      _now
    );

    if (reservedRewardPrice is not null)
    {
      var rewardId = new ShopRewardId("reserved-reward");
      economy.AddShopReward(
        _administratorId,
        rewardId,
        new ShopRewardDefinition(
          "Reserved reward",
          ShopRewardCategory.Gift,
          new LadirchenAmount(reservedRewardPrice.Value),
          1,
          true,
          new AvailabilityWindow()
        ),
        _now
      );
      economy.RequestShopReward(
        _childId,
        rewardId,
        new RedemptionRequestId("reserved-request"),
        new TransactionId("reserve-reward"),
        _now
      );
    }

    var progression = FamilyHouse.CreateInitial(family);
    var house = FamilyHouseShop.CreateInitial(family, progression.StoredLevel, subscriptionTier);
    var store = new TestTypedAggregateStore(timeProvider);
    store.Seed(
      family,
      economy,
      progression: progression,
      house: house,
      updatedBy: _administratorId
    );

    return store;
  }

  private static async Task<FamilyCommandResult> ExecuteAsync(
    ExecuteFamilyCommandHandler handler,
    FamilyMemberId actorId,
    string commandType,
    object payload
  )
  {
    return await handler.HandleAsync(
      new FamilyCommand(
        Guid.NewGuid(),
        _familyId,
        actorId,
        commandType,
        JsonSerializer.SerializeToElement(payload)
      ),
      CancellationToken.None
    );
  }

  private static HouseItem Accessory(TestTypedAggregateStore store, string id)
  {
    return store.RequiredHouse().Accessories.Single(item => item.Id == id);
  }

  private static HouseLayoutEntity Placement(TestTypedAggregateStore store, string id)
  {
    return store.RequiredHouse().Placements.Single(item => item.Id == id);
  }
}
