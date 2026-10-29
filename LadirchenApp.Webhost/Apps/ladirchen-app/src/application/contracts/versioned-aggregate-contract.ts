import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export type FamilyAggregateType =
  | "contributions"
  | "family-profile"
  | "family-progression"
  | "home-customization"
  | "reward-shop"
  | "savings";

export interface VersionedAggregateSnapshot<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> {
  readonly aggregateType: TAggregateType;
  readonly familyId: FamilyId;
  readonly revision: number;
  readonly schemaVersion: TSchemaVersion;
  readonly state: TState;
  readonly updatedAt: string;
  readonly updatedBy: FamilyMemberId;
}
