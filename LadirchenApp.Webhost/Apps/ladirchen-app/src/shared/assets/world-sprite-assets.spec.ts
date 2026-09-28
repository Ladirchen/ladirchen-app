import { describe, expect, it } from "vitest";
import { getWorldDecorationSpriteUrl } from "./world-sprite-assets";
import type { WorldDecorationSpriteId } from "./world-sprite-assets";

const WORLD_DECORATION_SPRITE_IDS: WorldDecorationSpriteId[] = ["guide-branch", "ladi-perch"];

describe("world decoration sprite asset registry", () => {
  it.each(WORLD_DECORATION_SPRITE_IDS)('resolves a bundled sprite for "%s"', async (id) => {
    await expect(getWorldDecorationSpriteUrl(id)).resolves.toEqual(expect.any(String));
  });
});
