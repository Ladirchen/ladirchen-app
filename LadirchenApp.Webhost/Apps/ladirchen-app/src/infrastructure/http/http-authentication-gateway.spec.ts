import { afterEach, describe, expect, it, vi } from "vite-plus/test";

import { createDomainId } from "@/domain/shared/identifiers";
import { HttpAuthenticationGateway } from "./http-authentication-gateway";

const familyId = createDomainId.family("c776a9b6-c37d-4a51-91ba-269a67274478");
const memberId = createDomainId.familyMember("member-1");
const session = { familyId, familyName: "Example family", memberId };

describe("HttpAuthenticationGateway", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("loads whether bootstrap registration is still available", async () => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
      new Response(JSON.stringify({ registrationAvailable: true }), {
        headers: { "Content-Type": "application/json" },
        status: 200,
      }),
    );
    vi.stubGlobal("fetch", fetchMock);
    const gateway = new HttpAuthenticationGateway("");

    await expect(gateway.getRegistrationStatus()).resolves.toEqual({ registrationAvailable: true });
    expect(fetchMock).toHaveBeenCalledWith("/api/auth/registration-status", {
      cache: "no-store",
      credentials: "include",
      method: "GET",
    });
  });

  it("sends an email address or username as the login identifier", async () => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
      new Response(JSON.stringify(session), {
        headers: { "Content-Type": "application/json" },
        status: 200,
      }),
    );
    vi.stubGlobal("fetch", fetchMock);
    const gateway = new HttpAuthenticationGateway("");

    await expect(gateway.authenticate("member@example.test", "secret-password")).resolves.toEqual(session);
    expect(fetchMock).toHaveBeenCalledWith(
      "/api/auth/login",
      expect.objectContaining({
        body: JSON.stringify({ identifier: "member@example.test", password: "secret-password" }),
      }),
    );
  });

  it("restores the authenticated session from the server cookie", async () => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
      new Response(JSON.stringify(session), {
        headers: { "Content-Type": "application/json" },
        status: 200,
      }),
    );
    vi.stubGlobal("fetch", fetchMock);
    const gateway = new HttpAuthenticationGateway("");

    await expect(gateway.getCurrentSession()).resolves.toEqual(session);
    expect(fetchMock).toHaveBeenCalledWith("/api/auth/session", {
      cache: "no-store",
      credentials: "include",
      method: "GET",
    });
  });

  it("returns no current session when the cookie is missing or expired", async () => {
    vi.stubGlobal("fetch", vi.fn<typeof fetch>().mockResolvedValue(new Response(null, { status: 401 })));
    const gateway = new HttpAuthenticationGateway("");

    await expect(gateway.getCurrentSession()).resolves.toBeNull();
  });

  it("includes the first guardian email in bootstrap registration", async () => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
      new Response(JSON.stringify(session), {
        headers: { "Content-Type": "application/json" },
        status: 200,
      }),
    );
    vi.stubGlobal("fetch", fetchMock);
    const gateway = new HttpAuthenticationGateway("");
    const command = {
      email: "guardian@example.test",
      familyId,
      familyName: "Example family",
      guardianName: "Alex",
      memberId,
      password: "secret-password",
      username: "alex",
    };

    await expect(gateway.register(command)).resolves.toEqual(session);
    expect(fetchMock).toHaveBeenCalledWith(
      "/api/auth/register",
      expect.objectContaining({ body: JSON.stringify(command) }),
    );
  });

  it("activates an invited account and returns its authenticated session", async () => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
      new Response(JSON.stringify(session), {
        headers: { "Content-Type": "application/json" },
        status: 200,
      }),
    );
    vi.stubGlobal("fetch", fetchMock);
    const gateway = new HttpAuthenticationGateway("");
    const command = {
      activationCode: "one-time-code",
      email: "member@example.test",
      password: "secret-password",
      username: "member",
    };

    await expect(gateway.activate(command)).resolves.toEqual(session);
    expect(fetchMock).toHaveBeenCalledWith(
      "/api/auth/activate",
      expect.objectContaining({ body: JSON.stringify(command) }),
    );
  });

  it("validates the one-time invitation response", async () => {
    const invitation = {
      activationCode: "one-time-code",
      email: "member@example.test",
      expiresAt: "2026-11-05T12:00:00Z",
      memberId: "member-2",
    };
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(
      new Response(JSON.stringify(invitation), {
        headers: { "Content-Type": "application/json" },
        status: 200,
      }),
    );
    vi.stubGlobal("fetch", fetchMock);
    const gateway = new HttpAuthenticationGateway("");

    await expect(gateway.inviteMember({ email: invitation.email, name: "Sam", role: "child" })).resolves.toEqual(
      invitation,
    );
  });

  it("rejects a malformed invitation response", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn<typeof fetch>().mockResolvedValue(
        new Response(
          JSON.stringify({
            activationCode: "one-time-code",
            email: "member@example.test",
            expiresAt: "next week",
            memberId: "member-2",
          }),
          { headers: { "Content-Type": "application/json" }, status: 200 },
        ),
      ),
    );
    const gateway = new HttpAuthenticationGateway("");

    await expect(gateway.inviteMember({ email: "member@example.test", name: "Sam", role: "child" })).rejects.toThrow(
      "The member invitation response is invalid.",
    );
  });
});
