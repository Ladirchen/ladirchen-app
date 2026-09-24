import type { HouseAccessoryTranslationNamespaceKey } from "@/locales/translation-keys";
import type { FamilyMemberId, FamilyPetId, HouseLayoutPlacementId } from "@/domain/shared/identifiers";
import type { CatalogNewBadgeSchedule, FurniturePlacement, FurnitureSetId, FurnitureVisualId, HouseAccessoryCategory, HouseAccessoryId, HouseAccessoryInteraction, HouseAccessoryMobility, HouseAccessoryMotion, HouseAccessoryPlacement, HouseLayoutEntityType, HouseRoomId, HouseStageLevel, HouseZoneId } from "./types";

export interface HouseAccessory {
  id: HouseAccessoryId;
  translationKey?: HouseAccessoryTranslationNamespaceKey;
  category: HouseAccessoryCategory;
  title: string;
  description: string;
  icon: string;
  price: number;
  placement: HouseAccessoryPlacement;
  roomId?: HouseRoomId;
  setIds?: ReadonlyArray<FurnitureSetId>;
  minimumHouseLevel?: HouseStageLevel;
  newBadge?: CatalogNewBadgeSchedule;
  motion?: HouseAccessoryMotion;
  mobility: HouseAccessoryMobility;
  interaction?: HouseAccessoryInteraction;
  visual?: FurnitureVisualId;
  scene?: FurniturePlacement;
  owned: boolean;
  equipped: boolean;
}

interface HouseLayoutPlacementBase {
  id: HouseLayoutPlacementId;
  entityType: HouseLayoutEntityType;
  zoneId: HouseZoneId;
  x: number;
  y: number;
  scale: number;
}

export type HouseLayoutPlacement =
  | (HouseLayoutPlacementBase & { entityType: "furniture"; entityId: HouseAccessoryId })
  | (HouseLayoutPlacementBase & { entityType: "member"; entityId: FamilyMemberId })
  | (HouseLayoutPlacementBase & { entityType: "pet"; entityId: FamilyPetId })
  | (HouseLayoutPlacementBase & { entityType: "ladi"; entityId: "family-ladi" });
