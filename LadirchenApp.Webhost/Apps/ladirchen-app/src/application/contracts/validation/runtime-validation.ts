import { isDomainIdValue, isUuidValue } from '@/domain/shared/identifiers';
import type { FamilyId, FamilyMemberId } from '@/domain/shared/identifiers';

export type StateGuard<TState> = (value: unknown) => value is TState;

export const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null && !Array.isArray(value);
export const isNonEmptyString = (value: unknown): value is string =>
  typeof value === 'string' && value.trim().length > 0;
export const isOptionalString = (value: unknown): value is string | undefined =>
  value === undefined || typeof value === 'string';
export const isFiniteNumber = (value: unknown): value is number =>
  typeof value === 'number' && Number.isFinite(value);
export const isInteger = (value: unknown): value is number =>
  isFiniteNumber(value) && Number.isInteger(value);
export const isNonNegativeInteger = (value: unknown): value is number =>
  isInteger(value) && value >= 0;
export const isOptionalBoolean = (value: unknown): value is boolean | undefined =>
  value === undefined || typeof value === 'boolean';
export const isOptionalNonNegativeNumber = (value: unknown): value is number | undefined =>
  value === undefined || (isFiniteNumber(value) && value >= 0);
export const isOptionalPositiveNumber = (value: unknown): value is number | undefined =>
  value === undefined || (isFiniteNumber(value) && value >= 1);
export const isArrayOf = <TItem>(value: unknown, guard: (item: unknown) => item is TItem): value is TItem[] =>
  Array.isArray(value) && value.every(guard);
export const hasUniqueIds = (entities: ReadonlyArray<{ id: string }>): boolean =>
  new Set(entities.map(entity => entity.id)).size === entities.length;

export const isDomainId = (value: unknown): value is string => isDomainIdValue(value);
export const isFamilyId = (value: unknown): value is FamilyId => isUuidValue(value);
export const isFamilyMemberId = (value: unknown): value is FamilyMemberId => isDomainId(value);
export const hasOptionalDomainId = (value: unknown): boolean => value === undefined || isDomainId(value);

export const isKnownString = <TValue extends string>(value: unknown, allowed: ReadonlySet<string>): value is TValue =>
  typeof value === 'string' && allowed.has(value);
export const exhaustiveValues = <TValue extends string>(items: Record<TValue, true>): ReadonlySet<string> =>
  new Set(Object.keys(items));
export const values = <TValue extends string>(items: readonly TValue[]): ReadonlySet<string> => new Set(items);

export const isHexColor = (value: unknown): boolean => {
  if (typeof value !== 'string' || !value.startsWith('#')) {return false;}
  const hexadecimal = value.slice(1);
  return [3, 4, 6, 8].includes(hexadecimal.length) && /^[\da-f]+$/iu.test(hexadecimal);
};
export const isCalendarDate = (value: unknown): value is string => {
  if (typeof value !== 'string' || !/^\d{4}-\d{2}-\d{2}$/u.test(value)) {return false;}
  const date = new Date(`${value}T00:00:00.000Z`);
  return !Number.isNaN(date.getTime()) && date.toISOString().startsWith(value);
};
export const isIsoDateTime = (value: unknown): value is string =>
  typeof value === 'string' &&
  /^\d{4}-\d{2}-\d{2}T(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d(?:\.\d{1,9})?(?:Z|[+-](?:[01]\d|2[0-3]):[0-5]\d)$/u.test(value) &&
  isCalendarDate(value.slice(0, 10)) &&
  !Number.isNaN(Date.parse(value));
export const isClockTime = (value: unknown): value is string =>
  typeof value === 'string' && /^(?:[01]\d|2[0-3]):[0-5]\d$/u.test(value);
export const hasLocalizedValue = (entity: Record<string, unknown>, field: string): boolean =>
  isNonEmptyString(entity[field]) || isNonEmptyString(entity.translationKey);
