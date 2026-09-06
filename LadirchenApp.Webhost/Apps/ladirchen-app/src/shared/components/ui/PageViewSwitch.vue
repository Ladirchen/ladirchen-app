<template>
  <div
    class="page-view-switch"
    :class="[`page-view-switch--${tone}`, { 'page-view-switch--compact': compact, 'page-view-switch--single': options.length === 1 }]"
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

<style scoped>
.page-view-switch {
  --switch-accent: #4e8fdd;
  --switch-accent-dark: #2d6da9;
  --switch-alt: #2a9b73;
  --switch-alt-dark: #21795b;
  --switch-bg: #edf7ff;
  position: relative;
  isolation: isolate;
  padding: 8px;
  @apply d-grid overflow-hidden;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 9px;
  border: 2px solid color-mix(in srgb, var(--switch-accent) 20%, white);
  border-radius: 27px;
  background:
    radial-gradient(
      circle at 88% 12%,
      color-mix(in srgb, var(--switch-alt) 10%, transparent) 0 34px,
      transparent 35px
    ),
    linear-gradient(145deg, var(--switch-bg), #fffdf8);
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--switch-accent) 14%, transparent),
    0 12px 24px color-mix(in srgb, var(--switch-accent) 7%, transparent);
}
.page-view-switch--amber {
  --switch-accent: #e49b2f;
  --switch-accent-dark: #b56d12;
  --switch-alt: #39ae82;
  --switch-alt-dark: #247a5a;
  --switch-bg: #fff6dc;
}
.page-view-switch--single {
  grid-template-columns: minmax(0, 1fr);
}
.page-view-switch button {
  min-height: 76px;
  padding: 11px 12px;
  @apply position-relative d-flex align-center ga-2 text-left cursor-pointer overflow-hidden;
  color: var(--lad-muted);
  border: 1px solid color-mix(in srgb, var(--switch-accent) 9%, white);
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.42);
  font: inherit;
  transition:
    color 180ms ease,
    background 180ms ease,
    box-shadow 180ms ease,
    transform 180ms ease;
}
.page-view-switch button:hover {
  transform: translateY(-2px);
}
.page-view-switch button.active {
  color: var(--switch-accent-dark);
  transform: translateY(-2px);
  border-color: color-mix(in srgb, var(--switch-accent) 30%, white);
  background: linear-gradient(145deg, #fff, #f8fcff);
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--switch-accent) 19%, transparent),
    0 11px 17px color-mix(in srgb, var(--switch-accent) 12%, transparent);
}
.page-view-switch button:nth-child(2).active {
  border-color: color-mix(in srgb, var(--switch-alt) 30%, white);
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--switch-alt) 19%, transparent),
    0 11px 17px color-mix(in srgb, var(--switch-alt) 11%, transparent);
}
.page-view-switch button:focus-visible {
  outline: 3px solid color-mix(in srgb, var(--switch-accent) 26%, transparent);
  outline-offset: 2px;
}
.page-view-icon {
  width: 46px;
  height: 46px;
  @apply d-grid place-center flex-shrink-0;
  border: 2px solid rgba(255, 255, 255, 0.85);
  border-radius: 16px;
  background: color-mix(in srgb, var(--switch-accent) 11%, white);
  box-shadow: 0 3px 0 color-mix(in srgb, var(--switch-accent) 13%, transparent);
  transform: rotate(-3deg);
  transition:
    transform 180ms ease,
    background 180ms ease;
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
  box-shadow: 0 4px 0 var(--switch-accent-dark);
  transform: rotate(-7deg) scale(1.06);
}
.page-view-switch button:nth-child(2).active .page-view-icon {
  background: linear-gradient(
    145deg,
    color-mix(in srgb, var(--switch-alt) 78%, white),
    var(--switch-alt)
  );
  box-shadow: 0 3px 0 var(--switch-alt-dark);
}
.page-view-copy {
  @apply min-w-0;
}
.page-view-copy strong,
.page-view-copy small {
  @apply d-block;
}
.page-view-copy strong {
  font-size: 12px;
  line-height: 1.2;
}
.page-view-copy small {
  margin-top: 3px;
  color: var(--lad-muted);
  font-size: 9px;
}
.page-view-check {
  width: 21px;
  height: 21px;
  @apply position-absolute d-grid place-center;
  top: 7px;
  right: 7px;
  color: white;
  border: 2px solid #fff;
  border-radius: 7px;
  background: var(--switch-accent);
  box-shadow: 0 2px 0
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
  padding: 6px;
  border-radius: 21px;
}
.page-view-switch--compact button {
  min-height: 60px;
  padding: 8px 10px;
  border-radius: 16px;
}
.page-view-switch--compact .page-view-icon {
  width: 34px;
  height: 34px;
  border-radius: 11px;
}
.page-view-switch--compact .page-view-copy strong {
  font-size: 11px;
}
.page-view-switch--compact .page-view-copy small {
  font-size: 8px;
}
.page-view-switch--compact .page-view-check {
  width: 17px;
  height: 17px;
  top: 5px;
  right: 5px;
}
</style>
