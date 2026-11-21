import { DateTime } from "luxon";
import type { IanaTimeZone } from "@/domain/family/types";

export interface DueDateParts {
  readonly date: string;
  readonly daysRemaining: number;
  readonly weekday: string;
}

export function dueDateParts(
  dueDate: string,
  currentTimeMilliseconds: number,
  timeZone: IanaTimeZone,
  locale: string,
): DueDateParts | undefined {
  const today = DateTime.fromMillis(currentTimeMilliseconds, { zone: timeZone }).startOf("day");
  const due = DateTime.fromISO(dueDate, { zone: timeZone }).startOf("day");

  if (!due.isValid) {
    return undefined;
  }

  return {
    date: due.setLocale(locale).toLocaleString(DateTime.DATE_SHORT),
    daysRemaining: Math.max(0, Math.round(due.diff(today, "days").days)),
    weekday: due.setLocale(locale).toFormat("cccc"),
  };
}
