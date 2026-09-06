<template>
  <div
    class="dollhouse-layout"
    :class="[{ compact, editable, 'single-zone': isSingleZone, 'contextual-zone': isContextualZone, 'is-dragging-furniture': draggingFurniture }, `room-count-${visibleZoneIds.length}`]"
    :style="contextGridStyle"
  >
    <section
      v-for="room in displayedRooms"
      :key="room.id"
      class="dollhouse-room"
      :class="[`room-${room.id}`, { 'is-focused': room.id === selectedZoneId, 'is-peek': isContextualZone && room.id !== selectedZoneId }]"
      :data-zone-id="room.id"
      :style="{ '--room-wall': room.wall, '--room-floor': room.floor }"
    >
      <button v-if="isPreviewZone(room.id)" class="room-navigation-hitbox" type="button" :aria-label="`${room.name} öffnen`" @click="selectPreviewZone(room.id)" />
      <header v-if="showRoomLabels"><span>{{ room.icon }}</span><strong>{{ room.name }}</strong></header>
      <div class="room-window" aria-hidden="true"><i /><i /><i /><i /></div>
      <div class="room-baseboard" aria-hidden="true" />
      <div class="room-floor" aria-hidden="true" />
      <div v-if="room.id === 'kitchen'" class="room-detail kitchen-tiles" aria-hidden="true" />
      <div v-if="room.id === 'children-room'" class="room-detail bunting" aria-hidden="true"><i v-for="index in 5" :key="index" /></div>
      <div v-if="room.id === 'creative-room'" class="room-detail paint-dots" aria-hidden="true"><i v-for="index in 6" :key="index" /></div>
      <div v-if="room.id === 'living-room'" class="ladi-perch" :class="{ occupied: isLadiOnPerch }" aria-hidden="true">
        <svg preserveAspectRatio="none" viewBox="0 0 150 120">
          <path class="perch-rope" d="M38 0q-2 54 7 101M112 0q2 54-7 101" />
          <path class="perch-branch" d="M19 105q37-10 67 0 24 7 47-3" />
          <path class="perch-twig" d="M34 103 23 89m91 15 13-14M82 104l9-16" />
          <path class="perch-leaf" d="M19 88q15-9 17 5-13 8-17-5Zm105 0q-13-10-17 4 11 10 17-4Zm-31-4q9-12 17-1-5 12-17 1Z" />
        </svg>
      </div>
      <HouseLayoutEntity
        v-for="placement in visiblePlacements(room.id)"
        :key="placement.id"
        :accessory="accessoryFor(placement)"
        :drag-offset="dragOffset(placement.id)"
        :editable="editable || (storageOpen && placement.entityType === 'furniture')"
        :member="memberFor(placement)"
        :pet="petFor(placement)"
        :perched="ladiIsPerched(placement)"
        :placement="placement"
        :score="score"
        :speech="placement.entityType === 'ladi' ? ladiSpeech : ''"
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
      <button v-if="isPreviewZone('garden')" class="room-navigation-hitbox" type="button" aria-label="Garten öffnen" @click="selectPreviewZone('garden')" />
      <header v-if="showRoomLabels"><span>🌿</span><strong>Garten</strong></header>
      <div class="garden-sky" aria-hidden="true"><i /></div>
      <div class="garden-mountains" aria-hidden="true"><i /><i /><i /></div>
      <div class="garden-hills" aria-hidden="true"><i /><i /></div>
      <div class="garden-hedge" aria-hidden="true" />
      <div class="garden-lawn" aria-hidden="true" />
      <div class="garden-path" aria-hidden="true"><i /><i /><i /><i /></div>
      <div class="garden-flower-bed" aria-hidden="true"><i /><i /><i /><i /><i /></div>
      <HouseLayoutEntity
        v-for="placement in visiblePlacements('garden')"
        :key="placement.id"
        :accessory="accessoryFor(placement)"
        :drag-offset="dragOffset(placement.id)"
        :editable="editable || (storageOpen && placement.entityType === 'furniture')"
        :member="memberFor(placement)"
        :pet="petFor(placement)"
        :perched="false"
        :placement="placement"
        :score="score"
        :speech="placement.entityType === 'ladi' ? ladiSpeech : ''"
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
import { computed, onMounted, onUnmounted, ref } from 'vue';

