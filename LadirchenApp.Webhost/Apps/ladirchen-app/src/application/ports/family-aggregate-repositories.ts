import type { CONTRIBUTIONS_AGGREGATE_TYPE, CONTRIBUTIONS_SCHEMA_VERSION, ContributionsState } from "@/application/contracts/contributions-contract";
import type { FAMILY_PROFILE_AGGREGATE_TYPE, FAMILY_PROFILE_SCHEMA_VERSION, FamilyProfileState } from "@/application/contracts/family-profile-contract";
import type { FAMILY_PROGRESSION_AGGREGATE_TYPE, FAMILY_PROGRESSION_SCHEMA_VERSION, FamilyProgressionState } from "@/application/contracts/family-progression-contract";
import type { HOME_CUSTOMIZATION_AGGREGATE_TYPE, HOME_CUSTOMIZATION_SCHEMA_VERSION, HomeCustomizationState } from "@/application/contracts/home-customization-contract";
import type { REWARD_SHOP_AGGREGATE_TYPE, REWARD_SHOP_SCHEMA_VERSION, RewardShopState } from "@/application/contracts/reward-shop-contract";
import type { SAVINGS_AGGREGATE_TYPE, SAVINGS_SCHEMA_VERSION, SavingsState } from "@/application/contracts/savings-contract";
import type { VersionedAggregateRepository } from "./versioned-aggregate-repository";

export type ContributionsRepository = VersionedAggregateRepository<
  typeof CONTRIBUTIONS_AGGREGATE_TYPE,
  typeof CONTRIBUTIONS_SCHEMA_VERSION,
  ContributionsState
>;

export type FamilyProfileRepository = VersionedAggregateRepository<
  typeof FAMILY_PROFILE_AGGREGATE_TYPE,
  typeof FAMILY_PROFILE_SCHEMA_VERSION,
  FamilyProfileState
>;

export type FamilyProgressionRepository = VersionedAggregateRepository<
  typeof FAMILY_PROGRESSION_AGGREGATE_TYPE,
  typeof FAMILY_PROGRESSION_SCHEMA_VERSION,
  FamilyProgressionState
>;

export type HomeCustomizationRepository = VersionedAggregateRepository<
  typeof HOME_CUSTOMIZATION_AGGREGATE_TYPE,
  typeof HOME_CUSTOMIZATION_SCHEMA_VERSION,
  HomeCustomizationState
>;

export type RewardShopRepository = VersionedAggregateRepository<
  typeof REWARD_SHOP_AGGREGATE_TYPE,
  typeof REWARD_SHOP_SCHEMA_VERSION,
  RewardShopState
>;

export type SavingsRepository = VersionedAggregateRepository<
  typeof SAVINGS_AGGREGATE_TYPE,
  typeof SAVINGS_SCHEMA_VERSION,
  SavingsState
>;

export interface FamilyAggregateRepositories {
  readonly contributions: ContributionsRepository;
  readonly familyProfile: FamilyProfileRepository;
  readonly familyProgression: FamilyProgressionRepository;
  readonly homeCustomization: HomeCustomizationRepository;
  readonly rewardShop: RewardShopRepository;
  readonly savings: SavingsRepository;
}
