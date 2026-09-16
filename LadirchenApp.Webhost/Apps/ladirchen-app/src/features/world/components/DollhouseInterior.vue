<template>
  <div
    class="dollhouse-layout"
    :class="[{ compact, editable, 'single-zone': isSingleZone, 'contextual-zone': isContextualZone, 'has-pixi-background': activeSceneDesign, 'is-dragging-furniture': draggingFurniture }, `room-count-${visibleZoneIds.length}`, energyClass]"
    :style="contextGridStyle"
  >
    <PixiRoomScene v-if="activeSceneDesign" :key="`${activeSceneDesign.id}-${resolveHouseEnergyVisualLevel(energy)}`" class="pixi-scene-background" :design="activeSceneDesign" :energy="energy" />
    <section
      v-for="room in displayedRooms"
      :key="room.id"
      class="dollhouse-room"
      :class="[`room-${room.id}`, { 'is-focused': room.id === selectedZoneId, 'is-peek': isContextualZone && room.id !== selectedZoneId }]"
      :data-zone-id="room.id"
      :style="{ '--room-wall': room.wall, '--room-floor': room.floor }"
    >
      <button v-if="isPreviewZone(room.id)" class="room-navigation-hitbox" type="button" :aria-label="t('catalog.rooms.open', { room: t(room.nameKey) })" @click="selectPreviewZone(room.id)" />
      <header v-if="showRoomLabels"><span>{{ room.icon }}</span><strong>{{ t(room.nameKey) }}</strong></header>
      <div class="room-window" aria-hidden="true"><i /><i /><i /><i /></div>
      <div class="room-baseboard" aria-hidden="true" />
      <div class="room-floor" aria-hidden="true" />
      <div class="room-partition" aria-hidden="true" />
      <div v-if="energy < 70" class="room-wear" aria-hidden="true"><i v-for="mark in 6" :key="mark" /></div>
      <div v-if="room.id === 'kitchen'" class="room-detail kitchen-tiles" aria-hidden="true" />
      <div v-if="room.id === 'children-room'" class="room-detail bunting" aria-hidden="true"><i v-for="index in 5" :key="index" /></div>
      <div v-if="room.id === 'creative-room'" class="room-detail paint-dots" aria-hidden="true"><i v-for="index in 6" :key="index" /></div>
      <div v-if="room.id === 'living-room'" class="ladi-perch" :class="{ occupied: isLadiOnPerch }" aria-hidden="true">
        <img :src="WORLD_DECORATION_SPRITE_URLS['ladi-perch']" alt="">
      </div>
      <HouseLayoutEntity
        v-for="placement in visiblePlacements(room.id)"
        :key="placement.id"
        :accessory="accessoryFor(placement)"
        :drag-offset="dragOffset(placement.id)"
        :editable="accessoryFor(placement)?.mobility !== 'fixed' && (editable || (storageOpen && placement.entityType === 'furniture'))"
        :member="memberFor(placement)"
        :pet="petFor(placement)"
        :perched="ladiIsPerched(placement)"
        :placement="placement"
        :score="score"
        :speech="placement.entityType === 'ladi' ? ladiSpeech : ''"
        :viewer-member-id="viewerMemberId"
        @ladi-interact="motivateLadi"
        @lostpointercapture="finishDragAtLastPosition"
        @pointercancel="cancelDrag"
        @pointerdown="startDrag"
        @pointermove="trackDrag"
        @pointerup="finishDrag"
      />
    </section>

    <section
      v-if="gardenIsVisible"
      class="dollhouse-room garden-zone"
      :class="{ 'is-focused': selectedZoneId === 'garden', 'is-peek': isContextualZone && selectedZoneId !== 'garden' }"
      data-zone-id="garden"
    >
      <button v-if="isPreviewZone('garden')" class="room-navigation-hitbox" type="button" :aria-label="t('catalog.rooms.open', { room: t('catalog.rooms.garden') })" @click="selectPreviewZone('garden')" />
      <header v-if="showRoomLabels"><span>🌿</span><strong>{{ t('catalog.rooms.garden') }}</strong></header>
      <div class="garden-sky" aria-hidden="true"><i /></div>
      <div class="garden-mountains" aria-hidden="true"><i /><i /><i /></div>
      <div class="garden-trellis" aria-hidden="true"><i v-for="index in 6" :key="index" /></div>
      <div class="garden-plant-shelf" aria-hidden="true"><span v-for="index in 3" :key="index"><i /></span></div>
      <div class="garden-patio" aria-hidden="true" />
      <div v-if="showsTerraceTransition" class="terrace-transition" :class="{ 'opens-to-garden': selectedZoneId === 'garden' }" aria-hidden="true">
        <span class="terrace-frame"><i class="terrace-glass terrace-glass--fixed" /><i class="terrace-glass terrace-glass--door" /><b class="terrace-handle" /></span>
        <span class="terrace-threshold" />
      </div>
      <div class="garden-path" aria-hidden="true"><i /><i /><i /><i /></div>
      <div class="garden-flower-bed" aria-hidden="true"><i /><i /><i /><i /><i /></div>
      <HouseLayoutEntity
        v-for="placement in visiblePlacements('garden')"
        :key="placement.id"
        :accessory="accessoryFor(placement)"
        :drag-offset="dragOffset(placement.id)"
        :editable="accessoryFor(placement)?.mobility !== 'fixed' && (editable || (storageOpen && placement.entityType === 'furniture'))"
        :member="memberFor(placement)"
        :pet="petFor(placement)"
        :perched="false"
        :placement="placement"
        :score="score"
        :speech="placement.entityType === 'ladi' ? ladiSpeech : ''"
        :viewer-member-id="viewerMemberId"
        @ladi-interact="motivateLadi"
        @lostpointercapture="finishDragAtLastPosition"
        @pointercancel="cancelDrag"
        @pointerdown="startDrag"
        @pointermove="trackDrag"
        @pointerup="finishDrag"
      />
    </section>

  </div>
