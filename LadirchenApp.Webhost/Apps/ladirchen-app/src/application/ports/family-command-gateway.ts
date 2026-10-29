import type { FamilyReadModelType } from "@/application/contracts/family-read-model-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export type FamilyCommandPayload = Readonly<Record<string, unknown>>;

export interface FamilyCommandGateway {
  execute: (
    familyId: FamilyId,
    actorId: FamilyMemberId,
    commandType: string,
    payload: FamilyCommandPayload,
  ) => Promise<ReadonlyArray<FamilyReadModelType>>;
}
