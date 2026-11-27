import type { IanaTimeZone } from "@/domain/family/time-zone";
import type { AccountStatementEntry, AccountStatementTransactionType } from "@/domain/savings/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export interface AccountStatementFilters {
  memberId?: FamilyMemberId;
  transactionType?: AccountStatementTransactionType;
  dateFrom?: string;
  dateTo?: string;
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
