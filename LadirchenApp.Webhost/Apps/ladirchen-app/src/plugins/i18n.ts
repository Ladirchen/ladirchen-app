import { createI18n } from "vue-i18n";

import { localeMessagesSource } from "@/app/composition-root";
import { DEFAULT_LOCALE, isSupportedLocale } from "@/locales";
import type { SupportedLocale } from "@/locales";
import { browserClientStorage } from "@/infrastructure/storage/browser-client-storage";

export type { SupportedLocale } from "@/locales";

const LOCALE_STORAGE_KEY = "ladirchen:locale";
function resolveInitialLocale(): SupportedLocale {
  const storedLocale = browserClientStorage.getItem(LOCALE_STORAGE_KEY);
  if (isSupportedLocale(storedLocale)) {
    return storedLocale;
  }
  if (typeof navigator !== "undefined") {
    const language = navigator.language.toLowerCase().split("-")[0];
    if (isSupportedLocale(language)) {
      return language;
    }
  }
  return DEFAULT_LOCALE;
}

const i18n = createI18n({
  legacy: false,
  locale: DEFAULT_LOCALE,
  fallbackLocale: DEFAULT_LOCALE,
  flatJson: true,
});

const pendingLocaleLoads = new Map<SupportedLocale, Promise<void>>();
async function ensureLocaleMessages(locale: SupportedLocale): Promise<void> {
  let pending = pendingLocaleLoads.get(locale);
  if (!pending) {
    pending = localeMessagesSource.load(locale).then((messages) => {
      // flatJson unflattens in place, so keep the source object untouched.
      i18n.global.setLocaleMessage(locale, { ...messages });
    });
    pendingLocaleLoads.set(locale, pending);
    pending.catch(() => pendingLocaleLoads.delete(locale));
  }
  return pending;
}

function applyLocale(locale: SupportedLocale): void {
  i18n.global.locale.value = locale;
  if (typeof document !== "undefined") {
    document.documentElement.lang = locale;
  }
}

export async function initializeI18n(): Promise<void> {
  const initialLocale = resolveInitialLocale();
  await Promise.all([ensureLocaleMessages(DEFAULT_LOCALE), ensureLocaleMessages(initialLocale)]);
  applyLocale(initialLocale);
}

export async function setActiveLocale(locale: SupportedLocale): Promise<void> {
  await ensureLocaleMessages(locale);
  applyLocale(locale);
  browserClientStorage.setItem(LOCALE_STORAGE_KEY, locale);
}

export default i18n;
