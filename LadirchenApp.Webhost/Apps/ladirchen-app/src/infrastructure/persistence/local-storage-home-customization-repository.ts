import { HOME_CUSTOMIZATION_SCHEMA_VERSION } from '@/application/contracts/home-customization-contract';
import type { HomeCustomizationSnapshot, SaveHomeCustomizationCommand } from '@/application/contracts/home-customization-contract';
import { HomeCustomizationConflictError } from '@/application/ports/home-customization-repository';
import type { HomeCustomizationRepository } from '@/application/ports/home-customization-repository';
import { createDomainId } from '@/domain/types';
import type { FamilyId, HouseLayoutPlacement } from '@/domain/types';

const STORAGE_PREFIX = 'ladirchen-home-customization';
const LEGACY_ACCESSORIES_KEY = 'ladirchen-house-accessories';
const LEGACY_LAYOUT_KEY = 'ladirchen-house-layout';

const storageAvailable = (): boolean => typeof localStorage !== 'undefined';
const storageKey = (familyId: FamilyId): string => `${STORAGE_PREFIX}:${familyId}`;

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null;

const isSnapshot = (value: unknown): value is HomeCustomizationSnapshot =>
  isRecord(value) &&
  value.schemaVersion === HOME_CUSTOMIZATION_SCHEMA_VERSION &&
  typeof value.familyId === 'string' &&
  typeof value.revision === 'number' &&
  typeof value.updatedAt === 'string' &&
  typeof value.updatedBy === 'string' &&
  Array.isArray(value.accessories) &&
  Array.isArray(value.editions) &&
  Array.isArray(value.placements) &&
  typeof value.selectedEditionId === 'string';

const isVersionOneSnapshot = (value: unknown): value is Omit<HomeCustomizationSnapshot, 'editions' | 'schemaVersion' | 'selectedEditionId'> & { schemaVersion: 1 } =>
  isRecord(value) &&
  value.schemaVersion === 1 &&
  typeof value.familyId === 'string' &&
  typeof value.revision === 'number' &&
  typeof value.updatedAt === 'string' &&
  typeof value.updatedBy === 'string' &&
  Array.isArray(value.accessories) &&
  Array.isArray(value.placements);

const migrateVersionOneSnapshot = (snapshot: ReturnType<typeof parseStoredValue>): HomeCustomizationSnapshot | null => {
  if (!isVersionOneSnapshot(snapshot)) {
    return null;
  }
  return {
    ...snapshot,
    schemaVersion: HOME_CUSTOMIZATION_SCHEMA_VERSION,
    editions: [{ id: 'sunny-dollhouse', owned: true }],
    selectedEditionId: 'sunny-dollhouse',
  };
};

const parseStoredValue = (key: string): unknown => {
  const serialized = localStorage.getItem(key);
  if (serialized === null) {
    return null;
  }
  try {
    return JSON.parse(serialized) as unknown;
  } catch {
    return null;
  }
};

export class LocalStorageHomeCustomizationRepository implements HomeCustomizationRepository {
  public async load(familyId: FamilyId): Promise<HomeCustomizationSnapshot | null> {
    if (!storageAvailable()) {
      return null;
    }
    const current = parseStoredValue(storageKey(familyId));
    if (isSnapshot(current) && current.familyId === familyId) {
      return current;
    }
    const migrated = migrateVersionOneSnapshot(current);
    if (migrated?.familyId === familyId) {
      return migrated;
    }
    return this.loadLegacySnapshot(familyId);
  }

  public async save(command: SaveHomeCustomizationCommand): Promise<HomeCustomizationSnapshot> {
    if (storageAvailable()) {
      const current = parseStoredValue(storageKey(command.familyId));
      if (isSnapshot(current) && current.revision !== command.expectedRevision) {
        throw new HomeCustomizationConflictError();
      }
    }
    const snapshot: HomeCustomizationSnapshot = {
      ...command,
      schemaVersion: HOME_CUSTOMIZATION_SCHEMA_VERSION,
      revision: command.expectedRevision + 1,
      updatedAt: new Date().toISOString(),
      accessories: structuredClone(command.accessories),
      editions: structuredClone(command.editions),
      placements: structuredClone(command.placements),
    };
    if (storageAvailable()) {
      localStorage.setItem(storageKey(command.familyId), JSON.stringify(snapshot));
    }
    return snapshot;
  }

  private loadLegacySnapshot(familyId: FamilyId): HomeCustomizationSnapshot | null {
    const accessories = parseStoredValue(LEGACY_ACCESSORIES_KEY);
    const placements = parseStoredValue(LEGACY_LAYOUT_KEY);
    if (!Array.isArray(accessories) && !Array.isArray(placements)) {
      return null;
    }
    return {
      schemaVersion: HOME_CUSTOMIZATION_SCHEMA_VERSION,
      familyId,
      revision: 0,
      updatedAt: new Date(0).toISOString(),
      updatedBy: createDomainId.familyMember('legacy-local-user'),
      accessories: Array.isArray(accessories)
        ? accessories.filter(isRecord).filter(item => typeof item.id === 'string').map(item => ({
            id: item.id as HomeCustomizationSnapshot['accessories'][number]['id'],
            equipped: item.equipped === true,
            owned: item.owned === true,
          }))
        : [],
      editions: [{ id: 'sunny-dollhouse', owned: true }],
      placements: Array.isArray(placements) ? placements as HouseLayoutPlacement[] : [],
      selectedEditionId: 'sunny-dollhouse',
    };
  }
}
