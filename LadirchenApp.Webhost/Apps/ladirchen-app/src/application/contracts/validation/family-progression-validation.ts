import type { FamilyProgressionState } from "@/application/contracts/family-progression-contract";
import { HOUSE_STAGES } from "@/domain/house/catalog";
import { isInteger, isNonNegativeInteger, isRecord } from "./runtime-validation";

const stageLevels = new Set<number>(HOUSE_STAGES.map((item) => item.level));

export function isFamilyProgressionState(value: unknown): value is FamilyProgressionState {
  return (
    isRecord(value) &&
    isNonNegativeInteger(value.completedWeeklyStreak) &&
    isInteger(value.currentWeekTarget) &&
    value.currentWeekTarget > 0 &&
    isInteger(value.houseLevel) &&
    stageLevels.has(value.houseLevel)
  );
}
