import router from "../router";
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
import { familyWorldDependencies } from '@/app/composition-root';

export function registerPlugins(app: App) {
  app.use(vuetify);
  const pinia = createPinia();
  pinia.use(() => ({ $familyWorld: familyWorldDependencies }));
  app.use(pinia);
  app.use(i18n);
  app.use(router);
}
