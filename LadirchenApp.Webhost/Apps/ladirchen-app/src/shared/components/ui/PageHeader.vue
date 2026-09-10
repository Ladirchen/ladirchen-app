<template>
  <header class="family-world-page-header" :class="`family-world-page-header--${tone}`">
    <div class="page-header-copy">
      <p class="page-header-kicker">{{ eyebrow }}</p>
      <h1>{{ title }}</h1>
      <p>{{ description }}</p>
    </div>
    <div class="page-header-side">
      <div v-if="icon || $slots.icon" class="page-header-icon" aria-hidden="true">
        <slot name="icon">
          <v-icon :icon="icon" size="30" />
        </slot>
      </div>
      <slot name="action" />
    </div>
  </header>
</template>

<script lang="ts" setup>
withDefaults(defineProps<{
  description: string;
  eyebrow: string;
  icon?: string;
  title: string;
  tone?: 'mint' | 'blue' | 'amber' | 'coral';
}>(), {
  icon: undefined,
  tone: 'mint',
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.family-world-page-header {
  --header-accent: var(--lad-palette-teal-550);
  --header-accent-soft: var(--lad-palette-background);
  min-height: 8.625rem;
  @apply mb-5 pa-5 position-relative overflow-hidden d-flex align-center justify-space-between ga-4;
  @include raised-surface(
    color-mix(in srgb, var(--header-accent) 18%, white),
    color-mix(in srgb, var(--header-accent) 14%, transparent),
    var(--lad-radius-large),
    0.3125rem,
    0.0625rem
  );
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface) 0%,
    var(--header-accent-soft) 145%
  );
}
.family-world-page-header::before,
.family-world-page-header::after {
  content: "";
  @apply position-absolute;
  border-radius: 50%;
  @apply pointer-events-none;
}
.family-world-page-header::before {
  width: 8.125rem;
  height: 8.125rem;
  top: -4.8125rem;
  right: -2rem;
  background: color-mix(in srgb, var(--header-accent-soft) 68%, transparent);
  box-shadow: 0 0 0 1.125rem
    color-mix(in srgb, var(--header-accent-soft) 35%, transparent);
}
.family-world-page-header::after {
  width: 5.625rem;
  height: 1.75rem;
  right: 3rem;
  bottom: -1.0625rem;
  background: color-mix(in srgb, var(--header-accent-soft) 55%, transparent);
}
.family-world-page-header--blue {
  --header-accent: var(--lad-palette-blue);
  --header-accent-soft: var(--lad-palette-background);
}
.family-world-page-header--amber {
  --header-accent: var(--lad-palette-amber-450);
  --header-accent-soft: var(--lad-palette-amber-100);
}
.family-world-page-header--coral {
  --header-accent: var(--lad-palette-coral);
  --header-accent-soft: var(--lad-palette-amber-100);
}
.page-header-copy {
  max-width: 19.375rem;
  @apply position-relative;
  z-index: 1;
}
.page-header-kicker {
  margin: 0 0 0.3125rem;
  @include overline(var(--header-accent));
}
h1 {
  @apply ma-0;
  @include heading(var(--lad-font-size-page), 1.08, -0.045em);
}
.page-header-copy > p:last-child {
  margin: 0.4375rem 0 0;
  @include body-copy(var(--lad-font-size-body), 1.45);
}
.page-header-side {
  min-width: 3.375rem;
  @apply position-relative;
  z-index: 1;
  @apply d-flex align-center ga-2;
}
.page-header-icon {
  @include icon-tile(
    3.375rem,
    var(--lad-radius-medium),
    color-mix(in srgb, var(--lad-palette-white) 75%, transparent),
    color-mix(in srgb, var(--header-accent) 13%, transparent),
    0deg,
    0
  );
  color: var(--header-accent);
}
@include respond-down(narrow) {
  .family-world-page-header {
    min-height: 7.875rem;
    padding: 1.0625rem;
  }
  .page-header-copy {
    max-width: 15.3125rem;
  }
  .page-header-copy > p:last-child {
    font-size: 0.6875rem;
  }
  .page-header-icon {
    width: 2.875rem;
    height: 2.875rem;
    border-radius: 0.9375rem;
  }
}
</style>
