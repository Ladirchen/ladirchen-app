using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record AddContributionPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string Title,
  [property: JsonRequired] string Description,
  [property: JsonRequired] string Icon,
  [property: JsonRequired] decimal Reward,
  [property: JsonRequired] decimal Energy,
  [property: JsonRequired] string Kind,
  string? AssigneeId
);
