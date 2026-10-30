import UnoCSS from "unocss/vite";
import Vue from "@vitejs/plugin-vue";
import { VitePWA } from "vite-plugin-pwa";
import Vuetify, { transformAssetUrls } from "vite-plugin-vuetify";
import { defineConfig, lazyPlugins } from "vite-plus";
import { fileURLToPath, URL } from "node:url";

const enableProductionDevtools = process.env.ENABLE_VUE_DEVTOOLS === "true";

// https://vitejs.dev/config/
export default defineConfig({
  test: {
    // Vitest v4 compatibility: preserve mock call history.
    // Remove after tests no longer rely on calls from setup or earlier tests.
    // https://viteplus.dev/guide/vitest-v5#remove-unneeded-compatibility-settings
    // https://vitest.dev/guide/migration/#clearmocks-is-enabled-by-default
    clearMocks: false,
  },
  plugins: lazyPlugins(() => [
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
    VitePWA({
      includeAssets: ["favicon.ico", "ladirchen-logo.png"],
      manifest: {
        background_color: "#f7fbf9",
        description: "Die gemeinsame Familienwelt für Aufgaben, Wünsche, Sparziele und Belohnungen.",
        display: "standalone",
        icons: [
          {
            src: "/pwa-192x192.png",
            sizes: "192x192",
            type: "image/png",
          },
          {
            src: "/pwa-512x512.png",
            sizes: "512x512",
            type: "image/png",
          },
          {
            purpose: "maskable",
            src: "/pwa-maskable-512x512.png",
            sizes: "512x512",
            type: "image/png",
          },
        ],
        id: "/",
        lang: "de",
        name: "Ladirchen – Familienwelt",
        scope: "/",
        short_name: "Ladirchen",
        start_url: "/",
        theme_color: "#08766a",
      },
      registerType: "autoUpdate",
      workbox: {
        cleanupOutdatedCaches: true,
        navigateFallback: "/index.html",
        navigateFallbackDenylist: [/^\/api\//, /^\/health\//, /^\/hubs\//],
      },
    }),
  ]),
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
