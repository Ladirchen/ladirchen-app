using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record ContributionPartnersPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] IReadOnlyList<string> PartnerIds
);
