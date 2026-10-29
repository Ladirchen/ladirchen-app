using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed partial class FamilyHouseShop
{
  public IReadOnlyCollection<HouseItem> Accessories
  {
    get
    {
      return _stateAccessories.Values;
    }
  }

  public IReadOnlyCollection<HouseTheme> Themes
  {
    get
    {
      return _stateThemes.Values;
    }
  }

  public IReadOnlyCollection<RoomDesign> RoomDesigns
  {
    get
    {
      return _stateRoomDesigns.Values;
    }
  }

  public IReadOnlyCollection<HouseLayoutEntity> Placements
  {
    get
    {
      return _statePlacements.Values;
    }
  }

  public bool CanPurchaseAccessory(
    string accessoryId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    return CanPurchaseAccessory(
      RequireAccessory(accessoryId),
      actorIsChild,
      availableBalance
    );
  }

  public bool CanPurchaseFurnitureSet(
    string furnitureSetId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    return CanPurchaseSet(
      RequireFurnitureSet(furnitureSetId),
      actorIsChild,
      availableBalance
    );
  }

  public bool CanPurchaseTheme(
    string themeId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    return CanPurchaseTheme(RequireTheme(themeId), actorIsChild, availableBalance);
  }

  public bool CanPurchaseRoomDesign(
    string designId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    var design = RequireRoomDesign(designId);

    return CanPurchaseDesign(
      design,
      RequireTheme(design.ThemeId),
      actorIsChild,
      availableBalance
    );
  }

  public bool PurchaseAccessory(string accessoryId, bool actorIsChild, decimal availableBalance)
  {
    var accessory = RequireAccessory(accessoryId);
    var purchased = PurchaseAccessory(accessory, actorIsChild, availableBalance);

    if (purchased)
    {
      SynchronizeAccessoryPlacement(accessory);
    }

    return purchased;
  }

  public HousePurchase? PurchaseAccessoryContent(
    string accessoryId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    var accessory = RequireAccessory(accessoryId);

    return PurchaseAccessory(accessoryId, actorIsChild, availableBalance)
      ? new HousePurchase(accessory.Id, accessory.Price)
      : null;
  }

  public bool PurchaseFurnitureSet(
    string furnitureSetId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    var set = RequireFurnitureSet(furnitureSetId);
    var purchased = PurchaseSet(set, actorIsChild, availableBalance);

    if (purchased)
    {
      foreach (var accessory in set.Items)
      {
        SynchronizeAccessoryPlacement(accessory);
      }
    }

    return purchased;
  }

  public HousePurchase? PurchaseFurnitureSetContent(
    string furnitureSetId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    var set = RequireFurnitureSet(furnitureSetId);

    return PurchaseFurnitureSet(furnitureSetId, actorIsChild, availableBalance)
      ? new HousePurchase(set.Id, set.Price)
      : null;
  }

  public bool MoveAccessory(string accessoryId, string area, decimal x, decimal y)
  {
    var accessory = RequireAccessory(accessoryId);
    var moved = Move(accessory, area, x, y);

    if (moved)
    {
      SynchronizeAccessoryPlacement(accessory);
    }

    return moved;
  }

  public bool StoreAccessory(string accessoryId)
  {
    var accessory = RequireAccessory(accessoryId);
    var stored = Store(accessory);

    if (stored)
    {
      SynchronizeAccessoryPlacement(accessory);
    }

    return stored;
  }

  public bool PlaceAccessory(string accessoryId, string area)
  {
    var accessory = RequireAccessory(accessoryId);
    var placed = Place(accessory, area);

    if (placed)
    {
      SynchronizeAccessoryPlacement(accessory);
    }

    return placed;
  }

  public bool ToggleAccessory(string accessoryId)
  {
    var accessory = RequireAccessory(accessoryId);
    var toggled = Toggle(accessory);

    if (toggled)
    {
      SynchronizeAccessoryPlacement(accessory);
    }

    return toggled;
  }

  public bool ResetAccessory(string accessoryId)
  {
    var accessory = RequireAccessory(accessoryId);
    var reset = Reset(accessory);

    if (reset)
    {
      SynchronizeAccessoryPlacement(accessory);
    }

    return reset;
  }

  public bool PurchaseTheme(string themeId, bool actorIsChild, decimal availableBalance)
  {
    return PurchaseTheme(
      RequireTheme(themeId),
      _stateRoomDesigns.Values,
      actorIsChild,
      availableBalance
    );
  }

  public HousePurchase? PurchaseThemeContent(
    string themeId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    var theme = RequireTheme(themeId);

    return PurchaseTheme(themeId, actorIsChild, availableBalance)
      ? new HousePurchase(theme.Id, theme.Price)
      : null;
  }

  public bool SelectTheme(string themeId)
  {
    return SelectTheme(RequireTheme(themeId), _stateRoomDesigns.Values);
  }

  public bool PurchaseRoomDesign(
    string designId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    var design = RequireRoomDesign(designId);

    return PurchaseDesign(
      design,
      RequireTheme(design.ThemeId),
      _stateRoomDesigns.Values,
      actorIsChild,
      availableBalance
    );
  }

  public HousePurchase? PurchaseRoomDesignContent(
    string designId,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    var design = RequireRoomDesign(designId);

    return PurchaseRoomDesign(designId, actorIsChild, availableBalance)
      ? new HousePurchase(design.Id, design.Price)
      : null;
  }

  public bool SelectRoomDesign(string designId)
  {
    var design = RequireRoomDesign(designId);

    return SelectDesign(
      design,
      RequireTheme(design.ThemeId),
      _stateRoomDesigns.Values
    );
  }

  public HousePlacement? ArrangeEntity(
    string placementId,
    string area,
    decimal x,
    decimal y
  )
  {
    var entity = RequirePlacement(placementId);
    var furniture = FurnitureFor(entity);
    var placement = ArrangeEntity(
      entity,
      area,
      x,
      y,
      SubscriptionTier,
      furniture,
      _statePlacements.Values
    );

    if (placement is null)
    {
      return null;
    }

    _statePlacements[entity.Id] = entity with { Placement = placement, Active = true };

    if (furniture is not null)
    {
      SynchronizeAccessoryPlacement(furniture);
    }

    return placement;
  }

  public HousePlacement? ResetEntity(string placementId)
  {
    var entity = RequirePlacement(placementId);

    if (!_initialPlacements.TryGetValue(placementId, out var initialPlacement))
    {
      return null;
    }

    var furniture = FurnitureFor(entity);
    var placement = ResetEntity(
      entity,
      initialPlacement,
      SubscriptionTier,
      furniture,
      _statePlacements.Values
    );

    if (placement is null)
    {
      return null;
    }

    _statePlacements[entity.Id] = entity with { Placement = placement, Active = true };

    if (furniture is not null)
    {
      SynchronizeAccessoryPlacement(furniture);
    }

    return placement;
  }

  private HouseItem RequireAccessory(string id)
  {
    var normalized = Rules.Text(id);

    return _stateAccessories.GetValueOrDefault(normalized)
      ?? throw new DomainRuleException(DomainError.NotFound, "The house accessory is unknown.");
  }

  private FurnitureSet RequireFurnitureSet(string id)
  {
    var normalized = Rules.Text(id);
    Rules.Require(
      HouseShopCatalog.FurnitureSets.TryGetValue(normalized, out var definition),
      DomainError.NotFound,
      "The furniture set is unknown."
    );

    return new FurnitureSet(
      normalized,
      definition.Price,
      definition.MinimumLevel,
      definition.AccessoryIds.Select(RequireAccessory)
    );
  }

  private HouseTheme RequireTheme(string id)
  {
    var normalized = Rules.Text(id);

    return _stateThemes.GetValueOrDefault(normalized)
      ?? throw new DomainRuleException(DomainError.NotFound, "The house theme is unknown.");
  }

  private RoomDesign RequireRoomDesign(string id)
  {
    var normalized = Rules.Text(id);

    return _stateRoomDesigns.GetValueOrDefault(normalized)
      ?? throw new DomainRuleException(DomainError.NotFound, "The room design is unknown.");
  }

  private HouseLayoutEntity RequirePlacement(string id)
  {
    var normalized = Rules.Text(id);

    return _statePlacements.GetValueOrDefault(normalized)
      ?? throw new DomainRuleException(DomainError.NotFound, "The house placement is unknown.");
  }

  private HouseItem? FurnitureFor(HouseLayoutEntity entity)
  {
    if (entity.EntityType != "furniture")
    {
      return null;
    }

    return RequireAccessory(entity.EntityId);
  }

  private void SynchronizeAccessoryPlacement(HouseItem accessory)
  {
    var entity = _statePlacements.Values.Single(item =>
      item.EntityType == "furniture" && item.EntityId == accessory.Id
    );
    _statePlacements[entity.Id] = entity with
    {
      Placement = accessory.Placement,
      Active = accessory.Owned && accessory.Equipped,
    };
  }
}
