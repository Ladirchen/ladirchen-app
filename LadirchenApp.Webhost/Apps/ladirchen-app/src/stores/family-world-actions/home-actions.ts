import { MINIMUM_HOUSE_ENERGY_PERCENT } from '@/domain/contributions/energy';
import { FURNITURE_SETS, furnitureVisualDefinitionFor, HOUSE_LAYOUT_CONSTRAINTS, HOUSE_ROOMS, HOUSE_STAGES, HOUSE_THEMES, ROOM_DESIGNS, roomDesignsForTheme } from '@/domain/house';
import type { FurnitureSetId, HouseAccessoryId, HouseThemeId, HouseZoneId, RoomDesignId } from '@/domain/house';
import type { HouseAccessory } from '@/domain/house/entities';
import type { HouseLayoutPlacementId } from '@/domain/shared/identifiers';
import { clamp, PERCENTAGE_BASE } from '@/domain/shared/numbers';
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from '../family-world-store-context';

const applyHouseThemeRoomDesigns = (store: FamilyWorldStoreContext, themeId: HouseThemeId) => {
  for (const design of roomDesignsForTheme(themeId)) {
    if (!store.ownedRoomDesignIds.includes(design.id)) { store.ownedRoomDesignIds.push(design.id); }
    store.selectedRoomDesignIds[design.zoneId] = design.id;
  }
};

