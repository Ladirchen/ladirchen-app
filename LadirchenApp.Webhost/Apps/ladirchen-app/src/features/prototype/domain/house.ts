export type FurnitureVisual = 'rug' | 'sofa' | 'bookshelf' | 'lamp' | 'bunk-bed' | 'table' | 'plant' | 'wall-art' | 'cat-tree' | 'pet-bed' | 'dog-blanket' | 'food-bowl';

export interface FurniturePlacement {
  x: number;
  y: number;
  scale?: number;
}

export interface HouseStageDefinition {
  level: number;
  name: string;
  icon: string;
  rooms: number;
}

export interface HouseThemeDefinition {
  id: string;
  name: string;
  wall: string;
  wallUpper: string;
  floor: string;
  roof: string;
  roofShade: string;
  trim: string;
}
