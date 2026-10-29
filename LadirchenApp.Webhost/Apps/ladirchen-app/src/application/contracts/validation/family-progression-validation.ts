import type { FamilyProgressionState } from "@/application/contracts/family-progression-contract";
import { HOUSE_STAGES } from "@/domain/house/catalog";
import { isFiniteNumber, isInteger, isNonNegativeInteger, isRecord } from "./runtime-validation";

const stageLevels = new Set<number>(HOUSE_STAGES.map((item) => item.level));
const ladiStageIds = new Set(["idle-ladi", "starter-ladi", "garden-ladi", "sun-ladi", "aurora-ladi", "super-ladi"]);

function isMemberProgression(value: unknown): boolean {
  return (
    isRecord(value) &&
    isNonNegativeInteger(value.streakDays) &&
    isFiniteNumber(value.completionPercent) &&
    isFiniteNumber(value.averageBasicRating) &&
    isFiniteNumber(value.participationValue) &&
    typeof value.ladiStageId === "string" &&
    ladiStageIds.has(value.ladiStageId) &&
    isInteger(value.ladiStagePosition) &&
    value.ladiStagePosition >= 1 &&
    value.ladiStagePosition <= 6 &&
    isFiniteNumber(value.energyContribution) &&
    isFiniteNumber(value.savingsInterestRate)
  );
}

export function isFamilyProgressionState(value: unknown): value is FamilyProgressionState {
  return (
    isRecord(value) &&
    isNonNegativeInteger(value.completedWeeklyStreak) &&
    isInteger(value.currentWeekTarget) &&
    value.currentWeekTarget > 0 &&
    isInteger(value.houseLevel) &&
    stageLevels.has(value.houseLevel) &&
    isFiniteNumber(value.familyEnergy) &&
    isInteger(value.houseVisualLevel) &&
    isFiniteNumber(value.maximumSavingsInterestRate) &&
    isRecord(value.memberProgressions) &&
    Object.values(value.memberProgressions).every(isMemberProgression)
  );
}
