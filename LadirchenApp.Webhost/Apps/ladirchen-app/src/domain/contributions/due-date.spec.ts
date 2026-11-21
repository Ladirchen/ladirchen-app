import { describe, expect, it } from "vite-plus/test";
import { dueDateParts } from "./due-date";
import type { IanaTimeZone } from "@/domain/family/types";

const zurich = "Europe/Zurich" as IanaTimeZone;

describe("dueDateParts", () => {
  it("returns the remaining calendar days and localized deadline", () => {
    const currentTime = new Date("2026-11-22T20:00:00+01:00").getTime();

    expect(dueDateParts("2026-11-27", currentTime, zurich, "de-CH")).toEqual({
      date: "27.11.2026",
      daysRemaining: 5,
      weekday: "Freitag",
    });
  });

  it("uses the family time zone when calculating today", () => {
    const currentTime = new Date("2026-11-22T23:30:00Z").getTime();

    expect(dueDateParts("2026-11-23", currentTime, zurich, "de-CH")?.daysRemaining).toBe(0);
  });
});
