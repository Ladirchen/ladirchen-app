import type { Promotion } from './types';
import type { IanaTimeZone } from '../family/time-zone';
import { MILLISECONDS_PER_SECOND, SECONDS_PER_HOUR, SECONDS_PER_MINUTE } from '../shared/time';
import { secondsSinceStartOfDay } from '../shared/zoned-calendar';

const deadlineSeconds = (deadline: string): number => {
  const [hours = 0, minutes = 0] = deadline.split(':').map(Number);
  return hours * SECONDS_PER_HOUR + minutes * SECONDS_PER_MINUTE;
};

export const remainingPromotionMilliseconds = (
  deadline: string,
  timeZone: IanaTimeZone,
  now = new Date(),
): number => Math.max(0,
  (deadlineSeconds(deadline) - secondsSinceStartOfDay(now, timeZone)) * MILLISECONDS_PER_SECOND - now.getUTCMilliseconds(),
);

export const isPromotionAvailable = (
  promotion: Promotion,
  timeZone: IanaTimeZone,
  now = new Date(),
): boolean => promotion.active && remainingPromotionMilliseconds(promotion.deadline, timeZone, now) > 0;
