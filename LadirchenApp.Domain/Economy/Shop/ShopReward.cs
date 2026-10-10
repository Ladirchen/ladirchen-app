using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class ShopReward : Entity<ShopRewardId>
{
  internal ShopReward(ShopRewardId id, ShopRewardDefinition definition) : base(id)
  {
    var price = definition.Price;
    var window = definition.Window;
    var quantity = definition.Quantity;
    ArgumentNullException.ThrowIfNull(price);
    ArgumentNullException.ThrowIfNull(window);
    ArgumentOutOfRangeException.ThrowIfNegative(quantity);
    Title = Rules.Text(definition.Title);
    Category = Rules.Defined(definition.Category);
    Price = price;
    Quantity = quantity;
    IsVisible = definition.IsVisible;
    Window = window;
    Status = quantity > 0 ? ShopRewardStatus.Available : ShopRewardStatus.Redeemed;
  }

  public string Title
  {
    get; private set;
  }
  public ShopRewardCategory Category
  {
    get;
  }
  public LadirchenAmount Price
  {
    get; private set;
  }
  public int Quantity
  {
    get; private set;
  }
  public bool IsVisible
  {
    get; private set;
  }
  public AvailabilityWindow Window
  {
    get; private set;
  }
  public ShopRewardStatus Status
  {
    get; private set;
  }
  public bool Deleted
  {
    get; private set;
  }
  public bool IsPublished(DateTimeOffset now, IanaTimeZone zone)
  {

    return !Deleted && IsVisible && Window.Contains(zone.LocalDate(now));
  }

  internal void SetVisibility(bool visible)
  {
    IsVisible = visible;
  }

  internal void Reserve()
  {
    Status = ShopRewardStatus.Requested;
  }

  internal void Release()
  {
    Status = Quantity > 0 ? ShopRewardStatus.Available : ShopRewardStatus.Redeemed;
  }

  internal void Redeem()
  {
    Quantity--;
    Release();
  }
  internal void Delete()
  {
    Deleted = true;
  }

  internal void Update(string title, LadirchenAmount price, int quantity, AvailabilityWindow window)
  {
    Title = title;
    Price = price;
    Quantity = quantity;
    Window = window;
    Release();
  }
}