using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record SavingsVoucherState(
  SavingGoalId GoalId,
  decimal Amount,
  FamilyMemberId? OwnerId,
  IReadOnlyList<FamilyMemberId> ParticipantIds,
  IReadOnlyList<FamilyMemberId> ConsentedMemberIds,
  DateTimeOffset? RedeemedAt
);
