<template>
  <svg class="room-furniture" viewBox="0 0 120 120" aria-hidden="true">
    <HouseFurniture :item="renderItem" />
  </svg>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

import HouseFurniture from './HouseFurniture.vue';
import type { HouseAccessory } from '@/domain/types';

const props = defineProps<{ item: HouseAccessory }>();
const renderItem = computed<HouseAccessory>(() => ({
  ...props.item,
  scene: { x: 60, y: 64, scale: .9 },
}));
</script>

<style scoped>
.room-furniture {
  @apply d-block w-100 h-100 overflow-visible;
}
</style>
