import { HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import type { HubConnection } from "@microsoft/signalr";
import type {
  FamilyAggregateChanged,
  FamilyAggregateChangedHandler,
  FamilyAggregateReconnectedHandler,
  FamilyAggregateUpdatesGateway,
} from "@/application/ports/family-aggregate-updates";
import { isDomainIdValue, isUuidValue } from "@/domain/shared/identifiers";
import type { FamilyAggregateType } from "@/application/contracts/versioned-aggregate-contract";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";

const aggregateTypes = new Set<FamilyAggregateType>([
  "contributions",
  "family-profile",
  "family-progression",
  "home-customization",
  "reward-shop",
  "savings",
]);

function isFamilyAggregateType(value: unknown): value is FamilyAggregateType {
  return typeof value === "string" && aggregateTypes.has(value as FamilyAggregateType);
}

function parseChange(value: unknown): FamilyAggregateChanged | null {
  if (typeof value !== "object" || value === null) {
    return null;
  }

  const candidate = value as Record<string, unknown>;

  if (
    !isFamilyAggregateType(candidate.aggregateType) ||
    !isUuidValue(candidate.familyId) ||
    !Number.isSafeInteger(candidate.revision) ||
    (candidate.revision as number) < 1 ||
    typeof candidate.updatedAt !== "string" ||
    !isDomainIdValue(candidate.updatedBy)
  ) {
    return null;
  }

  return {
    aggregateType: candidate.aggregateType,
    familyId: candidate.familyId as FamilyId,
    revision: candidate.revision as number,
    updatedAt: candidate.updatedAt,
    updatedBy: candidate.updatedBy as FamilyMemberId,
  };
}

export class SignalRFamilyAggregateUpdatesGateway implements FamilyAggregateUpdatesGateway {
  private readonly connection: HubConnection;
  private activeFamilyId: FamilyId | null = null;
  private onChanged: FamilyAggregateChangedHandler | null = null;
  private onReconnected: FamilyAggregateReconnectedHandler | null = null;

  public constructor(apiBaseUrl: string) {
    this.connection = new HubConnectionBuilder()
      .withUrl(`${apiBaseUrl}/hubs/family-updates`, { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
    this.connection.on("FamilyAggregateChanged", (value: unknown) => {
      const change = parseChange(value);

      if (change && change.familyId === this.activeFamilyId) {
        this.onChanged?.(change);
      }
    });
    this.connection.onreconnected(() => {
      void this.resubscribeAndRefresh().catch(() => undefined);
    });
  }

  public async connect(
    familyId: FamilyId,
    onChanged: FamilyAggregateChangedHandler,
    onReconnected: FamilyAggregateReconnectedHandler,
  ): Promise<void> {
    const previousFamilyId = this.activeFamilyId;
    this.activeFamilyId = familyId;
    this.onChanged = onChanged;
    this.onReconnected = onReconnected;

    if (this.connection.state === HubConnectionState.Disconnected) {
      await this.connection.start();
    }

    if (previousFamilyId && previousFamilyId !== familyId && this.connection.state === HubConnectionState.Connected) {
      await this.connection.invoke("UnsubscribeFromFamily", previousFamilyId);
    }

    await this.subscribeToActiveFamily();
  }

  public async disconnect(): Promise<void> {
    this.activeFamilyId = null;
    this.onChanged = null;
    this.onReconnected = null;

    if (this.connection.state !== HubConnectionState.Disconnected) {
      await this.connection.stop();
    }
  }

  private async subscribeToActiveFamily(): Promise<void> {
    const familyId = this.activeFamilyId;

    if (familyId && this.connection.state === HubConnectionState.Connected) {
      await this.connection.invoke("SubscribeToFamily", familyId);
    }
  }

  private async resubscribeAndRefresh(): Promise<void> {
    await this.subscribeToActiveFamily();
    await this.onReconnected?.();
  }
}
