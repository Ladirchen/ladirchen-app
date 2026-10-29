import type { FamilyCommandGateway, FamilyCommandPayload } from "@/application/ports/family-command-gateway";
import type { FamilyAggregateType } from "@/application/contracts/versioned-aggregate-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

interface CommandSnapshotResponse {
  readonly aggregateType: FamilyAggregateType;
}

interface CommandResponse {
  readonly changedAggregates: ReadonlyArray<CommandSnapshotResponse>;
}

export class HttpFamilyCommandGateway implements FamilyCommandGateway {
  public constructor(private readonly apiBaseUrl: string) {}

  public async execute(
    familyId: FamilyId,
    actorId: FamilyMemberId,
    commandType: string,
    payload: FamilyCommandPayload,
  ): Promise<ReadonlyArray<FamilyAggregateType>> {
    const response = await fetch(`${this.apiBaseUrl}/api/families/${encodeURIComponent(familyId)}/commands`, {
      body: JSON.stringify({ actorId, commandType, payload }),
      headers: { "Content-Type": "application/json" },
      method: "POST",
    });

    if (!response.ok) {
      throw new Error(`Family command failed with HTTP ${response.status}.`);
    }

    const result = (await response.json()) as CommandResponse;

    return result.changedAggregates.map((snapshot) => snapshot.aggregateType);
  }
}
