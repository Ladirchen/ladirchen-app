import { describe, expect, it } from 'vitest';

import { normalizeContributionRating, percentageOf } from './rating';

describe('contribution rating rules', () => {
  it('rounds and clamps ratings to the supported range', () => {
    expect(normalizeContributionRating(0)).toBe(1);
    expect(normalizeContributionRating(3.6)).toBe(4);
    expect(normalizeContributionRating(8)).toBe(5);
  });

  it('calculates percentage-based rewards', () => {
    expect(percentageOf(80, 5)).toBe(4);
  });
});

