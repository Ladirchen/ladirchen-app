import UnoCSS from "unocss/vite";
import Vue from "@vitejs/plugin-vue";
import Vuetify, { transformAssetUrls } from "vite-plugin-vuetify";
import { defineConfig } from "vite-plus";
import { fileURLToPath, URL } from "node:url";

const enableProductionDevtools = process.env.ENABLE_VUE_DEVTOOLS === "true";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    Vue({
      template: { transformAssetUrls },
    }), // https://github.com/vuetifyjs/vuetify-loader/tree/master/packages/vite-plugin#readme
    Vuetify({
      autoImport: true,
      styles: {
        configFile: "src/styles/settings.scss",
      },
    }),
    UnoCSS(),
  ],
  define: {
    "process.env": {},
    __VUE_PROD_DEVTOOLS__: enableProductionDevtools,
  },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("src", import.meta.url)),
    },
    extensions: [".js", ".json", ".jsx", ".mjs", ".ts", ".tsx", ".vue"],
  },
  server: {
    host: true,
    port: 3000,
    strictPort: true,
  },
});
