using System;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed;

internal static class HouseCustomizationPersistenceMapper
{
  public static FamilyHouseShop ToDomain(
    HouseCustomizationRootEntity entity,
    Family family,
    int currentHouseLevel
  )
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(family);
    var state = new FamilyHouseShopState(
      new FamilyId(entity.FamilyId),
      entity.SelectedThemeId,
      (HouseSubscriptionTier)entity.SubscriptionTier,
      entity.Accessories
        .OrderBy(item => item.AccessoryId, StringComparer.Ordinal)
        .Select(item => new HouseItemState(item.AccessoryId, item.Owned, item.Equipped)),
      entity.Themes
        .OrderBy(item => item.ThemeId, StringComparer.Ordinal)
        .Select(item => new HouseThemeState(item.ThemeId, item.Owned)),
      entity.RoomDesigns
        .OrderBy(item => item.RoomDesignId, StringComparer.Ordinal)
        .Select(item => new RoomDesignState(item.RoomDesignId, item.Owned, item.Selected)),
      entity.Placements
        .OrderBy(item => item.PlacementId, StringComparer.Ordinal)
        .Select(item => new HouseLayoutEntityState(
          item.PlacementId,
          item.EntityType,
          item.EntityId,
          new HousePlacement(item.Area, item.X, item.Y, item.Scale)
        ))
    );

    return FamilyHouseShop.Reconstitute(family, currentHouseLevel, state);
  }

  public static HouseCustomizationRootEntity CreateEntity(FamilyHouseShop house)
  {
    ArgumentNullException.ThrowIfNull(house);
    var entity = new HouseCustomizationRootEntity();
    Apply(entity, house);

    return entity;
  }

  public static void Apply(HouseCustomizationRootEntity entity, FamilyHouseShop house)
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(house);
    var state = house.CaptureState();
    entity.FamilyId = state.Id.Value;
    entity.SelectedThemeId = state.SelectedThemeId;
    entity.SubscriptionTier = (int)state.SubscriptionTier;
    entity.Accessories.Clear();
    entity.Themes.Clear();
    entity.RoomDesigns.Clear();
    entity.Placements.Clear();

    foreach (var accessory in state.Accessories)
    {
      entity.Accessories.Add(new HouseAccessoryEntity
      {
        FamilyId = state.Id.Value,
        AccessoryId = accessory.Id,
        Owned = accessory.Owned,
        Equipped = accessory.Equipped,
      });
    }

    foreach (var theme in state.Themes)
    {
      entity.Themes.Add(new HouseThemeEntity
      {
        FamilyId = state.Id.Value,
        ThemeId = theme.Id,
        Owned = theme.Owned,
      });
    }

    foreach (var roomDesign in state.RoomDesigns)
    {
      entity.RoomDesigns.Add(new HouseRoomDesignEntity
      {
        FamilyId = state.Id.Value,
        RoomDesignId = roomDesign.Id,
        Owned = roomDesign.Owned,
        Selected = roomDesign.Selected,
      });
    }

    foreach (var placement in state.Placements)
    {
      entity.Placements.Add(new HousePlacementEntity
      {
        FamilyId = state.Id.Value,
        PlacementId = placement.Id,
        EntityType = placement.EntityType,
        EntityId = placement.EntityId,
        Area = placement.Placement.Area,
        X = placement.Placement.X,
        Y = placement.Placement.Y,
        Scale = placement.Placement.Scale,
      });
    }
  }
}
