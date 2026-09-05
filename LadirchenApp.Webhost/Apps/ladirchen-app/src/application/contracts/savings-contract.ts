import type { VersionedAggregateSnapshot } from './versioned-aggregate-contract';
import type { FamilyCurrency, FamilyMemberId, GuardianGift, SavingGoal } from '@/domain/types';

export const SAVINGS_SCHEMA_VERSION = 1 as const;
export const SAVINGS_AGGREGATE_TYPE = 'savings' as const;

export interface SavingsState {
  readonly balances: Readonly<Record<FamilyMemberId, number>>;
  readonly familyCurrencyCode: FamilyCurrency;
  readonly goals: ReadonlyArray<SavingGoal>;
  readonly ladirchenPerCurrencyUnit: number;
  readonly pendingGuardianGifts: ReadonlyArray<GuardianGift>;
}

export type SavingsSnapshot = VersionedAggregateSnapshot<
  typeof SAVINGS_AGGREGATE_TYPE,
  typeof SAVINGS_SCHEMA_VERSION,
  SavingsState
>;
