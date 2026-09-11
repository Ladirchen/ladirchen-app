import type { TranslationKey } from '@/locales/translation-keys';

/** Stable IDs and definitions used by the family house domain. */

export const FURNITURE_VISUAL_IDS = [
  'rug', 'sofa', 'bookshelf', 'lamp', 'bunk-bed', 'table', 'plant', 'wall-art', 'cat-tree', 'pet-bed',
  'dog-blanket', 'food-bowl', 'kitchen-counter', 'retro-fridge', 'dining-table', 'double-bed', 'wardrobe',
  'bedside-table', 'art-desk', 'storage-cabinet', 'pool', 'garden-chair', 'garden-table', 'trampoline',
  'sunshade', 'flower-box', 'string-lights', 'hammock', 'telescope', 'pumpkin-arch', 'bat-garland',
  'candy-bush', 'candy-fence', 'family-flag',
  // design-generator:furniture-visual-id
] as const;

export type FurnitureVisualId = typeof FURNITURE_VISUAL_IDS[number];

export const HOUSE_ACCESSORY_IDS = [
  'flower-boxes', 'garden-lights', 'hammock', 'telescope', 'round-rug', 'cozy-sofa', 'bookshelf', 'floor-lamp',
  'bunk-bed', 'play-table', 'plant-corner', 'wall-art', 'cat-tree', 'cat-bed', 'dog-blanket', 'pet-bowl',
  'kitchen-counter', 'retro-fridge', 'family-dining-table', 'double-bed', 'wardrobe', 'bedside-table', 'art-desk',
  'storage-cabinet', 'garden-pool', 'garden-chair', 'garden-table', 'garden-trampoline', 'garden-sunshade',
  'halloween-pumpkin-arch', 'halloween-bat-garland', 'candy-cloud-bushes', 'candy-striped-fence', 'family-flag',
  // design-generator:house-accessory-id
] as const;

export type HouseAccessoryId = typeof HOUSE_ACCESSORY_IDS[number];

export type HouseAccessoryCategory = 'furniture' | 'garden' | 'special';
export type HouseAccessoryMotion = 'flutter' | 'glow' | 'none' | 'wave';
export type HouseAccessoryMobility = 'fixed' | 'movable';
export type HouseAccessoryInteraction = 'toggle-door';
export type HouseAccessoryPlacement = 'inside' | 'outside';
export type HouseRoomId =
  | 'living-room'
  | 'kitchen'
  | 'bathroom'
  | 'children-room'
  | 'play-room'
  | 'nursery'
  | 'pet-room'
  | 'bedroom'
  | 'wardrobe-room'
  | 'gamer-room'
  | 'creative-room'
  | 'makeup-room';
export type HouseZoneId = HouseRoomId | 'garden';
export type FurnitureSetId = 'cozy-living-set' | 'family-kitchen-set' | 'adventure-kids-set' | 'calm-bedroom-set' | 'creative-studio-set' | 'pet-comfort-set' | 'summer-garden-set' | 'garden-play-set' | 'halloween-exterior-set' | 'cotton-candy-exterior-set';
export type HouseLayoutEntityType = 'furniture' | 'member' | 'pet' | 'ladi';
export type HouseStageId = 'starter-home' | 'family-home' | 'garden-home' | 'tower-home' | 'dream-home';
export type HouseStageLevel = 0 | 1 | 2 | 3 | 4;
export type HouseRoomCount = 2 | 4 | 7 | 10 | 12;
export const HOUSE_THEME_IDS = [
  'sunny-dollhouse', 'halloween-night', 'christmas-wonderland', 'cotton-candy-dream', 'starlight-palace',
  // design-generator:house-theme-id
] as const;
export type HouseThemeId = typeof HOUSE_THEME_IDS[number];
export type HouseEnergyVisualLevel = 1 | 2 | 3 | 4 | 5;
export type HouseExteriorBackgroundAssetId = `${HouseThemeId}-energy-${HouseEnergyVisualLevel}-exterior-background`;
export type HouseExteriorHouseAssetId = `${HouseStageId}-${HouseThemeId}-house`;
export type HouseExteriorAssetId = HouseExteriorBackgroundAssetId | HouseExteriorHouseAssetId;
export type HouseEditionKind = 'standard' | 'seasonal' | 'fantasy';
export type HexColor = `#${string}`;

export const resolveHouseEnergyVisualLevel = (energy: number): HouseEnergyVisualLevel => {
  if (energy < 20) { return 1; }
  if (energy < 40) { return 2; }
  if (energy < 65) { return 3; }
  if (energy < 85) { return 4; }
  return 5;
};

export interface CatalogNewBadgeSchedule {
  readonly from: string;
  readonly until: string;
}

export interface FurniturePlacement {
  readonly x: number;
  readonly y: number;
  readonly scale?: number;
}

export interface HouseStageDefinition {
  readonly id: HouseStageId;
  readonly level: HouseStageLevel;
  readonly nameKey: TranslationKey;
  readonly icon: string;
  readonly rooms: HouseRoomCount;
}

export interface HouseRoomDefinition {
  readonly id: HouseRoomId;
  readonly nameKey: TranslationKey;
  readonly icon: string;
  readonly minimumHouseLevel: HouseStageLevel;
  readonly wall: HexColor;
  readonly floor: HexColor;
}

export interface FurnitureSetDefinition {
  readonly id: FurnitureSetId;
  readonly nameKey: TranslationKey;
  readonly descriptionKey: TranslationKey;
  readonly icon: string;
  readonly price: number;
  readonly zoneId: HouseZoneId;
  readonly minimumHouseLevel: HouseStageLevel;
  readonly accessoryIds: ReadonlyArray<HouseAccessoryId>;
}

export interface HouseThemeDefinition {
  readonly id: HouseThemeId;
  readonly nameKey: TranslationKey;
  readonly descriptionKey: TranslationKey;
  readonly icon: string;
  readonly price: number;
  readonly kind: HouseEditionKind;
  readonly ownedByDefault: boolean;
  readonly newBadge?: CatalogNewBadgeSchedule;
  readonly wall: HexColor;
  readonly wallUpper: HexColor;
  readonly floor: HexColor;
  readonly roof: HexColor;
  readonly roofShade: HexColor;
  readonly trim: HexColor;
  readonly door: HexColor;
  readonly window: HexColor;
  readonly landscapeAccent: HexColor;
}
