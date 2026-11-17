using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record ShopTimeRewardReadModel(
  [property: JsonRequired] string RequestId,
  [property: JsonRequired] string RewardId,
  [property: JsonRequired] string ChildId,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] int DurationMinutes,
  [property: JsonRequired] int RemainingSeconds,
  [property: JsonRequired] string Status,
  string? StartedAt,
  string? EndsAt,
  bool CanPause,
  bool CanResume,
  bool CanReset,
  bool CanCancel
);
