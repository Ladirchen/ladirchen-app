import { describe, expect, it } from "vite-plus/test";

import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from "./appearance";
import { AVATAR_ACCESSORY_CATALOG } from "./accessory-catalog";
import { avatarPartsForProfile } from "./catalog";
import { AVATAR_HAIR_CATALOG } from "./hair-catalog";
import { AVATAR_OUTFIT_CATALOG } from "./outfit-catalog";
import { isAvatarAppearance } from "./validation";

function idsForProfile(
  catalog: Parameters<typeof avatarPartsForProfile>[0],
  profile: Parameters<typeof avatarPartsForProfile>[1],
) {
  return avatarPartsForProfile(catalog, profile).map((part) => part.id);
}

describe("avatar catalogs", () => {
  it("derives child and guardian hair choices from catalog availability", () => {
    expect(idsForProfile(AVATAR_HAIR_CATALOG, "child")).toContain("braids");
    expect(idsForProfile(AVATAR_HAIR_CATALOG, "guardian")).not.toContain("braids");
    expect(idsForProfile(AVATAR_HAIR_CATALOG, "guardian")).toContain("short");
  });

  it("keeps shared and profile-specific outfits in one source of truth", () => {
    expect(idsForProfile(AVATAR_OUTFIT_CATALOG, "child")).toEqual(expect.arrayContaining(["superhero", "explorer"]));
    expect(idsForProfile(AVATAR_OUTFIT_CATALOG, "child")).not.toContain("blazer");
    expect(idsForProfile(AVATAR_OUTFIT_CATALOG, "guardian")).toEqual(expect.arrayContaining(["blazer", "explorer"]));
    expect(idsForProfile(AVATAR_OUTFIT_CATALOG, "guardian")).not.toContain("superhero");
  });

  it("offers the popstar and time-forest collections only to children", () => {
    const childAccessories = idsForProfile(AVATAR_ACCESSORY_CATALOG, "child");
    const guardianAccessories = idsForProfile(AVATAR_ACCESSORY_CATALOG, "guardian");
    const childHair = idsForProfile(AVATAR_HAIR_CATALOG, "child");
    const childOutfits = idsForProfile(AVATAR_OUTFIT_CATALOG, "child");

    expect(childAccessories).toEqual(
      expect.arrayContaining(["gemstone-earrings", "headset-microphone", "fantasy-ears", "blue-ocarina"]),
    );
    expect(guardianAccessories).not.toEqual(
      expect.arrayContaining(["gemstone-earrings", "headset-microphone", "fantasy-ears", "blue-ocarina"]),
    );
    expect(childHair).toEqual(expect.arrayContaining(["popstar-space-buns", "forest-braid"]));
    expect(childOutfits).toEqual(expect.arrayContaining(["glitter-stage-jacket", "forest-adventurer"]));
  });

  it("keeps every configured default and guardian preset runtime-valid", () => {
    expect(isAvatarAppearance(createDefaultAvatarAppearance())).toBe(true);
    expect(isAvatarAppearance(createGuardianAvatarAppearance("adult"))).toBe(true);
    expect(isAvatarAppearance(createGuardianAvatarAppearance("grandma"))).toBe(true);
    expect(isAvatarAppearance(createGuardianAvatarAppearance("grandpa"))).toBe(true);
  });
});
