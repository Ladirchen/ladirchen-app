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
import "@/theme/color-palette.css";
import "@/styles/fonts.scss";
import "@/styles/main.scss";

window.addEventListener("vite:preloadError", (event) => {
  event.preventDefault();
  window.location.reload();
});

const legacyHashPath = window.location.hash.startsWith("#/") ? window.location.hash.slice(1) : undefined;

if (legacyHashPath) {
  const appPath = legacyHashPath === "/" ? "/app" : `/app${legacyHashPath}`;
  window.history.replaceState(null, "", `${appPath}${window.location.search}`);
}

const app = createApp(App);

registerPlugins(app);

void initializeI18n()
  .catch((error: unknown) => {
    console.error("Locale messages could not be loaded.", error);
  })
  .finally(() => {
    app.mount("#app");
  });
