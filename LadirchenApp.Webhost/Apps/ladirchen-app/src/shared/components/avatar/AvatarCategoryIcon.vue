<template>
  <svg class="category-icon" viewBox="0 0 48 48" aria-hidden="true">
    <g v-if="name === 'base'">
      <circle class="soft" cx="24" cy="24" r="18" />
      <circle class="skin" cx="24" cy="21" r="10" />
      <path class="line" d="M12 41c2-9 7-13 12-13s10 4 12 13" />
    </g>
    <g v-else-if="name === 'face'">
      <circle class="skin" cx="24" cy="24" r="17" />
      <circle class="dark" cx="18" cy="21" r="2.5" /><circle class="dark" cx="30" cy="21" r="2.5" />
      <path class="line" d="M17 29q7 8 14 0" />
    </g>
    <g v-else-if="name === 'hair'">
      <circle class="skin" cx="24" cy="27" r="13" />
      <path class="hair" d="M10 25C9 10 17 5 25 6c10 1 15 8 14 20-7-1-12-5-15-11-3 6-8 9-14 10Z" />
      <path class="line" d="M16 32q8 7 16 0" />
    </g>
    <g v-else-if="name === 'outfit'">
      <path class="mint" d="m15 10 7 4h4l7-4 9 9-6 6v17H12V25l-6-6Z" />
      <path class="line" d="M22 14v9h4v-9M13 26h22" />
    </g>
    <g v-else-if="name === 'extras'">
      <circle class="glass" cx="16" cy="25" r="9" /><circle class="glass" cx="32" cy="25" r="9" />
      <path class="line" d="M25 24h-2M7 21l-5-3m39 3 5-3" />
      <path class="gold" d="m36 5 2 5 5 2-5 2-2 5-2-5-5-2 5-2Z" />
    </g>
    <g v-else-if="name === 'fun'">
      <path class="mustache" d="M24 27c-5-8-14-6-14 1 0 8 10 9 14 2 4 7 14 6 14-2 0-7-9-9-14-1Z" />
      <circle class="red" cx="24" cy="17" r="7" /><circle class="shine" cx="22" cy="15" r="2" />
    </g>
    <g v-else>
      <path class="red" d="M8 27C13 9 31 3 40 20l-19 9Z" />
      <circle class="soft" cx="40" cy="20" r="6" /><path class="soft" d="M7 27q17-7 34-1l-1 7q-16-5-32 1Z" />
      <path class="gold" d="m13 5 2 4 4 2-4 2-2 4-2-4-4-2 4-2Z" />
    </g>
  </svg>
</template>

<script lang="ts" setup>
defineProps<{ name: "base" | "face" | "hair" | "outfit" | "extras" | "fun" | "season" }>();
</script>

<style scoped>
.category-icon {
  width: 34px;
  height: 34px;
  --uno: overflow-visible;
}
.line {
  fill: none;
  stroke: var(--lad-palette-muted-700);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2.6;
}
.soft {
  fill: var(--lad-palette-amber-100);
  stroke: var(--lad-palette-orange-350);
  stroke-width: 1.5;
}
.skin {
  fill: var(--lad-palette-amber-200);
  stroke: var(--lad-palette-orange-600);
  stroke-width: 1.8;
}
.dark {
  fill: var(--lad-palette-muted-700);
}
.hair,
.mustache {
  fill: var(--lad-palette-orange-750);
  stroke: var(--lad-palette-muted-750);
  stroke-linejoin: round;
  stroke-width: 1.8;
}
.mint {
  fill: var(--lad-palette-teal-400);
  stroke: var(--lad-palette-teal-700);
  stroke-linejoin: round;
  stroke-width: 2;
}
.glass {
  fill: var(--lad-palette-blue-150);
  stroke: var(--lad-palette-muted-700);
  stroke-width: 2.5;
}
.gold {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-linejoin: round;
  stroke-width: 1.2;
}
.red {
  fill: var(--lad-palette-red-400);
  stroke: var(--lad-palette-red-600);
  stroke-linejoin: round;
  stroke-width: 1.8;
}
.shine {
  fill: var(--lad-palette-red-100);
}
</style>
