import type { FamilyPermissions } from "@/domain/family/permissions";
import type { FurnitureSetId } from "@/domain/house";
import type { Contribution } from "@/domain/contributions/types";
import type { FamilyMember } from "@/domain/family/types";
import type { SavingGoal } from "@/domain/savings/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";
import type { TranslationKey } from "@/locales/translation-keys";
import type { FamilyWorldState } from "./family-world-state";
import type { FamilyWorldDependencies } from "@/application/ports/family-world-dependencies";
import type { FamilyAggregateType } from "@/application/contracts/versioned-aggregate-contract";

declare module "pinia" {
  export interface PiniaCustomProperties {
    readonly $familyWorld: FamilyWorldDependencies;
  }
}

export type FamilyWorldStoreContext = FamilyWorldState & {
  readonly $familyWorld: FamilyWorldDependencies;
  readonly activeChild: FamilyMember;
  readonly availableBalance: number;
  readonly balance: number;
  readonly canArrangeHouse: boolean;
  readonly currentWeekDays: number;
  readonly houseMeetsMinimumEnergy: boolean;
  readonly ownedFurnitureSetIds: FurnitureSetId[];
  readonly permissions: FamilyPermissions;
  readonly signedInMember: FamilyMember;
  averageTaskRatingFor: (memberId: FamilyMemberId) => number;
  balanceFor: (memberId: FamilyMemberId) => number;
  contributionProgress: (memberId: FamilyMemberId) => number;
  hydrateFamilyAggregates: () => Promise<void>;
  hydrateHomeCustomization: () => Promise<void>;
  notify: (messageKey: TranslationKey, params?: Record<string, number | string>) => void;
  refreshFamilyAggregate: (aggregateType: FamilyAggregateType) => Promise<void>;
  playRewardAnimation: (contribution: Contribution) => void;
  refreshCurrentTime: () => void;
  revealNextContributionReward: () => void;
  revealNextGuardianGift: () => void;
  savingsInterestRateFor: (memberId: FamilyMemberId) => number;
  switchSession: (memberId: FamilyMemberId, revealRewards?: boolean) => void;
  todayEarnedFor: (memberId: FamilyMemberId) => number;
  withdrawableGoalBalance: (goal?: SavingGoal) => number;
  $reset: () => void;
};

export type FamilyWorldAction = (this: FamilyWorldStoreContext, ...args: never[]) => unknown;
export type FamilyWorldActionGroup = Record<string, FamilyWorldAction>;
