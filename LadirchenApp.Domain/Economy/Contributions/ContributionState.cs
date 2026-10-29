using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

internal sealed record ContributionState(
  ContributionId Id,
  string Title,
  ContributionKind Kind,
  LadirchenAmount Reward,
  Percentage Energy,
  FamilyMemberId? AssigneeId,
  ContributionStatus Status,
  StarRating? Rating,
  ContributionRewardBreakdown? EarnedReward,
  DateTimeOffset? ApprovedAt,
  IReadOnlyList<FamilyMemberId> InvitedPartnerIds
);
