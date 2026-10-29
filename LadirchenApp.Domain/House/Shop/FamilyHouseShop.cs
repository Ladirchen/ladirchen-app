using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed class FamilyHouseShop
{
  private readonly HouseShopPolicy _policy = new();
  private readonly HouseLayoutPolicy _layoutPolicy = new();
  private readonly HashSet<string> _unlockedAreas = new(StringComparer.OrdinalIgnoreCase);

  public decimal Balance { get; private set; }
  public int HouseLevel { get; private set; }
  public string SelectedThemeId { get; private set; } = "default";

  public void Restore(decimal balance, int houseLevel)
  {
    Rules.Require(balance >= 0 && houseLevel >= 0, DomainError.InvalidValue, "Invalid house-shop state.");
    Balance = balance;
    HouseLevel = houseLevel;
  }

  public void UnlockArea(string area)
  {
    _unlockedAreas.Add(area);
  }

  public bool PurchaseAccessory(HouseItem item, bool actorIsChild)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!actorIsChild || item.Owned || item.Price > Balance)
    {
      return false;
    }

    Balance -= item.Price;
    item.Acquire(false);

    return true;
  }

  public bool PurchaseSet(FurnitureSet set, bool actorIsChild)
  {
    ArgumentNullException.ThrowIfNull(set);

    if (!actorIsChild || set.Owned || set.MinimumLevel > HouseLevel || set.Price > Balance)
    {
      return false;
    }

    Balance -= set.Price;

    foreach (var item in set.Items)
    {
      item.Acquire(false);
    }

    return true;
  }

  public bool PurchaseTheme(HouseTheme theme, bool actorIsChild)
  {
    ArgumentNullException.ThrowIfNull(theme);

    if (!actorIsChild || theme.Owned || theme.Price > Balance)
    {
      return false;
    }

    Balance -= theme.Price;
    theme.Owned = true;
    ApplyTheme(theme);

    return true;
  }

  public bool SelectTheme(HouseTheme theme)
  {
    ArgumentNullException.ThrowIfNull(theme);

    if (!theme.Owned)
    {
      return false;
    }

    ApplyTheme(theme);

    return true;
  }

  public bool PurchaseDesign(RoomDesign design, HouseTheme theme, bool actorIsChild)
  {
    ArgumentNullException.ThrowIfNull(design);
    ArgumentNullException.ThrowIfNull(theme);

    if (!actorIsChild || design.Owned || !theme.Owned || design.MinimumLevel > HouseLevel || design.Price > Balance)
    {
      return false;
    }

    Balance -= design.Price;
    design.Owned = true;
    design.Selected = true;
    SelectedThemeId = theme.Id;

    return true;
  }

  public bool SelectDesign(RoomDesign design)
  {
    ArgumentNullException.ThrowIfNull(design);

    if (!design.Owned || design.MinimumLevel > HouseLevel)
    {
      return false;
    }

    design.Selected = true;
    SelectedThemeId = design.ThemeId;

    return true;
  }

  public bool Move(HouseItem item, string area, decimal x, decimal y)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!item.Owned || item.Mobility != HouseItemMobility.Movable || !_unlockedAreas.Contains(area))
    {
      return false;
    }

    var compatibleArea = _policy.CompatibleArea(item.IntendedArea, area);
    item.SetPlacement(_layoutPolicy.Normalize("furniture", item.Id, compatibleArea, x, y, item.Placement.Scale), true);

    return true;
  }

  public bool Store(HouseItem item)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!item.Owned || item.Mobility != HouseItemMobility.Movable)
    {
      return false;
    }

    item.SetPlacement(item.Placement, false);

    return true;
  }

  public bool Place(HouseItem item, string area)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (item.Equipped || !_unlockedAreas.Contains(area))
    {
      return false;
    }

    return Move(item, area, item.InitialPlacement.X, item.InitialPlacement.Y);
  }

  public bool Reset(HouseItem item)
  {
    ArgumentNullException.ThrowIfNull(item);

    if (!item.Owned || item.Mobility != HouseItemMobility.Movable)
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

  private void ApplyTheme(HouseTheme theme)
  {
    SelectedThemeId = theme.Id;

    foreach (var design in theme.Designs)
    {
      design.Owned = true;
      design.Selected = true;
    }
  }
}
