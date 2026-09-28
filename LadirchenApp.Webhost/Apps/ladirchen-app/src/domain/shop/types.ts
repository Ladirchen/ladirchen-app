import type { ShopRewardTranslationNamespaceKey } from "@/locales/translation-keys";
import type { FamilyMemberId, ShopRewardId } from "@/domain/shared/identifiers";

export const SHOP_REWARD_STATUSES = ["available", "requested", "redeemed"] as const;
export type ShopRewardStatus = typeof SHOP_REWARD_STATUSES[number];
export type ShopRewardCategory = "time" | "activity" | "allowance" | "gift" | "privilege" | "custom";

export interface ShopReward {
  id: ShopRewardId;
  translationKey?: ShopRewardTranslationNamespaceKey;
  title: string;
  description: string;
  icon: string;
  price: number;
  category: ShopRewardCategory;
  quantity: number;
  conditions: string;
  availableFrom?: string;
  availableUntil?: string;
  isVisible?: boolean;
  status: ShopRewardStatus;
  requesterId?: FamilyMemberId;
}

export interface NewShopReward {
  title: string;
  description: string;
  icon: string;
  price: number;
  category: ShopRewardCategory;
  quantity: number;
  conditions: string;
  availableFrom?: string;
  availableUntil?: string;
  isVisible?: boolean;
}
