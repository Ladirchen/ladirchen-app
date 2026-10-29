using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

public sealed record SavingsGoalCancellationRequestReadProjection(
  FamilyMemberId RequestedByMemberId,
  IReadOnlyList<FamilyMemberId> ConsentedMemberIds,
  IReadOnlyList<FamilyMemberId> PendingMemberIds
);
