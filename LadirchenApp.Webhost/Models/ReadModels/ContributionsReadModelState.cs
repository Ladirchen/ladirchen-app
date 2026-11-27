using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record ContributionsReadModelState(
  [property: JsonRequired] IReadOnlyList<ContributionReadModelItem> Contributions,
  [property: JsonRequired] IReadOnlyDictionary<string, int> CurrentWeekApprovedContributionCountByMember,
  [property: JsonRequired] IReadOnlyDictionary<string, decimal> CurrentWeekEarnedByMember,
  decimal? PerfectRatingBonusPercent,
  [property: JsonRequired] IReadOnlyList<ContributionPromotionReadModelItem> Promotions,
  [property: JsonRequired] IReadOnlyDictionary<string, decimal> TodayEarnedByMember,
  [property: JsonRequired] IReadOnlyList<FocusGoalReadModelItem> FocusGoals,
  [property: JsonRequired] IReadOnlyList<FocusAchievementReadModelItem> FocusAchievements,
  [property: JsonRequired] IReadOnlyDictionary<string, decimal> TotalEarnedByMember,
  [property: JsonRequired] IReadOnlyDictionary<string, int> TotalApprovedContributionCountByMember
);
