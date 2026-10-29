using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record FamilySetupPetPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string Name,
  [property: JsonRequired] string Kind,
  [property: JsonRequired] string Avatar,
  [property: JsonRequired] string Color
);
