import { nextTick, ref } from "vue";
import { describe, expect, it } from "vitest";

import { useLazyAssetUrl } from "./use-lazy-asset-url";

describe("useLazyAssetUrl", () => {
  it("ignores an in-flight result after its source is cleared", async () => {
    let resolveAsset: (url: string) => void = () => {};
    const pendingAsset = new Promise<string>((resolve) => {
      resolveAsset = resolve;
    });
    const source = ref<string | undefined>("asset");
    const url = useLazyAssetUrl(source, () => pendingAsset);

    source.value = undefined;
    await nextTick();
    resolveAsset("stale-url");
    await pendingAsset;
    await nextTick();

    expect(url.value).toBeUndefined();
  });
});
