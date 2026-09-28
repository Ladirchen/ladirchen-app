import { describe, expect, it } from "vitest";

import { createIanaTimeZone } from "@/domain/family/time-zone";
import { remainingPromotionMilliseconds } from "./promotions";

describe("promotion timing", () => {
  const zurich = createIanaTimeZone("Europe/Zurich");

  it("calculates the remaining time in the family time zone", () => {
    const now = new Date("2026-09-24T16:30:15.250Z");

    expect(remainingPromotionMilliseconds("19:00", zurich, now)).toBe(1_784_750);
  });

  it("returns zero after the deadline", () => {
    const now = new Date("2026-09-24T17:00:00.001Z");

    expect(remainingPromotionMilliseconds("19:00", zurich, now)).toBe(0);
  });
});
