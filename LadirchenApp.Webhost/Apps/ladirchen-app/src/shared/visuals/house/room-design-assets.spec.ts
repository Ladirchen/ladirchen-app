import { describe, expect, it } from "vitest";
import { ROOM_BACKGROUND_ASSET_IDS } from "@/domain/house/room-designs";
import { getRoomDesignBackgroundUrl } from "./room-design-assets";

describe("room design asset registry", () => {
  it.each(ROOM_BACKGROUND_ASSET_IDS)('resolves a bundled background for "%s"', async (id) => {
    await expect(getRoomDesignBackgroundUrl(id)).resolves.toEqual(expect.any(String));
  });
});
