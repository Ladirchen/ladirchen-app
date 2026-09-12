import { describe, expect, it } from 'vitest';

import { isContributionsState } from './contributions-validation';
import { isFamilyPetKindId } from './family-profile-validation';
import { isFamilyProgressionState } from './family-progression-validation';
import { isHomeCustomizationState, isHouseAccessoryId, isHouseThemeId } from './home-customization-validation';
import { isFamilyCurrency } from './savings-validation';

describe('family aggregate runtime validation', () => {
  it('rejects localized labels and unknown catalog identifiers', () => {
    expect(isFamilyPetKindId('cat')).toBe(true);
    expect(isFamilyPetKindId('Katze')).toBe(false);
    expect(isHouseAccessoryId('round-rug')).toBe(true);
    expect(isHouseAccessoryId('unknown-furniture')).toBe(false);
    expect(isHouseThemeId('sunny-dollhouse')).toBe(true);
    expect(isHouseThemeId('unknown-theme')).toBe(false);
    expect(isFamilyCurrency('CHF')).toBe(true);
    expect(isFamilyCurrency('USD')).toBe(false);
  });

  it('rejects a weekly progression without a positive target', () => {
    expect(isFamilyProgressionState({
      completedWeeklyStreak: 2,
      currentWeekTarget: 0,
      houseLevel: 2,
    })).toBe(false);
  });

  it('requires the selected house edition to be owned', () => {
    expect(isHomeCustomizationState({
      accessories: [],
      editions: [{ id: 'sunny-dollhouse', owned: false }],
      placements: [],
      selectedEditionId: 'sunny-dollhouse',
    })).toBe(false);
  });

  it('rejects promotions that reference a missing contribution', () => {
    expect(isContributionsState({
      contributions: [],
      promotions: [{
        id: 'promotion-1',
        contributionId: 'contribution-1',
        title: 'Double reward',
        multiplier: 2,
        deadline: '17:00',
        teamworkBonus: 10,
        active: true,
      }],
    })).toBe(false);
  });
});
