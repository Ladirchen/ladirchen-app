using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyPromotionEntity
{
  public Guid FamilyId { get; set; }
  public string PromotionId { get; set; } = string.Empty;
  public string ContributionId { get; set; } = string.Empty;
  public decimal Multiplier { get; set; }
  public decimal TeamworkBonus { get; set; }
  public TimeOnly DeadlineLocalTime { get; set; }
  public DateTime? DeadlineLocalDateTime { get; set; }
  public bool Active { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
