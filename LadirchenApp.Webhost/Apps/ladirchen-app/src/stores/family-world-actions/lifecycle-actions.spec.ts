import { beforeEach, describe, expect, it, vi } from 'vitest';

import { createFamilyWorldState } from '../family-world-state';
import type { FamilyWorldStoreContext } from '../family-world-store-context';
import { createContributions, createFamilyWorldInitialData } from '@/infrastructure/fixtures/family-world-fixtures';
import { lifecycleActions } from './lifecycle-actions';

const serviceMocks = vi.hoisted(() => ({
  contributionsLoad: vi.fn(),
  familyProfileLoad: vi.fn(),
  familyProgressionLoad: vi.fn(),
  homeCustomizationLoad: vi.fn(),
  rewardShopLoad: vi.fn(),
  savingsLoad: vi.fn(),
}));

const dependencies = {
  clientStorage: { getItem: vi.fn(() => null), removeItem: vi.fn(), setItem: vi.fn() },
  contributionsService: { load: serviceMocks.contributionsLoad },
  familyContext: { activeFamilyId: 'c776a9b6-c37d-4a51-91ba-269a67274478', setActiveFamilyId: vi.fn() },
  familyProfileService: { load: serviceMocks.familyProfileLoad },
  familyProgressionService: { load: serviceMocks.familyProgressionLoad },
  homeCustomizationService: { load: serviceMocks.homeCustomizationLoad },
  rewardShopService: { load: serviceMocks.rewardShopLoad },
  savingsService: { load: serviceMocks.savingsLoad },
};

describe('hydrateFamilyAggregates', () => {
  beforeEach(() => {
    Object.values(serviceMocks).forEach(mock => mock.mockReset().mockResolvedValue(null));
  });

  it('applies successful aggregate loads and remains retryable after a partial failure', async () => {
    const contributions = createContributions();
    const firstContribution = contributions[0]!;
    firstContribution.title = 'Loaded contribution';
    serviceMocks.contributionsLoad.mockResolvedValue({
      aggregateType: 'contributions',
      familyId: 'c776a9b6-c37d-4a51-91ba-269a67274478',
      revision: 3,
      schemaVersion: 1,
      state: { contributions, promotions: [] },
      updatedAt: '2026-09-10T08:00:00.000Z',
      updatedBy: 'member-1',
    });
    serviceMocks.familyProfileLoad.mockRejectedValue(new Error('Profile endpoint unavailable'));
    const context = Object.assign(createFamilyWorldState(createFamilyWorldInitialData()), {
      $familyWorld: dependencies,
      hydrateHomeCustomization: vi.fn().mockResolvedValue(undefined),
      notify: vi.fn(),
      persistContributions: vi.fn(),
    }) as unknown as FamilyWorldStoreContext;

    await lifecycleActions.hydrateFamilyAggregates.call(context);

    expect(context.contributions[0]?.title).toBe('Loaded contribution');
    expect(context.familyAggregatesHydrated).toBe(false);
    expect(context.notify).toHaveBeenCalledWith('notifications.load.family');
    expect(context.hydrateHomeCustomization).toHaveBeenCalledOnce();
  });

  it('marks hydration complete only when every aggregate load succeeds', async () => {
    const context = Object.assign(createFamilyWorldState(createFamilyWorldInitialData()), {
      $familyWorld: dependencies,
      hydrateHomeCustomization: vi.fn().mockResolvedValue(undefined),
      notify: vi.fn(),
      persistContributions: vi.fn(),
    }) as unknown as FamilyWorldStoreContext;

    await lifecycleActions.hydrateFamilyAggregates.call(context);

    expect(context.familyAggregatesHydrated).toBe(true);
    expect(context.notify).not.toHaveBeenCalled();
  });
});
