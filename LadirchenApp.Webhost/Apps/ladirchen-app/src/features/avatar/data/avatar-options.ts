import { avatarHairColors, avatarOutfitColors, avatarSkinToneColors } from '@/domain/avatar';
import type { AvatarAccessoryId, AvatarFace, AvatarFaceShape, AvatarFunAccessoryId, AvatarHair, AvatarHairColorId, AvatarOutfit, AvatarOutfitColorId, AvatarSeasonalAccessoryId, AvatarSkinToneId } from '@/domain/avatar';

export interface AvatarOption<T extends string> {
  id: AvatarCatalogItemId;
  value: T;
  label: string;
}

export interface AvatarColorOption<T extends string> extends AvatarOption<T> {
  color: string;
}

export type AvatarCatalogCategory = 'skin-tone' | 'face' | 'face-shape' | 'hair' | 'hair-color' | 'outfit' | 'outfit-color' | 'accessory' | 'fun-accessory' | 'seasonal-accessory';
export type AvatarCatalogItemId = `avatar.${AvatarCatalogCategory}.${string}`;

const defineAvatarOptions = <T extends string>(category: AvatarCatalogCategory, options: Array<Omit<AvatarOption<T>, 'id'>>): AvatarOption<T>[] =>
  options.map(option => ({ ...option, id: `avatar.${category}.${option.value}` }));

const defineAvatarColorOptions = <T extends string>(category: AvatarCatalogCategory, colors: Record<T, string>, ids: T[]): AvatarColorOption<T>[] =>
  ids.map((value, index) => ({ id: `avatar.${category}.${value}`, value, label: `Farbe ${index + 1}`, color: colors[value] }));

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
  { value: 'happy', label: 'Fröhlich' },
  { value: 'freckles', label: 'Sommersprossen' },
  { value: 'wink', label: 'Zwinkern' },
  { value: 'surprised', label: 'Überrascht' },
  { value: 'confident', label: 'Selbstbewusst' },
  { value: 'dreamy', label: 'Verträumt' },
  { value: 'silly', label: 'Quatschig' },
  { value: 'sparkle', label: 'Strahlend' },
]);

export const faceShapeOptions = defineAvatarOptions<AvatarFaceShape>('face-shape', [
  { value: 'soft', label: 'Sanft' },
  { value: 'round', label: 'Rund' },
  { value: 'oval', label: 'Oval' },
  { value: 'angular', label: 'Markant' },
]);

export const hairOptions = defineAvatarOptions<AvatarHair>('hair', [
  { value: 'short', label: 'Kurz' },
  { value: 'waves', label: 'Lange Wellen' },
  { value: 'ponytail', label: 'Pferdeschwanz' },
  { value: 'curls', label: 'Locken' },
  { value: 'afro', label: 'Afro' },
  { value: 'bob', label: 'Bob' },
  { value: 'braids', label: 'Zöpfe' },
  { value: 'bun', label: 'Dutt' },
  { value: 'space-buns', label: 'Space Buns' },
  { value: 'undercut', label: 'Undercut' },
]);

export const adultHairOptions: AvatarOption<AvatarHair>[] = hairOptions.filter(
  option => !['space-buns', 'braids'].includes(option.value),
);

export const outfitOptions = defineAvatarOptions<AvatarOutfit>('outfit', [
  { value: 'superhero', label: 'Superheld' },
  { value: 'dinosaur', label: 'Dinokostüm' },
  { value: 'monster', label: 'Monsterkostüm' },
  { value: 'vampire', label: 'Vampirkostüm' },
  { value: 'shark', label: 'Haikostüm' },
  { value: 'robot', label: 'Roboter' },
  { value: 'space', label: 'Astronaut' },
  { value: 'fairy', label: 'Flügelwesen' },
  { value: 'hoodie', label: 'Hoodie' },
  { value: 'overalls', label: 'Latzhose' },
  { value: 'explorer', label: 'Entdecker' },
  { value: 'party', label: 'Party' },
  { value: 'sporty', label: 'Sportlich' },
  { value: 'pajamas', label: 'Pyjama' },
]);

export const adultOutfitOptions = defineAvatarOptions<AvatarOutfit>('outfit', [
  { value: 'shirt', label: 'Hemd' },
  { value: 'blouse', label: 'Bluse' },
  { value: 'cardigan', label: 'Cardigan' },
  { value: 'blazer', label: 'Blazer' },
  { value: 'explorer', label: 'Freizeitjacke' },
  { value: 'sporty', label: 'Sportlich' },
  { value: 'party', label: 'Festlich' },
]);

export const accessoryOptions = defineAvatarOptions<AvatarAccessoryId>('accessory', [
  { value: 'none', label: 'Ohne' },
  { value: 'glasses', label: 'Brille' },
  { value: 'headphones', label: 'Kopfhörer' },
  { value: 'cat-ears', label: 'Katzenohren' },
  { value: 'cap', label: 'Kappe' },
  { value: 'crown', label: 'Krone' },
  { value: 'star-glasses', label: 'Sternenbrille' },
  { value: 'flower-crown', label: 'Blumenkrone' },
  { value: 'propeller-cap', label: 'Propellerkappe' },
]);

export const funOptions = defineAvatarOptions<AvatarFunAccessoryId>('fun-accessory', [
  { value: 'none', label: 'Ohne' },
  { value: 'mustache', label: 'Schnurrbart' },
  { value: 'whiskers', label: 'Schnurrhaare' },
  { value: 'rainbow', label: 'Regenbogen' },
  { value: 'clown-nose', label: 'Clownsnase' },
  { value: 'pirate', label: 'Piratenklappe' },
  { value: 'monster-horns', label: 'Monsterhörner' },
]);

export const seasonOptions = defineAvatarOptions<AvatarSeasonalAccessoryId>('seasonal-accessory', [
  { value: 'none', label: 'Ohne' },
  { value: 'witch', label: 'Hexenhut' },
  { value: 'pumpkin', label: 'Kürbis' },
  { value: 'santa', label: 'Weihnachtsmütze' },
  { value: 'reindeer', label: 'Rentiergeweih' },
  { value: 'bat', label: 'Fledermaus' },
  { value: 'elf', label: 'Wichtelmütze' },
  { value: 'snow-monster', label: 'Schneemonster' },
]);
