using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Ladi;

public sealed record PersonalLadiEvaluation
{
  public PersonalLadiEvaluation(
    FamilyMemberId memberId,
    int isoYear,
    int isoWeek,
    PersonalParticipation participation,
    ParticipationValue value,
    LadiStage stage
  )
  {
    ArgumentNullException.ThrowIfNull(memberId);
    ArgumentNullException.ThrowIfNull(participation);
    ArgumentNullException.ThrowIfNull(value);
    ArgumentNullException.ThrowIfNull(stage);
    Rules.Require(isoWeek >= 1 && isoWeek <= 53, DomainError.InvalidValue, "Invalid ISO week.");
    MemberId = memberId;
    IsoYear = isoYear;
    IsoWeek = isoWeek;
    Participation = participation;
    Value = value;
    Stage = stage;
  }

  public FamilyMemberId MemberId { get; }
  public int IsoYear { get; }
  public int IsoWeek { get; }
  public PersonalParticipation Participation { get; }
  public ParticipationValue Value { get; }
  public LadiStage Stage { get; }
}
