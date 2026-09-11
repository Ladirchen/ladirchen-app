<template>
  <g class="cat-ears">
    <path class="cat-ear-outer" d="M39 36 44 7l27 23Z" /><path class="cat-ear-outer" d="m89 30 27-23 5 29Z" />
    <path class="cat-ear-inner" d="m47 27 2-12 12 13Zm66 0-2-12-12 13Z" />
    <path class="cat-ear-band" d="M44 36q36-24 72 0" />
    <circle class="cat-ear-dot" cx="45" cy="10" r="3" /><circle class="cat-ear-dot" cx="115" cy="10" r="3" />
  </g>
</template>

<style scoped>
.cat-ear-outer {
  fill: var(--lad-palette-rose-250);
  stroke: var(--lad-palette-orange-650);
  stroke-linejoin: round;
  stroke-width: 3.5;
}
.cat-ear-inner {
  fill: var(--lad-palette-red-100);
  stroke: none;
}
.cat-ear-band {
  fill: none;
  stroke: var(--lad-palette-violet-500);
  stroke-linecap: round;
  stroke-width: 5;
}
.cat-ear-dot {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1;
}
</style>
