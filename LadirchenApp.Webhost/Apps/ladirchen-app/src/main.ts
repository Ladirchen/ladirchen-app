/**
 * main.ts
 *
 * Bootstraps Vuetify and other plugins then mounts the App`
 */

// Composables
import { createApp } from "vue";

// Plugins
import { registerPlugins } from "@/plugins";

// Components
import App from "./App.vue";
import { installCssColorPalette } from "./theme/color-palette";

// Styles
import "unfonts.css";
import "virtual:uno.css";
import "./styles/main.scss";

installCssColorPalette();
const app = createApp(App);

registerPlugins(app);

app.mount("#app");
