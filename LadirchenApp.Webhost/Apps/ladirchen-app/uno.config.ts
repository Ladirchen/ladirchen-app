import { defineConfig, presetIcons, transformerDirectives } from "unocss";
import { presetVuetify } from "unocss-preset-vuetify";
import { aliases as vuetifyIconAliases } from "vuetify/iconsets/mdi-unocss";

const vuetifyIconSafelist = Object.values(vuetifyIconAliases).filter(
  (icon): icon is string => typeof icon === "string",
);

const applicationIconSafelist = [
  "i-mdi:account-star-outline",
  "i-mdi:calendar-star",
  "i-mdi:chart-line",
  "i-mdi:check-circle-outline",
  "i-mdi:creation-outline",
  "i-mdi:door-open",
  "i-mdi:flower-outline",
  "i-mdi:gift-outline",
  "i-mdi:hand-heart-outline",
  "i-mdi:home-city-outline",
  "i-mdi:progress-clock",
  "i-mdi:sofa-outline",
  "i-mdi:star-four-points",
  "i-mdi:view-grid-plus-outline",
  "i-mdi:wallet-plus-outline",
];

export default defineConfig({
  presets: [
    presetIcons({ scale: 1.2 }),
    presetVuetify({
      font: {
        heading: "Roboto, sans-serif",
        body: "Roboto, sans-serif",
        mono: "\"Roboto Mono\", sans-serif",
      },
      typography: "md3",
      elevation: "md3",
    }),
  ],
  rules: [
    ["d-grid", { display: "grid" }],
    ["d-inline-grid", { display: "inline-grid" }],
    ["position-sticky", { position: "sticky" }],
    ["overflow-x-auto", { "overflow-x": "auto" }],
    ["overflow-y-auto", { "overflow-y": "auto" }],
    ["overflow-visible", { overflow: "visible" }],
    ["align-stretch", { "align-items": "stretch" }],
    ["place-center", { "place-items": "center" }],
    ["place-end-center", { "place-items": "end center" }],
    ["justify-self-center", { "justify-self": "center" }],
    ["cursor-grab", { cursor: "grab" }],
    ["cursor-grabbing", { cursor: "grabbing" }],
    ["inset-0", { inset: "0" }],
    ["select-none", { "user-select": "none" }],
    ["font-weight-950", { "font-weight": "950" }],
  ],
  transformers: [transformerDirectives({ applyVariable: ["--uno"] })],
  safelist: [
    ...vuetifyIconSafelist,
    ...applicationIconSafelist,
    ...Array.from({ length: 6 }, (_, i) => `elevation-${i}`),
    ...["", "-0", "-sm", "-lg", "-xl", "-pill", "-circle", "-shaped"].map((suffix) => `rounded${suffix}`),
  ],
  outputToCssLayers: {
    cssLayerName: (layer) => (layer === "properties" ? null : `uno-${layer}`),
  },
});
