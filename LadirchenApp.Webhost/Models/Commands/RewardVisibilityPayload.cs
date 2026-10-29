using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record RewardVisibilityPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] bool IsVisible
);
