import { describe, expect, it } from "vite-plus/test";

import { isContributionsState } from "./contributions-validation";
import { isFamilyPetKindId } from "./family-profile-validation";
import { isFamilyProgressionState } from "./family-progression-validation";
import { isHomeCustomizationState, isHouseAccessoryId, isHouseThemeId } from "./home-customization-validation";
import { isFamilyCurrency, isSavingsState } from "./savings-validation";

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

  it("requires backend-projected available and shared savings balances", () => {
    const savings = {
      availableBalances: {},
      balances: {},
      familyCurrencyCode: "CHF",
      goals: [],
      ladirchenPerCurrencyUnit: 1,
      pendingGuardianGifts: [],
      settledInterestPeriods: [],
      sharedSavedBalances: {},
      vouchers: [],
    };

    expect(isSavingsState(savings)).toBe(true);
    expect(isSavingsState({ ...savings, availableBalances: undefined })).toBe(false);
    expect(isSavingsState({ ...savings, sharedSavedBalances: undefined })).toBe(false);
  });

  it("validates cancellation requests and backend-projected decision capabilities", () => {
    const goal = {
      cancellationRequest: {
        requestedByMemberId: "member-1",
        consentedMemberIds: ["member-1"],
        pendingMemberIds: ["member-2"],
      },
      cheered: false,
      commandLimits: {
        "member-2": {
          canApproveCancellation: true,
          canCancel: false,
          canRejectCancellation: true,
          canRequestCancellation: false,
          canWithdraw: false,
          maximumDepositAmount: 0,
          maximumSupportAmount: 0,
        },
      },
      icon: "🎯",
      id: "goal-1",
      ownerId: "family",
      participantIds: ["member-1", "member-2"],
      saved: 25,
      shared: true,
      target: 100,
      title: "Family goal",
      visibility: "family",
    };
    const savings = {
      availableBalances: {},
      balances: {},
      familyCurrencyCode: "CHF",
      goals: [goal],
      ladirchenPerCurrencyUnit: 1,
      pendingGuardianGifts: [],
      settledInterestPeriods: [],
      sharedSavedBalances: {},
      vouchers: [],
    };

    expect(isSavingsState(savings)).toBe(true);
    expect(
      isSavingsState({
        ...savings,
        goals: [
          {
            ...goal,
            cancellationRequest: {
              requestedByMemberId: "member-1",
              consentedMemberIds: ["member-1"],
            },
          },
        ],
      }),
    ).toBe(false);
    expect(
      isSavingsState({
        ...savings,
        goals: [
          {
            ...goal,
            commandLimits: {
              "member-2": {
                ...goal.commandLimits["member-2"],
                canRejectCancellation: undefined,
              },
            },
          },
        ],
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

  it("validates backend-projected contribution partner identifiers", () => {
    const contribution = {
      area: "Family home",
      description: "Work together",
      dueLabel: "Optional",
      energy: 10,
      icon: "🤝",
      id: "extra-task",
      kind: "extra",
      partnerableMemberIds: ["guardian-1", "child-2"],
      reward: 10,
      status: "available",
      title: "Extra task",
    };

    const currentWeekReadModel = {
      currentWeekApprovedContributionCountByMember: { "child-2": 2 },
      currentWeekEarnedByMember: { "child-2": 25 },
    };

    expect(isContributionsState({ contributions: [contribution], ...currentWeekReadModel, promotions: [] })).toBe(true);
    expect(
      isContributionsState({
        contributions: [{ ...contribution, partnerableMemberIds: ["guardian-1", "guardian-1"] }],
        ...currentWeekReadModel,
        promotions: [],
      }),
    ).toBe(false);
    expect(
      isContributionsState({
        contributions: [contribution],
        ...currentWeekReadModel,
        currentWeekApprovedContributionCountByMember: { "child-2": 1.5 },
        promotions: [],
      }),
    ).toBe(false);
  });
});
