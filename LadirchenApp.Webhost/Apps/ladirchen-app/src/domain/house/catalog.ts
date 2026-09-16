import type { FurniturePlacement, FurnitureSetDefinition, FurnitureVisualId, HouseAccessoryCategory, HouseRoomDefinition, HouseRoomId, HouseStageDefinition, HouseThemeDefinition, HouseZoneId } from './types';
import { createDomainId } from '../shared/identifiers';
import type { HouseAccessory, HouseLayoutPlacement } from './entities';
import { houseRoomColorPalette, houseThemeColorPalette } from '@/theme/color-palette';

export const HOUSE_STAGES: HouseStageDefinition[] = [
  { id: 'starter-home', level: 0, nameKey: 'catalog.stages.starter-home', icon: '🏠', rooms: 2 },
  { id: 'family-home', level: 1, nameKey: 'catalog.stages.family-home', icon: '🏡', rooms: 4 },
  { id: 'garden-home', level: 2, nameKey: 'catalog.stages.garden-home', icon: '🏘️', rooms: 7 },
  { id: 'tower-home', level: 3, nameKey: 'catalog.stages.tower-home', icon: '🏰', rooms: 10 },
  { id: 'dream-home', level: 4, nameKey: 'catalog.stages.dream-home', icon: '👑', rooms: 12 },
];

export const DEFAULT_HOUSE_STAGE = HOUSE_STAGES[0]!;

export const HOUSE_ROOMS: ReadonlyArray<HouseRoomDefinition> = [
  { id: 'living-room', nameKey: 'catalog.rooms.living-room', icon: '🛋️', minimumHouseLevel: 0, ...houseRoomColorPalette.livingRoom },
  { id: 'kitchen', nameKey: 'catalog.rooms.kitchen', icon: '🍳', minimumHouseLevel: 0, ...houseRoomColorPalette.kitchen },
  { id: 'bathroom', nameKey: 'catalog.rooms.bathroom', icon: '🛁', minimumHouseLevel: 1, ...houseRoomColorPalette.bathroom },
  { id: 'children-room', nameKey: 'catalog.rooms.children-room', icon: '🧸', minimumHouseLevel: 1, ...houseRoomColorPalette.childrenRoom },
  { id: 'play-room', nameKey: 'catalog.rooms.play-room', icon: '🧩', minimumHouseLevel: 2, ...houseRoomColorPalette.playRoom },
  { id: 'nursery', nameKey: 'catalog.rooms.nursery', icon: '🍼', minimumHouseLevel: 2, ...houseRoomColorPalette.nursery },
  { id: 'pet-room', nameKey: 'catalog.rooms.pet-room', icon: '🐾', minimumHouseLevel: 2, ...houseRoomColorPalette.petRoom },
  { id: 'bedroom', nameKey: 'catalog.rooms.bedroom', icon: '🛏️', minimumHouseLevel: 3, ...houseRoomColorPalette.bedroom },
  { id: 'wardrobe-room', nameKey: 'catalog.rooms.wardrobe-room', icon: '👗', minimumHouseLevel: 3, ...houseRoomColorPalette.wardrobeRoom },
  { id: 'gamer-room', nameKey: 'catalog.rooms.gamer-room', icon: '🎮', minimumHouseLevel: 3, ...houseRoomColorPalette.gamerRoom },
  { id: 'creative-room', nameKey: 'catalog.rooms.creative-room', icon: '🎨', minimumHouseLevel: 4, ...houseRoomColorPalette.creativeRoom },
  { id: 'makeup-room', nameKey: 'catalog.rooms.makeup-room', icon: '🪞', minimumHouseLevel: 4, ...houseRoomColorPalette.makeupRoom },
];

