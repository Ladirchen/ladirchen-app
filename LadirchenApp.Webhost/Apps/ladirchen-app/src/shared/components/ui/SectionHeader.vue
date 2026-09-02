<template>
  <div class="family-world-section-header">
    <div class="section-copy">
      <span v-if="eyebrow">{{ eyebrow }}</span>
      <h2>{{ title }}</h2>
      <p v-if="description">{{ description }}</p>
    </div>
    <div v-if="$slots.action" class="section-action"><slot name="action" /></div>
  </div>
</template>

<script lang="ts" setup>
withDefaults(defineProps<{
  description?: string;
  eyebrow?: string;
  title: string;
}>(), {
  description: undefined,
  eyebrow: undefined,
});
</script>

<style scoped>
.family-world-section-header {
  @apply d-flex align-end justify-space-between mb-3 ga-3;
}
.section-copy {
  @apply min-w-0;
}
.section-copy > span {
  @apply d-block;
  margin-bottom: 2px;
  color: var(--lad-mint-dark);
  font-size: 8px;
  font-weight: 950;
  letter-spacing: 0.1em;
  @apply text-uppercase;
}
h2 {
  @apply ma-0;
  color: var(--lad-text);
  font-size: 18px;
  line-height: 1.2;
  letter-spacing: -0.025em;
}
p {
  margin: 3px 0 0;
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.4;
}
.section-action {
  @apply flex-shrink-0;
}
</style>
