using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ContributionAssignmentPayload(
  [property: JsonRequired] string Id,
  string? MemberId
);
