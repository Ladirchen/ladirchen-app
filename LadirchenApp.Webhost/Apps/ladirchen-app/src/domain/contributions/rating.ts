import { clamp, PERCENTAGE_BASE } from "@/domain/shared/numbers";

export const CONTRIBUTION_RATING = Object.freeze({
  maximum: 5,
  minimum: 1,
  perfect: 5,
});

export const normalizeContributionRating = (rating: number): number =>
  clamp(Math.round(rating), CONTRIBUTION_RATING.minimum, CONTRIBUTION_RATING.maximum);

export const percentageOf = (value: number, percentage: number): number =>
  value * (percentage / PERCENTAGE_BASE);

