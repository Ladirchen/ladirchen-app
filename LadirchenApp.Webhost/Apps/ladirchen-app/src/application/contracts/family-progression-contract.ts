import type { VersionedAggregateSnapshot } from './versioned-aggregate-contract';
import type { HouseStageLevel } from '@/domain/house';

export const FAMILY_PROGRESSION_SCHEMA_VERSION = 2 as const;
export const FAMILY_PROGRESSION_AGGREGATE_TYPE = 'family-progression' as const;

export interface FamilyProgressionState {
  readonly completedWeeklyStreak: number;
  readonly currentWeekTarget: number;
  readonly houseLevel: HouseStageLevel;
}

export type FamilyProgressionSnapshot = VersionedAggregateSnapshot<
  typeof FAMILY_PROGRESSION_AGGREGATE_TYPE,
  typeof FAMILY_PROGRESSION_SCHEMA_VERSION,
  FamilyProgressionState
>;
