export type AvatarFace = 'happy' | 'freckles' | 'wink' | 'surprised' | 'confident' | 'dreamy' | 'silly' | 'sparkle';
export type AvatarFaceShape = 'soft' | 'round' | 'oval' | 'angular';
export type AvatarHair = 'short' | 'curls' | 'ponytail' | 'bob' | 'undercut' | 'bun' | 'braids' | 'waves' | 'afro' | 'space-buns';
export type AvatarOutfit = 'hoodie' | 'overalls' | 'explorer' | 'space' | 'party' | 'sporty' | 'pajamas' | 'superhero' | 'dinosaur' | 'monster' | 'vampire' | 'shark' | 'robot' | 'fairy';
export type AvatarAccessory = 'none' | 'glasses' | 'headphones' | 'cat-ears' | 'cap' | 'crown' | 'star-glasses' | 'flower-crown' | 'propeller-cap';
export type AvatarFun = 'none' | 'mustache' | 'whiskers' | 'rainbow' | 'clown-nose' | 'pirate' | 'monster-horns';
export type AvatarSeason = 'none' | 'witch' | 'pumpkin' | 'santa' | 'reindeer' | 'bat' | 'elf' | 'snow-monster';

export interface AvatarAppearance {
  skinColor: string;
  faceShape: AvatarFaceShape;
  face: AvatarFace;
  hair: AvatarHair;
  hairColor: string;
  outfit: AvatarOutfit;
  outfitColor: string;
  accessory: AvatarAccessory;
  fun: AvatarFun;
  season: AvatarSeason;
}

export const createDefaultAvatarAppearance = (): AvatarAppearance => ({
  skinColor: '#efba91',
  faceShape: 'soft',
  face: 'happy',
  hair: 'ponytail',
  hairColor: '#69432b',
  outfit: 'hoodie',
  outfitColor: '#6f8df5',
  accessory: 'none',
  fun: 'none',
  season: 'none',
});
