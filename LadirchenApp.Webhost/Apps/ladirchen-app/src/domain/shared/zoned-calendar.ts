import { DateTime } from "luxon";
import type { IanaTimeZone } from "@/domain/family/time-zone";
import { DAYS_PER_WEEK } from "./time";
export interface ZonedCalendarParts {
  readonly date: string;
  readonly hour: number;
  readonly minute: number;
  readonly second: number;
}
function toIsoDate(dateTime: DateTime): string {
  const date = dateTime.toISODate();

  if (!date) {
    throw new TypeError(dateTime.invalidExplanation ?? "Invalid date");
  }

  return date;
}
function inTimeZone(instant: Date, timeZone: IanaTimeZone): DateTime {

  return DateTime.fromJSDate(instant, { zone: timeZone });
}
export function zonedCalendarParts(instant: Date, timeZone: IanaTimeZone): ZonedCalendarParts {
  const dateTime = inTimeZone(instant, timeZone);

  return {
    date: toIsoDate(dateTime),
    hour: dateTime.hour,
    minute: dateTime.minute,
    second: dateTime.second,
  };
}
export function calendarDateInTimeZone(instant: Date, timeZone: IanaTimeZone): string {

  return zonedCalendarParts(instant, timeZone).date;
}
export function isInstantInIsoWeek(value: string, reference: Date, timeZone: IanaTimeZone): boolean {
  const instant = DateTime.fromISO(value, { setZone: true });

  if (!instant.isValid) {

    return false;
  }

  const start = inTimeZone(reference, timeZone).startOf("week");
  const zonedInstant = instant.setZone(timeZone);

  return zonedInstant >= start && zonedInstant < start.plus({ days: DAYS_PER_WEEK });
}
export function isSameCalendarDay(value: string, reference: Date, timeZone: IanaTimeZone): boolean {
  const instant = DateTime.fromISO(value, { setZone: true });

  return instant.isValid && instant.setZone(timeZone).hasSame(inTimeZone(reference, timeZone), "day");
}
export function calendarDateIsWithin(date: string, from?: string, until?: string): boolean {

  return (!from || from <= date) && (!until || date <= until);
}
