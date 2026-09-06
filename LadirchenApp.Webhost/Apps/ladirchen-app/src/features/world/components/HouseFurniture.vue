<template>
  <g
    v-if="item.visual && item.scene"
    class="furniture-piece"
    :class="[`furniture-piece--${item.visual}`, { 'furniture-piece--animated': item.motion !== 'none' }]"
    :transform="`translate(${item.scene.x} ${item.scene.y}) scale(${item.scene.scale})`"
  >
    <component :is="furnitureVisualComponents[item.visual]" />
  </g>
</template>

<script lang="ts" setup>
import type { HouseAccessory } from '@/domain/types';
import { furnitureVisualComponents } from './furniture-visuals';

defineProps<{ item: HouseAccessory }>();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.furniture-piece {
  animation: furniture-arrive 480ms cubic-bezier(0.18, 0.89, 0.32, 1.28);
  filter: drop-shadow(
    0 4px 2px color-mix(in srgb, var(--lad-palette-muted-750) 15%, transparent)
  );
}
:deep(.book-spines) {
  fill: none;
  stroke: var(--lad-palette-yellow);
  stroke-linecap: round;
  stroke-width: 5;
}
:deep(.pumpkin-lights) {
  fill: var(--lad-palette-orange-400);
  stroke: var(--lad-palette-orange-650);
  stroke-width: 2;
}
:deep(.garland-bats) path {
  transform-box: fill-box;
  transform-origin: center;
  animation: garland-flutter 1.3s ease-in-out infinite alternate;
}
:deep(.garland-bats) path:nth-child(2) {
  animation-delay: -0.5s;
}
:deep(.garland-bats) path:nth-child(3) {
  animation-delay: -0.9s;
}
:deep(.candy-bush-cloud) {
  fill: var(--lad-palette-rose-200);
  stroke: var(--lad-palette-white);
  stroke-width: 3;
}
:deep(.candy-bush-cloud) circle:nth-child(2n) {
  fill: var(--lad-palette-blue-250);
}
:deep(.candy-fence-posts) {
  fill: none;
  stroke: var(--lad-palette-rose-250);
  stroke-dasharray: 8 5;
  stroke-linecap: round;
  stroke-width: 9;
}
:deep(.flag-cloth) {
  transform-box: fill-box;
  transform-origin: left center;
  animation: flag-wave 1.8s ease-in-out infinite alternate;
}
:deep(.flag-ribbon) {
  stroke-dasharray: 25 8;
  animation: flag-ribbon-flow 2.2s linear infinite;
}
:deep(.flag-shine) {
  opacity: 0.48;
  stroke-dasharray: 8 52;
  animation: flag-shimmer 2.6s ease-in-out infinite;
}
@keyframes furniture-arrive {
  from {
    opacity: 0;
    transform: scale(0.3) translateY(18px);
  }
  to {
    opacity: 1;
  }
}
@keyframes garland-flutter {
  from {
    transform: scaleY(0.75) translateY(0);
  }
  to {
    transform: scaleY(1.15) translateY(-3px);
  }
}
@keyframes flag-wave {
  from {
    transform: skewY(-2deg) scaleX(0.97);
  }
  to {
    transform: skewY(4deg) scaleX(1.03);
  }
}
@keyframes flag-ribbon-flow {
  to {
    stroke-dashoffset: -33;
  }
}
@keyframes flag-shimmer {
  0%,
  35% {
    stroke-dashoffset: 58;
    opacity: 0;
  }
  55% {
    opacity: 0.5;
  }
  80%,
  100% {
    stroke-dashoffset: -24;
    opacity: 0;
  }
}
@include reduced-motion {
  .furniture-piece,
  :deep(.flag-cloth),
  :deep(.flag-ribbon),
  :deep(.flag-shine) {
    animation: none;
  }
}
</style>
