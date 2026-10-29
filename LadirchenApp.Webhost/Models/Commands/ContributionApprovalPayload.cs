using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ContributionApprovalPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] int Stars
);
