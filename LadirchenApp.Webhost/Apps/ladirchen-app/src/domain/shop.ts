import type { ShopReward } from '@/domain/types';
import type { IanaTimeZone } from './time-zone';
import { calendarDateInTimeZone, calendarDateIsWithin, zonedCalendarParts } from './zoned-calendar';

export const SHOP_REDEMPTION_CUTOFF_HOUR = 18;

export const shopRedemptionIsOpen = (timeZone: IanaTimeZone, date = new Date()): boolean =>
  zonedCalendarParts(date, timeZone).hour < SHOP_REDEMPTION_CUTOFF_HOUR;

export const shopRewardIsPublished = (reward: ShopReward, timeZone: IanaTimeZone, date = new Date()): boolean =>
  reward.isVisible !== false &&
  calendarDateIsWithin(calendarDateInTimeZone(date, timeZone), reward.availableFrom, reward.availableUntil);
