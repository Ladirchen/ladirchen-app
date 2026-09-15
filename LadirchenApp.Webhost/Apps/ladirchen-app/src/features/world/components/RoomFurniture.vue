<template>
  <span class="room-furniture" aria-hidden="true">
    <img v-if="spriteUrl" :src="spriteUrl" alt="" draggable="false">
  </span>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

import { FURNITURE_SPRITE_DEFINITIONS } from '../furniture-sprite-assets';
import type { HouseAccessory } from '@/domain/house';

const props = withDefaults(defineProps<{ item: HouseAccessory; open?: boolean }>(), { open: false });
const spriteUrl = computed(() => {
  if (!props.item.visual) return undefined;
  const definition = FURNITURE_SPRITE_DEFINITIONS[props.item.visual];
  return props.open ? definition.openUrl ?? definition.closedUrl : definition.closedUrl;
});
</script>

<style scoped>
.room-furniture {
  @apply d-grid w-100 h-100 place-center overflow-visible;
}
.room-furniture img {
  @apply d-block w-100 h-100;
  @apply overflow-visible;
  object-fit: contain;
  filter: drop-shadow(
    0 0.25rem 2px
      color-mix(in srgb, var(--lad-palette-muted-750) 18%, transparent)
  );
  pointer-events: none;
  user-select: none;
}
</style>
