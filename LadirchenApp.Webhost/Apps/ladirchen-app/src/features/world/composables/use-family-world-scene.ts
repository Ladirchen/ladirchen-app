import { computed, onUnmounted, ref } from "vue";
import { useI18n } from "vue-i18n";

import { resolveFamilyMemberAvatarAppearance } from "@/domain/avatar";
import type { AvatarAppearance } from "@/domain/avatar";
import type { WorldEffect } from "@/domain/contributions/types";
import type { FamilyMember, FamilyPet } from "@/domain/family/types";
import { HOUSE_ENERGY_THRESHOLDS, HOUSE_STAGES, HOUSE_THEMES } from "@/domain/house";
import { PERCENTAGE_BASE } from "@/domain/shared/numbers";
import type { HouseAccessory, HouseAccessoryId, HouseLayoutPlacement, HouseRoomDefinition, HouseStageLevel, HouseThemeId, HouseZoneId, RoomDesignDefinition } from "@/domain/house";
import type { FamilyMemberId, FamilyPetId, HouseLayoutPlacementId } from "@/domain/shared/identifiers";
import { HOUSE_EXTERIOR_ASSET_URLS, houseExteriorBackgroundAssetId, houseExteriorHouseAssetId } from "@/shared/visuals/house/house-exterior-assets";

type HouseView = "front" | "inside" | "garden";

const MEMBER_LABEL_DURATION_MS = 1900;
const MINIMUM_SUN_OPACITY = 0.08;

export interface FamilyWorldSceneProps {
  energy: number;
  houseLevel: HouseStageLevel;
  houseThemeId?: HouseThemeId;
  houseLayout: HouseLayoutPlacement[];
  rooms: ReadonlyArray<HouseRoomDefinition>;
  ladiScore: number;
  canArrangeHouse: boolean;
  effects: WorldEffect[];
  accessories: HouseAccessory[];
  members: FamilyMember[];
  pets: FamilyPet[];
  roomDesigns: ReadonlyArray<RoomDesignDefinition>;
  revealVersion: number;
  viewerMemberId: FamilyMemberId;
}

export interface FamilyWorldSceneEmit {
  (event: "move-entity", placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number): void;
  (event: "place-furniture", accessoryId: HouseAccessoryId, zoneId: HouseZoneId): void;
  (event: "reset-entity", placementId: HouseLayoutPlacementId): void;
  (event: "store-furniture", accessoryId: HouseAccessoryId): void;
}

