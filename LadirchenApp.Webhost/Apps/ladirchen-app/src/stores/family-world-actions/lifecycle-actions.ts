import type { HouseZoneId, RoomDesignId } from "@/domain/house";
import type { TranslationKey } from "@/locales/translation-keys";
import { AUTH_STATE_KEY } from "./family-world-store-utils";
import { mergeHouseLayout, normalizeFamilyMembers, normalizeFamilyPets } from "@/stores/family-world-state";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import type { FamilyAggregateType } from "@/application/contracts/versioned-aggregate-contract";
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
export const lifecycleActions = {
  async hydrateFamilyAggregates(this: FamilyWorldStoreContext) {
    if (this.familyAggregatesHydrated) {
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
    const [profileResult, contributionsResult, savingsResult, rewardShopResult, progressionResult] =
      await Promise.allSettled([
        familyProfileService.load(familyId),
        contributionsService.load(familyId),
        savingsService.load(familyId),
        rewardShopService.load(familyId),
        familyProgressionService.load(familyId),
      ]);
    const profileLoaded = applySettledLoad(profileResult, (profile) => {
      if (profile) {
        this.members = normalizeFamilyMembers([...profile.state.members]);
        this.pets = normalizeFamilyPets(profile.state.pets);
        this.onboardingCompleted = profile.state.onboardingCompleted;
        this.subscriptionTier = profile.state.subscriptionTier;
        this.familyTimeZone = profile.state.timeZone;
      }
    });
    const contributionsLoaded = applySettledLoad(contributionsResult, (contributions) => {
      if (contributions) {
        this.contributions = [...contributions.state.contributions];
        this.promotions = [...contributions.state.promotions];
      }
    });
    const savingsLoaded = applySettledLoad(savingsResult, (savings) => {
      if (savings) {
        this.balances = { ...savings.state.balances };
        this.familyCurrencyCode = savings.state.familyCurrencyCode;
        this.goals = [...savings.state.goals];
        this.ladirchenPerCurrencyUnit = savings.state.ladirchenPerCurrencyUnit;
        this.pendingGuardianGifts = [...savings.state.pendingGuardianGifts];
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
        this.houseVisualLevel = progression.state.houseVisualLevel;
        this.maxSavingsRatePercent = progression.state.maximumSavingsInterestRate;
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
    this.familyAggregatesHydrated = hydrationSucceeded;

    if (hydrationSucceeded && progressionResult.status === "fulfilled" && progressionResult.value !== null) {
      try {
        await this.$familyWorld.familyCommands.execute(familyId, this.signedInMemberId, "progression.refresh", {});
        await this.refreshFamilyAggregate("family-progression");
      } catch {
        this.notify("notifications.load.family");
      }
    }

    if (!hydrationSucceeded) {
      this.notify("notifications.load.family");
    }

    await this.hydrateHomeCustomization();

    try {
      await this.$familyWorld.familyAggregateUpdates.connect(familyId, (change) => {
        if (change.updatedBy !== this.signedInMemberId) {
          void this.refreshFamilyAggregate(change.aggregateType);
        }
      });
    } catch {
      this.notify("notifications.load.family");
    }
  },
  async refreshFamilyAggregate(this: FamilyWorldStoreContext, aggregateType: FamilyAggregateType) {
    const {
      contributionsService,
      familyContext,
      familyProfileService,
      familyProgressionService,
      rewardShopService,
      savingsService,
    } = this.$familyWorld;
    const familyId = familyContext.activeFamilyId;

    try {
      if (aggregateType === "family-profile") {
        const snapshot = await familyProfileService.load(familyId);

        if (snapshot) {
          this.members = normalizeFamilyMembers([...snapshot.state.members]);
          this.pets = normalizeFamilyPets(snapshot.state.pets);
          this.onboardingCompleted = snapshot.state.onboardingCompleted;
          this.subscriptionTier = snapshot.state.subscriptionTier;
          this.familyTimeZone = snapshot.state.timeZone;
        }

        return;
      }

      if (aggregateType === "contributions") {
        const snapshot = await contributionsService.load(familyId);

        if (snapshot) {
          this.contributions = [...snapshot.state.contributions];
          this.promotions = [...snapshot.state.promotions];
        }

        return;
      }

      if (aggregateType === "savings") {
        const snapshot = await savingsService.load(familyId);

        if (snapshot) {
          this.balances = { ...snapshot.state.balances };
          this.familyCurrencyCode = snapshot.state.familyCurrencyCode;
          this.goals = [...snapshot.state.goals];
          this.ladirchenPerCurrencyUnit = snapshot.state.ladirchenPerCurrencyUnit;
          this.pendingGuardianGifts = [...snapshot.state.pendingGuardianGifts];
        }

        return;
      }

      if (aggregateType === "reward-shop") {
        const snapshot = await rewardShopService.load(familyId);

        if (snapshot) {
          this.shopRewards = [...snapshot.state.rewards];
          this.rewardShopRedemptionOpen = snapshot.state.redemptionOpen ?? false;
        }

        return;
      }

      if (aggregateType === "family-progression") {
        const snapshot = await familyProgressionService.load(familyId);

        if (snapshot) {
          this.completedWeeklyStreak = snapshot.state.completedWeeklyStreak;
          this.currentWeekTarget = snapshot.state.currentWeekTarget;
          this.houseLevel = snapshot.state.houseLevel;
          this.memberProgressions = { ...snapshot.state.memberProgressions };
          this.familyEnergyReadModel = snapshot.state.familyEnergy;
          this.houseVisualLevel = snapshot.state.houseVisualLevel;
          this.maxSavingsRatePercent = snapshot.state.maximumSavingsInterestRate;
        }

        return;
      }

      this.homeCustomizationHydrated = false;
      await this.hydrateHomeCustomization();
    } catch {
      this.notify(aggregateType === "home-customization" ? "notifications.load.home" : "notifications.load.family");
    }
  },
  async hydrateHomeCustomization(this: FamilyWorldStoreContext) {
    if (this.homeCustomizationHydrated) {
      return;
    }

    const { familyContext, homeCustomizationService } = this.$familyWorld;

    try {
      const snapshot = await homeCustomizationService.load(familyContext.activeFamilyId);

      if (snapshot) {
        this.houseCatalog = snapshot.state.catalog;
        const accessoryStateById = new Map(snapshot.state.accessories.map((item) => [item.id, item]));
        const initialData = this.$familyWorld.initialDataFactory.create();
        this.accessories = initialData.accessories.map(function callback(accessory) {
          const saved = accessoryStateById.get(accessory.id);

          return saved ? { ...accessory, owned: saved.owned, equipped: saved.equipped } : accessory;
        });
        this.houseLayout = mergeHouseLayout(snapshot.state.placements, initialData);
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
