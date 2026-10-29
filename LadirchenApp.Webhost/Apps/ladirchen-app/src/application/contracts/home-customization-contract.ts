import type {
  FurnitureSetId,
  HouseAccessoryId,
  HouseRoomCount,
  HouseStageId,
  HouseStageLevel,
  HouseThemeId,
  HouseZoneId,
  RoomDesignId,
  SelectedRoomDesignState,
} from "@/domain/house";
import type { HouseLayoutPlacement } from "@/domain/house/entities";
import type { FamilyMemberId } from "@/domain/shared/identifiers";
import type { FamilyReadModel } from "./family-read-model-contract";

export const HOME_CUSTOMIZATION_READ_MODEL_TYPE = "home-customization";

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

export interface PurchasableHouseCatalogEntryState<TId extends string> {
  readonly available: boolean;
  readonly id: TId;
  readonly owned: boolean;
  readonly price: number;
  readonly purchasableByMemberIds: ReadonlyArray<FamilyMemberId>;
}

export interface HouseCatalogState {
  readonly accessories: ReadonlyArray<
    PurchasableHouseCatalogEntryState<HouseAccessoryId> & {
      readonly catalogArea: "inside" | "outside" | "special";
      readonly minimumHouseLevel: HouseStageLevel;
    }
  >;
  readonly areas: ReadonlyArray<{
    readonly available: boolean;
    readonly id: HouseZoneId;
    readonly minimumHouseLevel: HouseStageLevel;
  }>;
  readonly furnitureSets: ReadonlyArray<
    PurchasableHouseCatalogEntryState<FurnitureSetId> & {
      readonly accessoryIds: ReadonlyArray<HouseAccessoryId>;
      readonly catalogArea: "inside" | "outside";
      readonly minimumHouseLevel: HouseStageLevel;
    }
  >;
  readonly houseStages: ReadonlyArray<{
    readonly active: boolean;
    readonly id: HouseStageId;
    readonly level: HouseStageLevel;
    readonly roomCount: HouseRoomCount;
  }>;
  readonly roomDesigns: ReadonlyArray<
    PurchasableHouseCatalogEntryState<RoomDesignId> & {
      readonly areaId: HouseZoneId;
      readonly minimumHouseLevel: HouseStageLevel;
      readonly themeId: HouseThemeId;
    }
  >;
  readonly themes: ReadonlyArray<PurchasableHouseCatalogEntryState<HouseThemeId>>;
}

export interface HomeCustomizationState {
  readonly capabilities: {
    readonly canArrangeHouse: boolean;
  };
  readonly catalog: HouseCatalogState;
  readonly accessories: ReadonlyArray<HouseAccessoryState>;
  readonly editions: ReadonlyArray<HouseEditionState>;
  readonly placements: ReadonlyArray<HouseLayoutPlacement>;
  readonly roomDesigns: ReadonlyArray<RoomDesignState>;
  readonly selectedRoomDesigns: ReadonlyArray<SelectedRoomDesignState>;
  readonly selectedEditionId: HouseThemeId;
}

export type HomeCustomizationReadModel = FamilyReadModel<
  typeof HOME_CUSTOMIZATION_READ_MODEL_TYPE,
  HomeCustomizationState
>;
