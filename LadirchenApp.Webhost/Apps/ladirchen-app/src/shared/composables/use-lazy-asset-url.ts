import { ref, watch } from "vue";
import type { ComputedRef, Ref } from "vue";

/** Resolves an async asset URL reactively; keeps the previous URL visible while the next one loads. */
export function useLazyAssetUrl<T>(
  source: Ref<T | undefined> | ComputedRef<T | undefined>,
  resolve: (value: T) => Promise<string>,
): Ref<string | undefined> {
  const url = ref<string>();
  let requestId = 0;
  watch(
    source,
    async (value) => {
      if (value === undefined) {
        url.value = undefined;
        return;
      }
      const currentRequest = ++requestId;
      const resolvedUrl = await resolve(value);
      if (currentRequest === requestId) {
        url.value = resolvedUrl;
      }
    },
    { immediate: true },
  );
  return url;
}
