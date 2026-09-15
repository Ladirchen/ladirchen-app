import { FAMILY_AGGREGATE_DESCRIPTORS } from '@/application/contracts/family-aggregate-descriptors';
import type { FamilyAggregateRepositories } from '@/application/ports/family-aggregate-repositories';

import { HttpVersionedAggregateRepository } from './http-versioned-aggregate-repository';

export const createHttpFamilyAggregateRepositories = (apiBaseUrl: string): FamilyAggregateRepositories => ({
  contributions: new HttpVersionedAggregateRepository({
    ...FAMILY_AGGREGATE_DESCRIPTORS.contributions,
    apiBaseUrl,
  }),
  familyProfile: new HttpVersionedAggregateRepository({
    ...FAMILY_AGGREGATE_DESCRIPTORS.familyProfile,
    apiBaseUrl,
  }),
  familyProgression: new HttpVersionedAggregateRepository({
    ...FAMILY_AGGREGATE_DESCRIPTORS.familyProgression,
    apiBaseUrl,
  }),
  homeCustomization: new HttpVersionedAggregateRepository({
    ...FAMILY_AGGREGATE_DESCRIPTORS.homeCustomization,
    apiBaseUrl,
  }),
  rewardShop: new HttpVersionedAggregateRepository({
    ...FAMILY_AGGREGATE_DESCRIPTORS.rewardShop,
    apiBaseUrl,
  }),
  savings: new HttpVersionedAggregateRepository({
    ...FAMILY_AGGREGATE_DESCRIPTORS.savings,
    apiBaseUrl,
  }),
});
