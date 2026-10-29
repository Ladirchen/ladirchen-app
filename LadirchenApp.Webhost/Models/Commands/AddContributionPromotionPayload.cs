using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record AddContributionPromotionPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string ContributionId,
  [property: JsonRequired] decimal Multiplier,
  [property: JsonRequired] string Deadline,
  [property: JsonRequired] decimal TeamworkBonus
);
