using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record ChildBehaviorStatusReadModel(
  [property: JsonRequired] string ChildId,
  [property: JsonRequired] string Level,
  [property: JsonRequired] int WarningCount,
  [property: JsonRequired] bool TasksBlocked,
  string? LastReason,
  string? UpdatedAtUtc
);
