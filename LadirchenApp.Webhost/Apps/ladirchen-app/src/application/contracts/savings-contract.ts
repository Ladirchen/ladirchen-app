import type { VersionedAggregateSnapshot } from "./versioned-aggregate-contract";
import type { FamilyCurrency, GuardianGift, SavingGoal, SavingsVoucher } from "@/domain/savings/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export const SAVINGS_SCHEMA_VERSION = 2;
export const SAVINGS_AGGREGATE_TYPE = "savings";

export interface SavingsState {
  readonly availableBalances?: Readonly<Record<FamilyMemberId, number>>;
  readonly balances: Readonly<Record<FamilyMemberId, number>>;
  readonly familyCurrencyCode: FamilyCurrency;
  readonly goals: ReadonlyArray<SavingGoal>;
  readonly ladirchenPerCurrencyUnit: number;
  readonly pendingGuardianGifts: ReadonlyArray<GuardianGift>;
  readonly settledInterestPeriods: ReadonlyArray<string>;
  readonly sharedSavedBalances?: Readonly<Record<FamilyMemberId, number>>;
  readonly vouchers: ReadonlyArray<SavingsVoucher>;
}

export type SavingsSnapshot = VersionedAggregateSnapshot<
  typeof SAVINGS_AGGREGATE_TYPE,
  typeof SAVINGS_SCHEMA_VERSION,
  SavingsState
>;
