import type { FamilyAggregateType } from "@/application/contracts/versioned-aggregate-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export type FamilyCommandPayload = Readonly<
  Record<string, boolean | number | string | ReadonlyArray<string> | undefined>
>;

export interface FamilyCommandGateway {
  readonly backendDriven: boolean;
  execute: (
    familyId: FamilyId,
    actorId: FamilyMemberId,
    commandType: string,
    payload: FamilyCommandPayload,
  ) => Promise<ReadonlyArray<FamilyAggregateType>>;
}
