import type { FamilyReadModelType, FamilyReadModel } from "@/application/contracts/family-read-model-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export interface FamilyReadModelRepository<TReadModelType extends FamilyReadModelType, TState> {
  load: (
    familyId: FamilyId,
    viewerMemberId?: FamilyMemberId,
  ) => Promise<FamilyReadModel<TReadModelType, TState> | null>;
}
