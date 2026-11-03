import {
  isFamilyBootstrapReadModel,
  type FamilyBootstrapReadModel,
} from "@/application/contracts/family-bootstrap-contract";
import type { FamilyBootstrapService } from "@/application/ports/family-bootstrap-service";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import { FamilyReadModelApiError } from "@/infrastructure/http/http-family-read-model-repository";

export class HttpFamilyBootstrapService implements FamilyBootstrapService {
  public constructor(
    private readonly apiBaseUrl: string,
    private readonly fetchRequest: typeof fetch = globalThis.fetch.bind(globalThis),
  ) {}

  public async load(familyId: FamilyId, viewerMemberId?: FamilyMemberId): Promise<FamilyBootstrapReadModel | null> {
    const response = await this.fetchRequest(this.endpoint(familyId, viewerMemberId), {
      credentials: "include",
      headers: { Accept: "application/json" },
      method: "GET",
    });

    if (response.status === 404) {
      return null;
    }

    if (!response.ok) {
      throw new FamilyReadModelApiError("The family bootstrap read model could not be loaded.", response.status);
    }

    const value: unknown = await response.json();

    if (!isFamilyBootstrapReadModel(value)) {
      throw new FamilyReadModelApiError("The family bootstrap API returned an invalid response.", response.status);
    }

    return value;
  }

  private endpoint(familyId: FamilyId, viewerMemberId?: FamilyMemberId): string {
    const endpoint = `${this.apiBaseUrl}/api/families/${encodeURIComponent(familyId)}/read-models/bootstrap`;

    return viewerMemberId === undefined ? endpoint : `${endpoint}?viewerMemberId=${encodeURIComponent(viewerMemberId)}`;
  }
}
