import { avatarHairColorPalette, avatarOutfitColorPalette, avatarSkinTonePalette } from "@/theme/color-palette";
import { defineAvatarPart } from "./catalog";
import type { AvatarPartDefinition } from "./catalog";

export const avatarSkinToneColors = avatarSkinTonePalette;
export const avatarHairColors = avatarHairColorPalette;
export const avatarOutfitColors = avatarOutfitColorPalette;

export type AvatarSkinToneId = keyof typeof avatarSkinToneColors;
export type AvatarHairColorId = keyof typeof avatarHairColors;
export type AvatarOutfitColorId = keyof typeof avatarOutfitColors;

export const AVATAR_SKIN_TONE_CATALOG = [
  defineAvatarPart("skin-light"),
  defineAvatarPart("skin-medium"),
  defineAvatarPart("skin-tan"),
  defineAvatarPart("skin-deep"),
] as const satisfies readonly AvatarPartDefinition<AvatarSkinToneId>[];

export const AVATAR_HAIR_COLOR_CATALOG = [
  defineAvatarPart("hair-black"),
  defineAvatarPart("hair-brown"),
  defineAvatarPart("hair-auburn"),
  defineAvatarPart("hair-blonde"),
  defineAvatarPart("hair-purple", ["child"]),
  defineAvatarPart("hair-blue", ["child"]),
  defineAvatarPart("hair-gray", ["guardian"]),
  defineAvatarPart("hair-silver", ["guardian"]),
  defineAvatarPart("hair-white", ["guardian"]),
] as const satisfies readonly AvatarPartDefinition<AvatarHairColorId>[];

export const AVATAR_OUTFIT_COLOR_CATALOG = [
  defineAvatarPart("outfit-blue"),
  defineAvatarPart("outfit-green"),
  defineAvatarPart("outfit-coral"),
  defineAvatarPart("outfit-gold"),
  defineAvatarPart("outfit-purple"),
  defineAvatarPart("outfit-navy"),
] as const satisfies readonly AvatarPartDefinition<AvatarOutfitColorId>[];
