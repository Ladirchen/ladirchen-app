using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

internal sealed record HouseThemeState
{
  public HouseThemeState(string id, bool owned)
  {
    Id = Rules.Text(id);
    Owned = owned;
  }

  public string Id { get; }
  public bool Owned { get; }
}
