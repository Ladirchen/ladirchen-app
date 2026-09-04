import type { FamilyId } from '@/domain/types';

export interface FamilyContext {
  readonly activeFamilyId: FamilyId;
}
