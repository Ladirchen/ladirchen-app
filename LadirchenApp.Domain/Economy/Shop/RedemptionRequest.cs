using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class RedemptionRequest : Entity<RedemptionRequestId>
{
  internal RedemptionRequest(RedemptionRequestId id, ShopRewardId rewardId, FamilyMemberId requesterId,
    LadirchenAmount price, int? durationMinutes, DateTimeOffset now) : base(id)
  {
    RewardId = rewardId;
    RequesterId = requesterId;
    Price = price;
    DurationMinutes = durationMinutes;
    RequestedAt = now.ToUniversalTime();
  }
  public ShopRewardId RewardId
  {
    get;
  }
  public FamilyMemberId RequesterId
  {
    get;
  }
  public LadirchenAmount Price
  {
    get;
  }
  public DateTimeOffset RequestedAt
  {
    get;
  }
  public RedemptionStatus Status
  {
    get; private set;
  }
  public DateTimeOffset? DecidedAt
  {
    get; private set;
  }
  public int? DurationMinutes { get; }
  public int? TimerRemainingSeconds { get; private set; }
  public DateTimeOffset? TimerStartedAt { get; private set; }
  public DateTimeOffset? TimerCancelledAt { get; private set; }

  public int RemainingSeconds(DateTimeOffset now)
  {
    if (TimerRemainingSeconds is null)
    {
      return 0;
    }

    if (TimerStartedAt is null)
    {
      return TimerRemainingSeconds.Value;
    }

    var elapsed = Math.Max(0, (int)Math.Floor((now.ToUniversalTime() - TimerStartedAt.Value).TotalSeconds));

    return Math.Max(0, TimerRemainingSeconds.Value - elapsed);
  }

  internal void StartTimer(DateTimeOffset now)
  {
    if (DurationMinutes is null)
    {
      return;
    }

    TimerRemainingSeconds = checked(DurationMinutes.Value * 60);
    TimerStartedAt = now.ToUniversalTime();
  }

  internal void PauseTimer(DateTimeOffset now)
  {
    Rules.Require(TimerStartedAt is not null && RemainingSeconds(now) > 0, DomainError.InvalidState,
      "A running time reward is required.");
    TimerRemainingSeconds = RemainingSeconds(now);
    TimerStartedAt = null;
  }

  internal void ResumeTimer(DateTimeOffset now)
  {
    Rules.Require(TimerRemainingSeconds > 0 && TimerStartedAt is null && TimerCancelledAt is null,
      DomainError.InvalidState, "A paused time reward is required.");
    TimerStartedAt = now.ToUniversalTime();
  }

  internal void ResetTimer(DateTimeOffset now)
  {
    var durationMinutes = DurationMinutes;
    Rules.Require(durationMinutes is not null && TimerCancelledAt is null, DomainError.InvalidState,
      "An active time reward is required.");
    TimerRemainingSeconds = checked(durationMinutes.GetValueOrDefault() * 60);
    TimerStartedAt = now.ToUniversalTime();
  }

  internal void CancelTimer(DateTimeOffset now)
  {
    Rules.Require(DurationMinutes is not null && TimerRemainingSeconds is not null && TimerCancelledAt is null,
      DomainError.InvalidState, "An active time reward is required.");
    TimerRemainingSeconds = RemainingSeconds(now);
    TimerStartedAt = null;
    TimerCancelledAt = now.ToUniversalTime();
  }

  internal void ExpireTimer(DateTimeOffset now)
  {
    Rules.Require(TimerStartedAt is not null && TimerCancelledAt is null && RemainingSeconds(now) == 0,
      DomainError.InvalidState, "An elapsed running time reward is required.");
    TimerRemainingSeconds = 0;
    TimerStartedAt = null;
  }
  internal void Decide(RedemptionStatus status, DateTimeOffset now)
  {
    Status = status;
    DecidedAt = now.ToUniversalTime();
  }

  internal void Restore(
    RedemptionStatus status,
    DateTimeOffset? decidedAt,
    int? timerRemainingSeconds = null,
    DateTimeOffset? timerStartedAt = null,
    DateTimeOffset? timerCancelledAt = null
  )
  {
    Rules.Defined(status);
    Rules.Require(
      status == RedemptionStatus.Pending ? decidedAt is null : decidedAt is not null,
      DomainError.InvalidState,
      "A request decision time must match its status."
    );
    Status = status;
    DecidedAt = decidedAt?.ToUniversalTime();
    Rules.Require(timerRemainingSeconds is null || DurationMinutes is not null, DomainError.InvalidState,
      "Only a time reward can restore timer state.");
    Rules.Require(timerRemainingSeconds is null || timerRemainingSeconds >= 0, DomainError.InvalidValue,
      "Timer remaining seconds cannot be negative.");
    Rules.Require(timerStartedAt is null || timerRemainingSeconds is not null, DomainError.InvalidState,
      "A running timer requires remaining time.");
    Rules.Require(timerCancelledAt is null || timerStartedAt is null, DomainError.InvalidState,
      "A cancelled timer cannot be running.");
    TimerRemainingSeconds = timerRemainingSeconds;
    TimerStartedAt = timerStartedAt?.ToUniversalTime();
    TimerCancelledAt = timerCancelledAt?.ToUniversalTime();
  }
}
