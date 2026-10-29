import type { VersionedAggregateSnapshot } from "./versioned-aggregate-contract";
import type { Contribution, Promotion } from "@/domain/contributions/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export const CONTRIBUTIONS_SCHEMA_VERSION = 1;
export const CONTRIBUTIONS_AGGREGATE_TYPE = "contributions";

export interface ContributionsState {
  readonly contributions: ReadonlyArray<Contribution>;
  readonly perfectRatingBonusPercent?: number;
  readonly promotions: ReadonlyArray<Promotion>;
  readonly todayEarnedByMember?: Readonly<Record<FamilyMemberId, number>>;
}

export type ContributionsSnapshot = VersionedAggregateSnapshot<
  typeof CONTRIBUTIONS_AGGREGATE_TYPE,
  typeof CONTRIBUTIONS_SCHEMA_VERSION,
  ContributionsState
>;
