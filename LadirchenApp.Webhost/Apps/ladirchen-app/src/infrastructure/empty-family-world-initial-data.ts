import type { FamilyWorldInitialData } from "@/application/ports/family-world-initial-data";
import { createDomainId } from "@/domain/shared/identifiers";

const emptyMemberId = createDomainId.familyMember("00000000-0000-0000-0000-000000000000");
const emptyGoalId = createDomainId.savingGoal("00000000-0000-0000-0000-000000000000");

export function createEmptyFamilyWorldInitialData(): FamilyWorldInitialData {
  return {
    activeChildId: emptyMemberId,
    activeGoalId: emptyGoalId,
    accessories: [],
    balances: {},
    contributions: [],
    goals: [],
    houseLayout: [],
    members: [],
    pets: [],
    promotions: [],
    shopRewards: [],
    signedInMemberId: emptyMemberId,
  };
}
