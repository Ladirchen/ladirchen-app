import { CONTRIBUTIONS_AGGREGATE_TYPE } from '@/application/contracts/contributions-contract';
import type { CONTRIBUTIONS_SCHEMA_VERSION, ContributionsState } from '@/application/contracts/contributions-contract';
import { FAMILY_PROFILE_AGGREGATE_TYPE } from '@/application/contracts/family-profile-contract';
import type { FAMILY_PROFILE_SCHEMA_VERSION, FamilyProfileState } from '@/application/contracts/family-profile-contract';
import { FAMILY_PROGRESSION_AGGREGATE_TYPE } from '@/application/contracts/family-progression-contract';
import type { FAMILY_PROGRESSION_SCHEMA_VERSION, FamilyProgressionState } from '@/application/contracts/family-progression-contract';
import { HOME_CUSTOMIZATION_AGGREGATE_TYPE } from '@/application/contracts/home-customization-contract';
import type { HOME_CUSTOMIZATION_SCHEMA_VERSION, HomeCustomizationState } from '@/application/contracts/home-customization-contract';
import { REWARD_SHOP_AGGREGATE_TYPE } from '@/application/contracts/reward-shop-contract';
import type { REWARD_SHOP_SCHEMA_VERSION, RewardShopState } from '@/application/contracts/reward-shop-contract';
import { SAVINGS_AGGREGATE_TYPE } from '@/application/contracts/savings-contract';
import type { SAVINGS_SCHEMA_VERSION, SavingsState } from '@/application/contracts/savings-contract';
import type { ContributionsRepository, FamilyProfileRepository, FamilyProgressionRepository, HomeCustomizationRepository, RewardShopRepository, SavingsRepository } from '@/application/ports/family-aggregate-repositories';

import { VersionedAggregateService } from './versioned-aggregate-service';

export class ContributionsService extends VersionedAggregateService<typeof CONTRIBUTIONS_AGGREGATE_TYPE, typeof CONTRIBUTIONS_SCHEMA_VERSION, ContributionsState> {
  public constructor(repository: ContributionsRepository) {
    super(CONTRIBUTIONS_AGGREGATE_TYPE, repository);
  }
}

export class FamilyProfileService extends VersionedAggregateService<typeof FAMILY_PROFILE_AGGREGATE_TYPE, typeof FAMILY_PROFILE_SCHEMA_VERSION, FamilyProfileState> {
  public constructor(repository: FamilyProfileRepository) {
    super(FAMILY_PROFILE_AGGREGATE_TYPE, repository);
  }
}

export class FamilyProgressionService extends VersionedAggregateService<typeof FAMILY_PROGRESSION_AGGREGATE_TYPE, typeof FAMILY_PROGRESSION_SCHEMA_VERSION, FamilyProgressionState> {
  public constructor(repository: FamilyProgressionRepository) {
    super(FAMILY_PROGRESSION_AGGREGATE_TYPE, repository);
  }
}

export class HomeCustomizationService extends VersionedAggregateService<typeof HOME_CUSTOMIZATION_AGGREGATE_TYPE, typeof HOME_CUSTOMIZATION_SCHEMA_VERSION, HomeCustomizationState> {
  public constructor(repository: HomeCustomizationRepository) {
    super(HOME_CUSTOMIZATION_AGGREGATE_TYPE, repository);
  }
}

export class RewardShopService extends VersionedAggregateService<typeof REWARD_SHOP_AGGREGATE_TYPE, typeof REWARD_SHOP_SCHEMA_VERSION, RewardShopState> {
  public constructor(repository: RewardShopRepository) {
    super(REWARD_SHOP_AGGREGATE_TYPE, repository);
  }
}

export class SavingsService extends VersionedAggregateService<typeof SAVINGS_AGGREGATE_TYPE, typeof SAVINGS_SCHEMA_VERSION, SavingsState> {
  public constructor(repository: SavingsRepository) {
    super(SAVINGS_AGGREGATE_TYPE, repository);
  }
}
