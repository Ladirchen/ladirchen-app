using System.Collections.Generic;

namespace LadirchenApp.Domain.House.Shop;

public static class HouseShopCatalog
{
  private static readonly HashSet<string> SpecialAccessoryIds =
  [
    "halloween-pumpkin-arch",
    "halloween-bat-garland",
    "candy-cloud-bushes",
    "candy-striped-fence",
    "family-flag",
  ];

  public static readonly IReadOnlyDictionary<string, (decimal Price, int MinimumLevel, bool Movable, string IntendedArea, string AssignedArea, HousePlacement Initial)> Accessories =
    new Dictionary<string, (decimal, int, bool, string, string, HousePlacement)>
    {
      ["flower-boxes"] = (2, 0, true, "outside", "garden", new HousePlacement("garden", 18, 77, 0.72m)),
      ["garden-lights"] = (4, 0, true, "outside", "garden", new HousePlacement("garden", 47, 24, 0.82m)),
      ["hammock"] = (5, 0, true, "outside", "garden", new HousePlacement("garden", 22, 72, 0.78m)),
      ["telescope"] = (7, 0, true, "outside", "garden", new HousePlacement("garden", 88, 57, 0.72m)),
      ["round-rug"] = (3, 0, true, "inside", "living-room", new HousePlacement("living-room", 49, 76, 1)),
      ["cozy-sofa"] = (5, 0, true, "inside", "living-room", new HousePlacement("living-room", 70, 52, 0.9m)),
      ["bookshelf"] = (4, 2, true, "inside", "children-room", new HousePlacement("children-room", 85, 46, 0.76m)),
      ["floor-lamp"] = (3, 0, true, "inside", "living-room", new HousePlacement("living-room", 88, 48, 0.82m)),
      ["bunk-bed"] = (6, 2, true, "inside", "children-room", new HousePlacement("children-room", 22, 55, 0.8m)),
      ["play-table"] = (3, 2, true, "inside", "children-room", new HousePlacement("children-room", 65, 76, 0.78m)),
      ["plant-corner"] = (2, 0, true, "inside", "creative-room", new HousePlacement("creative-room", 17, 62, 0.78m)),
      ["wall-art"] = (2, 0, true, "inside", "living-room", new HousePlacement("living-room", 22, 25, 0.78m)),
      ["cat-tree"] = (5, 0, true, "inside", "living-room", new HousePlacement("living-room", 14, 59, 0.8m)),
      ["cat-bed"] = (2, 0, true, "inside", "living-room", new HousePlacement("living-room", 26, 82, 0.72m)),
      ["dog-blanket"] = (3, 0, true, "inside", "living-room", new HousePlacement("living-room", 75, 84, 0.72m)),
      ["pet-bowl"] = (2, 0, true, "inside", "living-room", new HousePlacement("living-room", 90, 82, 0.7m)),
      ["kitchen-counter"] = (6, 0, false, "inside", "kitchen", new HousePlacement("kitchen", 38, 61, 1.2m)),
      ["retro-fridge"] = (5, 0, false, "inside", "kitchen", new HousePlacement("kitchen", 51, 54, 0.6m)),
      ["family-dining-table"] = (5, 0, true, "inside", "kitchen", new HousePlacement("kitchen", 48, 74, 0.95m)),
      ["double-bed"] = (6, 3, true, "inside", "bedroom", new HousePlacement("bedroom", 48, 65, 0.88m)),
      ["wardrobe"] = (5, 3, true, "inside", "bedroom", new HousePlacement("bedroom", 84, 48, 0.76m)),
      ["bedside-table"] = (3, 3, true, "inside", "bedroom", new HousePlacement("bedroom", 18, 66, 0.7m)),
      ["art-desk"] = (5, 4, true, "inside", "creative-room", new HousePlacement("creative-room", 54, 69, 0.82m)),
      ["storage-cabinet"] = (4, 4, true, "inside", "creative-room", new HousePlacement("creative-room", 84, 49, 0.75m)),
      ["garden-pool"] = (7, 1, true, "outside", "garden", new HousePlacement("garden", 25, 59, 0.9m)),
      ["garden-chair"] = (3, 1, true, "outside", "garden", new HousePlacement("garden", 68, 66, 0.76m)),
      ["garden-table"] = (4, 1, true, "outside", "garden", new HousePlacement("garden", 82, 69, 0.74m)),
      ["garden-trampoline"] = (6, 2, true, "outside", "garden", new HousePlacement("garden", 74, 38, 0.82m)),
      ["garden-sunshade"] = (4, 1, true, "outside", "garden", new HousePlacement("garden", 49, 48, 0.8m)),
      ["halloween-pumpkin-arch"] = (4, 0, true, "outside", "garden", new HousePlacement("garden", 48, 66, 0.78m)),
      ["halloween-bat-garland"] = (3, 0, true, "outside", "garden", new HousePlacement("garden", 49, 23, 0.9m)),
      ["candy-cloud-bushes"] = (4, 0, true, "outside", "garden", new HousePlacement("garden", 30, 66, 0.86m)),
      ["candy-striped-fence"] = (4, 0, true, "outside", "garden", new HousePlacement("garden", 68, 70, 0.82m)),
      ["family-flag"] = (6, 0, true, "outside", "garden", new HousePlacement("garden", 84, 62, 0.9m)),
    };

