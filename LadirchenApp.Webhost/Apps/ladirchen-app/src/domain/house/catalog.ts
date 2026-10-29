import type {
  FurnitureSetDefinition,
  FurnitureVisualId,
  HouseAccessoryCategory,
  HouseRoomDefinition,
  HouseRoomId,
  HouseStageDefinition,
  HouseStageLevel,
  HouseThemeDefinition,
} from "./types";
import type { HouseAccessory } from "./entities";
import { houseRoomColorPalette, houseThemeColorPalette } from "@/theme/color-palette";
export const HOUSE_STAGES: HouseStageDefinition[] = [
  { id: "starter-home", level: 0, nameKey: "catalog.stages.starter-home", icon: "🏠" },
  { id: "family-home", level: 1, nameKey: "catalog.stages.family-home", icon: "🏡" },
  { id: "garden-home", level: 2, nameKey: "catalog.stages.garden-home", icon: "🏘️" },
  { id: "tower-home", level: 3, nameKey: "catalog.stages.tower-home", icon: "🏰" },
  { id: "dream-home", level: 4, nameKey: "catalog.stages.dream-home", icon: "👑" },
];
export const DEFAULT_HOUSE_STAGE = HOUSE_STAGES[0]!;
export const HOUSE_ROOMS: ReadonlyArray<HouseRoomDefinition> = [
  {
    id: "living-room",
    nameKey: "catalog.rooms.living-room",
    icon: "🛋️",
    ...houseRoomColorPalette.livingRoom,
  },
  {
    id: "kitchen",
    nameKey: "catalog.rooms.kitchen",
    icon: "🍳",
    ...houseRoomColorPalette.kitchen,
  },
  {
    id: "bathroom",
    nameKey: "catalog.rooms.bathroom",
    icon: "🛁",
    ...houseRoomColorPalette.bathroom,
  },
  {
    id: "children-room",
    nameKey: "catalog.rooms.children-room",
    icon: "🧸",
    ...houseRoomColorPalette.childrenRoom,
  },
  {
    id: "play-room",
    nameKey: "catalog.rooms.play-room",
    icon: "🧩",
    ...houseRoomColorPalette.playRoom,
  },
  {
    id: "nursery",
    nameKey: "catalog.rooms.nursery",
    icon: "🍼",
    ...houseRoomColorPalette.nursery,
  },
  {
    id: "pet-room",
    nameKey: "catalog.rooms.pet-room",
    icon: "🐾",
    ...houseRoomColorPalette.petRoom,
  },
  {
    id: "bedroom",
    nameKey: "catalog.rooms.bedroom",
    icon: "🛏️",
    ...houseRoomColorPalette.bedroom,
  },
  {
    id: "wardrobe-room",
    nameKey: "catalog.rooms.wardrobe-room",
    icon: "👗",
    ...houseRoomColorPalette.wardrobeRoom,
  },
  {
    id: "gamer-room",
    nameKey: "catalog.rooms.gamer-room",
    icon: "🎮",
    ...houseRoomColorPalette.gamerRoom,
  },
  {
    id: "creative-room",
    nameKey: "catalog.rooms.creative-room",
    icon: "🎨",
    ...houseRoomColorPalette.creativeRoom,
  },
  {
    id: "makeup-room",
    nameKey: "catalog.rooms.makeup-room",
    icon: "🪞",
    ...houseRoomColorPalette.makeupRoom,
  },
];
export const FURNITURE_SETS: ReadonlyArray<FurnitureSetDefinition> = [
  {
    id: "cozy-living-set",
    nameKey: "catalog.sets.cozy-living-set.name",
    descriptionKey: "catalog.sets.cozy-living-set.description",
    icon: "🛋️",
  },
  {
    id: "family-kitchen-set",
    nameKey: "catalog.sets.family-kitchen-set.name",
    descriptionKey: "catalog.sets.family-kitchen-set.description",
    icon: "🍳",
  },
  {
    id: "adventure-kids-set",
    nameKey: "catalog.sets.adventure-kids-set.name",
    descriptionKey: "catalog.sets.adventure-kids-set.description",
    icon: "🧸",
  },
  {
    id: "calm-bedroom-set",
    nameKey: "catalog.sets.calm-bedroom-set.name",
    descriptionKey: "catalog.sets.calm-bedroom-set.description",
    icon: "🛏️",
  },
  {
    id: "creative-studio-set",
    nameKey: "catalog.sets.creative-studio-set.name",
    descriptionKey: "catalog.sets.creative-studio-set.description",
    icon: "🎨",
  },
  {
    id: "pet-comfort-set",
    nameKey: "catalog.sets.pet-comfort-set.name",
    descriptionKey: "catalog.sets.pet-comfort-set.description",
    icon: "🐾",
  },
  {
    id: "summer-garden-set",
    nameKey: "catalog.sets.summer-garden-set.name",
    descriptionKey: "catalog.sets.summer-garden-set.description",
    icon: "🏊",
  },
  {
    id: "garden-play-set",
    nameKey: "catalog.sets.garden-play-set.name",
    descriptionKey: "catalog.sets.garden-play-set.description",
    icon: "🤸",
  },
  {
    id: "halloween-exterior-set",
    nameKey: "catalog.sets.halloween-exterior-set.name",
    descriptionKey: "catalog.sets.halloween-exterior-set.description",
    icon: "🎃",
  },
  {
    id: "cotton-candy-exterior-set",
    nameKey: "catalog.sets.cotton-candy-exterior-set.name",
    descriptionKey: "catalog.sets.cotton-candy-exterior-set.description",
    icon: "🍭",
  },
];
type HouseThemeSeed = Omit<HouseThemeDefinition, "descriptionKey" | "kind" | "nameKey"> & {
  readonly kind?: HouseThemeDefinition["kind"];
};
export function defineHouseTheme(seed: HouseThemeSeed): HouseThemeDefinition {
  return {
    ...seed,
    descriptionKey: `catalog.themes.${seed.id}.description`,
    kind: seed.kind ?? "standard",
    nameKey: `catalog.themes.${seed.id}.name`,
  };
}
export const HOUSE_THEMES: HouseThemeDefinition[] = [
  defineHouseTheme({
    id: "sunny-dollhouse",
    icon: "🏡",
    ...houseThemeColorPalette.sunnyDollhouse,
  }),
  defineHouseTheme({
    id: "halloween-night",
    icon: "🎃",
    kind: "seasonal",
    ...houseThemeColorPalette.halloweenNight,
  }),
  defineHouseTheme({
    id: "christmas-wonderland",
    icon: "🎄",
    kind: "seasonal",
    ...houseThemeColorPalette.christmasWonderland,
  }),
  defineHouseTheme({
    id: "cotton-candy-dream",
    icon: "🍬",
    kind: "fantasy",
    ...houseThemeColorPalette.cottonCandyDream,
  }),
  defineHouseTheme({
    id: "starlight-palace",
    icon: "👑",
    kind: "fantasy",
    ...houseThemeColorPalette.starlightPalace,
  }),
  // design-generator:house-theme-definition
];
export type HouseAccessoryDefinition = Omit<
  HouseAccessory,
  "equipped" | "minimumHouseLevel" | "owned" | "price" | "purchasableByMemberIds"
