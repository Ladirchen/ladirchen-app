using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House;

public sealed class FamilyHouse
{
  private readonly Dictionary<FamilyMemberId, ParticipantProgress> _participants = new();

  public const int MaximumStoredLevel = 4;

  public int StoredLevel { get; private set; }
  public int CompletedWeeklyStreak { get; private set; }
  public string Theme { get; private set; } = "default";

  public void RegisterParticipant(FamilyMemberId memberId, bool participates, int storedWeeklyStreak)
  {
    ArgumentNullException.ThrowIfNull(memberId);
    Rules.Require(storedWeeklyStreak >= 0, DomainError.InvalidValue, "A stored streak cannot be negative.");
    _participants[memberId] = new ParticipantProgress(participates, storedWeeklyStreak);
  }

  public int StoredWeeklyStreakFor(FamilyMemberId memberId)
  {
    ArgumentNullException.ThrowIfNull(memberId);

    return _participants.TryGetValue(memberId, out var progress) ? progress.StoredWeeklyStreak : 0;
  }

  public void Restore(int storedLevel, int completedWeeklyStreak)
  {
    Rules.Require(storedLevel >= 0 && storedLevel <= MaximumStoredLevel, DomainError.InvalidValue, "Invalid house level.");
    Rules.Require(completedWeeklyStreak >= 0, DomainError.InvalidValue, "The family streak cannot be negative.");
    StoredLevel = storedLevel;
    CompletedWeeklyStreak = completedWeeklyStreak;
  }

  public bool CloseSuccessfulWeek(int energy)
  {
    if (energy < 60)
    {
      return false;
    }

    StoredLevel = Math.Min(MaximumStoredLevel, StoredLevel + 1);
    CompletedWeeklyStreak++;
    SynchronizeParticipantStreaks();

    return true;
  }

  public void CloseFailedWeek(int energy)
  {
    Rules.Require(energy < 60, DomainError.InvalidState, "A successful week cannot be closed as failed.");
    StoredLevel = Math.Max(0, StoredLevel - 1);

    if (energy < 30)
    {
      CompletedWeeklyStreak = Math.Max(0, CompletedWeeklyStreak - 1);
    }

    SynchronizeParticipantStreaks();
  }

  private void SynchronizeParticipantStreaks()
  {
    foreach (var item in _participants)
    {
      if (item.Value.Participates)
      {
        item.Value.StoredWeeklyStreak = CompletedWeeklyStreak;
      }
    }
  }
}
