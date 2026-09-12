import type { FamilyPetKindId } from '@/domain/types';

const legacyFamilyPetKindIds: Readonly<Record<string, FamilyPetKindId>> = {
  bird: 'bird',
  cat: 'cat',
  dog: 'dog',
  other: 'other',
  rabbit: 'rabbit',
  Vogel: 'bird',
  Katze: 'cat',
  Hund: 'dog',
  'Anderes Tier': 'other',
  Kaninchen: 'rabbit',
};

export const normalizeLegacyFamilyPetKind = (value: unknown): FamilyPetKindId =>
  typeof value === 'string' ? (legacyFamilyPetKindIds[value] ?? 'other') : 'other';
