using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record SavingsGoalReadProjection(
  decimal ProjectedInterestCredit,
  decimal RemainingAmount,
  decimal WithdrawableBalance,
  IReadOnlyList<FamilyMemberId> VisibleToMemberIds,
  IReadOnlyDictionary<FamilyMemberId, SavingsGoalCommandLimits> CommandLimits,
  SavingsGoalCancellationRequestReadProjection? CancellationRequest
);
