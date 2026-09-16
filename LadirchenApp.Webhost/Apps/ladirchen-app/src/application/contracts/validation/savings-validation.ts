import type { SavingsState } from '../savings-contract';
import type { FamilyCurrency, GoalVisibility, GuardianGift, SavingGoal } from '@/domain/savings/types';
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
  type StateGuard,
} from './runtime-validation';

const familyCurrencies = exhaustiveValues<FamilyCurrency>({ CHF: true, EUR: true, HUF: true });
const goalVisibilities = exhaustiveValues<GoalVisibility>({ family: true, guardians: true, private: true });

export const isFamilyCurrency = (value: unknown): value is FamilyCurrency =>
  isKnownString(value, familyCurrencies);

const isSavingGoal = (value: unknown): value is SavingGoal =>
  isRecord(value) &&
  isDomainId(value.id) &&
  isOptionalString(value.translationKey) &&
  hasLocalizedValue(value, 'title') &&
  isNonEmptyString(value.icon) &&
  (value.ownerId === 'family' || isFamilyMemberId(value.ownerId)) &&
  isFiniteNumber(value.target) && value.target > 0 &&
  isFiniteNumber(value.saved) && value.saved >= 0 &&
  isOptionalNonNegativeNumber(value.starterBonus) &&
  isOptionalNonNegativeNumber(value.interestEarned) &&
  isKnownString(value.visibility, goalVisibilities) &&
  typeof value.shared === 'boolean' &&
  typeof value.cheered === 'boolean';

const isGuardianGift = (value: unknown): value is GuardianGift =>
  isRecord(value) &&
  isDomainId(value.id) &&
  isFamilyMemberId(value.childId) &&
  isNonEmptyString(value.guardianName) &&
  isNonEmptyString(value.goalTitle) &&
  (value.destination === 'balance' || value.destination === 'goal') &&
  isFiniteNumber(value.amount) && value.amount > 0;

export const isSavingsState: StateGuard<SavingsState> = (value): value is SavingsState =>
  isRecord(value) &&
  isRecord(value.balances) &&
  Object.entries(value.balances).every(([memberId, balance]) =>
    isFamilyMemberId(memberId) && isFiniteNumber(balance) && balance >= 0) &&
  isFamilyCurrency(value.familyCurrencyCode) &&
  isArrayOf(value.goals, isSavingGoal) && hasUniqueIds(value.goals) &&
  isFiniteNumber(value.ladirchenPerCurrencyUnit) && value.ladirchenPerCurrencyUnit >= 1 &&
  isArrayOf(value.pendingGuardianGifts, isGuardianGift) && hasUniqueIds(value.pendingGuardianGifts);
