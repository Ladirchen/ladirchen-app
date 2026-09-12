import { CONTRIBUTIONS_AGGREGATE_TYPE, CONTRIBUTIONS_SCHEMA_VERSION } from '@/application/contracts/contributions-contract';
import { FAMILY_PROFILE_AGGREGATE_TYPE, FAMILY_PROFILE_SCHEMA_VERSION } from '@/application/contracts/family-profile-contract';
import { FAMILY_PROGRESSION_AGGREGATE_TYPE, FAMILY_PROGRESSION_SCHEMA_VERSION } from '@/application/contracts/family-progression-contract';
import { HOME_CUSTOMIZATION_AGGREGATE_TYPE, HOME_CUSTOMIZATION_SCHEMA_VERSION } from '@/application/contracts/home-customization-contract';
import { REWARD_SHOP_AGGREGATE_TYPE, REWARD_SHOP_SCHEMA_VERSION } from '@/application/contracts/reward-shop-contract';
import { SAVINGS_AGGREGATE_TYPE, SAVINGS_SCHEMA_VERSION } from '@/application/contracts/savings-contract';
import { isContributionsState, isFamilyProfileState, isFamilyProgressionState, isHomeCustomizationState, isRewardShopState, isSavingsState } from '@/application/contracts/family-aggregate-validation';
import type { FamilyAggregateRepositories } from '@/application/ports/family-aggregate-repositories';

import { HttpVersionedAggregateRepository } from './http-versioned-aggregate-repository';

export const createHttpFamilyAggregateRepositories = (apiBaseUrl: string): FamilyAggregateRepositories => ({
  contributions: new HttpVersionedAggregateRepository({
    aggregateType: CONTRIBUTIONS_AGGREGATE_TYPE,
    apiBaseUrl,
    isState: isContributionsState,
    schemaVersion: CONTRIBUTIONS_SCHEMA_VERSION,
  }),
  familyProfile: new HttpVersionedAggregateRepository({
    aggregateType: FAMILY_PROFILE_AGGREGATE_TYPE,
    apiBaseUrl,
    isState: isFamilyProfileState,
    schemaVersion: FAMILY_PROFILE_SCHEMA_VERSION,
  }),
  familyProgression: new HttpVersionedAggregateRepository({
    aggregateType: FAMILY_PROGRESSION_AGGREGATE_TYPE,
    apiBaseUrl,
    isState: isFamilyProgressionState,
    schemaVersion: FAMILY_PROGRESSION_SCHEMA_VERSION,
  }),
  homeCustomization: new HttpVersionedAggregateRepository({
    aggregateType: HOME_CUSTOMIZATION_AGGREGATE_TYPE,
    apiBaseUrl,
    isState: isHomeCustomizationState,
    schemaVersion: HOME_CUSTOMIZATION_SCHEMA_VERSION,
  }),
  rewardShop: new HttpVersionedAggregateRepository({
    aggregateType: REWARD_SHOP_AGGREGATE_TYPE,
    apiBaseUrl,
    isState: isRewardShopState,
    schemaVersion: REWARD_SHOP_SCHEMA_VERSION,
  }),
  savings: new HttpVersionedAggregateRepository({
    aggregateType: SAVINGS_AGGREGATE_TYPE,
    apiBaseUrl,
    isState: isSavingsState,
    schemaVersion: SAVINGS_SCHEMA_VERSION,
  }),
});
