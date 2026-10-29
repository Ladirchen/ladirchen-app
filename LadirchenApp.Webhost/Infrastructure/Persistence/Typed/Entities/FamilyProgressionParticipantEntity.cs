using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class FamilyProgressionParticipantEntity
{
  public Guid FamilyId { get; set; }
  public string MemberId { get; set; } = string.Empty;
  public int StoredWeeklyStreak { get; set; }
  public FamilyProgressionRootEntity Progression { get; set; } = null!;
}
