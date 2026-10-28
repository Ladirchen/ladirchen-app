using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed class FurnitureSet
{
  public FurnitureSet(string id, decimal price, int minimumLevel, IEnumerable<HouseItem> items)
  {
    Id = Rules.Text(id);
    Rules.Require(price >= 0, DomainError.InvalidValue, "A set price cannot be negative.");
    Rules.Require(minimumLevel >= 0, DomainError.InvalidValue, "A minimum level cannot be negative.");
    Price = price;
    MinimumLevel = minimumLevel;
    Items = (items ?? throw new ArgumentNullException(nameof(items))).ToArray();
  }

  public string Id { get; }
  public decimal Price { get; }
  public int MinimumLevel { get; }
  public IReadOnlyList<HouseItem> Items { get; }
  public bool Owned
  {
    get
    {
      return Items.Count > 0 && Items.All(item => item.Owned);
    }
  }
}
