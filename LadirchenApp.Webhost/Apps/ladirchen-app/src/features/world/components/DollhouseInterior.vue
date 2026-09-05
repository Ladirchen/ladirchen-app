<template>
  <div
    class="dollhouse-layout"
    :class="[{ compact, editable, 'single-zone': selectedZoneId !== 'all' }, `room-count-${displayedRooms.length}`]"
    @pointermove="trackDrag"
    @pointerup="finishDrag"
    @pointercancel="cancelDrag"
  >
    <section
      v-for="room in displayedRooms"
      :key="room.id"
      class="dollhouse-room"
      :class="`room-${room.id}`"
      :data-zone-id="room.id"
      :style="{ '--room-wall': room.wall, '--room-floor': room.floor }"
    >
      <header v-if="showRoomLabels"><span>{{ room.icon }}</span><strong>{{ room.name }}</strong></header>
      <div class="room-window" aria-hidden="true"><i /><i /><i /><i /></div>
      <div class="room-baseboard" aria-hidden="true" />
      <div class="room-floor" aria-hidden="true" />
      <div v-if="room.id === 'kitchen'" class="room-detail kitchen-tiles" aria-hidden="true" />
      <div v-if="room.id === 'children-room'" class="room-detail bunting" aria-hidden="true"><i v-for="index in 5" :key="index" /></div>
      <div v-if="room.id === 'creative-room'" class="room-detail paint-dots" aria-hidden="true"><i v-for="index in 6" :key="index" /></div>
      <HouseLayoutEntity
        v-for="placement in visiblePlacements(room.id)"
        :key="placement.id"
        :accessory="accessoryFor(placement)"
        :drag-offset="dragOffset(placement.id)"
        :editable="editable"
        :member="memberFor(placement)"
        :pet="petFor(placement)"
        :placement="placement"
        :score="score"
        @ladi-interact="motivateLadi"
        @pointerdown="startDrag"
      />
    </section>

    <section v-if="gardenIsVisible" class="dollhouse-room garden-zone" data-zone-id="garden">
      <header v-if="showRoomLabels"><span>🌿</span><strong>Garten</strong></header>
      <div class="garden-sky" aria-hidden="true"><i /></div>
      <div class="garden-hedge" aria-hidden="true" />
      <div class="garden-lawn" aria-hidden="true" />
      <div class="garden-path" aria-hidden="true"><i /><i /><i /><i /></div>
      <div class="garden-flower-bed" aria-hidden="true"><i /><i /><i /><i /><i /></div>
      <HouseLayoutEntity
        v-for="placement in visiblePlacements('garden')"
        :key="placement.id"
        :accessory="accessoryFor(placement)"
        :drag-offset="dragOffset(placement.id)"
        :editable="editable"
        :member="memberFor(placement)"
        :pet="petFor(placement)"
        :placement="placement"
        :score="score"
        @ladi-interact="motivateLadi"
        @pointerdown="startDrag"
      />
    </section>

    <Transition name="motivation">
      <div v-if="ladiMotivation" class="ladi-motivation" role="status"><span>🦥</span>{{ ladiMotivation }}</div>
    </Transition>
  </div>
</template>

<script lang="ts" setup>
import { computed, onUnmounted, ref } from 'vue';

import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import type { HouseRoomDefinition, HouseZoneId } from '@/domain/house';
import type { FamilyMember, FamilyPet, HouseAccessory, HouseLayoutPlacement, HouseLayoutPlacementId } from '@/domain/types';

import HouseLayoutEntity from './HouseLayoutEntity.vue';

interface DragPayload {
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
  members: FamilyMember[];
  pets: FamilyPet[];
  placements: HouseLayoutPlacement[];
  rooms: ReadonlyArray<HouseRoomDefinition>;
  score: number;
  selectedZoneId?: HouseZoneId | 'all';
  showRoomLabels?: boolean;
}>(), { compact: false, editable: false, includeGarden: false, selectedZoneId: 'all', showRoomLabels: true });
const emit = defineEmits<{
  move: [placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number];
}>();

const drag = ref<DragPayload | null>(null);
const ladiMotivation = ref('');
let motivationTimer: number | undefined;
const unlockedZones = computed<HouseZoneId[]>(() => [
  ...props.rooms.map((room) => room.id),
  ...(props.includeGarden ? ['garden' as const] : []),
]);
const displayedRooms = computed(() => props.selectedZoneId === 'all'
  ? props.rooms
  : props.rooms.filter(room => room.id === props.selectedZoneId));
