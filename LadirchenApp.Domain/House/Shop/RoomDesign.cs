using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed partial class RoomDesign
{
  public RoomDesign(string id, string themeId, string room, decimal price, int minimumLevel)
  {
    Id = Rules.Text(id);
    ThemeId = Rules.Text(themeId);
    Room = Rules.Text(room);
    Rules.Require(price >= 0, DomainError.InvalidValue, "A room-design price cannot be negative.");
    Rules.Require(minimumLevel >= 0, DomainError.InvalidValue, "A minimum level cannot be negative.");
    Price = price;
    MinimumLevel = minimumLevel;
  }

  public string Id { get; }
  public string ThemeId { get; }
  public string Room { get; }
  public decimal Price { get; }
  public int MinimumLevel { get; }
  public bool Owned { get; internal set; }
  public bool Selected { get; internal set; }
}
