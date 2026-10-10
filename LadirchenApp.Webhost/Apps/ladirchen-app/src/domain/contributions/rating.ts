import { clamp, PERCENTAGE_BASE } from "@/domain/shared/numbers";
export const CONTRIBUTION_RATING = Object.freeze({
  maximum: 5,
  minimum: 1,
  perfect: 5,
});
export function normalizeContributionRating(rating: number): number {

  return clamp(Math.round(rating), CONTRIBUTION_RATING.minimum, CONTRIBUTION_RATING.maximum);
}
export function percentageOf(value: number, percentage: number): number {

  return value * (percentage / PERCENTAGE_BASE);
}
