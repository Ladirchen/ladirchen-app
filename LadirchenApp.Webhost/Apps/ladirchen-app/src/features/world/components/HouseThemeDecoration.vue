<template>
  <g v-if="themeId === 'halloween-night'" class="edition-decor halloween-edition" :aria-label="label">
    <path class="bat bat-one" d="M155 111q8-10 16 0 8-10 16 0-8-2-16 10-8-12-16-10Z" /><path class="bat bat-two" d="M317 105q6-8 13 0 6-8 13 0-7-1-13 9-6-10-13-9Z" />
    <circle class="pumpkin" cx="191" cy="255" r="11" /><circle class="pumpkin" cx="275" cy="256" r="10" /><path class="pumpkin-face" d="m186 252 3 3 3-3m-5 8q4 3 8 0m75-8 3 3 3-3m-5 8q4 3 8 0" />
    <path class="spooky-vine" d="M144 213q-20-22 0-43m174 44q20-24 2-45" />
  </g>
  <g v-else-if="themeId === 'cotton-candy-dream'" class="edition-decor candy-edition" :aria-label="label">
    <g class="candy-cloud candy-left"><circle cx="119" cy="238" r="17" /><circle cx="135" cy="229" r="20" /><circle cx="151" cy="240" r="16" /></g>
    <g class="candy-cloud candy-right"><circle cx="314" cy="238" r="17" /><circle cx="331" cy="228" r="20" /><circle cx="348" cy="240" r="16" /></g>
    <path class="candy-swirl" d="M217 122q25-23 38 0-8 17-27 6 4-10 14-6" /><path class="lollipop" d="M112 208v35m-10-38q10-17 20 0-10 17-20 0Z" /><path class="lollipop lollipop-blue" d="M352 206v38m-10-40q10-17 20 0-10 17-20 0Z" />
    <g class="wrapped-candies"><path d="m169 191-9-5v10Zm18 0 9-5v10Z" /><rect x="169" y="184" width="18" height="14" rx="7" /><path d="m273 178-8-5v10Zm17 0 8-5v10Z" /><rect x="273" y="171" width="17" height="14" rx="7" /></g>
    <g class="candy-sprinkles"><circle cx="177" cy="121" r="3" /><circle cx="289" cy="133" r="3" /><circle cx="309" cy="157" r="2.5" /><circle cx="154" cy="153" r="2.5" /></g>
  </g>
  <g v-else-if="themeId === 'starlight-palace'" class="edition-decor starlight-edition" :aria-label="label">
    <path class="palace-garland" d="M151 168q80 34 161 0" /><g class="palace-lights"><circle cx="165" cy="174" r="4" /><circle cx="190" cy="181" r="4" /><circle cx="217" cy="185" r="4" /><circle cx="245" cy="185" r="4" /><circle cx="272" cy="181" r="4" /><circle cx="298" cy="174" r="4" /></g>
    <path class="palace-crown" d="m213 129 9 8 10-18 10 18 9-8-3 24h-32Z" /><g class="palace-flags"><path d="M151 153v-35l19 7-19 9m162 19v-35l-19 7 19 9" /></g>
    <g class="palace-stars"><path d="m178 119 3 7 7 3-7 3-3 7-3-7-7-3 7-3Z" /><path d="m294 124 3 7 7 3-7 3-3 7-3-7-7-3 7-3Z" /><path d="m329 199 2 5 5 2-5 2-2 5-2-5-5-2 5-2Z" /></g>
  </g>
</template>

<script lang="ts" setup>
import type { HouseThemeId } from '@/domain/house';

defineProps<{ label: string; themeId: HouseThemeId }>();
</script>

