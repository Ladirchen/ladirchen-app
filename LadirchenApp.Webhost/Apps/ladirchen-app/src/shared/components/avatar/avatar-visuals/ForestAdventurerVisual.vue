<template>
  <g v-if="layer === 'back'" class="forest-adventurer back" aria-hidden="true">
    <path d="M50 127 30 141l-5 35 22-10m63-39 20 14 5 35-22-10" />
    <path class="cape" d="m57 124-24 54 47-15 47 15-24-54-23 12Z" />
  </g>
  <g v-else-if="layer === 'front'" class="forest-adventurer front" aria-hidden="true">
    <path d="M44 138q9-13 25-16l11 14 11-14q16 4 25 16l2 42H42Z" />
    <path class="vest" d="m63 125 17 11 17-11 8 55H55Z" />
    <path class="belt" d="M51 162h58" />
    <path class="buckle" d="M75 157h10v10H75Z" />
    <path class="leaf" d="M92 144q12-10 16 2-10 9-16-2Z" />
  </g>
</template>

<script lang="ts" setup>
export interface Props {
  layer: "back" | "front" | "head";
}
defineProps<Props>();
</script>

<style scoped>
.forest-adventurer.back {
  fill: var(--lad-palette-green-500);
  stroke: var(--lad-palette-teal-700);
  stroke-linejoin: round;
  stroke-width: 3;
}
.forest-adventurer .cape {
  fill: var(--lad-palette-teal-700);
}
.forest-adventurer.front > path:first-child {
  fill: color-mix(in srgb, var(--outfit) 55%, var(--lad-palette-green-500));
  stroke: var(--lad-palette-muted-700);
}
.forest-adventurer .vest {
  fill: var(--lad-palette-amber-450);
  stroke: var(--lad-palette-amber-700);
}
.forest-adventurer .belt {
  fill: none;
  stroke: var(--lad-palette-orange-750);
  stroke-width: 6;
}
.forest-adventurer .buckle {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-700);
  stroke-width: 1.5;
}
.forest-adventurer .leaf {
  fill: var(--lad-palette-green-500);
  stroke: var(--lad-palette-teal-700);
  stroke-width: 1.5;
}
</style>
