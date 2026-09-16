import { describe, expect, it } from 'vitest';

import { createIanaTimeZone } from '../family/time-zone';
import { calendarDateInTimeZone, isInstantInIsoWeek, isSameCalendarDay, startOfIsoWeek } from './zoned-calendar';

describe('zoned calendar', () => {
  const zurich = createIanaTimeZone('Europe/Zurich');
  const newYork = createIanaTimeZone('America/New_York');

  it('uses the family time zone instead of the browser time zone', () => {
    const instant = new Date('2026-09-06T22:30:00.000Z');
    expect(calendarDateInTimeZone(instant, zurich)).toBe('2026-09-07');
    expect(calendarDateInTimeZone(instant, newYork)).toBe('2026-09-06');
    expect(startOfIsoWeek(instant, zurich)).toBe('2026-09-07');
    expect(startOfIsoWeek(instant, newYork)).toBe('2026-08-31');
  });

  it('compares days and weeks in the supplied time zone', () => {
    const reference = new Date('2026-09-07T00:30:00.000Z');
    const value = '2026-09-06T22:30:00.000Z';
    expect(isSameCalendarDay(value, reference, zurich)).toBe(true);
    expect(isInstantInIsoWeek(value, reference, zurich)).toBe(true);
    expect(isInstantInIsoWeek(value, reference, newYork)).toBe(true);
  });
});
