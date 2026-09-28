/**
 * main.ts
 *
 * Bootstraps Vuetify and other plugins then mounts the App`
 */

// Composables
import { createApp } from "vue";

// Plugins
import { registerPlugins } from "@/plugins";
import { initializeI18n } from "@/plugins/i18n";

// Components
import App from "./App.vue";

// Styles
import "virtual:uno.css";
import "./theme/color-palette.css";
import "./styles/fonts.scss";
import "./styles/main.scss";

const app = createApp(App);

registerPlugins(app);

void initializeI18n()
  .catch((error: unknown) => {
    console.error("Locale messages could not be loaded.", error);
  })
  .finally(() => {
    app.mount("#app");
  });
