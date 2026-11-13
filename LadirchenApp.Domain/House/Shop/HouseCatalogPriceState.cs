using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed record HouseCatalogPriceState
{
  public HouseCatalogPriceState(
    string catalogType,
    string id,
    decimal price,
    decimal? promotionalPrice = null,
    DateOnly? promotionUntil = null
  )
  {
    CatalogType = Rules.Text(catalogType);
    Id = Rules.Text(id);
    Rules.Require(
      price >= 0 && price <= 10,
      DomainError.InvalidValue,
      "A house-catalog price must be between 0 and 10 Ladirchen."
    );
    Price = price;
    Rules.Require(
      (promotionalPrice is null) == (promotionUntil is null),
      DomainError.InvalidState,
      "A house-catalog promotion requires both a price and an end date."
    );
    Rules.Require(
      promotionalPrice is null || promotionalPrice >= 0 && promotionalPrice < price,
      DomainError.InvalidValue,
      "A promotional price must be lower than the regular price."
    );
    PromotionalPrice = promotionalPrice;
    PromotionUntil = promotionUntil;
  }

  public string CatalogType { get; }
  public string Id { get; }
  public decimal Price { get; }
  public decimal? PromotionalPrice { get; }
  public DateOnly? PromotionUntil { get; }

  public decimal EffectivePrice(DateOnly localDate)
  {
    return PromotionalPrice is not null && PromotionUntil is not null && localDate <= PromotionUntil
      ? PromotionalPrice.Value
      : Price;
  }
}
