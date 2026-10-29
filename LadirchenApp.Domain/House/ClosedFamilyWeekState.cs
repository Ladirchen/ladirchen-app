using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House;

internal sealed record ClosedFamilyWeekState
{
  public ClosedFamilyWeekState(
    IsoWeekPeriod period,
    FamilyWeekOutcome outcome,
    DateTimeOffset evaluatedAt,
    IEnumerable<PersonalLadiEvaluation> memberEvaluations
  )
  {
    ArgumentNullException.ThrowIfNull(period);
    ArgumentNullException.ThrowIfNull(memberEvaluations);
    Period = period;
    Outcome = Rules.Defined(outcome);
    EvaluatedAt = evaluatedAt.ToUniversalTime();
    MemberEvaluations = memberEvaluations.ToArray();
  }

  public IsoWeekPeriod Period { get; }
  public FamilyWeekOutcome Outcome { get; }
  public DateTimeOffset EvaluatedAt { get; }
  public IReadOnlyList<PersonalLadiEvaluation> MemberEvaluations { get; }
}
