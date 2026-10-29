using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record AvatarAppearancePayload(
  [property: JsonRequired] string Age,
  [property: JsonRequired] string SkinToneId,
  [property: JsonRequired] string FaceShape,
  [property: JsonRequired] string Face,
  [property: JsonRequired] string Hair,
  [property: JsonRequired] string HairColorId,
  [property: JsonRequired] string Outfit,
  [property: JsonRequired] string OutfitColorId,
  [property: JsonRequired] string AccessoryId,
  [property: JsonRequired] string FunAccessoryId,
  [property: JsonRequired] string SeasonalAccessoryId
);
