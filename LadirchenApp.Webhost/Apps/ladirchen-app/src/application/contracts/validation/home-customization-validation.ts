import type { HomeCustomizationState } from '../home-customization-contract';
import type { HouseAccessoryId, HouseThemeId, HouseZoneId } from '@/domain/house';
import { HOUSE_ROOMS, HOUSE_THEMES, createHouseAccessories } from '@/domain/house-catalog';
import type { HouseLayoutPlacement } from '@/domain/types';
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
} from './runtime-validation';

const accessoryIds = values<HouseAccessoryId>(createHouseAccessories().map(item => item.id));
const themeIds = values<HouseThemeId>(HOUSE_THEMES.map(item => item.id));
const zoneIds = values<HouseZoneId>(['garden', ...HOUSE_ROOMS.map(room => room.id)]);

export const isHouseAccessoryId = (value: unknown): value is HouseAccessoryId =>
  isKnownString(value, accessoryIds);
export const isHouseThemeId = (value: unknown): value is HouseThemeId =>
  isKnownString(value, themeIds);
export const isHouseZoneId = (value: unknown): value is HouseZoneId =>
  isKnownString(value, zoneIds);

const isHouseLayoutPlacement = (value: unknown): value is HouseLayoutPlacement => {
  if (!isRecord(value) || !isDomainId(value.id) || !isHouseZoneId(value.zoneId) ||
    !isFiniteNumber(value.x) || value.x < 0 || value.x > 100 ||
    !isFiniteNumber(value.y) || value.y < 0 || value.y > 100 ||
    !isFiniteNumber(value.scale) || value.scale <= 0) {
    return false;
  }
  if (value.entityType === 'furniture') {return isHouseAccessoryId(value.entityId);}
  if (value.entityType === 'member') {return isFamilyMemberId(value.entityId);}
  if (value.entityType === 'pet') {return isDomainId(value.entityId);}
  return value.entityType === 'ladi' && value.entityId === 'family-ladi';
};

export const isHomeCustomizationState: StateGuard<HomeCustomizationState> = (value): value is HomeCustomizationState => {
  if (!isRecord(value) ||
    !isArrayOf(value.accessories, (item): item is HomeCustomizationState['accessories'][number] =>
      isRecord(item) && isHouseAccessoryId(item.id) && typeof item.equipped === 'boolean' && typeof item.owned === 'boolean') ||
    !hasUniqueIds(value.accessories) ||
    !isArrayOf(value.editions, (item): item is HomeCustomizationState['editions'][number] =>
      isRecord(item) && isHouseThemeId(item.id) && typeof item.owned === 'boolean') ||
    !hasUniqueIds(value.editions) ||
    !isArrayOf(value.placements, isHouseLayoutPlacement) || !hasUniqueIds(value.placements) ||
    !isHouseThemeId(value.selectedEditionId)) {
    return false;
  }
  return value.editions.some(edition => edition.id === value.selectedEditionId && edition.owned);
};
