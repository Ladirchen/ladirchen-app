using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HouseEditionReadModel(
  [property: JsonRequired] string Id,
  [property: JsonRequired] bool Owned
);
