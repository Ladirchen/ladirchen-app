import { defineAvatarPart } from "./catalog";

export const AVATAR_ACCESSORY_CATALOG = [
  defineAvatarPart("none"),
  defineAvatarPart("glasses"),
  defineAvatarPart("headphones"),
  defineAvatarPart("cat-ears"),
  defineAvatarPart("cap"),
  defineAvatarPart("crown"),
  defineAvatarPart("star-glasses"),
  defineAvatarPart("flower-crown"),
  defineAvatarPart("propeller-cap"),
  // design-generator:avatar-accessory-id
] as const;

export type AvatarAccessoryId = typeof AVATAR_ACCESSORY_CATALOG[number]["id"];

const BUILT_IN_AVATAR_FUN_ACCESSORY_CATALOG = [
  defineAvatarPart("none"),
  defineAvatarPart("mustache"),
  defineAvatarPart("whiskers"),
  defineAvatarPart("rainbow"),
  defineAvatarPart("clown-nose"),
  defineAvatarPart("pirate"),
  defineAvatarPart("monster-horns"),
] as const;

export const AVATAR_FUN_ACCESSORY_EXTENSION_CATALOG = [
  // design-generator:avatar-fun-accessory-id
] as const;

export const AVATAR_FUN_ACCESSORY_CATALOG = [
  ...BUILT_IN_AVATAR_FUN_ACCESSORY_CATALOG,
  ...AVATAR_FUN_ACCESSORY_EXTENSION_CATALOG,
] as const;

export type AvatarFunAccessoryId = typeof AVATAR_FUN_ACCESSORY_CATALOG[number]["id"];
export type AvatarFunAccessoryExtensionId = typeof AVATAR_FUN_ACCESSORY_EXTENSION_CATALOG[number]["id"];

const BUILT_IN_AVATAR_SEASONAL_ACCESSORY_CATALOG = [
  defineAvatarPart("none"),
  defineAvatarPart("witch"),
  defineAvatarPart("pumpkin"),
  defineAvatarPart("santa"),
  defineAvatarPart("reindeer"),
  defineAvatarPart("bat"),
  defineAvatarPart("elf"),
  defineAvatarPart("snow-monster"),
] as const;

export const AVATAR_SEASONAL_ACCESSORY_EXTENSION_CATALOG = [
  // design-generator:avatar-seasonal-accessory-id
] as const;

export const AVATAR_SEASONAL_ACCESSORY_CATALOG = [
  ...BUILT_IN_AVATAR_SEASONAL_ACCESSORY_CATALOG,
  ...AVATAR_SEASONAL_ACCESSORY_EXTENSION_CATALOG,
] as const;

export type AvatarSeasonalAccessoryId = typeof AVATAR_SEASONAL_ACCESSORY_CATALOG[number]["id"];
export type AvatarSeasonalAccessoryExtensionId = typeof AVATAR_SEASONAL_ACCESSORY_EXTENSION_CATALOG[number]["id"];
