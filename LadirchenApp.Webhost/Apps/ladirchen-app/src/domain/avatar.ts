export type AvatarFace = 'happy' | 'freckles' | 'wink' | 'surprised' | 'confident' | 'dreamy' | 'silly' | 'sparkle';
export type AvatarFaceShape = 'soft' | 'round' | 'oval' | 'angular';
export type AvatarHair = 'short' | 'curls' | 'ponytail' | 'bob' | 'undercut' | 'bun' | 'braids' | 'waves' | 'afro' | 'space-buns';
export type AvatarAge = 'child' | 'adult' | 'senior';
export type AvatarOutfit = 'hoodie' | 'overalls' | 'explorer' | 'space' | 'party' | 'sporty' | 'pajamas' | 'superhero' | 'dinosaur' | 'monster' | 'vampire' | 'shark' | 'robot' | 'fairy' | 'blouse' | 'shirt' | 'cardigan' | 'blazer';
export type AvatarAccessoryId = 'none' | 'glasses' | 'headphones' | 'cat-ears' | 'cap' | 'crown' | 'star-glasses' | 'flower-crown' | 'propeller-cap';
export type AvatarFunAccessoryId = 'none' | 'mustache' | 'whiskers' | 'rainbow' | 'clown-nose' | 'pirate' | 'monster-horns';
export type AvatarSeasonalAccessoryId = 'none' | 'witch' | 'pumpkin' | 'santa' | 'reindeer' | 'bat' | 'elf' | 'snow-monster';

export const avatarSkinToneColors = {
  'skin-light': '#f8d6bd',
  'skin-medium': '#efba91',
  'skin-tan': '#c9865a',
  'skin-deep': '#75452f',
} as const;

export const avatarHairColors = {
  'hair-black': '#33251f',
  'hair-brown': '#69432b',
  'hair-auburn': '#b66b32',
  'hair-blonde': '#e7c36c',
  'hair-purple': '#7a55b2',
  'hair-blue': '#3b8aaa',
  'hair-gray': '#817d78',
  'hair-silver': '#aaa69f',
  'hair-white': '#dedbd2',
} as const;

export const avatarOutfitColors = {
  'outfit-blue': '#6f8df5',
  'outfit-green': '#42ad83',
  'outfit-coral': '#f07e70',
  'outfit-gold': '#e6a83f',
  'outfit-purple': '#8e67c4',
  'outfit-navy': '#394f68',
  'outfit-lavender': '#9a78b8',
  'outfit-slate': '#557187',
  'outfit-emerald': '#4d9c7c',
  'outfit-rose': '#dd7b91',
  'outfit-mint': '#58aa82',
  'outfit-ocean': '#3b8aaa',
} as const;

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
): T => {
  if (currentId) {return currentId;}
  const legacyId = Object.entries(colors).find(([, color]) => color === legacyColor)?.[0] as T | undefined;
  return legacyId ?? fallbackId;
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
    hairColorId: normalizeColorId(appearance.hairColorId, hairColor, avatarHairColors, fallback.hairColorId),
    outfitColorId: normalizeColorId(appearance.outfitColorId, outfitColor, avatarOutfitColors, fallback.outfitColorId),
    seasonalAccessoryId: appearance.seasonalAccessoryId ?? season ?? fallback.seasonalAccessoryId,
    skinToneId: normalizeColorId(appearance.skinToneId, skinColor, avatarSkinToneColors, fallback.skinToneId),
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
