import { clamp } from "@/domain/shared/numbers";
export const SAVINGS_RULES = Object.freeze({
  defaultTransferAmount: 25,
  goalStarterBonus: 5,
  maximumSpecialGift: 10000,
  minimumInterestCredit: 1,
  minimumSpecialGift: 1,
});
export function normalizeSpecialGiftAmount(amount: number): number {

  return clamp(Math.round(amount), SAVINGS_RULES.minimumSpecialGift, SAVINGS_RULES.maximumSpecialGift);
}
