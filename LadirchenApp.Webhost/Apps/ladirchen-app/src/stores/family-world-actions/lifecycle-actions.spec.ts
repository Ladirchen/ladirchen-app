import { beforeEach, describe, expect, it, vi } from "vite-plus/test";

import { createContributions, createFamilyWorldInitialData } from "@/infrastructure/fixtures/family-world-fixtures";
import { HOUSE_THEMES, ROOM_DESIGNS } from "@/domain/house";
import type { FamilyBootstrapReadModel } from "@/application/contracts/family-bootstrap-contract";
import type { FamilyWorldDependencies } from "@/application/ports/family-world-dependencies";
import { createDomainId } from "@/domain/shared/identifiers";
import { createFamilyWorldTestDependencies, createFamilyWorldTestStore } from "@/stores/family-world-test-support";
import type { FamilyWorldStoreContext } from "@/stores/family-world-store-context";

const serviceMocks = vi.hoisted(() => ({
  contributionsLoad: vi.fn<FamilyWorldDependencies["contributionsService"]["load"]>(),
  familyBootstrapLoad: vi.fn<FamilyWorldDependencies["familyBootstrapService"]["load"]>(),
  familyProfileLoad: vi.fn<FamilyWorldDependencies["familyProfileService"]["load"]>(),
  familyProgressionLoad: vi.fn<FamilyWorldDependencies["familyProgressionService"]["load"]>(),
  homeCustomizationLoad: vi.fn<FamilyWorldDependencies["homeCustomizationService"]["load"]>(),
  updatesConnect: vi.fn<FamilyWorldDependencies["familyAggregateUpdates"]["connect"]>(),
  rewardShopLoad: vi.fn<FamilyWorldDependencies["rewardShopService"]["load"]>(),
  savingsLoad: vi.fn<FamilyWorldDependencies["savingsService"]["load"]>(),
}));

const dependencies = createFamilyWorldTestDependencies({
  contributionsService: { load: serviceMocks.contributionsLoad },
  familyBootstrapService: { load: serviceMocks.familyBootstrapLoad },
  familyAggregateUpdates: { connect: serviceMocks.updatesConnect, disconnect: vi.fn(async () => undefined) },
  familyContext: {
    activeFamilyId: createDomainId.family("c776a9b6-c37d-4a51-91ba-269a67274478"),
    setActiveFamilyId: vi.fn(),
  },
  familyProfileService: { load: serviceMocks.familyProfileLoad },
  familyProgressionService: { load: serviceMocks.familyProgressionLoad },
  homeCustomizationService: { load: serviceMocks.homeCustomizationLoad },
  rewardShopService: { load: serviceMocks.rewardShopLoad },
  savingsService: { load: serviceMocks.savingsLoad },
});

function createBootstrapReadModel(
  context: FamilyWorldStoreContext,
  contributions = context.contributions,
): FamilyBootstrapReadModel {
  return {
    contributions: {
      readModelType: "contributions",
      revision: 1,
      state: {
        contributions,
        currentWeekApprovedContributionCountByMember: context.currentWeekApprovedContributionCountByMember,
        currentWeekEarnedByMember: context.currentWeekEarnedByMember,
        perfectRatingBonusPercent: context.perfectRatingBonusPercent,
        promotions: context.promotions,
        todayEarnedByMember: context.todayEarnedByMember,
      },
    },
    familyProfile: {
      readModelType: "family-profile",
      revision: 1,
      state: {
        avatarCatalog: context.avatarCatalog,
        members: context.members,
        onboardingCompleted: context.onboardingCompleted,
        pets: context.pets,
        subscriptionTier: context.subscriptionTier,
        timeZone: context.familyTimeZone,
      },
    },
    familyProgression: {
      readModelType: "family-progression",
      revision: 1,
      state: {
        canCloseWeekSuccessfully: context.canCloseWeekSuccessfully,
        completedWeeklyStreak: context.completedWeeklyStreak,
        currentWeekTarget: context.currentWeekTarget,
        familyEnergy: context.familyEnergyReadModel,
        houseAtmosphere: context.houseAtmosphere,
        houseLevel: context.houseLevel,
        houseVisualLevel: context.houseVisualLevel,
        maximumSavingsInterestRate: context.maxSavingsRatePercent,
        memberProgressions: context.memberProgressions,
        minimumSuccessfulWeekEnergy: context.minimumSuccessfulWeekEnergy,
      },
    },
    homeCustomization: null,
    rewardShop: {
      readModelType: "reward-shop",
      revision: 1,
      state: {
        redemptionOpen: context.rewardShopRedemptionOpen,
        rewards: context.shopRewards,
      },
    },
    savings: {
      readModelType: "savings",
      revision: 1,
      state: {
        availableBalances: context.availableBalances,
        balances: context.balances,
        behaviorStatuses: context.behaviorStatuses,
        familyCurrencyCode: context.familyCurrencyCode,
        goals: context.goals,
        ladirchenPerCurrencyUnit: context.ladirchenPerCurrencyUnit,
        pendingBehaviorNotices: context.pendingBehaviorNotices,
        pendingGuardianGifts: context.pendingGuardianGifts,
        settledInterestPeriods: [],
        sharedSavedBalances: context.sharedSavedBalances,
        vouchers: context.savingsVouchers,
      },
    },
  } as FamilyBootstrapReadModel;
}