  public static readonly IReadOnlyDictionary<string, (decimal Price, int MinimumLevel, IReadOnlyList<string> AccessoryIds)> FurnitureSets =
    new Dictionary<string, (decimal, int, IReadOnlyList<string>)>
    {
      ["cozy-living-set"] = (7, 0, ["cozy-sofa", "round-rug", "floor-lamp"]),
      ["family-kitchen-set"] = (9, 0, ["kitchen-counter", "retro-fridge", "family-dining-table"]),
      ["adventure-kids-set"] = (8, 2, ["bunk-bed", "play-table", "bookshelf"]),
      ["calm-bedroom-set"] = (9, 3, ["double-bed", "wardrobe", "bedside-table"]),
      ["creative-studio-set"] = (8, 4, ["art-desk", "storage-cabinet", "plant-corner"]),
      ["pet-comfort-set"] = (7, 0, ["cat-tree", "cat-bed", "dog-blanket", "pet-bowl"]),
      ["summer-garden-set"] = (10, 1, ["garden-pool", "garden-chair", "garden-table", "garden-sunshade"]),
      ["garden-play-set"] = (8, 2, ["garden-trampoline", "hammock", "garden-lights"]),
      ["halloween-exterior-set"] = (6, 0, ["halloween-pumpkin-arch", "halloween-bat-garland"]),
      ["cotton-candy-exterior-set"] = (7, 0, ["candy-cloud-bushes", "candy-striped-fence"]),
    };

  public static readonly IReadOnlyDictionary<string, decimal> Themes = new Dictionary<string, decimal>
  {
    ["sunny-dollhouse"] = 0,
    ["halloween-night"] = 7,
    ["christmas-wonderland"] = 8,
    ["cotton-candy-dream"] = 9,
    ["starlight-palace"] = 10,
  };

  public static readonly IReadOnlyDictionary<string, (string ThemeId, string Area, decimal Price, int MinimumLevel)> RoomDesigns =
    CreateRoomDesigns();

  public static readonly IReadOnlyDictionary<string, int> AreaMinimumLevels = new Dictionary<string, int>
  {
    ["garden"] = 0,
    ["living-room"] = 0,
    ["kitchen"] = 0,
    ["bathroom"] = 1,
    ["children-room"] = 1,
    ["play-room"] = 2,
    ["nursery"] = 2,
    ["pet-room"] = 2,
    ["bedroom"] = 3,
    ["wardrobe-room"] = 3,
    ["gamer-room"] = 3,
    ["creative-room"] = 4,
    ["makeup-room"] = 4,
  };

  public static readonly IReadOnlyList<(string Id, int Level, int RoomCount)> HouseStages =
  [
    ("starter-home", 0, 2),
    ("family-home", 1, 4),
    ("garden-home", 2, 7),
    ("tower-home", 3, 10),
    ("dream-home", 4, 12),
  ];

  public static string CatalogAreaForAccessory(string id)
  {
    if (SpecialAccessoryIds.Contains(id))
    {
      return "special";
    }

    return Accessories.TryGetValue(id, out var definition) && definition.IntendedArea == "outside"
      ? "outside"
      : "inside";
  }

  private static IReadOnlyDictionary<string, (string, string, decimal, int)> CreateRoomDesigns()
  {
    var result = new Dictionary<string, (string, string, decimal, int)>
    {
      ["garden-ladi-hills"] = ("sunny-dollhouse", "garden", 0, 0),
      ["living-ladi-classic"] = ("sunny-dollhouse", "living-room", 0, 0),
      ["kitchen-ladi-classic"] = ("sunny-dollhouse", "kitchen", 0, 0),
    };

    foreach (var themeId in Themes.Keys)
    {
      if (themeId == "sunny-dollhouse")
      {
        continue;
      }

      result[$"garden-{themeId}"] = (themeId, "garden", 0, 0);
      result[$"living-{themeId}"] = (themeId, "living-room", 0, 0);
      result[$"kitchen-{themeId}"] = (themeId, "kitchen", 0, 0);
    }

    return result;
  }
}
