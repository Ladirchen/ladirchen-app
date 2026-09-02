import type { Contribution, FamilyMemberId } from './types';

export const DEFAULT_ENERGY_PERCENT = 60;
export const MINIMUM_HOUSE_ENERGY_PERCENT = 60;

export function calculateContributionProgress(
  contributions: Contribution[],
  memberId: FamilyMemberId,
  emptyProgress = DEFAULT_ENERGY_PERCENT,
): number {
  const baseContributions = contributions.filter(
    (contribution) => contribution.kind === 'basic' && contribution.assigneeId === memberId,
  );
  const totalEnergy = baseContributions.reduce((sum, contribution) => sum + contribution.energy, 0);
  if (totalEnergy === 0) {
    return emptyProgress;
  }

  const completedEnergy = baseContributions
    .filter((contribution) => contribution.status === 'approved')
    .reduce((sum, contribution) => sum + contribution.energy, 0);
  return Math.min(100, Math.round((completedEnergy / totalEnergy) * 100));
}

export function calculateAverageEnergy(progressValues: number[]): number {
  if (progressValues.length === 0) {
    return DEFAULT_ENERGY_PERCENT;
  }
  return Math.round(progressValues.reduce((sum, progress) => sum + progress, 0) / progressValues.length);
}
