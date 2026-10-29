using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

internal sealed record RoomDesignState
{
  public RoomDesignState(string id, bool owned, bool selected)
  {
    Id = Rules.Text(id);
    Rules.Require(!selected || owned, DomainError.InvalidState, "A selected room design must be owned.");
    Owned = owned;
    Selected = selected;
  }

  public string Id { get; }
  public bool Owned { get; }
  public bool Selected { get; }
}
