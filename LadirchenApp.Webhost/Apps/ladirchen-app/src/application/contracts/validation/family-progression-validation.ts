import type { FamilyProgressionState } from '../family-progression-contract';
import { HOUSE_STAGES } from '@/domain/house/catalog';
import {
  isInteger,
  isNonNegativeInteger,
  isRecord,
  type StateGuard,
} from './runtime-validation';

const stageLevels = new Set<number>(HOUSE_STAGES.map(item => item.level));

export const isFamilyProgressionState: StateGuard<FamilyProgressionState> = (value): value is FamilyProgressionState =>
  isRecord(value) &&
  isNonNegativeInteger(value.completedWeeklyStreak) &&
  isInteger(value.currentWeekTarget) && value.currentWeekTarget > 0 &&
  isInteger(value.houseLevel) && stageLevels.has(value.houseLevel);
