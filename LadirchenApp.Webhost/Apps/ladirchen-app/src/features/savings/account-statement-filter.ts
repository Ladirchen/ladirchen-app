import { DateTime } from "luxon";
import type { IanaTimeZone } from "@/domain/family/time-zone";
import type { AccountStatementEntry, AccountStatementTransactionType } from "@/domain/savings/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";
import { DAYS_PER_WEEK } from "@/domain/shared/time";

export interface AccountStatementFilters {
  memberId?: FamilyMemberId;
  transactionType?: AccountStatementTransactionType;
  dateFrom?: string;
  dateTo?: string;
}

export interface AccountStatementDateRange {
  readonly dateFrom: string;
  readonly dateTo: string;
}

export function recentAccountStatementDateRange(reference: Date, timeZone: IanaTimeZone): AccountStatementDateRange {
  const dateTo = DateTime.fromJSDate(reference, { zone: timeZone }).startOf("day");
  const dateFrom = dateTo.minus({ days: DAYS_PER_WEEK - 1 });

  return {
    dateFrom: dateFrom.toISODate() ?? "",
    dateTo: dateTo.toISODate() ?? "",
  };
}

export function filterAccountStatementEntries(
  entries: ReadonlyArray<AccountStatementEntry>,
  filters: AccountStatementFilters,
  timeZone: IanaTimeZone,
): AccountStatementEntry[] {
  return entries.filter((entry) => {
    if (filters.memberId && entry.memberId !== filters.memberId) {
      return false;
    }

    if (filters.transactionType && entry.transactionType !== filters.transactionType) {
      return false;
    }

    const localDate = localDateKey(entry.occurredAtUtc, timeZone);

    return (!filters.dateFrom || localDate >= filters.dateFrom) && (!filters.dateTo || localDate <= filters.dateTo);
  });
}

function localDateKey(value: string, timeZone: IanaTimeZone): string {
  const parts = new Intl.DateTimeFormat("en-CA", {
    day: "2-digit",
    month: "2-digit",
    timeZone,
    year: "numeric",
  }).formatToParts(new Date(value));

  function part(type: Intl.DateTimeFormatPartTypes): string {
    return parts.find((item) => item.type === type)?.value ?? "";
  }

  return `${part("year")}-${part("month")}-${part("day")}`;
}
