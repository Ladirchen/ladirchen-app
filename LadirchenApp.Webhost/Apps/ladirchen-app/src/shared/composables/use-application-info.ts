import { ref } from "vue";
import type { InjectionKey } from "vue";
import type { ApplicationInfo, ApplicationInfoGateway } from "@/application/ports/application-info-gateway";

export const APPLICATION_INFO_GATEWAY: InjectionKey<ApplicationInfoGateway> = Symbol("ApplicationInfoGateway");

export function useApplicationInfo(gateway: ApplicationInfoGateway) {
  const info = ref<ApplicationInfo>();
  const loading = ref(true);
  const failed = ref(false);

  async function refresh() {
    loading.value = true;
    failed.value = false;
    info.value = undefined;

    try {
      info.value = await gateway.getInfo();
    } catch (error) {
      failed.value = true;
      console.error("Could not load the backend application version.", error);
    } finally {
      loading.value = false;
    }
  }

  return { info, loading, failed, refresh };
}