import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import type { HouseRoomDefinition, HouseZoneId } from '@/domain/house';
import type { FamilyMember, FamilyPet, HouseAccessory, HouseLayoutPlacement, HouseLayoutPlacementId } from '@/domain/types';

import HouseLayoutEntity from './HouseLayoutEntity.vue';

interface DragPayload {
  captureTarget: HTMLElement;
  lastX: number;
  lastY: number;
  placementId: HouseLayoutPlacementId;
  pointerId: number;
  startX: number;
  startY: number;
  offsetX: number;
  offsetY: number;
}

const props = withDefaults(defineProps<{
  accessories: HouseAccessory[];
  compact?: boolean;
  editable?: boolean;
  includeGarden?: boolean;
  contextualNeighbors?: boolean;
  members: FamilyMember[];
  pets: FamilyPet[];
  placements: HouseLayoutPlacement[];
  rooms: ReadonlyArray<HouseRoomDefinition>;
  score: number;
  selectedZoneId?: HouseZoneId | 'all';
  showRoomLabels?: boolean;
  storageOpen?: boolean;
}>(), { compact: false, contextualNeighbors: false, editable: false, includeGarden: false, selectedZoneId: 'all', showRoomLabels: true, storageOpen: false });
const emit = defineEmits<{
  move: [placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number];
  reset: [placementId: HouseLayoutPlacementId];
  'select-zone': [zoneId: HouseZoneId];
  'drag-state': [entityType: HouseLayoutPlacement['entityType'] | null];
  store: [accessoryId: HouseAccessory['id']];
}>();

const drag = ref<DragPayload | null>(null);
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
const visibleZoneIds = computed<HouseZoneId[]>(() => orderedZoneIds.value.filter((zoneId) => contextualZoneIds.value.includes(zoneId)));
const isContextualZone = computed(() => props.contextualNeighbors && props.selectedZoneId !== 'all' && visibleZoneIds.value.length > 1);
const isSingleZone = computed(() => props.selectedZoneId !== 'all' && !isContextualZone.value);
const contextGridStyle = computed(() => {
  if (!isContextualZone.value) return undefined;
  const selectedIndex = visibleZoneIds.value.indexOf(props.selectedZoneId as HouseZoneId);
  const columns = visibleZoneIds.value.map((_, index) => index === selectedIndex ? 'minmax(0, 3fr)' : 'minmax(52px, 1fr)').join(' ');
  return { '--context-columns': columns };
});
const isPreviewZone = (zoneId: HouseZoneId) => isContextualZone.value && zoneId !== props.selectedZoneId;
const selectPreviewZone = (zoneId: HouseZoneId) => {
  if (isPreviewZone(zoneId)) emit('select-zone', zoneId);
};
const visiblePlacements = (zoneId: HouseZoneId) => props.placements
  .filter((placement) => placement.zoneId === zoneId)
  .filter((placement) => placement.entityType !== 'furniture' || Boolean(accessoryFor(placement)?.owned && accessoryFor(placement)?.equipped))
  .sort((left, right) => left.y - right.y);
const accessoryFor = (placement: HouseLayoutPlacement) => placement.entityType === 'furniture'
  ? props.accessories.find((accessory) => accessory.id === placement.entityId)
  : undefined;
