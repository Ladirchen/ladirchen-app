import { contributionsService, familyContext, familyProfileService, familyProgressionService, homeCustomizationService, rewardShopService, savingsService } from '@/app/composition-root';
import { HOUSE_THEMES } from '@/domain/house-catalog';
import { createHouseAccessories } from '@/infrastructure/fixtures/family-world-fixtures';
import { mergeHouseLayout, normalizeFamilyMembers, normalizeFamilyPets } from '@/stores/family-world-state';
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from '../family-world-store-context';

export const lifecycleActions = {
  async hydrateFamilyAggregates(this: FamilyWorldStoreContext) {
    if (this.familyAggregatesHydrated) {return;}
    try {
      const familyId = familyContext.activeFamilyId;
      const [profile, contributions, savings, rewardShop, progression] = await Promise.all([
        familyProfileService.load(familyId),
        contributionsService.load(familyId),
        savingsService.load(familyId),
        rewardShopService.load(familyId),
        familyProgressionService.load(familyId),
      ]);
      if (profile) {
        this.members = normalizeFamilyMembers([...profile.state.members]);
        this.pets = normalizeFamilyPets(profile.state.pets);
        this.onboardingCompleted = profile.state.onboardingCompleted;
        this.subscriptionTier = profile.state.subscriptionTier;
      }
      if (contributions) {
        this.contributions = [...contributions.state.contributions];
        this.promotions = [...contributions.state.promotions];
        let contributionRewardsMigrated = false;
        this.contributions.forEach((contribution) => {
          if (contribution.status !== 'approved' || contribution.earnedReward !== undefined) {return;}
          const promotion = this.promotions.find(item => item.active && item.contributionId === contribution.id);
          contribution.earnedReward = contribution.reward * (promotion?.multiplier ?? 1) + (promotion?.teamworkBonus ?? 0);
          contribution.earnedRatingBonus = 0;
          contribution.earnedPromotionMultiplier = promotion?.multiplier;
          contribution.rewardCelebrated = !promotion;
          contributionRewardsMigrated = true;
        });
        if (contributionRewardsMigrated) {this.persistContributions();}
      }
      if (savings) {
        this.balances = { ...savings.state.balances };
        this.familyCurrencyCode = savings.state.familyCurrencyCode;
        this.goals = [...savings.state.goals];
        this.ladirchenPerCurrencyUnit = savings.state.ladirchenPerCurrencyUnit;
        this.pendingGuardianGifts = [...savings.state.pendingGuardianGifts];
      }
      if (rewardShop) {
        this.shopRewards = [...rewardShop.state.rewards];
      }
      if (progression) {
        this.completedWeeklyStreak = progression.state.completedWeeklyStreak;
        this.currentWeekDays = progression.state.currentWeekDays;
        this.currentWeekTarget = progression.state.currentWeekTarget;
        this.houseLevel = progression.state.houseLevel;
      }
      const activeChild = this.members.find(member => member.id === this.activeChildId && member.role === 'child') ??
        this.members.find(member => member.role === 'child');
      if (activeChild) {
        this.activeChildId = activeChild.id;
      }
    } catch {
      this.notify('notifications.load.family');
    } finally {
      this.familyAggregatesHydrated = true;
      this.familySetupOpen = !this.onboardingCompleted;
    }
    await this.hydrateHomeCustomization();
  },
  async hydrateHomeCustomization(this: FamilyWorldStoreContext) {
    if (this.homeCustomizationHydrated) {return;}
    try {
      const snapshot = await homeCustomizationService.load(familyContext.activeFamilyId);
      if (snapshot) {
        const accessoryStateById = new Map(snapshot.state.accessories.map(item => [item.id, item]));
        this.accessories = createHouseAccessories().map((accessory) => {
          const saved = accessoryStateById.get(accessory.id);
          return saved
            ? { ...accessory, owned: accessory.owned || saved.owned, equipped: accessory.equipped || saved.equipped }
            : accessory;
        });
        this.houseLayout = mergeHouseLayout(snapshot.state.placements);
        this.ownedHouseThemeIds = snapshot.state.editions.filter(edition => edition.owned).map(edition => edition.id);
        this.houseThemeId = this.ownedHouseThemeIds.includes(snapshot.state.selectedEditionId)
          ? snapshot.state.selectedEditionId
          : 'sunny-dollhouse';
      }
    } catch {
      this.notify('notifications.load.home');
    } finally {
      this.homeCustomizationHydrated = true;
    }
  },
  persistHomeCustomization(this: FamilyWorldStoreContext) {
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
        selectedEditionId: this.houseThemeId,
      },
    }, () => this.notify('notifications.save.home'));
  },
  persistFamilyProfile(this: FamilyWorldStoreContext) {
    familyProfileService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      state: { members: this.members, onboardingCompleted: this.onboardingCompleted, pets: this.pets, subscriptionTier: this.subscriptionTier },
      updatedBy: this.signedInMemberId,
    }, () => this.notify('notifications.save.profile'));
  },
  persistContributions(this: FamilyWorldStoreContext) {
    contributionsService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      state: { contributions: this.contributions, promotions: this.promotions },
      updatedBy: this.signedInMemberId,
    }, () => this.notify('notifications.save.contributions'));
  },
  persistSavings(this: FamilyWorldStoreContext) {
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
    }, () => this.notify('notifications.save.savings'));
  },
  persistRewardShop(this: FamilyWorldStoreContext) {
    rewardShopService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      state: { rewards: this.shopRewards },
      updatedBy: this.signedInMemberId,
    }, () => this.notify('notifications.save.shop'));
  },
  persistFamilyProgression(this: FamilyWorldStoreContext) {
    familyProgressionService.scheduleSave({
      familyId: familyContext.activeFamilyId,
      state: {
        completedWeeklyStreak: this.completedWeeklyStreak,
        currentWeekDays: this.currentWeekDays,
        currentWeekTarget: this.currentWeekTarget,
        houseLevel: this.houseLevel,
      },
      updatedBy: this.signedInMemberId,
    }, () => this.notify('notifications.save.progression'));
  },
  notify(this: FamilyWorldStoreContext, messageKey: string, params: Record<string, number | string> = {}) {
    this.snackbar.messageKey = messageKey;
    this.snackbar.params = params;
    this.snackbar.visible = true;
  },
} satisfies FamilyWorldActionGroup;
