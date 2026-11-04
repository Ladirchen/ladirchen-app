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
      includeAssets: ["apple-touch-icon.png", "favicon.ico", "ladirchen-logo.png"],
      manifest: {
        background_color: "#f7fbf9",
        description: "Die gemeinsame Familienwelt für Aufgaben, Wünsche, Sparziele und Belohnungen.",
        display: "standalone",
        icons: [
          {
            purpose: "any",
            src: "/ladirli-icon-192x192.png",
            sizes: "192x192",
            type: "image/png",
          },
          {
            purpose: "any",
            src: "/ladirli-icon-512x512.png",
            sizes: "512x512",
            type: "image/png",
          },
          {
            purpose: "maskable",
            src: "/ladirli-icon-maskable-512x512.png",
            sizes: "512x512",
            type: "image/png",
          },
        ],
        id: "/app",
        lang: "de",
        name: "Ladirli – Familienwelt",
        scope: "/app",
        short_name: "Ladirli",
        start_url: "/app",
        theme_color: "#08766a",
      },
      registerType: "autoUpdate",
      workbox: {
        cleanupOutdatedCaches: true,
        globIgnores: [
          "**/*Page-*.css",
          "**/*Page-*.js",
          "**/*Dialog-*.css",
          "**/*Dialog-*.js",
          "**/DevelopmentToolsPanel-*.css",
          "**/DevelopmentToolsPanel-*.js",
          "**/PixiRoomScene-*.js",
          "**/CanvasPool-*.js",
          "**/GraphicsContext-*.js",
          "**/RenderTargetSystem-*.js",
          "**/GCManagedHash-*.js",
          "**/getTexelRangeRects-*.js",
          "**/canvasUtils-*.js",
        ],
        importScripts: ["/push-notifications-sw.js"],
        navigateFallback: "/index.html",
        navigateFallbackDenylist: [/^\/api\//, /^\/health\//, /^\/hubs\//, /^\/robots\.txt$/, /^\/sitemap\.xml$/],
        runtimeCaching: [
          {
            urlPattern: ({ request, url }) =>
              url.pathname.startsWith("/assets/") && ["font", "script", "style"].includes(request.destination),
            handler: "CacheFirst",
            options: {
              cacheName: "ladirli-runtime-code",
              cacheableResponse: {
                statuses: [0, 200],
              },
              expiration: {
                maxAgeSeconds: 30 * 24 * 60 * 60,
                maxEntries: 120,
              },
            },
          },
          {
            urlPattern: ({ request, url }) => url.pathname.startsWith("/assets/") && request.destination === "image",
            handler: "CacheFirst",
            options: {
              cacheName: "ladirli-runtime-images",
              cacheableResponse: {
                statuses: [0, 200],
              },
              expiration: {
                maxAgeSeconds: 30 * 24 * 60 * 60,
                maxEntries: 160,
              },
            },
          },
        ],
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