export const FURNITURE_SETS: ReadonlyArray<FurnitureSetDefinition> = [
  { id: 'cozy-living-set', nameKey: 'catalog.sets.cozy-living-set.name', descriptionKey: 'catalog.sets.cozy-living-set.description', icon: '🛋️', price: 250, zoneId: 'living-room', minimumHouseLevel: 0, accessoryIds: ['cozy-sofa', 'round-rug', 'floor-lamp'] },
  { id: 'family-kitchen-set', nameKey: 'catalog.sets.family-kitchen-set.name', descriptionKey: 'catalog.sets.family-kitchen-set.description', icon: '🍳', price: 420, zoneId: 'kitchen', minimumHouseLevel: 0, accessoryIds: ['kitchen-counter', 'retro-fridge', 'family-dining-table'] },
  { id: 'adventure-kids-set', nameKey: 'catalog.sets.adventure-kids-set.name', descriptionKey: 'catalog.sets.adventure-kids-set.description', icon: '🧸', price: 310, zoneId: 'children-room', minimumHouseLevel: 2, accessoryIds: ['bunk-bed', 'play-table', 'bookshelf'] },
  { id: 'calm-bedroom-set', nameKey: 'catalog.sets.calm-bedroom-set.name', descriptionKey: 'catalog.sets.calm-bedroom-set.description', icon: '🛏️', price: 390, zoneId: 'bedroom', minimumHouseLevel: 3, accessoryIds: ['double-bed', 'wardrobe', 'bedside-table'] },
  { id: 'creative-studio-set', nameKey: 'catalog.sets.creative-studio-set.name', descriptionKey: 'catalog.sets.creative-studio-set.description', icon: '🎨', price: 280, zoneId: 'creative-room', minimumHouseLevel: 4, accessoryIds: ['art-desk', 'storage-cabinet', 'plant-corner'] },
  { id: 'pet-comfort-set', nameKey: 'catalog.sets.pet-comfort-set.name', descriptionKey: 'catalog.sets.pet-comfort-set.description', icon: '🐾', price: 260, zoneId: 'living-room', minimumHouseLevel: 0, accessoryIds: ['cat-tree', 'cat-bed', 'dog-blanket', 'pet-bowl'] },
  { id: 'summer-garden-set', nameKey: 'catalog.sets.summer-garden-set.name', descriptionKey: 'catalog.sets.summer-garden-set.description', icon: '🏊', price: 520, zoneId: 'garden', minimumHouseLevel: 1, accessoryIds: ['garden-pool', 'garden-chair', 'garden-table', 'garden-sunshade'] },
  { id: 'garden-play-set', nameKey: 'catalog.sets.garden-play-set.name', descriptionKey: 'catalog.sets.garden-play-set.description', icon: '🤸', price: 340, zoneId: 'garden', minimumHouseLevel: 2, accessoryIds: ['garden-trampoline', 'hammock', 'garden-lights'] },
  { id: 'halloween-exterior-set', nameKey: 'catalog.sets.halloween-exterior-set.name', descriptionKey: 'catalog.sets.halloween-exterior-set.description', icon: '🎃', price: 190, zoneId: 'garden', minimumHouseLevel: 0, accessoryIds: ['halloween-pumpkin-arch', 'halloween-bat-garland'] },
  { id: 'cotton-candy-exterior-set', nameKey: 'catalog.sets.cotton-candy-exterior-set.name', descriptionKey: 'catalog.sets.cotton-candy-exterior-set.description', icon: '🍭', price: 210, zoneId: 'garden', minimumHouseLevel: 0, accessoryIds: ['candy-cloud-bushes', 'candy-striped-fence'] },
];

type HouseThemeSeed = Omit<HouseThemeDefinition, 'descriptionKey' | 'kind' | 'nameKey' | 'ownedByDefault'> & {
  readonly kind?: HouseThemeDefinition['kind'];
  readonly ownedByDefault?: boolean;
};

export const defineHouseTheme = (seed: HouseThemeSeed): HouseThemeDefinition => ({
  ...seed,
  descriptionKey: `catalog.themes.${seed.id}.description`,
  kind: seed.kind ?? 'standard',
  nameKey: `catalog.themes.${seed.id}.name`,
  ownedByDefault: seed.ownedByDefault ?? false,
});

