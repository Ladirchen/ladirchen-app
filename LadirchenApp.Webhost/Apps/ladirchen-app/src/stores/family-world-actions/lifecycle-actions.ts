import type { HouseZoneId, RoomDesignId } from "@/domain/house";
import type { TranslationKey } from "@/locales/translation-keys";
import { AUTH_STATE_KEY } from "./family-world-store-utils";
import { normalizeFamilyMembers, normalizeFamilyPets } from "@/stores/family-world-state";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { FAMILY_READ_MODEL_TYPES, type FamilyReadModelType } from "@/application/contracts/family-read-model-contract";
import { createHouseAccessories } from "@/domain/house/catalog";
import type { FamilyMemberId } from "@/domain/shared/identifiers";
const emptyMemberId = "00000000-0000-0000-0000-000000000000";
function applySettledLoad<T>(result: PromiseSettledResult<T>, apply: (value: T) => void): boolean {
  if (result.status === "rejected") {
    return false;
  }

  try {
    apply(result.value);

    return true;
  } catch {
    return false;
  }
}
function readModelViewerId(context: FamilyWorldStoreContext): FamilyMemberId | undefined {
  return context.signedInMemberId === emptyMemberId ? undefined : context.signedInMemberId;
}
export const lifecycleActions = {
  async hydrateFamilyReadModels(this: FamilyWorldStoreContext) {
    if (this.familyReadModelsHydrated) {
      return;
    }

    const {
      contributionsService,
      familyContext,
      familyProfileService,
      familyProgressionService,
      rewardShopService,
      savingsService,
    } = this.$familyWorld;

    if (this.$familyWorld.clientStorage.getItem(AUTH_STATE_KEY) === "signed-out") {
      this.isAuthenticated = false;

      return;
    }

    const familyId = familyContext.activeFamilyId;
    const viewerMemberId = readModelViewerId(this);
    const [profileResult, contributionsResult, savingsResult, rewardShopResult, progressionResult] =
      await Promise.allSettled([
        familyProfileService.load(familyId, viewerMemberId),
        contributionsService.load(familyId, viewerMemberId),
        savingsService.load(familyId, viewerMemberId),
        rewardShopService.load(familyId, viewerMemberId),
        familyProgressionService.load(familyId, viewerMemberId),
      ]);
    const profileLoaded = applySettledLoad(profileResult, (profile) => {
      if (profile) {
        this.members = normalizeFamilyMembers([...profile.state.members]);
        this.pets = normalizeFamilyPets(profile.state.pets);
        this.onboardingCompleted = profile.state.onboardingCompleted;
        this.subscriptionTier = profile.state.subscriptionTier;
        this.familyTimeZone = profile.state.timeZone;
        this.avatarCatalog = profile.state.avatarCatalog;
      }
    });
    const contributionsLoaded = applySettledLoad(contributionsResult, (contributions) => {
      if (contributions) {
        this.contributions = [...contributions.state.contributions];
        this.perfectRatingBonusPercent =
          contributions.state.perfectRatingBonusPercent ?? this.perfectRatingBonusPercent;
        this.promotions = [...contributions.state.promotions];
        this.todayEarnedByMember = { ...contributions.state.todayEarnedByMember };
        this.currentWeekEarnedByMember = { ...contributions.state.currentWeekEarnedByMember };
        this.currentWeekApprovedContributionCountByMember = {
          ...contributions.state.currentWeekApprovedContributionCountByMember,
        };
      }
    });
    const savingsLoaded = applySettledLoad(savingsResult, (savings) => {
      if (savings) {
        this.balances = { ...savings.state.balances };
        this.availableBalances = { ...savings.state.availableBalances };
        this.familyCurrencyCode = savings.state.familyCurrencyCode;
        this.goals = [...savings.state.goals];
        this.ladirchenPerCurrencyUnit = savings.state.ladirchenPerCurrencyUnit;
        this.pendingGuardianGifts = [...savings.state.pendingGuardianGifts];
        this.savingsVouchers = [...savings.state.vouchers];
        this.sharedSavedBalances = { ...savings.state.sharedSavedBalances };
      }
    });
    const rewardShopLoaded = applySettledLoad(rewardShopResult, (rewardShop) => {
      if (rewardShop) {
        this.shopRewards = [...rewardShop.state.rewards];
        this.rewardShopRedemptionOpen = rewardShop.state.redemptionOpen ?? false;
      }
    });
    const progressionLoaded = applySettledLoad(progressionResult, (progression) => {
      if (progression) {
        this.completedWeeklyStreak = progression.state.completedWeeklyStreak;
        this.currentWeekTarget = progression.state.currentWeekTarget;
        this.houseLevel = progression.state.houseLevel;
        this.memberProgressions = { ...progression.state.memberProgressions };
        this.familyEnergyReadModel = progression.state.familyEnergy;
        this.houseAtmosphere = progression.state.houseAtmosphere;
        this.houseVisualLevel = progression.state.houseVisualLevel;
        this.maxSavingsRatePercent = progression.state.maximumSavingsInterestRate;
        this.canCloseWeekSuccessfully = progression.state.canCloseWeekSuccessfully;
        this.minimumSuccessfulWeekEnergy = progression.state.minimumSuccessfulWeekEnergy;
      }
    });

    if (profileLoaded) {
      const activeChild =
        this.members.find((member) => member.id === this.activeChildId && member.role === "child") ??
        this.members.find((member) => member.role === "child");

      if (activeChild) {
        this.activeChildId = activeChild.id;
      }

      this.familySetupOpen = !this.onboardingCompleted;
    }

    const hydrationSucceeded =
      profileLoaded && contributionsLoaded && savingsLoaded && rewardShopLoaded && progressionLoaded;
    this.familyReadModelsHydrated = hydrationSucceeded;

    if (!hydrationSucceeded) {
      this.notify("notifications.load.family");
    }

    await this.hydrateHomeCustomization();

    try {
      await this.$familyWorld.familyAggregateUpdates.connect(
        familyId,
        (change) => {
          if (change.updatedBy !== this.signedInMemberId) {
            void this.refreshFamilyReadModel(change.aggregateType);
          }
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
    for (const readModelType of FAMILY_READ_MODEL_TYPES) {
      await this.refreshFamilyReadModel(readModelType);
    }
  },
  async refreshFamilyReadModel(this: FamilyWorldStoreContext, readModelType: FamilyReadModelType) {
    const {
      contributionsService,
      familyContext,
      familyProfileService,
      familyProgressionService,
      rewardShopService,
      savingsService,
    } = this.$familyWorld;
    const familyId = familyContext.activeFamilyId;
    const viewerMemberId = readModelViewerId(this);

    try {
      if (readModelType === "family-profile") {
        const snapshot = await familyProfileService.load(familyId, viewerMemberId);

        if (snapshot) {
          this.members = normalizeFamilyMembers([...snapshot.state.members]);
          this.pets = normalizeFamilyPets(snapshot.state.pets);
          this.onboardingCompleted = snapshot.state.onboardingCompleted;
          this.subscriptionTier = snapshot.state.subscriptionTier;
          this.familyTimeZone = snapshot.state.timeZone;
          this.avatarCatalog = snapshot.state.avatarCatalog;
        }

        return;
      }

      if (readModelType === "contributions") {
        const snapshot = await contributionsService.load(familyId, viewerMemberId);

        if (snapshot) {
          this.contributions = [...snapshot.state.contributions];
          this.perfectRatingBonusPercent = snapshot.state.perfectRatingBonusPercent ?? this.perfectRatingBonusPercent;
          this.promotions = [...snapshot.state.promotions];
          this.todayEarnedByMember = { ...snapshot.state.todayEarnedByMember };
          this.currentWeekEarnedByMember = { ...snapshot.state.currentWeekEarnedByMember };
          this.currentWeekApprovedContributionCountByMember = {
            ...snapshot.state.currentWeekApprovedContributionCountByMember,
          };
        }

        return;
      }

      if (readModelType === "savings") {
        const snapshot = await savingsService.load(familyId, viewerMemberId);

        if (snapshot) {
          this.balances = { ...snapshot.state.balances };
          this.availableBalances = { ...snapshot.state.availableBalances };
          this.familyCurrencyCode = snapshot.state.familyCurrencyCode;
          this.goals = [...snapshot.state.goals];
          this.ladirchenPerCurrencyUnit = snapshot.state.ladirchenPerCurrencyUnit;
          this.pendingGuardianGifts = [...snapshot.state.pendingGuardianGifts];
          this.savingsVouchers = [...snapshot.state.vouchers];
          this.sharedSavedBalances = { ...snapshot.state.sharedSavedBalances };
        }

        return;
      }

      if (readModelType === "reward-shop") {
        const [snapshot, savingsSnapshot] = await Promise.all([
          rewardShopService.load(familyId, viewerMemberId),
          savingsService.load(familyId, viewerMemberId),
        ]);

        if (snapshot) {
          this.shopRewards = [...snapshot.state.rewards];
          this.rewardShopRedemptionOpen = snapshot.state.redemptionOpen ?? false;
        }

        if (savingsSnapshot) {
          this.balances = { ...savingsSnapshot.state.balances };
          this.availableBalances = { ...savingsSnapshot.state.availableBalances };
          this.goals = [...savingsSnapshot.state.goals];
          this.pendingGuardianGifts = [...savingsSnapshot.state.pendingGuardianGifts];
          this.savingsVouchers = [...savingsSnapshot.state.vouchers];
          this.sharedSavedBalances = { ...savingsSnapshot.state.sharedSavedBalances };
        }

        return;
      }

      if (readModelType === "family-progression") {
        const [snapshot, savingsSnapshot] = await Promise.all([
          familyProgressionService.load(familyId, viewerMemberId),
          savingsService.load(familyId, viewerMemberId),
        ]);

        if (snapshot) {
          this.completedWeeklyStreak = snapshot.state.completedWeeklyStreak;
          this.currentWeekTarget = snapshot.state.currentWeekTarget;
          this.houseLevel = snapshot.state.houseLevel;
          this.memberProgressions = { ...snapshot.state.memberProgressions };
          this.familyEnergyReadModel = snapshot.state.familyEnergy;
          this.houseAtmosphere = snapshot.state.houseAtmosphere;
          this.houseVisualLevel = snapshot.state.houseVisualLevel;
          this.maxSavingsRatePercent = snapshot.state.maximumSavingsInterestRate;
          this.canCloseWeekSuccessfully = snapshot.state.canCloseWeekSuccessfully;
          this.minimumSuccessfulWeekEnergy = snapshot.state.minimumSuccessfulWeekEnergy;
        }

        if (savingsSnapshot) {
          this.balances = { ...savingsSnapshot.state.balances };
          this.availableBalances = { ...savingsSnapshot.state.availableBalances };
          this.goals = [...savingsSnapshot.state.goals];
          this.pendingGuardianGifts = [...savingsSnapshot.state.pendingGuardianGifts];
          this.savingsVouchers = [...savingsSnapshot.state.vouchers];
          this.sharedSavedBalances = { ...savingsSnapshot.state.sharedSavedBalances };
        }

        this.homeCustomizationHydrated = false;
        await this.hydrateHomeCustomization();

        return;
      }

      this.homeCustomizationHydrated = false;
      await this.hydrateHomeCustomization();
    } catch {
      this.notify(readModelType === "home-customization" ? "notifications.load.home" : "notifications.load.family");
    }
  },
  async hydrateHomeCustomization(this: FamilyWorldStoreContext) {
    if (this.homeCustomizationHydrated) {
      return;
    }

    const { familyContext, homeCustomizationService } = this.$familyWorld;

    try {
      const snapshot = await homeCustomizationService.load(familyContext.activeFamilyId, readModelViewerId(this));

      if (snapshot) {
        this.houseCatalog = snapshot.state.catalog;
        this.accessories = createHouseAccessories(snapshot.state.accessories, snapshot.state.catalog.accessories);
        this.canArrangeHouseReadModel = snapshot.state.capabilities.canArrangeHouse;
        this.houseLayout = [...snapshot.state.placements];
        this.ownedHouseThemeIds = snapshot.state.editions
          .filter((edition) => edition.owned)
          .map((edition) => edition.id);
        this.houseThemeId = snapshot.state.selectedEditionId;
        this.ownedRoomDesignIds = snapshot.state.roomDesigns
          .filter((design) => design.owned)
          .map((design) => design.id);
        const selectedRoomDesignIds: Partial<Record<HouseZoneId, RoomDesignId>> = {};

        for (const selection of snapshot.state.selectedRoomDesigns) {
          selectedRoomDesignIds[selection.zoneId] = selection.designId;
        }

        this.selectedRoomDesignIds = selectedRoomDesignIds;
      }

      this.homeCustomizationHydrated = true;
    } catch {
      this.homeCustomizationHydrated = false;
      this.notify("notifications.load.home");
    }
  },
  notify(this: FamilyWorldStoreContext, messageKey: TranslationKey, params: Record<string, number | string> = {}) {
    this.snackbar.messageKey = messageKey;
    this.snackbar.params = params;
    this.snackbar.visible = true;
  },
} satisfies FamilyWorldActionGroup;
