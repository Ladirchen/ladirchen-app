using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class FamilyProgressionRootEntity
{
  public Guid FamilyId { get; set; }
  public long Revision { get; set; }
  public string UpdatedBy { get; set; } = string.Empty;
  public DateTimeOffset UpdatedAt { get; set; }
  public int StoredLevel { get; set; }
  public int CompletedWeeklyStreak { get; set; }
  public int CurrentWeekTarget { get; set; }
  public decimal MaximumSavingsInterestRate { get; set; }
  public ICollection<FamilyProgressionParticipantEntity> Participants { get; set; } =
    new List<FamilyProgressionParticipantEntity>();
  public ICollection<ClosedFamilyWeekEntity> ClosedWeeks { get; set; } =
    new List<ClosedFamilyWeekEntity>();
}