export const HOUSE_THEMES: HouseThemeDefinition[] = [
  defineHouseTheme({
    id: 'sunny-dollhouse',
    icon: '🏡',
    price: 0,
    ownedByDefault: true,
    ...houseThemeColorPalette.sunnyDollhouse,
  }),
  defineHouseTheme({
    id: 'halloween-night',
    icon: '🎃',
    price: 320,
    kind: 'seasonal',
    ...houseThemeColorPalette.halloweenNight,
  }),
  defineHouseTheme({
    id: 'christmas-wonderland',
    icon: '🎄',
    price: 340,
    kind: 'seasonal',
    ...houseThemeColorPalette.christmasWonderland,
  }),
  defineHouseTheme({
    id: 'cotton-candy-dream',
    icon: '🍬',
    price: 360,
    kind: 'fantasy',
    ...houseThemeColorPalette.cottonCandyDream,
  }),
  defineHouseTheme({
    id: 'starlight-palace',
    icon: '👑',
    price: 420,
    kind: 'fantasy',
    ...houseThemeColorPalette.starlightPalace,
  }),
  // design-generator:house-theme-definition
];

export interface HouseAccessoryDefinition extends HouseAccessory {
  readonly defaultPlacement: Required<FurniturePlacement> & { readonly zoneId: HouseZoneId };
}

type HouseAccessorySeed = Omit<HouseAccessory, 'category' | 'description' | 'equipped' | 'mobility' | 'motion' | 'owned' | 'placement' | 'roomId' | 'scene' | 'title' | 'translationKey' | 'visual'> & {
  readonly defaultPlacement: Required<FurniturePlacement>;
  readonly equipped?: boolean;
  readonly motion?: HouseAccessory['motion'];
  readonly mobility?: HouseAccessory['mobility'];
  readonly owned?: boolean;
  readonly visual: FurnitureVisualId;
};

type IndoorFurnitureSeed = HouseAccessorySeed & { readonly roomId: HouseRoomId };
type OutdoorAccessorySeed = HouseAccessorySeed & { readonly category?: Exclude<HouseAccessoryCategory, 'furniture'> };

const defineHouseAccessory = (
  seed: HouseAccessorySeed,
  placement: HouseAccessory['placement'],
  zoneId: HouseZoneId,
  category: HouseAccessoryCategory,
  roomId?: HouseRoomId,
): HouseAccessoryDefinition => {
  const { defaultPlacement, equipped = false, minimumHouseLevel = 0, mobility = 'movable', motion = 'none', owned = false, ...accessory } = seed;
  return {
    ...accessory,
    category,
    defaultPlacement: { ...defaultPlacement, zoneId },
    description: '',
    equipped,
    minimumHouseLevel,
    mobility,
    motion,
    owned,
    placement,
    ...(roomId ? { roomId } : {}),
    title: '',
    translationKey: `catalog.accessories.${seed.id}`,
  };
};

export const defineIndoorFurniture = (seed: IndoorFurnitureSeed): HouseAccessoryDefinition =>
  defineHouseAccessory(seed, 'inside', seed.roomId, 'furniture', seed.roomId);

export const defineOutdoorAccessory = (seed: OutdoorAccessorySeed): HouseAccessoryDefinition =>
  defineHouseAccessory(seed, 'outside', 'garden', seed.category ?? 'garden');

