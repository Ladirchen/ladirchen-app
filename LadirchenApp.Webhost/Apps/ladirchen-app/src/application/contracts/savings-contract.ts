import type { FamilyReadModel } from "./family-read-model-contract";
import type { FamilyCurrency, GuardianGift, SavingGoal, SavingsVoucher } from "@/domain/savings/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";

export const SAVINGS_READ_MODEL_TYPE = "savings";

export interface SavingsState {
  readonly availableBalances: Readonly<Record<FamilyMemberId, number>>;
  readonly balances: Readonly<Record<FamilyMemberId, number>>;
  readonly familyCurrencyCode: FamilyCurrency;
  readonly goals: ReadonlyArray<SavingGoal>;
  readonly ladirchenPerCurrencyUnit: number;
  readonly pendingGuardianGifts: ReadonlyArray<GuardianGift>;
  readonly settledInterestPeriods: ReadonlyArray<string>;
  readonly sharedSavedBalances: Readonly<Record<FamilyMemberId, number>>;
  readonly vouchers: ReadonlyArray<SavingsVoucher>;
}

export type SavingsReadModel = FamilyReadModel<typeof SAVINGS_READ_MODEL_TYPE, SavingsState>;
