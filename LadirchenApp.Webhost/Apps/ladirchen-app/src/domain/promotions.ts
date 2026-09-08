import type { Promotion } from './types';
import type { IanaTimeZone } from './time-zone';
import { secondsSinceStartOfDay } from './zoned-calendar';

const deadlineSeconds = (deadline: string): number => {
  const [hours = 0, minutes = 0] = deadline.split(':').map(Number);
  return hours * 3_600 + minutes * 60;
};

export const remainingPromotionMilliseconds = (deadline: string, timeZone: IanaTimeZone, now = new Date()): number =>
  Math.max(0, (deadlineSeconds(deadline) - secondsSinceStartOfDay(now, timeZone)) * 1_000 - now.getUTCMilliseconds());

export const isPromotionAvailable = (promotion: Promotion, timeZone: IanaTimeZone, now = new Date()): boolean =>
  promotion.active && remainingPromotionMilliseconds(promotion.deadline, timeZone, now) > 0;
