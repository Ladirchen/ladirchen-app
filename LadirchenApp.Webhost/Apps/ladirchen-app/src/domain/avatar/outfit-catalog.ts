import { defineAvatarPart } from "./catalog";

const BUILT_IN_AVATAR_OUTFIT_CATALOG = [
  defineAvatarPart("superhero", ["child"]),
  defineAvatarPart("dinosaur", ["child"]),
  defineAvatarPart("monster", ["child"]),
  defineAvatarPart("vampire", ["child"]),
  defineAvatarPart("shark", ["child"]),
  defineAvatarPart("robot", ["child"]),
  defineAvatarPart("space", ["child"]),
  defineAvatarPart("fairy", ["child"]),
  defineAvatarPart("hoodie", ["child"]),
  defineAvatarPart("overalls", ["child"]),
  defineAvatarPart("explorer"),
  defineAvatarPart("party"),
  defineAvatarPart("sporty"),
  defineAvatarPart("pajamas", ["child"]),
  defineAvatarPart("shirt", ["guardian"]),
  defineAvatarPart("blouse", ["guardian"]),
  defineAvatarPart("cardigan", ["guardian"]),
  defineAvatarPart("blazer", ["guardian"]),
] as const;

export const AVATAR_OUTFIT_EXTENSION_CATALOG = [
  // design-generator:avatar-outfit-id
] as const;

export const AVATAR_OUTFIT_CATALOG = [
  ...BUILT_IN_AVATAR_OUTFIT_CATALOG,
  ...AVATAR_OUTFIT_EXTENSION_CATALOG,
] as const;

export type AvatarOutfit = typeof AVATAR_OUTFIT_CATALOG[number]["id"];
export type AvatarOutfitExtensionId = typeof AVATAR_OUTFIT_EXTENSION_CATALOG[number]["id"];
