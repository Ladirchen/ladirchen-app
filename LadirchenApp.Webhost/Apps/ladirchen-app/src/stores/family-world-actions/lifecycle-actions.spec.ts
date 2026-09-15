import { beforeEach, describe, expect, it, vi } from 'vitest';

import { createFamilyWorldState } from '../family-world-state';
import type { FamilyWorldStoreContext } from '../family-world-store-context';
import { createContributions, createFamilyWorldInitialData } from '@/infrastructure/fixtures/family-world-fixtures';
import { HOUSE_THEMES, ROOM_DESIGNS } from '@/domain/house';
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

describe('hydrateHomeCustomization', () => {
  beforeEach(() => {
    serviceMocks.homeCustomizationLoad.mockReset();
  });

  const createHomeCustomizationContext = () => {
    const initialData = createFamilyWorldInitialData();
    const context = Object.assign(createFamilyWorldState(initialData), {
      $familyWorld: {
        ...dependencies,
        initialDataFactory: { create: createFamilyWorldInitialData },
      },
      notify: vi.fn(),
    }) as unknown as FamilyWorldStoreContext;
    return { context, initialData };
  };

  it('restores an unequipped default accessory', async () => {
    const { context, initialData } = createHomeCustomizationContext();
    serviceMocks.homeCustomizationLoad.mockResolvedValue({
      aggregateType: 'home-customization',
      familyId: dependencies.familyContext.activeFamilyId,
      revision: 1,
      schemaVersion: 3,
      state: {
        accessories: initialData.accessories.map(accessory => ({
          id: accessory.id,
          equipped: accessory.id === 'round-rug' ? false : accessory.equipped,
          owned: accessory.owned,
        })),
        editions: HOUSE_THEMES.map(edition => ({ id: edition.id, owned: edition.ownedByDefault })),
        placements: initialData.houseLayout,
        roomDesigns: ROOM_DESIGNS.map(design => ({ id: design.id, owned: design.ownedByDefault })),
        selectedEditionId: 'sunny-dollhouse',
        selectedRoomDesigns: ROOM_DESIGNS
          .filter(design => design.ownedByDefault)
          .map(design => ({ designId: design.id, zoneId: design.zoneId })),
      },
      updatedAt: '2026-09-10T08:00:00.000Z',
      updatedBy: 'member-1',
    });

    await lifecycleActions.hydrateHomeCustomization.call(context);

    expect(context.accessories.find(accessory => accessory.id === 'round-rug')?.equipped).toBe(false);
  });

  it('keeps persisted room selections instead of replacing them with theme defaults', async () => {
    const { context, initialData } = createHomeCustomizationContext();
    const ownedThemeIds = new Set(['sunny-dollhouse', 'halloween-night']);
    serviceMocks.homeCustomizationLoad.mockResolvedValue({
      aggregateType: 'home-customization',
      familyId: dependencies.familyContext.activeFamilyId,
      revision: 1,
      schemaVersion: 3,
      state: {
        accessories: initialData.accessories.map(accessory => ({
          id: accessory.id,
          equipped: accessory.equipped,
          owned: accessory.owned,
        })),
        editions: HOUSE_THEMES.map(edition => ({ id: edition.id, owned: ownedThemeIds.has(edition.id) })),
        placements: initialData.houseLayout,
        roomDesigns: ROOM_DESIGNS.map(design => ({ id: design.id, owned: ownedThemeIds.has(design.houseThemeId) })),
        selectedEditionId: 'halloween-night',
        selectedRoomDesigns: [
          { designId: 'garden-halloween-night', zoneId: 'garden' },
          { designId: 'living-ladi-classic', zoneId: 'living-room' },
          { designId: 'kitchen-halloween-night', zoneId: 'kitchen' },
        ],
      },
      updatedAt: '2026-09-10T08:00:00.000Z',
      updatedBy: 'member-1',
    });

    await lifecycleActions.hydrateHomeCustomization.call(context);

    expect(context.selectedRoomDesignIds).toMatchObject({
      garden: 'garden-halloween-night',
      'living-room': 'living-ladi-classic',
      kitchen: 'kitchen-halloween-night',
    });
  });
});
