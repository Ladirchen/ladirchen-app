import { afterEach, describe, expect, it, vi } from "vite-plus/test";

import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
import type { FamilyCommandHttpError } from "./http-family-command-gateway";
import { HttpFamilyCommandGateway } from "./http-family-command-gateway";

const familyId = "c776a9b6-c37d-4a51-91ba-269a67274478" as FamilyId;
const actorId = "member-1" as FamilyMemberId;

describe("HttpFamilyCommandGateway", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("retries a command once after a revision conflict", async () => {
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ detail: "Revision conflict" }), {
          headers: { "Content-Type": "application/problem+json" },
          status: 409,
        }),
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ changedReadModelTypes: ["savings"] }), {
          headers: { "Content-Type": "application/json" },
          status: 200,
        }),
      );
    vi.stubGlobal("fetch", fetchMock);
    const gateway = new HttpFamilyCommandGateway("");

    await expect(gateway.execute(familyId, actorId, "savings.deposit", { amount: 10 })).resolves.toEqual(["savings"]);
    expect(fetchMock).toHaveBeenCalledTimes(2);
    const firstRequest = fetchMock.mock.calls[0]?.[1];
    const secondRequest = fetchMock.mock.calls[1]?.[1];
    expect(firstRequest?.body).toBe(secondRequest?.body);
    const firstBody = firstRequest?.body;
    expect(typeof firstBody).toBe("string");

    if (typeof firstBody !== "string") {
      throw new TypeError("The command request body must be serialized JSON.");
    }

    const command = JSON.parse(firstBody) as Record<string, unknown>;
    expect(command).toMatchObject({
      actorId,
      commandType: "savings.deposit",
      payload: { amount: 10 },
    });
    expect(command.commandId).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i);
  });

  it("reuses the command ID when retrying an uncertain network failure", async () => {
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockRejectedValueOnce(new TypeError("The response was lost"))
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ changedReadModelTypes: ["savings"] }), {
          headers: { "Content-Type": "application/json" },
          status: 200,
        }),
      );
    vi.stubGlobal("fetch", fetchMock);
    const gateway = new HttpFamilyCommandGateway("");

    await expect(gateway.execute(familyId, actorId, "savings.deposit", { amount: 10 })).resolves.toEqual(["savings"]);
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(fetchMock.mock.calls[0]?.[1]?.body).toBe(fetchMock.mock.calls[1]?.[1]?.body);
  });

  it("exposes a repeated conflict as a typed HTTP error", async () => {
    function conflict() {
      return new Response(JSON.stringify({ detail: "Revision conflict" }), {
        headers: { "Content-Type": "application/problem+json" },
        status: 409,
      });
    }
    vi.stubGlobal(
      "fetch",
      vi.fn<typeof fetch>().mockImplementation(async () => conflict()),
    );
    const gateway = new HttpFamilyCommandGateway("");

    const execution = gateway.execute(familyId, actorId, "savings.deposit", { amount: 10 });

    await expect(execution).rejects.toMatchObject<Partial<FamilyCommandHttpError>>({
      message: "Revision conflict",
      status: 409,
    });
  });

  it("rejects an invalid successful response instead of leaving the cache stale", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn<typeof fetch>().mockResolvedValue(
        new Response(JSON.stringify({ changedReadModelTypes: ["savings", "unknown"] }), {
          headers: { "Content-Type": "application/json" },
          status: 200,
        }),
      ),
    );
    const gateway = new HttpFamilyCommandGateway("");

    await expect(gateway.execute(familyId, actorId, "savings.deposit", { amount: 10 })).rejects.toThrow(
      "The family command response contains invalid read-model types.",
    );
  });

  it("deduplicates changed read-model types", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn<typeof fetch>().mockResolvedValue(
        new Response(JSON.stringify({ changedReadModelTypes: ["savings", "savings"] }), {
          headers: { "Content-Type": "application/json" },
          status: 200,
        }),
      ),
    );
    const gateway = new HttpFamilyCommandGateway("");

    await expect(gateway.execute(familyId, actorId, "savings.deposit", { amount: 10 })).resolves.toEqual(["savings"]);
  });
});
