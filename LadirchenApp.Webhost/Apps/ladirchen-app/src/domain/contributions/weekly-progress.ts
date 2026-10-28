import type { IanaTimeZone } from "@/domain/family/time-zone";
import { calendarDateInTimeZone, isInstantInIsoWeek } from "@/domain/shared/zoned-calendar";
import type { Contribution } from "./types";
export function approvedContributionDatesInCurrentWeek(
  contributions: ReadonlyArray<Contribution>,
  timeZone: IanaTimeZone,
  now = new Date(),
): ReadonlySet<string> {

  return new Set(
    contributions.flatMap(function callback(contribution) {

      if (
        contribution.status !== "approved" ||
        !contribution.approvedAt ||
        !isInstantInIsoWeek(contribution.approvedAt, now, timeZone)
      ) {

        return [];
      }

      return [calendarDateInTimeZone(new Date(contribution.approvedAt), timeZone)];
    }),
  );
}
export function currentWeekDaysFromContributions(
  contributions: ReadonlyArray<Contribution>,
  timeZone: IanaTimeZone,
  now = new Date(),
): number {

  return approvedContributionDatesInCurrentWeek(contributions, timeZone, now).size;
}
