import type { FamilyContext } from '@/application/ports/family-context';
import type { FamilyId } from '@/domain/types';
import { createDomainId, isUuidValue } from '@/domain/types';

const ACTIVE_FAMILY_ID_KEY = 'ladirchen-active-family-id';

let inMemoryFamilyId: ReturnType<typeof createDomainId.family> | null = null;

const generateFamilyId = () => createDomainId.family(globalThis.crypto.randomUUID());

const resolveActiveFamilyId = () => {
  if (inMemoryFamilyId !== null) {
    return inMemoryFamilyId;
  }
  try {
    const storedFamilyId = localStorage.getItem(ACTIVE_FAMILY_ID_KEY);
    if (isUuidValue(storedFamilyId)) {
      inMemoryFamilyId = createDomainId.family(storedFamilyId);
      return inMemoryFamilyId;
    }
    inMemoryFamilyId = generateFamilyId();
    localStorage.setItem(ACTIVE_FAMILY_ID_KEY, inMemoryFamilyId);
  } catch {
    inMemoryFamilyId = generateFamilyId();
  }
  return inMemoryFamilyId;
};

export interface LocalFamilyContext extends FamilyContext {
  setActiveFamilyId: (familyId: FamilyId) => void;
}

export const localFamilyContext: LocalFamilyContext = {
  get activeFamilyId() {
    return resolveActiveFamilyId();
  },
  setActiveFamilyId(familyId) {
    inMemoryFamilyId = familyId;
    localStorage.setItem(ACTIVE_FAMILY_ID_KEY, familyId);
  },
};
