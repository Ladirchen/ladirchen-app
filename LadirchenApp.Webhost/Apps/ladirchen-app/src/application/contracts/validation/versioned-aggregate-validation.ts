import type { FamilyAggregateType, VersionedAggregateSnapshot } from '../versioned-aggregate-contract';
import {
  isFamilyId,
  isFamilyMemberId,
  isIsoDateTime,
  isNonNegativeInteger,
  isRecord,
  type StateGuard,
} from './runtime-validation';

export const isVersionedAggregateSnapshot = <
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
>(
  value: unknown,
  aggregateType: TAggregateType,
  schemaVersion: TSchemaVersion,
  isState: StateGuard<TState>,
): value is VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> =>
  isRecord(value) &&
  value.aggregateType === aggregateType &&
  value.schemaVersion === schemaVersion &&
  isFamilyId(value.familyId) &&
  isNonNegativeInteger(value.revision) &&
  isIsoDateTime(value.updatedAt) &&
  isFamilyMemberId(value.updatedBy) &&
  isState(value.state);
