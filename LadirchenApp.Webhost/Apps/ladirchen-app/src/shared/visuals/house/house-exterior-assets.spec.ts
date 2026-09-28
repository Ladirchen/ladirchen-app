import { describe, expect, it } from "vitest";

import { HOUSE_STAGES, HOUSE_THEME_IDS } from "@/domain/house";
import {
  getHouseExteriorAssetUrl,
  houseExteriorBackgroundAssetId,
  houseExteriorHouseAssetId,
} from "./house-exterior-assets";

describe("house exterior asset registry", () => {
  it.each(HOUSE_THEME_IDS.flatMap((themeId) => [1, 2, 3, 4, 5].map((level) => [themeId, level] as const)))(
    'resolves a bundled background for theme "%s" at energy level %s',
    async (themeId, level) => {
      const energy = { 1: 0, 2: 25, 3: 45, 4: 70, 5: 90 }[level]!;
      await expect(getHouseExteriorAssetUrl(houseExteriorBackgroundAssetId(themeId, energy))).resolves.toEqual(
        expect.any(String),
      );
    },
  );

  it.each(HOUSE_STAGES.flatMap((stage) => HOUSE_THEME_IDS.map((themeId) => [stage.id, themeId] as const)))(
    'resolves a bundled house for stage "%s" and theme "%s"',
    async (stageId, themeId) => {
      await expect(getHouseExteriorAssetUrl(houseExteriorHouseAssetId(stageId, themeId))).resolves.toEqual(
        expect.any(String),
      );
    },
  );
});
