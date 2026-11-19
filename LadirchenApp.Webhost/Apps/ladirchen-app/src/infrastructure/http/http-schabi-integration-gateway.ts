import { createDomainId } from "@/domain/shared/identifiers";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import type {
  SaveSchabiIntegrationCommand,
  SchabiIntegration,
  SchabiIntegrationGateway,
} from "@/application/ports/schabi-integration-gateway";
import {
  isFamilyMemberId,
  isIsoDateTime,
  isNonEmptyString,
  isRecord,
} from "@/application/contracts/validation/runtime-validation";

interface SchabiIntegrationDto {
  readonly memberId: string;
  readonly username: string;
  readonly configured: boolean;
  readonly enabled: boolean;
  readonly syncDayOfWeek: number;
  readonly syncTimeLocal: string;
  readonly lastSuccessfulSyncAt?: string | null;
  readonly lastError?: string | null;
}

function isSchabiIntegrationDto(value: unknown): value is SchabiIntegrationDto {
  return (
    isRecord(value) &&
    isFamilyMemberId(value.memberId) &&
    typeof value.username === "string" &&
    typeof value.configured === "boolean" &&
    typeof value.enabled === "boolean" &&
    typeof value.syncDayOfWeek === "number" &&
    Number.isInteger(value.syncDayOfWeek) &&
    value.syncDayOfWeek >= 0 &&
    value.syncDayOfWeek <= 6 &&
    typeof value.syncTimeLocal === "string" &&
    /^(?:[01]\d|2[0-3]):[0-5]\d$/u.test(value.syncTimeLocal) &&
    (value.lastSuccessfulSyncAt === null ||
      value.lastSuccessfulSyncAt === undefined ||
      isIsoDateTime(value.lastSuccessfulSyncAt)) &&
    (value.lastError === null || value.lastError === undefined || isNonEmptyString(value.lastError))
  );
}

function fromDto(value: SchabiIntegrationDto): SchabiIntegration {
  return {
    configured: value.configured,
    enabled: value.enabled,
    lastError: value.lastError ?? undefined,
    lastSuccessfulSyncAt: value.lastSuccessfulSyncAt ?? undefined,
    memberId: createDomainId.familyMember(value.memberId),
    syncDayOfWeek: value.syncDayOfWeek,
    syncTimeLocal: value.syncTimeLocal,
    username: value.username,
  };
}

export class HttpSchabiIntegrationGateway implements SchabiIntegrationGateway {
  readonly #baseUrl: string;

  constructor(baseUrl: string) {
    this.#baseUrl = baseUrl.replace(/\/+$/u, "");
  }

  async list(familyId: FamilyId): Promise<SchabiIntegration[]> {
    const response = await fetch(`${this.#baseUrl}/api/families/${familyId}/integrations/schabi`, {
      cache: "no-store",
      credentials: "include",
    });

    if (!response.ok) {
      throw new Error(`Schabi integrations failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!Array.isArray(value) || !value.every(isSchabiIntegrationDto)) {
      throw new TypeError("The Schabi integration response is invalid.");
    }

    return value.map(fromDto);
  }

  async remove(familyId: FamilyId, memberId: FamilyMemberId): Promise<void> {
    const response = await fetch(`${this.#baseUrl}/api/families/${familyId}/integrations/schabi/${memberId}`, {
      credentials: "include",
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error(`Removing the Schabi integration failed with status ${response.status}.`);
    }
  }

  async save(
    familyId: FamilyId,
    memberId: FamilyMemberId,
    command: SaveSchabiIntegrationCommand,
  ): Promise<SchabiIntegration> {
    const response = await fetch(`${this.#baseUrl}/api/families/${familyId}/integrations/schabi/${memberId}`, {
      body: JSON.stringify(command),
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      method: "PUT",
    });

    if (!response.ok) {
      throw new Error(`Saving the Schabi integration failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!isSchabiIntegrationDto(value)) {
      throw new TypeError("The Schabi integration response is invalid.");
    }

    return fromDto(value);
  }

  async synchronize(familyId: FamilyId, memberId: FamilyMemberId): Promise<SchabiIntegration> {
    const response = await fetch(
      `${this.#baseUrl}/api/families/${familyId}/integrations/schabi/${memberId}/synchronize`,
      {
        credentials: "include",
        method: "POST",
      },
    );

    if (!response.ok) {
      throw new Error(`Synchronizing the Schabi integration failed with status ${response.status}.`);
    }

    const value: unknown = await response.json();

    if (!isSchabiIntegrationDto(value)) {
      throw new TypeError("The Schabi integration response is invalid.");
    }

    return fromDto(value);
  }
}
