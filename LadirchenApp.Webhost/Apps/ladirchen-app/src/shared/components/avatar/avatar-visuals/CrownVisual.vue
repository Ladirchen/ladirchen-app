<template>
  <g class="crown">
    <path class="crown-body" d="m49 30 6-25 18 16L82 0l12 20 17-17 2 28Z" />
    <circle class="crown-tip" cx="55" cy="5" r="4" /><circle class="crown-tip" cx="82" cy="1" r="4" /><circle class="crown-tip" cx="111" cy="3" r="4" />
    <path class="crown-gems" d="m65 24 5-5 5 5-5 5Zm24 0 5-5 5 5-5 5Z" /><path class="crown-shine" d="m83 7 2 4 4 2-4 2-2 4-2-4-4-2 4-2Z" />
  </g>
</template>

<style scoped>
.crown-body {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-linejoin: round;
  stroke-width: 3;
}
.crown-tip {
  fill: var(--lad-palette-blue-250);
  stroke: var(--lad-palette-blue-600);
  stroke-width: 1.5;
}
.crown-gems {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-red-600);
  stroke-width: 1;
}
.crown-shine {
  fill: var(--lad-palette-amber-150);
  stroke: none;
}
</style>
