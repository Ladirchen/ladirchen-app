using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.House.Shop;

public sealed partial class FamilyHouseShop : AggregateRoot<FamilyId>
{
  private readonly HouseShopPolicy _policy = new();
  private readonly HouseLayoutPolicy _layoutPolicy = new();
  private readonly HashSet<string> _unlockedAreas = new(StringComparer.OrdinalIgnoreCase);

  internal FamilyHouseShop()
    : this(new FamilyId(Guid.NewGuid()))
  {
  }

  private FamilyHouseShop(FamilyId id)
    : base(id)
  {
  }

  public int HouseLevel
  {
    get; private set;
  }
  public string SelectedThemeId { get; private set; } = "default";

  internal void Restore(int houseLevel)
  {
    SetCurrentHouseLevel(houseLevel);
  }

  private void SetCurrentHouseLevel(int houseLevel)
  {
    Rules.Require(houseLevel >= 0, DomainError.InvalidValue, "Invalid house-shop state.");
    HouseLevel = houseLevel;
    _unlockedAreas.Clear();

    foreach (var area in HouseShopCatalog.AreaMinimumLevels.Where(item => item.Value <= houseLevel))
    {
      _unlockedAreas.Add(area.Key);
    }
  }

  internal void UnlockArea(string area)
  {
    _unlockedAreas.Add(area);
  }

  public bool IsAvailableAtCurrentLevel(int minimumLevel)
  {
    Rules.Require(minimumLevel >= 0, DomainError.InvalidValue, "A minimum level cannot be negative.");

    return minimumLevel <= HouseLevel;
  }

  internal bool CanArrange(HouseSubscriptionTier subscriptionTier)
  {
    return _policy.CanArrange(subscriptionTier);
  }

  public bool CanArrange()
  {
    return _policy.CanArrange(SubscriptionTier);
  }

  public void ChangeSubscriptionTier(HouseSubscriptionTier subscriptionTier)
  {
    SubscriptionTier = Rules.Defined(subscriptionTier);
  }

  internal bool CanPurchaseAccessory(
    HouseItem item,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    ArgumentNullException.ThrowIfNull(item);
    Rules.Require(availableBalance >= 0, DomainError.InvalidValue, "An available balance cannot be negative.");

    return actorIsChild
      && !item.Owned
      && IsAvailableAtCurrentLevel(item.MinimumLevel)
      && item.Price <= availableBalance;
  }

  internal bool CanPurchaseSet(
    FurnitureSet set,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    ArgumentNullException.ThrowIfNull(set);
    Rules.Require(availableBalance >= 0, DomainError.InvalidValue, "An available balance cannot be negative.");

    return actorIsChild
      && !set.Owned
      && IsAvailableAtCurrentLevel(set.MinimumLevel)
      && set.Price <= availableBalance;
  }

  internal bool CanPurchaseTheme(
    HouseTheme theme,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    ArgumentNullException.ThrowIfNull(theme);
    Rules.Require(availableBalance >= 0, DomainError.InvalidValue, "An available balance cannot be negative.");

    return actorIsChild && !theme.Owned && theme.Price <= availableBalance;
  }

  internal bool CanPurchaseDesign(
    RoomDesign design,
    HouseTheme theme,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    ArgumentNullException.ThrowIfNull(design);
    ArgumentNullException.ThrowIfNull(theme);
    Rules.Require(availableBalance >= 0, DomainError.InvalidValue, "An available balance cannot be negative.");

    return actorIsChild
      && !design.Owned
      && theme.Owned
      && theme.Id == design.ThemeId
      && IsAvailableAtCurrentLevel(design.MinimumLevel)
      && design.Price <= availableBalance;
  }

