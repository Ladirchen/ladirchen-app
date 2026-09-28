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
      <span class="page-view-check"><v-icon icon="i-mdi:check" size="14" /></span>
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
  tone?: "amber" | "blue";
}>(), {
  compact: false,
  tone: "blue",
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.page-view-switch {
  --switch-accent: var(--lad-color-info);
  --switch-accent-dark: var(--lad-color-info-strong);
  --switch-alt: var(--lad-color-primary-strong);
  --switch-alt-dark: var(--lad-color-primary-deep);
  --switch-bg: var(--lad-surface-soft);
  --uno: position-relative d-grid overflow-hidden;
  isolation: isolate;
  padding: 0.5rem;

  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: rem(9);
  border: rem(2) solid
    color-mix(in srgb, var(--switch-accent) 20%, var(--lad-border-on-accent));
  border-radius: rem(27);
  background:
    radial-gradient(
      circle at 88% 12%,
      color-mix(in srgb, var(--switch-alt) 10%, transparent) 0 rem(34),
      transparent rem(35)
    ),
    linear-gradient(145deg, var(--switch-bg), var(--lad-surface));
  box-shadow:
    0 rem(6) 0 color-mix(in srgb, var(--switch-accent) 14%, transparent),
    0 0.75rem 1.5rem color-mix(in srgb, var(--switch-accent) 7%, transparent);
}
.page-view-switch--amber {
  --switch-accent: var(--lad-color-reward-accent);
  --switch-accent-dark: var(--lad-color-reward-deep);
  --switch-alt: var(--lad-color-primary);
  --switch-alt-dark: var(--lad-color-primary-deep);
  --switch-bg: var(--lad-color-reward-soft);
}
.page-view-switch--single {
  grid-template-columns: minmax(0, 1fr);
}
.page-view-switch--many {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}
.page-view-switch--many button {
  padding-inline: 0.5rem;
  gap: rem(6);
}
.page-view-switch--many .page-view-icon {
  width: rem(39);
  height: rem(39);
  border-radius: rem(13);
}
.page-view-switch--many .page-view-copy strong {
  font-size: rem(10);
}
.page-view-switch--many .page-view-copy small {
  font-size: 0.5rem;
}
.page-view-switch button {
  min-height: 4.75rem;
  padding: rem(11) 0.75rem;
  --uno: position-relative d-flex align-center ga-2 text-left cursor-pointer overflow-hidden;
  color: var(--lad-muted);
  border: rem(1) solid
    color-mix(in srgb, var(--switch-accent) 9%, var(--lad-border-on-accent));
  border-radius: 1.25rem;
  background: color-mix(in srgb, var(--lad-surface-raised) 40%, transparent);
  font: inherit;
  transition:
    color 180ms ease,
    background 180ms ease,
    box-shadow 180ms ease,
    transform 180ms ease;
}
.page-view-switch button:hover {
  transform: translateY(rem(-2));
}
.page-view-switch button.active {
  color: var(--switch-accent-dark);
  transform: translateY(rem(-2));
  border-color: color-mix(
    in srgb,
    var(--switch-accent) 30%,
    var(--lad-border-on-accent)
  );
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-surface-raised)
  );
  box-shadow:
    0 rem(6) 0 color-mix(in srgb, var(--switch-accent) 19%, transparent),
    0 rem(11) rem(17)
      color-mix(in srgb, var(--switch-accent) 12%, transparent);
}
.page-view-switch button:nth-child(2).active {
  border-color: color-mix(
    in srgb,
    var(--switch-alt) 30%,
    var(--lad-border-on-accent)
  );
  box-shadow:
    0 rem(6) 0 color-mix(in srgb, var(--switch-alt) 19%, transparent),
    0 rem(11) rem(17) color-mix(in srgb, var(--switch-alt) 11%, transparent);
}
.page-view-switch button:focus-visible {
  @include focus-ring(
    color-mix(in srgb, var(--switch-accent) 26%, transparent)
  );
}
.page-view-icon {
  @include icon-tile(
    rem(46),
    1rem,
    color-mix(in srgb, var(--switch-accent) 11%, var(--lad-surface-raised)),
    color-mix(in srgb, var(--switch-accent) 13%, transparent),
    -3deg
  );
  box-shadow: 0 rem(3) 0
    color-mix(in srgb, var(--switch-accent) 13%, transparent);
  transition:
    transform var(--lad-motion-normal) ease,
    background var(--lad-motion-normal) ease;
}
.page-view-icon :deep(.v-icon) {
  --uno: d-block ma-auto;

  color: inherit;
  opacity: 1;
}
.page-view-switch button:nth-child(2) .page-view-icon {
  background: color-mix(
    in srgb,
    var(--switch-alt) 11%,
    var(--lad-surface-raised)
  );
}
.active .page-view-icon {
  color: var(--lad-text-inverse);
  background: linear-gradient(
    145deg,
    color-mix(in srgb, var(--switch-accent) 78%, var(--lad-surface-raised)),
    var(--switch-accent)
  );
  box-shadow: 0 0.25rem 0 var(--switch-accent-dark);
  transform: rotate(-7deg) scale(1.06);
}
.page-view-switch button:nth-child(2).active .page-view-icon {
  background: linear-gradient(
    145deg,
    color-mix(in srgb, var(--switch-alt) 78%, var(--lad-surface-raised)),
    var(--switch-alt)
  );
  box-shadow: 0 rem(3) 0 var(--switch-alt-dark);
}
.page-view-copy {
  --uno: min-w-0;
}
.page-view-copy strong,
.page-view-copy small {
  --uno: d-block;
}
.page-view-copy strong {
  font-size: 0.75rem;
  line-height: 1.2;
}
.page-view-copy small {
  margin-top: rem(3);
  color: var(--lad-muted);
  font-size: rem(9);
}
.page-view-check {
  width: rem(21);
  height: rem(21);
  --uno: position-absolute d-grid place-center;
  top: rem(7);
  right: rem(7);
  color: var(--lad-text-inverse);
  border: rem(2) solid var(--lad-border-on-accent);
  border-radius: rem(7);
  background: var(--switch-accent);
  box-shadow: 0 rem(2) 0
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
  padding: rem(6);
  border-radius: rem(21);
}
.page-view-switch--compact button {
  min-height: 3.75rem;
  padding: 0.5rem rem(10);
  border-radius: 1rem;
}
.page-view-switch--compact .page-view-icon {
  width: rem(34);
  height: rem(34);
  border-radius: rem(11);
}
.page-view-switch--compact .page-view-copy strong {
  font-size: rem(11);
}
.page-view-switch--compact .page-view-copy small {
  font-size: 0.5rem;
}
.page-view-switch--compact .page-view-check {
  width: rem(17);
  height: rem(17);
  top: rem(5);
  right: rem(5);
}
@include respond-down(phone) {
  .page-view-switch--many button {
    min-height: rem(82);
    --uno: flex-column justify-center;

    padding: rem(7) 0.25rem;
    text-align: center;
  }
  .page-view-switch--many .page-view-copy small {
    margin-top: rem(1);
  }
  .page-view-switch--many .page-view-check {
    top: 0.25rem;
    right: 0.25rem;
  }
}
</style>
