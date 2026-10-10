import {
  AVATAR_ACCESSORY_CATALOG,
  AVATAR_FUN_ACCESSORY_CATALOG,
  AVATAR_SEASONAL_ACCESSORY_CATALOG,
} from "./accessory-catalog";
import { AVATAR_AGES } from "./appearance";
import { avatarHairColors, avatarOutfitColors, avatarSkinToneColors } from "./color-catalog";
import { AVATAR_FACE_CATALOG, AVATAR_FACE_SHAPE_CATALOG } from "./face-catalog";
import { AVATAR_HAIR_CATALOG } from "./hair-catalog";
import { AVATAR_OUTFIT_CATALOG } from "./outfit-catalog";
import type { AvatarAppearance } from "./appearance";
import type { AvatarHairColorId, AvatarOutfitColorId, AvatarSkinToneId } from "./color-catalog";
function isRecord(value: unknown): value is Record<string, unknown> {

  return typeof value === "object" && value !== null;
}
function isOption<TOption extends string>(value: unknown, options: readonly TOption[]): value is TOption {

  return typeof value === "string" && options.some((option) => option === value);
}
function catalogIds<Id extends string>(
  catalog: readonly {
    readonly id: Id;
  }[],
): Id[] {

  return catalog.map((entry) => entry.id);
}
export function isAvatarSkinToneId(value: unknown): value is AvatarSkinToneId {

  return isOption(value, Object.keys(avatarSkinToneColors));
}
export function isAvatarHairColorId(value: unknown): value is AvatarHairColorId {

  return isOption(value, Object.keys(avatarHairColors));
}
export function isAvatarOutfitColorId(value: unknown): value is AvatarOutfitColorId {

  return isOption(value, Object.keys(avatarOutfitColors));
}
export function isAvatarAppearance(value: unknown): value is AvatarAppearance {

  return (
    isRecord(value) &&
    isOption(value.age, AVATAR_AGES) &&
    isOption(value.face, catalogIds(AVATAR_FACE_CATALOG)) &&
    isOption(value.faceShape, catalogIds(AVATAR_FACE_SHAPE_CATALOG)) &&
    isOption(value.hair, catalogIds(AVATAR_HAIR_CATALOG)) &&
    isOption(value.outfit, catalogIds(AVATAR_OUTFIT_CATALOG)) &&
    isOption(value.accessoryId, catalogIds(AVATAR_ACCESSORY_CATALOG)) &&
    isOption(value.funAccessoryId, catalogIds(AVATAR_FUN_ACCESSORY_CATALOG)) &&
    isOption(value.seasonalAccessoryId, catalogIds(AVATAR_SEASONAL_ACCESSORY_CATALOG)) &&
    isAvatarSkinToneId(value.skinToneId) &&
    isAvatarHairColorId(value.hairColorId) &&
    isAvatarOutfitColorId(value.outfitColorId)
  );
}
