import type {
  FurnitureSetId,
  HouseAccessoryId,
  HouseStageId,
  HouseThemeId,
  HouseZoneId,
  RoomDesignId,
  SelectedRoomDesignState,
} from "@/domain/house";
import type { HouseLayoutPlacement } from "@/domain/house/entities";
import type { VersionedAggregateSnapshot } from "./versioned-aggregate-contract";

export const HOME_CUSTOMIZATION_SCHEMA_VERSION = 3;
export const HOME_CUSTOMIZATION_AGGREGATE_TYPE = "home-customization";

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

export interface HouseCatalogEntryState {
  readonly available: boolean;
  readonly id: string;
  readonly minimumHouseLevel?: number;
  readonly owned?: boolean;
  readonly price?: number;
}

export interface HouseCatalogState {
  readonly accessories: ReadonlyArray<
    HouseCatalogEntryState & { readonly catalogArea: "inside" | "outside" | "special" }
  >;
  readonly areas: ReadonlyArray<HouseCatalogEntryState & { readonly id: HouseZoneId }>;
  readonly furnitureSets: ReadonlyArray<
    HouseCatalogEntryState & {
      readonly accessoryIds: ReadonlyArray<HouseAccessoryId>;
      readonly catalogArea: "inside" | "outside";
      readonly id: FurnitureSetId;
    }
  >;
  readonly houseStages: ReadonlyArray<{
    readonly active: boolean;
    readonly id: HouseStageId;
    readonly level: number;
    readonly roomCount: number;
  }>;
  readonly roomDesigns: ReadonlyArray<
    HouseCatalogEntryState & { readonly areaId: HouseZoneId; readonly id: RoomDesignId; readonly themeId: HouseThemeId }
  >;
  readonly themes: ReadonlyArray<HouseCatalogEntryState & { readonly id: HouseThemeId }>;
}

export interface HomeCustomizationState {
  readonly catalog?: HouseCatalogState;
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
