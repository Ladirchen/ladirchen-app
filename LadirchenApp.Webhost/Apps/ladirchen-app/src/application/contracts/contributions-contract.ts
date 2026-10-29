import type { FamilyReadModel } from "./family-read-model-contract";
import type { Contribution, Promotion } from "@/domain/contributions/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export const CONTRIBUTIONS_READ_MODEL_TYPE = "contributions";

export interface ContributionsState {
  readonly contributions: ReadonlyArray<Contribution>;
  readonly currentWeekApprovedContributionCountByMember: Readonly<Record<FamilyMemberId, number>>;
  readonly currentWeekEarnedByMember: Readonly<Record<FamilyMemberId, number>>;
  readonly perfectRatingBonusPercent: number;
  readonly promotions: ReadonlyArray<Promotion>;
  readonly todayEarnedByMember: Readonly<Record<FamilyMemberId, number>>;
}

export type ContributionsReadModel = FamilyReadModel<typeof CONTRIBUTIONS_READ_MODEL_TYPE, ContributionsState>;
