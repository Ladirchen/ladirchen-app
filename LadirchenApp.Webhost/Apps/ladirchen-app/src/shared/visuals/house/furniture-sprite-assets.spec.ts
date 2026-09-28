import { describe, expect, it } from "vitest";
import { FURNITURE_VISUAL_IDS } from "@/domain/house";
import { getFurnitureSpriteUrls } from "./furniture-sprite-assets";

describe("furniture sprite asset registry", () => {
  it.each(FURNITURE_VISUAL_IDS)('resolves a bundled sprite for "%s"', async (visual) => {
    const urls = getFurnitureSpriteUrls(visual);
    await expect(urls.closedUrl).resolves.toEqual(expect.any(String));
    if (urls.openUrl) {
      await expect(urls.openUrl).resolves.toEqual(expect.any(String));
    }
  });
});
