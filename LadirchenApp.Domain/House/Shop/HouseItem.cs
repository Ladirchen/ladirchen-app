using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed class HouseItem
{
  public HouseItem(
    string id,
    decimal price,
    HouseItemCategory category,
    HouseItemMobility mobility,
    string intendedArea,
    HousePlacement initialPlacement
  )
  {
    Id = Rules.Text(id);
    Rules.Require(price >= 0, DomainError.InvalidValue, "A house-item price cannot be negative.");
    Price = price;
    Category = Rules.Defined(category);
    Mobility = Rules.Defined(mobility);
    IntendedArea = Rules.Text(intendedArea);
    InitialPlacement = initialPlacement ?? throw new ArgumentNullException(nameof(initialPlacement));
    Placement = initialPlacement;
  }

  public string Id { get; }
  public decimal Price { get; }
  public HouseItemCategory Category { get; }
  public HouseItemMobility Mobility { get; }
  public string IntendedArea { get; }
  public HousePlacement InitialPlacement { get; }
  public HousePlacement Placement { get; private set; }
  public bool Owned { get; private set; }
  public bool Equipped { get; private set; }

  internal void Acquire(bool equipped)
  {
    Owned = true;
    Equipped = equipped;
  }

  internal void SetPlacement(HousePlacement placement, bool equipped)
  {
    Placement = placement;
    Equipped = equipped;
  }
}
