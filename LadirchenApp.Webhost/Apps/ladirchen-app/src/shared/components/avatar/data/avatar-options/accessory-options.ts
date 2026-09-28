import { AVATAR_ACCESSORY_CATALOG, AVATAR_FUN_ACCESSORY_CATALOG, AVATAR_SEASONAL_ACCESSORY_CATALOG } from "@/domain/avatar";
import { defineAvatarOptions } from "./types";

export const accessoryOptions = defineAvatarOptions("accessory", AVATAR_ACCESSORY_CATALOG);
export const funOptions = defineAvatarOptions("fun-accessory", AVATAR_FUN_ACCESSORY_CATALOG);
export const seasonOptions = defineAvatarOptions("seasonal-accessory", AVATAR_SEASONAL_ACCESSORY_CATALOG);