describe("hydrateFamilyReadModels", () => {
  beforeEach(() => {
    serviceMocks.contributionsLoad.mockReset().mockResolvedValue(null);
    serviceMocks.familyBootstrapLoad.mockReset().mockResolvedValue(null);
    serviceMocks.familyProfileLoad.mockReset().mockResolvedValue(null);
    serviceMocks.familyProgressionLoad.mockReset().mockResolvedValue(null);
    serviceMocks.homeCustomizationLoad.mockReset().mockResolvedValue(null);
    serviceMocks.rewardShopLoad.mockReset().mockResolvedValue(null);
    serviceMocks.savingsLoad.mockReset().mockResolvedValue(null);
    serviceMocks.updatesConnect.mockReset().mockResolvedValue(undefined);
  });

  it("keeps the current state and remains retryable when bootstrap loading fails", async () => {
    const contributions = createContributions();
    const firstContribution = contributions[0]!;
    firstContribution.title = "Loaded contribution";
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });
    const bootstrap = createBootstrapReadModel(context, contributions);
    serviceMocks.familyBootstrapLoad.mockRejectedValue(new Error("Bootstrap endpoint unavailable"));
    const notify = vi.spyOn(context, "notify").mockImplementation(() => undefined);

    await context.hydrateFamilyReadModels();

    expect(context.contributions[0]?.title).not.toBe("Loaded contribution");
    expect(serviceMocks.familyBootstrapLoad).toHaveBeenCalledWith(
      dependencies.familyContext.activeFamilyId,
      context.signedInMemberId,
    );
    expect(context.familyReadModelsHydrated).toBe(false);
    expect(notify).toHaveBeenCalledWith("notifications.load.family");

    serviceMocks.familyBootstrapLoad.mockResolvedValue(bootstrap);
    await context.hydrateFamilyReadModels();

    expect(context.contributions[0]?.title).toBe("Loaded contribution");
    expect(context.familyReadModelsHydrated).toBe(true);
  });

  it("marks hydration complete only when every aggregate load succeeds", async () => {
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });
    serviceMocks.familyBootstrapLoad.mockResolvedValue(createBootstrapReadModel(context));
    const notify = vi.spyOn(context, "notify").mockImplementation(() => undefined);
    const refreshFamilyReadModel = vi.spyOn(context, "refreshFamilyReadModel").mockResolvedValue(undefined);
    const refreshAllFamilyReadModels = vi.spyOn(context, "refreshAllFamilyReadModels").mockResolvedValue(undefined);

    await context.hydrateFamilyReadModels();

    expect(context.familyReadModelsHydrated).toBe(true);
    expect(notify).not.toHaveBeenCalled();
    const onChanged = serviceMocks.updatesConnect.mock.calls[0]?.[1];
    expect(onChanged).toBeDefined();
    const onReconnected = serviceMocks.updatesConnect.mock.calls[0]?.[2];
    expect(onReconnected).toBeDefined();
    await onReconnected?.();
    expect(refreshAllFamilyReadModels).toHaveBeenCalledOnce();
    onChanged?.({
      aggregateType: "savings",
      familyId: dependencies.familyContext.activeFamilyId,
      revision: 2,
      updatedAt: "2026-09-10T08:00:00.000Z",
      updatedBy: createDomainId.familyMember("other-member"),
    });
    expect(refreshFamilyReadModel).toHaveBeenCalledWith("savings", 2);
    refreshFamilyReadModel.mockClear();
    onChanged?.({
      aggregateType: "savings",
      familyId: dependencies.familyContext.activeFamilyId,
      revision: 3,
      updatedAt: "2026-09-10T08:01:00.000Z",
      updatedBy: context.signedInMemberId,
    });
    expect(refreshFamilyReadModel).toHaveBeenCalledWith("savings", 3);
  });

  it("does not auto-open account setup for a viewer without the invitation capability", async () => {
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });
    const supporterId = createDomainId.familyMember("hannes");
    context.signedInMemberId = supporterId;
    context.members = context.members.map((member) =>
      member.id === supporterId
        ? {
            ...member,
            permissions: {
              canInviteMembers: false,
              canManageChildTasks: false,
              canManageContent: false,
              canManageFamily: false,
              canManageGoals: false,
              canSupportChildGoals: true,
              canReviewChildTasks: false,
              canViewFamilyGoals: true,
              canViewGuardianGoals: false,
              canViewHouse: true,
              canPurchaseHouseContentWithoutBalance: true,
              canManageShop: true,
              canReceivePushNotifications: true,
              canViewTaskAssignments: false,
            },
          }
        : member,
    );
    serviceMocks.familyBootstrapLoad.mockResolvedValue(createBootstrapReadModel(context));

    await context.hydrateFamilyReadModels();

    expect(context.onboardingCompleted).toBe(false);
    expect(context.canInviteFamilyMembers).toBe(false);
    expect(context.familySetupOpen).toBe(false);
  });
});

