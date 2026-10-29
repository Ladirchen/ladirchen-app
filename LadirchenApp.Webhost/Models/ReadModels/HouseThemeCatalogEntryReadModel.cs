using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HouseThemeCatalogEntryReadModel(
  [property: JsonRequired] bool Available,
  [property: JsonRequired] string Id,
  bool? Owned,
  decimal? Price,
  [property: JsonRequired] IReadOnlyList<string> PurchasableByMemberIds
);
