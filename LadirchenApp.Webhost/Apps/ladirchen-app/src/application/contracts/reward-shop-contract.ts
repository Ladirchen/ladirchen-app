import type { FamilyReadModel } from "./family-read-model-contract";
import type { ShopReward, ShopTimeReward } from "@/domain/shop/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export const REWARD_SHOP_READ_MODEL_TYPE = "reward-shop";

export interface RewardShopState {
  readonly redemptionOpen: boolean;
  readonly shopTaskGateOverrideMemberIds: ReadonlyArray<FamilyMemberId>;
  readonly rewards: ReadonlyArray<ShopReward>;
  readonly timeRewards: ReadonlyArray<ShopTimeReward>;
}

export type RewardShopReadModel = FamilyReadModel<typeof REWARD_SHOP_READ_MODEL_TYPE, RewardShopState>;
