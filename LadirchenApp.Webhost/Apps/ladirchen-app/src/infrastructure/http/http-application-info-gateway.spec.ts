import { afterEach, describe, expect, it, vi } from "vite-plus/test";
import { HttpApplicationInfoGateway } from "./http-application-info-gateway";

const info = { semVer: "1.2.3-beta.4", informationalVersion: "1.2.3-beta.4+Branch.main.Sha.abc123" };

describe("HttpApplicationInfoGateway", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("retrieves version metadata from the configured backend without caching", async () => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(Response.json({ ...info, name: "Webhost" }));
    vi.stubGlobal("fetch", fetchMock);

    await expect(new HttpApplicationInfoGateway("https://api.example.test///").getInfo()).resolves.toEqual(info);
    expect(fetchMock).toHaveBeenCalledWith("https://api.example.test/about", {
      cache: "no-store",
      credentials: "include",
      method: "GET",
    });
  });

  it("supports a release version on the current origin", async () => {
    const release = { semVer: "1.2.3", informationalVersion: "1.2.3+abc123" };
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(Response.json(release));
    vi.stubGlobal("fetch", fetchMock);

    await expect(new HttpApplicationInfoGateway("").getInfo()).resolves.toEqual(release);
    expect(fetchMock.mock.calls[0]?.[0]).toBe("/about");
  });

  it.each([
    null,
    {},
    { semVer: "Unknown SemVer", informationalVersion: "Unknown SemVer" },
    { semVer: "1.2.3", informationalVersion: null },
    { semVer: "1.2.3", informationalVersion: "9.0.0+sha" },
    { semVer: " ", informationalVersion: " " },
  ])("rejects invalid backend metadata: %j", async (value) => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(Response.json(value)));

    await expect(new HttpApplicationInfoGateway("").getInfo()).rejects.toThrow(TypeError);
  });

  it("surfaces non-success status codes", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(null, { status: 503 })));

    await expect(new HttpApplicationInfoGateway("").getInfo()).rejects.toThrow("status 503");
  });

  it("surfaces network failures", async () => {
    vi.stubGlobal("fetch", vi.fn().mockRejectedValue(new Error("Offline")));

    await expect(new HttpApplicationInfoGateway("").getInfo()).rejects.toThrow("Offline");
  });
});
