using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House;

public sealed class ClosedFamilyWeek
{
  private readonly IReadOnlyDictionary<FamilyMemberId, PersonalLadiEvaluation> _memberEvaluations;

  public ClosedFamilyWeek(
    IsoWeekPeriod period,
    FamilyWeekOutcome outcome,
    DateTimeOffset evaluatedAt,
    IEnumerable<PersonalLadiEvaluation> memberEvaluations
  )
  {
    ArgumentNullException.ThrowIfNull(period);
    ArgumentNullException.ThrowIfNull(memberEvaluations);
    Rules.Defined(outcome);
    var evaluations = memberEvaluations.ToArray();
    Rules.Require(
      evaluations.Select(item => item.MemberId).Distinct().Count() == evaluations.Length,
      DomainError.Duplicate,
      "A closed family week can contain only one evaluation per member."
    );

    foreach (var evaluation in evaluations)
    {
      Rules.Require(
        evaluation.IsoYear == period.Year && evaluation.IsoWeek == period.Week,
        DomainError.InvalidValue,
        "Every Ladi evaluation must belong to the closed family week."
      );
    }

    Period = period;
    Outcome = outcome;
    EvaluatedAt = evaluatedAt.ToUniversalTime();
    _memberEvaluations = evaluations.ToDictionary(item => item.MemberId);
  }

  public IsoWeekPeriod Period { get; }
  public FamilyWeekOutcome Outcome { get; }
  public DateTimeOffset EvaluatedAt { get; }
  public IReadOnlyDictionary<FamilyMemberId, PersonalLadiEvaluation> MemberEvaluations
  {
    get
    {
      return _memberEvaluations;
    }
  }
}
