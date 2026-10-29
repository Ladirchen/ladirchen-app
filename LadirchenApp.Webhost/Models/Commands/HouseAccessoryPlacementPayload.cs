using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record HouseAccessoryPlacementPayload(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string ZoneId
);
