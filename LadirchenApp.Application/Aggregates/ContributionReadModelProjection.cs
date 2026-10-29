using System.Collections.Generic;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Aggregates;

internal sealed record ContributionReadModelProjection(
  decimal PerfectRatingBonusPercent,
  IReadOnlyDictionary<ContributionId, ContributionReadProjection> Contributions,
  IReadOnlyDictionary<PromotionId, bool> PromotionAvailability,
  IReadOnlyDictionary<FamilyMemberId, decimal> TodayEarnedByMember,
  IReadOnlyDictionary<FamilyMemberId, decimal> CurrentWeekEarnedByMember,
  IReadOnlyDictionary<FamilyMemberId, int> CurrentWeekApprovedContributionCountByMember
);
