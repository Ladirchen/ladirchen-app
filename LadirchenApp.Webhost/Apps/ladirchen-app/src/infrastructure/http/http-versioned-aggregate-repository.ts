import { isVersionedAggregateSnapshot } from "@/application/contracts/family-aggregate-validation";
import type { FamilyAggregateDescriptor } from "@/application/contracts/family-aggregate-descriptors";
import type {
  FamilyAggregateType,
  VersionedAggregateSnapshot,
} from "@/application/contracts/versioned-aggregate-contract";
import type { VersionedAggregateRepository } from "@/application/ports/versioned-aggregate-repository";
import type { FamilyId } from "@/domain/shared/identifiers";
export class AggregateApiError extends Error {
  public constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = "AggregateApiError";
  }
}
type HttpAggregateRepositoryOptions<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> = FamilyAggregateDescriptor<TAggregateType, TSchemaVersion, TState> & {
  readonly apiBaseUrl: string;
};
async function parseJson(response: Response): Promise<unknown> {
  try {
    const parsed: unknown = await response.json();

    return parsed;
  } catch {
    throw new AggregateApiError("The aggregate API returned invalid JSON.", response.status);
  }
}
export class HttpVersionedAggregateRepository<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> implements VersionedAggregateRepository<TAggregateType, TSchemaVersion, TState> {
  public constructor(
    private readonly options: HttpAggregateRepositoryOptions<TAggregateType, TSchemaVersion, TState>,
    private readonly fetchRequest: typeof fetch = globalThis.fetch.bind(globalThis),
  ) {}
  public async load(
    familyId: FamilyId,
  ): Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> | null> {
    const response = await this.fetchRequest(this.endpoint(familyId), {
      credentials: "include",
      headers: { Accept: "application/json" },
      method: "GET",
    });

    if (response.status === 404) {
      return null;
    }

    if (!response.ok) {
      throw new AggregateApiError("The aggregate could not be loaded.", response.status);
    }

    return this.readSnapshot(await parseJson(response), familyId, response.status);
  }
  private endpoint(familyId: FamilyId): string {
    return `${this.options.apiBaseUrl}/api/families/${encodeURIComponent(familyId)}/aggregates/${this.options.aggregateType}`;
  }
  private readSnapshot(
    value: unknown,
    familyId: FamilyId,
    status: number,
  ): VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> {
    if (
      !isVersionedAggregateSnapshot(
        value,
        this.options.aggregateType,
        this.options.schemaVersion,
        this.options.isState,
      ) ||
      value.familyId !== familyId
    ) {
      throw new AggregateApiError("The aggregate API returned an invalid snapshot.", status);
    }

    return value;
  }
}
