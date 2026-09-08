import type { Contribution } from './types';
import type { IanaTimeZone } from './time-zone';
import { calendarDateInTimeZone, isInstantInIsoWeek } from './zoned-calendar';

export const approvedContributionDatesInCurrentWeek = (
  contributions: ReadonlyArray<Contribution>,
  timeZone: IanaTimeZone,
  now = new Date(),
): ReadonlySet<string> => new Set(contributions.flatMap((contribution) => {
  if (contribution.status !== 'approved' || !contribution.approvedAt ||
    !isInstantInIsoWeek(contribution.approvedAt, now, timeZone)) {return [];}
  return [calendarDateInTimeZone(new Date(contribution.approvedAt), timeZone)];
}));

export const currentWeekDaysFromContributions = (
  contributions: ReadonlyArray<Contribution>,
  timeZone: IanaTimeZone,
  now = new Date(),
): number => approvedContributionDatesInCurrentWeek(contributions, timeZone, now).size;
