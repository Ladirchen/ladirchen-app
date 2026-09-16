import { clamp } from '@/domain/shared/numbers';

export const SAVINGS_RULES = Object.freeze({
  defaultTransferAmount: 25,
  goalStarterBonus: 5,
  maximumSpecialGift: 10_000,
  minimumInterestCredit: 1,
  minimumSpecialGift: 1,
});

export const normalizeSpecialGiftAmount = (amount: number): number =>
  clamp(Math.round(amount), SAVINGS_RULES.minimumSpecialGift, SAVINGS_RULES.maximumSpecialGift);

