import type { FamilyAggregateType, SaveVersionedAggregateCommand, VersionedAggregateSnapshot } from "@/application/contracts/versioned-aggregate-contract";
import type { FamilyId } from "@/domain/shared/identifiers";

export class AggregateConflictError extends Error {
  public constructor(aggregateType: FamilyAggregateType) {
    super(`The ${aggregateType} aggregate was changed by another session.`);
    this.name = "AggregateConflictError";
  }
}

export interface VersionedAggregateRepository<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> {
  load: (familyId: FamilyId) => Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> | null>;
  save: (command: SaveVersionedAggregateCommand<TAggregateType, TState>) => Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState>>;
}
