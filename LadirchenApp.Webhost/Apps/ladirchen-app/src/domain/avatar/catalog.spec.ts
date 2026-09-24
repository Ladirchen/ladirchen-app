import { describe, expect, it } from "vitest";

import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from "./appearance";
import { avatarPartsForProfile } from "./catalog";
import { AVATAR_HAIR_CATALOG } from "./hair-catalog";
import { AVATAR_OUTFIT_CATALOG } from "./outfit-catalog";
import { isAvatarAppearance } from "./validation";

const idsForProfile = (
  catalog: Parameters<typeof avatarPartsForProfile>[0],
  profile: Parameters<typeof avatarPartsForProfile>[1],
) => avatarPartsForProfile(catalog, profile).map(part => part.id);

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

  it("keeps every configured default and guardian preset runtime-valid", () => {
    expect(isAvatarAppearance(createDefaultAvatarAppearance())).toBe(true);
    expect(isAvatarAppearance(createGuardianAvatarAppearance("adult"))).toBe(true);
    expect(isAvatarAppearance(createGuardianAvatarAppearance("grandma"))).toBe(true);
    expect(isAvatarAppearance(createGuardianAvatarAppearance("grandpa"))).toBe(true);
  });
});
