import { describe, expect, it, vi } from "vite-plus/test";

import { copyTextToClipboard } from "./clipboard";

describe("copyTextToClipboard", () => {
  it("copies text through the available clipboard", async () => {
    const writeText = vi.fn(async () => undefined);

    await expect(copyTextToClipboard("activation-code", { writeText })).resolves.toBe(true);
    expect(writeText).toHaveBeenCalledWith("activation-code");
  });

  it("fails safely when no clipboard is available", async () => {
    await expect(copyTextToClipboard("activation-code", null)).resolves.toBe(false);
  });

  it("fails safely when clipboard access is rejected", async () => {
    const writeText = vi.fn(async () => {
      throw new DOMException("Clipboard access denied");
    });

    await expect(copyTextToClipboard("activation-code", { writeText })).resolves.toBe(false);
  });
});
