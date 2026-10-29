import type { SavingsState } from "@/application/contracts/savings-contract";
import type {
  FamilyCurrency,
  GoalVisibility,
  GuardianGift,
  SavingGoal,
  SavingsVoucher,
  SavingsVoucherStatus,
} from "@/domain/savings/types";
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
const voucherStatuses = exhaustiveValues<SavingsVoucherStatus>({ pending: true, redeemable: true, redeemed: true });
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
    isOptionalString(value.createdAt) &&
    isOptionalNonNegativeNumber(value.interestEarned) &&
    isOptionalNonNegativeNumber(value.projectedInterestCredit) &&
    isOptionalNonNegativeNumber(value.remainingAmount) &&
    isOptionalNonNegativeNumber(value.withdrawableBalance) &&
    (value.visibleToMemberIds === undefined || isArrayOf(value.visibleToMemberIds, isFamilyMemberId)) &&
    (value.participantIds === undefined || isArrayOf(value.participantIds, isFamilyMemberId)) &&
    (value.voucherId === undefined || isDomainId(value.voucherId)) &&
    (value.commandLimits === undefined ||
      (isRecord(value.commandLimits) &&
        Object.entries(value.commandLimits).every(
          ([memberId, limit]) =>
            isFamilyMemberId(memberId) &&
            isRecord(limit) &&
            typeof limit.canCancel === "boolean" &&
            typeof limit.canWithdraw === "boolean" &&
            isFiniteNumber(limit.maximumDepositAmount) &&
            limit.maximumDepositAmount >= 0 &&
            isFiniteNumber(limit.maximumSupportAmount) &&
            limit.maximumSupportAmount >= 0,
        ))) &&
    isKnownString(value.visibility, goalVisibilities) &&
    typeof value.shared === "boolean" &&
    typeof value.cheered === "boolean"
  );
}
function isSavingsVoucher(value: unknown): value is SavingsVoucher {
  return (
    isRecord(value) &&
    isDomainId(value.id) &&
    isDomainId(value.goalId) &&
    isFiniteNumber(value.amount) &&
    value.amount > 0 &&
    (value.ownerId === undefined || value.ownerId === null || isFamilyMemberId(value.ownerId)) &&
    isArrayOf(value.participantIds, isFamilyMemberId) &&
    isArrayOf(value.consentedMemberIds, isFamilyMemberId) &&
    isKnownString(value.status, voucherStatuses) &&
    isOptionalString(value.redeemedAt) &&
    isArrayOf(value.visibleToMemberIds, isFamilyMemberId) &&
    isArrayOf(value.consentableByMemberIds, isFamilyMemberId) &&
    isArrayOf(value.redeemableByMemberIds, isFamilyMemberId)
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
    hasUniqueIds(value.pendingGuardianGifts) &&
    isArrayOf(value.settledInterestPeriods, isNonEmptyString) &&
    isArrayOf(value.vouchers, isSavingsVoucher) &&
    hasUniqueIds(value.vouchers)
  );
}
