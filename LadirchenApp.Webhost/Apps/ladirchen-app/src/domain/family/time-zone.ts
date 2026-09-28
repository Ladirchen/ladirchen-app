import { IANAZone } from "luxon";

declare const ianaTimeZoneBrand: unique symbol;

export type IanaTimeZone = string & { readonly [ianaTimeZoneBrand]: "iana-time-zone" };

export const isIanaTimeZone = (value: unknown): value is IanaTimeZone => {
  return typeof value === "string" && IANAZone.isValidZone(value);
};

export const createIanaTimeZone = (value: string): IanaTimeZone => {
  if (!isIanaTimeZone(value)) {throw new TypeError(`Invalid IANA time zone: ${value}`);}
  return value;
};

export const DEFAULT_FAMILY_TIME_ZONE = createIanaTimeZone("Europe/Zurich");
