using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record HouseEntityMovePayload(
  [property: JsonRequired] string PlacementId,
  [property: JsonRequired] string ZoneId,
  [property: JsonRequired] decimal X,
  [property: JsonRequired] decimal Y
);
