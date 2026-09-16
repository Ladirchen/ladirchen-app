import type { VersionedAggregateSnapshot } from './versioned-aggregate-contract';
import type { Contribution, Promotion } from '@/domain/types';

export const CONTRIBUTIONS_SCHEMA_VERSION = 1 as const;
export const CONTRIBUTIONS_AGGREGATE_TYPE = 'contributions' as const;

export interface ContributionsState {
  readonly contributions: ReadonlyArray<Contribution>;
  readonly promotions: ReadonlyArray<Promotion>;
}

export type ContributionsSnapshot = VersionedAggregateSnapshot<
  typeof CONTRIBUTIONS_AGGREGATE_TYPE,
  typeof CONTRIBUTIONS_SCHEMA_VERSION,
  ContributionsState
>;
