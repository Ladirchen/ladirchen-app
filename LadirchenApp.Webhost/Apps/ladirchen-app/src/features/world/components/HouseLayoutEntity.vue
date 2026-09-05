<template>
  <button
    :aria-label="accessibleLabel"
    class="layout-entity"
    :class="[`entity-${placement.entityType}`, { editable }]"
    :style="entityStyle"
    type="button"
    @click="interactWithLadi"
    @pointerdown="forwardPointerDown"
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
    <LadiMascot
      v-else-if="placement.entityType === 'ladi'"
      :score="score"
      :show-score="false"
      :size="74"
    />
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
  pet?: FamilyPet;
  placement: HouseLayoutPlacement;
  score: number;
}>();
const emit = defineEmits<{
  'ladi-interact': [];
  pointerdown: [event: PointerEvent, placement: HouseLayoutPlacement];
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
const entityStyle = computed(() => ({
  left: `${props.placement.x}%`,
  top: `${props.placement.y}%`,
  transform: `translate(-50%, -70%) translate(${props.dragOffset?.x ?? 0}px, ${props.dragOffset?.y ?? 0}px) scale(${props.placement.scale})`,
  zIndex: Math.round(props.placement.y) + 5,
}));
const interactWithLadi = () => {
  if (props.placement.entityType === 'ladi') {
    emit('ladi-interact');
  }
};
const forwardPointerDown = (event: PointerEvent) => {
  emit('pointerdown', event, props.placement);
};
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
@media (prefers-reduced-motion: reduce) {
  .layout-entity {
    transition: none;
  }
}
</style>
