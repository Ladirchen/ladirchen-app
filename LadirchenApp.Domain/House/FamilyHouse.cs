using System;
using System.Collections.Generic;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.House;

public sealed class FamilyHouse
{
  private readonly Dictionary<FamilyMemberId, ParticipantProgress> _participants = new();
  private readonly Dictionary<IsoWeekPeriod, ClosedFamilyWeek> _closedWeeks = new();

  public const int MaximumStoredLevel = 4;

  public int StoredLevel { get; private set; }
  public int CompletedWeeklyStreak { get; private set; }
  public string Theme { get; private set; } = "default";
  public IReadOnlyCollection<ClosedFamilyWeek> ClosedWeeks
  {
    get
    {
      return _closedWeeks.Values;
    }
  }

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
    if (energy < HouseEnergyPolicy.MinimumSuccessfulWeekEnergy)
    {
      return false;
    }

    StoredLevel = Math.Min(MaximumStoredLevel, StoredLevel + 1);
    CompletedWeeklyStreak++;
    SynchronizeParticipantStreaks();

    return true;
  }

  public bool CloseSuccessfulWeek(
    IsoWeekPeriod period,
    int energy,
    IEnumerable<PersonalLadiEvaluation> memberEvaluations,
    DateTimeOffset evaluatedAt
  )
  {
    RequireOpenWeek(period);

    if (!CloseSuccessfulWeek(energy))
    {
      return false;
    }

    StoreClosedWeek(period, FamilyWeekOutcome.Completed, evaluatedAt, memberEvaluations);

    return true;
  }

  public void CloseFailedWeek(int energy)
  {
    Rules.Require(
      energy < HouseEnergyPolicy.MinimumSuccessfulWeekEnergy,
      DomainError.InvalidState,
      "A successful week cannot be closed as failed."
    );
    StoredLevel = Math.Max(0, StoredLevel - 1);

    if (energy < HouseEnergyPolicy.SevereFailureEnergyThreshold)
    {
      CompletedWeeklyStreak = Math.Max(0, CompletedWeeklyStreak - 1);
    }

    SynchronizeParticipantStreaks();
  }

  public void CloseFailedWeek(
    IsoWeekPeriod period,
    int energy,
    IEnumerable<PersonalLadiEvaluation> memberEvaluations,
    DateTimeOffset evaluatedAt
  )
  {
    RequireOpenWeek(period);
    CloseFailedWeek(energy);
    StoreClosedWeek(period, FamilyWeekOutcome.Failed, evaluatedAt, memberEvaluations);
  }

  public void RestoreClosedWeek(ClosedFamilyWeek week)
  {
    ArgumentNullException.ThrowIfNull(week);
    Rules.Require(
      _closedWeeks.TryAdd(week.Period, week),
      DomainError.Duplicate,
      "The family week has already been closed."
    );
  }

  public bool IsWeekClosed(IsoWeekPeriod period)
  {
    ArgumentNullException.ThrowIfNull(period);

    return _closedWeeks.ContainsKey(period);
  }

  public ClosedFamilyWeek ClosedWeekFor(IsoWeekPeriod period)
  {
    ArgumentNullException.ThrowIfNull(period);

    return _closedWeeks.TryGetValue(period, out var week)
      ? week
      : throw new DomainRuleException(DomainError.NotFound, "The family week has not been closed.");
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

  private void RequireOpenWeek(IsoWeekPeriod period)
  {
    ArgumentNullException.ThrowIfNull(period);
    Rules.Require(
      !_closedWeeks.ContainsKey(period),
      DomainError.InvalidState,
      "The family week has already been closed."
    );
  }

  private void StoreClosedWeek(
    IsoWeekPeriod period,
    FamilyWeekOutcome outcome,
    DateTimeOffset evaluatedAt,
    IEnumerable<PersonalLadiEvaluation> memberEvaluations
  )
  {
    var week = new ClosedFamilyWeek(period, outcome, evaluatedAt, memberEvaluations);
    _closedWeeks.Add(period, week);
  }
}
