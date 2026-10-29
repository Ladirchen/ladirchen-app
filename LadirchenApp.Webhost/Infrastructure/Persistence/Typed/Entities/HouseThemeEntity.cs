using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class HouseThemeEntity
{
  public Guid FamilyId { get; set; }
  public string ThemeId { get; set; } = string.Empty;
  public bool Owned { get; set; }
  public HouseCustomizationRootEntity HouseCustomization { get; set; } = null!;
}
