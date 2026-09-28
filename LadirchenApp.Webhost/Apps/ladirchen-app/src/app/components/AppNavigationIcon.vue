<template>
  <svg class="app-navigation-icon" viewBox="0 0 48 48" aria-hidden="true">
    <template v-if="name === 'world'">
      <path class="soft" d="M8 23 24 9l16 14v17H8Z" />
      <path class="line" d="m5 24 19-17 19 17M10 21v20h28V21" />
      <path class="accent" d="M20 28h9v13h-9z" /><path class="line thin" d="M14 26h7m-3-4v8m11-4h6m-3-4v8" />
    </template>
    <template v-else-if="name === 'contributions'">
      <path class="accent" d="M24 42V21" /><path class="line" d="M24 42V21" />
      <path class="soft" d="M23 25C11 26 7 18 8 10c11-1 19 5 15 15Z" /><path class="line" d="M23 25C11 26 7 18 8 10c11-1 19 5 15 15Z" />
      <path class="soft" d="M25 30c13 1 18-8 16-17-11-1-19 7-16 17Z" /><path class="line" d="M25 30c13 1 18-8 16-17-11-1-19 7-16 17Z" />
      <path class="line thin" d="m12 14 11 11m14-8L25 29" />
    </template>
    <template v-else-if="name === 'wishes'">
      <path class="accent" d="m24 5 4.5 11 11.5 1-9 7.5 3 11.5-10-6-10 6 3-11.5-9-7.5 11.5-1Z" />
      <path class="line" d="m24 5 4.5 11 11.5 1-9 7.5 3 11.5-10-6-10 6 3-11.5-9-7.5 11.5-1Z" />
      <path class="shine" d="m37 7 1.5 4 4 1.5-4 1.5-1.5 4-1.5-4-4-1.5 4-1.5Zm-28 29 1 3 3 1-3 1-1 3-1-3-3-1 3-1Z" />
    </template>
    <template v-else-if="name === 'shop'">
      <path class="soft" d="M8 20h32v23H8Z" /><path class="line" d="M8 20h32v23H8Zm16 0v23M6 15h36v8H6Z" />
      <path class="accent" d="M21 15c-9 0-10-10-4-11 5-1 7 6 7 11Zm6 0c9 0 10-10 4-11-5-1-7 6-7 11Z" />
      <path class="line thin" d="M21 15c-9 0-10-10-4-11 5-1 7 6 7 11Zm6 0c9 0 10-10 4-11-5-1-7 6-7 11Z" />
    </template>
    <template v-else-if="name === 'family'">
      <circle class="soft" cx="24" cy="15" r="7" /><circle class="soft" cx="11" cy="20" r="5" /><circle class="soft" cx="37" cy="20" r="5" />
      <path class="accent" d="M13 42c0-10 4-17 11-17s11 7 11 17Z" /><path class="soft" d="M2 42c0-8 3-14 9-14 4 0 6 3 7 6m28 8c0-8-3-14-9-14-4 0-6 3-7 6" />
      <circle class="line" cx="24" cy="15" r="7" /><circle class="line thin" cx="11" cy="20" r="5" /><circle class="line thin" cx="37" cy="20" r="5" /><path class="line" d="M13 42c0-10 4-17 11-17s11 7 11 17" /><path class="line thin" d="M2 42c0-8 3-14 9-14 4 0 6 3 7 6m28 8c0-8-3-14-9-14-4 0-6 3-7 6" />
    </template>
    <template v-else>
      <circle class="accent" cx="24" cy="15" r="9" /><path class="soft" d="M8 43c1-12 6-19 16-19s15 7 16 19Z" />
      <circle class="line" cx="24" cy="15" r="9" /><path class="line" d="M8 43c1-12 6-19 16-19s15 7 16 19" />
      <path class="shine" d="m36 5 1.2 3.2L40.5 9l-3.3 1.2L36 13.5l-1.2-3.3L31.5 9l3.3-.8Z" />
    </template>
  </svg>
</template>

<script lang="ts" setup>
defineProps<{
  name: "world" | "contributions" | "wishes" | "shop" | "family" | "profile";
}>();
</script>

<style scoped>
.app-navigation-icon {
  width: 27px;
  height: 27px;
  --uno: d-block overflow-visible;
}
.line {
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}
.line.thin {
  stroke-width: 2;
}
.soft {
  fill: var(--lad-palette-background);
}
.accent {
  fill: var(--lad-palette-yellow);
}
.shine {
  fill: var(--lad-palette-teal-400);
  stroke: currentColor;
  stroke-width: 0.8;
}
</style>
