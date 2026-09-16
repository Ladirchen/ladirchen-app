<template>
  <g class="star-glasses">
    <path class="star-glasses-lenses" d="m59 57 4 8 9 1-7 6 2 9-8-5-8 5 2-9-7-6 9-1Zm42 0 4 8 9 1-7 6 2 9-8-5-8 5 2-9-7-6 9-1Z" />
    <path class="star-glasses-bridge" d="M68 69q12-6 24 0M47 66l-10-4m76 4 10-4" />
    <path class="star-glasses-shine" d="m55 64 4-2m38 2 4-2" />
  </g>
</template>

<style scoped>
.star-glasses-lenses {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-violet-500);
  stroke-linejoin: round;
  stroke-width: 3;
}
.star-glasses-bridge {
  fill: none;
  stroke: var(--lad-palette-violet-500);
  stroke-linecap: round;
  stroke-width: 3;
}
.star-glasses-shine {
  fill: none;
  stroke: var(--lad-palette-white);
  stroke-linecap: round;
  stroke-width: 2;
}
</style>
