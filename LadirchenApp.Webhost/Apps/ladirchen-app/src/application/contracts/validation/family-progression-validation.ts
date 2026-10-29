import type { FamilyProgressionState } from "@/application/contracts/family-progression-contract";
import { HOUSE_STAGES } from "@/domain/house/catalog";
import { isArrayOf, isFiniteNumber, isInteger, isNonNegativeInteger, isRecord } from "./runtime-validation";

const stageLevels = new Set<number>(HOUSE_STAGES.map((item) => item.level));
const ladiStageIds = new Set(["idle-ladi", "starter-ladi", "garden-ladi", "sun-ladi", "aurora-ladi", "super-ladi"]);

function isMemberProgression(value: unknown): boolean {
  return (
    isRecord(value) &&
    isArrayOf(
      value.approvedDates,
      (date): date is string => typeof date === "string" && /^\d{4}-\d{2}-\d{2}$/.test(date),
    ) &&
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
    typeof value.canCloseWeekSuccessfully === "boolean" &&
    isNonNegativeInteger(value.completedWeeklyStreak) &&
    isInteger(value.currentWeekTarget) &&
    value.currentWeekTarget > 0 &&
    isInteger(value.houseLevel) &&
    stageLevels.has(value.houseLevel) &&
    isFiniteNumber(value.familyEnergy) &&
    (value.houseAtmosphere === "cloudy" || value.houseAtmosphere === "rainy" || value.houseAtmosphere === "sunny") &&
    isInteger(value.houseVisualLevel) &&
    value.houseVisualLevel >= 1 &&
    value.houseVisualLevel <= 5 &&
    isFiniteNumber(value.maximumSavingsInterestRate) &&
    isFiniteNumber(value.minimumSuccessfulWeekEnergy) &&
    isRecord(value.memberProgressions) &&
    Object.values(value.memberProgressions).every(isMemberProgression)
  );
}
