import { avatarHairColors, avatarOutfitColors, avatarSkinToneColors } from '@/domain/avatar';
import type { AvatarAccessoryId, AvatarFace, AvatarFaceShape, AvatarFunAccessoryId, AvatarHair, AvatarHairColorId, AvatarOutfit, AvatarOutfitColorId, AvatarSeasonalAccessoryId, AvatarSkinToneId } from '@/domain/avatar';

export interface AvatarOption<T extends string> {
  id: AvatarCatalogItemId;
  value: T;
}

export interface AvatarColorOption<T extends string> extends AvatarOption<T> {
  color: string;
}

export type AvatarCatalogCategory = 'skin-tone' | 'face' | 'face-shape' | 'hair' | 'hair-color' | 'outfit' | 'outfit-color' | 'accessory' | 'fun-accessory' | 'seasonal-accessory';
export type AvatarCatalogItemId = `avatar.${AvatarCatalogCategory}.${string}`;

const defineAvatarOptions = <T extends string>(category: AvatarCatalogCategory, values: T[]): AvatarOption<T>[] =>
  values.map(value => ({ id: `avatar.${category}.${value}`, value }));

const defineAvatarColorOptions = <T extends string>(category: AvatarCatalogCategory, colors: Record<T, string>, ids: T[]): AvatarColorOption<T>[] =>
  ids.map(value => ({ id: `avatar.${category}.${value}`, value, color: colors[value] }));

export const skinToneOptions = defineAvatarColorOptions<AvatarSkinToneId>('skin-tone', avatarSkinToneColors, [
  'skin-light', 'skin-medium', 'skin-tan', 'skin-deep',
]);

export const hairColorOptions = defineAvatarColorOptions<AvatarHairColorId>('hair-color', avatarHairColors, [
  'hair-black', 'hair-brown', 'hair-auburn', 'hair-blonde', 'hair-purple', 'hair-blue',
]);

export const adultHairColorOptions = defineAvatarColorOptions<AvatarHairColorId>('hair-color', avatarHairColors, [
  'hair-black', 'hair-brown', 'hair-auburn', 'hair-blonde', 'hair-gray', 'hair-silver', 'hair-white',
]);

export const outfitColorOptions = defineAvatarColorOptions<AvatarOutfitColorId>('outfit-color', avatarOutfitColors, [
  'outfit-blue', 'outfit-green', 'outfit-coral', 'outfit-gold', 'outfit-purple', 'outfit-navy',
]);

export const faceOptions = defineAvatarOptions<AvatarFace>('face', [
  'happy',
  'freckles',
  'wink',
  'surprised',
  'confident',
  'dreamy',
  'silly',
  'sparkle',
]);

export const faceShapeOptions = defineAvatarOptions<AvatarFaceShape>('face-shape', [
  'soft',
  'round',
  'oval',
  'angular',
]);

export const hairOptions = defineAvatarOptions<AvatarHair>('hair', [
  'short',
  'waves',
  'ponytail',
  'curls',
  'afro',
  'bob',
  'braids',
  'bun',
  'space-buns',
  'undercut',
]);

export const adultHairOptions: AvatarOption<AvatarHair>[] = hairOptions.filter(
  option => !['space-buns', 'braids'].includes(option.value),
);

export const outfitOptions = defineAvatarOptions<AvatarOutfit>('outfit', [
  'superhero',
  'dinosaur',
  'monster',
  'vampire',
  'shark',
  'robot',
  'space',
  'fairy',
  'hoodie',
  'overalls',
  'explorer',
  'party',
  'sporty',
  'pajamas',
]);

export const adultOutfitOptions = defineAvatarOptions<AvatarOutfit>('outfit', [
  'shirt',
  'blouse',
  'cardigan',
  'blazer',
  'explorer',
  'sporty',
  'party',
]);

export const accessoryOptions = defineAvatarOptions<AvatarAccessoryId>('accessory', [
  'none',
  'glasses',
  'headphones',
  'cat-ears',
  'cap',
  'crown',
  'star-glasses',
  'flower-crown',
  'propeller-cap',
]);

export const funOptions = defineAvatarOptions<AvatarFunAccessoryId>('fun-accessory', [
  'none',
  'mustache',
  'whiskers',
  'rainbow',
  'clown-nose',
  'pirate',
  'monster-horns',
]);

export const seasonOptions = defineAvatarOptions<AvatarSeasonalAccessoryId>('seasonal-accessory', [
  'none',
  'witch',
  'pumpkin',
  'santa',
  'reindeer',
  'bat',
  'elf',
  'snow-monster',
]);