</template>

<script lang="ts" setup>
import { computed, defineAsyncComponent, onMounted, onUnmounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import { resolveFamilyMemberAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import { furnitureVisualDefinitionFor, resolveHouseEnergyVisualLevel } from '@/domain/house';
import type { HouseAccessory, HouseLayoutPlacement, HouseRoomDefinition, HouseZoneId, RoomDesignDefinition } from '@/domain/house';
import type { FamilyMember, FamilyPet } from '@/domain/family/types';
import type { FamilyMemberId, HouseLayoutPlacementId } from '@/domain/shared/identifiers';
import { WORLD_DECORATION_SPRITE_URLS } from '@/shared/assets/world-sprite-assets';

import HouseLayoutEntity from './HouseLayoutEntity.vue';
import { useDollhouseDrag } from '../composables/use-dollhouse-drag';

const PixiRoomScene = defineAsyncComponent(() => import('./PixiRoomScene.vue'));

const { t } = useI18n();

const props = withDefaults(defineProps<{
  accessories: HouseAccessory[];
  compact?: boolean;
  editable?: boolean;
  energy?: number;
  includeGarden?: boolean;
  contextualNeighbors?: boolean;
  members: FamilyMember[];
  pets: FamilyPet[];
  placements: HouseLayoutPlacement[];
  roomDesigns: ReadonlyArray<RoomDesignDefinition>;
  rooms: ReadonlyArray<HouseRoomDefinition>;
  score: number;
  selectedZoneId?: HouseZoneId | 'all';
  showRoomLabels?: boolean;
  storageOpen?: boolean;
  viewerMemberId?: FamilyMemberId;
}>(), { compact: false, contextualNeighbors: false, editable: false, energy: 100, includeGarden: false, selectedZoneId: 'all', showRoomLabels: true, storageOpen: false });
const emit = defineEmits<{
  move: [placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number];
  reset: [placementId: HouseLayoutPlacementId];
  'select-zone': [zoneId: HouseZoneId];
  'drag-state': [entityType: HouseLayoutPlacement['entityType'] | null];
  store: [accessoryId: HouseAccessory['id']];
}>();

const ladiMotivation = ref('');
const perchMessage = ref('');
let motivationTimer: number | undefined;
let perchMessageTimer: number | undefined;
let perchScheduleTimer: number | undefined;
const unlockedZones = computed<HouseZoneId[]>(() => [
  ...props.rooms.map((room) => room.id),
  ...(props.includeGarden ? ['garden' as const] : []),
]);
const orderedZoneIds = computed<HouseZoneId[]>(() => {
  const roomIds = props.rooms.map((room) => room.id);
  if (!props.includeGarden) return roomIds;
  const kitchenIndex = roomIds.indexOf('kitchen');
  if (kitchenIndex < 0) return [...roomIds, 'garden'];
  return [...roomIds.slice(0, kitchenIndex + 1), 'garden', ...roomIds.slice(kitchenIndex + 1)];
});
const contextualZoneIds = computed<HouseZoneId[]>(() => {
  if (props.selectedZoneId === 'all') return props.rooms.map((room) => room.id);
  if (!props.contextualNeighbors) return [props.selectedZoneId];
  if (props.selectedZoneId === 'garden' && orderedZoneIds.value.includes('kitchen')) return ['kitchen', 'garden'];
  if (props.selectedZoneId === 'living-room' && orderedZoneIds.value.includes('kitchen')) return ['living-room', 'kitchen'];
  const selectedIndex = orderedZoneIds.value.indexOf(props.selectedZoneId);
  if (selectedIndex < 0) return [props.selectedZoneId];
  const startIndex = Math.max(0, Math.min(selectedIndex - 1, orderedZoneIds.value.length - 3));
  return orderedZoneIds.value.slice(startIndex, startIndex + 3);
});
const displayedRooms = computed(() => props.rooms.filter((room) => contextualZoneIds.value.includes(room.id)));
const gardenIsVisible = computed(() => props.includeGarden && contextualZoneIds.value.includes('garden'));
const showsTerraceTransition = computed(() => gardenIsVisible.value && displayedRooms.value.some(room => room.id === 'kitchen'));
const visibleZoneIds = computed<HouseZoneId[]>(() => orderedZoneIds.value.filter((zoneId) => contextualZoneIds.value.includes(zoneId)));
const isContextualZone = computed(() => props.contextualNeighbors && props.selectedZoneId !== 'all' && visibleZoneIds.value.length > 1);
const isSingleZone = computed(() => props.selectedZoneId !== 'all' && !isContextualZone.value);
const activeSceneDesign = computed(() => props.selectedZoneId === 'all'
  ? undefined
  : props.roomDesigns.find(design => design.zoneId === props.selectedZoneId));
const contextGridStyle = computed(() => {
  if (!isContextualZone.value) return undefined;
  const selectedZoneId = props.selectedZoneId;
  if (selectedZoneId === 'all') return undefined;
  const selectedIndex = visibleZoneIds.value.indexOf(selectedZoneId);
  const columns = visibleZoneIds.value.map((_, index) => index === selectedIndex ? 'minmax(0, 3fr)' : 'minmax(52px, 1fr)').join(' ');
  return { '--context-columns': columns };
});
const isPreviewZone = (zoneId: HouseZoneId) => isContextualZone.value && zoneId !== props.selectedZoneId;
const selectPreviewZone = (zoneId: HouseZoneId) => {
  if (isPreviewZone(zoneId)) emit('select-zone', zoneId);
};
const visiblePlacements = (zoneId: HouseZoneId) => props.placements
  .filter((placement) => placement.zoneId === zoneId)
  .filter(() => !isPreviewZone(zoneId))
  .filter((placement) => placement.entityType !== 'furniture' || Boolean(accessoryFor(placement)?.owned && accessoryFor(placement)?.equipped))
  .filter((placement) => {
    const visual = accessoryFor(placement)?.visual;
    return !visual || furnitureVisualDefinitionFor(visual).renderInLayout !== false;
  })
  .sort((left, right) => left.y - right.y);
const accessoryFor = (placement: HouseLayoutPlacement) => placement.entityType === 'furniture'
  ? props.accessories.find((accessory) => accessory.id === placement.entityId)
  : undefined;
const memberFor = (placement: HouseLayoutPlacement): (FamilyMember & { resolvedAppearance: AvatarAppearance }) | undefined => {
  if (placement.entityType !== 'member') return undefined;
  const member = props.members.find((item) => item.id === placement.entityId);
  if (!member) return undefined;
  return { ...member, resolvedAppearance: resolveFamilyMemberAvatarAppearance(member, props.members) };
};
const petFor = (placement: HouseLayoutPlacement): FamilyPet | undefined => placement.entityType === 'pet'
  ? props.pets.find((pet) => pet.id === placement.entityId)
  : undefined;
const ladiPlacement = computed(() => props.placements.find((placement) => placement.entityType === 'ladi'));
const isLadiOnPerch = computed(() => ladiPlacement.value?.zoneId === 'living-room'
  && Math.abs(ladiPlacement.value.x - 18) <= 7
  && Math.abs(ladiPlacement.value.y - 30) <= 8);
const ladiIsPerched = (placement: HouseLayoutPlacement) => placement.entityType === 'ladi' && isLadiOnPerch.value;
const ladiSpeech = computed(() => ladiMotivation.value || perchMessage.value);
const energyClass = computed(() => props.energy < 30 ? 'energy-critical' : props.energy < 55 ? 'energy-low' : props.energy < 70 ? 'energy-tired' : 'energy-bright');
const { cancelDrag, dragOffset, draggingFurniture, finishDrag, finishDragAtLastPosition, startDrag, trackDrag } =
  useDollhouseDrag(props, emit, unlockedZones, accessoryFor);
const motivateLadi = () => {
  ladiMotivation.value = props.score < 2.5
    ? t('world.interior.motivation.wakeUp')
    : props.score >= 4.8
      ? t('world.interior.motivation.superTeam')
      : props.score >= 4.3
        ? t('world.interior.motivation.coolTeam')
        : t('world.interior.motivation.default');
  if (motivationTimer !== undefined) window.clearTimeout(motivationTimer);
  motivationTimer = window.setTimeout(() => { ladiMotivation.value = ''; }, 4800);
};
const perchMessageKeys = ['together', 'smallSteps', 'whoHelps', 'believe'] as const;
const perchMessages = computed(() => perchMessageKeys.map(key => t(`world.interior.perch.${key}`)));
const schedulePerchMessage = () => {
  perchScheduleTimer = window.setTimeout(() => {
    if (isLadiOnPerch.value) {
      perchMessage.value = perchMessages.value[Math.floor(Math.random() * perchMessages.value.length)] ?? perchMessages.value[0]!;
      if (perchMessageTimer !== undefined) window.clearTimeout(perchMessageTimer);
      perchMessageTimer = window.setTimeout(() => { perchMessage.value = ''; }, 4200);
    }
    schedulePerchMessage();
  }, 6500 + Math.round(Math.random() * 4500));
};
onMounted(schedulePerchMessage);
onUnmounted(() => {
  if (motivationTimer !== undefined) window.clearTimeout(motivationTimer);
  if (perchMessageTimer !== undefined) window.clearTimeout(perchMessageTimer);
  if (perchScheduleTimer !== undefined) window.clearTimeout(perchScheduleTimer);
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.dollhouse-layout {
  @apply position-relative d-grid overflow-hidden;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 7px;
  min-height: 430px;
  padding: 8px 8px 38px;
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-muted-750-2) 18%, transparent);
  border-radius: 24px;
  background: var(--lad-palette-orange-500);
  box-shadow:
    inset 0 0 0 5px
      color-mix(in srgb, var(--lad-palette-white) 18%, transparent),
    0 8px 0 color-mix(in srgb, var(--lad-palette-orange-750) 12%, transparent);
  touch-action: none;
}
.dollhouse-layout.room-count-1:not(:has(.garden-zone)) {
  grid-template-columns: 1fr;
}
.dollhouse-layout.room-count-1:not(:has(.garden-zone)) .dollhouse-room {
  min-height: 360px;
}
.dollhouse-layout.single-zone {
  grid-template-columns: 1fr;
}
.dollhouse-layout.single-zone .dollhouse-room {
  grid-column: auto;
  min-height: 390px;
  background: linear-gradient(
    180deg,
    color-mix(in srgb, var(--room-wall) 88%, var(--lad-palette-white)) 0 48%,
    var(--room-floor) 48% 100%
  );
}
.dollhouse-layout.single-zone .dollhouse-room::after {
  content: "";
  @apply position-absolute;
  right: 0;
  bottom: 0;
  left: 0;
  height: 52%;
  z-index: 0;
  transform-origin: bottom;
  background:
    repeating-linear-gradient(
      100deg,
      transparent 0 32px,
      color-mix(in srgb, var(--lad-palette-orange-750) 10%, transparent) 33px
        35px
    ),
    linear-gradient(
      var(--room-floor),
      color-mix(in srgb, var(--room-floor) 68%, var(--lad-palette-orange-600))
    );
  clip-path: polygon(7% 0, 93% 0, 100% 100%, 0 100%);
  pointer-events: none;
}
.dollhouse-layout.single-zone .room-floor {
  height: 52%;
  z-index: 1;
  opacity: 0.32;
  transform-origin: bottom;
  transform: perspective(260px) rotateX(9deg);
}
.dollhouse-layout.single-zone .room-baseboard {
  bottom: 51%;
}
.dollhouse-layout.contextual-zone {
  grid-template-columns: var(--context-columns);
  gap: 0;
  @apply pa-0;
  background: transparent;
}
.pixi-scene-background {
  @apply position-absolute;
  inset: 0;
  z-index: 0;
}
.dollhouse-layout.has-pixi-background {
  background: var(--lad-palette-background);
}
.dollhouse-layout.has-pixi-background .dollhouse-room {
  z-index: 1;
  background: transparent;
}
.dollhouse-layout.has-pixi-background .room-window,
.dollhouse-layout.has-pixi-background .room-baseboard,
.dollhouse-layout.has-pixi-background .room-floor,
.dollhouse-layout.has-pixi-background .room-partition,
.dollhouse-layout.has-pixi-background .room-wear,
.dollhouse-layout.has-pixi-background .room-detail,
.dollhouse-layout.has-pixi-background .garden-sky,
.dollhouse-layout.has-pixi-background .garden-mountains,
.dollhouse-layout.has-pixi-background .garden-trellis,
.dollhouse-layout.has-pixi-background .garden-plant-shelf,
.dollhouse-layout.has-pixi-background .garden-patio,
.dollhouse-layout.has-pixi-background .terrace-transition,
.dollhouse-layout.has-pixi-background .garden-path,
.dollhouse-layout.has-pixi-background .garden-flower-bed {
  @apply d-none;
}
.dollhouse-layout.is-dragging-furniture,
.dollhouse-layout.is-dragging-furniture .dollhouse-room {
  @apply overflow-visible;
}
.dollhouse-layout.contextual-zone .dollhouse-room {
  grid-column: auto;
  min-height: 390px;
  border: 0;
  border-radius: 0;
  background: linear-gradient(
    180deg,
    color-mix(in srgb, var(--room-wall) 88%, var(--lad-palette-white)) 0 45%,
    var(--room-floor) 45% 100%
  );
  box-shadow: none;
  transition:
    filter 0.24s ease,
    opacity 0.24s ease;
}
.dollhouse-layout.contextual-zone .garden-zone {
  grid-column: auto;
}
.dollhouse-layout.contextual-zone .dollhouse-room + .dollhouse-room {
  border-left-width: 5px;
}
.room-partition {
  @apply d-none;
}
.dollhouse-layout.contextual-zone:not(.has-pixi-background) .room-partition {
  width: 0.5rem;
  height: 54%;
  @apply d-block;
  @apply position-absolute;
  top: 0;
  right: -0.25rem;
  z-index: 30;
  border-radius: 0 0 0.3rem 0.3rem;
  background: linear-gradient(
    90deg,
    var(--lad-palette-orange-650),
    var(--lad-palette-amber-150) 35% 68%,
    var(--lad-palette-orange-650)
  );
  box-shadow:
    -0.2rem 0 0
      color-mix(in srgb, var(--lad-palette-orange-750) 10%, transparent),
    0.2rem 0 0 color-mix(in srgb, var(--lad-palette-white) 35%, transparent);
  pointer-events: none;
}
.dollhouse-layout.contextual-zone:not(.has-pixi-background)
  .room-partition::after {
  width: 1rem;
  height: 0.45rem;
  content: "";
  @apply position-absolute;
  right: -0.25rem;
  bottom: -0.2rem;
  border-radius: var(--lad-radius-pill);
  background: var(--lad-palette-orange-650);
  box-shadow: 0 0.18rem 0
    color-mix(in srgb, var(--lad-palette-orange-750) 14%, transparent);
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-focused {
  z-index: 2;
}
.dollhouse-layout.contextual-zone .room-floor {
  height: 56%;
  z-index: 1;
  opacity: 0.74;
  transform-origin: bottom;
  transform: perspective(260px) rotateX(8deg);
  clip-path: polygon(5% 0, 95% 0, 100% 100%, 0 100%);
}
.dollhouse-layout.contextual-zone .room-baseboard {
  bottom: 55%;
  z-index: 2;
  box-shadow: 0 3px 4px
    color-mix(in srgb, var(--lad-palette-orange-750) 15%, transparent);
}
.dollhouse-layout.contextual-zone .room-window {
  top: 24px;
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-focused .room-window {
  width: 94px;
  height: 66px;
  top: 20px;
  border-width: 7px;
  border-radius: 18px 18px 10px 10px;
  background:
    radial-gradient(
      circle at 74% 24%,
      var(--lad-palette-amber-250) 0 12%,
      transparent 13%
    ),
    linear-gradient(
      155deg,
      transparent 0 62%,
      var(--lad-palette-teal-400) 63% 100%
    ),
    linear-gradient(var(--lad-palette-blue-250), var(--lad-palette-background));
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-palette-orange-650) 15%, transparent),
    0 9px 16px color-mix(in srgb, var(--lad-palette-teal-600) 12%, transparent);
}
.dollhouse-layout.contextual-zone
  .dollhouse-room.is-focused
  .room-window::after {
  content: "";
  @apply position-absolute;
  right: -13px;
  bottom: -11px;
  left: -13px;
  height: 10px;
  border-radius: 5px;
  background: var(--lad-palette-amber-100);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-orange-650) 15%, transparent);
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-peek {
  opacity: 0.9;
  filter: saturate(0.86);
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-peek::before {
  content: "";
  @apply position-absolute;
  inset: 0;
  z-index: 35;
  background: linear-gradient(
    90deg,
    color-mix(in srgb, var(--lad-palette-white) 5%, transparent),
    color-mix(in srgb, var(--lad-palette-white) 18%, transparent)
  );
  pointer-events: none;
  transition: background 0.18s ease;
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-peek:hover::before,
.dollhouse-layout.contextual-zone
  .dollhouse-room.is-peek:focus-visible::before {
  background: color-mix(in srgb, var(--lad-palette-white) 5%, transparent);
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-peek:focus-visible {
  outline: 4px solid
    color-mix(in srgb, var(--lad-palette-mint) 60%, transparent);
  outline-offset: -4px;
}
.dollhouse-layout.contextual-zone
  .dollhouse-room.is-peek
  :deep(.layout-entity) {
  pointer-events: none;
}
.room-navigation-hitbox {
  @apply position-absolute cursor-pointer;
  inset: 0;
  z-index: 110;
  border: 0;
  background: transparent;
}
.room-navigation-hitbox:focus-visible {
  outline: 4px solid
    color-mix(in srgb, var(--lad-palette-mint) 60%, transparent);
  outline-offset: -4px;
}
.dollhouse-layout.room-count-3 .dollhouse-room:first-child,
.dollhouse-layout.room-count-5 .dollhouse-room:first-child {
  grid-column: span 2;
}
.dollhouse-layout.contextual-zone.room-count-3 .dollhouse-room:first-child {
  grid-column: auto;
}
.dollhouse-room {
  --room-wall: var(--lad-palette-amber-100);
  --room-floor: var(--lad-palette-orange-350);
  min-width: 0;
  min-height: 190px;
  @apply position-relative overflow-hidden;
  border: 3px solid
    color-mix(in srgb, var(--lad-palette-orange-750) 60%, transparent);
  border-radius: 17px 17px 10px 10px;
  background: linear-gradient(
    180deg,
    color-mix(in srgb, var(--room-wall) 88%, var(--lad-palette-white)) 0 64%,
    var(--room-floor) 64% 100%
  );
  box-shadow: inset 0 2px 0
    color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
}
.dollhouse-room > header {
  @apply position-absolute d-flex align-center;
  top: 7px;
  left: 8px;
  z-index: 40;
  gap: 4px;
  padding: 3px 7px;
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-muted-750) 10%, transparent);
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  color: var(--lad-palette-muted-750-2);
  font-size: 0.5rem;
  backdrop-filter: blur(5px);
}
.dollhouse-room > header strong {
  font-weight: var(--lad-font-weight-heavy);
}
.room-window {
  width: 46px;
  height: 39px;
  @apply position-absolute d-grid;
  top: 39px;
  left: 50%;
  grid-template-columns: 1fr 1fr;
  z-index: 0;
  transform: translateX(-50%);
  border: 5px solid var(--lad-palette-surface);
  border-radius: 9px;
  background: var(--lad-palette-blue-250);
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-palette-muted-700) 12%, transparent);
}
.room-window i {
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
}
.room-baseboard {
  height: 6px;
  @apply position-absolute;
  left: 0;
  right: 0;
  bottom: 34%;
  z-index: 1;
  background: color-mix(
    in srgb,
    var(--room-floor) 74%,
    var(--lad-palette-orange-650)
  );
}
.room-floor {
  height: 34%;
  @apply position-absolute;
  left: 0;
  right: 0;
  bottom: 0;
  background:
    repeating-linear-gradient(
      105deg,
      transparent 0 22px,
      color-mix(in srgb, var(--lad-palette-orange-750) 10%, transparent) 23px
        25px
    ),
    linear-gradient(
      var(--room-floor),
      color-mix(in srgb, var(--room-floor) 78%, var(--lad-palette-orange-600))
    );
}
.room-wear {
  @apply position-absolute pointer-events-none;
  inset: 0;
  z-index: 3;
  opacity: 0.18;
  transition: opacity 0.35s ease;
}
.room-wear::before {
  width: 48px;
  height: 42px;
  content: "";
  @apply position-absolute;
  top: 0;
  right: 0;
  background: repeating-radial-gradient(
    circle at 100% 0,
    transparent 0 8px,
    color-mix(in srgb, var(--lad-palette-orange-650) 35%, transparent) 9px 10px,
    transparent 11px 15px
  );
  clip-path: polygon(28% 0, 100% 0, 100% 74%);
}
.room-wear i {
  width: 22px;
  height: 7px;
  @apply position-absolute;
  bottom: 8%;
  border-radius: 50%;
  background: color-mix(
    in srgb,
    var(--lad-palette-orange-750) 40%,
    transparent
  );
  filter: blur(0.4px);
}
.room-wear i:nth-child(1) {
  left: 8%;
  transform: rotate(-8deg);
}
.room-wear i:nth-child(2) {
  left: 33%;
  bottom: 17%;
  width: 13px;
}
.room-wear i:nth-child(3) {
  right: 12%;
  bottom: 9%;
  width: 29px;
}
.room-wear i:nth-child(4) {
  left: 57%;
  bottom: 4%;
  width: 9px;
}
.room-wear i:nth-child(5) {
  right: 31%;
  bottom: 20%;
  width: 17px;
}
.room-wear i:nth-child(6) {
  left: 19%;
  bottom: 3%;
  width: 8px;
}
.energy-low .room-wear {
  opacity: 0.52;
}
.energy-critical .room-wear {
  opacity: 0.88;
}
.energy-low .room-window {
  filter: saturate(0.72) brightness(0.9);
}
.energy-critical .room-window {
  filter: saturate(0.45) brightness(0.76);
  box-shadow:
    inset 0 0 0 2px
      color-mix(in srgb, var(--lad-palette-orange-650) 15%, transparent),
    0 2px 0 color-mix(in srgb, var(--lad-palette-muted-700) 12%, transparent);
}
.energy-low :deep(.entity-member),
.energy-low :deep(.entity-pet) {
  filter: saturate(0.78);
}
.energy-critical :deep(.entity-member),
.energy-critical :deep(.entity-pet) {
  filter: saturate(0.58) brightness(0.88);
}
.energy-low .garden-flower-bed i,
.energy-low .garden-plant-shelf i {
  transform: translateY(5px) rotate(18deg);
  filter: saturate(0.65);
}
.energy-critical .garden-flower-bed i,
.energy-critical .garden-plant-shelf i {
  transform: translateY(9px) rotate(38deg);
  filter: saturate(0.35) brightness(0.8);
}
.energy-low .garden-patio {
  filter: saturate(0.8);
}
.energy-critical .garden-patio,
.energy-critical .garden-trellis {
  filter: saturate(0.55) brightness(0.9);
}
.energy-low :deep(.plant-leaves),
.energy-low :deep(.flower-heads) {
  transform-box: fill-box;
  transform-origin: bottom center;
  transform: rotate(12deg) translateY(4px);
  filter: saturate(0.62);
}
.energy-critical :deep(.plant-leaves),
.energy-critical :deep(.flower-heads) {
  transform-box: fill-box;
  transform-origin: bottom center;
  transform: rotate(28deg) translateY(9px) scaleY(0.82);
  filter: saturate(0.32) brightness(0.78);
}
.kitchen-tiles {
  @apply position-absolute;
  inset: 31% 4% 35%;
  opacity: 0.25;
  background:
    repeating-linear-gradient(
      0deg,
      transparent 0 14px,
      var(--lad-palette-muted) 15px 16px
    ),
    repeating-linear-gradient(
      90deg,
      transparent 0 14px,
      var(--lad-palette-muted) 15px 16px
    );
}
.bunting {
  @apply position-absolute d-flex;
  top: 31px;
  right: 9px;
}
.bunting i {
  width: 0;
  height: 0;
  border-top: 0;
  border-right: 6px solid transparent;
  border-bottom: 12px solid var(--lad-palette-red-300);
  border-left: 6px solid transparent;
}
.bunting i:nth-child(2n) {
  border-bottom-color: var(--lad-palette-yellow);
}
.bunting i:nth-child(3n) {
  border-bottom-color: var(--lad-palette-teal-400);
}
.paint-dots {
  @apply position-absolute;
  top: 39px;
  right: 11px;
}
.paint-dots i {
  width: 8px;
  height: 8px;
  @apply d-inline-block;
  margin: 2px;
  border-radius: 50%;
  background: var(--lad-palette-red-300);
}
.paint-dots i:nth-child(2n) {
  background: var(--lad-palette-teal-400);
}
.paint-dots i:nth-child(3n) {
  background: var(--lad-palette-yellow);
}
.ladi-perch {
  width: 126px;
  height: 46%;
  @apply position-absolute;
  top: 0;
  left: 18%;
  z-index: 5;
  transform: translateX(-50%);
  pointer-events: none;
}
.ladi-perch img {
  width: 100%;
  height: 100%;
  @apply d-block;
  object-fit: fill;
  filter: drop-shadow(
    0 4px 3px color-mix(in srgb, var(--lad-palette-orange-750) 20%, transparent)
  );
}
.ladi-perch.occupied {
  animation: perch-sway 4.8s ease-in-out infinite;
}
.garden-zone {
  grid-column: span 2;
  min-height: 235px;
  border: 0;
  background: linear-gradient(
    var(--lad-palette-background) 0 57%,
    var(--lad-palette-surface) 57% 100%
  );
  box-shadow: none;
  isolation: isolate;
}
.garden-zone::after {
  @apply d-none;
}
.garden-sky {
  @apply position-absolute inset-0;
  z-index: 0;
  background: linear-gradient(
    var(--lad-palette-background),
    var(--lad-palette-background) 59%,
    transparent 60%
  );
}
.garden-sky::before,
.garden-sky::after {
  content: "";
  width: 66px;
  height: 21px;
  @apply position-absolute;
  top: 18%;
  left: 10%;
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-palette-white) 85%, transparent);
  box-shadow:
    20px -9px 0 3px
      color-mix(in srgb, var(--lad-palette-white) 85%, transparent),
    38px 0 0 color-mix(in srgb, var(--lad-palette-white) 85%, transparent);
  animation: garden-cloud-drift 9s ease-in-out infinite alternate;
}
.garden-sky::after {
  top: 31%;
  left: 68%;
  opacity: 0.78;
  transform: scale(0.7);
  animation-delay: -4s;
  animation-duration: 12s;
}
.garden-sky i {
  width: 52px;
  height: 52px;
  @apply position-absolute;
  top: 20px;
  right: 12%;
  border-radius: 50%;
  background: var(--lad-palette-amber-250);
  box-shadow:
    0 0 0 11px color-mix(in srgb, var(--lad-palette-amber-250) 20%, transparent),
    0 0 30px color-mix(in srgb, var(--lad-palette-amber-250) 30%, transparent);
  animation: garden-sun-glow 3.6s ease-in-out infinite;
}
.garden-sky i::before {
  content: "";
  @apply position-absolute;
  inset: -24px;
  background: repeating-conic-gradient(
    from 0deg,
    var(--lad-palette-amber-250) 0 5deg,
    transparent 5deg 45deg
  );
  mask: radial-gradient(
    circle,
    transparent 0 45%,
    var(--lad-palette-black) 47% 58%,
    transparent 60%
  );
  animation: garden-sun-rays 3.6s ease-in-out infinite;
}
.garden-mountains {
  height: 47%;
  @apply position-absolute;
  right: -3%;
  bottom: 39%;
  left: -3%;
  z-index: 0;
  transform-origin: center bottom;
  animation: garden-mountain-breathe 15s ease-in-out infinite;
}
.garden-mountains i {
  @apply position-absolute;
  bottom: 0;
  background: var(--lad-palette-muted-250);
  clip-path: polygon(0 100%, 48% 3%, 100% 100%);
}
.garden-mountains i::after {
  content: "";
  width: 46%;
  height: 40%;
  @apply position-absolute;
  top: 4%;
  left: 27%;
  background: color-mix(in srgb, var(--lad-palette-surface) 90%, transparent);
  clip-path: polygon(50% 0, 100% 100%, 70% 69%, 54% 87%, 38% 64%, 0 100%);
}
.garden-mountains i:first-child {
  width: 45%;
  height: 78%;
  left: -4%;
  opacity: 0.88;
}
.garden-mountains i:nth-child(2) {
  width: 54%;
  height: 100%;
  left: 24%;
}
.garden-mountains i:last-child {
  width: 44%;
  height: 73%;
  right: -4%;
  opacity: 0.9;
}
.garden-trellis {
  width: 43%;
  height: 38%;
  @apply position-absolute overflow-hidden;
  top: 18%;
  right: 5%;
  z-index: 0;
  border: 0.25rem solid var(--lad-palette-orange-500);
  border-radius: 0.75rem;
  background:
    repeating-linear-gradient(
      45deg,
      transparent 0 0.6875rem,
      color-mix(in srgb, var(--lad-palette-orange-500) 45%, transparent) 0.75rem
        0.875rem
    ),
    repeating-linear-gradient(
      -45deg,
      transparent 0 0.6875rem,
      color-mix(in srgb, var(--lad-palette-orange-500) 45%, transparent) 0.75rem
        0.875rem
    ),
    color-mix(in srgb, var(--lad-palette-background) 70%, transparent);
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-palette-orange-750) 10%, transparent);
}
.garden-trellis i {
  width: 1rem;
  height: 0.625rem;
  @apply position-absolute;
  border-radius: 70% 30% 65% 35%;
  background: var(--lad-palette-mint-450);
  transform: rotate(-25deg);
}
.garden-trellis i:nth-child(1) {
  top: 10%;
  left: 8%;
}
.garden-trellis i:nth-child(2) {
  top: 31%;
  left: 28%;
  transform: rotate(20deg);
}
.garden-trellis i:nth-child(3) {
  top: 9%;
  right: 24%;
}
.garden-trellis i:nth-child(4) {
  top: 50%;
  right: 9%;
  transform: rotate(28deg);
}
.garden-trellis i:nth-child(5) {
  bottom: 8%;
  left: 16%;
}
.garden-trellis i:nth-child(6) {
  right: 36%;
  bottom: 17%;
  transform: rotate(16deg);
}
.garden-plant-shelf {
  width: 38%;
  height: 1.125rem;
  @apply position-absolute d-flex align-end justify-space-around;
  top: 47%;
  right: 7%;
  z-index: 1;
  border: 0.1875rem solid var(--lad-palette-orange-650);
  border-radius: 0.375rem;
  background: var(--lad-palette-orange-400-2);
  box-shadow: 0 0.375rem 0
    color-mix(in srgb, var(--lad-palette-orange-750) 16%, transparent);
}
.garden-plant-shelf span {
  width: 1.375rem;
  height: 1rem;
  @apply position-relative;
  bottom: 0.5rem;
  border: 0.125rem solid var(--lad-palette-orange-650);
  border-radius: 0.25rem 0.25rem 0.5rem 0.5rem;
  background: var(--lad-palette-amber-200);
}
.garden-plant-shelf i {
  width: 1.5rem;
  height: 1.75rem;
  @apply position-absolute;
  left: -0.1875rem;
  bottom: 0.625rem;
  background:
    radial-gradient(
      ellipse at 30% 62%,
      var(--lad-palette-mint-450) 0 29%,
      transparent 31%
    ),
    radial-gradient(
      ellipse at 70% 58%,
      var(--lad-palette-teal-400) 0 28%,
      transparent 30%
    ),
    radial-gradient(
      ellipse at 50% 25%,
      var(--lad-palette-green-250) 0 31%,
      transparent 33%
    );
  transform-origin: bottom center;
}
.garden-patio {
  height: 48%;
  @apply position-absolute;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 0;
  border-top: 0.1875rem solid
    color-mix(in srgb, var(--lad-palette-teal-600) 12%, transparent);
  background:
    linear-gradient(
        32deg,
        transparent 47%,
        color-mix(in srgb, var(--lad-palette-teal-600) 10%, transparent) 48% 51%,
        transparent 52%
      )
      0 0/3.5rem 2.25rem,
    linear-gradient(
        -32deg,
        transparent 47%,
        color-mix(in srgb, var(--lad-palette-teal-600) 10%, transparent) 48% 51%,
        transparent 52%
      )
      0 0/3.5rem 2.25rem,
    linear-gradient(var(--lad-palette-background), var(--lad-palette-surface));
}
.garden-zone:has(.terrace-transition) {
  @apply overflow-visible;
}
.terrace-transition {
  width: 106px;
  height: 184px;
  @apply position-absolute pointer-events-none;
  left: -53px;
  bottom: 4%;
  z-index: 34;
  filter: drop-shadow(
    0 7px 5px
      color-mix(in srgb, var(--lad-palette-muted-750-2) 18%, transparent)
  );
}
.terrace-frame {
  @apply position-absolute;
  inset: 0 7px 20px;
  border: 5px solid var(--lad-palette-muted-600-2);
  border-radius: 9px 9px 3px 3px;
  background: var(--lad-palette-blue-150);
  box-shadow:
    0 0 0 3px var(--lad-palette-background),
    0 0 0 5px color-mix(in srgb, var(--lad-palette-muted-700) 18%, transparent),
    inset 0 0 0 2px
      color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  perspective: 190px;
}
.terrace-frame::before {
  width: 4px;
  content: "";
  @apply position-absolute;
  top: 0;
  bottom: 0;
  left: calc(50% - 2px);
  z-index: 2;
  background: var(--lad-palette-muted-600-2);
  box-shadow: 0 0 0 1px
    color-mix(in srgb, var(--lad-palette-white) 50%, transparent);
}
.terrace-frame::after {
  height: 4px;
  content: "";
  @apply position-absolute;
  right: 0;
  bottom: 49%;
  left: 0;
  z-index: 2;
  background: var(--lad-palette-muted-600-2);
  box-shadow: 0 0 0 1px
    color-mix(in srgb, var(--lad-palette-white) 50%, transparent);
}
.terrace-glass {
  @apply position-absolute;
  top: 3px;
  bottom: 3px;
  width: calc(50% - 4px);
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
  background:
    linear-gradient(
      145deg,
      color-mix(in srgb, var(--lad-palette-white) 72%, transparent) 0 17%,
      transparent 18% 46%,
      color-mix(in srgb, var(--lad-palette-blue-350) 20%, transparent) 47% 52%,
      transparent 53%
    ),
    linear-gradient(
      180deg,
      color-mix(in srgb, var(--lad-palette-blue-150) 82%, transparent),
      color-mix(in srgb, var(--lad-palette-surface) 70%, transparent)
    );
  backdrop-filter: blur(1px);
}
.terrace-glass--fixed {
  left: 2px;
}
.terrace-glass--door {
  right: 2px;
  transform-origin: right center;
  transform: rotateY(-9deg);
  box-shadow: -3px 1px 4px
    color-mix(in srgb, var(--lad-palette-muted-700) 14%, transparent);
}
.opens-to-garden .terrace-glass--door {
  transform: rotateY(-56deg);
  box-shadow: -8px 2px 7px
    color-mix(in srgb, var(--lad-palette-muted-700) 18%, transparent);
}
.terrace-handle {
  width: 4px;
  height: 15px;
  @apply position-absolute;
  top: 48%;
  right: 9px;
  z-index: 4;
  border-radius: var(--lad-radius-pill);
  background: var(--lad-palette-orange-650);
  box-shadow:
    0 0 0 2px var(--lad-palette-amber-200),
    0 2px 2px color-mix(in srgb, var(--lad-palette-muted-700) 18%, transparent);
}
.terrace-threshold {
  height: 18px;
  @apply position-absolute;
  right: 0;
  bottom: 3px;
  left: 0;
  border: 3px solid var(--lad-palette-muted-600-2);
  border-radius: 4px 4px 9px 9px;
  background: repeating-linear-gradient(
    90deg,
    var(--lad-palette-amber-200) 0 15px,
    var(--lad-palette-orange-350) 16px 18px
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-muted-700) 16%, transparent);
  transform: perspective(76px) rotateX(46deg);
}
.garden-path {
  @apply position-absolute d-flex align-end;
  right: 9%;
  bottom: 4%;
  left: 38%;
  z-index: 1;
  gap: 7px;
  transform: rotate(-7deg);
}
.garden-path i {
  width: 31px;
  height: 13px;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-orange-600) 20%, transparent);
  border-radius: 48%;
  background: var(--lad-palette-amber-200);
  box-shadow: inset 0 2px 0
    color-mix(in srgb, var(--lad-palette-white) 40%, transparent);
}
.garden-path i:nth-child(2n) {
  transform: translateY(-7px) scale(0.88);
}
.garden-flower-bed {
  @apply position-absolute d-flex align-end;
  bottom: 48px;
  left: 10%;
  z-index: 2;
}
.garden-flower-bed::before {
  content: "";
  width: 94px;
  height: 23px;
  @apply position-absolute;
  left: -8px;
  bottom: -5px;
  border-radius: 50%;
  background: var(--lad-palette-orange-600);
  opacity: 0.75;
}
.garden-flower-bed i {
  width: 13px;
  height: 13px;
  z-index: 1;
  margin-inline: 1px;
  border: 3px solid var(--lad-palette-amber-150);
  border-radius: 50%;
  background: var(--lad-palette-red-300);
  box-shadow: 0 9px 0 -5px var(--lad-palette-teal-600);
}
.garden-flower-bed i:nth-child(2n) {
  background: var(--lad-palette-purple-350);
  transform: translateY(-6px);
}
.garden-flower-bed i:nth-child(3n) {
  background: var(--lad-palette-yellow);
}
@keyframes garden-cloud-drift {
  from {
    translate: -5px 0;
  }
  to {
    translate: 8px 2px;
  }
}
@keyframes garden-mountain-breathe {
  0%,
  100% {
    transform: scaleX(1) translateY(0);
  }
  50% {
    transform: scaleX(1.012) translateY(1px);
  }
}
@keyframes garden-sun-glow {
  0%,
  100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.055);
  }
}
@keyframes garden-sun-rays {
  0%,
  100% {
    opacity: 0.82;
    transform: rotate(0deg) scale(0.96);
  }
  50% {
    opacity: 1;
    transform: rotate(4deg) scale(1.04);
  }
}
@keyframes perch-sway {
  0%,
  100% {
    rotate: -1deg;
  }
  50% {
    rotate: 1deg;
  }
}
.dollhouse-layout.compact {
  min-height: 164px;
  padding: 4px;
  gap: 3px;
  border-width: 0;
  border-radius: 10px;
  box-shadow: none;
}
.dollhouse-layout.compact .dollhouse-room {
  min-height: 78px;
  border-width: 2px;
  border-radius: 9px 9px 4px 4px;
}
.dollhouse-layout.compact.single-zone .dollhouse-room,
.dollhouse-layout.compact.room-count-1:not(:has(.garden-zone)) .dollhouse-room {
  min-height: 164px;
}
.compact .dollhouse-room > header {
  top: 4px;
  left: 4px;
  padding: 2px 5px;
  font-size: 0.375rem;
}
.compact .room-window {
  width: 27px;
  height: 23px;
  top: 12px;
  border-width: 3px;
}
.dollhouse-layout.compact .garden-zone {
  min-height: 78px;
}
.dollhouse-layout.compact.single-zone .garden-zone {
  min-height: 164px;
}
.compact .ladi-motivation {
  bottom: 8px;
}
.dollhouse-layout.contextual-zone.has-pixi-background > .dollhouse-room {
  background: transparent;
}
@include respond-down(mobile) {
  .dollhouse-layout:not(.compact) {
    grid-template-columns: 1fr;
  }
  .dollhouse-layout:not(.compact) .dollhouse-room,
  .dollhouse-layout:not(.compact) .garden-zone {
    grid-column: auto;
    min-height: 245px;
  }
}
@include reduced-motion {
  .garden-sky::before,
  .garden-sky::after,
  .garden-sky i,
  .garden-sky i::before,
  .garden-mountains,
  .ladi-perch {
    animation: none;
  }
}
</style>
