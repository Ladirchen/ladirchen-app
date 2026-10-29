using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public static class InitialFamilyAggregateFactory
{
  private static readonly IanaTimeZone DefaultTimeZone = new("Europe/Zurich");

  public static TypedAggregateSaveBatch Create(
    FamilyId familyId,
    FamilyMemberId actorId,
    string guardianName
  )
  {
    var family = new Family(familyId, actorId, guardianName, DefaultTimeZone);
    var economy = new FamilyEconomy(family);
    var progression = FamilyHouse.CreateInitial(family);
    var houseCustomization = FamilyHouseShop.CreateInitial(
      family,
      progression.StoredLevel,
      HouseSubscriptionTier.Free
    );

    return new TypedAggregateSaveBatch(
      familyId,
      actorId,
      family: new PendingFamilySave(family, 0),
      familyEconomy: new PendingFamilyEconomySave(
        economy,
        FamilyEconomyPresentationState.Empty,
        0
      ),
      familyProgression: new PendingFamilyProgressionSave(progression, 0),
      houseCustomization: new PendingHouseCustomizationSave(houseCustomization, 0)
    );
  }
}
