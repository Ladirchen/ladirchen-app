<template>
  <span class="energy-star" :style="starStyle" aria-hidden="true">
    <svg viewBox="0 0 64 64">
      <circle class="star-halo" cx="32" cy="32" r="25" />
      <path class="star-shape" d="M32 8c2.8 12.6 8.8 18.6 21 21-12.2 2.4-18.2 8.4-21 21-2.8-12.6-8.8-18.6-21-21 12.2-2.4 18.2-8.4 21-21Z" />
      <circle class="star-dot star-dot--left" cx="10" cy="17" r="3" />
      <path class="star-spark" d="M52 10c.8 3.7 2.6 5.5 6 6-3.4.5-5.2 2.3-6 6-.8-3.7-2.6-5.5-6-6 3.4-.5 5.2-2.3 6-6Z" />
    </svg>
  </span>
</template>

<script lang="ts" setup>
import { computed } from "vue";

const props = withDefaults(defineProps<{ size?: number }>(), { size: 32 });
const starStyle = computed(() => ({ width: `${props.size}px`, height: `${props.size}px` }));
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.energy-star {
  --uno: d-inline-grid place-center flex-shrink-0;
}
svg {
  --uno: d-block w-100 h-100 overflow-visible;
}
.star-halo {
  fill: var(--lad-palette-amber-150);
  opacity: 0.72;
  transform-box: fill-box;
  transform-origin: center;
  animation: star-halo-breathe 2.8s ease-in-out infinite;
}
.star-shape {
  fill: var(--lad-palette-amber-450);
  stroke: var(--lad-palette-amber-600);
  stroke-linejoin: round;
  stroke-width: 2.5;
  filter: drop-shadow(
    0 3px 2px color-mix(in srgb, var(--lad-palette-amber-650) 20%, transparent)
  );
  transform-box: fill-box;
  transform-origin: center;
  animation: star-celebrate 2.8s cubic-bezier(0.34, 1.4, 0.64, 1) infinite;
}
.star-dot,
.star-spark {
  fill: var(--lad-palette-mint);
  transform-box: fill-box;
  transform-origin: center;
  animation: star-twinkle 2.8s ease-in-out infinite;
}
.star-spark {
  animation-delay: -1.4s;
}
@keyframes star-celebrate {
  0%,
  28%,
  100% {
    transform: rotate(-5deg) scale(0.96);
  }
  43% {
    transform: rotate(7deg) scale(1.12);
  }
  58% {
    transform: rotate(-2deg) scale(1.02);
  }
}
@keyframes star-halo-breathe {
  0%,
  100% {
    opacity: 0.42;
    transform: scale(0.86);
  }
  48% {
    opacity: 0.9;
    transform: scale(1.08);
  }
}
@keyframes star-twinkle {
  0%,
  25%,
  70%,
  100% {
    opacity: 0.2;
    transform: scale(0.55);
  }
  45%,
  58% {
    opacity: 1;
    transform: scale(1.15);
  }
}
@include reduced-motion {
  .star-halo,
  .star-shape,
  .star-dot,
  .star-spark {
    animation: none;
  }
}
</style>
