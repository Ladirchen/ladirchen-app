import type { RewardShopState } from "@/application/contracts/reward-shop-contract";
import { SHOP_REWARD_STATUSES } from "@/domain/shop/types";
import type { ShopReward, ShopRewardCategory, ShopRewardStatus, ShopTimeReward } from "@/domain/shop/types";
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
  isOptionalString,
  isRecord,
  values,
} from "./runtime-validation";
const statuses = values<ShopRewardStatus>(SHOP_REWARD_STATUSES);
const categories = exhaustiveValues<ShopRewardCategory>({
  activity: true,
  allowance: true,
  custom: true,
  gift: true,
  privilege: true,
  time: true,
});
function isUniqueDomainIdArray(value: unknown): boolean {
  return isArrayOf(value, isDomainId) && new Set(value).size === value.length;
}
function hasValidPromotion(value: Record<string, unknown>): boolean {
  return (
    isFiniteNumber(value.price) &&
    value.price >= 0 &&
    (value.regularPrice === undefined || (isFiniteNumber(value.regularPrice) && value.regularPrice > value.price)) &&
    typeof value.promotionActive === "boolean" &&
    (value.promotionUntil === undefined || isCalendarDate(value.promotionUntil))
  );
}
function isShopReward(value: unknown): value is ShopReward {
  return (
    isRecord(value) &&
    isDomainId(value.id) &&
    isOptionalString(value.translationKey) &&
    hasLocalizedValue(value, "title") &&
    typeof value.description === "string" &&
    isNonEmptyString(value.icon) &&
    hasValidPromotion(value) &&
    isKnownString(value.category, categories) &&
    (value.category === "time"
      ? isNonNegativeInteger(value.durationMinutes) && value.durationMinutes > 0 && value.durationMinutes <= 1_440
      : value.durationMinutes === undefined) &&
    isNonNegativeInteger(value.quantity) &&
    typeof value.conditions === "string" &&
    (value.availableFrom === undefined || isCalendarDate(value.availableFrom)) &&
    (value.availableUntil === undefined || isCalendarDate(value.availableUntil)) &&
    typeof value.isVisible === "boolean" &&
    typeof value.published === "boolean" &&
    (value.publicationStatus === "expired" ||
      value.publicationStatus === "hidden" ||
      value.publicationStatus === "scheduled" ||
      value.publicationStatus === "visible") &&
    isOptionalString(value.nextStatusChangeAt) &&
    isUniqueDomainIdArray(value.requestableByMemberIds) &&
    isUniqueDomainIdArray(value.cancellableByMemberIds) &&
    isUniqueDomainIdArray(value.approvableByMemberIds) &&
    isKnownString(value.status, statuses) &&
    hasOptionalDomainId(value.requesterId)
  );
}
function isShopTimeReward(value: unknown): value is ShopTimeReward {
  return (
    isRecord(value) &&
    isDomainId(value.requestId) &&
    isDomainId(value.rewardId) &&
    isDomainId(value.childId) &&
    isNonEmptyString(value.title) &&
    isNonEmptyString(value.icon) &&
    isNonNegativeInteger(value.durationMinutes) &&
    value.durationMinutes > 0 &&
    isNonNegativeInteger(value.remainingSeconds) &&
    (value.status === "running" || value.status === "paused" || value.status === "expired") &&
    isOptionalString(value.startedAt) &&
    isOptionalString(value.endsAt) &&
    typeof value.canPause === "boolean" &&
    typeof value.canResume === "boolean" &&
    typeof value.canReset === "boolean" &&
    typeof value.canCancel === "boolean"
  );
}
export function isRewardShopState(value: unknown): value is RewardShopState {
  return (
    isRecord(value) &&
    typeof value.redemptionOpen === "boolean" &&
    isUniqueDomainIdArray(value.shopTaskGateOverrideMemberIds) &&
    isArrayOf(value.rewards, isShopReward) &&
    isArrayOf(value.timeRewards, isShopTimeReward) &&
    hasUniqueIds(value.rewards) &&
    value.rewards.every(
      (reward) =>
        reward.availableFrom === undefined ||
        reward.availableUntil === undefined ||
        reward.availableFrom <= reward.availableUntil,
    )
  );
}