describe("hydrateHomeCustomization", () => {
  beforeEach(() => {
    serviceMocks.homeCustomizationLoad.mockReset();
  });

  function createHomeCustomizationContext() {
    const initialData = createFamilyWorldInitialData();
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });
    vi.spyOn(context, "notify").mockImplementation(() => undefined);

    return { context, initialData };
  }

  it("restores an unequipped default accessory", async () => {
    const { context, initialData } = createHomeCustomizationContext();
    serviceMocks.homeCustomizationLoad.mockResolvedValue({
      readModelType: "home-customization",
      revision: 1,
      state: {
        capabilities: { canArrangeHouse: true },
        catalog: {
          accessories: initialData.accessories.map((accessory) => ({
            available: true,
            catalogArea: accessory.category === "special" ? "special" : accessory.placement,
            id: accessory.id,
            minimumHouseLevel: accessory.minimumHouseLevel ?? 0,
            owned: accessory.owned,
            price: accessory.price,
            purchasableByMemberIds: [],
          })),
          areas: [],
          furnitureSets: [],
          houseStages: [],
          roomDesigns: [],
          themes: [],
        },
        accessories: initialData.accessories.map((accessory) => ({
          id: accessory.id,
          equipped: accessory.id === "round-rug" ? false : accessory.equipped,
          owned: accessory.owned,
        })),
        editions: HOUSE_THEMES.map((edition) => ({ id: edition.id, owned: edition.id === "sunny-dollhouse" })),
        placements: initialData.houseLayout,
        roomDesigns: ROOM_DESIGNS.map((design) => ({
          id: design.id,
          owned: design.houseThemeId === "sunny-dollhouse",
        })),
        selectedEditionId: "sunny-dollhouse",
        selectedRoomDesigns: ROOM_DESIGNS.filter((design) => design.houseThemeId === "sunny-dollhouse").map(
          (design) => ({
            designId: design.id,
            zoneId: design.zoneId,
          }),
        ),
      },
    });

    await context.hydrateHomeCustomization();

    expect(context.accessories.find((accessory) => accessory.id === "round-rug")?.equipped).toBe(false);
  });

  it("keeps persisted room selections instead of replacing them with theme defaults", async () => {
    const { context, initialData } = createHomeCustomizationContext();
    const ownedThemeIds = new Set(["sunny-dollhouse", "halloween-night"]);
    serviceMocks.homeCustomizationLoad.mockResolvedValue({
      readModelType: "home-customization",
      revision: 1,
      state: {
        capabilities: { canArrangeHouse: true },
        catalog: {
          accessories: initialData.accessories.map((accessory) => ({
            available: true,
            catalogArea: accessory.category === "special" ? "special" : accessory.placement,
            id: accessory.id,
            minimumHouseLevel: accessory.minimumHouseLevel ?? 0,
            owned: accessory.owned,
            price: accessory.price,
            purchasableByMemberIds: [],
          })),
          areas: [],
          furnitureSets: [],
          houseStages: [],
          roomDesigns: [],
          themes: [],
        },
        accessories: initialData.accessories.map((accessory) => ({
          id: accessory.id,
          equipped: accessory.equipped,
          owned: accessory.owned,
        })),
        editions: HOUSE_THEMES.map((edition) => ({ id: edition.id, owned: ownedThemeIds.has(edition.id) })),
        placements: initialData.houseLayout,
        roomDesigns: ROOM_DESIGNS.map((design) => ({ id: design.id, owned: ownedThemeIds.has(design.houseThemeId) })),
        selectedEditionId: "halloween-night",
        selectedRoomDesigns: [
          { designId: "garden-halloween-night", zoneId: "garden" },
          { designId: "living-ladi-classic", zoneId: "living-room" },
          { designId: "kitchen-halloween-night", zoneId: "kitchen" },
        ],
      },
    });

    await context.hydrateHomeCustomization();

    expect(context.selectedRoomDesignIds).toMatchObject({
      garden: "garden-halloween-night",
      "living-room": "living-ladi-classic",
      kitchen: "kitchen-halloween-night",
    });
  });
});

