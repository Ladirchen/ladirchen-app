<template>
  <g v-if="layer === 'back'" class="glitter-stage-jacket back" aria-hidden="true">
    <path d="M48 128 27 139l-8 36 26-8m67-39 21 11 8 36-26-8" />
  </g>
  <g v-else-if="layer === 'front'" class="glitter-stage-jacket front" aria-hidden="true">
    <path d="M45 138q9-12 24-16l11 14 11-14q16 4 24 16l3 42H92l-12-44-12 44H42Z" />
    <path class="lapels" d="m68 123 12 13-13 19-11-25m36-7-12 13 13 19 11-25" />
    <path class="sparkles" d="m53 147 2 4 4 2-4 2-2 4-2-4-4-2 4-2Zm53 8 2 4 4 2-4 2-2 4-2-4-4-2 4-2Z" />
  </g>
</template>

<script lang="ts" setup>
export interface Props {
  layer: "back" | "front" | "head";
}
defineProps<Props>();
</script>

<style scoped>
.glitter-stage-jacket.back {
  fill: var(--lad-palette-violet-400);
  stroke: var(--lad-palette-violet-650);
  stroke-linejoin: round;
  stroke-width: 3;
}
.glitter-stage-jacket.front > path:first-child {
  fill: color-mix(in srgb, var(--outfit) 65%, var(--lad-palette-violet-400));
  stroke: var(--lad-palette-muted-700);
}
.glitter-stage-jacket .lapels {
  fill: color-mix(in srgb, var(--lad-palette-white) 35%, transparent);
  stroke: var(--lad-palette-background);
}
.glitter-stage-jacket .sparkles {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1;
}
</style>
