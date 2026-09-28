import type { VersionedAggregateSnapshot } from "./versioned-aggregate-contract";
import type { ShopReward } from "@/domain/shop/types";

export const REWARD_SHOP_SCHEMA_VERSION = 1;
export const REWARD_SHOP_AGGREGATE_TYPE = "reward-shop";

export interface RewardShopState {
  readonly rewards: ReadonlyArray<ShopReward>;
}

export type RewardShopSnapshot = VersionedAggregateSnapshot<
  typeof REWARD_SHOP_AGGREGATE_TYPE,
  typeof REWARD_SHOP_SCHEMA_VERSION,
  RewardShopState
>;
