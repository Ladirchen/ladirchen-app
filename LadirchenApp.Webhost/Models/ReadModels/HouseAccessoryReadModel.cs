using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HouseAccessoryReadModel(
  [property: JsonRequired] string Id,
  [property: JsonRequired] bool Equipped,
  [property: JsonRequired] bool Owned
);
