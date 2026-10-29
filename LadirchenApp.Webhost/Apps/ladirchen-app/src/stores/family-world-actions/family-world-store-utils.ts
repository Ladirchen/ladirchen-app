import type { FamilyCurrency } from "@/domain/savings/types";
export const supportedFamilyCurrencies: ReadonlyArray<FamilyCurrency> = ["CHF", "EUR", "HUF"];
export function createUuid(): string {
  return globalThis.crypto.randomUUID();
}
