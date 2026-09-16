<template>
  <button
    :aria-label="accessibleLabel"
    class="layout-entity"
    :class="[`entity-${placement.entityType}`, { editable, dragging: Boolean(dragOffset) }]"
    :style="entityStyle"
    type="button"
    @click="interactWithLadi"
    @lostpointercapture="forwardLostPointerCapture"
    @pointercancel="forwardPointerCancel"
    @pointerdown="forwardPointerDown"
    @pointermove="forwardPointerMove"
    @pointerup="forwardPointerUp"
  >
    <RoomFurniture v-if="placement.entityType === 'furniture' && accessory" :item="accessory" />
    <AvatarFigure
      v-else-if="placement.entityType === 'member' && member"
      :appearance="member.resolvedAppearance"
      :calm="true"
      full-body
      :size="62"
    />
    <AnimatedPet
      v-else-if="placement.entityType === 'pet' && pet"
      :pet="pet"
      :size="54"
    />
    <span v-else-if="placement.entityType === 'ladi'" class="ladi-on-perch" :class="{ perched }">
      <LadiMascot
        :score="score"
        :show-scene-base="!perched"
        :show-score="false"
        :size="74"
      />
      <Transition name="ladi-speech">
        <span v-if="speech" class="ladi-speech" :class="{ 'opens-left': placement.x > 70 }" role="status">{{ speech }}</span>
      </Transition>
    </span>
  </button>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

import type { AvatarAppearance } from '@/domain/avatar';
import type { FamilyMember, FamilyPet, HouseAccessory, HouseLayoutPlacement } from '@/domain/types';
import LadiMascot from '@/shared/components/LadiMascot.vue';
import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';

import AnimatedPet from './AnimatedPet.vue';
import RoomFurniture from './RoomFurniture.vue';

const props = defineProps<{
  accessory?: HouseAccessory;
  dragOffset?: { readonly x: number; readonly y: number };
  editable: boolean;
  member?: FamilyMember & { resolvedAppearance: AvatarAppearance };
  perched?: boolean;
  pet?: FamilyPet;
  placement: HouseLayoutPlacement;
  score: number;
  speech?: string;
}>();
const emit = defineEmits<{
  'ladi-interact': [];
  lostpointercapture: [event: PointerEvent];
  pointercancel: [event: PointerEvent];
  pointerdown: [event: PointerEvent, placement: HouseLayoutPlacement];
  pointermove: [event: PointerEvent];
  pointerup: [event: PointerEvent];
}>();

const entityName = computed(() => {
  if (props.placement.entityType === 'furniture') {return props.accessory?.title ?? 'Möbelstück';}
  if (props.placement.entityType === 'member') {return props.member?.name ?? 'Familienmitglied';}
  if (props.placement.entityType === 'pet') {return props.pet?.name ?? 'Haustier';}
  return 'Ladi';
});
const accessibleLabel = computed(() => props.editable
  ? `${entityName.value} verschieben`
  : entityName.value);
const displayY = computed(() => props.accessory?.visual === 'string-lights' ? Math.max(58, props.placement.y) : props.placement.y);
const entityStyle = computed(() => ({
  left: `${props.placement.x}%`,
  top: `${displayY.value}%`,
  transform: `translate(-50%, -70%) translate(${props.dragOffset?.x ?? 0}px, ${props.dragOffset?.y ?? 0}px) scale(${props.placement.scale})`,
  zIndex: props.dragOffset ? 1000 : (props.placement.entityType === 'furniture' ? 10 : 300) + Math.round(displayY.value),
}));
const interactWithLadi = () => {
  if (props.placement.entityType === 'ladi') {
    emit('ladi-interact');
  }
};
const forwardPointerDown = (event: PointerEvent) => {
  emit('pointerdown', event, props.placement);
};
const forwardLostPointerCapture = (event: PointerEvent) => emit('lostpointercapture', event);
const forwardPointerMove = (event: PointerEvent) => emit('pointermove', event);
const forwardPointerUp = (event: PointerEvent) => emit('pointerup', event);
const forwardPointerCancel = (event: PointerEvent) => emit('pointercancel', event);
</script>

<style scoped>
.layout-entity {
  width: 72px;
  height: 72px;
  @apply position-absolute pa-0 d-grid place-center;
  transform-origin: center bottom;
  border: 0;
  background: transparent;
  transition: filter 0.16s ease;
  touch-action: none;
  user-select: none;
}
.layout-entity.entity-furniture {
  width: 86px;
  height: 86px;
  pointer-events: none;
}
.layout-entity.entity-member {
  width: 56px;
  height: 76px;
}
.layout-entity.entity-pet {
  width: 58px;
  height: 58px;
}
.layout-entity.entity-ladi {
  width: 78px;
  height: 78px;
}
.layout-entity.editable {
  cursor: grab;
  pointer-events: auto;
}
.layout-entity.editable:active {
  cursor: grabbing;
  filter: drop-shadow(0 8px 8px rgba(51, 45, 39, 0.26));
}
.entity-label {
  @apply position-absolute text-no-wrap;
  left: 50%;
  bottom: -8px;
  padding: 2px 5px;
  transform: translateX(-50%);
  border: 1px solid rgba(50, 63, 58, 0.13);
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.9);
  color: #42554f;
  font-size: 6px;
  font-weight: 900;
  pointer-events: none;
}
.layout-entity :deep(.avatar-figure),
.layout-entity :deep(.animated-pet),
.layout-entity :deep(.ladi-mascot) {
  @apply d-block;
  pointer-events: none;
}
.ladi-on-perch {
  @apply d-block;
  transform-origin: center bottom;
}
.ladi-on-perch.perched {
  animation: ladi-perch-hello 5.4s ease-in-out infinite;
}
.ladi-speech {
  width: max-content;
  max-width: 148px;
  @apply position-absolute text-left;
  bottom: 65%;
  left: 66%;
  z-index: 500;
  padding: 7px 9px;
  color: #36584e;
  border: 2px solid #fff;
  border-radius: 13px 13px 13px 4px;
  background: #fff8dc;
  box-shadow: 0 7px 15px rgba(76, 59, 44, 0.2);
  font-size: 9px;
  font-weight: 850;
  line-height: 1.25;
  pointer-events: none;
}
.ladi-speech::after {
  content: "";
  width: 10px;
  height: 10px;
  @apply position-absolute;
  bottom: -5px;
  left: 7px;
  transform: rotate(45deg);
  border-right: 2px solid #fff;
  border-bottom: 2px solid #fff;
  background: #fff8dc;
}
.ladi-speech.opens-left {
  right: 66%;
  left: auto;
  border-radius: 13px 13px 4px 13px;
}
.ladi-speech.opens-left::after {
  right: 7px;
  left: auto;
}
.ladi-speech-enter-active,
.ladi-speech-leave-active {
  transition:
    opacity 0.18s ease,
    transform 0.18s ease;
}
.ladi-speech-enter-from,
.ladi-speech-leave-to {
  opacity: 0;
  transform: translateY(6px) scale(0.9);
}
@keyframes ladi-perch-hello {
  0%,
  68%,
  100% {
    transform: translateY(0) rotate(0);
  }
  73% {
    transform: translateY(-5px) rotate(-4deg);
  }
  79% {
    transform: translateY(0) rotate(4deg);
  }
  85% {
    transform: translateY(-2px) rotate(0);
  }
}
@media (prefers-reduced-motion: reduce) {
  .layout-entity {
    transition: none;
  }
}
</style>
