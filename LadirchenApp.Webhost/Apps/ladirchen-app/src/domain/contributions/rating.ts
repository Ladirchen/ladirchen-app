import type { Contribution } from "./types";

export const CONTRIBUTION_RATING = Object.freeze({
  maximum: 5,
  minimum: 1,
  perfect: 5,
});

export function totalEarnedContributionReward(contribution: Contribution): number {
  return (contribution.earnedReward ?? contribution.reward) + (contribution.earnedRatingBonus ?? 0);
}
