import { HOME_CUSTOMIZATION_AGGREGATE_TYPE, HOME_CUSTOMIZATION_SCHEMA_VERSION } from '@/application/contracts/home-customization-contract';
import type { HomeCustomizationState } from '@/application/contracts/home-customization-contract';
import { isHomeCustomizationState, isRecord } from '@/application/contracts/family-aggregate-validation';
import type { FamilyId } from '@/domain/types';

import { LocalStorageVersionedAggregateRepository, parseLocalStorageValue } from './local-storage-versioned-aggregate-repository';

const LEGACY_STORAGE_PREFIX = 'ladirchen-home-customization';
const LEGACY_ACCESSORIES_KEY = 'ladirchen-house-accessories';
const LEGACY_LAYOUT_KEY = 'ladirchen-house-layout';
const stateFromLegacySnapshot = (value: unknown): HomeCustomizationState | null => {
  if (!isRecord(value) || !Array.isArray(value.accessories) || !Array.isArray(value.placements)) {
    return null;
  }
  const candidate: unknown = {
    accessories: value.accessories,
    editions: Array.isArray(value.editions)
      ? value.editions
      : [{ id: 'sunny-dollhouse', owned: true }],
    placements: value.placements,
    selectedEditionId: typeof value.selectedEditionId === 'string'
      ? value.selectedEditionId
      : 'sunny-dollhouse',
  };
  return isHomeCustomizationState(candidate) ? candidate : null;
};

const migrate = (_stored: unknown, familyId: FamilyId): HomeCustomizationState | null => {
  const previousSnapshot = parseLocalStorageValue(`${LEGACY_STORAGE_PREFIX}:${familyId}`);
  const snapshotState = stateFromLegacySnapshot(previousSnapshot);
  if (snapshotState) {
    return snapshotState;
  }
  const accessories = parseLocalStorageValue(LEGACY_ACCESSORIES_KEY);
  const placements = parseLocalStorageValue(LEGACY_LAYOUT_KEY);
  if (!Array.isArray(accessories) && !Array.isArray(placements)) {
    return null;
  }
  return stateFromLegacySnapshot({ accessories: Array.isArray(accessories) ? accessories : [], placements: Array.isArray(placements) ? placements : [] });
};

export class LocalStorageHomeCustomizationRepository extends LocalStorageVersionedAggregateRepository<
  typeof HOME_CUSTOMIZATION_AGGREGATE_TYPE,
  typeof HOME_CUSTOMIZATION_SCHEMA_VERSION,
  HomeCustomizationState
> {
  public constructor() {
    super({ aggregateType: HOME_CUSTOMIZATION_AGGREGATE_TYPE, isState: isHomeCustomizationState, migrate, schemaVersion: HOME_CUSTOMIZATION_SCHEMA_VERSION });
  }
}