export const homeActions = {
  purchaseAccessory(this: FamilyWorldStoreContext, id: HouseAccessory['id']) {
    const accessory = this.accessories.find((item) => item.id === id);
    if (this.viewerRole !== 'child' || !accessory || accessory.owned || accessory.price > this.availableBalance) {return;}
    accessory.owned = true;
    accessory.equipped = false;
    this.balances[this.activeChildId] = this.balance - accessory.price;
    this.persistHomeCustomization();
    this.persistSavings();
    this.notify('notifications.home.accessoryPurchased', { title: accessory.title });
  },
  purchaseFurnitureSet(this: FamilyWorldStoreContext, id: FurnitureSetId) {
    const set = FURNITURE_SETS.find((item) => item.id === id);
    if (this.viewerRole !== 'child' || !set || set.minimumHouseLevel > this.houseLevel || this.ownedFurnitureSetIds.includes(id) || set.price > this.availableBalance) {return;}
    for (const accessory of this.accessories) {
      if (!set.accessoryIds.includes(accessory.id)) {continue;}
      accessory.owned = true;
      accessory.equipped = false;
    }
    this.balances[this.activeChildId] = this.balance - set.price;
    this.persistHomeCustomization();
    this.persistSavings();
    this.notify('notifications.home.setPurchased');
  },
  toggleAccessory(this: FamilyWorldStoreContext, id: HouseAccessory['id']) {
    const accessory = this.accessories.find((item) => item.id === id);
    if (!accessory?.owned || accessory.mobility === 'fixed') {return;}
    accessory.equipped = !accessory.equipped;
    this.persistHomeCustomization();
  },
  storeHouseAccessory(this: FamilyWorldStoreContext, id: HouseAccessoryId) {
    const accessory = this.accessories.find(item => item.id === id);
    if (!accessory?.owned || !accessory.equipped || accessory.mobility === 'fixed') {return;}
    accessory.equipped = false;
    this.persistHomeCustomization();
  },
  placeStoredHouseAccessory(this: FamilyWorldStoreContext, id: HouseAccessoryId, requestedZoneId: HouseZoneId) {
    const accessory = this.accessories.find(item => item.id === id);
    const placement = this.houseLayout.find(item => item.entityType === 'furniture' && item.entityId === id);
    if (!accessory?.owned || !placement || accessory.mobility === 'fixed') {return;}
    const targetZoneId: HouseZoneId = accessory.placement === 'outside'
      ? 'garden'
      : requestedZoneId === 'garden'
        ? accessory.roomId ?? 'living-room'
        : requestedZoneId;
    const targetRoom = HOUSE_ROOMS.find(item => item.id === targetZoneId);
    if (targetRoom && targetRoom.minimumHouseLevel > this.houseLevel) {return;}
    if (placement.zoneId !== targetZoneId) {
      placement.zoneId = targetZoneId;
      placement.x = HOUSE_LAYOUT_CONSTRAINTS.defaultX;
      const visualDefinition = accessory.visual ? furnitureVisualDefinitionFor(accessory.visual) : undefined;
      placement.y = visualDefinition?.placementY ?? visualDefinition?.minimumY ?? (targetZoneId === 'garden'
        ? HOUSE_LAYOUT_CONSTRAINTS.defaultGardenY
        : HOUSE_LAYOUT_CONSTRAINTS.defaultIndoorY);
    }
    accessory.equipped = true;
    this.persistHomeCustomization();
  },
  purchaseHouseTheme(this: FamilyWorldStoreContext, id: HouseThemeId) {
    const edition = HOUSE_THEMES.find(item => item.id === id);
    if (this.viewerRole !== 'child' || !edition || this.ownedHouseThemeIds.includes(id) || edition.price > this.availableBalance) {return;}
    this.ownedHouseThemeIds.push(id);
    this.houseThemeId = id;
    applyHouseThemeRoomDesigns(this, id);
    this.balances[this.activeChildId] = this.balance - edition.price;
    this.persistHomeCustomization();
    this.persistSavings();
    this.notify('notifications.home.themePurchased');
  },
  selectHouseTheme(this: FamilyWorldStoreContext, id: HouseThemeId) {
    if (!this.ownedHouseThemeIds.includes(id)) {return;}
    this.houseThemeId = id;
    applyHouseThemeRoomDesigns(this, id);
    this.persistHomeCustomization();
    this.notify('notifications.home.themeSelected');
  },
  purchaseRoomDesign(this: FamilyWorldStoreContext, id: RoomDesignId) {
    const design = ROOM_DESIGNS.find(item => item.id === id);
    if (this.viewerRole !== 'child' || !design || design.minimumHouseLevel > this.houseLevel ||
      !this.ownedHouseThemeIds.includes(design.houseThemeId) || this.ownedRoomDesignIds.includes(id) ||
      design.price > this.availableBalance) { return; }
    this.ownedRoomDesignIds.push(id);
    this.selectedRoomDesignIds[design.zoneId] = id;
    this.balances[this.activeChildId] = this.balance - design.price;
    this.persistHomeCustomization();
    this.persistSavings();
    this.notify('notifications.home.roomDesignPurchased');
  },
  selectRoomDesign(this: FamilyWorldStoreContext, id: RoomDesignId) {
    const design = ROOM_DESIGNS.find(item => item.id === id);
    if (!design || design.minimumHouseLevel > this.houseLevel ||
      !this.ownedHouseThemeIds.includes(design.houseThemeId) || !this.ownedRoomDesignIds.includes(id)) { return; }
    this.houseThemeId = design.houseThemeId;
    this.selectedRoomDesignIds[design.zoneId] = id;
    this.persistHomeCustomization();
    this.notify('notifications.home.roomDesignSelected');
  },
  moveHouseEntity(this: FamilyWorldStoreContext, placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number) {
    if (!this.canArrangeHouse) {return;}
    const room = HOUSE_ROOMS.find((item) => item.id === zoneId);
    if (room && room.minimumHouseLevel > this.houseLevel) {return;}
    const placement = this.houseLayout.find((item) => item.id === placementId);
    if (!placement) {return;}
    if (placement.entityType === 'furniture' && this.accessories.find(accessory => accessory.id === placement.entityId)?.mobility === 'fixed') {return;}
    placement.zoneId = zoneId;
    placement.x = clamp(
      Number(x.toFixed(HOUSE_LAYOUT_CONSTRAINTS.coordinatePrecision)),
      HOUSE_LAYOUT_CONSTRAINTS.minimumX,
      HOUSE_LAYOUT_CONSTRAINTS.maximumX,
    );
    placement.y = clamp(
      Number(y.toFixed(HOUSE_LAYOUT_CONSTRAINTS.coordinatePrecision)),
      HOUSE_LAYOUT_CONSTRAINTS.minimumY,
      HOUSE_LAYOUT_CONSTRAINTS.maximumY,
    );
    this.persistHomeCustomization();
  },
  resetHouseEntityPosition(this: FamilyWorldStoreContext, placementId: HouseLayoutPlacementId) {
    if (!this.canArrangeHouse) {return;}
    const initialPlacement = this.$familyWorld.initialDataFactory.create().houseLayout.find((item) => item.id === placementId);
    const placement = this.houseLayout.find((item) => item.id === placementId);
    if (!initialPlacement || !placement) {return;}
    if (placement.entityType === 'furniture' && this.accessories.find(accessory => accessory.id === placement.entityId)?.mobility === 'fixed') {return;}
    placement.zoneId = initialPlacement.zoneId;
    placement.x = initialPlacement.x;
    placement.y = initialPlacement.y;
    placement.scale = initialPlacement.scale;
    this.persistHomeCustomization();
  },
  setSimulatedEnergy(this: FamilyWorldStoreContext, value: number | null) {
    if (!import.meta.env.DEV && !this.permissions.canManageContent) {return;}
    this.simulatedEnergy = value === null ? null : clamp(Math.round(value), 0, PERCENTAGE_BASE);
  },
  completeWeekDemo(this: FamilyWorldStoreContext): boolean {
    if (!import.meta.env.DEV && !this.permissions.canManageContent) {return false;}
    if (!this.houseMeetsMinimumEnergy) {
      this.notify('notifications.home.energyTooLow', { minimum: MINIMUM_HOUSE_ENERGY_PERCENT });
      return false;
    }
    this.completedWeeklyStreak += 1;
    this.houseLevel = HOUSE_STAGES[Math.min(HOUSE_STAGES.length - 1, this.houseLevel + 1)]!.level;
    this.revealVersion += 1;
    this.members
      .filter((member) => member.role === 'child' || member.participatesInWeeklyGoal)
      .forEach((member) => { member.weeklyStreak = this.completedWeeklyStreak; });
    this.persistFamilyProgression();
    this.persistFamilyProfile();
    this.notify('notifications.home.weekCompleted');
    return true;
  },
  failWeekDemo(this: FamilyWorldStoreContext) {
    if (!import.meta.env.DEV && !this.permissions.canManageContent) {return;}
    this.completedWeeklyStreak = 0;
    this.houseLevel = HOUSE_STAGES[Math.max(0, this.houseLevel - 1)]!.level;
    this.revealVersion += 1;
    this.members
      .filter((member) => member.role === 'child' || member.participatesInWeeklyGoal)
      .forEach((member) => { member.weeklyStreak = 0; });
    this.persistFamilyProgression();
    this.persistFamilyProfile();
    this.notify('notifications.home.weekFailed');
  },
} satisfies FamilyWorldActionGroup;
