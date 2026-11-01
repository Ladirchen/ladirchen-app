using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record CopyContributionPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string SourceId
);
