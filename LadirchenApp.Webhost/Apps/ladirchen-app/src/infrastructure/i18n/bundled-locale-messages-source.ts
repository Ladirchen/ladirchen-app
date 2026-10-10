import type { LocaleMessagesSource } from "@/application/ports/locale-messages-source";
import type { SupportedLocale } from "@/locales";
import type { LocaleMessageSchema } from "@/locales/translation-keys";
const bundledLocaleImports: Readonly<
  Record<
    SupportedLocale,
    () => Promise<{
      default: LocaleMessageSchema;
    }>
  >
> = {
  async de(this: void) {

    return import("@/locales/de.json");
  },
  async en(this: void) {

    return import("@/locales/en.json");
  },
};
export function createBundledLocaleMessagesSource(): LocaleMessagesSource {

  return {
    async load(locale) {

      return (await bundledLocaleImports[locale]()).default;
    },
  };
}