<style scoped>
.edition-decor {
  pointer-events: none;
}
.bat {
  fill: var(--lad-palette-text);
  transform-box: fill-box;
  transform-origin: center;
  animation: bat-flutter 1.2s ease-in-out infinite alternate;
}
.bat-two {
  animation-delay: -0.55s;
}
.pumpkin {
  fill: var(--lad-palette-orange-400);
  stroke: var(--lad-palette-orange-650);
  stroke-width: 3;
}
.pumpkin-face {
  fill: none;
  stroke: var(--lad-palette-muted-750);
  stroke-linecap: round;
  stroke-width: 2;
}
.spooky-vine {
  fill: none;
  stroke: var(--lad-palette-muted-600-2);
  stroke-linecap: round;
  stroke-width: 5;
}
.candy-cloud {
  fill: var(--lad-palette-rose-200);
  stroke: var(--lad-palette-surface);
  stroke-width: 3;
  filter: drop-shadow(
    0 4px 2px color-mix(in srgb, var(--lad-palette-muted-500) 20%, transparent)
  );
}
.candy-right {
  fill: var(--lad-palette-blue-250);
}
.candy-swirl {
  fill: none;
  stroke: var(--lad-palette-white);
  stroke-linecap: round;
  stroke-width: 6;
}
.lollipop {
  fill: var(--lad-palette-rose-250);
  stroke: var(--lad-palette-white);
  stroke-width: 4;
}
.lollipop-blue {
  fill: var(--lad-palette-blue-250);
}
.wrapped-candies {
  fill: var(--lad-palette-amber-250);
  stroke: var(--lad-palette-white);
  stroke-linejoin: round;
  stroke-width: 2;
}
.wrapped-candies rect:last-of-type {
  fill: var(--lad-palette-blue-250);
}
.candy-sprinkles circle:nth-child(odd) {
  fill: var(--lad-palette-amber-250);
}
.candy-sprinkles circle:nth-child(even) {
  fill: var(--lad-palette-blue-250);
}
.palace-garland {
  fill: none;
  stroke: var(--lad-palette-amber-550);
  stroke-width: 3;
}
.palace-lights circle {
  fill: var(--lad-palette-amber-250);
  stroke: var(--lad-palette-amber-100);
  stroke-width: 2;
  filter: drop-shadow(0 0 4px var(--lad-palette-yellow));
  animation: palace-light 1.8s ease-in-out infinite;
}
.palace-lights circle:nth-child(even) {
  fill: var(--lad-palette-purple-200);
  animation-delay: -0.9s;
}
.palace-crown {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-150);
  stroke-linejoin: round;
  stroke-width: 3;
  filter: drop-shadow(
    0 3px 2px color-mix(in srgb, var(--lad-palette-amber-700) 25%, transparent)
  );
  animation: palace-crown-float 2.8s ease-in-out infinite;
  transform-box: fill-box;
  transform-origin: center;
}
.palace-flags {
  fill: var(--lad-palette-violet-500);
  stroke: var(--lad-palette-amber-550);
  stroke-linejoin: round;
  stroke-width: 2;
}
.palace-stars {
  fill: var(--lad-palette-amber-150);
  filter: drop-shadow(0 0 5px var(--lad-palette-amber-150));
}
.palace-stars path {
  transform-box: fill-box;
  transform-origin: center;
  animation: palace-star 2.1s ease-in-out infinite;
}
.palace-stars path:nth-child(2) {
  animation-delay: -0.7s;
}
.palace-stars path:nth-child(3) {
  animation-delay: -1.4s;
}
@keyframes bat-flutter {
  from {
    transform: translateY(0) scaleY(0.82);
  }
  to {
    transform: translateY(-4px) scaleY(1.12);
  }
}
@keyframes palace-light {
  0%,
  100% {
    opacity: 0.68;
    transform: scale(0.86);
  }
  50% {
    opacity: 1;
    transform: scale(1.14);
  }
}
@keyframes palace-crown-float {
  0%,
  100% {
    transform: translateY(1px) rotate(-2deg);
  }
  50% {
    transform: translateY(-4px) rotate(2deg);
  }
}
@keyframes palace-star {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.7) rotate(-8deg);
  }
  50% {
    opacity: 1;
    transform: scale(1.18) rotate(14deg);
  }
}
@media (prefers-reduced-motion: reduce) {
  .bat,
  .palace-lights circle,
  .palace-crown,
  .palace-stars path {
    animation: none;
  }
}
</style>
