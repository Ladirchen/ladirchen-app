import type { RewardShopState } from "@/application/contracts/reward-shop-contract";
import { SHOP_REWARD_STATUSES } from "@/domain/shop/types";
import type { ShopReward, ShopRewardCategory, ShopRewardStatus } from "@/domain/shop/types";
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
  values,
} from "./runtime-validation";

const statuses = values<ShopRewardStatus>(SHOP_REWARD_STATUSES);
const categories = exhaustiveValues<ShopRewardCategory>({ activity: true, allowance: true, custom: true, gift: true, privilege: true, time: true });

const isShopReward = (value: unknown): value is ShopReward =>
  isRecord(value) &&
  isDomainId(value.id) &&
  isOptionalString(value.translationKey) &&
  hasLocalizedValue(value, "title") &&
  typeof value.description === "string" &&
  isNonEmptyString(value.icon) &&
  isFiniteNumber(value.price) && value.price >= 0 &&
  isKnownString(value.category, categories) &&
  isNonNegativeInteger(value.quantity) &&
  typeof value.conditions === "string" &&
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
