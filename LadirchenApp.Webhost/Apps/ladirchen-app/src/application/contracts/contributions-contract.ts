import type { VersionedAggregateSnapshot } from "./versioned-aggregate-contract";
import type { Contribution, Promotion } from "@/domain/contributions/types";

export const CONTRIBUTIONS_SCHEMA_VERSION = 1;
export const CONTRIBUTIONS_AGGREGATE_TYPE = "contributions";

export interface ContributionsState {
  readonly contributions: ReadonlyArray<Contribution>;
  readonly promotions: ReadonlyArray<Promotion>;
}

export type ContributionsSnapshot = VersionedAggregateSnapshot<
  typeof CONTRIBUTIONS_AGGREGATE_TYPE,
  typeof CONTRIBUTIONS_SCHEMA_VERSION,
  ContributionsState
>;
