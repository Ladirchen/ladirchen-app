using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class HouseAccessoryEntity
{
  public Guid FamilyId { get; set; }
  public string AccessoryId { get; set; } = string.Empty;
  public bool Owned { get; set; }
  public bool Equipped { get; set; }
  public HouseCustomizationRootEntity HouseCustomization { get; set; } = null!;
}