export const useFamilyWorldScene = (props: FamilyWorldSceneProps, emit: FamilyWorldSceneEmit) => {
  const { t } = useI18n();
  const view = ref<HouseView>("front");
  const selectedRoomView = ref<HouseZoneId | "all">("living-room");
  const storageOpen = ref(false);
  const draggingEntityType = ref<HouseLayoutPlacement["entityType"] | null>(null);
  const speakingMemberId = ref<FamilyMemberId | null>(null);
  const activePetId = ref<FamilyPetId | null>(null);
  let memberNameTimer: number | undefined;
  let petNameTimer: number | undefined;
  const viewLabel = computed(() => t(`world.scene.views.${view.value}`));
  const weather = computed(() => {
    if (props.energy >= HOUSE_ENERGY_THRESHOLDS.bright) {return "sunny";}
    if (props.energy >= HOUSE_ENERGY_THRESHOLDS.cloudy) {return "cloudy";}
    return "rainy";
  });
  const weatherLabel = computed(() => t(`world.scene.weather.${weather.value}`));
  const houseEnergyClass = computed(() => {
    if (props.energy < HOUSE_ENERGY_THRESHOLDS.critical) {return "energy-critical";}
    if (props.energy < HOUSE_ENERGY_THRESHOLDS.low) {return "energy-low";}
    if (props.energy < HOUSE_ENERGY_THRESHOLDS.bright) {return "energy-tired";}
    return "energy-bright";
  });
  const sunOpacity = computed(() => Math.max(MINIMUM_SUN_OPACITY, props.energy / PERCENTAGE_BASE));
  const familyChildren = computed(() => props.members.filter(member => member.role === "child"));
  const familyGuardians = computed(() => props.members.filter(member => member.role === "guardian"));
  const familyLineup = computed(() => {
    const guardianSplit = Math.ceil(familyGuardians.value.length / 2);
    const lineup = [...familyGuardians.value.slice(0, guardianSplit), ...familyChildren.value, ...familyGuardians.value.slice(guardianSplit)];
    const activeMember = lineup.find(member => member.id === props.viewerMemberId);
    if (!activeMember) {return lineup;}
    const otherMembers = lineup.filter(member => member.id !== props.viewerMemberId);
    const centerIndex = Math.ceil(otherMembers.length / 2);
    return [...otherMembers.slice(0, centerIndex), activeMember, ...otherMembers.slice(centerIndex)];
  });
  const activeFamilyMember = computed(() => familyLineup.value.find(member => member.id === props.viewerMemberId));
  const worldMemberName = (member: FamilyMember) => {
    const name = member.nickname?.trim() || member.name;
    return member.id === props.viewerMemberId ? t("world.scene.memberYou", { name }) : name;
  };
  const backgroundFamilyMembers = computed(() => familyLineup.value.filter(member => member.id !== props.viewerMemberId));
  const backgroundMemberGroups = computed(() => {
    const split = Math.ceil(backgroundFamilyMembers.value.length / 2);
    return [
      { side: "left", members: backgroundFamilyMembers.value.slice(0, split) },
      { side: "right", members: backgroundFamilyMembers.value.slice(split) },
    ];
  });
  const activeTheme = computed(() => HOUSE_THEMES.find(theme => theme.id === props.houseThemeId) ?? HOUSE_THEMES[0]);
  const activeThemeId = computed(() => activeTheme.value?.id ?? "sunny-dollhouse");
  const activeThemeLabel = computed(() => t("world.scene.editionAria", { name: activeTheme.value ? t(activeTheme.value.nameKey) : "" }));
  const activeHouseStage = computed(() => HOUSE_STAGES.find(stage => stage.level === props.houseLevel));
  const frontExterior = computed(() => {
    const stage = activeHouseStage.value;
    if (!stage) {return undefined;}
    return {
      backgroundUrl: HOUSE_EXTERIOR_ASSET_URLS[houseExteriorBackgroundAssetId(activeThemeId.value, props.energy)],
      houseUrl: HOUSE_EXTERIOR_ASSET_URLS[houseExteriorHouseAssetId(stage.id, activeThemeId.value)],
    };
  });
  const sceneStyle = computed(() => ({
    "--house-wall": activeTheme.value?.wall ?? "var(--lad-palette-amber-100)",
    "--house-wall-upper": activeTheme.value?.wallUpper ?? "var(--lad-palette-background)",
    "--house-floor": activeTheme.value?.floor ?? "var(--lad-palette-amber-200)",
    "--house-roof": activeTheme.value?.roof ?? "var(--lad-palette-red-400)",
    "--house-roof-shade": activeTheme.value?.roofShade ?? "var(--lad-palette-red-500)",
    "--house-trim": activeTheme.value?.trim ?? "var(--lad-palette-orange-500)",
    "--house-door": activeTheme.value?.door ?? "var(--lad-palette-mint-450)",
    "--house-window": activeTheme.value?.window ?? "var(--lad-palette-blue-250)",
    "--landscape-accent": activeTheme.value?.landscapeAccent ?? "var(--lad-palette-mint-450)",
    "--world-saturation": `${.55 + props.energy / 200}`,
    "--world-brightness": `${.82 + props.energy / 550}`,
  }));
  const ariaLabel = computed(() => t("world.scene.aria", { level: props.houseLevel + 1, view: viewLabel.value, energy: props.energy, weather: weatherLabel.value }));
  const hasEffect = (effect: WorldEffect) => props.effects.includes(effect);
  const storageAccessories = computed(() => props.accessories.filter(accessory => accessory.owned && !accessory.equipped));
  const storageTargetZone = computed<HouseZoneId>(() => {
    if (view.value === "garden") {return "garden";}
    if (selectedRoomView.value === "all" || selectedRoomView.value === "garden") {return "living-room";}
    return selectedRoomView.value;
  });
  const storeAccessory = (accessoryId: HouseAccessoryId) => emit("store-furniture", accessoryId);
  const placeStoredAccessory = (accessory: HouseAccessory) => emit("place-furniture", accessory.id, storageTargetZone.value);
  const appearanceFor = (member: FamilyMember, _index: number): AvatarAppearance => {
    const members = props.members.length > 0 ? props.members : [member];
    return resolveFamilyMemberAvatarAppearance(member, members);
  };
  const showMemberName = (memberId: FamilyMemberId) => {
    speakingMemberId.value = memberId;
    if (memberNameTimer !== undefined) {window.clearTimeout(memberNameTimer);}
    memberNameTimer = window.setTimeout(() => { speakingMemberId.value = null; }, MEMBER_LABEL_DURATION_MS);
  };
  const showPetName = (petId: FamilyPetId) => {
    activePetId.value = petId;
    if (petNameTimer !== undefined) {window.clearTimeout(petNameTimer);}
    petNameTimer = window.setTimeout(() => { activePetId.value = null; }, MEMBER_LABEL_DURATION_MS);
  };
  const forwardEntityMove = (placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number) => emit("move-entity", placementId, zoneId, x, y);
  const forwardEntityReset = (placementId: HouseLayoutPlacementId) => emit("reset-entity", placementId);
  const enterHouse = () => { selectedRoomView.value = "living-room"; view.value = "inside"; };
  const selectZone = (zoneId: HouseZoneId | "all") => { selectedRoomView.value = zoneId; view.value = zoneId === "garden" ? "garden" : "inside"; };
  const leaveScene = () => { storageOpen.value = false; view.value = "front"; };
  onUnmounted(() => {
    if (memberNameTimer !== undefined) {window.clearTimeout(memberNameTimer);}
    if (petNameTimer !== undefined) {window.clearTimeout(petNameTimer);}
  });

  return {
    activeFamilyMember, activePetId, activeThemeId, activeThemeLabel, appearanceFor,
    ariaLabel, backgroundMemberGroups, draggingEntityType, enterHouse, forwardEntityMove,
    forwardEntityReset, frontExterior, hasEffect, houseEnergyClass, leaveScene, placeStoredAccessory,
    sceneStyle, selectedRoomView, selectZone, showMemberName, showPetName, speakingMemberId, storageAccessories,
    storageOpen, storeAccessory, sunOpacity, view, worldMemberName,
  };
};
