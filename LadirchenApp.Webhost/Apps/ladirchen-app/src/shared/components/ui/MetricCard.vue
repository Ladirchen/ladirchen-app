<template>
  <div class="metric-card" :class="[`metric-card--${tone}`, { 'metric-card--compact': compact }]">
    <slot />
  </div>
</template>

<script lang="ts" setup>
export type MetricCardTone = 'balance' | 'bonus' | 'energy' | 'info' | 'neutral' | 'reward' | 'streak';

withDefaults(defineProps<{
  compact?: boolean;
  tone?: MetricCardTone;
}>(), {
  compact: false,
  tone: 'neutral',
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.metric-card {
  --metric-card-accent: var(--lad-color-info);
  --metric-card-tint: var(--lad-color-info-soft);
  --metric-card-lift: var(--lad-color-info-strong);

  min-height: 4.25rem;
  @apply pa-2 d-flex align-center overflow-hidden;
  gap: 0.5rem;
  @include metric-card;
}

.metric-card--compact {
  min-height: 3.375rem;
  padding: 0.25rem 0.3125rem 0.25rem 0.25rem;
  gap: 0.25rem;
  border-radius: 0.875rem;
}

.metric-card :deep(> span:last-child) {
  flex: 1 1 auto;
  overflow: hidden;
}

.metric-card :deep(> span:last-child),
.metric-card :deep(small),
.metric-card :deep(strong) {
  @apply d-block min-w-0;
  max-width: 100%;
  overflow-wrap: anywhere;
}

.metric-card :deep(small) {
  color: var(--lad-muted);
  font-size: 0.5rem;
  line-height: 1.2;
}

.metric-card :deep(strong) {
  margin-top: 0.125rem;
  color: var(--lad-text-strong);
  font-size: 1rem;
  line-height: 1.15;
}

.metric-card--compact :deep(small) {
  font-size: 0.4375rem;
  font-weight: var(--lad-font-weight-heavy);
  line-height: 1.1;
}

.metric-card--compact :deep(strong) {
  font-size: 0.6875rem;
  font-weight: var(--lad-font-weight-black);
}

.metric-card--neutral {
  --metric-card-accent: var(--lad-neutral-accent);
  --metric-card-tint: var(--lad-neutral-soft);
  --metric-card-lift: var(--lad-text-subtle);
}

.metric-card--reward,
.metric-card--balance {
  --metric-card-accent: var(--lad-color-reward-border);
  --metric-card-tint: var(--lad-color-reward-soft);
  --metric-card-lift: var(--lad-color-reward-shadow);
}

.metric-card--energy {
  --metric-card-accent: var(--lad-color-primary-muted);
  --metric-card-tint: var(--lad-color-primary-soft);
  --metric-card-lift: var(--lad-color-primary-supporting);
}

.metric-card--bonus {
  --metric-card-accent: var(--lad-color-bonus-muted);
  --metric-card-tint: var(--lad-color-bonus-soft);
  --metric-card-lift: var(--lad-color-bonus-strong);
}

.metric-card--streak {
  --metric-card-accent: var(--lad-tone-streak-accent);
  --metric-card-tint: var(--lad-tone-streak-tint);
  --metric-card-lift: var(--lad-tone-streak-lift);
}
</style>
