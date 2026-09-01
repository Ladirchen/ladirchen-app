<template>
  <span class="animated-streak-flame" :style="flameStyle" aria-hidden="true">
    <svg viewBox="0 0 100 112">
      <ellipse class="flame-shadow" cx="50" cy="101" rx="30" ry="7" />
      <path class="flame-body" d="M51 96C27 96 15 80 18 61c2-14 12-23 21-33 8-9 11-18 11-27 17 11 27 25 25 41 7-5 10-11 11-17 10 13 10 30 4 44-7 17-20 27-39 27Z" />
      <path class="flame-heart" d="M51 88c-13 0-21-9-19-21 1-8 8-13 13-19 5-5 7-10 7-16 11 8 17 18 14 29 4-2 7-6 8-10 5 9 4 20-1 27-5 7-12 10-22 10Z" />
      <g class="flame-face">
        <ellipse cx="42" cy="67" rx="3.5" ry="5" />
        <ellipse cx="61" cy="67" rx="3.5" ry="5" />
        <circle cx="43" cy="65" r="1.2" />
        <circle cx="62" cy="65" r="1.2" />
        <path d="M43 76q8 8 16 0" />
      </g>
      <g class="flame-sparks">
        <path d="m16 30 2 5 5 2-5 2-2 5-2-5-5-2 5-2Z" />
        <path d="m84 13 2 4 4 2-4 2-2 4-2-4-4-2 4-2Z" />
      </g>
    </svg>
  </span>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

const props = withDefaults(defineProps<{ size?: number }>(), { size: 25 });
const flameStyle = computed(() => ({
  width: `${props.size}px`,
  height: `${Math.round(props.size * 1.12)}px`,
}));
</script>

<style scoped>
.animated-streak-flame {
  display: inline-grid;
  flex: 0 0 auto;
  place-items: center;
  transform-origin: center bottom;
  animation: flame-bob 3.4s ease-in-out infinite;
}
svg {
  width: 100%;
  height: 100%;
  overflow: visible;
  filter: drop-shadow(0 3px 2px rgba(142, 83, 20, 0.2));
}
.flame-shadow {
  fill: rgba(94, 67, 39, 0.15);
  transform-box: fill-box;
  transform-origin: center;
  animation: shadow-pulse 3.4s ease-in-out infinite;
}
.flame-body {
  fill: #f3a52e;
  stroke: #a96018;
  stroke-linejoin: round;
  stroke-width: 4;
}
.flame-heart {
  fill: #ffd85d;
  transform-box: fill-box;
  transform-origin: center bottom;
  animation: heart-flicker 2.1s ease-in-out infinite;
}
.flame-face ellipse {
  fill: #704218;
  transform-box: fill-box;
  transform-origin: center;
  animation: flame-blink 7s ease-in-out infinite;
}
.flame-face circle {
  fill: white;
}
.flame-face path {
  fill: none;
  stroke: #704218;
  stroke-linecap: round;
  stroke-width: 3;
}
.flame-sparks {
  fill: #ffd85d;
}
.flame-sparks path {
  transform-box: fill-box;
  transform-origin: center;
  animation: spark-pop 2.4s ease-in-out infinite;
}
.flame-sparks path:last-child {
  animation-delay: -1.2s;
}
@keyframes flame-bob {
  0%,
  100% {
    transform: translateY(1px) rotate(-2deg);
  }
  50% {
    transform: translateY(-2px) rotate(2deg);
  }
}
@keyframes shadow-pulse {
  0%,
  100% {
    transform: scaleX(1);
  }
  50% {
    transform: scaleX(0.8);
  }
}
@keyframes heart-flicker {
  0%,
  100% {
    transform: scale(0.95) rotate(-1deg);
  }
  50% {
    transform: scale(1.05) rotate(2deg);
  }
}
@keyframes flame-blink {
  0%,
  45%,
  49%,
  100% {
    transform: scaleY(1);
  }
  47%,
  48% {
    transform: scaleY(0.1);
  }
}
@keyframes spark-pop {
  0%,
  100% {
    opacity: 0.25;
    transform: scale(0.7) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.1) rotate(20deg);
  }
}
@media (prefers-reduced-motion: reduce) {
  .animated-streak-flame,
  .flame-shadow,
  .flame-heart,
  .flame-face ellipse,
  .flame-sparks path {
    animation: none;
  }
}
</style>
