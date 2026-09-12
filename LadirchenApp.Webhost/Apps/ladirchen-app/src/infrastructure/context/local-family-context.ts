import type { FamilyContext } from '@/application/ports/family-context';
import { createDomainId } from '@/domain/types';

const ACTIVE_FAMILY_ID_KEY = 'ladirchen-active-family-id';
const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/iu;

let inMemoryFamilyId: ReturnType<typeof createDomainId.family> | null = null;

const generateFamilyId = () => createDomainId.family(globalThis.crypto.randomUUID());

const resolveActiveFamilyId = () => {
  if (inMemoryFamilyId !== null) {
    return inMemoryFamilyId;
  }
  try {
    const storedFamilyId = localStorage.getItem(ACTIVE_FAMILY_ID_KEY);
    if (storedFamilyId && UUID_PATTERN.test(storedFamilyId)) {
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

export const localFamilyContext: FamilyContext = {
  get activeFamilyId() {
    return resolveActiveFamilyId();
  },
};
