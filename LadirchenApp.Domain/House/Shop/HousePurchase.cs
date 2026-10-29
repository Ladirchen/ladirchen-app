using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed record HousePurchase
{
  public HousePurchase(string contentId, decimal price)
  {
    ContentId = Rules.Text(contentId);
    Rules.Require(price >= 0, DomainError.InvalidValue, "A house purchase price cannot be negative.");
    Price = price;
  }

  public string ContentId { get; }
  public decimal Price { get; }
}
