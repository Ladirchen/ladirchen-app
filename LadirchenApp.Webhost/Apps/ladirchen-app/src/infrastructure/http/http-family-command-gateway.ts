import type {
  FamilyCommandGateway,
  FamilyCommandPayload,
  FamilyCommandType,
} from "@/application/ports/family-command-gateway";
import { isFamilyReadModelType, type FamilyReadModelType } from "@/application/contracts/family-read-model-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import { isRecord } from "@/application/contracts/validation/runtime-validation";

interface ProblemResponse {
  readonly detail?: string;
  readonly title?: string;
}

function parseCommandResponse(value: unknown): ReadonlyArray<FamilyReadModelType> {
  if (!isRecord(value)) {
    throw new TypeError("The family command response must be an object.");
  }

  const changedReadModelTypes = value.changedReadModelTypes;

  if (!Array.isArray(changedReadModelTypes) || !changedReadModelTypes.every(isFamilyReadModelType)) {
    throw new TypeError("The family command response contains invalid read-model types.");
  }

  return [...new Set(changedReadModelTypes)];
}

function parseProblemResponse(value: unknown): ProblemResponse {
  if (!isRecord(value)) {
    return {};
  }

  return {
    detail: typeof value.detail === "string" ? value.detail : undefined,
    title: typeof value.title === "string" ? value.title : undefined,
  };
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

  public async execute<TCommandType extends FamilyCommandType>(
    familyId: FamilyId,
    actorId: FamilyMemberId,
    commandType: TCommandType,
    payload: FamilyCommandPayload<NoInfer<TCommandType>>,
  ): Promise<ReadonlyArray<FamilyReadModelType>> {
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

    const result: unknown = await response.json();

    return parseCommandResponse(result);
  }

  private async readProblem(response: Response): Promise<ProblemResponse> {
    try {
      const value: unknown = await response.json();

      return parseProblemResponse(value);
    } catch {
      return {};
    }
  }
}
