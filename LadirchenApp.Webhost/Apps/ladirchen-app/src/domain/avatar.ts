import { avatarHairColorPalette, avatarOutfitColorPalette, avatarSkinTonePalette } from '@/theme/color-palette';

export type AvatarFace = 'happy' | 'freckles' | 'wink' | 'surprised' | 'confident' | 'dreamy' | 'silly' | 'sparkle';
export type AvatarFaceShape = 'soft' | 'round' | 'oval' | 'angular';
export type AvatarHair = 'short' | 'curls' | 'ponytail' | 'bob' | 'undercut' | 'bun' | 'braids' | 'waves' | 'afro' | 'space-buns';
export type AvatarAge = 'child' | 'adult' | 'senior';
export type AvatarOutfit = 'hoodie' | 'overalls' | 'explorer' | 'space' | 'party' | 'sporty' | 'pajamas' | 'superhero' | 'dinosaur' | 'monster' | 'vampire' | 'shark' | 'robot' | 'fairy' | 'blouse' | 'shirt' | 'cardigan' | 'blazer';
export type AvatarAccessoryId = 'none' | 'glasses' | 'headphones' | 'cat-ears' | 'cap' | 'crown' | 'star-glasses' | 'flower-crown' | 'propeller-cap';
export type AvatarFunAccessoryId = 'none' | 'mustache' | 'whiskers' | 'rainbow' | 'clown-nose' | 'pirate' | 'monster-horns';
export type AvatarSeasonalAccessoryId = 'none' | 'witch' | 'pumpkin' | 'santa' | 'reindeer' | 'bat' | 'elf' | 'snow-monster';

export const avatarSkinToneColors = avatarSkinTonePalette;
export const avatarHairColors = avatarHairColorPalette;
export const avatarOutfitColors = avatarOutfitColorPalette;

export type AvatarSkinToneId = keyof typeof avatarSkinToneColors;
export type AvatarHairColorId = keyof typeof avatarHairColors;
export type AvatarOutfitColorId = keyof typeof avatarOutfitColors;

export interface AvatarAppearance {
  age: AvatarAge;
  skinToneId: AvatarSkinToneId;
  faceShape: AvatarFaceShape;
  face: AvatarFace;
  hair: AvatarHair;
  hairColorId: AvatarHairColorId;
  outfit: AvatarOutfit;
  outfitColorId: AvatarOutfitColorId;
  accessoryId: AvatarAccessoryId;
  funAccessoryId: AvatarFunAccessoryId;
  seasonalAccessoryId: AvatarSeasonalAccessoryId;
}

export type PersistedAvatarAppearance = Partial<AvatarAppearance> & {
  accessory?: AvatarAccessoryId;
  fun?: AvatarFunAccessoryId;
  hairColor?: string;
  outfitColor?: string;
  season?: AvatarSeasonalAccessoryId;
  skinColor?: string;
};

const avatarOptionIds = {
  accessoryId: ['none', 'glasses', 'headphones', 'cat-ears', 'cap', 'crown', 'star-glasses', 'flower-crown', 'propeller-cap'],
  age: ['child', 'adult', 'senior'],
  face: ['happy', 'freckles', 'wink', 'surprised', 'confident', 'dreamy', 'silly', 'sparkle'],
  faceShape: ['soft', 'round', 'oval', 'angular'],
  funAccessoryId: ['none', 'mustache', 'whiskers', 'rainbow', 'clown-nose', 'pirate', 'monster-horns'],
  hair: ['short', 'curls', 'ponytail', 'bob', 'undercut', 'bun', 'braids', 'waves', 'afro', 'space-buns'],
  outfit: ['hoodie', 'overalls', 'explorer', 'space', 'party', 'sporty', 'pajamas', 'superhero', 'dinosaur', 'monster', 'vampire', 'shark', 'robot', 'fairy', 'blouse', 'shirt', 'cardigan', 'blazer'],
  seasonalAccessoryId: ['none', 'witch', 'pumpkin', 'santa', 'reindeer', 'bat', 'elf', 'snow-monster'],
} as const;

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null;
const isOptionalString = (value: unknown): boolean => value === undefined || typeof value === 'string';
const isOption = <TOption extends string>(value: unknown, options: readonly TOption[]): value is TOption =>
  typeof value === 'string' && options.some(option => option === value);
const isOptionalOption = (value: unknown, options: readonly string[]): boolean =>
  value === undefined || isOption(value, options);

export const isAvatarSkinToneId = (value: unknown): value is AvatarSkinToneId =>
  isOption(value, Object.keys(avatarSkinToneColors));
export const isAvatarHairColorId = (value: unknown): value is AvatarHairColorId =>
  isOption(value, Object.keys(avatarHairColors));
export const isAvatarOutfitColorId = (value: unknown): value is AvatarOutfitColorId =>
  isOption(value, Object.keys(avatarOutfitColors));

export const isAvatarAppearance = (value: unknown): value is AvatarAppearance =>
  isRecord(value) &&
  isOption(value.age, avatarOptionIds.age) &&
  isOption(value.face, avatarOptionIds.face) &&
  isOption(value.faceShape, avatarOptionIds.faceShape) &&
  isOption(value.hair, avatarOptionIds.hair) &&
  isOption(value.outfit, avatarOptionIds.outfit) &&
  isOption(value.accessoryId, avatarOptionIds.accessoryId) &&
  isOption(value.funAccessoryId, avatarOptionIds.funAccessoryId) &&
  isOption(value.seasonalAccessoryId, avatarOptionIds.seasonalAccessoryId) &&
  isAvatarSkinToneId(value.skinToneId) &&
  isAvatarHairColorId(value.hairColorId) &&
  isAvatarOutfitColorId(value.outfitColorId);

