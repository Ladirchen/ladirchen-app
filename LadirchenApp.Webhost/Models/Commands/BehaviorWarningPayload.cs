using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record BehaviorWarningPayload(
  [property: JsonRequired] string ChildId,
  [property: JsonRequired] decimal Deduction,
  [property: JsonRequired] string IncidentId,
  [property: JsonRequired] string Reason
);
