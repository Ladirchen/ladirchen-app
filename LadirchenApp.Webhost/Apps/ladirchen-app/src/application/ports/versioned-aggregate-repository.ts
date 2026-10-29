import type {
  FamilyAggregateType,
  VersionedAggregateSnapshot,
} from "@/application/contracts/versioned-aggregate-contract";
import type { FamilyId } from "@/domain/shared/identifiers";

export interface VersionedAggregateRepository<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> {
  load: (familyId: FamilyId) => Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> | null>;
}
