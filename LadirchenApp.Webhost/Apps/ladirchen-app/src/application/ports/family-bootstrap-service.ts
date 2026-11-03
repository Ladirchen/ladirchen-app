import type { FamilyBootstrapReadModel } from "@/application/contracts/family-bootstrap-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export interface FamilyBootstrapService {
  load: (familyId: FamilyId, viewerMemberId?: FamilyMemberId) => Promise<FamilyBootstrapReadModel | null>;
}
