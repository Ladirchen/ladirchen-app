import { isFamilyReadModel } from "@/application/contracts/family-read-model-validation";
import type { FamilyReadModelDescriptor } from "@/application/contracts/family-read-model-descriptors";
import type { FamilyReadModelType, FamilyReadModel } from "@/application/contracts/family-read-model-contract";
import type { FamilyReadModelRepository } from "@/application/ports/family-read-model-repository";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

export class FamilyReadModelApiError extends Error {
  public constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = "FamilyReadModelApiError";
  }
}

type HttpFamilyReadModelRepositoryOptions<
  TReadModelType extends FamilyReadModelType,
  TState,
> = FamilyReadModelDescriptor<TReadModelType, TState> & {
  readonly apiBaseUrl: string;
};

async function parseJson(response: Response): Promise<unknown> {
  try {
    const parsed: unknown = await response.json();

    return parsed;
  } catch {
    throw new FamilyReadModelApiError("The family read-model API returned invalid JSON.", response.status);
  }
}

export class HttpFamilyReadModelRepository<
  TReadModelType extends FamilyReadModelType,
  TState,
> implements FamilyReadModelRepository<TReadModelType, TState> {
  public constructor(
    private readonly options: HttpFamilyReadModelRepositoryOptions<TReadModelType, TState>,
    private readonly fetchRequest: typeof fetch = globalThis.fetch.bind(globalThis),
  ) {}

  public async load(
    familyId: FamilyId,
    viewerMemberId?: FamilyMemberId,
  ): Promise<FamilyReadModel<TReadModelType, TState> | null> {
    const response = await this.fetchRequest(this.endpoint(familyId, viewerMemberId), {
      credentials: "include",
      headers: { Accept: "application/json" },
      method: "GET",
    });

    if (response.status === 404) {
      return null;
    }

    if (!response.ok) {
      throw new FamilyReadModelApiError("The family read model could not be loaded.", response.status);
    }

    return this.readModel(await parseJson(response), response.status);
  }

  private endpoint(familyId: FamilyId, viewerMemberId?: FamilyMemberId): string {
    const endpoint = `${this.options.apiBaseUrl}/api/families/${encodeURIComponent(familyId)}/read-models/${this.options.readModelType}`;

    return viewerMemberId === undefined ? endpoint : `${endpoint}?viewerMemberId=${encodeURIComponent(viewerMemberId)}`;
  }

  private readModel(value: unknown, status: number): FamilyReadModel<TReadModelType, TState> {
    if (!isFamilyReadModel(value, this.options.readModelType, this.options.isState)) {
      throw new FamilyReadModelApiError("The family read-model API returned an invalid response.", status);
    }

    return value;
  }
}
