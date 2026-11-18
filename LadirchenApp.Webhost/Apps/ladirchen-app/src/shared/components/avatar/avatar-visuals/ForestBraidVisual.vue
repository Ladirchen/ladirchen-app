<template>
  <g v-if="layer === 'back'" class="forest-braid hair" aria-hidden="true">
    <path d="M37 67q0-54 44-54 43 0 42 55l-10 35H48Z" />
    <path class="braid" d="M109 69q18 10 7 24 14 10 2 24 11 11-1 25-13-7-9-20-12-11-4-25-10-14 5-28Z" />
    <path class="tie" d="m109 136 9 8 7-10" />
  </g>
  <g v-else class="forest-braid hair" aria-hidden="true">
    <path d="M38 61q0-49 43-49 39 0 41 47-17-3-28-24-15 18-56 26Z" />
    <path class="leaf" d="M103 25q13-9 16 4-11 8-16-4Z" />
  </g>
</template>

<script lang="ts" setup>
export interface Props {
  layer: "back" | "front";
}
defineProps<Props>();
</script>

<style scoped>
.forest-braid {
  fill: var(--hair);
  stroke: var(--lad-palette-muted-750);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.forest-braid .braid {
  fill: var(--hair);
}
.forest-braid .tie {
  fill: none;
  stroke: var(--lad-palette-amber-650);
  stroke-width: 4;
}
.forest-braid .leaf {
  fill: var(--lad-palette-green-500);
  stroke: var(--lad-palette-teal-700);
  stroke-width: 1.5;
}
</style>
