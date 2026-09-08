import { MINIMUM_HOUSE_ENERGY_PERCENT } from '@/domain/energy';
import type { FurnitureSetId, HouseAccessoryId, HouseThemeId, HouseZoneId } from '@/domain/house';
import { FURNITURE_SETS, HOUSE_ROOMS, HOUSE_STAGES, HOUSE_THEMES } from '@/domain/house-catalog';
import type { HouseAccessory, HouseLayoutPlacementId } from '@/domain/types';
import { createHouseLayoutPlacements } from '@/infrastructure/fixtures/family-world-fixtures';
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from '../family-world-store-context';

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
    if (!accessory?.owned) {return;}
    accessory.equipped = !accessory.equipped;
    this.persistHomeCustomization();
  },
  storeHouseAccessory(this: FamilyWorldStoreContext, id: HouseAccessoryId) {
    const accessory = this.accessories.find(item => item.id === id);
    if (!accessory?.owned || !accessory.equipped) {return;}
    accessory.equipped = false;
    this.persistHomeCustomization();
  },
  placeStoredHouseAccessory(this: FamilyWorldStoreContext, id: HouseAccessoryId, requestedZoneId: HouseZoneId) {
    const accessory = this.accessories.find(item => item.id === id);
    const placement = this.houseLayout.find(item => item.entityType === 'furniture' && item.entityId === id);
    if (!accessory?.owned || !placement) {return;}
    const targetZoneId: HouseZoneId = accessory.placement === 'outside'
      ? 'garden'
      : requestedZoneId === 'garden'
        ? accessory.roomId ?? 'living-room'
        : requestedZoneId;
    const targetRoom = HOUSE_ROOMS.find(item => item.id === targetZoneId);
    if (targetRoom && targetRoom.minimumHouseLevel > this.houseLevel) {return;}
    if (placement.zoneId !== targetZoneId) {
      placement.zoneId = targetZoneId;
      placement.x = 50;
      placement.y = accessory.visual === 'wall-art' ? 28 : targetZoneId === 'garden' ? 68 : 66;
    }
    accessory.equipped = true;
    this.persistHomeCustomization();
  },
  purchaseHouseTheme(this: FamilyWorldStoreContext, id: HouseThemeId) {
    const edition = HOUSE_THEMES.find(item => item.id === id);
    if (this.viewerRole !== 'child' || !edition || this.ownedHouseThemeIds.includes(id) || edition.price > this.availableBalance) {return;}
    this.ownedHouseThemeIds.push(id);
    this.houseThemeId = id;
    this.balances[this.activeChildId] = this.balance - edition.price;
    this.persistHomeCustomization();
    this.persistSavings();
    this.notify('notifications.home.themePurchased');
  },
  selectHouseTheme(this: FamilyWorldStoreContext, id: HouseThemeId) {
    if (!this.ownedHouseThemeIds.includes(id)) {return;}
    this.houseThemeId = id;
    this.persistHomeCustomization();
    this.notify('notifications.home.themeSelected');
  },
  moveHouseEntity(this: FamilyWorldStoreContext, placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number) {
    if (!this.canArrangeHouse) {return;}
    const room = HOUSE_ROOMS.find((item) => item.id === zoneId);
    if (room && room.minimumHouseLevel > this.houseLevel) {return;}
    const placement = this.houseLayout.find((item) => item.id === placementId);
    if (!placement) {return;}
    placement.zoneId = zoneId;
    placement.x = Math.max(4, Math.min(96, Number(x.toFixed(2))));
    placement.y = Math.max(8, Math.min(94, Number(y.toFixed(2))));
    this.persistHomeCustomization();
  },
  resetHouseEntityPosition(this: FamilyWorldStoreContext, placementId: HouseLayoutPlacementId) {
    if (!this.canArrangeHouse) {return;}
    const initialPlacement = createHouseLayoutPlacements().find((item) => item.id === placementId);
    const placement = this.houseLayout.find((item) => item.id === placementId);
    if (!initialPlacement || !placement) {return;}
    placement.zoneId = initialPlacement.zoneId;
    placement.x = initialPlacement.x;
    placement.y = initialPlacement.y;
    placement.scale = initialPlacement.scale;
    this.persistHomeCustomization();
  },
  setSimulatedEnergy(this: FamilyWorldStoreContext, value: number | null) {
    if (!import.meta.env.DEV && !this.permissions.canManageContent) {return;}
    this.simulatedEnergy = value === null ? null : Math.max(0, Math.min(100, Math.round(value)));
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
