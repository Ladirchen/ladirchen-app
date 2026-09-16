import { CONTRIBUTIONS_AGGREGATE_TYPE, CONTRIBUTIONS_SCHEMA_VERSION } from '@/application/contracts/contributions-contract';
import type { ContributionsState } from '@/application/contracts/contributions-contract';
import { FAMILY_PROFILE_AGGREGATE_TYPE, FAMILY_PROFILE_SCHEMA_VERSION } from '@/application/contracts/family-profile-contract';
import type { FamilyProfileState } from '@/application/contracts/family-profile-contract';
import { FAMILY_PROGRESSION_AGGREGATE_TYPE, FAMILY_PROGRESSION_SCHEMA_VERSION } from '@/application/contracts/family-progression-contract';
import type { FamilyProgressionState } from '@/application/contracts/family-progression-contract';
import { REWARD_SHOP_AGGREGATE_TYPE, REWARD_SHOP_SCHEMA_VERSION } from '@/application/contracts/reward-shop-contract';
import type { RewardShopState } from '@/application/contracts/reward-shop-contract';
import { SAVINGS_AGGREGATE_TYPE, SAVINGS_SCHEMA_VERSION } from '@/application/contracts/savings-contract';
import type { SavingsState } from '@/application/contracts/savings-contract';
import { isContributionsState, isFamilyProfileState, isFamilyProgressionState, isRewardShopState, isSavingsState } from '@/application/contracts/family-aggregate-validation';
import type { FamilyAggregateRepositories } from '@/application/ports/family-aggregate-repositories';

import { LocalStorageVersionedAggregateRepository } from './local-storage-versioned-aggregate-repository';
import { LocalStorageHomeCustomizationRepository } from './local-storage-home-customization-repository';

export class LocalStorageFamilyProfileRepository extends LocalStorageVersionedAggregateRepository<
  typeof FAMILY_PROFILE_AGGREGATE_TYPE,
  typeof FAMILY_PROFILE_SCHEMA_VERSION,
  FamilyProfileState
> {
  public constructor() {
    super({ aggregateType: FAMILY_PROFILE_AGGREGATE_TYPE, isState: isFamilyProfileState, schemaVersion: FAMILY_PROFILE_SCHEMA_VERSION });
  }
}

export class LocalStorageContributionsRepository extends LocalStorageVersionedAggregateRepository<
  typeof CONTRIBUTIONS_AGGREGATE_TYPE,
  typeof CONTRIBUTIONS_SCHEMA_VERSION,
  ContributionsState
> {
  public constructor() {
    super({ aggregateType: CONTRIBUTIONS_AGGREGATE_TYPE, isState: isContributionsState, schemaVersion: CONTRIBUTIONS_SCHEMA_VERSION });
  }
}

export class LocalStorageSavingsRepository extends LocalStorageVersionedAggregateRepository<
  typeof SAVINGS_AGGREGATE_TYPE,
  typeof SAVINGS_SCHEMA_VERSION,
  SavingsState
> {
  public constructor() {
    super({ aggregateType: SAVINGS_AGGREGATE_TYPE, isState: isSavingsState, schemaVersion: SAVINGS_SCHEMA_VERSION });
  }
}

export class LocalStorageRewardShopRepository extends LocalStorageVersionedAggregateRepository<
  typeof REWARD_SHOP_AGGREGATE_TYPE,
  typeof REWARD_SHOP_SCHEMA_VERSION,
  RewardShopState
> {
  public constructor() {
    super({ aggregateType: REWARD_SHOP_AGGREGATE_TYPE, isState: isRewardShopState, schemaVersion: REWARD_SHOP_SCHEMA_VERSION });
  }
}

export class LocalStorageFamilyProgressionRepository extends LocalStorageVersionedAggregateRepository<
  typeof FAMILY_PROGRESSION_AGGREGATE_TYPE,
  typeof FAMILY_PROGRESSION_SCHEMA_VERSION,
  FamilyProgressionState
> {
  public constructor() {
    super({ aggregateType: FAMILY_PROGRESSION_AGGREGATE_TYPE, isState: isFamilyProgressionState, schemaVersion: FAMILY_PROGRESSION_SCHEMA_VERSION });
  }
}

export const createLocalStorageFamilyAggregateRepositories = (): FamilyAggregateRepositories => ({
  contributions: new LocalStorageContributionsRepository(),
  familyProfile: new LocalStorageFamilyProfileRepository(),
  familyProgression: new LocalStorageFamilyProgressionRepository(),
  homeCustomization: new LocalStorageHomeCustomizationRepository(),
  rewardShop: new LocalStorageRewardShopRepository(),
  savings: new LocalStorageSavingsRepository(),
});
