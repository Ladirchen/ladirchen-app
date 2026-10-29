import type { FamilyReadModelType } from "@/application/contracts/family-read-model-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export interface FamilyAggregateChanged {
  readonly aggregateType: FamilyReadModelType;
  readonly familyId: FamilyId;
  readonly revision: number;
  readonly updatedAt: string;
  readonly updatedBy: FamilyMemberId;
}

export type FamilyAggregateChangedHandler = (change: FamilyAggregateChanged) => void;
export type FamilyAggregateReconnectedHandler = () => Promise<void> | void;

export interface FamilyAggregateUpdatesGateway {
  connect: (
    familyId: FamilyId,
    onChanged: FamilyAggregateChangedHandler,
    onReconnected: FamilyAggregateReconnectedHandler,
  ) => Promise<void>;
  disconnect: () => Promise<void>;
}
