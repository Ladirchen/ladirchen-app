import type { VersionedAggregateSnapshot } from './versioned-aggregate-contract';
import type { FamilyMember, FamilyPet, IanaTimeZone, SubscriptionTier } from '@/domain/family/types';

export const FAMILY_PROFILE_SCHEMA_VERSION = 2 as const;
export const FAMILY_PROFILE_AGGREGATE_TYPE = 'family-profile' as const;

export interface FamilyProfileState {
  readonly members: ReadonlyArray<FamilyMember>;
  readonly onboardingCompleted: boolean;
  readonly pets: ReadonlyArray<FamilyPet>;
  readonly subscriptionTier: SubscriptionTier;
  readonly timeZone: IanaTimeZone;
}

export type FamilyProfileSnapshot = VersionedAggregateSnapshot<
  typeof FAMILY_PROFILE_AGGREGATE_TYPE,
  typeof FAMILY_PROFILE_SCHEMA_VERSION,
  FamilyProfileState
>;
