import type { LadiStageId } from "@/domain/ladi";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export interface SavingsInterestSimulation {
  readonly interestRate: number;
  readonly ladiStageId: LadiStageId;
  readonly ladiStagePosition: number;
  readonly projectedCredit: number;
}

export interface SavingsInterestSimulationGateway {
  simulate: (
    familyId: FamilyId,
    memberId: FamilyMemberId,
    streakDays: number,
    completionPercent: number,
    averageRating: number,
  ) => Promise<SavingsInterestSimulation>;
}
