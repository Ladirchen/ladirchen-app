<template>
  <div
    class="page-view-switch"
    :class="[`page-view-switch--${tone}`, { 'page-view-switch--compact': compact, 'page-view-switch--single': options.length === 1, 'page-view-switch--many': options.length === 3 }]"
    role="tablist"
    :aria-label="label"
  >
    <button
      v-for="option in options"
      :key="option.id"
      :aria-selected="model === option.id"
      :class="{ active: model === option.id }"
      role="tab"
      type="button"
      @click="model = option.id"
    >
      <span class="page-view-icon"><v-icon :icon="option.icon" size="23" /></span>
      <span class="page-view-copy">
        <strong>{{ option.title }}</strong>
        <small>{{ option.subtitle }}</small>
      </span>
      <span class="page-view-check"><v-icon icon="mdi-check" size="14" /></span>
    </button>
  </div>
</template>

<script lang="ts" setup generic="T extends string">
export interface PageViewOption<T extends string = string> {
  id: T;
  icon: string;
  subtitle: string;
  title: string;
}

const model = defineModel<T>({ required: true });

withDefaults(defineProps<{
  compact?: boolean;
  label: string;
  options: readonly PageViewOption<T>[];
  tone?: 'amber' | 'blue';
}>(), {
  compact: false,
  tone: 'blue',
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.page-view-switch {
  --switch-accent: var(--lad-palette-blue);
  --switch-accent-dark: var(--lad-palette-blue-strong);
  --switch-alt: var(--lad-palette-mint-strong);
  --switch-alt-dark: var(--lad-palette-teal-700);
  --switch-bg: var(--lad-palette-background);
  position: relative;
  isolation: isolate;
  padding: 0.5rem;
  @apply d-grid overflow-hidden;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.5625rem;
  border: 0.125rem solid color-mix(in srgb, var(--switch-accent) 20%, white);
  border-radius: 1.6875rem;
  background:
    radial-gradient(
      circle at 88% 12%,
      color-mix(in srgb, var(--switch-alt) 10%, transparent) 0 2.125rem,
      transparent 2.1875rem
    ),
    linear-gradient(145deg, var(--switch-bg), var(--lad-palette-surface));
  box-shadow:
    0 0.375rem 0 color-mix(in srgb, var(--switch-accent) 14%, transparent),
    0 0.75rem 1.5rem color-mix(in srgb, var(--switch-accent) 7%, transparent);
}
.page-view-switch--amber {
  --switch-accent: var(--lad-palette-amber-500);
  --switch-accent-dark: var(--lad-palette-amber-600);
  --switch-alt: var(--lad-palette-mint);
  --switch-alt-dark: var(--lad-palette-teal-700);
  --switch-bg: var(--lad-palette-amber-100);
}
.page-view-switch--single {
  grid-template-columns: minmax(0, 1fr);
}
.page-view-switch--many {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}
.page-view-switch--many button {
  padding-inline: 0.5rem;
  gap: 0.375rem;
}
.page-view-switch--many .page-view-icon {
  width: 2.4375rem;
  height: 2.4375rem;
  border-radius: 0.8125rem;
}
.page-view-switch--many .page-view-copy strong {
  font-size: 0.625rem;
}
.page-view-switch--many .page-view-copy small {
  font-size: 0.5rem;
}
.page-view-switch button {
  min-height: 4.75rem;
  padding: 0.6875rem 0.75rem;
  @apply position-relative d-flex align-center ga-2 text-left cursor-pointer overflow-hidden;
  color: var(--lad-muted);
  border: 0.0625rem solid color-mix(in srgb, var(--switch-accent) 9%, white);
  border-radius: 1.25rem;
  background: color-mix(in srgb, var(--lad-palette-white) 40%, transparent);
  font: inherit;
  transition:
    color 180ms ease,
    background 180ms ease,
    box-shadow 180ms ease,
    transform 180ms ease;
}
.page-view-switch button:hover {
  transform: translateY(-0.125rem);
}
.page-view-switch button.active {
  color: var(--switch-accent-dark);
  transform: translateY(-0.125rem);
  border-color: color-mix(in srgb, var(--switch-accent) 30%, white);
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-white)
  );
  box-shadow:
    0 0.375rem 0 color-mix(in srgb, var(--switch-accent) 19%, transparent),
    0 0.6875rem 1.0625rem
      color-mix(in srgb, var(--switch-accent) 12%, transparent);
}
.page-view-switch button:nth-child(2).active {
  border-color: color-mix(in srgb, var(--switch-alt) 30%, white);
  box-shadow:
    0 0.375rem 0 color-mix(in srgb, var(--switch-alt) 19%, transparent),
    0 0.6875rem 1.0625rem color-mix(in srgb, var(--switch-alt) 11%, transparent);
}
.page-view-switch button:focus-visible {
  @include focus-ring(
    color-mix(in srgb, var(--switch-accent) 26%, transparent)
  );
}
.page-view-icon {
  @include icon-tile(
    2.875rem,
    1rem,
    color-mix(in srgb, var(--switch-accent) 11%, white),
    color-mix(in srgb, var(--switch-accent) 13%, transparent),
    -3deg
  );
  box-shadow: 0 0.1875rem 0
    color-mix(in srgb, var(--switch-accent) 13%, transparent);
  transition:
    transform var(--lad-motion-normal) ease,
    background var(--lad-motion-normal) ease;
}
.page-view-switch button:nth-child(2) .page-view-icon {
  background: color-mix(in srgb, var(--switch-alt) 11%, white);
}
.active .page-view-icon {
  color: white;
  background: linear-gradient(
    145deg,
    color-mix(in srgb, var(--switch-accent) 78%, white),
    var(--switch-accent)
  );
  box-shadow: 0 0.25rem 0 var(--switch-accent-dark);
  transform: rotate(-7deg) scale(1.06);
}
.page-view-switch button:nth-child(2).active .page-view-icon {
  background: linear-gradient(
    145deg,
    color-mix(in srgb, var(--switch-alt) 78%, white),
    var(--switch-alt)
  );
  box-shadow: 0 0.1875rem 0 var(--switch-alt-dark);
}
.page-view-copy {
  @apply min-w-0;
}
.page-view-copy strong,
.page-view-copy small {
  @apply d-block;
}
.page-view-copy strong {
  font-size: 0.75rem;
  line-height: 1.2;
}
.page-view-copy small {
  margin-top: 0.1875rem;
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.page-view-check {
  width: 1.3125rem;
  height: 1.3125rem;
  @apply position-absolute d-grid place-center;
  top: 0.4375rem;
  right: 0.4375rem;
  color: white;
  border: 0.125rem solid var(--lad-palette-white);
  border-radius: 0.4375rem;
  background: var(--switch-accent);
  box-shadow: 0 0.125rem 0
    color-mix(in srgb, var(--switch-accent-dark) 45%, transparent);
  opacity: 0;
  transform: rotate(12deg) scale(0.65);
  transition:
    opacity 180ms ease,
    transform 180ms ease;
}
.page-view-switch button:nth-child(2) .page-view-check {
  background: var(--switch-alt);
}
.page-view-switch button.active .page-view-check {
  opacity: 1;
  transform: rotate(12deg) scale(1);
}
.page-view-switch--compact {
  padding: 0.375rem;
  border-radius: 1.3125rem;
}
.page-view-switch--compact button {
  min-height: 3.75rem;
  padding: 0.5rem 0.625rem;
  border-radius: 1rem;
}
.page-view-switch--compact .page-view-icon {
  width: 2.125rem;
  height: 2.125rem;
  border-radius: 0.6875rem;
}
.page-view-switch--compact .page-view-copy strong {
  font-size: 0.6875rem;
}
.page-view-switch--compact .page-view-copy small {
  font-size: 0.5rem;
}
.page-view-switch--compact .page-view-check {
  width: 1.0625rem;
  height: 1.0625rem;
  top: 0.3125rem;
  right: 0.3125rem;
}
@include respond-down(phone) {
  .page-view-switch--many button {
    min-height: 5.125rem;
    flex-direction: column;
    justify-content: center;
    padding: 0.4375rem 0.25rem;
    text-align: center;
  }
  .page-view-switch--many .page-view-copy small {
    margin-top: 0.0625rem;
  }
  .page-view-switch--many .page-view-check {
    top: 0.25rem;
    right: 0.25rem;
  }
}
</style>
