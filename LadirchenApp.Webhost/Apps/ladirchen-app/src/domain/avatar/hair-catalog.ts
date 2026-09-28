import { defineAvatarPart } from "./catalog";

const BUILT_IN_AVATAR_HAIR_CATALOG = [
  defineAvatarPart("short"),
  defineAvatarPart("waves"),
  defineAvatarPart("ponytail"),
  defineAvatarPart("curls"),
  defineAvatarPart("afro"),
  defineAvatarPart("bob"),
  defineAvatarPart("braids", ["child"]),
  defineAvatarPart("bun"),
  defineAvatarPart("space-buns", ["child"]),
  defineAvatarPart("undercut"),
] as const;

export const AVATAR_HAIR_EXTENSION_CATALOG = [
  // design-generator:avatar-hair-id
] as const;

export const AVATAR_HAIR_CATALOG = [
  ...BUILT_IN_AVATAR_HAIR_CATALOG,
  ...AVATAR_HAIR_EXTENSION_CATALOG,
] as const;

export type AvatarHair = typeof AVATAR_HAIR_CATALOG[number]["id"];
export type AvatarHairExtensionId = typeof AVATAR_HAIR_EXTENSION_CATALOG[number]["id"];
