export const PERCENTAGE_BASE = 100;

export const clamp = (value: number, minimum: number, maximum: number): number =>
  Math.max(minimum, Math.min(maximum, value));

export const percentageOfTotal = (value: number, total: number): number =>
  total <= 0 ? 0 : clamp((value / total) * PERCENTAGE_BASE, 0, PERCENTAGE_BASE);
