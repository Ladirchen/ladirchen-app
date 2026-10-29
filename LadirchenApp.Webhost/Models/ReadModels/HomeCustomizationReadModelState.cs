using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.ReadModels;

public sealed record HomeCustomizationReadModelState(
  [property: JsonRequired] HomeCapabilitiesReadModel Capabilities,
  [property: JsonRequired] HomeCatalogReadModel Catalog,
  [property: JsonRequired] IReadOnlyList<HouseAccessoryReadModel> Accessories,
  [property: JsonRequired] IReadOnlyList<HouseEditionReadModel> Editions,
  [property: JsonRequired] IReadOnlyList<HousePlacementReadModel> Placements,
  [property: JsonRequired] IReadOnlyList<RoomDesignReadModel> RoomDesigns,
  [property: JsonRequired] IReadOnlyList<SelectedRoomDesignReadModel> SelectedRoomDesigns,
  [property: JsonRequired] string SelectedEditionId
);
