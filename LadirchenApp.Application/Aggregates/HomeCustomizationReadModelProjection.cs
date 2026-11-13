using System;

using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record HomeCustomizationReadModelProjection(
  long Revision,
  FamilyHouseShop Customization,
  int HouseLevel,
  DateOnly CatalogDate,
  FamilyMemberId CapabilityMemberId,
  HomeCatalogCapabilitiesProjection Capabilities
) : IFamilyReadModelProjection
{
  public string ReadModelType
  {
    get
    {
      return "home-customization";
    }
  }
}
