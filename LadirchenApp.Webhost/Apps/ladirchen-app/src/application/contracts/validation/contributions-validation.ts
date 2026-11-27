import type { ContributionsState } from "@/application/contracts/contributions-contract";
import {
  exhaustiveValues,
  hasLocalizedValue,
  hasOptionalDomainId,
  hasUniqueIds,
  isArrayOf,
  isClockTime,
  isDomainId,
  isFamilyMemberId,
  isFiniteNumber,
  isInteger,
  isIsoDateTime,
  isKnownString,
  isNonEmptyString,
  isOptionalBoolean,
  isOptionalNonNegativeNumber,
  isOptionalPositiveNumber,
  isOptionalString,
  isRecord,
  values,
} from "./runtime-validation";
import { CONTRIBUTION_RECURRENCES, CONTRIBUTION_STATUSES } from "@/domain/contributions/types";
import { CONTRIBUTION_RATING } from "@/domain/contributions/rating";
import type {
  Contribution,
  ContributionKind,
  ContributionRecurrence,
  ContributionStatus,
  FocusAchievement,
  Promotion,
  FocusGoal,
  WorldEffect,
} from "@/domain/contributions/types";
import type { ContributionId } from "@/domain/shared/identifiers";
const contributionKinds = exhaustiveValues<ContributionKind>({ basic: true, extra: true, school: true });
const contributionStatuses = values<ContributionStatus>(CONTRIBUTION_STATUSES);
const contributionRecurrences = values<ContributionRecurrence>(CONTRIBUTION_RECURRENCES);
const worldEffects = exhaustiveValues<WorldEffect>({
  flowers: true,
  garden: true,
  lights: true,
  smoke: true,
  sparkle: true,
});
function hasValidContent(value: Record<string, unknown>): boolean {
  return (
    isOptionalString(value.translationKey) &&
    hasLocalizedValue(value, "title") &&
    typeof value.description === "string" &&
    isNonEmptyString(value.icon) &&
    isOptionalString(value.category) &&
    (hasLocalizedValue(value, "area") || isNonEmptyString(value.areaKey)) &&
    isOptionalString(value.areaKey) &&
    isOptionalBoolean(value.isVisibleToChildren) &&
    typeof value.dueLabel === "string" &&
    isOptionalString(value.dueLabelKey) &&
    (value.dueDate === undefined || /^\d{4}-\d{2}-\d{2}$/.test(String(value.dueDate))) &&
    isKnownString(value.recurrence, contributionRecurrences)
  );
}
function hasValidResult(value: Record<string, unknown>): boolean {
  return (
    (value.stars === undefined ||
      (isInteger(value.stars) &&
        value.stars >= CONTRIBUTION_RATING.minimum &&
        value.stars <= CONTRIBUTION_RATING.maximum)) &&
    isOptionalNonNegativeNumber(value.earnedReward) &&
    isOptionalNonNegativeNumber(value.projectedReward) &&
    isOptionalNonNegativeNumber(value.earnedRatingBonus) &&
    isOptionalPositiveNumber(value.earnedPromotionMultiplier) &&
    (value.approvedAt === undefined || isIsoDateTime(value.approvedAt)) &&
    isOptionalBoolean(value.rewardCelebrated)
  );
}
function hasValidParticipants(value: Record<string, unknown>): boolean {
  return (
    hasOptionalDomainId(value.assigneeId) &&
    (value.invitedChildIds === undefined ||
      (isArrayOf(value.invitedChildIds, isFamilyMemberId) &&
        new Set(value.invitedChildIds).size === value.invitedChildIds.length)) &&
    (value.partnerableMemberIds === undefined ||
      (isArrayOf(value.partnerableMemberIds, isFamilyMemberId) &&
        new Set(value.partnerableMemberIds).size === value.partnerableMemberIds.length))
  );
}
function isContribution(value: unknown): value is Contribution {
  return (
    isRecord(value) &&
    isDomainId(value.id) &&
    hasValidContent(value) &&
    isKnownString(value.kind, contributionKinds) &&
    isKnownString(value.status, contributionStatuses) &&
    isFiniteNumber(value.reward) &&
    value.reward >= 0 &&
    isFiniteNumber(value.energy) &&
    value.energy >= 0 &&
    (value.worldEffect === undefined || isKnownString(value.worldEffect, worldEffects)) &&
    hasValidResult(value) &&
    hasValidParticipants(value)
  );
}
function isPromotion(value: unknown): value is Promotion {
  return (
    isRecord(value) &&
    isDomainId(value.id) &&
    isOptionalString(value.translationKey) &&
    isDomainId(value.contributionId) &&
    (hasLocalizedValue(value, "title") || isNonEmptyString(value.titleKey)) &&
    isOptionalString(value.titleKey) &&
    isFiniteNumber(value.multiplier) &&
    value.multiplier > 0 &&
    isClockTime(value.deadline) &&
    isFiniteNumber(value.teamworkBonus) &&
    value.teamworkBonus >= 0 &&
    typeof value.active === "boolean" &&
    isOptionalBoolean(value.available)
  );
}
function isFocusGoal(value: unknown): value is FocusGoal {
  return (
    isRecord(value) &&
    isDomainId(value.id) &&
    isNonEmptyString(value.title) &&
    typeof value.description === "string" &&
    isNonEmptyString(value.icon) &&
    isKnownString(value.period, new Set(["date", "weekly", "monthly"])) &&
    /^\d{4}-\d{2}-\d{2}$/.test(String(value.startDate)) &&
    /^\d{4}-\d{2}-\d{2}$/.test(String(value.endDate)) &&
    isFamilyMemberId(value.assigneeId) &&
    isFiniteNumber(value.completionBonus) &&
    value.completionBonus >= 0 &&
    typeof value.bonusPaid === "boolean" &&
    typeof value.completed === "boolean" &&
    isInteger(value.progressPercent) &&
    value.progressPercent >= 0 &&
    value.progressPercent <= 100 &&
    isInteger(value.remainingPercent) &&
    value.remainingPercent === 100 - value.progressPercent &&
    isArrayOf(value.preparationContributionIds, isDomainId) &&
    new Set(value.preparationContributionIds).size === value.preparationContributionIds.length
  );
}
function isFocusAchievement(value: unknown): value is FocusAchievement {
  return (
    isRecord(value) &&
    isDomainId(value.id) &&
    isFamilyMemberId(value.childId) &&
    isNonEmptyString(value.title) &&
    typeof value.description === "string" &&
    isNonEmptyString(value.icon) &&
    /^\d{4}-\d{2}-\d{2}$/.test(String(value.startDate)) &&
    /^\d{4}-\d{2}-\d{2}$/.test(String(value.endDate)) &&
    isIsoDateTime(value.achievedAt) &&
    isInteger(value.preparationCount) &&
    value.preparationCount >= 0 &&
    isFiniteNumber(value.earnedLadirchen) &&
    value.earnedLadirchen >= 0 &&
    isFiniteNumber(value.earnedEnergy) &&
    value.earnedEnergy >= 0
  );
}
function isNonNegativeAmountMap(value: unknown): boolean {
  return (
    isRecord(value) &&
    Object.entries(value).every(
      ([memberId, amount]) => isFamilyMemberId(memberId) && isFiniteNumber(amount) && amount >= 0,
    )
  );
}
function isNonNegativeIntegerMap(value: unknown): boolean {
  return (
    isRecord(value) &&
    Object.entries(value).every(([memberId, count]) => isFamilyMemberId(memberId) && isInteger(count) && count >= 0)
  );
}
export function isContributionsState(value: unknown): value is ContributionsState {
  if (
    !isRecord(value) ||
    !isArrayOf(value.contributions, isContribution) ||
    !hasUniqueIds(value.contributions) ||
    !isArrayOf(value.focusAchievements, isFocusAchievement) ||
    !hasUniqueIds(value.focusAchievements) ||
    (value.focusGoals !== undefined &&
      (!isArrayOf(value.focusGoals, isFocusGoal) || !hasUniqueIds(value.focusGoals))) ||
    !isNonNegativeIntegerMap(value.currentWeekApprovedContributionCountByMember) ||
    !isNonNegativeAmountMap(value.currentWeekEarnedByMember) ||
    !isFiniteNumber(value.perfectRatingBonusPercent) ||
    value.perfectRatingBonusPercent < 0 ||
    value.perfectRatingBonusPercent > 100 ||
    !isArrayOf(value.promotions, isPromotion) ||
    !hasUniqueIds(value.promotions) ||
    !isNonNegativeAmountMap(value.todayEarnedByMember) ||
    !isNonNegativeIntegerMap(value.totalApprovedContributionCountByMember) ||
    !isNonNegativeAmountMap(value.totalEarnedByMember)
  ) {
    return false;
  }

  const contributionIds = new Set<ContributionId>(value.contributions.map((contribution) => contribution.id));

  return value.promotions.every((promotion) => contributionIds.has(promotion.contributionId));
}
