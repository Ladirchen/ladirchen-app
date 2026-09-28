import type { HouseStageLevel, HouseThemeId, HouseZoneId } from "./types";

export const ROOM_DESIGN_IDS = [
  "garden-ladi-hills",
  "living-ladi-classic",
  "kitchen-ladi-classic",
  "garden-halloween-night",
  "living-halloween-night",
  "kitchen-halloween-night",
  "garden-christmas-wonderland",
  "living-christmas-wonderland",
  "kitchen-christmas-wonderland",
  "garden-cotton-candy-dream",
  "living-cotton-candy-dream",
  "kitchen-cotton-candy-dream",
  "garden-starlight-palace",
  "living-starlight-palace",
  "kitchen-starlight-palace",
] as const;

export const ROOM_BACKGROUND_ASSET_IDS = [
  "garden-ladi-hills-background",
  "garden-ladi-hills-energy-1-background",
  "garden-ladi-hills-energy-2-background",
  "garden-ladi-hills-energy-3-background",
  "garden-ladi-hills-energy-4-background",
  "living-ladi-classic-background",
  "kitchen-ladi-classic-background",
  "garden-halloween-night-background",
  "living-halloween-night-background",
  "kitchen-halloween-night-background",
  "garden-christmas-wonderland-background",
  "living-christmas-wonderland-background",
  "kitchen-christmas-wonderland-background",
  "garden-cotton-candy-dream-background",
  "living-cotton-candy-dream-background",
  "kitchen-cotton-candy-dream-background",
  "garden-starlight-palace-background",
  "living-starlight-palace-background",
  "kitchen-starlight-palace-background",
] as const;

export const ROOM_SCENE_OVERLAY_IDS = [
  "garden-hanging-leaves",
  "garden-wind-spinner",
] as const;

export const ROOM_SCENE_MOTIONS = [
  "spin",
  "sway",
] as const;

export type RoomDesignId = typeof ROOM_DESIGN_IDS[number];
export type RoomBackgroundAssetId = typeof ROOM_BACKGROUND_ASSET_IDS[number];
export type RoomSceneOverlayId = typeof ROOM_SCENE_OVERLAY_IDS[number];
export type RoomSceneMotion = typeof ROOM_SCENE_MOTIONS[number];
export type RoomDesignTranslationKey = `catalog.roomDesigns.${RoomDesignId}.${"description" | "name"}`;

export interface RoomSceneOverlayDefinition {
  readonly id: RoomSceneOverlayId;
  readonly motion: RoomSceneMotion;
  readonly xPercent: number;
  readonly yPercent: number;
  readonly scale: number;
}

export interface RoomDesignDefinition {
  readonly id: RoomDesignId;
  readonly houseThemeId: HouseThemeId;
  readonly zoneId: HouseZoneId;
  readonly nameKey: RoomDesignTranslationKey;
  readonly descriptionKey: RoomDesignTranslationKey;
  readonly icon: string;
  readonly price: number;
  readonly minimumHouseLevel: HouseStageLevel;
  readonly ownedByDefault: boolean;
  readonly backgroundAssetId: RoomBackgroundAssetId;
  readonly canvasWidth: number;
  readonly canvasHeight: number;
  readonly overlays: ReadonlyArray<RoomSceneOverlayDefinition>;
}

export interface SelectedRoomDesignState {
  readonly zoneId: HouseZoneId;
  readonly designId: RoomDesignId;
}
