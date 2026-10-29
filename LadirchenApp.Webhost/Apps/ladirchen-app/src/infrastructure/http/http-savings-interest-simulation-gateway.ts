import type {
  SavingsInterestSimulation,
  SavingsInterestSimulationGateway,
} from "@/application/ports/savings-interest-simulation-gateway";
import { isFiniteNumber, isInteger, isRecord } from "@/application/contracts/validation/runtime-validation";
import { LADI_STAGES, type LadiStageId } from "@/domain/ladi";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

function isLadiStageId(value: unknown): value is LadiStageId {
  return typeof value === "string" && LADI_STAGES.some((stage) => stage.id === value);
}

function isSavingsInterestSimulation(value: unknown): value is SavingsInterestSimulation {
  return (
    isRecord(value) &&
    isFiniteNumber(value.interestRate) &&
    isLadiStageId(value.ladiStageId) &&
    isInteger(value.ladiStagePosition) &&
    isFiniteNumber(value.projectedCredit)
  );
}

export class HttpSavingsInterestSimulationGateway implements SavingsInterestSimulationGateway {
  readonly #baseUrl: string;

  constructor(baseUrl: string) {
    this.#baseUrl = baseUrl.replace(/\/+$/u, "");
  }

  async simulate(
    familyId: FamilyId,
    memberId: FamilyMemberId,
    streakDays: number,
    completionPercent: number,
    averageRating: number,
  ): Promise<SavingsInterestSimulation> {
    const response = await fetch(`${this.#baseUrl}/api/families/${familyId}/projections/savings-interest`, {
      body: JSON.stringify({ averageRating, completionPercent, memberId, streakDays }),
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      method: "POST",
    });

    if (!response.ok) {
      throw new Error(`Savings-interest simulation failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!isSavingsInterestSimulation(value)) {
      throw new TypeError("The savings-interest simulation response is invalid.");
    }

    return value;
  }
}
