import { defineStore } from "pinia";
import { inject } from "vue";
import type { InjectionKey } from "vue";
import { HOUSE_ROOMS } from "@/domain/house/catalog";
import type { FurnitureSetId } from "@/domain/house";
import { NO_FAMILY_PERMISSIONS } from "@/domain/family/permissions";
import type { FamilyPermissions } from "@/domain/family/permissions";
import type { Contribution, WorldEffect } from "@/domain/contributions/types";
import type { FamilyMember } from "@/domain/family/types";
import type { SavingGoal } from "@/domain/savings/types";
import type { ContributionId, FamilyMemberId } from "@/domain/shared/identifiers";
import { contributionsActions } from "@/stores/family-world-actions/contributions-actions";
import { familyActions } from "@/stores/family-world-actions/family-actions";
import { homeActions } from "@/stores/family-world-actions/home-actions";
import { lifecycleActions } from "@/stores/family-world-actions/lifecycle-actions";
import { savingsActions } from "@/stores/family-world-actions/savings-actions";
import { shopActions } from "@/stores/family-world-actions/shop-actions";
import { createFamilyWorldState } from "@/stores/family-world-state";
import type { FamilyWorldInitialDataFactory } from "@/application/ports/family-world-initial-data";
export function createFamilyWorldStoreDefinition(initialDataFactory: FamilyWorldInitialDataFactory) {
  return defineStore("ladirchenFamilyWorld", {
    state(this: void) {
      return createFamilyWorldState(initialDataFactory.create());
    },
    getters: {
      activeChild(state): FamilyMember | undefined {
        return state.members.find((member) => member.id === state.activeChildId);
      },
      signedInMember(state): FamilyMember | undefined {
        return state.members.find((member) => member.id === state.signedInMemberId);
      },
      permissions(): FamilyPermissions {
        return this.signedInMember?.permissions ?? NO_FAMILY_PERMISSIONS;
      },
      isFamilyAdmin(): boolean {
        return this.permissions.canManageFamily;
      },
      canArrangeHouse(state): boolean {
        return state.canArrangeHouseReadModel;
      },
      unlockedHouseRooms(state) {
        const availableAreas = new Set(
          state.houseCatalog?.areas.filter((area) => area.available).map((area) => area.id) ?? [],
        );

        return HOUSE_ROOMS.filter((room) => availableAreas.has(room.id));
      },
      ownedFurnitureSetIds(state): FurnitureSetId[] {
        return state.houseCatalog?.furnitureSets.filter((set) => set.owned).map((set) => set.id) ?? [];
      },
      balanceFor(this: void, state) {
        return (memberId: FamilyMemberId): number => state.balances[memberId] ?? 0;
      },
      displayNameFor(this: void, state) {
        return (memberId: FamilyMemberId): string => {
          const member = state.members.find((item) => item.id === memberId);

          return member?.nickname?.trim() || member?.name || "";
        };
      },
      balance(): number {
        return this.balanceFor(this.activeChildId);
      },
      totalVisibleSavedFor(this: void, state) {
        return (memberId: FamilyMemberId): number => state.sharedSavedBalances[memberId] ?? 0;
      },
      averageTaskRatingFor(this: void, state) {
        return (memberId: FamilyMemberId): number => {
          const evaluated = state.memberProgressions[memberId]?.averageBasicRating;

          if (evaluated !== undefined) {
            return evaluated;
          }

          return 0;
        };
      },
      dailyEnergy(state): number {
        if (state.simulatedEnergy !== null) {
          return state.simulatedEnergy;
        }

        return state.memberProgressions[state.activeChildId]?.completionPercent ?? 0;
      },
      pendingContributions(state): Contribution[] {
        return state.contributions.filter((contribution) => contribution.status === "pending");
      },
      contributionProgress(this: void, state) {
        return (memberId: FamilyMemberId): number => {
          return state.memberProgressions[memberId]?.completionPercent ?? 0;
        };
      },
      familyEnergy(): number {
        if (this.simulatedEnergy !== null) {
          return this.simulatedEnergy;
        }

        return this.familyEnergyReadModel;
      },
      houseMeetsMinimumEnergy(): boolean {
        return this.simulatedEnergy === null
          ? this.canCloseWeekSuccessfully
          : this.familyEnergy >= this.minimumSuccessfulWeekEnergy;
      },
      currentWeekDays(state): number {
        return state.memberProgressions[state.activeChildId]?.approvedDates.length ?? 0;
      },
      currentParticipationStreakDays(): number {
        return this.memberProgressions[this.activeChildId]?.streakDays ?? 0;
      },
      rewardForContribution(this: void, state) {
        return (contributionId: ContributionId): number => {
          const contribution = state.contributions.find((item) => item.id === contributionId);

          return contribution?.projectedReward ?? contribution?.earnedReward ?? contribution?.reward ?? 0;
        };
      },
      activeGoal(state): SavingGoal | undefined {
        const visibleGoals = state.goals.filter((goal) => goal.visibleToMemberIds?.includes(state.signedInMemberId));

        return (
          visibleGoals.find((goal) => goal.id === state.activeGoalId) ??
          visibleGoals.find((goal) => goal.ownerId === state.activeChildId) ??
          visibleGoals[0]
        );
      },
      pendingGoalCancellation(state): SavingGoal | undefined {
        return state.goals.find((goal) => {
          const limits = goal.commandLimits?.[state.signedInMemberId];

          return (
            goal.cancellationRequest?.pendingMemberIds.includes(state.signedInMemberId) === true &&
            (limits?.canApproveCancellation === true || limits?.canRejectCancellation === true)
          );
        });
      },
      ownSavingGoals(state): SavingGoal[] {
        const ownerId = state.viewerRole === "child" ? state.signedInMemberId : state.activeChildId;

        return state.goals.filter(
          (goal) => goal.ownerId === ownerId && goal.visibleToMemberIds?.includes(state.signedInMemberId),
        );
      },
      totalSaved(): number {
        return this.ownSavingGoals.reduce((sum, goal) => sum + goal.saved, 0);
      },
      totalInterestEarned(): number {
        return this.ownSavingGoals.reduce((sum, goal) => sum + (goal.interestEarned ?? 0), 0);
      },
      todayEarnedFor(this: void, state) {
        return (memberId: FamilyMemberId): number => state.todayEarnedByMember[memberId] ?? 0;
      },
      todayEarned(): number {
        return this.todayEarnedFor(this.activeChildId);
      },
      currentWeekEarnedFor(this: void, state) {
        return (memberId: FamilyMemberId): number => state.currentWeekEarnedByMember[memberId] ?? 0;
      },
      currentWeekApprovedContributionCountFor(this: void, state) {
        return (memberId: FamilyMemberId): number => state.currentWeekApprovedContributionCountByMember[memberId] ?? 0;
      },
      withdrawableGoalBalance(this: void) {
        return (goal?: SavingGoal): number => {
          if (!goal) {
            return 0;
          }

          return goal.withdrawableBalance ?? 0;
        };
      },
      reservedBalance(state): number {
        return Math.max(0, this.balance - (state.availableBalances[state.activeChildId] ?? 0));
      },
      availableBalance(state): number {
        return state.availableBalances[state.activeChildId] ?? 0;
      },
      familyCurrencyValue(this: void, state) {
        return (ladirchen: number): number => Number((ladirchen / state.ladirchenPerCurrencyUnit).toFixed(2));
      },
      averageTaskRating(): number {
        return this.averageTaskRatingFor(this.activeChildId);
      },
      savingsInterestRateFor(this: void, state) {
        return (memberId: FamilyMemberId): number => {
          const evaluated = state.memberProgressions[memberId]?.savingsInterestRate;

          if (evaluated !== undefined) {
            return evaluated;
          }

          return 0;
        };
      },
      savingsInterestRate(): number {
        return this.savingsInterestRateFor(this.activeChildId);
      },
      activeWorldEffects(state): WorldEffect[] {
        return state.contributions
          .filter(
            (
              contribution,
            ): contribution is Contribution & {
              worldEffect: WorldEffect;
            } => contribution.status === "approved" && Boolean(contribution.worldEffect),
          )
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
}
export type FamilyWorldStoreDefinition = ReturnType<typeof createFamilyWorldStoreDefinition>;
export const FAMILY_WORLD_STORE_DEFINITION: InjectionKey<FamilyWorldStoreDefinition> =
  Symbol("FamilyWorldStoreDefinition");
export function useFamilyWorldStore() {
  const storeDefinition = inject(FAMILY_WORLD_STORE_DEFINITION);

  if (!storeDefinition) {
    throw new Error("The family world store has not been registered.");
  }

  return storeDefinition();
}