const gardenIsVisible = computed(() => props.includeGarden && props.selectedZoneId === 'garden');
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
const dragOffset = (placementId: HouseLayoutPlacementId) => drag.value?.placementId === placementId
  ? { x: drag.value.offsetX, y: drag.value.offsetY }
  : undefined;
const startDrag = (event: PointerEvent, placement: HouseLayoutPlacement) => {
  if (!props.editable) return;
  event.preventDefault();
  event.stopPropagation();
  const target = event.currentTarget as HTMLElement;
  target.setPointerCapture(event.pointerId);
  drag.value = { placementId: placement.id, pointerId: event.pointerId, startX: event.clientX, startY: event.clientY, offsetX: 0, offsetY: 0 };
};
const trackDrag = (event: PointerEvent) => {
  if (!drag.value || event.pointerId !== drag.value.pointerId) return;
  drag.value.offsetX = event.clientX - drag.value.startX;
  drag.value.offsetY = event.clientY - drag.value.startY;
};
const finishDrag = (event: PointerEvent) => {
  if (!drag.value || event.pointerId !== drag.value.pointerId) return;
  const zone = document.elementsFromPoint(event.clientX, event.clientY)
    .map((element) => element.closest<HTMLElement>('[data-zone-id]'))
    .find((element) => element?.dataset.zoneId && unlockedZones.value.includes(element.dataset.zoneId as HouseZoneId));
  if (zone?.dataset.zoneId) {
    const bounds = zone.getBoundingClientRect();
    const x = ((event.clientX - bounds.left) / bounds.width) * 100;
    const y = ((event.clientY - bounds.top) / bounds.height) * 100;
    emit('move', drag.value.placementId, zone.dataset.zoneId as HouseZoneId, x, y);
  }
  drag.value = null;
};
const cancelDrag = () => { drag.value = null; };
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
onUnmounted(() => {
  if (motivationTimer !== undefined) window.clearTimeout(motivationTimer);
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
.dollhouse-layout.room-count-3 .dollhouse-room:first-child,
.dollhouse-layout.room-count-5 .dollhouse-room:first-child {
  grid-column: span 2;
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
.garden-zone {
  grid-column: span 2;
  min-height: 235px;
  border: 0;
  background: linear-gradient(#ccecf0 0 45%, #a8d28e 45% 100%);
  box-shadow: none;
}
.garden-zone::after {
  display: none;
}
.garden-sky {
  @apply position-absolute inset-0;
  background: linear-gradient(#ccecf0, #ecf8ed 52%, transparent 53%);
}
.garden-sky i {
  width: 43px;
  height: 43px;
  @apply position-absolute;
  top: 15px;
  right: 13%;
  border-radius: 50%;
  background: #f7d86e;
  box-shadow: 0 0 0 8px rgba(247, 216, 110, 0.2);
}
.garden-hedge {
  height: 34%;
  @apply position-absolute;
  left: 0;
  right: 0;
  bottom: 31%;
  background:
    radial-gradient(circle at 10px 0, #6fab68 12px, transparent 13px) 0 0/28px
      25px repeat-x,
    linear-gradient(#78b770, #679f61);
}
.garden-lawn {
  height: 33%;
  @apply position-absolute;
  left: 0;
  right: 0;
  bottom: 0;
  background:
    repeating-linear-gradient(
      165deg,
      transparent 0 34px,
      rgba(66, 124, 70, 0.1) 35px 37px
    ),
    #a8d28e;
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
.ladi-motivation {
  max-width: min(86%, 360px);
  @apply position-absolute d-flex align-center;
  left: 50%;
  bottom: 34px;
  z-index: 80;
  gap: 7px;
  padding: 9px 12px;
  transform: translateX(-50%);
  color: #38584e;
  border: 2px solid white;
  border-radius: 14px;
  background: #fff9df;
  box-shadow: 0 8px 18px rgba(57, 65, 50, 0.2);
  font-size: 10px;
  font-weight: 850;
}
.ladi-motivation span {
  font-size: 18px;
}
.motivation-enter-active,
.motivation-leave-active {
  transition:
    opacity 0.18s ease,
    transform 0.18s ease;
}
.motivation-enter-from,
.motivation-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(8px) scale(0.94);
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
</style>
