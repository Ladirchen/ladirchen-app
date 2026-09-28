import type { LocaleMessagesSource } from "@/application/ports/locale-messages-source";
import type { SupportedLocale } from "@/locales";
import type { LocaleMessageSchema } from "@/locales/translation-keys";

const bundledLocaleImports: Readonly<Record<SupportedLocale, () => Promise<{ default: LocaleMessageSchema }>>> = {
  de: async () => import("@/locales/de.json"),
  en: async () => import("@/locales/en.json"),
};

export function createBundledLocaleMessagesSource(): LocaleMessagesSource {
  return {
    async load(locale) {
      return (await bundledLocaleImports[locale]()).default;
    },
  };
}
