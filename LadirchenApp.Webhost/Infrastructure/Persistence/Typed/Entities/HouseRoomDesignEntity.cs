using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class HouseRoomDesignEntity
{
  public Guid FamilyId { get; set; }
  public string RoomDesignId { get; set; } = string.Empty;
  public bool Owned { get; set; }
  public bool Selected { get; set; }
  public HouseCustomizationRootEntity HouseCustomization { get; set; } = null!;
}
