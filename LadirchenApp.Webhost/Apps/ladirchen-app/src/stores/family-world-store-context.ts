import type { FamilyPermissions } from '@/domain/family-permissions';
import type { FurnitureSetId } from '@/domain/house';
import type {
  Contribution,
  FamilyMember,
  FamilyMemberId,
  SavingGoal,
} from '@/domain/types';
import type { FamilyWorldState } from './family-world-state';

/**
 * Shared contract for domain action modules. It deliberately contains only
 * state plus the cross-domain getters/actions that an action may call.
 */
export type FamilyWorldStoreContext = FamilyWorldState & {
  readonly activeChild: FamilyMember;
  readonly availableBalance: number;
  readonly balance: number;
  readonly canArrangeHouse: boolean;
  readonly houseMeetsMinimumEnergy: boolean;
  readonly ownedFurnitureSetIds: FurnitureSetId[];
  readonly permissions: FamilyPermissions;
  readonly signedInMember: FamilyMember;
  averageTaskRatingFor: (memberId: FamilyMemberId) => number;
  balanceFor: (memberId: FamilyMemberId) => number;
  contributionProgress: (memberId: FamilyMemberId) => number;
  hydrateFamilyAggregates: () => Promise<void>;
  hydrateHomeCustomization: () => Promise<void>;
  notify: (messageKey: string, params?: Record<string, number | string>) => void;
  persistContributions: () => void;
  persistFamilyProfile: () => void;
  persistFamilyProgression: () => void;
  persistHomeCustomization: () => void;
  persistRewardShop: () => void;
  persistSavings: () => void;
  playRewardAnimation: (contribution: Contribution) => void;
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
