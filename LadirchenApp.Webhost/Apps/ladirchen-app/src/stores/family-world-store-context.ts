import type { FamilyPermissions } from "@/domain/family/permissions";
import type { FurnitureSetId } from "@/domain/house";
import type { Contribution } from "@/domain/contributions/types";
import type { FamilyMember } from "@/domain/family/types";
import type { SavingGoal } from "@/domain/savings/types";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import type { TranslationKey } from "@/locales/translation-keys";
import type { FamilyWorldState } from "./family-world-state";
import type { FamilyWorldDependencies } from "@/application/ports/family-world-dependencies";
import type { FamilyReadModelType } from "@/application/contracts/family-read-model-contract";

declare module "pinia" {
  export interface PiniaCustomProperties {
    readonly $familyWorld: FamilyWorldDependencies;
  }
}

export type FamilyWorldStoreContext = FamilyWorldState & {
  readonly $familyWorld: FamilyWorldDependencies;
  readonly activeChild: FamilyMember | undefined;
  readonly activeGoal: SavingGoal | undefined;
  readonly availableBalance: number;
  readonly balance: number;
  readonly canArrangeHouse: boolean;
  readonly canInviteFamilyMembers: boolean;
  readonly currentWeekDays: number;
  readonly houseMeetsMinimumEnergy: boolean;
  readonly ownedFurnitureSetIds: FurnitureSetId[];
  readonly permissions: FamilyPermissions;
  readonly pendingGoalCancellation: SavingGoal | undefined;
  readonly signedInMember: FamilyMember | undefined;
  averageTaskRatingFor: (memberId: FamilyMemberId) => number;
  balanceFor: (memberId: FamilyMemberId) => number;
  contributionProgress: (memberId: FamilyMemberId) => number;
  currentWeekApprovedContributionCountFor: (memberId: FamilyMemberId) => number;
  currentWeekEarnedFor: (memberId: FamilyMemberId) => number;
  hydrateFamilyReadModels: () => Promise<void>;
  hydrateHomeCustomization: () => Promise<void>;
  notify: (messageKey: TranslationKey, params?: Record<string, number | string>) => void;
  refreshAllFamilyReadModels: () => Promise<void>;
  refreshFamilyReadModel: (readModelType: FamilyReadModelType) => Promise<void>;
  playRewardAnimation: (contribution: Contribution) => void;
  refreshCurrentTime: () => void;
  revealNextContributionReward: () => void;
  revealNextGuardianGift: () => void;
  restoreAuthenticatedSession: () => Promise<boolean>;
  savingsInterestRateFor: (memberId: FamilyMemberId) => number;
  signInToFamily: (familyId: FamilyId, memberId: FamilyMemberId) => Promise<boolean>;
  todayEarnedFor: (memberId: FamilyMemberId) => number;
  withdrawableGoalBalance: (goal?: SavingGoal) => number;
  $reset: () => void;
};

export type FamilyWorldAction = (this: FamilyWorldStoreContext, ...args: never[]) => unknown;
export type FamilyWorldActionGroup = Record<string, FamilyWorldAction>;
