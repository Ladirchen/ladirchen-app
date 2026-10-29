import { describe, expect, it } from "vite-plus/test";

import { isContributionsState } from "./contributions-validation";
import { isFamilyPetKindId } from "./family-profile-validation";
import { isFamilyProgressionState } from "./family-progression-validation";
import { isHomeCustomizationState, isHouseAccessoryId, isHouseThemeId } from "./home-customization-validation";
import { isFamilyCurrency } from "./savings-validation";

describe("family aggregate runtime validation", () => {
  it("rejects localized labels and unknown catalog identifiers", () => {
    expect(isFamilyPetKindId("cat")).toBe(true);
    expect(isFamilyPetKindId("Katze")).toBe(false);
    expect(isHouseAccessoryId("round-rug")).toBe(true);
    expect(isHouseAccessoryId("unknown-furniture")).toBe(false);
    expect(isHouseThemeId("sunny-dollhouse")).toBe(true);
    expect(isHouseThemeId("unknown-theme")).toBe(false);
    expect(isFamilyCurrency("CHF")).toBe(true);
    expect(isFamilyCurrency("USD")).toBe(false);
  });

  it("rejects a weekly progression without a positive target", () => {
    expect(
      isFamilyProgressionState({
        completedWeeklyStreak: 2,
        currentWeekTarget: 0,
        houseLevel: 2,
      }),
    ).toBe(false);
  });

  it("owns week-closing eligibility at family level instead of personal progression level", () => {
    const progression = {
      canCloseWeekSuccessfully: true,
      completedWeeklyStreak: 2,
      currentWeekTarget: 7,
      familyEnergy: 60,
      houseAtmosphere: "sunny",
      houseLevel: 2,
      houseVisualLevel: 3,
      maximumSavingsInterestRate: 8,
      memberProgressions: {
        "member-1": {
          approvedDates: ["2026-10-29"],
          averageBasicRating: 4,
          completionPercent: 80,
          energyContribution: 60,
          ladiStageId: "sun-ladi",
          ladiStagePosition: 4,
          participationValue: 5,
          savingsInterestRate: 5.33,
          streakDays: 8,
        },
      },
      minimumSuccessfulWeekEnergy: 60,
    };

    expect(isFamilyProgressionState(progression)).toBe(true);
    expect(isFamilyProgressionState({ ...progression, canCloseWeekSuccessfully: undefined })).toBe(false);
  });

  it("requires the selected house edition to be owned", () => {
    expect(
      isHomeCustomizationState({
        accessories: [],
        editions: [{ id: "sunny-dollhouse", owned: false }],
        placements: [],
        selectedEditionId: "sunny-dollhouse",
      }),
    ).toBe(false);
  });

  it("rejects promotions that reference a missing contribution", () => {
    expect(
      isContributionsState({
        contributions: [],
        promotions: [
          {
            id: "promotion-1",
            contributionId: "contribution-1",
            title: "Double reward",
            multiplier: 2,
            deadline: "17:00",
            teamworkBonus: 10,
            active: true,
          },
        ],
      }),
    ).toBe(false);
  });
});
