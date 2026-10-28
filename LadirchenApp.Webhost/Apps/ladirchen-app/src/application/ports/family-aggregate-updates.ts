import type { FamilyAggregateType } from "@/application/contracts/versioned-aggregate-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export interface FamilyAggregateChanged {
  readonly aggregateType: FamilyAggregateType;
  readonly familyId: FamilyId;
  readonly revision: number;
  readonly updatedAt: string;
  readonly updatedBy: FamilyMemberId;
}

export type FamilyAggregateChangedHandler = (change: FamilyAggregateChanged) => void;

export interface FamilyAggregateUpdatesGateway {
  connect: (familyId: FamilyId, onChanged: FamilyAggregateChangedHandler) => Promise<void>;
  disconnect: () => Promise<void>;
}
