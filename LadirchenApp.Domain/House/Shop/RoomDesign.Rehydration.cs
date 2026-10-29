using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed partial class RoomDesign
{
  public void Restore(bool owned, bool selected)
  {
    Rules.Require(!selected || owned, DomainError.InvalidState, "A selected room design must be owned.");
    Owned = owned;
    Selected = selected;
  }
}
