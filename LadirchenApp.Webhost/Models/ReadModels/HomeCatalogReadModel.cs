using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HomeCatalogReadModel(
  [property: JsonRequired] IReadOnlyList<HouseAccessoryCatalogEntryReadModel> Accessories,
  [property: JsonRequired] IReadOnlyList<HouseAreaCatalogEntryReadModel> Areas,
  [property: JsonRequired] IReadOnlyList<FurnitureSetCatalogEntryReadModel> FurnitureSets,
  [property: JsonRequired] IReadOnlyList<HouseStageCatalogEntryReadModel> HouseStages,
  [property: JsonRequired] IReadOnlyList<RoomDesignCatalogEntryReadModel> RoomDesigns,
  [property: JsonRequired] IReadOnlyList<HouseThemeCatalogEntryReadModel> Themes
);
