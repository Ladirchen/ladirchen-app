<template>
  <g class="cap">
    <path class="cap-body" d="M42 43c4-31 65-40 78-4-25 10-53 11-78 4Z" /><path class="cap-brim" d="M83 38c22-6 41-1 49 7-20 5-36 3-49-7Z" />
    <path class="cap-seam" d="M80 18v23M54 25q25 11 52-2" /><circle class="cap-button" cx="80" cy="17" r="4" />
  </g>
</template>

<style scoped>
.cap-body {
  fill: var(--lad-palette-coral);
  stroke: var(--lad-palette-orange-650);
  stroke-linejoin: round;
  stroke-width: 3;
}
.cap-brim {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-orange-650);
  stroke-linejoin: round;
  stroke-width: 3;
}
.cap-seam {
  fill: none;
  stroke: var(--lad-palette-red-500);
  stroke-linecap: round;
  stroke-width: 2;
}
.cap-button {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-orange-650);
  stroke-width: 1.5;
}
</style>
