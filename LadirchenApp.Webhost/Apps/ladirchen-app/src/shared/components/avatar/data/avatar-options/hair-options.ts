import { AVATAR_HAIR_CATALOG, AVATAR_HAIR_COLOR_CATALOG, avatarHairColors, avatarPartsForProfile } from '@/domain/avatar';
import { defineAvatarColorOptions, defineAvatarOptions } from './types';

export const hairOptions = defineAvatarOptions(
  'hair',
  avatarPartsForProfile(AVATAR_HAIR_CATALOG, 'child'),
);

export const adultHairOptions = defineAvatarOptions(
  'hair',
  avatarPartsForProfile(AVATAR_HAIR_CATALOG, 'guardian'),
);

export const hairColorOptions = defineAvatarColorOptions(
  'hair-color',
  avatarPartsForProfile(AVATAR_HAIR_COLOR_CATALOG, 'child'),
  avatarHairColors,
);

export const adultHairColorOptions = defineAvatarColorOptions(
  'hair-color',
  avatarPartsForProfile(AVATAR_HAIR_COLOR_CATALOG, 'guardian'),
  avatarHairColors,
);
