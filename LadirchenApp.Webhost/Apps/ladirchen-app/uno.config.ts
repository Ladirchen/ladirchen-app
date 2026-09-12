import { defineConfig, transformerDirectives } from "unocss";
import { presetVuetify } from "unocss-preset-vuetify";

export default defineConfig({
  presets: [
    presetVuetify({
      font: {
        heading: "Roboto, sans-serif",
        body: "Roboto, sans-serif",
        mono: '"Roboto Mono", sans-serif',
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
  transformers: [transformerDirectives()],
  safelist: [
    ...Array.from({ length: 6 }, (_, i) => `elevation-${i}`),
    ...["", "-0", "-sm", "-lg", "-xl", "-pill", "-circle", "-shaped"].map((suffix) => `rounded${suffix}`),
  ],
  outputToCssLayers: {
    cssLayerName: (layer) => (layer === "properties" ? null : `uno-${layer}`),
  },
});
