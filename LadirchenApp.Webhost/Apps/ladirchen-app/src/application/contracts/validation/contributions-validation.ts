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
import { CONTRIBUTION_STATUSES } from "@/domain/contributions/types";
import { CONTRIBUTION_RATING } from "@/domain/contributions/rating";
import type {
  Contribution,
  ContributionKind,
  ContributionStatus,
  Promotion,
  WorldEffect,
} from "@/domain/contributions/types";
import type { ContributionId } from "@/domain/shared/identifiers";
const contributionKinds = exhaustiveValues<ContributionKind>({ basic: true, extra: true });
const contributionStatuses = values<ContributionStatus>(CONTRIBUTION_STATUSES);
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
    (hasLocalizedValue(value, "area") || isNonEmptyString(value.areaKey)) &&
    isOptionalString(value.areaKey) &&
    typeof value.dueLabel === "string" &&
    isOptionalString(value.dueLabelKey)
  );
}
function hasValidResult(value: Record<string, unknown>): boolean {
  return (
    (value.stars === undefined ||
      (isInteger(value.stars) &&
        value.stars >= CONTRIBUTION_RATING.minimum &&
        value.stars <= CONTRIBUTION_RATING.maximum)) &&
    isOptionalNonNegativeNumber(value.earnedReward) &&
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
        new Set(value.invitedChildIds).size === value.invitedChildIds.length))
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
    typeof value.active === "boolean"
  );
}
export function isContributionsState(value: unknown): value is ContributionsState {
  if (
    !isRecord(value) ||
    !isArrayOf(value.contributions, isContribution) ||
    !hasUniqueIds(value.contributions) ||
    !isArrayOf(value.promotions, isPromotion) ||
    !hasUniqueIds(value.promotions)
  ) {
    return false;
  }

  const contributionIds = new Set<ContributionId>(value.contributions.map((contribution) => contribution.id));

  return value.promotions.every((promotion) => contributionIds.has(promotion.contributionId));
}
