import type { FamilyCommandGateway, FamilyCommandPayload } from "@/application/ports/family-command-gateway";
import type { FamilyAggregateType } from "@/application/contracts/versioned-aggregate-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

interface CommandSnapshotResponse {
  readonly aggregateType: FamilyAggregateType;
}

interface CommandResponse {
  readonly changedAggregates: ReadonlyArray<CommandSnapshotResponse>;
}

interface ProblemResponse {
  readonly detail?: string;
  readonly title?: string;
}

export class FamilyCommandHttpError extends Error {
  public constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "FamilyCommandHttpError";
  }
}

export class HttpFamilyCommandGateway implements FamilyCommandGateway {
  public constructor(private readonly apiBaseUrl: string) {}

  public async execute(
    familyId: FamilyId,
    actorId: FamilyMemberId,
    commandType: string,
    payload: FamilyCommandPayload,
  ): Promise<ReadonlyArray<FamilyAggregateType>> {
    const url = `${this.apiBaseUrl}/api/families/${encodeURIComponent(familyId)}/commands`;
    const commandId = crypto.randomUUID();
    const request = {
      body: JSON.stringify({ commandId, actorId, commandType, payload }),
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      method: "POST",
    } satisfies RequestInit;
    let response: Response;

    try {
      response = await fetch(url, request);
    } catch {
      response = await fetch(url, request);
    }

    if (response.status === 409) {
      response = await fetch(url, request);
    }

    if (!response.ok) {
      const problem = await this.readProblem(response);

      throw new FamilyCommandHttpError(
        response.status,
        problem.detail ?? problem.title ?? `Family command failed with HTTP ${response.status}.`,
      );
    }

    const result = (await response.json()) as CommandResponse;

    return result.changedAggregates.map((snapshot) => snapshot.aggregateType);
  }

  private async readProblem(response: Response): Promise<ProblemResponse> {
    try {
      return (await response.json()) as ProblemResponse;
    } catch {
      return {};
    }
  }
}
