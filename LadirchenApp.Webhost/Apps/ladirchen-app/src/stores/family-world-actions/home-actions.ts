import type { FurnitureSetId, HouseAccessoryId, HouseThemeId, HouseZoneId, RoomDesignId } from "@/domain/house";
import type { HouseLayoutPlacementId } from "@/domain/shared/identifiers";
import { clamp, PERCENTAGE_BASE } from "@/domain/shared/numbers";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { executeBackendFamilyCommand } from "./backend-family-command";

export const homeActions = {
  async purchaseAccessory(this: FamilyWorldStoreContext, id: HouseAccessoryId) {
    const title = this.accessories.find((item) => item.id === id)?.title ?? "";
    await executeBackendFamilyCommand(this, "home.purchase-accessory", { id }, "notifications.save.home", () =>
      this.notify("notifications.home.accessoryPurchased", { title }),
    );
  },
  async purchaseFurnitureSet(this: FamilyWorldStoreContext, id: FurnitureSetId) {
    await executeBackendFamilyCommand(this, "home.purchase-furniture-set", { id }, "notifications.save.home", () =>
      this.notify("notifications.home.setPurchased"),
    );
  },
  async toggleAccessory(this: FamilyWorldStoreContext, id: HouseAccessoryId) {
    await executeBackendFamilyCommand(this, "home.toggle-accessory", { id }, "notifications.save.home");
  },
  async storeHouseAccessory(this: FamilyWorldStoreContext, id: HouseAccessoryId) {
    await executeBackendFamilyCommand(this, "home.store-accessory", { id }, "notifications.save.home");
  },
  async placeStoredHouseAccessory(this: FamilyWorldStoreContext, id: HouseAccessoryId, zoneId: HouseZoneId) {
    await executeBackendFamilyCommand(this, "home.place-accessory", { id, zoneId }, "notifications.save.home");
  },
  async purchaseHouseTheme(this: FamilyWorldStoreContext, id: HouseThemeId) {
    await executeBackendFamilyCommand(this, "home.purchase-theme", { id }, "notifications.save.home", () =>
      this.notify("notifications.home.themePurchased"),
    );
  },
  async selectHouseTheme(this: FamilyWorldStoreContext, id: HouseThemeId) {
    await executeBackendFamilyCommand(this, "home.select-theme", { id }, "notifications.save.home", () =>
      this.notify("notifications.home.themeSelected"),
    );
  },
  async purchaseRoomDesign(this: FamilyWorldStoreContext, id: RoomDesignId) {
    await executeBackendFamilyCommand(this, "home.purchase-room-design", { id }, "notifications.save.home", () =>
      this.notify("notifications.home.roomDesignPurchased"),
    );
  },
  async selectRoomDesign(this: FamilyWorldStoreContext, id: RoomDesignId) {
    await executeBackendFamilyCommand(this, "home.select-room-design", { id }, "notifications.save.home", () =>
      this.notify("notifications.home.roomDesignSelected"),
    );
  },
  async moveHouseEntity(
    this: FamilyWorldStoreContext,
    placementId: HouseLayoutPlacementId,
    zoneId: HouseZoneId,
    x: number,
    y: number,
  ) {
    await executeBackendFamilyCommand(
      this,
      "home.move-entity",
      { placementId, x, y, zoneId },
      "notifications.save.home",
    );
  },
  async resetHouseEntityPosition(this: FamilyWorldStoreContext, placementId: HouseLayoutPlacementId) {
    await executeBackendFamilyCommand(this, "home.reset-entity", { placementId }, "notifications.save.home");
  },
  setSimulatedEnergy(this: FamilyWorldStoreContext, value: number | null) {
    if (!import.meta.env.DEV && !this.permissions.canManageContent) {
      return;
    }

    this.simulatedEnergy = value === null ? null : clamp(Math.round(value), 0, PERCENTAGE_BASE);
  },
  async completeWeekDemo(this: FamilyWorldStoreContext): Promise<boolean> {
    let completed = false;
    await executeBackendFamilyCommand(this, "progression.complete-week", {}, "notifications.save.profile", () => {
      completed = true;
      this.revealVersion += 1;
      this.notify("notifications.home.weekCompleted");
    });

    return completed;
  },
  async failWeekDemo(this: FamilyWorldStoreContext) {
    await executeBackendFamilyCommand(this, "progression.fail-week", {}, "notifications.save.profile", () => {
      this.revealVersion += 1;
      this.notify("notifications.home.weekFailed");
    });
  },
} satisfies FamilyWorldActionGroup;
