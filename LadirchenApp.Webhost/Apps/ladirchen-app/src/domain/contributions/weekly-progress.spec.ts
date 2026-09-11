import { describe, expect, it } from 'vitest';

import { createIanaTimeZone } from '../family/time-zone';
import { createDomainId } from '../shared/identifiers';
import type { Contribution } from './types';
import { currentWeekDaysFromContributions } from './weekly-progress';

const approved = (id: string, approvedAt: string): Contribution => ({
  approvedAt,
  area: '',
  description: '',
  dueLabel: '',
  energy: 10,
  icon: '',
  id: createDomainId.contribution(id),
  kind: 'basic',
  reward: 10,
  status: 'approved',
  title: '',
});

describe('currentWeekDaysFromContributions', () => {
  it('counts distinct approval days in the family time zone', () => {
    const contributions = [
      approved('one', '2026-09-07T07:00:00.000Z'),
      approved('two', '2026-09-07T17:00:00.000Z'),
      approved('three', '2026-09-08T07:00:00.000Z'),
      approved('previous-week', '2026-09-06T17:00:00.000Z'),
    ];
    expect(currentWeekDaysFromContributions(
      contributions,
      createIanaTimeZone('Europe/Zurich'),
      new Date('2026-09-10T10:00:00.000Z'),
    )).toBe(2);
  });
});
