using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record RewardDecisionPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] bool Approved
);
