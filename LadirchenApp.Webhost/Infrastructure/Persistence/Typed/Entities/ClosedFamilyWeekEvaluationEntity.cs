using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class ClosedFamilyWeekEvaluationEntity
{
  public Guid FamilyId { get; set; }
  public int IsoYear { get; set; }
  public int IsoWeek { get; set; }
  public string MemberId { get; set; } = string.Empty;
  public int StreakDays { get; set; }
  public decimal CompletionPercent { get; set; }
  public decimal AverageBasicRating { get; set; }
  public decimal UncappedParticipationValue { get; set; }
  public decimal CappedParticipationValue { get; set; }
  public string LadiStageId { get; set; } = string.Empty;
  public int LadiStagePosition { get; set; }
  public ClosedFamilyWeekEntity ClosedWeek { get; set; } = null!;
}
