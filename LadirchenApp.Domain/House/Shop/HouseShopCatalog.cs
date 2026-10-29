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
      ["flower-boxes"] = (60, 0, true, "outside", "garden", new HousePlacement("garden", 18, 77, 0.72m)),
      ["garden-lights"] = (120, 0, true, "outside", "garden", new HousePlacement("garden", 47, 24, 0.82m)),
      ["hammock"] = (160, 0, true, "outside", "garden", new HousePlacement("garden", 22, 72, 0.78m)),
      ["telescope"] = (260, 0, true, "outside", "garden", new HousePlacement("garden", 88, 57, 0.72m)),
      ["round-rug"] = (70, 0, true, "inside", "living-room", new HousePlacement("living-room", 49, 76, 1)),
      ["cozy-sofa"] = (140, 0, true, "inside", "living-room", new HousePlacement("living-room", 70, 52, 0.9m)),
      ["bookshelf"] = (110, 2, true, "inside", "children-room", new HousePlacement("children-room", 85, 46, 0.76m)),
      ["floor-lamp"] = (90, 0, true, "inside", "living-room", new HousePlacement("living-room", 88, 48, 0.82m)),
      ["bunk-bed"] = (180, 2, true, "inside", "children-room", new HousePlacement("children-room", 22, 55, 0.8m)),
      ["play-table"] = (95, 2, true, "inside", "children-room", new HousePlacement("children-room", 65, 76, 0.78m)),
      ["plant-corner"] = (55, 0, true, "inside", "creative-room", new HousePlacement("creative-room", 17, 62, 0.78m)),
      ["wall-art"] = (45, 0, true, "inside", "living-room", new HousePlacement("living-room", 22, 25, 0.78m)),
      ["cat-tree"] = (135, 0, true, "inside", "living-room", new HousePlacement("living-room", 14, 59, 0.8m)),
      ["cat-bed"] = (65, 0, true, "inside", "living-room", new HousePlacement("living-room", 26, 82, 0.72m)),
      ["dog-blanket"] = (75, 0, true, "inside", "living-room", new HousePlacement("living-room", 75, 84, 0.72m)),
      ["pet-bowl"] = (50, 0, true, "inside", "living-room", new HousePlacement("living-room", 90, 82, 0.7m)),
      ["kitchen-counter"] = (210, 0, false, "inside", "kitchen", new HousePlacement("kitchen", 38, 61, 1.2m)),
      ["retro-fridge"] = (150, 0, false, "inside", "kitchen", new HousePlacement("kitchen", 51, 54, 0.6m)),
      ["family-dining-table"] = (130, 0, true, "inside", "kitchen", new HousePlacement("kitchen", 48, 74, 0.95m)),
      ["double-bed"] = (190, 3, true, "inside", "bedroom", new HousePlacement("bedroom", 48, 65, 0.88m)),
      ["wardrobe"] = (150, 3, true, "inside", "bedroom", new HousePlacement("bedroom", 84, 48, 0.76m)),
      ["bedside-table"] = (80, 3, true, "inside", "bedroom", new HousePlacement("bedroom", 18, 66, 0.7m)),
      ["art-desk"] = (140, 4, true, "inside", "creative-room", new HousePlacement("creative-room", 54, 69, 0.82m)),
      ["storage-cabinet"] = (120, 4, true, "inside", "creative-room", new HousePlacement("creative-room", 84, 49, 0.75m)),
      ["garden-pool"] = (260, 1, true, "outside", "garden", new HousePlacement("garden", 25, 59, 0.9m)),
      ["garden-chair"] = (95, 1, true, "outside", "garden", new HousePlacement("garden", 68, 66, 0.76m)),
      ["garden-table"] = (110, 1, true, "outside", "garden", new HousePlacement("garden", 82, 69, 0.74m)),
      ["garden-trampoline"] = (220, 2, true, "outside", "garden", new HousePlacement("garden", 74, 38, 0.82m)),
      ["garden-sunshade"] = (105, 1, true, "outside", "garden", new HousePlacement("garden", 49, 48, 0.8m)),
      ["halloween-pumpkin-arch"] = (120, 0, true, "outside", "garden", new HousePlacement("garden", 48, 66, 0.78m)),
      ["halloween-bat-garland"] = (95, 0, true, "outside", "garden", new HousePlacement("garden", 49, 23, 0.9m)),
      ["candy-cloud-bushes"] = (125, 0, true, "outside", "garden", new HousePlacement("garden", 30, 66, 0.86m)),
      ["candy-striped-fence"] = (110, 0, true, "outside", "garden", new HousePlacement("garden", 68, 70, 0.82m)),
      ["family-flag"] = (180, 0, true, "outside", "garden", new HousePlacement("garden", 84, 62, 0.9m)),
    };

  public static readonly IReadOnlyDictionary<string, (decimal Price, int MinimumLevel, IReadOnlyList<string> AccessoryIds)> FurnitureSets =
    new Dictionary<string, (decimal, int, IReadOnlyList<string>)>
    {
      ["cozy-living-set"] = (250, 0, ["cozy-sofa", "round-rug", "floor-lamp"]),
      ["family-kitchen-set"] = (420, 0, ["kitchen-counter", "retro-fridge", "family-dining-table"]),
      ["adventure-kids-set"] = (310, 2, ["bunk-bed", "play-table", "bookshelf"]),
      ["calm-bedroom-set"] = (390, 3, ["double-bed", "wardrobe", "bedside-table"]),
      ["creative-studio-set"] = (280, 4, ["art-desk", "storage-cabinet", "plant-corner"]),
      ["pet-comfort-set"] = (260, 0, ["cat-tree", "cat-bed", "dog-blanket", "pet-bowl"]),
      ["summer-garden-set"] = (520, 1, ["garden-pool", "garden-chair", "garden-table", "garden-sunshade"]),
      ["garden-play-set"] = (340, 2, ["garden-trampoline", "hammock", "garden-lights"]),
      ["halloween-exterior-set"] = (190, 0, ["halloween-pumpkin-arch", "halloween-bat-garland"]),
      ["cotton-candy-exterior-set"] = (210, 0, ["candy-cloud-bushes", "candy-striped-fence"]),
    };

  public static readonly IReadOnlyDictionary<string, decimal> Themes = new Dictionary<string, decimal>
  {
    ["sunny-dollhouse"] = 0,
    ["halloween-night"] = 320,
    ["christmas-wonderland"] = 340,
    ["cotton-candy-dream"] = 360,
    ["starlight-palace"] = 420,
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
