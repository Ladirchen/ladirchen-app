using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class FamilyRootEntity
{
  public Guid FamilyId { get; set; }
  public long Revision { get; set; }
  public string UpdatedBy { get; set; } = string.Empty;
  public DateTimeOffset UpdatedAt { get; set; }
  public string TimeZone { get; set; } = string.Empty;
  public int Currency { get; set; }
  public int LadirchenPerCurrencyUnit { get; set; }
  public string? ActiveChildId { get; set; }
  public bool OnboardingCompleted { get; set; }
  public ICollection<FamilyMemberEntity> Members { get; set; } = new List<FamilyMemberEntity>();
  public ICollection<FamilyPetEntity> Pets { get; set; } = new List<FamilyPetEntity>();
}