const memberFor = (placement: HouseLayoutPlacement): (FamilyMember & { resolvedAppearance: AvatarAppearance }) | undefined => {
  if (placement.entityType !== 'member') return undefined;
  const member = props.members.find((item) => item.id === placement.entityId);
  if (!member) return undefined;
  const index = props.members.filter((item) => item.role === member.role).findIndex((item) => item.id === member.id);
  if (member.appearance) return { ...member, resolvedAppearance: member.appearance };
  if (member.role === 'guardian') return { ...member, resolvedAppearance: createGuardianAvatarAppearance(index % 2 === 0 ? 'adult' : 'grandpa') };
  const fallback = createDefaultAvatarAppearance();
  const variants: ReadonlyArray<Partial<AvatarAppearance>> = [
    { hair: 'ponytail', outfitColorId: 'outfit-blue' },
    { hair: 'short', hairColorId: 'hair-black', outfit: 'overalls', outfitColorId: 'outfit-gold' },
    { hair: 'curls', hairColorId: 'hair-brown', outfit: 'space', outfitColorId: 'outfit-ocean' },
  ];
  return { ...member, resolvedAppearance: { ...fallback, ...(variants[index % variants.length] ?? {}) } };
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
const draggingFurniture = computed(() => {
  const placement = props.placements.find(item => item.id === drag.value?.placementId);
  return placement?.entityType === 'furniture';
});
const dragOffset = (placementId: HouseLayoutPlacementId) => drag.value?.placementId === placementId
  ? { x: drag.value.offsetX, y: drag.value.offsetY }
  : undefined;
const startDrag = (event: PointerEvent, placement: HouseLayoutPlacement) => {
  if (!props.editable && !(props.storageOpen && placement.entityType === 'furniture')) return;
  event.preventDefault();
  event.stopPropagation();
  const target = event.currentTarget as HTMLElement;
  stopDragTracking();
  drag.value = {
    captureTarget: target,
    lastX: event.clientX,
    lastY: event.clientY,
    placementId: placement.id,
    pointerId: event.pointerId,
    startX: event.clientX,
    startY: event.clientY,
    offsetX: 0,
    offsetY: 0,
  };
  emit('drag-state', placement.entityType);
  try {
    target.setPointerCapture(event.pointerId);
  } catch {
    // Global listeners below still keep mouse and touch drags reliable.
  }
  window.addEventListener('pointermove', trackDrag);
  window.addEventListener('pointerup', finishDrag);
  window.addEventListener('pointercancel', cancelDrag);
  window.addEventListener('blur', cancelDrag);
};
const trackDrag = (event: PointerEvent) => {
  if (!drag.value || event.pointerId !== drag.value.pointerId) return;
  event.preventDefault();
  drag.value.lastX = event.clientX;
  drag.value.lastY = event.clientY;
  drag.value.offsetX = event.clientX - drag.value.startX;
  drag.value.offsetY = event.clientY - drag.value.startY;
};
const commitDrag = (clientX: number, clientY: number) => {
  const activeDrag = drag.value;
  if (!activeDrag) return;
  const elementsAtDropPoint = document.elementsFromPoint(clientX, clientY);
  const placement = props.placements.find((item) => item.id === activeDrag.placementId);
  const isOverStorage = elementsAtDropPoint.some((element) => element.closest('[data-furniture-storage]'));
  if (isOverStorage) {
    try {
      if (placement?.entityType === 'furniture') emit('store', placement.entityId);
      else emit('reset', activeDrag.placementId);
    } finally {
      drag.value = null;
      emit('drag-state', null);
      stopDragTracking(activeDrag);
    }
    return;
  }
  const zone = elementsAtDropPoint
    .map((element) => element.closest<HTMLElement>('[data-zone-id]'))
    .find((element) => element?.dataset.zoneId && unlockedZones.value.includes(element.dataset.zoneId as HouseZoneId));
  try {
    if (!zone?.dataset.zoneId) {
      emit('reset', activeDrag.placementId);
      return;
    }
    const bounds = zone.getBoundingClientRect();
    const isFloorEntity = placement?.entityType === 'member' || placement?.entityType === 'pet' || placement?.entityType === 'ladi';
    const accessory = placement ? accessoryFor(placement) : undefined;
    const isWallDecoration = accessory?.visual === 'wall-art' || accessory?.visual === 'bat-garland';
    const rawX = ((clientX - bounds.left) / bounds.width) * 100;
    const rawY = ((clientY - bounds.top) / bounds.height) * 100;
    const snapsToLadiPerch = placement?.entityType === 'ladi'
      && zone.dataset.zoneId === 'living-room'
      && rawX >= 6 && rawX <= 31
      && rawY >= 12 && rawY <= 47;
    const horizontalInset = isFloorEntity ? 7 : 3;
    const minimumY = isFloorEntity && !snapsToLadiPerch ? 62 : isWallDecoration ? 12 : 52;
    const maximumY = isWallDecoration ? 46 : 94;
    const x = snapsToLadiPerch ? 18 : Math.min(100 - horizontalInset, Math.max(horizontalInset, rawX));
    const y = snapsToLadiPerch ? 30 : Math.min(maximumY, Math.max(minimumY, rawY));
    emit('move', activeDrag.placementId, zone.dataset.zoneId as HouseZoneId, x, y);
  } finally {
    drag.value = null;
    emit('drag-state', null);
    stopDragTracking(activeDrag);
  }
};
const finishDrag = (event: PointerEvent) => {
  if (!drag.value || event.pointerId !== drag.value.pointerId) return;
  event.preventDefault();
  commitDrag(event.clientX, event.clientY);
};
const finishDragAtLastPosition = (event: PointerEvent) => {
  if (!drag.value || event.pointerId !== drag.value.pointerId) return;
  commitDrag(drag.value.lastX, drag.value.lastY);
};
const stopDragTracking = (activeDrag = drag.value) => {
  window.removeEventListener('pointermove', trackDrag);
  window.removeEventListener('pointerup', finishDrag);
  window.removeEventListener('pointercancel', cancelDrag);
  window.removeEventListener('blur', cancelDrag);
  if (activeDrag?.captureTarget.hasPointerCapture(activeDrag.pointerId)) {
    activeDrag.captureTarget.releasePointerCapture(activeDrag.pointerId);
  }
};
const cancelDrag = () => {
  const activeDrag = drag.value;
  drag.value = null;
  emit('drag-state', null);
  stopDragTracking(activeDrag);
  if (activeDrag) emit('reset', activeDrag.placementId);
};
const motivateLadi = () => {
  ladiMotivation.value = props.score < 2.5
    ? 'Ein kleiner Beitrag, dann bin ich wieder hellwach!'
    : props.score >= 4.8
      ? 'Super-Team, auf zur nächsten Familienmission!'
      : props.score >= 4.3
        ? 'Cool bleiben und gemeinsam das Tagesziel knacken!'
        : 'Jeder Beitrag macht unser Zuhause ein Stück lebendiger!';
  if (motivationTimer !== undefined) window.clearTimeout(motivationTimer);
  motivationTimer = window.setTimeout(() => { ladiMotivation.value = ''; }, 2800);
};
const perchMessages = ['Gemeinsam schaffen wir das!', 'Kleine Schritte, große Wirkung!', 'Wer hilft heute mit?', 'Ich glaube an euch!'];
const schedulePerchMessage = () => {
  perchScheduleTimer = window.setTimeout(() => {
    if (isLadiOnPerch.value) {
      perchMessage.value = perchMessages[Math.floor(Math.random() * perchMessages.length)] ?? perchMessages[0]!;
      if (perchMessageTimer !== undefined) window.clearTimeout(perchMessageTimer);
      perchMessageTimer = window.setTimeout(() => { perchMessage.value = ''; }, 2400);
    }
    schedulePerchMessage();
  }, 6500 + Math.round(Math.random() * 4500));
};
onMounted(schedulePerchMessage);
onUnmounted(() => {
  stopDragTracking();
  if (motivationTimer !== undefined) window.clearTimeout(motivationTimer);
  if (perchMessageTimer !== undefined) window.clearTimeout(perchMessageTimer);
  if (perchScheduleTimer !== undefined) window.clearTimeout(perchScheduleTimer);
});
</script>

<style scoped>
.dollhouse-layout {
  @apply position-relative d-grid overflow-hidden;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 7px;
  min-height: 430px;
  padding: 8px 8px 38px;
  border: 1px solid rgba(83, 68, 55, 0.18);
  border-radius: 24px;
  background: #af7656;
  box-shadow:
    inset 0 0 0 5px rgba(255, 255, 255, 0.18),
    0 8px 0 rgba(85, 64, 49, 0.12);
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
    color-mix(in srgb, var(--room-wall) 88%, white) 0 48%,
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
      rgba(91, 56, 39, 0.11) 33px 35px
    ),
    linear-gradient(
      var(--room-floor),
      color-mix(in srgb, var(--room-floor) 68%, #8b5e43)
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
  padding: 0;
  background: transparent;
}
.dollhouse-layout.is-dragging-furniture,
.dollhouse-layout.is-dragging-furniture .dollhouse-room {
  overflow: visible;
}
.dollhouse-layout.contextual-zone .dollhouse-room {
  grid-column: auto;
  min-height: 390px;
  border: 0;
  border-radius: 0;
  background: linear-gradient(
    180deg,
    color-mix(in srgb, var(--room-wall) 88%, white) 0 45%,
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
  box-shadow: 0 3px 4px rgba(92, 57, 38, 0.14);
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
    radial-gradient(circle at 74% 24%, #ffe481 0 12%, transparent 13%),
    linear-gradient(155deg, transparent 0 62%, #87c48b 63% 100%),
    linear-gradient(#9cdef0, #dff5ef);
  box-shadow:
    0 5px 0 rgba(109, 76, 51, 0.15),
    0 9px 16px rgba(76, 107, 102, 0.13);
}
.dollhouse-layout.contextual-zone
  .dollhouse-room.is-focused
  .room-window::after {
  content: "";
  position: absolute;
  right: -13px;
  bottom: -11px;
  left: -13px;
  height: 10px;
  border-radius: 5px;
  background: #fff4df;
  box-shadow: 0 3px 0 rgba(116, 79, 55, 0.14);
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-peek {
  opacity: 0.9;
  filter: saturate(0.86);
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-peek::before {
  content: "";
  position: absolute;
  inset: 0;
  z-index: 35;
  background: linear-gradient(
    90deg,
    rgba(255, 255, 255, 0.04),
    rgba(255, 255, 255, 0.18)
  );
  pointer-events: none;
  transition: background 0.18s ease;
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-peek:hover::before,
.dollhouse-layout.contextual-zone
  .dollhouse-room.is-peek:focus-visible::before {
  background: rgba(255, 255, 255, 0.03);
}
.dollhouse-layout.contextual-zone .dollhouse-room.is-peek:focus-visible {
  outline: 4px solid rgba(66, 177, 136, 0.62);
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
  outline: 4px solid rgba(66, 177, 136, 0.62);
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
  --room-wall: #fff3d2;
  --room-floor: #d9a574;
  min-width: 0;
  min-height: 190px;
  @apply position-relative overflow-hidden;
  border: 3px solid rgba(94, 61, 45, 0.62);
  border-radius: 17px 17px 10px 10px;
  background: linear-gradient(
    180deg,
    color-mix(in srgb, var(--room-wall) 88%, white) 0 64%,
    var(--room-floor) 64% 100%
  );
  box-shadow: inset 0 2px 0 rgba(255, 255, 255, 0.72);
}
.dollhouse-room > header {
  @apply position-absolute d-flex align-center;
  top: 7px;
  left: 8px;
  z-index: 40;
  gap: 4px;
  padding: 3px 7px;
  border: 1px solid rgba(77, 58, 46, 0.11);
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.78);
  color: #4c433d;
  font-size: 8px;
  backdrop-filter: blur(5px);
}
.dollhouse-room > header strong {
  font-weight: 900;
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
  border: 5px solid #fff9eb;
  border-radius: 9px;
  background: #9ed8e4;
  box-shadow: 0 2px 0 rgba(74, 101, 101, 0.13);
}
.room-window i {
  border: 1px solid rgba(255, 255, 255, 0.8);
}
.room-baseboard {
  height: 6px;
  @apply position-absolute;
  left: 0;
  right: 0;
  bottom: 34%;
  z-index: 1;
  background: color-mix(in srgb, var(--room-floor) 74%, #76513e);
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
      rgba(93, 61, 42, 0.1) 23px 25px
    ),
    linear-gradient(
      var(--room-floor),
      color-mix(in srgb, var(--room-floor) 78%, #8f6548)
    );
}
.kitchen-tiles {
  @apply position-absolute;
  inset: 31% 4% 35%;
  opacity: 0.25;
  background:
    repeating-linear-gradient(0deg, transparent 0 14px, #759485 15px 16px),
    repeating-linear-gradient(90deg, transparent 0 14px, #759485 15px 16px);
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
  border-bottom: 12px solid #ef7d82;
  border-left: 6px solid transparent;
}
.bunting i:nth-child(2n) {
  border-bottom-color: #f0c65b;
}
.bunting i:nth-child(3n) {
  border-bottom-color: #6ab99b;
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
  background: #ef8179;
}
.paint-dots i:nth-child(2n) {
  background: #6bb69b;
}
.paint-dots i:nth-child(3n) {
  background: #f0c45a;
}
.ladi-perch {
  width: 116px;
  height: 43%;
  @apply position-absolute;
  top: 0;
  left: 18%;
  z-index: 5;
  transform: translateX(-50%);
  pointer-events: none;
}
.ladi-perch svg {
  width: 100%;
  height: 100%;
  overflow: visible;
}
.perch-rope {
  fill: none;
  stroke: #91715a;
  stroke-dasharray: 3 3;
  stroke-linecap: round;
  stroke-width: 2.5;
}
.perch-branch,
.perch-twig {
  fill: none;
  stroke: #765039;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 8;
}
.perch-twig {
  stroke-width: 4;
}
.perch-leaf {
  fill: #71ad70;
  stroke: #477f52;
  stroke-linejoin: round;
  stroke-width: 2;
}
.ladi-perch.occupied {
  animation: perch-sway 4.8s ease-in-out infinite;
}
.garden-zone {
  grid-column: span 2;
  min-height: 235px;
  border: 0;
  background: linear-gradient(#dff5f8 0 54%, #b9dfa4 54% 100%);
  box-shadow: none;
  isolation: isolate;
}
.garden-zone::after {
  display: none;
}
.garden-sky {
  @apply position-absolute inset-0;
  z-index: 0;
  background: linear-gradient(#d9f3f8, #eef9ef 59%, transparent 60%);
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
  background: rgba(255, 255, 255, 0.84);
  box-shadow:
    20px -9px 0 3px rgba(255, 255, 255, 0.84),
    38px 0 0 rgba(255, 255, 255, 0.84);
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
  background: #f8dc78;
  box-shadow:
    0 0 0 11px rgba(248, 220, 120, 0.2),
    0 0 30px rgba(255, 226, 119, 0.28);
  animation: garden-sun-glow 3.6s ease-in-out infinite;
}
.garden-sky i::before {
  content: "";
  @apply position-absolute;
  inset: -24px;
  background: repeating-conic-gradient(
    from 0deg,
    #f8d86f 0 5deg,
    transparent 5deg 45deg
  );
  mask: radial-gradient(
    circle,
    transparent 0 45%,
    #000 47% 58%,
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
  background: #b9ced0;
  clip-path: polygon(0 100%, 48% 3%, 100% 100%);
}
.garden-mountains i::after {
  content: "";
  width: 46%;
  height: 40%;
  @apply position-absolute;
  top: 4%;
  left: 27%;
  background: rgba(246, 251, 247, 0.88);
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
.garden-hills {
  height: 31%;
  @apply position-absolute;
  right: -12%;
  bottom: 34%;
  left: -12%;
  z-index: 0;
  transform-origin: center bottom;
  animation: garden-hill-drift 12s ease-in-out infinite alternate;
}
.garden-hills i {
  width: 68%;
  height: 100%;
  @apply position-absolute;
  bottom: 0;
  border-radius: 50% 50% 0 0 / 76% 76% 0 0;
  background: #9ed09a;
}
.garden-hills i:first-child {
  left: 0;
}
.garden-hills i:last-child {
  right: 0;
  background: #83bd82;
  transform: translateY(18%);
}
.garden-hedge {
  height: 16%;
  @apply position-absolute;
  left: -2%;
  right: -2%;
  bottom: 35%;
  z-index: 0;
  background:
    radial-gradient(circle at 15px 2px, #5fa767 14px, transparent 15px) 0 0/31px
      28px repeat-x,
    linear-gradient(transparent 22px, #61a463 23px);
  opacity: 0.82;
}
.garden-lawn {
  height: 43%;
  @apply position-absolute;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 0;
  background:
    radial-gradient(
      ellipse at 22% 4%,
      rgba(255, 255, 255, 0.2) 0 15%,
      transparent 16%
    ),
    repeating-linear-gradient(
      168deg,
      transparent 0 54px,
      rgba(66, 124, 70, 0.075) 55px 57px
    ),
    linear-gradient(#b8dfa3, #a8d18e);
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
  border: 2px solid rgba(135, 101, 67, 0.22);
  border-radius: 48%;
  background: #e7d6a9;
  box-shadow: inset 0 2px 0 rgba(255, 255, 255, 0.42);
}
.garden-path i:nth-child(2n) {
  transform: translateY(-7px) scale(0.88);
}
.garden-flower-bed {
  @apply position-absolute d-flex align-end;
  bottom: 8px;
  left: 8%;
  z-index: 1;
}
.garden-flower-bed::before {
  content: "";
  width: 94px;
  height: 23px;
  @apply position-absolute;
  left: -8px;
  bottom: -5px;
  border-radius: 50%;
  background: #846346;
  opacity: 0.75;
}
.garden-flower-bed i {
  width: 13px;
  height: 13px;
  z-index: 1;
  margin-inline: 1px;
  border: 3px solid #fff3a8;
  border-radius: 50%;
  background: #ee7e80;
  box-shadow: 0 9px 0 -5px #4f955c;
}
.garden-flower-bed i:nth-child(2n) {
  background: #9f8bd8;
  transform: translateY(-6px);
}
.garden-flower-bed i:nth-child(3n) {
  background: #efb84e;
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
@keyframes garden-hill-drift {
  from {
    transform: translateX(-3px);
  }
  to {
    transform: translateX(4px);
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
  font-size: 6px;
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
@media (max-width: 420px) {
  .dollhouse-layout:not(.compact) {
    grid-template-columns: 1fr;
  }
  .dollhouse-layout:not(.compact) .dollhouse-room,
  .dollhouse-layout:not(.compact) .garden-zone {
    grid-column: auto;
    min-height: 245px;
  }
}
@media (prefers-reduced-motion: reduce) {
  .garden-sky::before,
  .garden-sky::after,
  .garden-sky i,
  .garden-sky i::before,
  .garden-mountains,
  .garden-hills,
  .ladi-perch {
    animation: none;
  }
}
</style>
