import { CONTRIBUTION_RATING } from "@/domain/contributions/rating";
import { SAVINGS_RULES } from "@/domain/savings/rules";
import { PERCENTAGE_BASE } from "@/domain/shared/numbers";

/** Inputs used by a savings-interest policy. */
export interface SavingsInterestContext {
  baseRate: number;
  completionBonusRate: number;
  completionPercent: number;
  maxRate: number;
  rating: number;
  ratingBonusRate: number;
  streakBonusRate: number;
  streakDays: number;
}

export interface SavingsInterestStrategy {
  calculateRate: (context: SavingsInterestContext) => number;
}

export const calculateSavingsCredit = (
  saved: number,
  target: number,
  ratePercent: number,
): number => {
  if (saved <= 0 || saved >= target) {return 0;}
  const remaining = target - saved;
  const calculatedCredit = Math.max(
    SAVINGS_RULES.minimumInterestCredit,
    Math.round(saved * (ratePercent / PERCENTAGE_BASE)),
  );
  return Math.min(remaining, calculatedCredit);
};

export const familyParticipationInterestStrategy: SavingsInterestStrategy = {
  calculateRate(context) {
    const rate = context.baseRate
      + context.streakDays * context.streakBonusRate
      + (context.completionPercent / PERCENTAGE_BASE) * context.completionBonusRate
      + (context.rating / CONTRIBUTION_RATING.maximum) * context.ratingBonusRate;

    return Number(Math.min(context.maxRate, rate).toFixed(2));
  },
};
