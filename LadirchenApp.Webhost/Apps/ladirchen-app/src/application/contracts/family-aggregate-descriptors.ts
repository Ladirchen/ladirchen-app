import { CONTRIBUTIONS_AGGREGATE_TYPE, CONTRIBUTIONS_SCHEMA_VERSION } from "./contributions-contract";
import type { ContributionsState } from "./contributions-contract";
import { FAMILY_PROFILE_AGGREGATE_TYPE, FAMILY_PROFILE_SCHEMA_VERSION } from "./family-profile-contract";
import type { FamilyProfileState } from "./family-profile-contract";
import { FAMILY_PROGRESSION_AGGREGATE_TYPE, FAMILY_PROGRESSION_SCHEMA_VERSION } from "./family-progression-contract";
import type { FamilyProgressionState } from "./family-progression-contract";
import { HOME_CUSTOMIZATION_AGGREGATE_TYPE, HOME_CUSTOMIZATION_SCHEMA_VERSION } from "./home-customization-contract";
import type { HomeCustomizationState } from "./home-customization-contract";
import { REWARD_SHOP_AGGREGATE_TYPE, REWARD_SHOP_SCHEMA_VERSION } from "./reward-shop-contract";
import type { RewardShopState } from "./reward-shop-contract";
import { SAVINGS_AGGREGATE_TYPE, SAVINGS_SCHEMA_VERSION } from "./savings-contract";
import type { SavingsState } from "./savings-contract";
import {
  isContributionsState,
  isFamilyProfileState,
  isFamilyProgressionState,
  isHomeCustomizationState,
  isRewardShopState,
  isSavingsState,
} from "./family-aggregate-validation";
import type { StateGuard } from "./family-aggregate-validation";
import type { FamilyAggregateType } from "./versioned-aggregate-contract";

export interface FamilyAggregateDescriptor<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> {
  readonly aggregateType: TAggregateType;
  readonly isState: StateGuard<TState>;
  readonly schemaVersion: TSchemaVersion;
}

export const FAMILY_AGGREGATE_DESCRIPTORS = {
  contributions: {
    aggregateType: CONTRIBUTIONS_AGGREGATE_TYPE,
    isState: isContributionsState,
    schemaVersion: CONTRIBUTIONS_SCHEMA_VERSION,
  } satisfies FamilyAggregateDescriptor<typeof CONTRIBUTIONS_AGGREGATE_TYPE, typeof CONTRIBUTIONS_SCHEMA_VERSION, ContributionsState>,
  familyProfile: {
    aggregateType: FAMILY_PROFILE_AGGREGATE_TYPE,
    isState: isFamilyProfileState,
    schemaVersion: FAMILY_PROFILE_SCHEMA_VERSION,
  } satisfies FamilyAggregateDescriptor<typeof FAMILY_PROFILE_AGGREGATE_TYPE, typeof FAMILY_PROFILE_SCHEMA_VERSION, FamilyProfileState>,
  familyProgression: {
    aggregateType: FAMILY_PROGRESSION_AGGREGATE_TYPE,
    isState: isFamilyProgressionState,
    schemaVersion: FAMILY_PROGRESSION_SCHEMA_VERSION,
  } satisfies FamilyAggregateDescriptor<typeof FAMILY_PROGRESSION_AGGREGATE_TYPE, typeof FAMILY_PROGRESSION_SCHEMA_VERSION, FamilyProgressionState>,
  homeCustomization: {
    aggregateType: HOME_CUSTOMIZATION_AGGREGATE_TYPE,
    isState: isHomeCustomizationState,
    schemaVersion: HOME_CUSTOMIZATION_SCHEMA_VERSION,
  } satisfies FamilyAggregateDescriptor<typeof HOME_CUSTOMIZATION_AGGREGATE_TYPE, typeof HOME_CUSTOMIZATION_SCHEMA_VERSION, HomeCustomizationState>,
  rewardShop: {
    aggregateType: REWARD_SHOP_AGGREGATE_TYPE,
    isState: isRewardShopState,
    schemaVersion: REWARD_SHOP_SCHEMA_VERSION,
  } satisfies FamilyAggregateDescriptor<typeof REWARD_SHOP_AGGREGATE_TYPE, typeof REWARD_SHOP_SCHEMA_VERSION, RewardShopState>,
  savings: {
    aggregateType: SAVINGS_AGGREGATE_TYPE,
    isState: isSavingsState,
    schemaVersion: SAVINGS_SCHEMA_VERSION,
  } satisfies FamilyAggregateDescriptor<typeof SAVINGS_AGGREGATE_TYPE, typeof SAVINGS_SCHEMA_VERSION, SavingsState>,
} as const;
