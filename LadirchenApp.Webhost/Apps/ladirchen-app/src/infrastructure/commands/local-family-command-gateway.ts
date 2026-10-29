import type { FamilyCommandGateway, FamilyCommandPayload } from "@/application/ports/family-command-gateway";
import type { FamilyAggregateType } from "@/application/contracts/versioned-aggregate-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export const localFamilyCommandGateway: FamilyCommandGateway = {
  backendDriven: false,
  execute(
    _familyId: FamilyId,
    _actorId: FamilyMemberId,
    _commandType: string,
    _payload: FamilyCommandPayload,
  ): Promise<ReadonlyArray<FamilyAggregateType>> {
    return Promise.reject(new Error("Backend family commands are disabled."));
  },
};
