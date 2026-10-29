using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record ContributionReadModelItem(
  [property: JsonRequired] string Id,
  string? TranslationKey,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Description,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] string Area,
  string? AreaKey,
  [property: JsonRequired] string Kind,
  [property: JsonRequired] string Status,
  [property: JsonRequired] decimal Reward,
  decimal? ProjectedReward,
  [property: JsonRequired] decimal Energy,
  string? AssigneeId,
  [property: JsonRequired] string DueLabel,
  string? DueLabelKey,
  string? WorldEffect,
  int? Stars,
  IReadOnlyList<string>? InvitedChildIds,
  IReadOnlyList<string>? PartnerableMemberIds,
  decimal? EarnedReward,
  decimal? EarnedRatingBonus,
  decimal? EarnedPromotionMultiplier,
  string? ApprovedAt,
  bool? RewardCelebrated
);
