import type { HomeCustomizationState } from "@/application/contracts/home-customization-contract";
import type { HouseAccessoryId, HouseThemeId, HouseZoneId, RoomDesignId } from "@/domain/house";
import { HOUSE_ROOMS, HOUSE_THEMES, ROOM_DESIGNS, createHouseAccessories } from "@/domain/house";
import type { HouseLayoutPlacement } from "@/domain/house/entities";
import {
  hasUniqueIds,
  isArrayOf,
  isDomainId,
  isFamilyMemberId,
  isFiniteNumber,
  isKnownString,
  isRecord,
  type StateGuard,
  values,
} from "./runtime-validation";

const accessoryIds = values<HouseAccessoryId>(createHouseAccessories().map(item => item.id));
const themeIds = values<HouseThemeId>(HOUSE_THEMES.map(item => item.id));
const zoneIds = values<HouseZoneId>(["garden", ...HOUSE_ROOMS.map(room => room.id)]);
const roomDesignIds = values<RoomDesignId>(ROOM_DESIGNS.map(design => design.id));

export const isHouseAccessoryId = (value: unknown): value is HouseAccessoryId =>
  isKnownString(value, accessoryIds);
export const isHouseThemeId = (value: unknown): value is HouseThemeId =>
  isKnownString(value, themeIds);
export const isHouseZoneId = (value: unknown): value is HouseZoneId =>
  isKnownString(value, zoneIds);
export const isRoomDesignId = (value: unknown): value is RoomDesignId =>
  isKnownString(value, roomDesignIds);

const isHouseLayoutPlacement = (value: unknown): value is HouseLayoutPlacement => {
  if (!isRecord(value) || !isDomainId(value.id) || !isHouseZoneId(value.zoneId) ||
    !isFiniteNumber(value.x) || value.x < 0 || value.x > 100 ||
    !isFiniteNumber(value.y) || value.y < 0 || value.y > 100 ||
    !isFiniteNumber(value.scale) || value.scale <= 0) {
    return false;
  }
  if (value.entityType === "furniture") {return isHouseAccessoryId(value.entityId);}
  if (value.entityType === "member") {return isFamilyMemberId(value.entityId);}
  if (value.entityType === "pet") {return isDomainId(value.entityId);}
  return value.entityType === "ladi" && value.entityId === "family-ladi";
};

export const isHomeCustomizationState: StateGuard<HomeCustomizationState> = (value): value is HomeCustomizationState => {
  if (!isRecord(value)) { return false; }
  const roomDesigns = value.roomDesigns;
  const selectedRoomDesigns = value.selectedRoomDesigns;
  if (!isArrayOf(roomDesigns, (item): item is HomeCustomizationState["roomDesigns"][number] =>
    isRecord(item) && isRoomDesignId(item.id) && typeof item.owned === "boolean") ||
    !hasUniqueIds(roomDesigns) ||
    !isArrayOf(selectedRoomDesigns, (item): item is HomeCustomizationState["selectedRoomDesigns"][number] =>
      isRecord(item) && isHouseZoneId(item.zoneId) && isRoomDesignId(item.designId))) {
    return false;
  }
  if (
    !isArrayOf(value.accessories, (item): item is HomeCustomizationState["accessories"][number] =>
      isRecord(item) && isHouseAccessoryId(item.id) && typeof item.equipped === "boolean" && typeof item.owned === "boolean") ||
    !hasUniqueIds(value.accessories) ||
    !isArrayOf(value.editions, (item): item is HomeCustomizationState["editions"][number] =>
      isRecord(item) && isHouseThemeId(item.id) && typeof item.owned === "boolean") ||
    !hasUniqueIds(value.editions) ||
    !isArrayOf(value.placements, isHouseLayoutPlacement) || !hasUniqueIds(value.placements) ||
    !isHouseThemeId(value.selectedEditionId)) {
    return false;
  }
  const selectedZones = new Set(selectedRoomDesigns.map(selection => selection.zoneId));
  const selectionsAreValid = selectedZones.size === selectedRoomDesigns.length && selectedRoomDesigns.every((selection) => {
    const design = ROOM_DESIGNS.find(item => item.id === selection.designId);
    return design?.zoneId === selection.zoneId && roomDesigns.some(item => item.id === selection.designId && item.owned);
  });
  return selectionsAreValid && value.editions.some(edition => edition.id === value.selectedEditionId && edition.owned);
};
