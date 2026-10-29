import { DateTime } from "luxon";
import type { IanaTimeZone } from "@/domain/family/time-zone";
export function remainingPromotionMilliseconds(deadline: string, timeZone: IanaTimeZone, now = new Date()): number {
  const current = DateTime.fromJSDate(now, { zone: timeZone });
  const deadlineAt = DateTime.fromISO(`${current.toISODate()}T${deadline}`, { zone: timeZone });

  return Math.max(0, deadlineAt.diff(current).as("milliseconds"));
}
