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

export const familyParticipationInterestStrategy: SavingsInterestStrategy = {
  calculateRate(context) {
    const rate = context.baseRate
      + context.streakDays * context.streakBonusRate
      + (context.completionPercent / 100) * context.completionBonusRate
      + (context.rating / 5) * context.ratingBonusRate;

    return Number(Math.min(context.maxRate, rate).toFixed(2));
  },
};
