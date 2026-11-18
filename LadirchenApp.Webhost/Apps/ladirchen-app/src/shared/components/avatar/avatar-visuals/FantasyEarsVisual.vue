<template>
  <g class="fantasy-ears" aria-hidden="true">
    <path d="M42 70 14 57q2 24 29 31m74-18 29-13q-3 24-29 31" />
    <path class="inner" d="m35 72-14-9q5 11 18 16m86-7 14-9q-5 11-18 16" />
    <circle cx="24" cy="68" r="3" />
    <circle cx="136" cy="68" r="3" />
  </g>
</template>

<style scoped>
.fantasy-ears > path:first-child {
  fill: var(--skin);
  stroke: var(--lad-palette-orange-600);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.fantasy-ears .inner {
  fill: none;
  stroke: var(--lad-palette-red-300);
  stroke-linecap: round;
  stroke-width: 2;
}
.fantasy-ears circle {
  fill: var(--lad-palette-blue-450);
  stroke: var(--lad-palette-indigo-750);
  stroke-width: 1;
}
</style>
