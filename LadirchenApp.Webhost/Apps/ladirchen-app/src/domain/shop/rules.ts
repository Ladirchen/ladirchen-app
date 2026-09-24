import type { CatalogNewBadgeSchedule } from "@/domain/house";
import type { ShopReward } from "./types";
import type { IanaTimeZone } from "@/domain/family/time-zone";
import { calendarDateInTimeZone, calendarDateIsWithin, zonedCalendarParts } from "@/domain/shared/zoned-calendar";

export const SHOP_REDEMPTION_CUTOFF_HOUR = 18;

export const shopRedemptionIsOpen = (timeZone: IanaTimeZone, date = new Date()): boolean =>
  zonedCalendarParts(date, timeZone).hour < SHOP_REDEMPTION_CUTOFF_HOUR;

export const shopRewardIsPublished = (reward: ShopReward, timeZone: IanaTimeZone, date = new Date()): boolean =>
  reward.isVisible !== false &&
  calendarDateIsWithin(calendarDateInTimeZone(date, timeZone), reward.availableFrom, reward.availableUntil);

export const catalogNewBadgeIsActive = (schedule: CatalogNewBadgeSchedule | undefined, timeZone: IanaTimeZone, date = new Date()): boolean =>
  schedule !== undefined &&
  calendarDateIsWithin(calendarDateInTimeZone(date, timeZone), schedule.from, schedule.until);