export const HOUSE_ACCESSORY_DEFINITIONS: ReadonlyArray<HouseAccessoryDefinition> = [
  defineOutdoorAccessory({ id: 'flower-boxes', icon: '🌺', price: 60, visual: 'flower-box', owned: true, equipped: true, defaultPlacement: { x: 18, y: 77, scale: .72 } }),
  defineOutdoorAccessory({ id: 'garden-lights', icon: '🏮', price: 120, setIds: ['garden-play-set'], visual: 'string-lights', defaultPlacement: { x: 47, y: 24, scale: .82 } }),
  defineOutdoorAccessory({ id: 'hammock', icon: '🏖️', price: 160, setIds: ['garden-play-set'], visual: 'hammock', defaultPlacement: { x: 22, y: 72, scale: .78 } }),
  defineOutdoorAccessory({ id: 'telescope', icon: '🔭', price: 260, visual: 'telescope', defaultPlacement: { x: 88, y: 57, scale: .72 } }),
  defineIndoorFurniture({ id: 'round-rug', icon: '🟠', price: 70, roomId: 'living-room', setIds: ['cozy-living-set'], visual: 'rug', owned: true, equipped: true, defaultPlacement: { x: 49, y: 76, scale: 1 } }),
  defineIndoorFurniture({ id: 'cozy-sofa', icon: '🛋️', price: 140, roomId: 'living-room', setIds: ['cozy-living-set'], visual: 'sofa', owned: true, equipped: true, defaultPlacement: { x: 70, y: 52, scale: .9 } }),
  defineIndoorFurniture({ id: 'bookshelf', icon: '📚', price: 110, roomId: 'children-room', setIds: ['adventure-kids-set'], minimumHouseLevel: 2, visual: 'bookshelf', defaultPlacement: { x: 85, y: 46, scale: .76 } }),
  defineIndoorFurniture({ id: 'floor-lamp', icon: '💡', price: 90, roomId: 'living-room', setIds: ['cozy-living-set'], visual: 'lamp', defaultPlacement: { x: 88, y: 48, scale: .82 } }),
  defineIndoorFurniture({ id: 'bunk-bed', icon: '🛏️', price: 180, roomId: 'children-room', setIds: ['adventure-kids-set'], minimumHouseLevel: 2, visual: 'bunk-bed', defaultPlacement: { x: 22, y: 55, scale: .8 } }),
  defineIndoorFurniture({ id: 'play-table', icon: '🧩', price: 95, roomId: 'children-room', setIds: ['adventure-kids-set'], minimumHouseLevel: 2, visual: 'table', defaultPlacement: { x: 65, y: 76, scale: .78 } }),
  defineIndoorFurniture({ id: 'plant-corner', icon: '🪴', price: 55, roomId: 'creative-room', setIds: ['creative-studio-set'], visual: 'plant', defaultPlacement: { x: 17, y: 62, scale: .78 } }),
  defineIndoorFurniture({ id: 'wall-art', icon: '🖼️', price: 45, roomId: 'living-room', visual: 'wall-art', defaultPlacement: { x: 22, y: 25, scale: .78 } }),
  defineIndoorFurniture({ id: 'cat-tree', icon: '🐈', price: 135, roomId: 'living-room', setIds: ['pet-comfort-set'], visual: 'cat-tree', defaultPlacement: { x: 14, y: 59, scale: .8 } }),
  defineIndoorFurniture({ id: 'cat-bed', icon: '🐾', price: 65, roomId: 'living-room', setIds: ['pet-comfort-set'], visual: 'pet-bed', defaultPlacement: { x: 26, y: 82, scale: .72 } }),
  defineIndoorFurniture({ id: 'dog-blanket', icon: '🦴', price: 75, roomId: 'living-room', setIds: ['pet-comfort-set'], visual: 'dog-blanket', defaultPlacement: { x: 75, y: 84, scale: .72 } }),
  defineIndoorFurniture({ id: 'pet-bowl', icon: '🥣', price: 50, roomId: 'living-room', setIds: ['pet-comfort-set'], visual: 'food-bowl', defaultPlacement: { x: 90, y: 82, scale: .7 } }),
  defineIndoorFurniture({ id: 'kitchen-counter', icon: '🍳', price: 210, roomId: 'kitchen', setIds: ['family-kitchen-set'], mobility: 'fixed', visual: 'kitchen-counter', owned: true, equipped: true, defaultPlacement: { x: 38, y: 61, scale: 1.2 } }),
  defineIndoorFurniture({ id: 'retro-fridge', icon: '🧊', price: 150, roomId: 'kitchen', setIds: ['family-kitchen-set'], mobility: 'fixed', interaction: 'toggle-door', visual: 'retro-fridge', owned: true, equipped: true, defaultPlacement: { x: 51, y: 54, scale: .6 } }),
  defineIndoorFurniture({ id: 'family-dining-table', icon: '🍽️', price: 130, roomId: 'kitchen', setIds: ['family-kitchen-set'], visual: 'dining-table', owned: true, equipped: true, defaultPlacement: { x: 48, y: 74, scale: .95 } }),
  defineIndoorFurniture({ id: 'double-bed', icon: '🛏️', price: 190, roomId: 'bedroom', setIds: ['calm-bedroom-set'], minimumHouseLevel: 3, visual: 'double-bed', defaultPlacement: { x: 48, y: 65, scale: .88 } }),
  defineIndoorFurniture({ id: 'wardrobe', icon: '🚪', price: 150, roomId: 'bedroom', setIds: ['calm-bedroom-set'], minimumHouseLevel: 3, visual: 'wardrobe', defaultPlacement: { x: 84, y: 48, scale: .76 } }),
  defineIndoorFurniture({ id: 'bedside-table', icon: '🕯️', price: 80, roomId: 'bedroom', setIds: ['calm-bedroom-set'], minimumHouseLevel: 3, visual: 'bedside-table', defaultPlacement: { x: 18, y: 66, scale: .7 } }),
  defineIndoorFurniture({ id: 'art-desk', icon: '🎨', price: 140, roomId: 'creative-room', setIds: ['creative-studio-set'], minimumHouseLevel: 4, visual: 'art-desk', defaultPlacement: { x: 54, y: 69, scale: .82 } }),
  defineIndoorFurniture({ id: 'storage-cabinet', icon: '🗄️', price: 120, roomId: 'creative-room', setIds: ['creative-studio-set'], minimumHouseLevel: 4, visual: 'storage-cabinet', defaultPlacement: { x: 84, y: 49, scale: .75 } }),
  defineOutdoorAccessory({ id: 'garden-pool', icon: '🏊', price: 260, setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'pool', defaultPlacement: { x: 25, y: 59, scale: .9 } }),
  defineOutdoorAccessory({ id: 'garden-chair', icon: '🪑', price: 95, setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'garden-chair', defaultPlacement: { x: 68, y: 66, scale: .76 } }),
  defineOutdoorAccessory({ id: 'garden-table', icon: '🍹', price: 110, setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'garden-table', defaultPlacement: { x: 82, y: 69, scale: .74 } }),
  defineOutdoorAccessory({ id: 'garden-trampoline', icon: '🤸', price: 220, setIds: ['garden-play-set'], minimumHouseLevel: 2, visual: 'trampoline', defaultPlacement: { x: 74, y: 38, scale: .82 } }),
  defineOutdoorAccessory({ id: 'garden-sunshade', icon: '⛱️', price: 105, setIds: ['summer-garden-set'], minimumHouseLevel: 1, visual: 'sunshade', defaultPlacement: { x: 49, y: 48, scale: .8 } }),
  defineOutdoorAccessory({ id: 'halloween-pumpkin-arch', category: 'special', icon: '🎃', price: 120, setIds: ['halloween-exterior-set'], motion: 'glow', visual: 'pumpkin-arch', defaultPlacement: { x: 48, y: 66, scale: .78 } }),
  defineOutdoorAccessory({ id: 'halloween-bat-garland', category: 'special', icon: '🦇', price: 95, setIds: ['halloween-exterior-set'], motion: 'flutter', visual: 'bat-garland', defaultPlacement: { x: 49, y: 23, scale: .9 } }),
  defineOutdoorAccessory({ id: 'candy-cloud-bushes', icon: '🍬', price: 125, setIds: ['cotton-candy-exterior-set'], visual: 'candy-bush', defaultPlacement: { x: 30, y: 66, scale: .86 } }),
  defineOutdoorAccessory({ id: 'candy-striped-fence', icon: '🍭', price: 110, setIds: ['cotton-candy-exterior-set'], visual: 'candy-fence', defaultPlacement: { x: 68, y: 70, scale: .82 } }),
  defineOutdoorAccessory({ id: 'family-flag', category: 'special', icon: '🚩', price: 180, motion: 'wave', visual: 'family-flag', defaultPlacement: { x: 84, y: 62, scale: .9 } }),
  // design-generator:house-accessory-definition
];

export const createHouseAccessories = (): HouseAccessory[] => HOUSE_ACCESSORY_DEFINITIONS.map((definition) => {
  const { defaultPlacement: _defaultPlacement, ...accessory } = definition;
  return { ...accessory };
});

export const createHouseAccessoryLayoutPlacements = (): HouseLayoutPlacement[] =>
  HOUSE_ACCESSORY_DEFINITIONS.map(({ defaultPlacement, id }) => ({
    id: createDomainId.houseLayoutPlacement(`furniture-${id}`),
    entityId: id,
    entityType: 'furniture',
    ...defaultPlacement,
  }));
