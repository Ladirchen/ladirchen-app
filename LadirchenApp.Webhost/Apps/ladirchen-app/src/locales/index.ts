export const SUPPORTED_LOCALES = ["de", "en"] as const;

export type SupportedLocale = (typeof SUPPORTED_LOCALES)[number];

export interface LocaleOption {
  readonly code: SupportedLocale;
  readonly icon: string;
  readonly label: string;
}

export const DEFAULT_LOCALE: SupportedLocale = "de";
export const localeOptions: ReadonlyArray<LocaleOption> = [
  { code: "de", icon: "🇩🇪", label: "Deutsch" },
  { code: "en", icon: "🇬🇧", label: "English" },
];

export function isSupportedLocale(value: string | null | undefined): value is SupportedLocale {
  return (SUPPORTED_LOCALES as ReadonlyArray<string | null | undefined>).includes(value);
}
