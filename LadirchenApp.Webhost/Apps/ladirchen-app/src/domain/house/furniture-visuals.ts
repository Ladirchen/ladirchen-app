import type { FurnitureVisualId } from "./types";

export interface FurnitureCollisionBounds {
  readonly horizontalRadius: number;
  readonly verticalRadius: number;
}

export interface FurnitureVisualDefinition {
  readonly collision?: FurnitureCollisionBounds;
  readonly locksScale?: boolean;
  readonly maximumY?: number;
  readonly minimumY?: number;
  readonly placementY?: number;
  readonly renderInLayout?: boolean;
}

const DEFAULT_COLLISION: FurnitureCollisionBounds = { horizontalRadius: 8, verticalRadius: 9 };

const decoration = (minimumY?: number): FurnitureVisualDefinition => ({ minimumY });
const solid = (
  horizontalRadius = DEFAULT_COLLISION.horizontalRadius,
  verticalRadius = DEFAULT_COLLISION.verticalRadius,
  options: Omit<FurnitureVisualDefinition, "collision"> = {},
): FurnitureVisualDefinition => ({ collision: { horizontalRadius, verticalRadius }, ...options });

export const FURNITURE_VISUAL_DEFINITIONS = {
  "art-desk": solid(11, 9),
  "bat-garland": { ...decoration(12), maximumY: 46 },
  "bedside-table": solid(),
  bookshelf: solid(8, 12),
  "bunk-bed": solid(14, 12),
  "candy-bush": solid(),
  "candy-fence": solid(),
  "cat-tree": solid(),
  "dining-table": solid(9, 8, { locksScale: true, placementY: 66 }),
  "dog-blanket": decoration(),
  "double-bed": solid(15, 13),
  "family-flag": decoration(),
  "flower-box": { ...decoration(), renderInLayout: false },
  "food-bowl": decoration(),
  "garden-chair": solid(),
  "garden-table": solid(),
  hammock: solid(),
  "kitchen-counter": solid(12, 7),
  lamp: solid(),
  "pet-bed": decoration(),
  plant: solid(),
  pool: solid(),
  "pumpkin-arch": solid(),
  "retro-fridge": solid(6, 13),
  rug: decoration(),
  sofa: solid(14, 10),
  "storage-cabinet": solid(),
  "string-lights": decoration(58),
  sunshade: solid(),
  table: solid(9, 8, { locksScale: true, placementY: 66 }),
  telescope: solid(),
  trampoline: solid(),
  "wall-art": { ...decoration(12), maximumY: 46, placementY: 28 },
  wardrobe: solid(8, 13),
} as const satisfies Record<FurnitureVisualId, FurnitureVisualDefinition>;

export const furnitureVisualDefinitionFor = (visual: FurnitureVisualId): FurnitureVisualDefinition =>
  FURNITURE_VISUAL_DEFINITIONS[visual];
