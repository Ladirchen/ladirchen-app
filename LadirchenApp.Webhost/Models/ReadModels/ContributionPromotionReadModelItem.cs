using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record ContributionPromotionReadModelItem(
  [property: JsonRequired] string Id,
  string? TranslationKey,
  [property: JsonRequired] string ContributionId,
  [property: JsonRequired] string Title,
  string? TitleKey,
  [property: JsonRequired] decimal Multiplier,
  [property: JsonRequired] string Deadline,
  [property: JsonRequired] decimal TeamworkBonus,
  [property: JsonRequired] bool Active,
  bool? Available
);
