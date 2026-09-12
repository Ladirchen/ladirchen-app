import type { ClientStorage } from '@/application/ports/client-storage';

const storage = (): Storage | undefined => {
  try {
    return typeof localStorage === 'undefined' ? undefined : localStorage;
  } catch {
    return undefined;
  }
};

export const browserClientStorage: ClientStorage = {
  getItem(key) {
    try {
      return storage()?.getItem(key) ?? null;
    } catch {
      return null;
    }
  },
  removeItem(key) {
    try {
      storage()?.removeItem(key);
    } catch {
      // Browser privacy settings may disable persistent storage.
    }
  },
  setItem(key, value) {
    try {
      storage()?.setItem(key, value);
    } catch {
      // The application remains usable with in-memory state only.
    }
  },
};
