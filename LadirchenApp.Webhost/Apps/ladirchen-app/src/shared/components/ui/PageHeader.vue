<template>
  <header class="family-world-page-header" :class="`family-world-page-header--${tone}`">
    <HeaderDecoration :tone="tone === 'profile' ? 'profile' : 'world'" />
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
import HeaderDecoration from './HeaderDecoration.vue';

withDefaults(defineProps<{
  description: string;
  eyebrow: string;
  icon?: string;
  title: string;
  tone?: 'mint' | 'blue' | 'amber' | 'coral' | 'profile';
}>(), {
  icon: undefined,
  tone: 'mint',
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.family-world-page-header {
  --header-accent: var(--lad-color-primary-muted);
  --header-accent-soft: var(--lad-surface-soft);
  min-height: rem(138);
  @apply mb-5 pa-5 position-relative overflow-hidden d-flex align-center justify-space-between ga-4;
  @include raised-surface(
    color-mix(in srgb, var(--header-accent) 18%, var(--lad-surface-raised)),
    color-mix(in srgb, var(--header-accent) 14%, transparent),
    var(--lad-radius-large),
    rem(5),
    rem(1)
  );
  background: linear-gradient(
    145deg,
    var(--lad-surface) 0%,
    var(--header-accent-soft) 145%
  );
}
.family-world-page-header--blue {
  --header-accent: var(--lad-color-info);
  --header-accent-soft: var(--lad-surface-soft);
}
.family-world-page-header--amber {
  --header-accent: var(--lad-color-reward-border);
  --header-accent-soft: var(--lad-color-reward-soft);
}
.family-world-page-header--coral {
  --header-accent: var(--lad-color-accent-coral);
  --header-accent-soft: var(--lad-color-reward-soft);
}
.family-world-page-header--profile {
  --header-accent: var(--lad-tone-profile-accent);
  --header-accent-soft: var(--lad-tone-profile-tint);
}
.page-header-copy {
  max-width: rem(310);
  @apply position-relative;
  z-index: 1;
}
.page-header-kicker {
  margin: 0 0 rem(5);
  @include overline(var(--header-accent));
}
h1 {
  @apply ma-0;
  @include heading(var(--lad-font-size-page), 1.08, -0.045em);
}
.page-header-copy > p:last-child {
  margin: rem(7) 0 0;
  @include body-copy(var(--lad-font-size-body), 1.45);
}
.page-header-side {
  min-width: rem(54);
  @apply position-relative;
  z-index: 1;
  @apply d-flex align-center ga-2;
}
.page-header-icon {
  @include icon-tile(
    rem(54),
    var(--lad-radius-medium),
    color-mix(in srgb, var(--lad-surface-raised) 75%, transparent),
    color-mix(in srgb, var(--header-accent) 13%, transparent),
    0deg,
    0
  );
  color: var(--header-accent);
}
@include respond-down(narrow) {
  .family-world-page-header {
    min-height: rem(126);
    padding: rem(17);
  }
  .page-header-copy {
    max-width: rem(245);
  }
  .page-header-copy > p:last-child {
    font-size: rem(11);
  }
  .page-header-icon {
    width: rem(46);
    height: rem(46);
    border-radius: rem(15);
  }
}
</style>
