using System.Collections.Generic;

namespace LadirchenApp.Application.Aggregates;

public sealed record HomeCatalogCapabilitiesProjection(
  bool CanArrangeHouse,
  IReadOnlySet<string> AvailableAccessoryIds,
  IReadOnlySet<string> PurchasableAccessoryIds,
  IReadOnlySet<string> AvailableAreaIds,
  IReadOnlySet<string> AvailableFurnitureSetIds,
  IReadOnlySet<string> OwnedFurnitureSetIds,
  IReadOnlySet<string> PurchasableFurnitureSetIds,
  IReadOnlySet<string> PurchasableThemeIds,
  IReadOnlySet<string> AvailableRoomDesignIds,
  IReadOnlySet<string> PurchasableRoomDesignIds
);
