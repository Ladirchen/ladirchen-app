using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HouseAreaCatalogEntryReadModel(
  [property: JsonRequired] bool Available,
  [property: JsonRequired] string Id,
  int? MinimumHouseLevel
);
