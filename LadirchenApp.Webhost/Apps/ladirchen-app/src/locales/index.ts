import de from './de';
import en from './en';

export const localeMessages = { de, en } as const;

export type SupportedLocale = keyof typeof localeMessages;

export interface LocaleOption {
  readonly code: SupportedLocale;
  readonly icon: string;
  readonly label: string;
}

export const DEFAULT_LOCALE: SupportedLocale = 'de';
export const localeOptions: ReadonlyArray<LocaleOption> = [
  { code: 'de', icon: '🇩🇪', label: 'Deutsch' },
  { code: 'en', icon: '🇬🇧', label: 'English' },
];

export const isSupportedLocale = (value: string | null | undefined): value is SupportedLocale =>
  value !== null && value !== undefined && Object.hasOwn(localeMessages, value);
