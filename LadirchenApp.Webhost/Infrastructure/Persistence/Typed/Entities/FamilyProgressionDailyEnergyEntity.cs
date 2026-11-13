using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class FamilyProgressionDailyEnergyEntity
{
  public Guid FamilyId { get; set; }
  public DateOnly Date { get; set; }
  public int Energy { get; set; }
  public FamilyProgressionRootEntity Progression { get; set; } = null!;
}
