import { describe, expect, it } from 'vitest';

import { mergeSerializableAggregateState } from './three-way-aggregate-merge';

describe('mergeSerializableAggregateState', () => {
  it('keeps remote fields that were not changed locally', () => {
    const base = { currency: 'CHF', rate: 10 };
    const local = { currency: 'CHF', rate: 12 };
    const remote = { currency: 'EUR', rate: 10 };

    expect(mergeSerializableAggregateState(base, local, remote)).toEqual({ currency: 'EUR', rate: 12 });
  });

  it('merges unrelated changes in ID-based collections', () => {
    const base = { contributions: [{ id: 'one', title: 'One', status: 'available' }, { id: 'two', title: 'Two', status: 'available' }] };
    const local = { contributions: [{ id: 'one', title: 'One', status: 'approved' }, { id: 'two', title: 'Two', status: 'available' }] };
    const remote = { contributions: [{ id: 'one', title: 'One', status: 'available' }, { id: 'two', title: 'Updated', status: 'available' }, { id: 'three', title: 'Three', status: 'available' }] };

    expect(mergeSerializableAggregateState(base, local, remote)).toEqual({
      contributions: [
        { id: 'one', title: 'One', status: 'approved' },
        { id: 'two', title: 'Updated', status: 'available' },
        { id: 'three', title: 'Three', status: 'available' },
      ],
    });
  });
});
