using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record AvatarProfileCatalogReadModel(
  [property: JsonRequired] IReadOnlyList<string> AccessoryIds,
  [property: JsonRequired] IReadOnlyList<string> Ages,
  [property: JsonRequired] IReadOnlyList<string> FaceIds,
  [property: JsonRequired] IReadOnlyList<string> FaceShapeIds,
  [property: JsonRequired] IReadOnlyList<string> FunAccessoryIds,
  [property: JsonRequired] IReadOnlyList<string> HairColorIds,
  [property: JsonRequired] IReadOnlyList<string> HairIds,
  [property: JsonRequired] IReadOnlyList<string> OutfitColorIds,
  [property: JsonRequired] IReadOnlyList<string> OutfitIds,
  [property: JsonRequired] IReadOnlyList<string> SeasonalAccessoryIds,
  [property: JsonRequired] IReadOnlyList<string> SkinToneIds
);
