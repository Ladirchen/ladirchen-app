using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HousePlacementReadModel(
  [property: JsonRequired] string Id,
  [property: JsonRequired] string EntityType,
  [property: JsonRequired] string EntityId,
  [property: JsonRequired] string ZoneId,
  [property: JsonRequired] decimal X,
  [property: JsonRequired] decimal Y,
  [property: JsonRequired] decimal Scale
);
