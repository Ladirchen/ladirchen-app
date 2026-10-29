import { beforeEach, describe, expect, it, vi } from "vite-plus/test";

import { createContributions, createFamilyWorldInitialData } from "@/infrastructure/fixtures/family-world-fixtures";
import { HOUSE_THEMES, ROOM_DESIGNS } from "@/domain/house";
import type { FamilyWorldDependencies } from "@/application/ports/family-world-dependencies";
import { createDomainId } from "@/domain/shared/identifiers";
import { createFamilyWorldTestDependencies, createFamilyWorldTestStore } from "@/stores/family-world-test-support";

const serviceMocks = vi.hoisted(() => ({
  contributionsLoad: vi.fn<FamilyWorldDependencies["contributionsService"]["load"]>(),
  familyProfileLoad: vi.fn<FamilyWorldDependencies["familyProfileService"]["load"]>(),
  familyProgressionLoad: vi.fn<FamilyWorldDependencies["familyProgressionService"]["load"]>(),
  homeCustomizationLoad: vi.fn<FamilyWorldDependencies["homeCustomizationService"]["load"]>(),
  updatesConnect: vi.fn<FamilyWorldDependencies["familyAggregateUpdates"]["connect"]>(),
  rewardShopLoad: vi.fn<FamilyWorldDependencies["rewardShopService"]["load"]>(),
  savingsLoad: vi.fn<FamilyWorldDependencies["savingsService"]["load"]>(),
}));

const dependencies = createFamilyWorldTestDependencies({
  clientStorage: { getItem: vi.fn(() => null), removeItem: vi.fn(), setItem: vi.fn() },
  contributionsService: { load: serviceMocks.contributionsLoad },
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

describe("hydrateFamilyReadModels", () => {
  beforeEach(() => {
    serviceMocks.contributionsLoad.mockReset().mockResolvedValue(null);
    serviceMocks.familyProfileLoad.mockReset().mockResolvedValue(null);
    serviceMocks.familyProgressionLoad.mockReset().mockResolvedValue(null);
    serviceMocks.homeCustomizationLoad.mockReset().mockResolvedValue(null);
    serviceMocks.rewardShopLoad.mockReset().mockResolvedValue(null);
    serviceMocks.savingsLoad.mockReset().mockResolvedValue(null);
    serviceMocks.updatesConnect.mockReset().mockResolvedValue(undefined);
  });

  it("applies successful read-model loads and remains retryable after a partial failure", async () => {
    const contributions = createContributions();
    const firstContribution = contributions[0]!;
    firstContribution.title = "Loaded contribution";
    serviceMocks.contributionsLoad.mockResolvedValue({
      readModelType: "contributions",
      revision: 3,
      state: {
        contributions,
        currentWeekApprovedContributionCountByMember: {},
        currentWeekEarnedByMember: {},
        perfectRatingBonusPercent: 15,
        promotions: [],
        todayEarnedByMember: {},
      },
    });
    serviceMocks.familyProfileLoad.mockRejectedValue(new Error("Profile endpoint unavailable"));
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });
    vi.spyOn(context, "hydrateHomeCustomization").mockResolvedValue(undefined);
    const notify = vi.spyOn(context, "notify").mockImplementation(() => undefined);

    await context.hydrateFamilyReadModels();

    expect(context.contributions[0]?.title).toBe("Loaded contribution");
    expect(serviceMocks.familyProfileLoad).toHaveBeenCalledWith(
      dependencies.familyContext.activeFamilyId,
      context.signedInMemberId,
    );
    expect(context.familyReadModelsHydrated).toBe(false);
    expect(notify).toHaveBeenCalledWith("notifications.load.family");
    expect(context.hydrateHomeCustomization).toHaveBeenCalledOnce();
  });

  it("marks hydration complete only when every aggregate load succeeds", async () => {
    const context = createFamilyWorldTestStore(dependencies, { create: createFamilyWorldInitialData });
    vi.spyOn(context, "hydrateHomeCustomization").mockResolvedValue(undefined);
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
    expect(refreshFamilyReadModel).toHaveBeenCalledWith("savings");
    refreshFamilyReadModel.mockClear();
    onChanged?.({
      aggregateType: "savings",
      familyId: dependencies.familyContext.activeFamilyId,
      revision: 3,
      updatedAt: "2026-09-10T08:01:00.000Z",
      updatedBy: context.signedInMemberId,
    });
    expect(refreshFamilyReadModel).not.toHaveBeenCalled();
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
