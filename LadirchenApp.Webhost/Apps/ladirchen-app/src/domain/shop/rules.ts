import type { CatalogNewBadgeSchedule } from "@/domain/house";
import type { ShopReward } from "./types";
import type { IanaTimeZone } from "@/domain/family/time-zone";
import { calendarDateInTimeZone, calendarDateIsWithin, zonedCalendarParts } from "@/domain/shared/zoned-calendar";
export const SHOP_REDEMPTION_CUTOFF_HOUR = 18;
export function shopRedemptionIsOpen(timeZone: IanaTimeZone, date = new Date()): boolean {

  return zonedCalendarParts(date, timeZone).hour < SHOP_REDEMPTION_CUTOFF_HOUR;
}
export function shopRewardIsPublished(reward: ShopReward, timeZone: IanaTimeZone, date = new Date()): boolean {

  return (
    reward.isVisible !== false &&
    calendarDateIsWithin(calendarDateInTimeZone(date, timeZone), reward.availableFrom, reward.availableUntil)
  );
}
export function catalogNewBadgeIsActive(
  schedule: CatalogNewBadgeSchedule | undefined,
  timeZone: IanaTimeZone,
  date = new Date(),
): boolean {

  return (
    schedule !== undefined &&
    calendarDateIsWithin(calendarDateInTimeZone(date, timeZone), schedule.from, schedule.until)
  );
}
