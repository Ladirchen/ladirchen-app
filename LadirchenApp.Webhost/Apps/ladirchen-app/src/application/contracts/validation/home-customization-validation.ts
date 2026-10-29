import type { HomeCustomizationState } from "@/application/contracts/home-customization-contract";
import type { HouseAccessoryId, HouseThemeId, HouseZoneId, RoomDesignId } from "@/domain/house";
import { HOUSE_ACCESSORY_DEFINITIONS, HOUSE_ROOMS, HOUSE_THEMES, ROOM_DESIGNS } from "@/domain/house";
import type { HouseLayoutPlacement } from "@/domain/house/entities";
import {
  hasUniqueIds,
  isArrayOf,
  isDomainId,
  isFamilyMemberId,
  isFiniteNumber,
  isKnownString,
  isRecord,
  values,
} from "./runtime-validation";
const accessoryIds = values<HouseAccessoryId>(HOUSE_ACCESSORY_DEFINITIONS.map((item) => item.id));
const themeIds = values<HouseThemeId>(HOUSE_THEMES.map((item) => item.id));
const zoneIds = values<HouseZoneId>(["garden", ...HOUSE_ROOMS.map((room) => room.id)]);
const roomDesignIds = values<RoomDesignId>(ROOM_DESIGNS.map((design) => design.id));
export function isHouseAccessoryId(value: unknown): value is HouseAccessoryId {
  return isKnownString(value, accessoryIds);
}
export function isHouseThemeId(value: unknown): value is HouseThemeId {
  return isKnownString(value, themeIds);
}
export function isHouseZoneId(value: unknown): value is HouseZoneId {
  return isKnownString(value, zoneIds);
}
export function isRoomDesignId(value: unknown): value is RoomDesignId {
  return isKnownString(value, roomDesignIds);
}
function isCatalogEntry(value: unknown): value is Record<string, unknown> {
  return (
    isRecord(value) &&
    typeof value.available === "boolean" &&
    typeof value.id === "string" &&
    isArrayOf(value.purchasableByMemberIds, isFamilyMemberId)
  );
}
function isHouseCatalog(value: unknown): value is Record<string, unknown> {
  return (
    isRecord(value) &&
    isArrayOf(value.accessories, isCatalogEntry) &&
    isArrayOf(
      value.areas,
      (item): item is Record<string, unknown> => isRecord(item) && typeof item.available === "boolean",
    ) &&
    isArrayOf(value.furnitureSets, isCatalogEntry) &&
    isArrayOf(
      value.houseStages,
      (item): item is Record<string, unknown> => isRecord(item) && typeof item.active === "boolean",
    ) &&
    isArrayOf(value.roomDesigns, isCatalogEntry) &&
    isArrayOf(value.themes, isCatalogEntry)
  );
}
function isHouseLayoutPlacement(value: unknown): value is HouseLayoutPlacement {
  if (
    !isRecord(value) ||
    !isDomainId(value.id) ||
    !isHouseZoneId(value.zoneId) ||
    !isFiniteNumber(value.x) ||
    value.x < 0 ||
    value.x > 100 ||
    !isFiniteNumber(value.y) ||
    value.y < 0 ||
    value.y > 100 ||
    !isFiniteNumber(value.scale) ||
    value.scale <= 0
  ) {
    return false;
  }

  if (value.entityType === "furniture") {
    return isHouseAccessoryId(value.entityId);
  }

  if (value.entityType === "member") {
    return isFamilyMemberId(value.entityId);
  }

  if (value.entityType === "pet") {
    return isDomainId(value.entityId);
  }

  return value.entityType === "ladi" && value.entityId === "family-ladi";
}
export function isHomeCustomizationState(value: unknown): value is HomeCustomizationState {
  if (!isRecord(value)) {
    return false;
  }

  if (
    !isHouseCatalog(value.catalog) ||
    !isRecord(value.capabilities) ||
    typeof value.capabilities.canArrangeHouse !== "boolean"
  ) {
    return false;
  }

  const roomDesigns = value.roomDesigns;
  const selectedRoomDesigns = value.selectedRoomDesigns;

  if (
    !isArrayOf(
      roomDesigns,
      (item): item is HomeCustomizationState["roomDesigns"][number] =>
        isRecord(item) && isRoomDesignId(item.id) && typeof item.owned === "boolean",
    ) ||
    !hasUniqueIds(roomDesigns) ||
    !isArrayOf(
      selectedRoomDesigns,
      (item): item is HomeCustomizationState["selectedRoomDesigns"][number] =>
        isRecord(item) && isHouseZoneId(item.zoneId) && isRoomDesignId(item.designId),
    )
  ) {
    return false;
  }

  if (
    !isArrayOf(
      value.accessories,
      (item): item is HomeCustomizationState["accessories"][number] =>
        isRecord(item) &&
        isHouseAccessoryId(item.id) &&
        typeof item.equipped === "boolean" &&
        typeof item.owned === "boolean",
    ) ||
    !hasUniqueIds(value.accessories) ||
    !isArrayOf(
      value.editions,
      (item): item is HomeCustomizationState["editions"][number] =>
        isRecord(item) && isHouseThemeId(item.id) && typeof item.owned === "boolean",
    ) ||
    !hasUniqueIds(value.editions) ||
    !isArrayOf(value.placements, isHouseLayoutPlacement) ||
    !hasUniqueIds(value.placements) ||
    !isHouseThemeId(value.selectedEditionId)
  ) {
    return false;
  }

  const selectedZones = new Set(selectedRoomDesigns.map((selection) => selection.zoneId));
  const selectionsAreValid =
    selectedZones.size === selectedRoomDesigns.length &&
    selectedRoomDesigns.every(function callback(selection) {
      const design = ROOM_DESIGNS.find((item) => item.id === selection.designId);

      return (
        design?.zoneId === selection.zoneId && roomDesigns.some((item) => item.id === selection.designId && item.owned)
      );
    });

  return (
    selectionsAreValid && value.editions.some((edition) => edition.id === value.selectedEditionId && edition.owned)
  );
}
