import { describe, expect, it } from "vitest";
import { LADI_STAGES } from "@/domain/ladi";
import type { LadiSpriteId } from "@/domain/ladi";
import { getLadiSpriteUrl } from "./ladi-sprite-assets";

const LADI_SPRITE_IDS: LadiSpriteId[] = [...LADI_STAGES.map((stage) => stage.id), "perched-ladi", "smart-ladi"];

describe("ladi sprite asset registry", () => {
  it.each(LADI_SPRITE_IDS)('resolves a bundled sprite for "%s"', async (id) => {
    await expect(getLadiSpriteUrl(id)).resolves.toEqual(expect.any(String));
  });
});
