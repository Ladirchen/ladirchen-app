using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

using FamilyAggregate = LadirchenApp.Domain.Family.Family;

namespace LadirchenApp.Domain.House;

public sealed class FamilyHouse : AggregateRoot<FamilyId>
{
  private readonly Dictionary<FamilyMemberId, ParticipantProgress> _participants = new();
  private readonly Dictionary<IsoWeekPeriod, ClosedFamilyWeek> _closedWeeks = new();

  public const int MaximumStoredLevel = 4;

  internal FamilyHouse()
    : this(new FamilyId(Guid.NewGuid()))
  {
  }

  private FamilyHouse(FamilyId id)
    : base(id)
  {
  }

  public int StoredLevel { get; private set; }
  public int CompletedWeeklyStreak { get; private set; }
  public int CurrentWeekTarget { get; private set; } = PersonalStreakPolicy.DefaultWeeklyTargetDays;
  public Percentage MaximumSavingsInterestRate { get; private set; } = SavingsInterestPolicy.Default.MaximumRate;
  public IReadOnlyCollection<ClosedFamilyWeek> ClosedWeeks
  {
    get
    {
      return _closedWeeks.Values;
    }
  }

  internal static FamilyHouse Reconstitute(
    FamilyAggregate family,
    FamilyHouseState state
  )
  {
    ArgumentNullException.ThrowIfNull(family);
    ArgumentNullException.ThrowIfNull(state);
    Rules.Require(
      family.Id == state.Id,
      DomainError.InvalidState,
      "The family progression belongs to another family."
    );
    var house = new FamilyHouse(state.Id);
    house.ReconstituteProgression(state);
    var participantIds = CurrentParticipantIds(family);

    foreach (var participant in state.Participants)
    {
      ArgumentNullException.ThrowIfNull(participant);
      Rules.Require(
        !house._participants.ContainsKey(participant.MemberId),
        DomainError.Duplicate,
        "A family member can have only one progression state."
      );
      house._participants.Add(
        participant.MemberId,
        new ParticipantProgress(
          participantIds.Contains(participant.MemberId),
          participant.StoredWeeklyStreak
        )
      );
    }

    foreach (var participantId in participantIds)
    {
      house._participants.TryAdd(participantId, new ParticipantProgress(true, 0));
    }

    foreach (var week in state.ClosedWeeks)
    {
      ArgumentNullException.ThrowIfNull(week);
      house.AddClosedWeek(
        new ClosedFamilyWeek(
          week.Period,
          week.Outcome,
          week.EvaluatedAt,
          week.MemberEvaluations
        )
      );
    }

    return house;
  }

  public static FamilyHouse CreateInitial(FamilyAggregate family)
  {
    ArgumentNullException.ThrowIfNull(family);
    var house = new FamilyHouse(family.Id);

    foreach (var participantId in CurrentParticipantIds(family))
    {
      house._participants.Add(participantId, new ParticipantProgress(true, 0));
    }

    return house;
  }

  internal FamilyHouseState CaptureState()
  {
    return new FamilyHouseState(
      Id,
      StoredLevel,
      CompletedWeeklyStreak,
      CurrentWeekTarget,
      MaximumSavingsInterestRate,
      _participants
        .OrderBy(item => item.Key.Value, StringComparer.Ordinal)
        .Select(item => new ParticipantProgressState(
          item.Key,
          item.Value.StoredWeeklyStreak
        )),
      _closedWeeks.Values
        .OrderBy(item => item.Period.Start)
        .Select(item => new ClosedFamilyWeekState(
          item.Period,
          item.Outcome,
          item.EvaluatedAt,
          item.MemberEvaluations.Values
        ))
    );
  }

  internal void RegisterParticipant(FamilyMemberId memberId, bool participates, int storedWeeklyStreak)
  {
    ArgumentNullException.ThrowIfNull(memberId);
    Rules.Require(storedWeeklyStreak >= 0, DomainError.InvalidValue, "A stored streak cannot be negative.");
    _participants[memberId] = new ParticipantProgress(participates, storedWeeklyStreak);
  }

  public void SynchronizeParticipants(FamilyAggregate family)
  {
    ArgumentNullException.ThrowIfNull(family);
    Rules.Require(
      family.Id == Id,
      DomainError.InvalidState,
      "The family progression belongs to another family."
    );
    var familyMemberIds = family.Members.Select(item => item.Id).ToHashSet();
    var participantIds = CurrentParticipantIds(family);

    foreach (var departedMemberId in _participants.Keys.Where(item => !familyMemberIds.Contains(item)).ToArray())
    {
      _participants.Remove(departedMemberId);
    }

    foreach (var memberId in familyMemberIds)
    {
      if (_participants.TryGetValue(memberId, out var progress))
      {
        _participants[memberId] = new ParticipantProgress(
          participantIds.Contains(memberId),
          progress.StoredWeeklyStreak
        );
      }
    }

    foreach (var memberId in participantIds)
    {
      _participants.TryAdd(memberId, new ParticipantProgress(true, 0));
    }
  }

  public int StoredWeeklyStreakFor(FamilyMemberId memberId)
  {
    ArgumentNullException.ThrowIfNull(memberId);

    return _participants.TryGetValue(memberId, out var progress) ? progress.StoredWeeklyStreak : 0;
  }

  internal void Restore(int storedLevel, int completedWeeklyStreak)
  {
    RestoreProgression(storedLevel, completedWeeklyStreak);
  }

  public void SetMaximumSavingsInterestRate(Percentage maximumRate)
  {
    ArgumentNullException.ThrowIfNull(maximumRate);
    MaximumSavingsInterestRate = new SavingsInterestPolicy(maximumRate).MaximumRate;
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

  internal void RestoreClosedWeek(ClosedFamilyWeek week)
  {
    ArgumentNullException.ThrowIfNull(week);
    AddClosedWeek(week);
  }

  private void ReconstituteProgression(FamilyHouseState state)
  {
    RestoreProgression(state.StoredLevel, state.CompletedWeeklyStreak);
    Rules.Require(
      state.CurrentWeekTarget > 0,
      DomainError.InvalidValue,
      "The current-week target must be positive."
    );
    CurrentWeekTarget = state.CurrentWeekTarget;
    SetMaximumSavingsInterestRate(state.MaximumSavingsInterestRate);
  }

  private static HashSet<FamilyMemberId> CurrentParticipantIds(FamilyAggregate family)
  {
    return family.WeeklyGoalParticipants
      .Select(member => member.Id)
      .ToHashSet();
  }

  private void RestoreProgression(int storedLevel, int completedWeeklyStreak)
  {
    Rules.Require(
      storedLevel >= 0 && storedLevel <= MaximumStoredLevel,
      DomainError.InvalidValue,
      "Invalid house level."
    );
    Rules.Require(
      completedWeeklyStreak >= 0,
      DomainError.InvalidValue,
      "The family streak cannot be negative."
    );
    StoredLevel = storedLevel;
    CompletedWeeklyStreak = completedWeeklyStreak;
  }

  private void AddClosedWeek(ClosedFamilyWeek week)
  {
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
