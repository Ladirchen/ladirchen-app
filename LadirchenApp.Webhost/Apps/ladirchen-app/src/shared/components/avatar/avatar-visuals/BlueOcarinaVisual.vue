<template>
  <g class="blue-ocarina" aria-hidden="true">
    <path class="cord" d="M57 118q23 25 46 0" />
    <path class="instrument" d="M62 137q18-14 39-2l13-7-5 14q-2 14-25 14-20 0-25-10Z" />
    <circle cx="79" cy="139" r="2.5" />
    <circle cx="88" cy="136" r="2.5" />
    <circle cx="95" cy="143" r="2.5" />
    <path class="shine" d="M67 143q7-8 16-8" />
  </g>
</template>

<style scoped>
.blue-ocarina .cord {
  fill: none;
  stroke: var(--lad-palette-amber-650);
  stroke-linecap: round;
  stroke-width: 2;
}
.blue-ocarina .instrument {
  fill: var(--lad-palette-blue-450);
  stroke: var(--lad-palette-indigo-750);
  stroke-linejoin: round;
  stroke-width: 2;
}
.blue-ocarina circle {
  fill: var(--lad-palette-indigo-750);
}
.blue-ocarina .shine {
  fill: none;
  stroke: var(--lad-palette-blue-150);
  stroke-linecap: round;
  stroke-width: 2;
}
</style>
