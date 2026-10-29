using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed partial class HouseItem
{
  internal void Restore(bool owned, bool equipped, HousePlacement placement)
  {
    ArgumentNullException.ThrowIfNull(placement);
    Rules.Require(!equipped || owned, DomainError.InvalidState, "Equipped furniture must be owned.");
    Owned = owned;
    Equipped = equipped;
    Placement = placement;
  }
}
