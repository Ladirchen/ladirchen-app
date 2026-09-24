import { DEFAULT_ROOM_DESIGNS, HOUSE_THEMES, ROOM_DESIGNS, roomDesignsForTheme } from "@/domain/house";
import type { HouseZoneId, RoomDesignId } from "@/domain/house";
import type { TranslationKey } from "@/locales/translation-keys";
import { AUTH_STATE_KEY } from "./family-world-store-utils";
import { mergeHouseLayout, normalizeFamilyMembers, normalizeFamilyPets } from "@/stores/family-world-state";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";

const applySettledLoad = <T>(
  result: PromiseSettledResult<T>,
  apply: (value: T) => void,
): boolean => {
  if (result.status === "rejected") {return false;}
  try {
    apply(result.value);
    return true;
  } catch {
    return false;
  }
};

export const lifecycleActions = {
  async hydrateFamilyAggregates(this: FamilyWorldStoreContext) {
    if (this.familyAggregatesHydrated) {return;}
    const { contributionsService, familyContext, familyProfileService, familyProgressionService, rewardShopService, savingsService } = this.$familyWorld;
    if (this.$familyWorld.clientStorage.getItem(AUTH_STATE_KEY) === "signed-out") {
      this.isAuthenticated = false;
      return;
    }
    const familyId = familyContext.activeFamilyId;
    const [profileResult, contributionsResult, savingsResult, rewardShopResult, progressionResult] = await Promise.allSettled([
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
        let contributionRewardsMigrated = false;
        this.contributions.forEach((contribution) => {
          if (contribution.status !== "approved" || contribution.earnedReward !== undefined) {return;}
          const promotion = this.promotions.find(item => item.active && item.contributionId === contribution.id);
          contribution.earnedReward = contribution.reward * (promotion?.multiplier ?? 1) + (promotion?.teamworkBonus ?? 0);
          contribution.earnedRatingBonus = 0;
          contribution.earnedPromotionMultiplier = promotion?.multiplier;
          contribution.rewardCelebrated = !promotion;
          contributionRewardsMigrated = true;
        });
        if (contributionRewardsMigrated) {this.persistContributions();}
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
      }
    });
    const progressionLoaded = applySettledLoad(progressionResult, (progression) => {
      if (progression) {
        this.completedWeeklyStreak = progression.state.completedWeeklyStreak;
        this.currentWeekTarget = progression.state.currentWeekTarget;
        this.houseLevel = progression.state.houseLevel;
      }
    });
    if (profileLoaded) {
      const activeChild = this.members.find(member => member.id === this.activeChildId && member.role === "child") ??
        this.members.find(member => member.role === "child");
      if (activeChild) {
        this.activeChildId = activeChild.id;
      }
      this.familySetupOpen = !this.onboardingCompleted;
    }
    const hydrationSucceeded = profileLoaded && contributionsLoaded && savingsLoaded && rewardShopLoaded && progressionLoaded;
    this.familyAggregatesHydrated = hydrationSucceeded;
    if (!hydrationSucceeded) {this.notify("notifications.load.family");}
    await this.hydrateHomeCustomization();
  },
  async hydrateHomeCustomization(this: FamilyWorldStoreContext) {
    if (this.homeCustomizationHydrated) {return;}
    const { familyContext, homeCustomizationService } = this.$familyWorld;
    try {
      const snapshot = await homeCustomizationService.load(familyContext.activeFamilyId);
      if (snapshot) {
        const accessoryStateById = new Map(snapshot.state.accessories.map(item => [item.id, item]));
        const initialData = this.$familyWorld.initialDataFactory.create();
        this.accessories = initialData.accessories.map((accessory) => {
          const saved = accessoryStateById.get(accessory.id);
          return saved
            ? { ...accessory, owned: accessory.owned || saved.owned, equipped: saved.equipped }
            : accessory;
        });
        this.houseLayout = mergeHouseLayout(snapshot.state.placements, initialData);
        this.ownedHouseThemeIds = Array.from(new Set([
          ...HOUSE_THEMES.filter(theme => theme.ownedByDefault).map(theme => theme.id),
          ...snapshot.state.editions.filter(edition => edition.owned).map(edition => edition.id),
        ]));
        this.houseThemeId = this.ownedHouseThemeIds.includes(snapshot.state.selectedEditionId)
          ? snapshot.state.selectedEditionId
          : "sunny-dollhouse";
        this.ownedRoomDesignIds = Array.from(new Set([
          ...DEFAULT_ROOM_DESIGNS.map(design => design.id),
          ...this.ownedHouseThemeIds.flatMap(themeId => roomDesignsForTheme(themeId).map(design => design.id)),
          ...snapshot.state.roomDesigns.filter(design => design.owned).map(design => design.id),
        ]));
        const selectedRoomDesignIds: Partial<Record<HouseZoneId, RoomDesignId>> = {};
        for (const design of DEFAULT_ROOM_DESIGNS) {
          if (this.ownedRoomDesignIds.includes(design.id)) { selectedRoomDesignIds[design.zoneId] = design.id; }
        }
        for (const design of roomDesignsForTheme(this.houseThemeId)) {
          selectedRoomDesignIds[design.zoneId] = design.id;
        }
        for (const selection of snapshot.state.selectedRoomDesigns) {
          if (this.ownedRoomDesignIds.includes(selection.designId)) { selectedRoomDesignIds[selection.zoneId] = selection.designId; }
        }
        this.selectedRoomDesignIds = selectedRoomDesignIds;
      }
      this.homeCustomizationHydrated = true;
    } catch {
      this.homeCustomizationHydrated = false;
      this.notify("notifications.load.home");
    }
  },
  persistHomeCustomization(this: FamilyWorldStoreContext) {
    const { familyContext, homeCustomizationService } = this.$familyWorld;
    homeCustomizationService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      updatedBy: this.signedInMemberId,
      state: {
        accessories: this.accessories.map(accessory => ({
          id: accessory.id,
          equipped: accessory.equipped,
          owned: accessory.owned,
        })),
        editions: HOUSE_THEMES.map(edition => ({
          id: edition.id,
          owned: this.ownedHouseThemeIds.includes(edition.id),
        })),
        placements: this.houseLayout,
        roomDesigns: ROOM_DESIGNS.map(design => ({
          id: design.id,
          owned: this.ownedRoomDesignIds.includes(design.id),
        })),
        selectedRoomDesigns: ROOM_DESIGNS
          .filter(design => this.selectedRoomDesignIds[design.zoneId] === design.id)
          .map(design => ({ zoneId: design.zoneId, designId: design.id })),
        selectedEditionId: this.houseThemeId,
      },
    }, () => this.notify("notifications.save.home"));
  },
  persistFamilyProfile(this: FamilyWorldStoreContext) {
    const { familyContext, familyProfileService } = this.$familyWorld;
    familyProfileService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      state: { members: this.members, onboardingCompleted: this.onboardingCompleted, pets: this.pets, subscriptionTier: this.subscriptionTier, timeZone: this.familyTimeZone },
      updatedBy: this.signedInMemberId,
    }, () => this.notify("notifications.save.profile"));
  },
  persistContributions(this: FamilyWorldStoreContext) {
    const { contributionsService, familyContext } = this.$familyWorld;
    contributionsService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      state: { contributions: this.contributions, promotions: this.promotions },
      updatedBy: this.signedInMemberId,
    }, () => this.notify("notifications.save.contributions"));
  },
  persistSavings(this: FamilyWorldStoreContext) {
    const { familyContext, savingsService } = this.$familyWorld;
    savingsService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      state: {
        balances: this.balances,
        familyCurrencyCode: this.familyCurrencyCode,
        goals: this.goals,
        ladirchenPerCurrencyUnit: this.ladirchenPerCurrencyUnit,
        pendingGuardianGifts: this.pendingGuardianGifts,
      },
      updatedBy: this.signedInMemberId,
    }, () => this.notify("notifications.save.savings"));
  },
  persistRewardShop(this: FamilyWorldStoreContext) {
    const { familyContext, rewardShopService } = this.$familyWorld;
    rewardShopService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      state: { rewards: this.shopRewards },
      updatedBy: this.signedInMemberId,
    }, () => this.notify("notifications.save.shop"));
  },
  persistFamilyProgression(this: FamilyWorldStoreContext) {
    const { familyContext, familyProgressionService } = this.$familyWorld;
    familyProgressionService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      state: {
        completedWeeklyStreak: this.completedWeeklyStreak,
        currentWeekTarget: this.currentWeekTarget,
        houseLevel: this.houseLevel,
      },
      updatedBy: this.signedInMemberId,
    }, () => this.notify("notifications.save.progression"));
  },
  notify(this: FamilyWorldStoreContext, messageKey: TranslationKey, params: Record<string, number | string> = {}) {
    this.snackbar.messageKey = messageKey;
    this.snackbar.params = params;
    this.snackbar.visible = true;
  },
} satisfies FamilyWorldActionGroup;
