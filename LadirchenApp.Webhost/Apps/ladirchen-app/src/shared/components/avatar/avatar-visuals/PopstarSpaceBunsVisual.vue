<template>
  <g v-if="layer === 'back'" class="popstar-space-buns hair" aria-hidden="true">
    <circle cx="40" cy="27" r="20" />
    <circle cx="120" cy="27" r="20" />
    <path d="M37 69q0-55 44-55 43 0 42 56l-13 30H51Z" />
    <path class="ribbons" d="m28 27-12-8 5 16m111-8 12-8-5 16" />
  </g>
  <g v-else class="popstar-space-buns hair" aria-hidden="true">
    <path d="M38 61q0-49 43-49 39 0 41 45-17-1-29-22-17 19-55 26Z" />
    <path class="sparkles" d="m44 18 2 5 5 2-5 2-2 5-2-5-5-2 5-2Zm73 2 2 5 5 2-5 2-2 5-2-5-5-2 5-2Z" />
  </g>
</template>

<script lang="ts" setup>
export interface Props {
  layer: "back" | "front";
}
defineProps<Props>();
</script>

<style scoped>
.popstar-space-buns {
  fill: var(--hair);
  stroke: var(--lad-palette-muted-750);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.popstar-space-buns .ribbons {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-red-600);
  stroke-width: 2;
}
.popstar-space-buns .sparkles {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1;
}
</style>
