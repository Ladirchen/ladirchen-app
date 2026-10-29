using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HouseStageCatalogEntryReadModel(
  [property: JsonRequired] bool Active,
  [property: JsonRequired] string Id,
  [property: JsonRequired] int Level,
  [property: JsonRequired] int RoomCount
);
