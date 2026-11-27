import type { FamilyReadModel } from "./family-read-model-contract";
import type { Contribution, FocusAchievement, FocusGoal, Promotion } from "@/domain/contributions/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export const CONTRIBUTIONS_READ_MODEL_TYPE = "contributions";

export interface ContributionsState {
  readonly contributions: ReadonlyArray<Contribution>;
  readonly focusAchievements: ReadonlyArray<FocusAchievement>;
  readonly focusGoals?: ReadonlyArray<FocusGoal>;
  readonly currentWeekApprovedContributionCountByMember: Readonly<Record<FamilyMemberId, number>>;
  readonly currentWeekEarnedByMember: Readonly<Record<FamilyMemberId, number>>;
  readonly perfectRatingBonusPercent: number;
  readonly promotions: ReadonlyArray<Promotion>;
  readonly todayEarnedByMember: Readonly<Record<FamilyMemberId, number>>;
  readonly totalApprovedContributionCountByMember: Readonly<Record<FamilyMemberId, number>>;
  readonly totalEarnedByMember: Readonly<Record<FamilyMemberId, number>>;
}

export type ContributionsReadModel = FamilyReadModel<typeof CONTRIBUTIONS_READ_MODEL_TYPE, ContributionsState>;
