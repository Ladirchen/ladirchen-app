using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomySettledInterestPeriodEntity
{
  public Guid FamilyId { get; set; }
  public int IsoYear { get; set; }
  public int IsoWeek { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
