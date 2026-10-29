using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Models.ReadModels;
using LadirchenApp.Webhost.Tests.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Application.Queries;

public sealed class HomeCustomizationQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(
    Guid.Parse("b37c2c39-c76e-4fb0-b77f-1ef039f9c3eb")
  );
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");
  private static readonly DateTimeOffset _now = new(2026, 10, 29, 10, 0, 0, TimeSpan.Zero);

  [Test]
  public async Task QueryProjectsTypedHouseCatalogAndMemberCapabilities()
  {
    var family = new Family(
      _familyId,
      _guardianId,
      "Guardian",
      new IanaTimeZone("Europe/Zurich")
    );
    family.AddMember(
      _guardianId,
      _childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      _now
    );
    var economy = new FamilyEconomy(family);
    economy.GiveSpecialGift(
      _guardianId,
      _childId,
      new LadirchenAmount(500),
      "Home catalog test balance",
      new TransactionId("home-query-balance"),
      _now
    );
    var progression = FamilyHouse.CreateInitial(family);
    var customization = FamilyHouseShop.CreateInitial(
      family,
      progression.StoredLevel,
      HouseSubscriptionTier.Free
    );
    var store = new TestTypedAggregateStore(new FixedTimeProvider(_now));
    store.Seed(
      family,
      economy,
      progression: progression,
      house: customization,
      revision: 4,
      updatedBy: _guardianId
    );
    var handler = new GetFamilyAggregateQueryHandler(
      store,
      store,
      store,
      store,
      new FixedTimeProvider(_now)
    );

    var projection = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "home-customization", _childId),
      CancellationToken.None
    ) as HomeCustomizationReadModelProjection
      ?? throw new InvalidOperationException("The home customization result is required.");
    var response = HomeCustomizationReadModelResponse.FromProjection(projection);
    var wallArt = response.State.Catalog.Accessories.Single(item => item.Id == "wall-art");
    var bookshelf = response.State.Catalog.Accessories.Single(item => item.Id == "bookshelf");
    var halloweenTheme = response.State.Catalog.Themes.Single(
      item => item.Id == "halloween-night"
    );

    await Assert.That(response.Revision).IsEqualTo(4);
    await Assert.That(response.State.Capabilities.CanArrangeHouse).IsFalse();
    await Assert.That(wallArt.Available).IsTrue();
    await Assert.That(wallArt.PurchasableByMemberIds).IsEquivalentTo([_childId.Value]);
    await Assert.That(bookshelf.Available).IsFalse();
    await Assert.That(bookshelf.PurchasableByMemberIds).IsEmpty();
    await Assert.That(halloweenTheme.PurchasableByMemberIds)
      .IsEquivalentTo([_childId.Value]);
    await Assert.That(response.State.SelectedEditionId).IsEqualTo("sunny-dollhouse");
    await Assert.That(response.State.Placements).IsNotEmpty();
  }
}
