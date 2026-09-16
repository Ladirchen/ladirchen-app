import type { FamilyId, FamilyMemberId } from '@/domain/types';

export type FamilyAggregateType =
  | 'contributions'
  | 'family-profile'
  | 'family-progression'
  | 'home-customization'
  | 'reward-shop'
  | 'savings';

export interface AggregateDraft<TState> {
  readonly familyId: FamilyId;
  readonly state: TState;
  readonly updatedBy: FamilyMemberId;
}

export interface SaveVersionedAggregateCommand<TAggregateType extends FamilyAggregateType, TState> extends AggregateDraft<TState> {
  readonly aggregateType: TAggregateType;
  readonly expectedRevision: number;
}

export interface VersionedAggregateSnapshot<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> extends AggregateDraft<TState> {
  readonly aggregateType: TAggregateType;
  readonly revision: number;
  readonly schemaVersion: TSchemaVersion;
  readonly updatedAt: string;
}
