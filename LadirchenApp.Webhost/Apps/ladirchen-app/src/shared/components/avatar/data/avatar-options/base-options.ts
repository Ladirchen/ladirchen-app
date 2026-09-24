import { AVATAR_SKIN_TONE_CATALOG, avatarSkinToneColors } from "@/domain/avatar";
import { defineAvatarColorOptions } from "./types";

export const skinToneOptions = defineAvatarColorOptions(
  "skin-tone",
  AVATAR_SKIN_TONE_CATALOG,
  avatarSkinToneColors,
);