describe("refreshFamilyReadModel", () => {
  it("skips an event revision that is already applied", async () => {
    serviceMocks.contributionsLoad.mockReset();
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });
    context.readModelRevisions.contributions = 4;

    await context.refreshFamilyReadModel("contributions", 4);

    expect(serviceMocks.contributionsLoad).not.toHaveBeenCalled();
  });

  it("coalesces concurrent refreshes for the same read model", async () => {
    let completeLoad: (() => void) | undefined;
    serviceMocks.contributionsLoad.mockReset().mockImplementation(
      () =>
        new Promise((resolve) => {
          completeLoad = () => resolve(null);
        }),
    );
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });

    const firstRefresh = context.refreshFamilyReadModel("contributions", 2);
    const secondRefresh = context.refreshFamilyReadModel("contributions", 3);
    completeLoad?.();
    await Promise.all([firstRefresh, secondRefresh]);

    expect(serviceMocks.contributionsLoad).toHaveBeenCalledOnce();
  });

  it("reveals a newly approved contribution after a live update", async () => {
    const contributions = createContributions();
    serviceMocks.contributionsLoad.mockResolvedValue({
      readModelType: "contributions",
      revision: 4,
      state: {
        contributions,
        currentWeekApprovedContributionCountByMember: {},
        currentWeekEarnedByMember: {},
        perfectRatingBonusPercent: 5,
        promotions: [],
        todayEarnedByMember: {},
      },
    });
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });
    const revealNextContributionReward = vi
      .spyOn(context, "revealNextContributionReward")
      .mockImplementation(() => undefined);

    await context.refreshFamilyReadModel("contributions");

    expect(revealNextContributionReward).toHaveBeenCalledOnce();
  });

  it("reveals a newly received guardian gift after a live update", async () => {
    const initialData = createFamilyWorldInitialData();
    serviceMocks.savingsLoad.mockResolvedValue({
      readModelType: "savings",
      revision: 5,
      state: {
        availableBalances: initialData.balances,
        balances: initialData.balances,
        behaviorStatuses: {},
        familyCurrencyCode: "CHF",
        goals: initialData.goals,
        ladirchenPerCurrencyUnit: 1,
        pendingGuardianGifts: [],
        pendingBehaviorNotices: [],
        settledInterestPeriods: [],
        sharedSavedBalances: {},
        vouchers: [],
      },
    });
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });
    const revealNextGuardianGift = vi.spyOn(context, "revealNextGuardianGift").mockResolvedValue(undefined);

    await context.refreshFamilyReadModel("savings");

    expect(revealNextGuardianGift).toHaveBeenCalledOnce();
  });
});
