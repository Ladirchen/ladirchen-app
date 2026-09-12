import type { LocaleMessageSchema } from '@/locales/translation-keys';

declare module 'vue-i18n' {
  export interface DefineLocaleMessage extends LocaleMessageSchema {}
}
