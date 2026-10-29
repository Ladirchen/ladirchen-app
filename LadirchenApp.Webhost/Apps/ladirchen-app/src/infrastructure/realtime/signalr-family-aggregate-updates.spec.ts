import { HttpTransportType } from "@microsoft/signalr";
import { describe, expect, it, vi } from "vite-plus/test";

import { SignalRFamilyAggregateUpdatesGateway } from "./signalr-family-aggregate-updates";

const signalRMocks = vi.hoisted(() => ({
  configureLogging: vi.fn(),
  on: vi.fn(),
  onreconnected: vi.fn(),
  withAutomaticReconnect: vi.fn(),
  withUrl: vi.fn(),
}));

vi.mock("@microsoft/signalr", () => {
  class HubConnectionBuilder {
    public configureLogging(level: unknown): this {
      signalRMocks.configureLogging(level);

      return this;
    }

    public withAutomaticReconnect(): this {
      signalRMocks.withAutomaticReconnect();

      return this;
    }

    public withUrl(url: string, options: unknown): this {
      signalRMocks.withUrl(url, options);

      return this;
    }

    public build() {
      return {
        on: signalRMocks.on,
        onreconnected: signalRMocks.onreconnected,
      };
    }
  }

  return {
    HttpTransportType: { WebSockets: 1 },
    HubConnectionBuilder,
    HubConnectionState: { Connected: "Connected", Disconnected: "Disconnected" },
    LogLevel: { Warning: 3 },
  };
});

describe("SignalRFamilyAggregateUpdatesGateway", () => {
  it("connects directly with the WebSocket transport", () => {
    new SignalRFamilyAggregateUpdatesGateway("");

    expect(signalRMocks.withUrl).toHaveBeenCalledWith("/hubs/family-updates", {
      skipNegotiation: true,
      transport: HttpTransportType.WebSockets,
      withCredentials: true,
    });
  });
});
