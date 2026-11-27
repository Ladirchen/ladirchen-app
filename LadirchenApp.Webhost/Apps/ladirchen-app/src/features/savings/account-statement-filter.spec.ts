import { describe, expect, it } from "vite-plus/test";
import type { IanaTimeZone } from "@/domain/family/time-zone";
import type { AccountStatementEntry } from "@/domain/savings/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";
import { filterAccountStatementEntries, recentAccountStatementDateRange } from "./account-statement-filter";

const timeZone = "Europe/Zurich" as IanaTimeZone;
const childOne = "child-one" as FamilyMemberId;
const childTwo = "child-two" as FamilyMemberId;
const entries: AccountStatementEntry[] = [
  {
    id: "entry-one",
    memberId: childOne,
    occurredAtUtc: "2026-11-26T23:30:00Z",
    amount: 5,
    balanceChange: 5,
    resultingBalance: 10,
    direction: "credit",
    transactionType: "task-reward",
  },
  {
    id: "entry-two",
    memberId: childTwo,
    occurredAtUtc: "2026-11-27T12:00:00Z",
    amount: 2,
    balanceChange: -2,
    resultingBalance: 8,
    direction: "debit",
    transactionType: "shop-purchase",
  },
];

describe("filterAccountStatementEntries", () => {
  it("filters by member and transaction type", () => {
    expect(filterAccountStatementEntries(entries, { memberId: childTwo }, timeZone)).toEqual([entries[1]]);
    expect(filterAccountStatementEntries(entries, { transactionType: "task-reward" }, timeZone)).toEqual([entries[0]]);
  });

  it("uses the family time zone for inclusive date filters", () => {
    expect(filterAccountStatementEntries(entries, { dateFrom: "2026-11-27", dateTo: "2026-11-27" }, timeZone)).toEqual(
      entries,
    );
  });

  it("defaults to the latest seven calendar days in the family time zone", () => {
    expect(recentAccountStatementDateRange(new Date("2026-11-27T23:30:00Z"), timeZone)).toEqual({
      dateFrom: "2026-11-22",
      dateTo: "2026-11-28",
    });
  });
});
