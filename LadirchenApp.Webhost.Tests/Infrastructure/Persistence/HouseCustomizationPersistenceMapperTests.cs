using System;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class HouseCustomizationPersistenceMapperTests
{
  [Test]
  public async Task RoundTripsFamilyCatalogPricesAndPromotions()
  {
    var family = new Family(
      new FamilyId(Guid.Parse("b7fe0e98-fb67-4f8a-b978-81d807342edd")),
      new FamilyMemberId("administrator"),
      "Administrator",
      new IanaTimeZone("Europe/Zurich")
    );
    var localDate = new DateOnly(2026, 11, 13);
    var shop = FamilyHouseShop.CreateInitial(family, 0, HouseSubscriptionTier.Pro);
    shop.SetCatalogPrice(
      "furniture-set",
      "cozy-living-set",
      10,
      4,
      new DateOnly(2026, 11, 20),
      localDate
    );

    var entity = HouseCustomizationPersistenceMapper.CreateEntity(shop);
    var restored = HouseCustomizationPersistenceMapper.ToDomain(entity, family, 0);
    var price = restored.CatalogPrice("furniture-set", "cozy-living-set");

    await Assert.That(entity.CatalogPricesJson).Contains("cozy-living-set");
    await Assert.That(price.Price).IsEqualTo(10);
    await Assert.That(price.PromotionalPrice).IsEqualTo(4);
    await Assert.That(price.PromotionUntil).IsEqualTo(new DateOnly(2026, 11, 20));
  }
}
