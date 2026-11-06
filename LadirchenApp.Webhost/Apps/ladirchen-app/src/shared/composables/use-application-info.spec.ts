import { afterEach, describe, expect, it, vi } from "vite-plus/test";
import { useApplicationInfo } from "./use-application-info";

describe("useApplicationInfo", () => {
  afterEach(() => vi.restoreAllMocks());

  it("starts loading and displays only the backend result", async () => {
    const info = { semVer: "1.2.3", informationalVersion: "1.2.3+abc" };
    const state = useApplicationInfo({ getInfo: vi.fn().mockResolvedValue(info) });

    expect(state.loading.value).toBe(true);
    expect(state.info.value).toBeUndefined();
    await state.refresh();
    expect(state.info.value).toEqual(info);
    expect(state.loading.value).toBe(false);
    expect(state.failed.value).toBe(false);
  });

  it("reports failure and allows a successful retry without showing a stale version", async () => {
    const error = new Error("Offline");
    const report = vi.spyOn(console, "error").mockImplementation(() => {});
    const info = { semVer: "1.2.3", informationalVersion: "1.2.3+abc" };
    const getInfo = vi.fn().mockResolvedValueOnce(info).mockRejectedValueOnce(error).mockResolvedValueOnce(info);
    const state = useApplicationInfo({ getInfo });
    await state.refresh();
    await state.refresh();

    expect(state.failed.value).toBe(true);
    expect(state.loading.value).toBe(false);
    expect(state.info.value).toBeUndefined();
    expect(report).toHaveBeenCalledWith("Could not load the backend application version.", error);
    await state.refresh();
    expect(state.failed.value).toBe(false);
    expect(state.info.value).toEqual(info);
    expect(getInfo).toHaveBeenCalledTimes(3);
  });
});
