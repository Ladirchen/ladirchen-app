using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record SavingGoalState(
  SavingGoalId Id,
  FamilyMemberId? OwnerId,
  string Title,
  decimal Target,
  GoalVisibility Visibility,
  DateTimeOffset CreatedAt,
  IReadOnlyList<FamilyMemberId> ParticipantIds,
  decimal Saved,
  decimal StarterBonus,
  decimal InterestEarned,
  IReadOnlyList<RefundableContributionState> RefundableContributions,
  GoalCancellationRequestState? CancellationRequest,
  bool Cheered,
  bool Cancelled,
  IsoWeekPeriod? LastInterestPeriod,
  Percentage? LastInterestRate
);
