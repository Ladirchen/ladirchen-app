import type {
  ContributionTranslationNamespaceKey,
  PromotionTranslationNamespaceKey,
  TranslationKey,
} from "@/locales/translation-keys";
import type { ContributionId, FamilyMemberId, PromotionId } from "@/domain/shared/identifiers";

export type ContributionKind = "basic" | "extra" | "school";
export const CONTRIBUTION_RECURRENCES = ["none", "daily", "weekly"] as const;
export type ContributionRecurrence = (typeof CONTRIBUTION_RECURRENCES)[number];
export const CONTRIBUTION_STATUSES = ["available", "pending", "approved"] as const;
export type ContributionStatus = (typeof CONTRIBUTION_STATUSES)[number];
export type WorldEffect = "lights" | "flowers" | "garden" | "sparkle" | "smoke";

export interface Contribution {
  id: ContributionId;
  translationKey?: ContributionTranslationNamespaceKey;
  title: string;
  description: string;
  icon: string;
  category?: string;
  area: string;
  areaKey?: TranslationKey;
  kind: ContributionKind;
  status: ContributionStatus;
  reward: number;
  projectedReward?: number;
  energy: number;
  assigneeId?: FamilyMemberId;
  dueLabel: string;
  dueLabelKey?: TranslationKey;
  worldEffect?: WorldEffect;
  stars?: number;
  invitedChildIds?: FamilyMemberId[];
  isVisibleToChildren?: boolean;
  partnerableMemberIds?: FamilyMemberId[];
  earnedReward?: number;
  earnedRatingBonus?: number;
  earnedPromotionMultiplier?: number;
  approvedAt?: string;
  rewardCelebrated?: boolean;
  dueDate?: string;
  recurrence: ContributionRecurrence;
}

export interface Promotion {
  id: PromotionId;
  translationKey?: PromotionTranslationNamespaceKey;
  contributionId: ContributionId;
  title: string;
  titleKey?: TranslationKey;
  multiplier: number;
  deadline: string;
  teamworkBonus: number;
  active: boolean;
  available?: boolean;
}

export interface NewPromotion {
  contributionId: ContributionId;
  multiplier: number;
  deadline: string;
  teamworkBonus: number;
}

export interface NewContribution {
  title: string;
  description: string;
  icon: string;
  category: string;
  reward: number;
  energy: number;
  kind: ContributionKind;
  isVisibleToChildren: boolean;
  assigneeId?: FamilyMemberId;
  dueDate?: string;
  recurrence: ContributionRecurrence;
}

export type FocusPeriod = "date" | "weekly" | "monthly";

export interface FocusGoal {
  id: ContributionId;
  title: string;
  description: string;
  icon: string;
  period: FocusPeriod;
  startDate: string;
  endDate: string;
  assigneeId: FamilyMemberId;
  completionBonus: number;
  bonusPaid: boolean;
  completed: boolean;
  progressPercent: number;
  remainingPercent: number;
  preparationContributionIds: ContributionId[];
}

export interface FocusAchievement {
  id: ContributionId;
  childId: FamilyMemberId;
  title: string;
  description: string;
  icon: string;
  startDate: string;
  endDate: string;
  achievedAt: string;
  preparationCount: number;
  earnedLadirchen: number;
  earnedEnergy: number;
}

export interface NewFocusGoal {
  title: string;
  description: string;
  icon: string;
  period: FocusPeriod;
  startDate: string;
  endDate: string;
  assigneeId: FamilyMemberId;
  completionBonus: number;
}
