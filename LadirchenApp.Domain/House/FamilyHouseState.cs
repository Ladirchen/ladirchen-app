using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House;

internal sealed record FamilyHouseState
{
  public FamilyHouseState(
    FamilyId id,
    int storedLevel,
    int completedWeeklyStreak,
    int currentWeekTarget,
    Percentage maximumSavingsInterestRate,
    IEnumerable<ParticipantProgressState> participants,
    IEnumerable<ClosedFamilyWeekState> closedWeeks
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    ArgumentNullException.ThrowIfNull(maximumSavingsInterestRate);
    ArgumentNullException.ThrowIfNull(participants);
    ArgumentNullException.ThrowIfNull(closedWeeks);
    Id = id;
    StoredLevel = storedLevel;
    CompletedWeeklyStreak = completedWeeklyStreak;
    CurrentWeekTarget = currentWeekTarget;
    MaximumSavingsInterestRate = maximumSavingsInterestRate;
    Participants = participants.ToArray();
    ClosedWeeks = closedWeeks.ToArray();
  }

  public FamilyHouseState(
    FamilyId id,
    int storedLevel,
    int completedWeeklyStreak,
    IEnumerable<ParticipantProgressState> participants,
    IEnumerable<ClosedFamilyWeekState> closedWeeks
  )
    : this(
      id,
      storedLevel,
      completedWeeklyStreak,
      PersonalStreakPolicy.DefaultWeeklyTargetDays,
      SavingsInterestPolicy.Default.MaximumRate,
      participants,
      closedWeeks
    )
  {
  }

  public FamilyId Id { get; }
  public int StoredLevel { get; }
  public int CompletedWeeklyStreak { get; }
  public int CurrentWeekTarget { get; }
  public Percentage MaximumSavingsInterestRate { get; }
  public IReadOnlyList<ParticipantProgressState> Participants { get; }
  public IReadOnlyList<ClosedFamilyWeekState> ClosedWeeks { get; }
}
