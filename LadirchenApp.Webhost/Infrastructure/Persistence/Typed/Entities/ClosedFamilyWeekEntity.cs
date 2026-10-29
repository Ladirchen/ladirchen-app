using System;
using System.Collections.Generic;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class ClosedFamilyWeekEntity
{
  public Guid FamilyId { get; set; }
  public int IsoYear { get; set; }
  public int IsoWeek { get; set; }
  public int Outcome { get; set; }
  public DateTimeOffset EvaluatedAt { get; set; }
  public FamilyProgressionRootEntity Progression { get; set; } = null!;
  public ICollection<ClosedFamilyWeekEvaluationEntity> MemberEvaluations { get; set; } =
    new List<ClosedFamilyWeekEvaluationEntity>();
}
