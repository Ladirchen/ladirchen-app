<template>
  <span class="room-furniture" :class="`room-furniture--${item.visual}`" aria-hidden="true">
    <img v-if="spriteUrl" :src="spriteUrl" alt="" draggable="false">
  </span>
</template>

<script lang="ts" setup>
import { computed } from "vue";

import { FURNITURE_SPRITE_DEFINITIONS } from "@/shared/visuals/house/furniture-sprite-assets";
import type { HouseAccessory } from "@/domain/house";

const props = withDefaults(defineProps<{ item: HouseAccessory; open?: boolean }>(), { open: false });
const spriteUrl = computed(() => {
  if (!props.item.visual) return undefined;
  const definition = FURNITURE_SPRITE_DEFINITIONS[props.item.visual];
  return props.open ? definition.openUrl ?? definition.closedUrl : definition.closedUrl;
});
</script>

<style scoped>
.room-furniture {
  --uno: d-grid w-100 h-100 place-center overflow-visible;
}
.room-furniture img {
  --uno: d-block w-100 h-100 overflow-visible;

  object-fit: contain;
  filter: drop-shadow(
    0 0.25rem 2px
      color-mix(in srgb, var(--lad-palette-muted-750) 18%, transparent)
  );
  pointer-events: none;
  user-select: none;
}
.room-furniture--kitchen-counter img,
.room-furniture--retro-fridge img {
  object-position: center bottom;
}
</style>
