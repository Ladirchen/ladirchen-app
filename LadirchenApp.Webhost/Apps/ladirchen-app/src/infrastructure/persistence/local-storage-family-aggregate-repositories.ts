import { FAMILY_AGGREGATE_DESCRIPTORS } from '@/application/contracts/family-aggregate-descriptors';
import type { FamilyAggregateRepositories } from '@/application/ports/family-aggregate-repositories';

import { LocalStorageVersionedAggregateRepository } from './local-storage-versioned-aggregate-repository';

export const createLocalStorageFamilyAggregateRepositories = (): FamilyAggregateRepositories => ({
  contributions: new LocalStorageVersionedAggregateRepository(FAMILY_AGGREGATE_DESCRIPTORS.contributions),
  familyProfile: new LocalStorageVersionedAggregateRepository(FAMILY_AGGREGATE_DESCRIPTORS.familyProfile),
  familyProgression: new LocalStorageVersionedAggregateRepository(FAMILY_AGGREGATE_DESCRIPTORS.familyProgression),
  homeCustomization: new LocalStorageVersionedAggregateRepository(FAMILY_AGGREGATE_DESCRIPTORS.homeCustomization),
  rewardShop: new LocalStorageVersionedAggregateRepository(FAMILY_AGGREGATE_DESCRIPTORS.rewardShop),
  savings: new LocalStorageVersionedAggregateRepository(FAMILY_AGGREGATE_DESCRIPTORS.savings),
});
