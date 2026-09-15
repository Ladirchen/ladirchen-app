<template>
  <g class="flower-crown">
    <path class="flower-band" d="M40 39q40-25 80 0" />
    <g transform="translate(50 25)"><circle cx="0" cy="-6" r="5" /><circle cx="6" cy="0" r="5" /><circle cx="0" cy="6" r="5" /><circle cx="-6" cy="0" r="5" /><circle class="flower-center" r="4" /></g>
    <g transform="translate(80 17)"><circle cx="0" cy="-7" r="6" /><circle cx="7" cy="0" r="6" /><circle cx="0" cy="7" r="6" /><circle cx="-7" cy="0" r="6" /><circle class="flower-center" r="4" /></g>
    <g transform="translate(110 25)"><circle cx="0" cy="-6" r="5" /><circle cx="6" cy="0" r="5" /><circle cx="0" cy="6" r="5" /><circle cx="-6" cy="0" r="5" /><circle class="flower-center" r="4" /></g>
    <path class="flower-leaf" d="M60 28q8-13 15-4-7 9-15 4Zm40 0q-8-13-15-4 7 9 15 4Z" />
  </g>
</template>

<style scoped>
.flower-band {
  fill: none;
  stroke: var(--lad-palette-teal-550);
  stroke-linecap: round;
  stroke-width: 5;
}
.flower-crown circle {
  fill: var(--lad-palette-rose-300);
  stroke: var(--lad-palette-red-600);
  stroke-width: 1;
}
.flower-crown g:nth-of-type(2) circle {
  fill: var(--lad-palette-violet-400);
}
.flower-crown .flower-center {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
}
.flower-leaf {
  fill: var(--lad-palette-mint-450);
  stroke: var(--lad-palette-teal-700);
  stroke-width: 1.5;
}
</style>