>;
type HouseAccessorySeed = Omit<
  HouseAccessoryDefinition,
  | "category"
  | "description"
  | "mobility"
  | "motion"
  | "placement"
  | "roomId"
  | "scene"
  | "title"
  | "translationKey"
  | "visual"
> & {
  readonly motion?: NonNullable<HouseAccessory["motion"]>;
  readonly mobility?: HouseAccessory["mobility"];
  readonly visual: FurnitureVisualId;
};
type IndoorFurnitureSeed = HouseAccessorySeed & {
  readonly roomId: HouseRoomId;
};
type OutdoorAccessorySeed = HouseAccessorySeed & {
  readonly category?: Exclude<HouseAccessoryCategory, "furniture">;
};
function defineHouseAccessory(
  seed: HouseAccessorySeed,
  placement: HouseAccessory["placement"],
  category: HouseAccessoryCategory,
  roomId?: HouseRoomId,
): HouseAccessoryDefinition {
  const { mobility = "movable", motion = "none", ...accessory } = seed;

  return {
    ...accessory,
    category,
    description: "",
    mobility,
    motion,
    placement,
    ...(roomId ? { roomId } : {}),
    title: "",
    translationKey: `catalog.accessories.${seed.id}`,
  };
}
export function defineIndoorFurniture(seed: IndoorFurnitureSeed): HouseAccessoryDefinition {
  return defineHouseAccessory(seed, "inside", "furniture", seed.roomId);
}
export function defineOutdoorAccessory(seed: OutdoorAccessorySeed): HouseAccessoryDefinition {
  return defineHouseAccessory(seed, "outside", seed.category ?? "garden");
}
export const HOUSE_ACCESSORY_DEFINITIONS: ReadonlyArray<HouseAccessoryDefinition> = [
  defineOutdoorAccessory({
    id: "flower-boxes",
    icon: "🌺",
    visual: "flower-box",
  }),
  defineOutdoorAccessory({
    id: "garden-lights",
    icon: "🏮",
    visual: "string-lights",
  }),
  defineOutdoorAccessory({
    id: "hammock",
    icon: "🏖️",
    visual: "hammock",
  }),
  defineOutdoorAccessory({
    id: "telescope",
    icon: "🔭",
    visual: "telescope",
  }),
  defineIndoorFurniture({
    id: "round-rug",
    icon: "🟠",
    roomId: "living-room",
    visual: "rug",
  }),
  defineIndoorFurniture({
    id: "cozy-sofa",
    icon: "🛋️",
    roomId: "living-room",
    visual: "sofa",
  }),
  defineIndoorFurniture({
    id: "bookshelf",
    icon: "📚",
    roomId: "children-room",
    visual: "bookshelf",
  }),
  defineIndoorFurniture({
    id: "floor-lamp",
    icon: "💡",
    roomId: "living-room",
    visual: "lamp",
  }),
  defineIndoorFurniture({
    id: "bunk-bed",
    icon: "🛏️",
    roomId: "children-room",
    visual: "bunk-bed",
  }),
  defineIndoorFurniture({
    id: "play-table",
    icon: "🧩",
    roomId: "children-room",
    visual: "table",
  }),
  defineIndoorFurniture({
    id: "plant-corner",
    icon: "🪴",
    roomId: "creative-room",
    visual: "plant",
  }),
  defineIndoorFurniture({
    id: "wall-art",
    icon: "🖼️",
    roomId: "living-room",
    visual: "wall-art",
  }),
  defineIndoorFurniture({
    id: "cat-tree",
    icon: "🐈",
    roomId: "living-room",
    visual: "cat-tree",
  }),
  defineIndoorFurniture({
    id: "cat-bed",
    icon: "🐾",
    roomId: "living-room",
    visual: "pet-bed",
  }),
  defineIndoorFurniture({
    id: "dog-blanket",
    icon: "🦴",
    roomId: "living-room",
    visual: "dog-blanket",
  }),
  defineIndoorFurniture({
    id: "pet-bowl",
    icon: "🥣",
    roomId: "living-room",
    visual: "food-bowl",
  }),
  defineIndoorFurniture({
    id: "kitchen-counter",
    icon: "🍳",
    roomId: "kitchen",
    mobility: "fixed",
    visual: "kitchen-counter",
  }),
  defineIndoorFurniture({
    id: "retro-fridge",
    icon: "🧊",
    roomId: "kitchen",
    mobility: "fixed",
    interaction: "toggle-door",
    visual: "retro-fridge",
  }),
  defineIndoorFurniture({
    id: "family-dining-table",
    icon: "🍽️",
    roomId: "kitchen",
    visual: "dining-table",
  }),
  defineIndoorFurniture({
    id: "double-bed",
    icon: "🛏️",
    roomId: "bedroom",
    visual: "double-bed",
  }),
  defineIndoorFurniture({
    id: "wardrobe",
    icon: "🚪",
    roomId: "bedroom",
    visual: "wardrobe",
  }),
  defineIndoorFurniture({
    id: "bedside-table",
    icon: "🕯️",
    roomId: "bedroom",
    visual: "bedside-table",
  }),
  defineIndoorFurniture({
    id: "art-desk",
    icon: "🎨",
    roomId: "creative-room",
    visual: "art-desk",
  }),
  defineIndoorFurniture({
    id: "storage-cabinet",
    icon: "🗄️",
    roomId: "creative-room",
    visual: "storage-cabinet",
  }),
  defineOutdoorAccessory({
    id: "garden-pool",
    icon: "🏊",
    visual: "pool",
  }),
  defineOutdoorAccessory({
    id: "garden-chair",
    icon: "🪑",
    visual: "garden-chair",
  }),
  defineOutdoorAccessory({
    id: "garden-table",
    icon: "🍹",
    visual: "garden-table",
  }),
  defineOutdoorAccessory({
    id: "garden-trampoline",
    icon: "🤸",
    visual: "trampoline",
  }),
  defineOutdoorAccessory({
    id: "garden-sunshade",
    icon: "⛱️",
    visual: "sunshade",
  }),
  defineOutdoorAccessory({
    id: "halloween-pumpkin-arch",
    category: "special",
    icon: "🎃",
    motion: "glow",
    visual: "pumpkin-arch",
  }),
  defineOutdoorAccessory({
    id: "halloween-bat-garland",
    category: "special",
    icon: "🦇",
    motion: "flutter",
    visual: "bat-garland",
  }),
  defineOutdoorAccessory({
    id: "candy-cloud-bushes",
    icon: "🍬",
    visual: "candy-bush",
  }),
  defineOutdoorAccessory({
    id: "candy-striped-fence",
    icon: "🍭",
    visual: "candy-fence",
  }),
  defineOutdoorAccessory({
    id: "family-flag",
    category: "special",
    icon: "🚩",
    motion: "wave",
    visual: "family-flag",
  }),
  // design-generator:house-accessory-definition
];
export function createHouseAccessories(
  states: ReadonlyArray<Pick<HouseAccessory, "equipped" | "id" | "owned">> = [],
  catalogEntries: ReadonlyArray<{
    readonly id: string;
    readonly minimumHouseLevel?: number;
    readonly price?: number;
    readonly purchasableByMemberIds?: ReadonlyArray<string>;
  }> = [],
): HouseAccessory[] {
  const stateById = new Map(states.map((state) => [state.id, state]));
  const catalogById = new Map(catalogEntries.map((entry) => [entry.id, entry]));

  return HOUSE_ACCESSORY_DEFINITIONS.map(function callback(definition) {
    const state = stateById.get(definition.id);
    const catalogEntry = catalogById.get(definition.id);

    return {
      ...definition,
      equipped: state?.equipped ?? false,
      minimumHouseLevel: catalogEntry?.minimumHouseLevel as HouseStageLevel | undefined,
      owned: state?.owned ?? false,
      price: catalogEntry?.price ?? 0,
      purchasableByMemberIds: catalogEntry?.purchasableByMemberIds ?? [],
    };
  });
}
