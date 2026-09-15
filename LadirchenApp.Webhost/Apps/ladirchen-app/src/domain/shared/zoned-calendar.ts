import type { IanaTimeZone } from '../family/time-zone';
import { DAYS_PER_WEEK, SECONDS_PER_HOUR, SECONDS_PER_MINUTE } from './time';

export interface ZonedCalendarParts {
  readonly date: string;
  readonly hour: number;
  readonly minute: number;
  readonly second: number;
}

const dateTimeFormatter = (timeZone: IanaTimeZone) => new Intl.DateTimeFormat('en', {
  day: '2-digit',
  hour: '2-digit',
  hourCycle: 'h23',
  minute: '2-digit',
  month: '2-digit',
  second: '2-digit',
  timeZone,
  year: 'numeric',
});

const numericPart = (parts: Intl.DateTimeFormatPart[], type: Intl.DateTimeFormatPartTypes): number =>
  Number(parts.find(part => part.type === type)?.value ?? Number.NaN);

export const zonedCalendarParts = (instant: Date, timeZone: IanaTimeZone): ZonedCalendarParts => {
  const parts = dateTimeFormatter(timeZone).formatToParts(instant);
  const year = numericPart(parts, 'year');
  const month = numericPart(parts, 'month');
  const day = numericPart(parts, 'day');
  return {
    date: `${year.toString().padStart(4, '0')}-${month.toString().padStart(2, '0')}-${day.toString().padStart(2, '0')}`,
    hour: numericPart(parts, 'hour'),
    minute: numericPart(parts, 'minute'),
    second: numericPart(parts, 'second'),
  };
};

export const calendarDateInTimeZone = (instant: Date, timeZone: IanaTimeZone): string =>
  zonedCalendarParts(instant, timeZone).date;

export const addCalendarDays = (date: string, days: number): string => {
  const [year = 0, month = 1, day = 1] = date.split('-').map(Number);
  const shifted = new Date(Date.UTC(year, month - 1, day + days));
  return shifted.toISOString().slice(0, 10);
};

export const startOfIsoWeek = (instant: Date, timeZone: IanaTimeZone): string => {
  const date = calendarDateInTimeZone(instant, timeZone);
  const [year = 0, month = 1, day = 1] = date.split('-').map(Number);
  const weekday = new Date(Date.UTC(year, month - 1, day)).getUTCDay();
  return addCalendarDays(date, -((weekday + DAYS_PER_WEEK - 1) % DAYS_PER_WEEK));
};

export const isInstantInIsoWeek = (value: string, reference: Date, timeZone: IanaTimeZone): boolean => {
  const instant = new Date(value);
  if (Number.isNaN(instant.getTime())) {return false;}
  const date = calendarDateInTimeZone(instant, timeZone);
  const start = startOfIsoWeek(reference, timeZone);
  return date >= start && date <= addCalendarDays(start, DAYS_PER_WEEK - 1);
};

export const isSameCalendarDay = (value: string, reference: Date, timeZone: IanaTimeZone): boolean => {
  const instant = new Date(value);
  return !Number.isNaN(instant.getTime()) &&
    calendarDateInTimeZone(instant, timeZone) === calendarDateInTimeZone(reference, timeZone);
};

export const secondsSinceStartOfDay = (instant: Date, timeZone: IanaTimeZone): number => {
  const { hour, minute, second } = zonedCalendarParts(instant, timeZone);
  return hour * SECONDS_PER_HOUR + minute * SECONDS_PER_MINUTE + second;
};

export const calendarDateIsWithin = (date: string, from?: string, until?: string): boolean =>
  (!from || from <= date) && (!until || date <= until);
