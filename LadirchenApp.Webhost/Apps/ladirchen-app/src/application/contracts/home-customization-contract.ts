import type { HouseAccessoryId, HouseThemeId } from '@/domain/house';
import type { HouseLayoutPlacement } from '@/domain/types';
import type { VersionedAggregateSnapshot } from './versioned-aggregate-contract';

export const HOME_CUSTOMIZATION_SCHEMA_VERSION = 2 as const;
export const HOME_CUSTOMIZATION_AGGREGATE_TYPE = 'home-customization' as const;

export interface HouseAccessoryState {
  readonly id: HouseAccessoryId;
  readonly equipped: boolean;
  readonly owned: boolean;
}

export interface HouseEditionState {
  readonly id: HouseThemeId;
  readonly owned: boolean;
}

export interface HomeCustomizationState {
  readonly accessories: ReadonlyArray<HouseAccessoryState>;
  readonly editions: ReadonlyArray<HouseEditionState>;
  readonly placements: ReadonlyArray<HouseLayoutPlacement>;
  readonly selectedEditionId: HouseThemeId;
}

export type HomeCustomizationSnapshot = VersionedAggregateSnapshot<
  typeof HOME_CUSTOMIZATION_AGGREGATE_TYPE,
  typeof HOME_CUSTOMIZATION_SCHEMA_VERSION,
  HomeCustomizationState
>;
