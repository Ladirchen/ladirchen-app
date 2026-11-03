using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record BehaviorNoticeReadModel(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string ChildId,
  [property: JsonRequired] string GuardianId,
  [property: JsonRequired] string Level,
  [property: JsonRequired] decimal DeductedAmount,
  [property: JsonRequired] string Reason,
  [property: JsonRequired] string OccurredAtUtc
);
