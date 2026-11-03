import { describe, expect, it, vi } from "vite-plus/test";

import { createDomainId } from "@/domain/shared/identifiers";

import { SignalRFamilyAggregateUpdatesGateway } from "./signalr-family-aggregate-updates";

const signalRMocks = vi.hoisted(() => ({
  configureLogging: vi.fn(),
  connectionState: { value: "Disconnected" },
  invoke: vi.fn(),
  on: vi.fn(),
  onclose: vi.fn(),
  onreconnected: vi.fn(),
  start: vi.fn(),
  stop: vi.fn(),
  withAutomaticReconnect: vi.fn(),
  withUrl: vi.fn(),
}));

vi.mock("@microsoft/signalr", () => {
  class HubConnectionBuilder {
    public configureLogging(level: unknown): this {
      signalRMocks.configureLogging(level);

      return this;
    }

    public withAutomaticReconnect(retryDelays: ReadonlyArray<number>): this {
      signalRMocks.withAutomaticReconnect(retryDelays);

      return this;
    }

    public withUrl(url: string, options: unknown): this {
      signalRMocks.withUrl(url, options);

      return this;
    }

    public build() {
      return {
        get state() {
          return signalRMocks.connectionState.value;
        },
        invoke: signalRMocks.invoke,
        on: signalRMocks.on,
        onclose: signalRMocks.onclose,
        onreconnected: signalRMocks.onreconnected,
        start: signalRMocks.start,
        stop: signalRMocks.stop,
      };
    }
  }

  return {
    HubConnectionBuilder,
    HubConnectionState: { Connected: "Connected", Disconnected: "Disconnected" },
    LogLevel: { Warning: 3 },
  };
});

describe("SignalRFamilyAggregateUpdatesGateway", () => {
  it("negotiates the best available transport and configures reconnect delays", () => {
    new SignalRFamilyAggregateUpdatesGateway("");

    expect(signalRMocks.withUrl).toHaveBeenCalledWith("/hubs/family-updates", {
      withCredentials: true,
    });
    expect(signalRMocks.withAutomaticReconnect).toHaveBeenCalledWith([0, 2_000, 10_000, 30_000]);
  });

  it("retries an initial connection failure before subscribing", async () => {
    vi.useFakeTimers();
    signalRMocks.connectionState.value = "Disconnected";
    signalRMocks.invoke.mockReset().mockResolvedValue(undefined);
    signalRMocks.start
      .mockReset()
      .mockRejectedValueOnce(new Error("Network unavailable"))
      .mockRejectedValueOnce(new Error("Network unavailable"))
      .mockImplementationOnce(async () => {
        signalRMocks.connectionState.value = "Connected";
      });
    const gateway = new SignalRFamilyAggregateUpdatesGateway("");
    const familyId = createDomainId.family("7a611dfc-d898-4a4c-a9fe-8acc672fea03");

    try {
      const connection = gateway.connect(familyId, vi.fn(), vi.fn());
      await vi.advanceTimersByTimeAsync(4_000);
      await connection;

      expect(signalRMocks.start).toHaveBeenCalledTimes(3);
      expect(signalRMocks.invoke).toHaveBeenCalledWith("SubscribeToFamily", familyId);
    } finally {
      vi.useRealTimers();
    }
  });
});
