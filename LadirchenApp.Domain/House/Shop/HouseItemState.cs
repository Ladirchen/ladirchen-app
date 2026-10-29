using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

internal sealed record HouseItemState
{
  public HouseItemState(string id, bool owned, bool equipped)
  {
    Id = Rules.Text(id);
    Rules.Require(!equipped || owned, DomainError.InvalidState, "Equipped furniture must be owned.");
    Owned = owned;
    Equipped = equipped;
  }

  public string Id { get; }
  public bool Owned { get; }
  public bool Equipped { get; }
}
