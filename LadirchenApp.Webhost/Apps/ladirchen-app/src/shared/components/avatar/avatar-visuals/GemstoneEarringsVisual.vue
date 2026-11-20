<template>
  <g class="gemstone-earrings" aria-hidden="true">
    <path d="M34 84v9m92-9v9" />
    <path class="gem" d="m29 94 5-5 5 5-5 9Zm92 0 5-5 5 5-5 9Z" />
    <circle cx="34" cy="86" r="3" />
    <circle cx="126" cy="86" r="3" />
  </g>
</template>

<style scoped>
.gemstone-earrings > path:first-child {
  fill: none;
  stroke: var(--lad-palette-amber-650);
  stroke-linecap: round;
  stroke-width: 2;
}
.gemstone-earrings .gem {
  fill: var(--lad-palette-blue-450);
  stroke: var(--lad-palette-indigo-750);
  stroke-linejoin: round;
  stroke-width: 1.5;
}
.gemstone-earrings circle {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1;
}
</style>
