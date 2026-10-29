import type {
  FamilyAggregateChangedHandler,
  FamilyAggregateUpdatesGateway,
} from "@/application/ports/family-aggregate-updates";
import type { FamilyId } from "@/domain/shared/identifiers";

export const noopFamilyAggregateUpdatesGateway: FamilyAggregateUpdatesGateway = {
  async connect(_familyId: FamilyId, _onChanged: FamilyAggregateChangedHandler): Promise<void> {},
  async disconnect(): Promise<void> {},
};
