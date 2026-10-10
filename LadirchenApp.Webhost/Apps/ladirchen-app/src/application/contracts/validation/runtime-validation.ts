import { DateTime } from "luxon";
import { isDomainIdValue, isUuidValue } from "@/domain/shared/identifiers";
import type { FamilyId, FamilyMemberId } from "@/domain/shared/identifiers";
export type StateGuard<TState> = (value: unknown) => value is TState;
export function isRecord(value: unknown): value is Record<string, unknown> {

  return typeof value === "object" && value !== null && !Array.isArray(value);
}
export function isNonEmptyString(value: unknown): value is string {

  return typeof value === "string" && value.trim().length > 0;
}
export function isOptionalString(value: unknown): value is string | undefined {

  return value === undefined || typeof value === "string";
}
export function isFiniteNumber(value: unknown): value is number {

  return typeof value === "number" && Number.isFinite(value);
}
export function isInteger(value: unknown): value is number {

  return isFiniteNumber(value) && Number.isInteger(value);
}
export function isNonNegativeInteger(value: unknown): value is number {

  return isInteger(value) && value >= 0;
}
export function isOptionalBoolean(value: unknown): value is boolean | undefined {

  return value === undefined || typeof value === "boolean";
}
export function isOptionalNonNegativeNumber(value: unknown): value is number | undefined {

  return value === undefined || (isFiniteNumber(value) && value >= 0);
}
export function isOptionalPositiveNumber(value: unknown): value is number | undefined {

  return value === undefined || (isFiniteNumber(value) && value >= 1);
}
export function isArrayOf<TItem>(value: unknown, guard: (item: unknown) => item is TItem): value is TItem[] {

  return Array.isArray(value) && value.every(guard);
}
export function hasUniqueIds(
  entities: ReadonlyArray<{
    id: string;
  }>,
): boolean {

  return new Set(entities.map((entity) => entity.id)).size === entities.length;
}
export function isDomainId(value: unknown): value is string {

  return isDomainIdValue(value);
}
export function isFamilyId(value: unknown): value is FamilyId {

  return isUuidValue(value);
}
export function isFamilyMemberId(value: unknown): value is FamilyMemberId {

  return isDomainId(value);
}
export function hasOptionalDomainId(value: unknown): boolean {

  return value === undefined || isDomainId(value);
}
export function isKnownString<TValue extends string>(value: unknown, allowed: ReadonlySet<string>): value is TValue {

  return typeof value === "string" && allowed.has(value);
}
export function exhaustiveValues<TValue extends string>(items: Record<TValue, true>): ReadonlySet<string> {

  return new Set(Object.keys(items));
}
export function values<TValue extends string>(items: readonly TValue[]): ReadonlySet<string> {

  return new Set(items);
}
export function isHexColor(value: unknown): boolean {

  if (typeof value !== "string" || !value.startsWith("#")) {

    return false;
  }

  const hexadecimal = value.slice(1);

  return [3, 4, 6, 8].includes(hexadecimal.length) && /^[\da-f]+$/iu.test(hexadecimal);
}
export function isCalendarDate(value: unknown): value is string {

  if (typeof value !== "string" || !/^\d{4}-\d{2}-\d{2}$/u.test(value)) {

    return false;
  }

  const date = DateTime.fromFormat(value, "yyyy-MM-dd", { zone: "UTC" });

  return date.isValid && date.toFormat("yyyy-MM-dd") === value;
}
export function isIsoDateTime(value: unknown): value is string {

  return (
    typeof value === "string" &&
    /^\d{4}-\d{2}-\d{2}T(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d(?:\.\d{1,9})?(?:Z|[+-](?:[01]\d|2[0-3]):[0-5]\d)$/u.test(
      value,
    ) &&
    isCalendarDate(value.slice(0, 10)) &&
    DateTime.fromISO(value, { setZone: true }).isValid
  );
}
export function isClockTime(value: unknown): value is string {

  return typeof value === "string" && /^(?:[01]\d|2[0-3]):[0-5]\d$/u.test(value);
}
export function hasLocalizedValue(entity: Record<string, unknown>, field: string): boolean {

  return isNonEmptyString(entity[field]) || isNonEmptyString(entity.translationKey);
}
