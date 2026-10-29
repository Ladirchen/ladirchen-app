using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HouseAccessoryCatalogEntryReadModel(
  [property: JsonRequired] bool Available,
  [property: JsonRequired] string CatalogArea,
  [property: JsonRequired] string Id,
  int? MinimumHouseLevel,
  bool? Owned,
  decimal? Price,
  [property: JsonRequired] IReadOnlyList<string> PurchasableByMemberIds
);
