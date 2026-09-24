/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Composables
import { createVuetify } from "vuetify";
import { aliases, mdi } from "vuetify/iconsets/mdi-unocss";
// Styles
import "vuetify/styles";
import { applicationColorPalette } from "@/theme/color-palette";

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  display: {
    mobileBreakpoint: "md",
    thresholds: {
      xs: 0,
      sm: 600,
      md: 1024,
      lg: 1368,
      xl: 1920,
      xxl: 2560,
    },
  },
  icons: {
    aliases,
    defaultSet: "mdi",
    sets: { mdi },
  },
  theme: {
    defaultTheme: "ladirchen",
    themes: {
      ladirchen: {
        dark: false,
        colors: {
          background: applicationColorPalette.background,
          surface: applicationColorPalette.surface,
          primary: applicationColorPalette.primary,
          secondary: applicationColorPalette.secondary,
          info: applicationColorPalette.info,
          warning: applicationColorPalette.warning,
          error: applicationColorPalette.danger,
        },
      },
    },
  },
});
