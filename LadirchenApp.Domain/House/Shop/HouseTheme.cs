using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed class HouseTheme
{
  public HouseTheme(string id, decimal price, IEnumerable<RoomDesign> designs)
  {
    Id = Rules.Text(id);
    Rules.Require(price >= 0, DomainError.InvalidValue, "A theme price cannot be negative.");
    Price = price;
    Designs = (designs ?? throw new ArgumentNullException(nameof(designs))).ToArray();
  }

  public string Id { get; }
  public decimal Price { get; }
  public IReadOnlyList<RoomDesign> Designs { get; }
  public bool Owned { get; internal set; }
}
