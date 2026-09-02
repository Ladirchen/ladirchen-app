export type FurnitureVisualId = 'rug' | 'sofa' | 'bookshelf' | 'lamp' | 'bunk-bed' | 'table' | 'plant' | 'wall-art' | 'cat-tree' | 'pet-bed' | 'dog-blanket' | 'food-bowl';

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
  | 'pet-bowl';

export type HouseAccessoryPlacement = 'inside' | 'outside';
export type HouseStageId = 'starter-home' | 'family-home' | 'garden-home' | 'tower-home' | 'dream-home';
export type HouseStageLevel = 0 | 1 | 2 | 3 | 4;
export type HouseRoomCount = 1 | 2 | 3 | 4 | 5;
export type HouseThemeId = 'sunny-dollhouse';
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

export interface HouseThemeDefinition {
  readonly id: HouseThemeId;
  readonly name: string;
  readonly wall: HexColor;
  readonly wallUpper: HexColor;
  readonly floor: HexColor;
  readonly roof: HexColor;
  readonly roofShade: HexColor;
  readonly trim: HexColor;
}
