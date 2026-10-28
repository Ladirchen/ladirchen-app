using System;

namespace LadirchenApp.Domain.House.Shop;

public sealed class HouseShopPolicy
{
  public HousePlacement NormalizePlacement(string area, decimal x, decimal y, decimal scale = 1)
  {
    return new HousePlacement(
      area,
      decimal.Round(Math.Clamp(x, 4m, 96m), 2, MidpointRounding.AwayFromZero),
      decimal.Round(Math.Clamp(y, 8m, 94m), 2, MidpointRounding.AwayFromZero),
      scale
    );
  }

  public string CompatibleArea(string intendedArea, string requestedArea)
  {
    if (intendedArea == "outside" && requestedArea != "garden")
    {
      return "garden";
    }

    if (intendedArea == "inside" && requestedArea == "garden")
    {
      return "its assigned room";
    }

    return requestedArea;
  }

  public string CatalogArea(HouseItemCategory category)
  {
    return category switch
    {
      HouseItemCategory.Furniture => "inside",
      HouseItemCategory.Garden => "outside",
      HouseItemCategory.Special => "special",
      _ => throw new ArgumentOutOfRangeException(nameof(category)),
    };
  }

  public bool IsNew(DateOnly? start, DateOnly? end, DateTime localDateTime)
  {
    if (start is null || end is null)
    {
      return false;
    }

    var date = DateOnly.FromDateTime(localDateTime);

    return date >= start && date <= end;
  }
}
