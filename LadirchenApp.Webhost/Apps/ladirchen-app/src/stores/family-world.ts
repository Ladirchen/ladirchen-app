import { defineStore } from 'pinia';
import { inject } from 'vue';
import type { InjectionKey } from 'vue';

import { calculateAverageEnergy, calculateContributionProgress, MINIMUM_HOUSE_ENERGY_PERCENT } from '@/domain/contributions/energy';
import { FURNITURE_SETS, HOUSE_ROOMS } from '@/domain/house/catalog';
import type { FurnitureSetId } from '@/domain/house';
import { resolveFamilyPermissions } from '@/domain/family/permissions';
import type { FamilyPermissions } from '@/domain/family/permissions';
import { isPromotionAvailable } from '@/domain/contributions/promotions';
import { familyParticipationInterestStrategy } from '@/domain/savings/interest';
import type { Contribution, WorldEffect } from '@/domain/contributions/types';
import type { FamilyMember } from '@/domain/family/types';
import type { SavingGoal } from '@/domain/savings/types';
import type { ContributionId, FamilyMemberId } from '@/domain/shared/identifiers';
import { isSameCalendarDay } from '@/domain/shared/zoned-calendar';
import { currentWeekDaysFromContributions } from '@/domain/contributions/weekly-progress';
import { contributionsActions } from './family-world-actions/contributions-actions';
import { familyActions } from './family-world-actions/family-actions';
import { homeActions } from './family-world-actions/home-actions';
import { lifecycleActions } from './family-world-actions/lifecycle-actions';
import { savingsActions } from './family-world-actions/savings-actions';
import { shopActions } from './family-world-actions/shop-actions';
import { createFamilyWorldState } from './family-world-state';
import type { FamilyWorldInitialDataFactory } from '@/application/ports/family-world-initial-data';

