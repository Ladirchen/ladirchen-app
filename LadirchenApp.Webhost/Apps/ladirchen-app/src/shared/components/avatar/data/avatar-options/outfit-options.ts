import { AVATAR_OUTFIT_CATALOG, AVATAR_OUTFIT_COLOR_CATALOG, avatarOutfitColors, avatarPartsForProfile } from "@/domain/avatar";
import { defineAvatarColorOptions, defineAvatarOptions } from "./types";

export const outfitOptions = defineAvatarOptions(
  "outfit",
  avatarPartsForProfile(AVATAR_OUTFIT_CATALOG, "child"),
);

export const adultOutfitOptions = defineAvatarOptions(
  "outfit",
  avatarPartsForProfile(AVATAR_OUTFIT_CATALOG, "guardian"),
);

export const outfitColorOptions = defineAvatarColorOptions(
  "outfit-color",
  AVATAR_OUTFIT_COLOR_CATALOG,
  avatarOutfitColors,
);
