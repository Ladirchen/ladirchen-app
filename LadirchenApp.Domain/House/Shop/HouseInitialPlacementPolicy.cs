using System;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House.Shop;

public sealed class HouseInitialPlacementPolicy
{
  public HousePlacement FamilyLadi()
  {
    return new HousePlacement("living-room", 18, 30, 0.78m);
  }

  public HousePlacement FamilyMember(int index, MemberRole role)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(index);
    var scale = role == MemberRole.Guardian ? 0.78m : 0.75m;

    return new HousePlacement("living-room", Math.Min(75, 35 + index * 10), 72, scale);
  }

  public HousePlacement FamilyPet(int index)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(index);

    return new HousePlacement("living-room", Math.Min(88, 18 + index * 14), 80, 0.72m);
  }

  public HousePlacement Accessory(string id)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(id);

    return HouseShopCatalog.Accessories.TryGetValue(id, out var definition)
      ? definition.Initial
      : throw new DomainRuleException(DomainError.NotFound, "The house accessory is unknown.");
  }
}
