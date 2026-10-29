using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class HouseCustomizationRootEntity
{
  public Guid FamilyId { get; set; }
  public long Revision { get; set; }
  public string UpdatedBy { get; set; } = string.Empty;
  public DateTimeOffset UpdatedAt { get; set; }
  public string SelectedThemeId { get; set; } = string.Empty;
  public int SubscriptionTier { get; set; }
  public ICollection<HouseAccessoryEntity> Accessories { get; set; } = new List<HouseAccessoryEntity>();
  public ICollection<HouseThemeEntity> Themes { get; set; } = new List<HouseThemeEntity>();
  public ICollection<HouseRoomDesignEntity> RoomDesigns { get; set; } = new List<HouseRoomDesignEntity>();
  public ICollection<HousePlacementEntity> Placements { get; set; } = new List<HousePlacementEntity>();
}
