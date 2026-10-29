import type { SavingsState } from "@/application/contracts/savings-contract";
import type { FamilyCurrency, GoalVisibility, GuardianGift, SavingGoal } from "@/domain/savings/types";
import {
  exhaustiveValues,
  hasLocalizedValue,
  hasUniqueIds,
  isArrayOf,
  isDomainId,
  isFamilyMemberId,
  isFiniteNumber,
  isKnownString,
  isNonEmptyString,
  isOptionalNonNegativeNumber,
  isOptionalString,
  isRecord,
} from "./runtime-validation";
const familyCurrencies = exhaustiveValues<FamilyCurrency>({ CHF: true, EUR: true, HUF: true });
const goalVisibilities = exhaustiveValues<GoalVisibility>({ family: true, guardians: true, private: true });
export function isFamilyCurrency(value: unknown): value is FamilyCurrency {
  return isKnownString(value, familyCurrencies);
}
function isSavingGoal(value: unknown): value is SavingGoal {
  return (
    isRecord(value) &&
    isDomainId(value.id) &&
    isOptionalString(value.translationKey) &&
    hasLocalizedValue(value, "title") &&
    isNonEmptyString(value.icon) &&
    (value.ownerId === "family" || isFamilyMemberId(value.ownerId)) &&
    isFiniteNumber(value.target) &&
    value.target > 0 &&
    isFiniteNumber(value.saved) &&
    value.saved >= 0 &&
    isOptionalNonNegativeNumber(value.starterBonus) &&
    isOptionalNonNegativeNumber(value.interestEarned) &&
    isOptionalNonNegativeNumber(value.projectedInterestCredit) &&
    isOptionalNonNegativeNumber(value.remainingAmount) &&
    isOptionalNonNegativeNumber(value.withdrawableBalance) &&
    (value.visibleToMemberIds === undefined || isArrayOf(value.visibleToMemberIds, isFamilyMemberId)) &&
    isKnownString(value.visibility, goalVisibilities) &&
    typeof value.shared === "boolean" &&
    typeof value.cheered === "boolean"
  );
}
function isGuardianGift(value: unknown): value is GuardianGift {
  return (
    isRecord(value) &&
    isDomainId(value.id) &&
    isFamilyMemberId(value.childId) &&
    isNonEmptyString(value.guardianName) &&
    isNonEmptyString(value.goalTitle) &&
    (value.destination === "balance" || value.destination === "goal") &&
    isFiniteNumber(value.amount) &&
    value.amount > 0
  );
}
export function isSavingsState(value: unknown): value is SavingsState {
  return (
    isRecord(value) &&
    isRecord(value.balances) &&
    Object.entries(value.balances).every(
      ([memberId, balance]) => isFamilyMemberId(memberId) && isFiniteNumber(balance) && balance >= 0,
    ) &&
    (value.availableBalances === undefined ||
      (isRecord(value.availableBalances) &&
        Object.entries(value.availableBalances).every(
          ([memberId, balance]) => isFamilyMemberId(memberId) && isFiniteNumber(balance) && balance >= 0,
        ))) &&
    (value.sharedSavedBalances === undefined ||
      (isRecord(value.sharedSavedBalances) &&
        Object.entries(value.sharedSavedBalances).every(
          ([memberId, balance]) => isFamilyMemberId(memberId) && isFiniteNumber(balance) && balance >= 0,
        ))) &&
    isFamilyCurrency(value.familyCurrencyCode) &&
    isArrayOf(value.goals, isSavingGoal) &&
    hasUniqueIds(value.goals) &&
    isFiniteNumber(value.ladirchenPerCurrencyUnit) &&
    value.ladirchenPerCurrencyUnit >= 1 &&
    isArrayOf(value.pendingGuardianGifts, isGuardianGift) &&
    hasUniqueIds(value.pendingGuardianGifts)
  );
}
