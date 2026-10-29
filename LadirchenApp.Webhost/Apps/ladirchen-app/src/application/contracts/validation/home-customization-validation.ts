import type { HomeCustomizationState } from "@/application/contracts/home-customization-contract";
import type {
  FurnitureSetId,
  HouseAccessoryId,
  HouseRoomCount,
  HouseStageId,
  HouseStageLevel,
  HouseThemeId,
  HouseZoneId,
  RoomDesignId,
} from "@/domain/house";
import {
  FURNITURE_SETS,
  HOUSE_ACCESSORY_DEFINITIONS,
  HOUSE_ROOMS,
  HOUSE_STAGES,
  HOUSE_THEMES,
  ROOM_DESIGNS,
} from "@/domain/house";
import type { HouseLayoutPlacement } from "@/domain/house/entities";
import {
  hasUniqueIds,
  isArrayOf,
  isDomainId,
  isFamilyMemberId,
  isFiniteNumber,
  isInteger,
  isKnownString,
  isRecord,
  values,
} from "./runtime-validation";
const accessoryIds = values<HouseAccessoryId>(HOUSE_ACCESSORY_DEFINITIONS.map((item) => item.id));
const furnitureSetIds = values<FurnitureSetId>(FURNITURE_SETS.map((item) => item.id));
const houseStageIds = values<HouseStageId>(HOUSE_STAGES.map((item) => item.id));
const houseStageLevels = new Set<number>(HOUSE_STAGES.map((item) => item.level));
const houseRoomCounts = new Set<number>([2, 4, 7, 10, 12] satisfies HouseRoomCount[]);
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

function isFurnitureSetId(value: unknown): value is FurnitureSetId {
  return isKnownString(value, furnitureSetIds);
}

function isHouseStageId(value: unknown): value is HouseStageId {
  return isKnownString(value, houseStageIds);
}

function isHouseStageLevel(value: unknown): value is HouseStageLevel {
  return isInteger(value) && houseStageLevels.has(value);
}

function hasPurchaseAvailability(value: Record<string, unknown>): boolean {
  return (
    typeof value.available === "boolean" &&
    typeof value.owned === "boolean" &&
    isFiniteNumber(value.price) &&
    value.price >= 0 &&
    isArrayOf(value.purchasableByMemberIds, isFamilyMemberId) &&
    new Set(value.purchasableByMemberIds).size === value.purchasableByMemberIds.length
  );
}

function isAccessoryCatalogEntry(value: unknown): value is HomeCustomizationState["catalog"]["accessories"][number] {
  return (
    isRecord(value) &&
    isHouseAccessoryId(value.id) &&
    (value.catalogArea === "inside" || value.catalogArea === "outside" || value.catalogArea === "special") &&
    isHouseStageLevel(value.minimumHouseLevel) &&
    hasPurchaseAvailability(value)
  );
}

function isAreaCatalogEntry(value: unknown): value is HomeCustomizationState["catalog"]["areas"][number] {
  return (
    isRecord(value) &&
    typeof value.available === "boolean" &&
    isHouseZoneId(value.id) &&
    isHouseStageLevel(value.minimumHouseLevel)
  );
}

function isFurnitureSetCatalogEntry(
  value: unknown,
): value is HomeCustomizationState["catalog"]["furnitureSets"][number] {
  return (
    isRecord(value) &&
    isFurnitureSetId(value.id) &&
    (value.catalogArea === "inside" || value.catalogArea === "outside") &&
    isArrayOf(value.accessoryIds, isHouseAccessoryId) &&
    new Set(value.accessoryIds).size === value.accessoryIds.length &&
    isHouseStageLevel(value.minimumHouseLevel) &&
    hasPurchaseAvailability(value)
  );
}

function isHouseStageCatalogEntry(value: unknown): value is HomeCustomizationState["catalog"]["houseStages"][number] {
  if (
    !isRecord(value) ||
    typeof value.active !== "boolean" ||
    !isHouseStageId(value.id) ||
    !isHouseStageLevel(value.level) ||
    !isInteger(value.roomCount) ||
    !houseRoomCounts.has(value.roomCount)
  ) {
    return false;
  }

  return HOUSE_STAGES.some((stage) => stage.id === value.id && stage.level === value.level);
}

function isRoomDesignCatalogEntry(value: unknown): value is HomeCustomizationState["catalog"]["roomDesigns"][number] {
  if (
    !isRecord(value) ||
    !isRoomDesignId(value.id) ||
    !isHouseZoneId(value.areaId) ||
    !isHouseThemeId(value.themeId) ||
    !isHouseStageLevel(value.minimumHouseLevel) ||
    !hasPurchaseAvailability(value)
  ) {
    return false;
  }

  return ROOM_DESIGNS.some(
    (design) => design.id === value.id && design.zoneId === value.areaId && design.houseThemeId === value.themeId,
  );
}

function isThemeCatalogEntry(value: unknown): value is HomeCustomizationState["catalog"]["themes"][number] {
  return isRecord(value) && isHouseThemeId(value.id) && hasPurchaseAvailability(value);
}

function isHouseCatalog(value: unknown): value is HomeCustomizationState["catalog"] {
  if (!isRecord(value)) {
    return false;
  }

  const accessories = value.accessories;
  const areas = value.areas;
  const furnitureSets = value.furnitureSets;
  const houseStages = value.houseStages;
  const roomDesigns = value.roomDesigns;
  const themes = value.themes;

  return (
    isArrayOf(accessories, isAccessoryCatalogEntry) &&
    hasUniqueIds(accessories) &&
    isArrayOf(areas, isAreaCatalogEntry) &&
    hasUniqueIds(areas) &&
    isArrayOf(furnitureSets, isFurnitureSetCatalogEntry) &&
    hasUniqueIds(furnitureSets) &&
    isArrayOf(houseStages, isHouseStageCatalogEntry) &&
    hasUniqueIds(houseStages) &&
    houseStages.filter((stage) => stage.active).length === 1 &&
    isArrayOf(roomDesigns, isRoomDesignCatalogEntry) &&
    hasUniqueIds(roomDesigns) &&
    isArrayOf(themes, isThemeCatalogEntry) &&
    hasUniqueIds(themes)
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
