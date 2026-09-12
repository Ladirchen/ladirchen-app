import type { FamilyContext } from '@/application/ports/family-context';
import { createDomainId, isUuidValue } from '@/domain/shared/identifiers';
import { browserClientStorage } from '@/infrastructure/storage/browser-client-storage';

const ACTIVE_FAMILY_ID_KEY = 'ladirchen-active-family-id';

let inMemoryFamilyId: ReturnType<typeof createDomainId.family> | null = null;

const generateFamilyId = () => createDomainId.family(globalThis.crypto.randomUUID());

const resolveActiveFamilyId = () => {
  if (inMemoryFamilyId !== null) {
    return inMemoryFamilyId;
  }
  try {
    const storedFamilyId = browserClientStorage.getItem(ACTIVE_FAMILY_ID_KEY);
    if (isUuidValue(storedFamilyId)) {
      inMemoryFamilyId = createDomainId.family(storedFamilyId);
      return inMemoryFamilyId;
    }
    inMemoryFamilyId = generateFamilyId();
    browserClientStorage.setItem(ACTIVE_FAMILY_ID_KEY, inMemoryFamilyId);
  } catch {
    inMemoryFamilyId = generateFamilyId();
  }
  return inMemoryFamilyId;
};

export type LocalFamilyContext = FamilyContext;

export const localFamilyContext: LocalFamilyContext = {
  get activeFamilyId() {
    return resolveActiveFamilyId();
  },
  setActiveFamilyId(familyId) {
    inMemoryFamilyId = familyId;
    browserClientStorage.setItem(ACTIVE_FAMILY_ID_KEY, familyId);
  },
};
