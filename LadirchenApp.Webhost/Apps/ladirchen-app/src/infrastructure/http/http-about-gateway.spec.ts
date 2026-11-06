import { afterEach, beforeEach, describe, expect, it, vi } from "vite-plus/test";
import { Client } from "@api/LadirchenApp.Webhost";
import { fetchApplicationVersion } from "./http-about-gateway";

describe("fetchApplicationVersion", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
    vi.stubGlobal("window", {});
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("returns the backend semantic version", async () => {
    vi.spyOn(Client.prototype, "getAbout").mockResolvedValue({ semVer: "1.2.3" });

    await expect(fetchApplicationVersion("/api")).resolves.toBe("1.2.3");
    expect(Client.prototype.getAbout).toHaveBeenCalledOnce();
  });

  it("returns null when the version is missing", async () => {
    vi.spyOn(Client.prototype, "getAbout").mockResolvedValue({});

    await expect(fetchApplicationVersion("/api")).resolves.toBeNull();
  });
});
