import type { VersionedAggregateSnapshot } from "./versioned-aggregate-contract";
import type { HouseStageLevel } from "@/domain/house";
import type { LadiStageId } from "@/domain/ladi";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export const FAMILY_PROGRESSION_SCHEMA_VERSION = 3;
export const FAMILY_PROGRESSION_AGGREGATE_TYPE = "family-progression";

export interface MemberProgression {
  readonly approvedDates: ReadonlyArray<string>;
  readonly averageBasicRating: number;
  readonly completionPercent: number;
  readonly energyContribution: number;
  readonly ladiStageId: LadiStageId;
  readonly ladiStagePosition: number;
  readonly participationValue: number;
  readonly savingsInterestRate: number;
  readonly streakDays: number;
}

export interface FamilyProgressionState {
  readonly completedWeeklyStreak: number;
  readonly currentWeekTarget: number;
  readonly familyEnergy: number;
  readonly houseLevel: HouseStageLevel;
  readonly houseVisualLevel: number;
  readonly maximumSavingsInterestRate: number;
  readonly memberProgressions: Readonly<Partial<Record<FamilyMemberId, MemberProgression>>>;
}

export type FamilyProgressionSnapshot = VersionedAggregateSnapshot<
  typeof FAMILY_PROGRESSION_AGGREGATE_TYPE,
  typeof FAMILY_PROGRESSION_SCHEMA_VERSION,
  FamilyProgressionState
>;
