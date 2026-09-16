<template>
  <g class="glasses">
    <circle class="glasses-lens" cx="59" cy="71" r="13" /><circle class="glasses-lens" cx="101" cy="71" r="13" />
    <path class="glasses-frame" d="M72 70q8-5 16 0m-42-2-10-4m78 4 10-4" />
    <path class="glasses-shine" d="m52 64 5-3m36 3 5-3" />
  </g>
</template>

<style scoped>
.glasses-lens {
  fill: color-mix(in srgb, var(--lad-palette-blue-250) 20%, transparent);
  stroke: var(--lad-palette-muted-700);
  stroke-width: 3;
}
.glasses-frame {
  fill: none;
  stroke: var(--lad-palette-muted-700);
  stroke-linecap: round;
  stroke-width: 3;
}
.glasses-shine {
  fill: none;
  stroke: color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  stroke-linecap: round;
  stroke-width: 2.5;
}
</style>
