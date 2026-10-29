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
        new Response(JSON.stringify({ changedAggregates: [{ aggregateType: "savings" }] }), {
          headers: { "Content-Type": "application/json" },
          status: 200,
        }),
      );
    vi.stubGlobal("fetch", fetchMock);
    const gateway = new HttpFamilyCommandGateway("");

    await expect(gateway.execute(familyId, actorId, "savings.deposit", { amount: 10 })).resolves.toEqual(["savings"]);
    expect(fetchMock).toHaveBeenCalledTimes(2);
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
});
