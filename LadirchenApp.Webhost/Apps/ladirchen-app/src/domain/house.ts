export type FurnitureVisualId =
  | 'rug'
  | 'sofa'
  | 'bookshelf'
  | 'lamp'
  | 'bunk-bed'
  | 'table'
  | 'plant'
  | 'wall-art'
  | 'cat-tree'
  | 'pet-bed'
  | 'dog-blanket'
  | 'food-bowl'
  | 'kitchen-counter'
  | 'retro-fridge'
  | 'dining-table'
  | 'double-bed'
  | 'wardrobe'
  | 'bedside-table'
  | 'art-desk'
  | 'storage-cabinet'
  | 'pool'
  | 'garden-chair'
  | 'garden-table'
  | 'trampoline'
  | 'sunshade'
  | 'flower-box'
  | 'string-lights'
  | 'hammock'
  | 'telescope'
  | 'pumpkin-arch'
  | 'bat-garland'
  | 'candy-bush'
  | 'candy-fence';

export type HouseAccessoryId =
  | 'flower-boxes'
  | 'garden-lights'
  | 'hammock'
  | 'telescope'
  | 'round-rug'
  | 'cozy-sofa'
  | 'bookshelf'
  | 'floor-lamp'
  | 'bunk-bed'
  | 'play-table'
  | 'plant-corner'
  | 'wall-art'
  | 'cat-tree'
  | 'cat-bed'
  | 'dog-blanket'
  | 'pet-bowl'
  | 'kitchen-counter'
  | 'retro-fridge'
  | 'family-dining-table'
  | 'double-bed'
  | 'wardrobe'
  | 'bedside-table'
  | 'art-desk'
  | 'storage-cabinet'
  | 'garden-pool'
  | 'garden-chair'
  | 'garden-table'
  | 'garden-trampoline'
  | 'garden-sunshade'
  | 'halloween-pumpkin-arch'
  | 'halloween-bat-garland'
  | 'candy-cloud-bushes'
  | 'candy-striped-fence';

export type HouseAccessoryPlacement = 'inside' | 'outside';
export type HouseRoomId = 'living-room' | 'kitchen' | 'children-room' | 'bedroom' | 'creative-room';
export type HouseZoneId = HouseRoomId | 'garden';
export type FurnitureSetId = 'cozy-living-set' | 'family-kitchen-set' | 'adventure-kids-set' | 'calm-bedroom-set' | 'creative-studio-set' | 'pet-comfort-set' | 'summer-garden-set' | 'garden-play-set' | 'halloween-exterior-set' | 'cotton-candy-exterior-set';
export type HouseLayoutEntityType = 'furniture' | 'member' | 'pet' | 'ladi';
export type HouseStageId = 'starter-home' | 'family-home' | 'garden-home' | 'tower-home' | 'dream-home';
export type HouseStageLevel = 0 | 1 | 2 | 3 | 4;
export type HouseRoomCount = 1 | 2 | 3 | 4 | 5;
export type HouseThemeId = 'sunny-dollhouse' | 'halloween-night' | 'cotton-candy-dream';
export type HouseEditionKind = 'standard' | 'seasonal' | 'fantasy';
export type HexColor = `#${string}`;

export interface FurniturePlacement {
  readonly x: number;
  readonly y: number;
  readonly scale?: number;
}

export interface HouseStageDefinition {
  readonly id: HouseStageId;
  readonly level: HouseStageLevel;
  readonly name: string;
  readonly icon: string;
  readonly rooms: HouseRoomCount;
}

export interface HouseRoomDefinition {
  readonly id: HouseRoomId;
  readonly name: string;
  readonly icon: string;
  readonly minimumHouseLevel: HouseStageLevel;
  readonly wall: HexColor;
  readonly floor: HexColor;
}

export interface FurnitureSetDefinition {
  readonly id: FurnitureSetId;
  readonly name: string;
  readonly description: string;
  readonly icon: string;
  readonly price: number;
  readonly zoneId: HouseZoneId;
  readonly minimumHouseLevel: HouseStageLevel;
  readonly accessoryIds: ReadonlyArray<HouseAccessoryId>;
}

export interface HouseThemeDefinition {
  readonly id: HouseThemeId;
  readonly name: string;
  readonly description: string;
  readonly icon: string;
  readonly price: number;
  readonly kind: HouseEditionKind;
  readonly ownedByDefault: boolean;
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
