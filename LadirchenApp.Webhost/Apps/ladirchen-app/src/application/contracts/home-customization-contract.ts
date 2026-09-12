import type { HouseAccessoryId, HouseThemeId } from '@/domain/house';
import type { FamilyId, FamilyMemberId, HouseLayoutPlacement } from '@/domain/types';

export const HOME_CUSTOMIZATION_SCHEMA_VERSION = 2 as const;

export interface HouseAccessoryState {
  readonly id: HouseAccessoryId;
  readonly equipped: boolean;
  readonly owned: boolean;
}

export interface HouseEditionState {
  readonly id: HouseThemeId;
  readonly owned: boolean;
}

export interface HomeCustomizationSnapshot {
  readonly schemaVersion: typeof HOME_CUSTOMIZATION_SCHEMA_VERSION;
  readonly familyId: FamilyId;
  readonly revision: number;
  readonly updatedAt: string;
  readonly updatedBy: FamilyMemberId;
  readonly accessories: ReadonlyArray<HouseAccessoryState>;
  readonly editions: ReadonlyArray<HouseEditionState>;
  readonly placements: ReadonlyArray<HouseLayoutPlacement>;
  readonly selectedEditionId: HouseThemeId;
}

export interface SaveHomeCustomizationCommand {
  readonly familyId: FamilyId;
  readonly expectedRevision: number;
  readonly updatedBy: FamilyMemberId;
  readonly accessories: ReadonlyArray<HouseAccessoryState>;
  readonly editions: ReadonlyArray<HouseEditionState>;
  readonly placements: ReadonlyArray<HouseLayoutPlacement>;
  readonly selectedEditionId: HouseThemeId;
}
