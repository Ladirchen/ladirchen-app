using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record ContributionsReadModelState(
  [property: JsonRequired] IReadOnlyList<ContributionReadModelItem> Contributions,
  [property: JsonRequired] IReadOnlyDictionary<string, int> CurrentWeekApprovedContributionCountByMember,
  [property: JsonRequired] IReadOnlyDictionary<string, decimal> CurrentWeekEarnedByMember,
  decimal? PerfectRatingBonusPercent,
  [property: JsonRequired] IReadOnlyList<ContributionPromotionReadModelItem> Promotions,
  IReadOnlyDictionary<string, decimal>? TodayEarnedByMember
);
