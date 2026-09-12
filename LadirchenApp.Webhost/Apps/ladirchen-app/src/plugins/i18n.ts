import { createI18n } from 'vue-i18n';

import { DEFAULT_LOCALE, isSupportedLocale, localeMessages } from '@/locales';
import type { SupportedLocale } from '@/locales';
import { browserClientStorage } from '@/infrastructure/storage/browser-client-storage';

export type { SupportedLocale } from '@/locales';

const LOCALE_STORAGE_KEY = 'ladirchen:locale';
const resolveInitialLocale = (): SupportedLocale => {
  const storedLocale = browserClientStorage.getItem(LOCALE_STORAGE_KEY);
  if (isSupportedLocale(storedLocale)) {return storedLocale;}
  if (typeof navigator !== 'undefined') {
    const language = navigator.language.toLowerCase().split('-')[0];
    if (isSupportedLocale(language)) {return language;}
  }
  return DEFAULT_LOCALE;
};

const i18n = createI18n({
  legacy: false,
  locale: resolveInitialLocale(),
  fallbackLocale: DEFAULT_LOCALE,
  messages: localeMessages,
});

export const setActiveLocale = (locale: SupportedLocale): void => {
  i18n.global.locale.value = locale;
  browserClientStorage.setItem(LOCALE_STORAGE_KEY, locale);
  if (typeof document !== 'undefined') {document.documentElement.lang = locale;}
};

if (typeof document !== 'undefined') {document.documentElement.lang = i18n.global.locale.value;}

export default i18n;
