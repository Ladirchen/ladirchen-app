using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record GoalCancellationRequestState(
  FamilyMemberId RequestedByMemberId,
  IReadOnlyList<FamilyMemberId> ConsentedMemberIds
);
