import type { HouseAccessoryId, HouseThemeId, RoomDesignId, SelectedRoomDesignState } from '@/domain/house';
import type { HouseLayoutPlacement } from '@/domain/house/entities';
import type { VersionedAggregateSnapshot } from './versioned-aggregate-contract';

export const HOME_CUSTOMIZATION_SCHEMA_VERSION = 3 as const;
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

export interface RoomDesignState {
  readonly id: RoomDesignId;
  readonly owned: boolean;
}

export interface HomeCustomizationState {
  readonly accessories: ReadonlyArray<HouseAccessoryState>;
  readonly editions: ReadonlyArray<HouseEditionState>;
  readonly placements: ReadonlyArray<HouseLayoutPlacement>;
  readonly roomDesigns: ReadonlyArray<RoomDesignState>;
  readonly selectedRoomDesigns: ReadonlyArray<SelectedRoomDesignState>;
  readonly selectedEditionId: HouseThemeId;
}

export type HomeCustomizationSnapshot = VersionedAggregateSnapshot<
  typeof HOME_CUSTOMIZATION_AGGREGATE_TYPE,
  typeof HOME_CUSTOMIZATION_SCHEMA_VERSION,
  HomeCustomizationState
>;
