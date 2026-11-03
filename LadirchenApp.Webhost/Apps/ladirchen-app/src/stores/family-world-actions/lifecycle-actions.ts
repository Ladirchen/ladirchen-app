import type { HouseZoneId, RoomDesignId } from "@/domain/house";
import type { TranslationKey } from "@/locales/translation-keys";
import { normalizeFamilyMembers, normalizeFamilyPets } from "@/stores/family-world-state";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { FAMILY_READ_MODEL_TYPES, type FamilyReadModelType } from "@/application/contracts/family-read-model-contract";
import { createHouseAccessories } from "@/domain/house/catalog";
import type { FamilyMember } from "@/domain/family/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

const emptyMemberId = "00000000-0000-0000-0000-000000000000";
const readModelRefreshes = new WeakMap<object, Partial<Record<FamilyReadModelType, Promise<void>>>>();

function activeChildIdFrom(
  members: ReadonlyArray<FamilyMember>,
  currentMemberId: FamilyMemberId,
): FamilyMemberId | undefined {
  return (
    members.find((member) => member.id === currentMemberId && member.role === "child" && !member.invitationPending)
      ?.id ?? members.find((member) => member.role === "child" && !member.invitationPending)?.id
  );
}
function readModelViewerId(context: FamilyWorldStoreContext): FamilyMemberId | undefined {
  return context.signedInMemberId === emptyMemberId ? undefined : context.signedInMemberId;
}
export const lifecycleActions = {
  async hydrateFamilyReadModels(this: FamilyWorldStoreContext) {
    if (this.familyReadModelsHydrated) {
      return;
    }

    const { familyBootstrapService, familyContext } = this.$familyWorld;

    const familyId = familyContext.activeFamilyId;
    const viewerMemberId = readModelViewerId(this);

    try {
      const bootstrap = await familyBootstrapService.load(familyId, viewerMemberId);

      if (!bootstrap) {
        throw new Error("The family bootstrap read model is missing.");
      }

      const { contributions, familyProfile, familyProgression, homeCustomization, rewardShop, savings } = bootstrap;
      this.readModelRevisions = {
        contributions: contributions.revision,
        "family-profile": familyProfile.revision,
        "reward-shop": rewardShop.revision,
        savings: savings.revision,
      };

      if (familyProgression) {
        this.readModelRevisions["family-progression"] = familyProgression.revision;
      }

      if (homeCustomization) {
        this.readModelRevisions["home-customization"] = homeCustomization.revision;
      }

      this.members = normalizeFamilyMembers([...familyProfile.state.members]);
      this.pets = normalizeFamilyPets(familyProfile.state.pets);
      this.onboardingCompleted = familyProfile.state.onboardingCompleted;
      this.subscriptionTier = familyProfile.state.subscriptionTier;
      this.familyTimeZone = familyProfile.state.timeZone;
      this.avatarCatalog = familyProfile.state.avatarCatalog;
      this.contributions = [...contributions.state.contributions];
      this.perfectRatingBonusPercent = contributions.state.perfectRatingBonusPercent;
      this.promotions = [...contributions.state.promotions];
      this.todayEarnedByMember = { ...contributions.state.todayEarnedByMember };
      this.currentWeekEarnedByMember = { ...contributions.state.currentWeekEarnedByMember };
      this.currentWeekApprovedContributionCountByMember = {
        ...contributions.state.currentWeekApprovedContributionCountByMember,
      };
      this.balances = { ...savings.state.balances };
      this.availableBalances = { ...savings.state.availableBalances };
      this.behaviorStatuses = { ...savings.state.behaviorStatuses };
      this.familyCurrencyCode = savings.state.familyCurrencyCode;
      this.goals = [...savings.state.goals];
      this.ladirchenPerCurrencyUnit = savings.state.ladirchenPerCurrencyUnit;
      this.pendingGuardianGifts = [...savings.state.pendingGuardianGifts];
      this.pendingBehaviorNotices = [...savings.state.pendingBehaviorNotices];
      this.savingsVouchers = [...savings.state.vouchers];
      this.sharedSavedBalances = { ...savings.state.sharedSavedBalances };
      this.shopRewards = [...rewardShop.state.rewards];
      this.rewardShopRedemptionOpen = rewardShop.state.redemptionOpen;

      if (familyProgression) {
        this.completedWeeklyStreak = familyProgression.state.completedWeeklyStreak;
        this.currentWeekTarget = familyProgression.state.currentWeekTarget;
        this.houseLevel = familyProgression.state.houseLevel;
        this.memberProgressions = { ...familyProgression.state.memberProgressions };
        this.familyEnergyReadModel = familyProgression.state.familyEnergy;
        this.houseAtmosphere = familyProgression.state.houseAtmosphere;
        this.houseVisualLevel = familyProgression.state.houseVisualLevel;
        this.maxSavingsRatePercent = familyProgression.state.maximumSavingsInterestRate;
        this.canCloseWeekSuccessfully = familyProgression.state.canCloseWeekSuccessfully;
        this.minimumSuccessfulWeekEnergy = familyProgression.state.minimumSuccessfulWeekEnergy;
      }

      if (homeCustomization) {
        this.houseCatalog = homeCustomization.state.catalog;
        this.accessories = createHouseAccessories(
          homeCustomization.state.accessories,
          homeCustomization.state.catalog.accessories,
        );
        this.canArrangeHouseReadModel = homeCustomization.state.capabilities.canArrangeHouse;
        this.houseLayout = [...homeCustomization.state.placements];
        this.ownedHouseThemeIds = homeCustomization.state.editions
          .filter((edition) => edition.owned)
          .map((edition) => edition.id);
        this.houseThemeId = homeCustomization.state.selectedEditionId;
        this.ownedRoomDesignIds = homeCustomization.state.roomDesigns
          .filter((design) => design.owned)
          .map((design) => design.id);
        const selectedRoomDesignIds: Partial<Record<HouseZoneId, RoomDesignId>> = {};

        for (const selection of homeCustomization.state.selectedRoomDesigns) {
          selectedRoomDesignIds[selection.zoneId] = selection.designId;
        }

        this.selectedRoomDesignIds = selectedRoomDesignIds;
        this.homeCustomizationHydrated = true;
      }

      const activeChildId = activeChildIdFrom(this.members, this.activeChildId);

      if (activeChildId) {
        this.activeChildId = activeChildId;
      }

      this.familySetupOpen = !this.onboardingCompleted && this.canInviteFamilyMembers;
      this.familyReadModelsHydrated = true;
      this.revealNextContributionReward();
      this.revealNextGuardianGift();
      this.revealNextBehaviorNotice();
    } catch {
      this.familyReadModelsHydrated = false;
      this.notify("notifications.load.family");
    }

    try {
      await this.$familyWorld.familyAggregateUpdates.connect(
        familyId,
        (change) => {
          void this.refreshFamilyReadModel(change.aggregateType, change.revision);
        },
        async () => {
          await this.refreshAllFamilyReadModels();
        },
      );
    } catch {
      this.notify("notifications.load.family");
    }
  },
  async refreshAllFamilyReadModels(this: FamilyWorldStoreContext) {
    const externalGuardian = this.signedInMember?.guardianAccess === "external";
    const permittedReadModelTypes = externalGuardian
      ? FAMILY_READ_MODEL_TYPES.filter(
          (readModelType) => readModelType !== "family-progression" && readModelType !== "home-customization",
        )
      : FAMILY_READ_MODEL_TYPES;
    await Promise.all(permittedReadModelTypes.map((readModelType) => this.refreshFamilyReadModel(readModelType)));
  },
  async refreshFamilyReadModel(
    this: FamilyWorldStoreContext,
    readModelType: FamilyReadModelType,
    minimumRevision?: number,
  ) {
    if (minimumRevision !== undefined && (this.readModelRevisions[readModelType] ?? 0) >= minimumRevision) {
      return;
    }

    const currentRefreshes = readModelRefreshes.get(this) ?? {};
    const currentRefresh = currentRefreshes[readModelType];

    if (currentRefresh) {
      await currentRefresh;

      return;
    }

    const refresh = refreshFamilyReadModelSnapshot(this, readModelType);
    currentRefreshes[readModelType] = refresh;
    readModelRefreshes.set(this, currentRefreshes);

    try {
      await refresh;
    } finally {
      delete currentRefreshes[readModelType];
    }
  },
  async hydrateHomeCustomization(this: FamilyWorldStoreContext) {
    if (this.homeCustomizationHydrated) {
      return;
    }

    await this.refreshFamilyReadModel("home-customization");
  },
  notify(this: FamilyWorldStoreContext, messageKey: TranslationKey, params: Record<string, number | string> = {}) {
    this.snackbar.messageKey = messageKey;
    this.snackbar.params = params;
    this.snackbar.visible = true;
  },
} satisfies FamilyWorldActionGroup;

