import type { FamilyReadModel } from "./family-read-model-contract";
import type { ShopReward } from "@/domain/shop/types";

export const REWARD_SHOP_READ_MODEL_TYPE = "reward-shop";

export interface RewardShopState {
  readonly redemptionOpen: boolean;
  readonly rewards: ReadonlyArray<ShopReward>;
}

export type RewardShopReadModel = FamilyReadModel<typeof REWARD_SHOP_READ_MODEL_TYPE, RewardShopState>;
