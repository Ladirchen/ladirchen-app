import type { SupportedLocale } from "@/locales";
import type { LocaleMessageSchema } from "@/locales/translation-keys";

export interface LocaleMessagesSource {
  load: (locale: SupportedLocale) => Promise<LocaleMessageSchema>;
}
