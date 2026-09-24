import type { FamilyCurrency } from "@/domain/savings/types";

export const AUTH_STATE_KEY = "ladirchen:auth-state";
export const supportedFamilyCurrencies: ReadonlyArray<FamilyCurrency> = ["CHF", "EUR", "HUF"];
export const createUuid = (): string => globalThis.crypto.randomUUID();
