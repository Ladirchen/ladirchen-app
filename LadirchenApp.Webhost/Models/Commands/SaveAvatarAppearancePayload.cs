using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record SaveAvatarAppearancePayload(
  [property: JsonRequired] AvatarAppearancePayload Appearance
);
