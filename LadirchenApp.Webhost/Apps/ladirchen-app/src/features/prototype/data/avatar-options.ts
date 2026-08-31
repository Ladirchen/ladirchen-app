import type { AvatarAccessory, AvatarFace, AvatarFaceShape, AvatarFun, AvatarHair, AvatarOutfit, AvatarSeason } from '../domain/avatar';

export interface AvatarOption<T extends string> {
  value: T;
  label: string;
}

export const skinColors = ['#f8d6bd', '#efba91', '#c9865a', '#75452f'];
export const hairColors = ['#33251f', '#69432b', '#b66b32', '#e7c36c', '#7a55b2', '#3b8aaa'];
export const outfitColors = ['#6f8df5', '#42ad83', '#f07e70', '#e6a83f', '#8e67c4', '#394f68'];

export const faceOptions: AvatarOption<AvatarFace>[] = [
  { value: 'happy', label: 'Fröhlich' },
  { value: 'freckles', label: 'Sommersprossen' },
  { value: 'wink', label: 'Zwinkern' },
  { value: 'surprised', label: 'Überrascht' },
  { value: 'confident', label: 'Selbstbewusst' },
  { value: 'dreamy', label: 'Verträumt' },
  { value: 'silly', label: 'Quatschig' },
  { value: 'sparkle', label: 'Strahlend' },
];

export const faceShapeOptions: AvatarOption<AvatarFaceShape>[] = [
  { value: 'soft', label: 'Sanft' },
  { value: 'round', label: 'Rund' },
  { value: 'oval', label: 'Oval' },
  { value: 'angular', label: 'Markant' },
];

export const hairOptions: AvatarOption<AvatarHair>[] = [
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
];

export const outfitOptions: AvatarOption<AvatarOutfit>[] = [
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
];

export const accessoryOptions: AvatarOption<AvatarAccessory>[] = [
  { value: 'none', label: 'Ohne' },
  { value: 'glasses', label: 'Brille' },
  { value: 'headphones', label: 'Kopfhörer' },
  { value: 'cat-ears', label: 'Katzenohren' },
  { value: 'cap', label: 'Kappe' },
  { value: 'crown', label: 'Krone' },
  { value: 'star-glasses', label: 'Sternenbrille' },
  { value: 'flower-crown', label: 'Blumenkrone' },
  { value: 'propeller-cap', label: 'Propellerkappe' },
];

export const funOptions: AvatarOption<AvatarFun>[] = [
  { value: 'none', label: 'Ohne' },
  { value: 'mustache', label: 'Schnurrbart' },
  { value: 'whiskers', label: 'Schnurrhaare' },
  { value: 'rainbow', label: 'Regenbogen' },
  { value: 'clown-nose', label: 'Clownsnase' },
  { value: 'pirate', label: 'Piratenklappe' },
  { value: 'monster-horns', label: 'Monsterhörner' },
];

export const seasonOptions: AvatarOption<AvatarSeason>[] = [
  { value: 'none', label: 'Ohne' },
  { value: 'witch', label: 'Hexenhut' },
  { value: 'pumpkin', label: 'Kürbis' },
  { value: 'santa', label: 'Weihnachtsmütze' },
  { value: 'reindeer', label: 'Rentiergeweih' },
  { value: 'bat', label: 'Fledermaus' },
  { value: 'elf', label: 'Wichtelmütze' },
  { value: 'snow-monster', label: 'Schneemonster' },
];