export const createFamilyWorldStoreDefinition = (initialDataFactory: FamilyWorldInitialDataFactory) => defineStore('ladirchenFamilyWorld', {
  state: () => createFamilyWorldState(initialDataFactory.create()),

  getters: {
    activeChild(state): FamilyMember {
      return state.members.find((member) => member.id === state.activeChildId) ?? state.members[0]!;
    },
    signedInMember(state): FamilyMember {
      return state.members.find((member) => member.id === state.signedInMemberId) ?? state.members[0]!;
    },
    permissions(): FamilyPermissions {
      return resolveFamilyPermissions(this.signedInMember);
    },
    isFamilyAdmin(): boolean {
      return this.signedInMember.role === 'guardian' && this.signedInMember.guardianAccess === 'admin';
    },
    canArrangeHouse(state): boolean {
      return state.subscriptionTier === 'pro';
    },
    unlockedHouseRooms(state) {
      return HOUSE_ROOMS.filter((room) => room.minimumHouseLevel <= state.houseLevel);
    },
    ownedFurnitureSetIds(state): FurnitureSetId[] {
      return FURNITURE_SETS
        .filter((set) => set.accessoryIds.every((id) => state.accessories.find((accessory) => accessory.id === id)?.owned))
        .map((set) => set.id);
    },
    balanceFor: (state) => (memberId: FamilyMemberId): number => state.balances[memberId] ?? 0,
    displayNameFor: (state) => (memberId: FamilyMemberId): string => {
      const member = state.members.find((item) => item.id === memberId);
      return member?.nickname?.trim() || member?.name || '';
    },
    balance(): number {
      return this.balanceFor(this.activeChildId);
    },
    totalVisibleSavedFor: (state) => (memberId: FamilyMemberId): number => state.goals
      .filter((goal) => goal.ownerId === memberId && goal.visibility !== 'private')
      .reduce((sum, goal) => sum + goal.saved, 0),
    averageTaskRatingFor: (state) => (memberId: FamilyMemberId): number => {
      const rated = state.contributions.filter(
        (contribution) => contribution.assigneeId === memberId && contribution.stars,
      );
      return rated.length === 0
        ? 0
        : rated.reduce((sum, contribution) => sum + (contribution.stars ?? 0), 0) / rated.length;
    },
    pendingCountFor: (state) => (memberId: FamilyMemberId): number => state.contributions.filter(
      (contribution) => contribution.assigneeId === memberId && contribution.status === 'pending',
    ).length,
    openCountFor: (state) => (memberId: FamilyMemberId): number => state.contributions.filter(
      (contribution) => contribution.assigneeId === memberId && contribution.status === 'available',
    ).length,
    dailyBaseContributions(state): Contribution[] {
      return state.contributions.filter(
        (contribution) => contribution.kind === 'basic' && contribution.assigneeId === state.activeChildId,
      );
    },
    dailyEnergy(state): number {
      if (state.simulatedEnergy !== null) {return state.simulatedEnergy;}
      return calculateContributionProgress(state.contributions, state.activeChildId);
    },
    approvedBaseCount(): number {
      return this.dailyBaseContributions.filter((contribution) => contribution.status === 'approved').length;
    },
    pendingContributions(state): Contribution[] {
      return state.contributions.filter((contribution) => contribution.status === 'pending');
    },
    contributionProgress: (state) => (memberId: FamilyMemberId): number => {
      return calculateContributionProgress(state.contributions, memberId);
    },
    familyEnergy(): number {
      if (this.simulatedEnergy !== null) {return this.simulatedEnergy;}
      const participants = this.members.filter((member) => member.role === 'child' || member.participatesInWeeklyGoal);
      return calculateAverageEnergy(participants.map((member) => this.contributionProgress(member.id)));
    },
    houseMeetsMinimumEnergy(): boolean {
      return this.familyEnergy >= MINIMUM_HOUSE_ENERGY_PERCENT;
    },
    currentWeekDays(state): number {
      return currentWeekDaysFromContributions(state.contributions, state.familyTimeZone, new Date(state.currentTimeMilliseconds));
    },
    currentDailyStreak(): number {
      return this.completedWeeklyStreak * this.currentWeekTarget + this.currentWeekDays;
    },
    rewardForContribution: (state) => (contributionId: ContributionId): number => {
      const contribution = state.contributions.find((item) => item.id === contributionId);
      if (contribution?.status === 'approved' && contribution.earnedReward !== undefined) {
        return contribution.earnedReward;
      }
      const promotion = state.promotions.find(
        (item) => item.contributionId === contributionId && isPromotionAvailable(item, state.familyTimeZone, new Date(state.currentTimeMilliseconds)),
      );
      return (contribution?.reward ?? 0) * (promotion?.multiplier ?? 1) + (promotion?.teamworkBonus ?? 0);
    },
    activeGoal(state): SavingGoal {
      const visibleGoals = state.goals.filter(
        (goal) => {
          const permissions = resolveFamilyPermissions(state.members.find(member => member.id === state.signedInMemberId));
          return (goal.visibility === 'family' && (goal.ownerId !== 'family' || permissions.canViewFamilyGoals)) ||
          (state.viewerRole === 'child' && goal.ownerId === state.activeChildId) ||
          (state.viewerRole === 'guardian' && permissions.canViewGuardianGoals && goal.visibility === 'guardians');
        },
      );
      return visibleGoals.find((goal) => goal.id === state.activeGoalId) ??
        visibleGoals.find((goal) => goal.ownerId === state.activeChildId) ??
        visibleGoals[0] ??
        state.goals[0]!;
    },
    ownSavingGoals(state): SavingGoal[] {
      const ownerId = state.viewerRole === 'child' ? state.signedInMemberId : state.activeChildId;
      return state.goals.filter(
        (goal) => goal.ownerId === ownerId && (
          state.viewerRole === 'child' ||
          goal.visibility === 'family' ||
          (goal.visibility === 'guardians' && resolveFamilyPermissions(state.members.find(member => member.id === state.signedInMemberId)).canViewGuardianGoals)
        ),
      );
    },
    totalSaved(): number {
      return this.ownSavingGoals.reduce((sum, goal) => sum + goal.saved, 0);
    },
    totalInterestEarned(): number {
      return this.ownSavingGoals.reduce((sum, goal) => sum + (goal.interestEarned ?? 0), 0);
    },
    todayEarnedFor: (state) => (memberId: FamilyMemberId): number => {
      const today = new Date(state.currentTimeMilliseconds);
      return state.contributions
        .filter((contribution) => contribution.status === 'approved' &&
          contribution.assigneeId === memberId &&
          contribution.approvedAt !== undefined &&
          isSameCalendarDay(contribution.approvedAt, today, state.familyTimeZone))
        .reduce((sum, contribution) => sum +
          (contribution.earnedReward ?? contribution.reward) +
          (contribution.earnedRatingBonus ?? 0), 0);
    },
    todayEarned(): number {
      return this.todayEarnedFor(this.activeChildId);
    },
    withdrawableGoalBalance: () => (goal?: SavingGoal): number => {
      if (!goal) {return 0;}
      if (goal.saved >= goal.target) {return goal.saved;}
      return Math.max(0, goal.saved - (goal.starterBonus ?? 0));
    },
    reservedBalance(state): number {
      return state.shopRewards
        .filter((reward) => reward.status === 'requested' && reward.requesterId === state.activeChildId)
        .reduce((sum, reward) => sum + reward.price, 0);
    },
    availableBalance(): number {
      return Math.max(0, this.balance - this.reservedBalance);
    },
    familyCurrencyValue: (state) => (ladirchen: number): number =>
      Number((ladirchen / state.ladirchenPerCurrencyUnit).toFixed(2)),
    averageTaskRating(): number {
      return this.averageTaskRatingFor(this.activeChildId);
    },
    savingsInterestRateFor: (state) => (memberId: FamilyMemberId): number => {
      const baseContributions = state.contributions.filter(
        (contribution) => contribution.kind === 'basic' && contribution.assigneeId === memberId,
      );
      const streak = memberId === state.activeChildId
        ? state.completedWeeklyStreak * state.currentWeekTarget + currentWeekDaysFromContributions(state.contributions, state.familyTimeZone, new Date(state.currentTimeMilliseconds))
        : state.members.find((member) => member.id === memberId)?.weeklyStreak ?? 0;
      const ratedContributions = baseContributions.filter((contribution) => contribution.stars);
      const rating = ratedContributions.length === 0
        ? 0
        : ratedContributions.reduce((sum, contribution) => sum + (contribution.stars ?? 0), 0)
          / ratedContributions.length;
      return familyParticipationInterestStrategy.calculateRate({
        baseRate: state.baseSavingsRatePercent,
        completionBonusRate: state.completionBonusRate,
        completionPercent: calculateContributionProgress(state.contributions, memberId),
        maxRate: state.maxSavingsRatePercent,
        rating,
        ratingBonusRate: state.ratingBonusRate,
        streakBonusRate: state.streakBonusRate,
        streakDays: streak,
      });
    },
    savingsInterestRate(): number {
      return this.savingsInterestRateFor(this.activeChildId);
    },
    activeWorldEffects(state): WorldEffect[] {
      return state.contributions
        .filter((contribution): contribution is Contribution & { worldEffect: WorldEffect } => contribution.status === 'approved' && Boolean(contribution.worldEffect))
        .map((contribution) => contribution.worldEffect);
    },
  },

  actions: {
    ...lifecycleActions,
    ...familyActions,
    ...contributionsActions,
    ...savingsActions,
    ...shopActions,
    ...homeActions,
  },
});

export type FamilyWorldStoreDefinition = ReturnType<typeof createFamilyWorldStoreDefinition>;

export const FAMILY_WORLD_STORE_DEFINITION: InjectionKey<FamilyWorldStoreDefinition> = Symbol('FamilyWorldStoreDefinition');

export const useFamilyWorldStore = () => {
  const storeDefinition = inject(FAMILY_WORLD_STORE_DEFINITION);
  if (!storeDefinition) {throw new Error('The family world store has not been registered.');}
  return storeDefinition();
};
