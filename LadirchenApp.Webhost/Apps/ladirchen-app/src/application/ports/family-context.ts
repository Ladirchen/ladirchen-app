import type { FamilyId } from "@/domain/shared/identifiers";

export interface FamilyContext {
  readonly activeFamilyId: FamilyId;
  setActiveFamilyId: (familyId: FamilyId) => void;
}
