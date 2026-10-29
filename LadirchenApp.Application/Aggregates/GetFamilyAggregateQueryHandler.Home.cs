using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed partial class GetFamilyAggregateQueryHandler
{
  private async Task<HomeCustomizationReadModelProjection?> ProjectHomeCustomizationAsync(
    Family family,
    FamilyMemberId capabilityMemberId,
    CancellationToken cancellationToken
  )
  {
    var loadedProgression = RequiredDependency(
      await _progressionRepository.LoadAsync(family, cancellationToken),
      "family progression"
    );
    var loadedCustomization = await _houseCustomizationRepository.LoadAsync(
      family,
      loadedProgression.Root.StoredLevel,
      cancellationToken
    );

    if (loadedCustomization is null)
    {
      return null;
    }

    var loadedEconomy = RequiredDependency(
      await _economyRepository.LoadAsync(family, cancellationToken),
      "family economy"
    );
    var member = family.ActiveMember(capabilityMemberId);
    var availableBalance = loadedEconomy.Root.WalletFor(member.Id).Available;
    var customization = loadedCustomization.Root;
    var accessories = customization.Accessories.ToDictionary(item => item.Id);
    var availableAccessoryIds = customization.Accessories
      .Where(item => customization.IsAvailableAtCurrentLevel(item.MinimumLevel))
      .Select(item => item.Id)
      .ToHashSet();
    var purchasableAccessoryIds = customization.Accessories
      .Where(item => customization.CanPurchaseAccessory(
        item.Id,
        member.Role == MemberRole.Child,
        availableBalance
      ))
      .Select(item => item.Id)
      .ToHashSet();
    var availableAreaIds = HouseShopCatalog.AreaMinimumLevels
      .Where(item => customization.IsAvailableAtCurrentLevel(item.Value))
      .Select(item => item.Key)
      .ToHashSet();
    var availableFurnitureSetIds = HouseShopCatalog.FurnitureSets
      .Where(item => customization.IsAvailableAtCurrentLevel(item.Value.MinimumLevel))
      .Select(item => item.Key)
      .ToHashSet();
    var ownedFurnitureSetIds = HouseShopCatalog.FurnitureSets
      .Where(item => item.Value.AccessoryIds.All(id => accessories[id].Owned))
      .Select(item => item.Key)
      .ToHashSet();
    var purchasableFurnitureSetIds = HouseShopCatalog.FurnitureSets
      .Where(item => customization.CanPurchaseFurnitureSet(
        item.Key,
        member.Role == MemberRole.Child,
        availableBalance
      ))
      .Select(item => item.Key)
      .ToHashSet();
    var purchasableThemeIds = customization.Themes
      .Where(item => customization.CanPurchaseTheme(
        item.Id,
        member.Role == MemberRole.Child,
        availableBalance
      ))
      .Select(item => item.Id)
      .ToHashSet();
    var availableRoomDesignIds = customization.RoomDesigns
      .Where(item => customization.IsAvailableAtCurrentLevel(item.MinimumLevel))
      .Select(item => item.Id)
      .ToHashSet();
    var purchasableRoomDesignIds = customization.RoomDesigns
      .Where(item => customization.CanPurchaseRoomDesign(
        item.Id,
        member.Role == MemberRole.Child,
        availableBalance
      ))
      .Select(item => item.Id)
      .ToHashSet();
    var capabilities = new HomeCatalogCapabilitiesProjection(
      customization.CanArrange(),
      availableAccessoryIds,
      purchasableAccessoryIds,
      availableAreaIds,
      availableFurnitureSetIds,
      ownedFurnitureSetIds,
      purchasableFurnitureSetIds,
      purchasableThemeIds,
      availableRoomDesignIds,
      purchasableRoomDesignIds
    );

    return new HomeCustomizationReadModelProjection(
      loadedCustomization.Revision,
      customization,
      loadedProgression.Root.StoredLevel,
      capabilityMemberId,
      capabilities
    );
  }
}
