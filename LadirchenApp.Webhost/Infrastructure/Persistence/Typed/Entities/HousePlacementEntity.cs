using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class HousePlacementEntity
{
  public Guid FamilyId { get; set; }
  public string PlacementId { get; set; } = string.Empty;
  public string EntityType { get; set; } = string.Empty;
  public string EntityId { get; set; } = string.Empty;
  public string Area { get; set; } = string.Empty;
  public decimal X { get; set; }
  public decimal Y { get; set; }
  public decimal Scale { get; set; }
  public HouseCustomizationRootEntity HouseCustomization { get; set; } = null!;
}
