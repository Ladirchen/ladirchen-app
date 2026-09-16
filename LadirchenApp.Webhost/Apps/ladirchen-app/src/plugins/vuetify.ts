/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Composables
import { createVuetify } from 'vuetify';
// Styles
import '@mdi/font/css/materialdesignicons.css';
import 'vuetify/styles';
import { applicationColorPalette } from '@/theme/color-palette';

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  theme: {
    defaultTheme: 'ladirchen',
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
