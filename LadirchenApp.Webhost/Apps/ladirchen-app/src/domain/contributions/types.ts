import type { ContributionTranslationNamespaceKey, PromotionTranslationNamespaceKey, TranslationKey } from "@/locales/translation-keys";
import type { ContributionId, FamilyMemberId, PromotionId } from "@/domain/shared/identifiers";

export type ContributionKind = "basic" | "extra";
export const CONTRIBUTION_STATUSES = ["available", "pending", "approved"] as const;
export type ContributionStatus = typeof CONTRIBUTION_STATUSES[number];
export type WorldEffect = "lights" | "flowers" | "garden" | "sparkle" | "smoke";

export interface Contribution {
  id: ContributionId;
  translationKey?: ContributionTranslationNamespaceKey;
  title: string;
  description: string;
  icon: string;
  area: string;
  areaKey?: TranslationKey;
  kind: ContributionKind;
  status: ContributionStatus;
  reward: number;
  energy: number;
  assigneeId?: FamilyMemberId;
  dueLabel: string;
  dueLabelKey?: TranslationKey;
  worldEffect?: WorldEffect;
  stars?: number;
  invitedChildIds?: FamilyMemberId[];
  earnedReward?: number;
  earnedRatingBonus?: number;
  earnedPromotionMultiplier?: number;
  approvedAt?: string;
  rewardCelebrated?: boolean;
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
  reward: number;
  energy: number;
  kind: ContributionKind;
  assigneeId?: FamilyMemberId;
}
