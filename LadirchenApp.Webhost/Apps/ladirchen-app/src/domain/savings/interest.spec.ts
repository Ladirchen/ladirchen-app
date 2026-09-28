import { describe, expect, it } from "vitest";

import { calculateSavingsCredit, familyParticipationInterestStrategy } from "./interest";

describe("savings interest rules", () => {
  it("calculates the configured participation rate", () => {
    expect(familyParticipationInterestStrategy.calculateRate({
      baseRate: 1,
      completionBonusRate: 3.5,
      completionPercent: 60,
      maxRate: 8,
      rating: 4,
      ratingBonusRate: 2.5,
      streakBonusRate: 0.5,
      streakDays: 2,
    })).toBe(6.1);
  });

  it("limits a credit to the remaining goal amount", () => {
    expect(calculateSavingsCredit(99, 100, 8)).toBe(1);
    expect(calculateSavingsCredit(100, 100, 8)).toBe(0);
  });
});

