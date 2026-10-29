import type {
  SavingsInterestSimulation,
  SavingsInterestSimulationGateway,
} from "@/application/ports/savings-interest-simulation-gateway";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

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

    return (await response.json()) as SavingsInterestSimulation;
  }
}
