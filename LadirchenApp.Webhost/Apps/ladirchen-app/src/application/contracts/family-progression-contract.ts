import type { FamilyReadModel } from "./family-read-model-contract";
import type { HouseAtmosphere, HouseEnergyVisualLevel, HouseStageLevel } from "@/domain/house";
import type { LadiStageId } from "@/domain/ladi";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export const FAMILY_PROGRESSION_READ_MODEL_TYPE = "family-progression";

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
  readonly canCloseWeekSuccessfully: boolean;
  readonly completedWeeklyStreak: number;
  readonly currentWeekTarget: number;
  readonly familyEnergy: number;
  readonly houseAtmosphere: HouseAtmosphere;
  readonly houseLevel: HouseStageLevel;
  readonly houseVisualLevel: HouseEnergyVisualLevel;
  readonly maximumSavingsInterestRate: number;
  readonly minimumSuccessfulWeekEnergy: number;
  readonly memberProgressions: Readonly<Partial<Record<FamilyMemberId, MemberProgression>>>;
}

export type FamilyProgressionReadModel = FamilyReadModel<
  typeof FAMILY_PROGRESSION_READ_MODEL_TYPE,
  FamilyProgressionState
>;
