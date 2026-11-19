import { afterEach, describe, expect, it, vi } from "vite-plus/test";
import { createDomainId } from "@/domain/shared/identifiers";
import { HttpSchabiIntegrationGateway } from "./http-schabi-integration-gateway";

const familyId = createDomainId.family("00000000-0000-0000-0000-000000000001");
const integration = {
  configured: true,
  enabled: true,
  lastError: null,
  lastSuccessfulSyncAt: "2026-11-19T06:00:00Z",
  memberId: "child-1",
  syncDayOfWeek: 4,
  syncTimeLocal: "06:00",
  username: "student",
};

describe("HttpSchabiIntegrationGateway", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("uses the backend configuration state instead of deriving it from the username", async () => {
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValue(Response.json([{ ...integration, configured: true, username: "" }]));
    vi.stubGlobal("fetch", fetchMock);

    await expect(new HttpSchabiIntegrationGateway("").list(familyId)).resolves.toEqual([
      {
        configured: true,
        enabled: true,
        lastSuccessfulSyncAt: "2026-11-19T06:00:00Z",
        memberId: "child-1",
        syncDayOfWeek: 4,
        syncTimeLocal: "06:00",
        username: "",
      },
    ]);
  });

  it("rejects responses without an explicit configuration state", async () => {
    const { configured: _, ...invalidIntegration } = integration;
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(Response.json([invalidIntegration])));

    await expect(new HttpSchabiIntegrationGateway("").list(familyId)).rejects.toThrow(TypeError);
  });
});
