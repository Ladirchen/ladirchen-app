import router from "@/router";
import i18n from "./i18n";
import { createPinia } from "pinia";
/**
 * plugins/index.ts
 *
 * Automatically included in `./src/main.ts`
 */
// Types
import type { App } from "vue";
// Plugins
import vuetify from "./vuetify";
import { applicationInfoGateway, familyWorldDependencies } from "@/app/composition-root";
import { APPLICATION_INFO_GATEWAY } from "@/shared/composables/use-application-info";
import { createFamilyWorldStoreDefinition, FAMILY_WORLD_STORE_DEFINITION } from "@/stores/family-world";
export function registerPlugins(app: App): void {
  app.use(vuetify);
  const pinia = createPinia();
  pinia.use(() => ({ $familyWorld: familyWorldDependencies }));
  app.use(pinia);
  app.provide(APPLICATION_INFO_GATEWAY, applicationInfoGateway);
  app.provide(
    FAMILY_WORLD_STORE_DEFINITION,
    createFamilyWorldStoreDefinition(familyWorldDependencies.initialDataFactory),
  );
  app.use(i18n);
  app.use(router);
}
