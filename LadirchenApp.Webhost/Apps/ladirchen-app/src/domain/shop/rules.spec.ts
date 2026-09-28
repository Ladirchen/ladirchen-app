import { describe, expect, it } from "vitest";

import { catalogNewBadgeIsActive } from "./rules";
import { createIanaTimeZone } from "@/domain/family/time-zone";

describe("catalogNewBadgeIsActive", () => {
  const schedule = { from: "2026-09-10", until: "2026-09-23" };
  const zurich = createIanaTimeZone("Europe/Zurich");

  it("includes the complete start and end calendar days", () => {
    expect(catalogNewBadgeIsActive(schedule, zurich, new Date("2026-09-09T22:00:00.000Z"))).toBe(true);
    expect(catalogNewBadgeIsActive(schedule, zurich, new Date("2026-09-23T21:59:59.999Z"))).toBe(true);
  });

  it("hides the badge before and after its schedule", () => {
    expect(catalogNewBadgeIsActive(schedule, zurich, new Date("2026-09-09T21:59:59.999Z"))).toBe(false);
    expect(catalogNewBadgeIsActive(schedule, zurich, new Date("2026-09-23T22:00:00.000Z"))).toBe(false);
    expect(catalogNewBadgeIsActive(undefined, zurich, new Date("2026-09-15T12:00:00.000Z"))).toBe(false);
  });
});