/** Accepts current appearances and the supported legacy colour/accessory fields. */
export const isPersistedAvatarAppearance = (value: unknown): value is PersistedAvatarAppearance =>
  isRecord(value) &&
  isOptionalOption(value.age, avatarOptionIds.age) &&
  isOptionalOption(value.face, avatarOptionIds.face) &&
  isOptionalOption(value.faceShape, avatarOptionIds.faceShape) &&
  isOptionalOption(value.hair, avatarOptionIds.hair) &&
  isOptionalOption(value.outfit, avatarOptionIds.outfit) &&
  isOptionalOption(value.accessoryId, avatarOptionIds.accessoryId) &&
  isOptionalOption(value.funAccessoryId, avatarOptionIds.funAccessoryId) &&
  isOptionalOption(value.seasonalAccessoryId, avatarOptionIds.seasonalAccessoryId) &&
  isOptionalOption(value.skinToneId, Object.keys(avatarSkinToneColors)) &&
  isOptionalOption(value.hairColorId, Object.keys(avatarHairColors)) &&
  isOptionalOption(value.outfitColorId, Object.keys(avatarOutfitColors)) &&
  isOptionalOption(value.accessory, avatarOptionIds.accessoryId) &&
  isOptionalOption(value.fun, avatarOptionIds.funAccessoryId) &&
  isOptionalOption(value.season, avatarOptionIds.seasonalAccessoryId) &&
  isOptionalString(value.skinColor) &&
  isOptionalString(value.hairColor) &&
  isOptionalString(value.outfitColor);

export const createDefaultAvatarAppearance = (): AvatarAppearance => ({
  age: 'child',
  skinToneId: 'skin-medium',
  faceShape: 'soft',
  face: 'happy',
  hair: 'ponytail',
  hairColorId: 'hair-brown',
  outfit: 'hoodie',
  outfitColorId: 'outfit-blue',
  accessoryId: 'none',
  funAccessoryId: 'none',
  seasonalAccessoryId: 'none',
});

const normalizeColorId = <T extends string>(
  currentId: T | undefined,
  legacyColor: string | undefined,
  colors: Record<T, string>,
  fallbackId: T,
  isColorId: (value: unknown) => value is T,
): T => {
  if (currentId) {return currentId;}
  const legacyId = Object.entries(colors).find(([, color]) => color === legacyColor)?.[0];
  return isColorId(legacyId) ? legacyId : fallbackId;
};

export const normalizeAvatarAppearance = (
  appearance?: PersistedAvatarAppearance,
  fallback: AvatarAppearance = createDefaultAvatarAppearance(),
): AvatarAppearance => {
  if (!appearance) {return fallback;}
  const { accessory, fun, hairColor, outfitColor, season, skinColor, ...currentAppearance } = appearance;
  return {
    ...fallback,
    ...currentAppearance,
    accessoryId: appearance.accessoryId ?? accessory ?? fallback.accessoryId,
    funAccessoryId: appearance.funAccessoryId ?? fun ?? fallback.funAccessoryId,
    hairColorId: normalizeColorId(appearance.hairColorId, hairColor, avatarHairColors, fallback.hairColorId, isAvatarHairColorId),
    outfitColorId: normalizeColorId(appearance.outfitColorId, outfitColor, avatarOutfitColors, fallback.outfitColorId, isAvatarOutfitColorId),
    seasonalAccessoryId: appearance.seasonalAccessoryId ?? season ?? fallback.seasonalAccessoryId,
    skinToneId: normalizeColorId(appearance.skinToneId, skinColor, avatarSkinToneColors, fallback.skinToneId, isAvatarSkinToneId),
  };
};

export type GuardianAvatarPreset = 'adult' | 'grandma' | 'grandpa';

const guardianAvatarPresets: Record<GuardianAvatarPreset, Partial<AvatarAppearance>> = {
  adult: {
    age: 'adult',
    face: 'happy',
    faceShape: 'soft',
    hair: 'short',
    hairColorId: 'hair-brown',
    outfit: 'shirt',
    outfitColorId: 'outfit-emerald',
    accessoryId: 'none',
  },
  grandma: {
    age: 'senior',
    face: 'confident',
    faceShape: 'soft',
    hair: 'bun',
    hairColorId: 'hair-white',
    outfit: 'cardigan',
    outfitColorId: 'outfit-lavender',
    accessoryId: 'glasses',
  },
  grandpa: {
    age: 'senior',
    face: 'confident',
    faceShape: 'angular',
    hair: 'short',
    hairColorId: 'hair-silver',
    outfit: 'blazer',
    outfitColorId: 'outfit-slate',
    accessoryId: 'glasses',
  },
};

export const createGuardianAvatarAppearance = (preset: GuardianAvatarPreset = 'adult'): AvatarAppearance => ({
  ...createDefaultAvatarAppearance(),
  ...guardianAvatarPresets[preset],
});
