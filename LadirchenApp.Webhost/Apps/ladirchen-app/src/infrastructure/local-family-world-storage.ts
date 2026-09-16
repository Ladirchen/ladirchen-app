import type { FamilyWorldPersistence } from '../application/ports/family-world-persistence';

const storageAvailable = () => typeof localStorage !== 'undefined';

export const FAMILY_WORLD_STORAGE_KEYS = {
  exchangeRate: 'ladirchen-exchange-rate',
  familyCurrency: 'ladirchen-family-currency',
  familyMembers: 'ladirchen-family-members',
  familyPets: 'ladirchen-family-pets',
  familySetup: 'ladirchen-family-setup',
  houseAccessories: 'ladirchen-house-accessories',
  houseLayout: 'ladirchen-house-layout',
  pendingGifts: 'ladirchen-pending-gifts',
} as const;

export const localFamilyWorldPersistence: FamilyWorldPersistence = {
  readList<T>(key: string, fallback: () => T[]): T[] {
    if (!storageAvailable()) {
      return fallback();
    }

    try {
      const stored = JSON.parse(localStorage.getItem(key) ?? 'null');
      return Array.isArray(stored) && stored.length > 0 ? stored as T[] : fallback();
    } catch {
      return fallback();
    }
  },
  readNumber(key: string, fallback: number): number {
    const value = storageAvailable() ? localStorage.getItem(key) : null;
    if (value === null) {
      return fallback;
    }

    const stored = Number(value);
    return Number.isFinite(stored) ? stored : fallback;
  },
  readText(key: string): string | null {
    return storageAvailable() ? localStorage.getItem(key) : null;
  },
  writeList<T>(key: string, value: T[]): void {
    if (storageAvailable()) {
      localStorage.setItem(key, JSON.stringify(value));
    }
  },
  writeText(key: string, value: string): void {
    if (storageAvailable()) {
      localStorage.setItem(key, value);
    }
  },
};
