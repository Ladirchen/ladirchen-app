import type { RewardShopState } from '../reward-shop-contract';
import type { ShopReward, ShopRewardCategory, ShopRewardStatus } from '@/domain/types';
import {
  exhaustiveValues,
  hasLocalizedValue,
  hasOptionalDomainId,
  hasUniqueIds,
  isArrayOf,
  isCalendarDate,
  isDomainId,
  isFiniteNumber,
  isKnownString,
  isNonEmptyString,
  isNonNegativeInteger,
  isOptionalBoolean,
  isOptionalString,
  isRecord,
  type StateGuard,
} from './runtime-validation';

const statuses = exhaustiveValues<ShopRewardStatus>({ available: true, redeemed: true, requested: true });
const categories = exhaustiveValues<ShopRewardCategory>({ activity: true, allowance: true, custom: true, gift: true, privilege: true, time: true });

const isShopReward = (value: unknown): value is ShopReward =>
  isRecord(value) &&
  isDomainId(value.id) &&
  isOptionalString(value.translationKey) &&
  hasLocalizedValue(value, 'title') &&
  typeof value.description === 'string' &&
  isNonEmptyString(value.icon) &&
  isFiniteNumber(value.price) && value.price >= 0 &&
  isKnownString(value.category, categories) &&
  isNonNegativeInteger(value.quantity) &&
  typeof value.conditions === 'string' &&
  (value.availableFrom === undefined || isCalendarDate(value.availableFrom)) &&
  (value.availableUntil === undefined || isCalendarDate(value.availableUntil)) &&
  isOptionalBoolean(value.isVisible) &&
  isKnownString(value.status, statuses) &&
  hasOptionalDomainId(value.requesterId);

export const isRewardShopState: StateGuard<RewardShopState> = (value): value is RewardShopState =>
  isRecord(value) &&
  isArrayOf(value.rewards, isShopReward) && hasUniqueIds(value.rewards) &&
  value.rewards.every(reward =>
    reward.availableFrom === undefined || reward.availableUntil === undefined || reward.availableFrom <= reward.availableUntil);
