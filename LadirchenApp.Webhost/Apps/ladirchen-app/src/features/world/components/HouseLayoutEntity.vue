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
import { useI18n } from 'vue-i18n';

import type { AvatarAppearance } from '@/domain/avatar';
import type { FamilyMember, FamilyMemberId, FamilyPet, HouseAccessory, HouseLayoutPlacement } from '@/domain/types';
import LadiMascot from '@/shared/components/LadiMascot.vue';
import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';

import AnimatedPet from './AnimatedPet.vue';
import RoomFurniture from './RoomFurniture.vue';

const { t } = useI18n();

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
  viewerMemberId?: FamilyMemberId;
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
  if (props.placement.entityType === 'furniture') {return props.accessory?.title ?? t('world.scene.entity.furniture');}
  if (props.placement.entityType === 'member') {
    if (!props.member) return t('world.scene.entity.member');
    const name = props.member.nickname?.trim() || props.member.name;
    return props.member.id === props.viewerMemberId ? t('world.scene.memberYou', { name }) : name;
  }
  if (props.placement.entityType === 'pet') {return props.pet?.name ?? t('world.scene.entity.pet');}
  return 'Ladi';
});
const accessibleLabel = computed(() => props.editable
  ? t('world.scene.moveEntity', { name: entityName.value })
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

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
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
  filter: drop-shadow(
    0 8px 8px color-mix(in srgb, var(--lad-palette-muted-750) 25%, transparent)
  );
}
.entity-label {
  @apply position-absolute text-no-wrap;
  left: 50%;
  bottom: -8px;
  padding: 2px 5px;
  transform: translateX(-50%);
  border: 1px solid color-mix(in srgb, var(--lad-palette-text) 12%, transparent);
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  color: var(--lad-palette-muted-700);
  font-size: 0.375rem;
  font-weight: var(--lad-font-weight-heavy);
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
  max-width: 180px;
  @apply position-absolute text-left;
  bottom: 68%;
  left: 66%;
  z-index: 500;
  padding: 10px 12px;
  color: var(--lad-palette-muted-700);
  border: 2px solid var(--lad-palette-white);
  border-radius: 16px 16px 16px 5px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface),
    var(--lad-palette-background)
  );
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--lad-palette-mint-strong) 12%, transparent),
    0 10px 19px
      color-mix(in srgb, var(--lad-palette-muted-750) 18%, transparent);
  font-size: 0.6875rem;
  font-weight: var(--lad-font-weight-strong);
  line-height: 1.4;
  pointer-events: none;
}
.ladi-speech::after {
  content: "";
  width: 10px;
  height: 10px;
  @apply position-absolute;
  bottom: -5px;
  left: 9px;
  transform: rotate(45deg);
  border-right: 2px solid var(--lad-palette-white);
  border-bottom: 2px solid var(--lad-palette-white);
  background: var(--lad-palette-background);
}
.ladi-speech.opens-left {
  right: 66%;
  left: auto;
  border-radius: 16px 16px 5px 16px;
}
.ladi-speech.opens-left::after {
  right: 9px;
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
@include reduced-motion {
  .layout-entity {
    transition: none;
  }
}
</style>
