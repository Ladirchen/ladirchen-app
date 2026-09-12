<template>
  <span class="animated-house-energy" :style="iconStyle" aria-hidden="true">
    <svg viewBox="0 0 108 96">
      <circle class="soft-glow" cx="54" cy="47" r="37" />
      <ellipse class="house-shadow" cx="54" cy="86" rx="35" ry="6" />

      <g class="house-illustration">
        <path class="house-body" d="M25 43 54 20l29 23v39H25Z" />
        <path class="roof" d="m18 45 36-30 36 30-7 8-29-24-29 24Z" />
        <path class="roof-highlight" d="M29 40 54 20l25 20" />
        <rect class="door" x="47" y="56" width="15" height="26" rx="6" />
        <circle class="door-knob" cx="58" cy="69" r="1.7" />
        <g class="window window--left">
          <rect x="31" y="53" width="12" height="12" rx="3" />
          <path d="M37 53v12M31 59h12" />
        </g>
        <g class="window window--right">
          <rect x="66" y="53" width="12" height="12" rx="3" />
          <path d="M72 53v12M66 59h12" />
        </g>
        <path class="heart" d="M54 48c-5-5-12 2 0 10 12-8 5-15 0-10Z" />
      </g>

      <g class="energy-sparks">
        <path class="spark spark--one" d="m19 24 2 5 5 2-5 2-2 5-2-5-5-2 5-2Z" />
        <path class="spark spark--two" d="m88 19 1.5 3.5L93 24l-3.5 1.5L88 29l-1.5-3.5L83 24l3.5-1.5Z" />
        <circle class="spark-dot spark-dot--one" cx="92" cy="52" r="2.3" />
        <circle class="spark-dot spark-dot--two" cx="17" cy="59" r="1.8" />
      </g>
    </svg>
  </span>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

const props = withDefaults(defineProps<{ size?: number }>(), { size: 82 });
const iconStyle = computed(() => ({ width: `${props.size}px`, height: `${Math.round(props.size * .89)}px` }));
</script>

<style scoped>
.animated-house-energy {
  display: inline-grid;
  flex: 0 0 auto;
  place-items: center;
}
svg {
  width: 100%;
  height: 100%;
  display: block;
  overflow: visible;
}
.soft-glow {
  fill: #ffe8a0;
  opacity: 0.48;
  filter: blur(7px);
  transform-box: fill-box;
  transform-origin: center;
  animation: glow-breathe 4.2s ease-in-out infinite;
}
.house-shadow {
  fill: rgba(78, 68, 48, 0.14);
  transform-box: fill-box;
  transform-origin: center;
  animation: shadow-breathe 4.2s ease-in-out infinite;
}
.house-illustration {
  filter: drop-shadow(0 3px 2px rgba(91, 64, 39, 0.18));
}
.house-body {
  fill: #fff8dc;
  stroke: #9c6a4b;
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.roof {
  fill: #dc6d68;
  stroke: #a84e4d;
  stroke-linejoin: round;
  stroke-width: 3;
}
.roof-highlight {
  fill: none;
  stroke: #f08b80;
  stroke-linecap: round;
  stroke-width: 2.2;
}
.door {
  fill: #65a27f;
  stroke: #39715a;
  stroke-width: 2.3;
}
.door-knob {
  fill: #ffd165;
  stroke: #9a6828;
  stroke-width: 1;
}
.window rect {
  fill: #82cfdd;
  stroke: #f9fcf4;
  stroke-width: 2;
  animation: window-light 4.2s ease-in-out infinite;
}
.window path {
  fill: none;
  stroke: #eefcfc;
  stroke-width: 1.5;
}
.window--right rect {
  animation-delay: -0.35s;
}
.heart {
  fill: #ffd257;
  stroke: #d48b28;
  stroke-width: 1.4;
  transform-box: fill-box;
  transform-origin: center;
  animation: heart-light 4.2s ease-in-out infinite;
}
.spark {
  fill: #f2a73a;
  transform-box: fill-box;
  transform-origin: center;
  animation: sparkle 4.2s ease-in-out infinite;
}
.spark--two {
  animation-delay: -2.1s;
}
.spark-dot {
  fill: #54bf94;
  transform-box: fill-box;
  transform-origin: center;
  animation: dot-rise 4.2s ease-in-out infinite;
}
.spark-dot--two {
  animation-delay: -1.4s;
}
@keyframes glow-breathe {
  0%,
  22%,
  100% {
    opacity: 0.46;
    transform: scale(0.94);
  }
  48%,
  68% {
    opacity: 0.92;
    transform: scale(1.05);
  }
}
@keyframes shadow-breathe {
  0%,
  100% {
    opacity: 0.75;
    transform: scaleX(1);
  }
  50% {
    opacity: 0.48;
    transform: scaleX(0.9);
  }
}
@keyframes window-light {
  0%,
  28%,
  100% {
    fill: #82cfdd;
  }
  48%,
  66% {
    fill: #c9f2dc;
    filter: drop-shadow(0 0 5px #ffe99e);
  }
}
@keyframes heart-light {
  0%,
  34%,
  58%,
  100% {
    transform: scale(1);
    filter: drop-shadow(0 0 0 rgba(255, 204, 72, 0));
  }
  43% {
    transform: scale(1.24);
    filter: drop-shadow(0 0 5px rgba(255, 204, 72, 0.9));
  }
  50% {
    transform: scale(1.06);
  }
}
@keyframes sparkle {
  0%,
  24%,
  70%,
  100% {
    opacity: 0;
    transform: scale(0.45) rotate(0);
  }
  43%,
  55% {
    opacity: 1;
    transform: scale(1.08) rotate(22deg);
  }
}
@keyframes dot-rise {
  0%,
  25%,
  100% {
    opacity: 0;
    transform: translateY(4px) scale(0.7);
  }
  48% {
    opacity: 0.9;
    transform: translateY(-2px) scale(1);
  }
  72% {
    opacity: 0;
    transform: translateY(-9px) scale(0.65);
  }
}
@media (prefers-reduced-motion: reduce) {
  .soft-glow,
  .house-shadow,
  .window rect,
  .heart,
  .spark,
  .spark-dot {
    animation: none;
  }
}
</style>
