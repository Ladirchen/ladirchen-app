using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed partial class HouseTheme
{
  internal void Restore(bool owned)
  {
    Rules.Require(
      owned || Designs.All(item => !item.Owned),
      DomainError.InvalidState,
      "Owned room designs require their house theme."
    );
    Owned = owned;
  }
}
