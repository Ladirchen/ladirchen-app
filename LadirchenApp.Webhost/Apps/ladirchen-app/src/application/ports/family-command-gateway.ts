import type { FamilyCommandPayload, FamilyCommandType } from "@/application/contracts/family-command-contract";
import type { FamilyReadModelType } from "@/application/contracts/family-read-model-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export type {
  FamilyCommand,
  FamilyCommandPayload,
  FamilyCommandType,
} from "@/application/contracts/family-command-contract";

export interface FamilyCommandGateway {
  execute: <TCommandType extends FamilyCommandType>(
    familyId: FamilyId,
    actorId: FamilyMemberId,
    commandType: TCommandType,
    payload: FamilyCommandPayload<NoInfer<TCommandType>>,
  ) => Promise<ReadonlyArray<FamilyReadModelType>>;
}
