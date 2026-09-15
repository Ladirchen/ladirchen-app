import type { HouseLayoutPlacementId } from '../shared/identifiers';
import type { HouseAccessory, HouseLayoutPlacement } from './entities';
import type { FurnitureVisualId, HouseLayoutEntityType, HouseZoneId } from './types';
import { furnitureVisualDefinitionFor } from './furniture-visuals';
import type { FurnitureCollisionBounds } from './furniture-visuals';

const CHARACTER_COLLISION_BOUNDS = { horizontalRadius: 4, verticalRadius: 6 } as const satisfies FurnitureCollisionBounds;

export const HOUSE_LAYOUT_CONSTRAINTS = Object.freeze({
  coordinateMaximum: 100,
  coordinatePrecision: 2,
  defaultGardenY: 68,
  defaultIndoorY: 66,
  defaultX: 50,
  floorHorizontalInset: 7,
  floorMinimumY: 62,
  furnitureHorizontalInset: 3,
  furnitureMinimumY: 52,
  maximumScale: 1.35,
  maximumX: 96,
  maximumY: 94,
  minimumScale: 0.5,
  minimumX: 4,
  minimumY: 8,
  perch: Object.freeze({
    maximumX: 31,
    maximumY: 47,
    minimumX: 6,
    minimumY: 12,
    persistedMaximumY: 38,
    persistedMinimumY: 22,
    proximityToleranceX: 7,
    proximityToleranceY: 8,
    x: 18,
    y: 30,
  }),
});

export const ENTITY_LAYER_BASE = {
  furniture: 100,
  pet: 400,
  member: 500,
  ladi: 600,
} as const satisfies Record<HouseLayoutEntityType, number>;

const collisionBoundsFor = (visual: FurnitureVisualId) => furnitureVisualDefinitionFor(visual).collision;

export const characterCollidesWithFurniture = (
  placementId: HouseLayoutPlacementId,
  zoneId: HouseZoneId,
  x: number,
  y: number,
  placements: ReadonlyArray<HouseLayoutPlacement>,
  accessories: ReadonlyArray<HouseAccessory>,
): boolean => placements.some((placement) => {
  if (placement.id === placementId || placement.entityType !== 'furniture' || placement.zoneId !== zoneId) {return false;}
  const accessory = accessories.find(item => item.id === placement.entityId);
  if (!accessory?.owned || !accessory.equipped || !accessory.visual) {return false;}
  const bounds = collisionBoundsFor(accessory.visual);
  if (!bounds) {return false;}
  const horizontalLimit = bounds.horizontalRadius * placement.scale + CHARACTER_COLLISION_BOUNDS.horizontalRadius;
  const verticalLimit = bounds.verticalRadius * placement.scale + CHARACTER_COLLISION_BOUNDS.verticalRadius;
  return Math.abs(x - placement.x) < horizontalLimit && Math.abs(y - placement.y) < verticalLimit;
});
