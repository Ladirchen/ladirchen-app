using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House;

internal sealed record ParticipantProgressState
{
  public ParticipantProgressState(
    FamilyMemberId memberId,
    int storedWeeklyStreak
  )
  {
    ArgumentNullException.ThrowIfNull(memberId);
    Rules.Require(
      storedWeeklyStreak >= 0,
      DomainError.InvalidValue,
      "A stored streak cannot be negative."
    );
    MemberId = memberId;
    StoredWeeklyStreak = storedWeeklyStreak;
  }

  public FamilyMemberId MemberId { get; }
  public int StoredWeeklyStreak { get; }
}