async function refreshFamilyReadModelSnapshot(
  context: FamilyWorldStoreContext,
  readModelType: FamilyReadModelType,
): Promise<void> {
  const {
    contributionsService,
    familyContext,
    familyProfileService,
    familyProgressionService,
    homeCustomizationService,
    rewardShopService,
    savingsService,
  } = context.$familyWorld;
  const familyId = familyContext.activeFamilyId;
  const viewerMemberId = readModelViewerId(context);

  try {
    if (readModelType === "family-profile") {
      const snapshot = await familyProfileService.load(familyId, viewerMemberId);

      if (snapshot) {
        context.members = normalizeFamilyMembers([...snapshot.state.members]);
        context.pets = normalizeFamilyPets(snapshot.state.pets);
        context.onboardingCompleted = snapshot.state.onboardingCompleted;
        context.subscriptionTier = snapshot.state.subscriptionTier;
        context.familyTimeZone = snapshot.state.timeZone;
        context.avatarCatalog = snapshot.state.avatarCatalog;
        context.readModelRevisions[readModelType] = snapshot.revision;
        const activeChildId = activeChildIdFrom(context.members, context.activeChildId);

        if (activeChildId) {
          context.activeChildId = activeChildId;
        }
      }

      return;
    }

    if (readModelType === "contributions") {
      const snapshot = await contributionsService.load(familyId, viewerMemberId);

      if (snapshot) {
        context.contributions = [...snapshot.state.contributions];
        context.perfectRatingBonusPercent = snapshot.state.perfectRatingBonusPercent;
        context.promotions = [...snapshot.state.promotions];
        context.todayEarnedByMember = { ...snapshot.state.todayEarnedByMember };
        context.currentWeekEarnedByMember = { ...snapshot.state.currentWeekEarnedByMember };
        context.currentWeekApprovedContributionCountByMember = {
          ...snapshot.state.currentWeekApprovedContributionCountByMember,
        };
        context.readModelRevisions[readModelType] = snapshot.revision;
        context.revealNextContributionReward();
      }

      return;
    }

    if (readModelType === "savings") {
      const snapshot = await savingsService.load(familyId, viewerMemberId);

      if (snapshot) {
        context.balances = { ...snapshot.state.balances };
        context.availableBalances = { ...snapshot.state.availableBalances };
        context.behaviorStatuses = { ...snapshot.state.behaviorStatuses };
        context.familyCurrencyCode = snapshot.state.familyCurrencyCode;
        context.goals = [...snapshot.state.goals];
        context.ladirchenPerCurrencyUnit = snapshot.state.ladirchenPerCurrencyUnit;
        context.pendingGuardianGifts = [...snapshot.state.pendingGuardianGifts];
        context.pendingBehaviorNotices = [...snapshot.state.pendingBehaviorNotices];
        context.savingsVouchers = [...snapshot.state.vouchers];
        context.sharedSavedBalances = { ...snapshot.state.sharedSavedBalances };
        context.readModelRevisions[readModelType] = snapshot.revision;
        context.revealNextGuardianGift();
        context.revealNextBehaviorNotice();
      }

      return;
    }

    if (readModelType === "reward-shop") {
      const snapshot = await rewardShopService.load(familyId, viewerMemberId);

      if (snapshot) {
        context.shopRewards = [...snapshot.state.rewards];
        context.rewardShopRedemptionOpen = snapshot.state.redemptionOpen;
        context.readModelRevisions[readModelType] = snapshot.revision;
      }

      return;
    }

    if (readModelType === "family-progression") {
      const snapshot = await familyProgressionService.load(familyId, viewerMemberId);

      if (snapshot) {
        context.completedWeeklyStreak = snapshot.state.completedWeeklyStreak;
        context.currentWeekTarget = snapshot.state.currentWeekTarget;
        context.houseLevel = snapshot.state.houseLevel;
        context.memberProgressions = { ...snapshot.state.memberProgressions };
        context.familyEnergyReadModel = snapshot.state.familyEnergy;
        context.houseAtmosphere = snapshot.state.houseAtmosphere;
        context.houseVisualLevel = snapshot.state.houseVisualLevel;
        context.maxSavingsRatePercent = snapshot.state.maximumSavingsInterestRate;
        context.canCloseWeekSuccessfully = snapshot.state.canCloseWeekSuccessfully;
        context.minimumSuccessfulWeekEnergy = snapshot.state.minimumSuccessfulWeekEnergy;
        context.readModelRevisions[readModelType] = snapshot.revision;
      }

      context.homeCustomizationHydrated = false;

      return;
    }

    context.homeCustomizationHydrated = false;
    const snapshot = await homeCustomizationService.load(familyId, viewerMemberId);

    if (snapshot) {
      context.houseCatalog = snapshot.state.catalog;
      context.accessories = createHouseAccessories(snapshot.state.accessories, snapshot.state.catalog.accessories);
      context.canArrangeHouseReadModel = snapshot.state.capabilities.canArrangeHouse;
      context.houseLayout = [...snapshot.state.placements];
      context.ownedHouseThemeIds = snapshot.state.editions
        .filter((edition) => edition.owned)
        .map((edition) => edition.id);
      context.houseThemeId = snapshot.state.selectedEditionId;
      context.ownedRoomDesignIds = snapshot.state.roomDesigns
        .filter((design) => design.owned)
        .map((design) => design.id);
      const selectedRoomDesignIds: Partial<Record<HouseZoneId, RoomDesignId>> = {};

      for (const selection of snapshot.state.selectedRoomDesigns) {
        selectedRoomDesignIds[selection.zoneId] = selection.designId;
      }

      context.selectedRoomDesignIds = selectedRoomDesignIds;
      context.readModelRevisions[readModelType] = snapshot.revision;
      context.homeCustomizationHydrated = true;
    }
  } catch {
    context.notify(readModelType === "home-customization" ? "notifications.load.home" : "notifications.load.family");
  }
}
