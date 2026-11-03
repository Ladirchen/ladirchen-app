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
    get; private set;
  }
  public LadirchenAmount Price
  {
    get; private set;
  }
  public LadirchenAmount? PromotionalPrice
  {
    get; private set;
  }
  public DateOnly? PromotionUntil
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
    return PublicationStatus(zone.LocalDate(now)) == ShopRewardPublicationStatus.Visible;
  }

  public bool PromotionIsActive(DateOnly localDate)
  {
    return PromotionalPrice is not null && PromotionUntil is not null && localDate <= PromotionUntil;
  }

  public LadirchenAmount EffectivePrice(DateOnly localDate)
  {
    return PromotionIsActive(localDate) ? PromotionalPrice! : Price;
  }

  public ShopRewardPublicationStatus PublicationStatus(DateOnly localDate)
  {
    if (Deleted || !IsVisible)
    {
      return ShopRewardPublicationStatus.Hidden;
    }

    if (Window.From is not null && localDate < Window.From)
    {
      return ShopRewardPublicationStatus.Scheduled;
    }

    return Window.Until is not null && localDate > Window.Until
      ? ShopRewardPublicationStatus.Expired
      : ShopRewardPublicationStatus.Visible;
  }

  public DateOnly? NextPublicationChangeDate(DateOnly localDate)
  {
    if (Window.From is not null && localDate < Window.From)
    {
      return Window.From;
    }

    return Window.Until is not null && localDate <= Window.Until
      ? Window.Until.Value.AddDays(1)
      : null;
  }

  internal void SetVisibility(bool visible)
  {
    IsVisible = visible;
  }

  internal void SetPromotion(LadirchenAmount? price, DateOnly? until)
  {
    Rules.Require(
      (price is null) == (until is null),
      DomainError.InvalidState,
      "A shop promotion requires both a price and an end date."
    );
    Rules.Require(
      price is null || price.Value < Price.Value,
      DomainError.InvalidState,
      "A promotional price must be lower than the regular price."
    );
    PromotionalPrice = price;
    PromotionUntil = until;
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
  internal void Republish()
  {
    Quantity = Math.Max(1, Quantity);
    IsVisible = true;
    Window = new AvailabilityWindow();
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

  internal void Update(ShopRewardDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    Title = Rules.Text(definition.Title);
    Category = Rules.Defined(definition.Category);
    Price = definition.Price ?? throw new ArgumentNullException(nameof(definition));
    ArgumentOutOfRangeException.ThrowIfNegative(definition.Quantity);
    Quantity = definition.Quantity;
    IsVisible = definition.IsVisible;
    Window = definition.Window ?? throw new ArgumentNullException(nameof(definition));
    if (PromotionalPrice?.Value >= Price.Value)
    {
      SetPromotion(null, null);
    }
    Release();
  }

  internal void Restore(ShopRewardStatus status, bool deleted)
  {
    Rules.Defined(status);

    switch (status)
    {
      case ShopRewardStatus.Available:
        Rules.Require(Quantity > 0, DomainError.InvalidState, "An available reward requires positive stock.");
        break;
      case ShopRewardStatus.Requested:
        Rules.Require(Quantity > 0, DomainError.InvalidState, "A requested reward requires positive stock.");
        Reserve();
        break;
      case ShopRewardStatus.Redeemed:
        Rules.Require(Quantity == 0, DomainError.InvalidState, "A redeemed reward must have no remaining stock.");
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(status));
    }

    Rules.Require(
      !deleted || status != ShopRewardStatus.Requested,
      DomainError.InvalidState,
      "A deleted reward cannot retain a pending request."
    );
    Deleted = deleted;
  }
}
