import { HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import type { HubConnection } from "@microsoft/signalr";
import type {
  FamilyAggregateChanged,
  FamilyAggregateChangedHandler,
  FamilyAggregateReconnectedHandler,
  FamilyAggregateUpdatesGateway,
} from "@/application/ports/family-aggregate-updates";
import {
  isFamilyId,
  isFamilyMemberId,
  isInteger,
  isRecord,
} from "@/application/contracts/validation/runtime-validation";
import { isFamilyReadModelType } from "@/application/contracts/family-read-model-contract";
import type { FamilyId } from "@/domain/shared/identifiers";

const initialConnectionRetryDelays = [0, 1_000, 3_000] as const;

function parseChange(value: unknown): FamilyAggregateChanged | null {
  if (!isRecord(value)) {
    return null;
  }

  if (
    !isFamilyReadModelType(value.aggregateType) ||
    !isFamilyId(value.familyId) ||
    !isInteger(value.revision) ||
    !Number.isSafeInteger(value.revision) ||
    value.revision < 1 ||
    typeof value.updatedAt !== "string" ||
    !isFamilyMemberId(value.updatedBy)
  ) {
    return null;
  }

  return {
    aggregateType: value.aggregateType,
    familyId: value.familyId,
    revision: value.revision,
    updatedAt: value.updatedAt,
    updatedBy: value.updatedBy,
  };
}

export class SignalRFamilyAggregateUpdatesGateway implements FamilyAggregateUpdatesGateway {
  private readonly connection: HubConnection;
  private activeFamilyId: FamilyId | null = null;
  private onChanged: FamilyAggregateChangedHandler | null = null;
  private onReconnected: FamilyAggregateReconnectedHandler | null = null;
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;

  public constructor(apiBaseUrl: string) {
    this.connection = new HubConnectionBuilder()
      .withUrl(`${apiBaseUrl}/hubs/family-updates`, {
        withCredentials: true,
      })
      .withAutomaticReconnect([0, 2_000, 10_000, 30_000])
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
    this.connection.onclose(() => {
      this.scheduleReconnect();
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
      await this.startWithRetry();
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
    this.clearReconnectTimer();

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

  private scheduleReconnect(): void {
    if (!this.activeFamilyId || this.reconnectTimer) {
      return;
    }

    this.reconnectTimer = setTimeout(() => {
      this.reconnectTimer = null;
      void this.restartAfterDisconnect();
    }, 30_000);
  }

  private async restartAfterDisconnect(): Promise<void> {
    if (!this.activeFamilyId || this.connection.state !== HubConnectionState.Disconnected) {
      return;
    }

    try {
      await this.connection.start();
      await this.resubscribeAndRefresh();
    } catch {
      this.scheduleReconnect();
    }
  }

  private async startWithRetry(): Promise<void> {
    let lastError: unknown;

    for (const retryDelay of initialConnectionRetryDelays) {
      if (retryDelay > 0) {
        await new Promise<void>((resolve) => {
          setTimeout(resolve, retryDelay);
        });
      }

      if (!this.activeFamilyId) {
        return;
      }

      try {
        await this.connection.start();

        return;
      } catch (error) {
        lastError = error;
      }
    }

    throw lastError;
  }

  private clearReconnectTimer(): void {
    if (!this.reconnectTimer) {
      return;
    }

    clearTimeout(this.reconnectTimer);
    this.reconnectTimer = null;
  }
}