  internal bool PurchaseAccessory(HouseItem item, bool actorIsChild, decimal availableBalance)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!CanPurchaseAccessory(item, actorIsChild, availableBalance))
    {
      return false;
    }

    item.Acquire(false);

    return true;
  }

  internal bool PurchaseSet(FurnitureSet set, bool actorIsChild, decimal availableBalance)
  {
    ArgumentNullException.ThrowIfNull(set);

    if (!CanPurchaseSet(set, actorIsChild, availableBalance))
    {
      return false;
    }

    foreach (var item in set.Items)
    {
      item.Acquire(false);
    }

    return true;
  }

  internal bool PurchaseTheme(HouseTheme theme, bool actorIsChild, decimal availableBalance)
  {
    ArgumentNullException.ThrowIfNull(theme);

    return PurchaseTheme(theme, theme.Designs, actorIsChild, availableBalance);
  }

  internal bool PurchaseTheme(
    HouseTheme theme,
    IEnumerable<RoomDesign> catalogDesigns,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    ArgumentNullException.ThrowIfNull(theme);
    ArgumentNullException.ThrowIfNull(catalogDesigns);

    if (!CanPurchaseTheme(theme, actorIsChild, availableBalance))
    {
      return false;
    }

    theme.Owned = true;
    ApplyTheme(theme, catalogDesigns);

    return true;
  }

  internal bool SelectTheme(HouseTheme theme)
  {
    ArgumentNullException.ThrowIfNull(theme);

    return SelectTheme(theme, theme.Designs);
  }

  internal bool SelectTheme(HouseTheme theme, IEnumerable<RoomDesign> catalogDesigns)
  {
    ArgumentNullException.ThrowIfNull(theme);
    ArgumentNullException.ThrowIfNull(catalogDesigns);

    if (!theme.Owned)
    {
      return false;
    }

    ApplyTheme(theme, catalogDesigns);

    return true;
  }

  internal bool PurchaseDesign(
    RoomDesign design,
    HouseTheme theme,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    return PurchaseDesign(design, theme, [design], actorIsChild, availableBalance);
  }

  internal bool PurchaseDesign(
    RoomDesign design,
    HouseTheme theme,
    IEnumerable<RoomDesign> catalogDesigns,
    bool actorIsChild,
    decimal availableBalance
  )
  {
    ArgumentNullException.ThrowIfNull(design);
    ArgumentNullException.ThrowIfNull(theme);
    ArgumentNullException.ThrowIfNull(catalogDesigns);

    if (!CanPurchaseDesign(design, theme, actorIsChild, availableBalance))
    {
      return false;
    }

    design.Owned = true;
    SelectRoomDesign(design, catalogDesigns);
    SelectedThemeId = theme.Id;

    return true;
  }

  internal bool SelectDesign(RoomDesign design)
  {
    return SelectDesign(design, [design]);
  }

  internal bool SelectDesign(RoomDesign design, IEnumerable<RoomDesign> catalogDesigns)
  {
    ArgumentNullException.ThrowIfNull(design);
    ArgumentNullException.ThrowIfNull(catalogDesigns);

    if (!design.Owned || design.MinimumLevel > HouseLevel)
    {
      return false;
    }

    SelectRoomDesign(design, catalogDesigns);
    SelectedThemeId = design.ThemeId;

    return true;
  }

  internal bool SelectDesign(
    RoomDesign design,
    HouseTheme theme,
    IEnumerable<RoomDesign> catalogDesigns
  )
  {
    ArgumentNullException.ThrowIfNull(design);
    ArgumentNullException.ThrowIfNull(theme);
    ArgumentNullException.ThrowIfNull(catalogDesigns);

    if (!theme.Owned || theme.Id != design.ThemeId)
    {
      return false;
    }

    return SelectDesign(design, catalogDesigns);
  }

  internal bool Move(HouseItem item, string area, decimal x, decimal y)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!item.Owned || item.Mobility != HouseItemMobility.Movable)
    {
      return false;
    }

    var compatibleArea = CompatibleArea(item, area);

    if (!_unlockedAreas.Contains(compatibleArea))
    {
      return false;
    }

    item.SetPlacement(_layoutPolicy.Normalize("furniture", item.Id, compatibleArea, x, y, item.Placement.Scale), true);

    return true;
  }

  internal bool Store(HouseItem item)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!item.Owned || !item.Equipped || item.Mobility != HouseItemMobility.Movable)
    {
      return false;
    }

    item.SetPlacement(item.Placement, false);

    return true;
  }

  internal bool Place(HouseItem item, string area)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!item.Owned || item.Equipped || item.Mobility != HouseItemMobility.Movable)
    {
      return false;
    }

    var compatibleArea = CompatibleArea(item, area);

    if (!_unlockedAreas.Contains(compatibleArea))
    {
      return false;
    }

    item.SetPlacement(
      _layoutPolicy.Normalize(
        "furniture",
        item.Id,
        compatibleArea,
        item.InitialPlacement.X,
        item.InitialPlacement.Y,
        item.InitialPlacement.Scale
      ),
      true
    );

    return true;
  }

  internal bool Toggle(HouseItem item)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!item.Owned || item.Mobility != HouseItemMobility.Movable)
    {
      return false;
    }

    item.SetPlacement(item.Placement, !item.Equipped);

    return true;
  }

  internal bool Reset(HouseItem item)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (
      !item.Owned
      || item.Mobility != HouseItemMobility.Movable
      || !_unlockedAreas.Contains(item.InitialPlacement.Area)
    )
    {
      return false;
    }

    item.SetPlacement(
      _layoutPolicy.Normalize(
        "furniture",
        item.Id,
        item.InitialPlacement.Area,
        item.InitialPlacement.X,
        item.InitialPlacement.Y,
        item.InitialPlacement.Scale
      ),
      true
    );

    return true;
  }

  internal HousePlacement? ArrangeEntity(
    HouseLayoutEntity entity,
    string area,
    decimal x,
    decimal y,
    HouseSubscriptionTier subscriptionTier,
    HouseItem? furniture,
    IEnumerable<HouseLayoutEntity> entities
  )
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(entities);

    if (!_policy.CanArrange(subscriptionTier) || !IsSupportedEntityType(entity.EntityType))
    {
      return null;
    }

    var targetArea = area;

    if (entity.EntityType == "furniture")
    {
      if (
        furniture is null
        || furniture.Id != entity.EntityId
        || !furniture.Owned
        || furniture.Mobility != HouseItemMobility.Movable
      )
      {
        return null;
      }

      targetArea = CompatibleArea(furniture, area);
    }

    if (!_unlockedAreas.Contains(targetArea))
    {
      return null;
    }

    var normalized = _layoutPolicy.Normalize(
      entity.EntityType,
      entity.EntityId,
      targetArea,
      x,
      y,
      entity.Placement.Scale
    );
    var moved = entity with
    {
      Placement = normalized,
      Active = true
    };

    if (_layoutPolicy.CollidesWithFurniture(moved, entities))
    {
      return null;
    }

    furniture?.SetPlacement(normalized, true);

    return normalized;
  }

  internal HousePlacement? ResetEntity(
    HouseLayoutEntity entity,
    HousePlacement initialPlacement,
    HouseSubscriptionTier subscriptionTier,
    HouseItem? furniture,
    IEnumerable<HouseLayoutEntity> entities
  )
  {
    ArgumentNullException.ThrowIfNull(entity);
    ArgumentNullException.ThrowIfNull(initialPlacement);
    ArgumentNullException.ThrowIfNull(entities);

    if (!_policy.CanArrange(subscriptionTier) || !IsSupportedEntityType(entity.EntityType))
    {
      return null;
    }

    if (
      entity.EntityType == "furniture"
      && (
        furniture is null
        || furniture.Id != entity.EntityId
        || !furniture.Owned
        || furniture.Mobility != HouseItemMobility.Movable
      )
    )
    {
      return null;
    }

    if (!_unlockedAreas.Contains(initialPlacement.Area))
    {
      return null;
    }

    var normalized = _layoutPolicy.Normalize(
      entity.EntityType,
      entity.EntityId,
      initialPlacement.Area,
      initialPlacement.X,
      initialPlacement.Y,
      initialPlacement.Scale
    );
    var reset = entity with
    {
      Placement = normalized,
      Active = true
    };

    if (_layoutPolicy.CollidesWithFurniture(reset, entities))
    {
      return null;
    }

    furniture?.SetPlacement(normalized, true);

    return normalized;
  }

  private void ApplyTheme(HouseTheme theme, IEnumerable<RoomDesign> catalogDesigns)
  {
    SelectedThemeId = theme.Id;

    foreach (var design in theme.Designs)
    {
      design.Owned = true;
      SelectRoomDesign(design, catalogDesigns);
    }
  }

  private static void SelectRoomDesign(RoomDesign design, IEnumerable<RoomDesign> catalogDesigns)
  {
    foreach (var candidate in catalogDesigns.Where(item => item.Room == design.Room))
    {
      candidate.Selected = candidate == design;
    }
  }

  private string CompatibleArea(HouseItem item, string requestedArea)
  {
    return _policy.CompatibleArea(item.IntendedArea, requestedArea, item.AssignedArea);
  }

  private static bool IsSupportedEntityType(string entityType)
  {
    return entityType is "furniture" or "ladi" or "member" or "pet";
  }
}
