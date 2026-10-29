using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  private readonly HashSet<IsoWeekPeriod> _settledInterestPeriods = new();

  public InterestEvaluationContext EvaluateParticipation(
    FamilyMemberId memberId,
    IsoWeekPeriod period,
    int completedWeeklyStreak,
    int weeklyTarget
  )
  {
    _family.ActiveMember(memberId);
    ArgumentNullException.ThrowIfNull(period);
    ArgumentOutOfRangeException.ThrowIfNegative(completedWeeklyStreak);
    ArgumentOutOfRangeException.ThrowIfLessThan(weeklyTarget, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(weeklyTarget, PersonalStreakPolicy.DefaultWeeklyTargetDays);
    var days = ContributionParticipationPolicy.ApprovedDays(_contributions, memberId, period, _family.TimeZone);

    return new(
      checked(completedWeeklyStreak * weeklyTarget + days),
      ContributionParticipationPolicy.Completion(_contributions, memberId),
      ContributionParticipationPolicy.AverageRating(_contributions, memberId)
    );
  }

  public decimal ApplyWeeklyInterest(
    FamilyMemberId actor,
    IsoWeekPeriod period,
    IReadOnlyDictionary<FamilyMemberId, InterestEvaluationContext> evaluation,
    TransactionId operationId,
    DateTimeOffset now,
    SavingsInterestPolicy? policy = null
  )
  {
    _family.RequireAdministrator(actor);
    ArgumentNullException.ThrowIfNull(period);
    ArgumentNullException.ThrowIfNull(evaluation);
    Rules.Require(
      period.Start < IsoWeekPeriod.For(_family.TimeZone.LocalDate(now)).Start,
      DomainError.InvalidState,
      "Only a closed ISO week can be settled."
    );

    if (_settledInterestPeriods.Contains(period))
    {
      return 0;
    }

    RequireNewOperation(operationId);
    var snapshot = evaluation.ToDictionary(item => item.Key, item => item.Value);

    foreach (var item in snapshot)
    {
      _family.ActiveMember(item.Key);
      ArgumentNullException.ThrowIfNull(item.Value);
    }

    policy ??= SavingsInterestPolicy.Default;
    var changes = new List<(SavingGoal Goal, decimal Saved, decimal Interest, decimal Credit, Percentage Rate)>();
    decimal total = 0;

    foreach (
      var goal in _goals.Where(item =>
        !item.Cancelled
        && !item.Owner.IsFamily
        && !item.Completed
        && item.Saved > 0
        && _family.TimeZone.LocalDate(item.CreatedAt) <= period.Start.AddDays(6)
      )
    )
    {
      Rules.Require(
        snapshot.TryGetValue(goal.Owner.MemberId!, out var context),
        DomainError.InvalidValue,
        "Every eligible goal owner requires an interest evaluation."
      );
      var rate = policy.CalculateRate(context!);
      var credit = SavingsInterestPolicy.CalculateCredit(new(goal.Saved), new(goal.Target), rate);
      var saved = checked(goal.Saved + credit);
      var earned = checked(goal.InterestEarned + credit);
      total = checked(total + credit);
      changes.Add((goal, saved, earned, credit, rate));
    }

    var entries = changes
      .Select(change => new LedgerEntry(
        operationId,
        change.Goal.Owner.MemberId!,
        change.Goal.Id,
        LedgerKind.GoalCredit,
        change.Credit,
        change.Goal.Id,
        $"Weekly interest {period.Year}-W{period.Week:D2}",
        now.ToUniversalTime(),
        actor
      ))
      .ToArray();

    foreach (var (goal, saved, interest, credit, rate) in changes)
    {
      goal.SetInterest(saved, interest);
      AddDomainEvent(new GoalInterestCredited(Id, goal.Id, period, rate, credit, now));

      if (goal.Completed)
      {
        AddDomainEvent(new GoalCompleted(Id, goal.Id, now));
      }
    }

    _settledInterestPeriods.Add(period);
    Record(operationId, entries);
    AddDomainEvent(new WeeklyInterestCredited(Id, period, total, now));

    return total;
  }
}
