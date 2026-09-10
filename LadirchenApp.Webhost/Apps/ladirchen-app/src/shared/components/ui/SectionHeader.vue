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

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.family-world-section-header {
  @apply d-flex align-end justify-space-between mb-3 ga-3;
}
.section-copy {
  @apply min-w-0;
}
.section-copy > span {
  @apply d-block;
  margin-bottom: 0.125rem;
  @include overline(var(--lad-mint-dark), var(--lad-font-size-micro));
}
h2 {
  @apply ma-0;
  @include heading(var(--lad-font-size-section), 1.2, -0.025em);
}
p {
  margin: 0.1875rem 0 0;
  @include body-copy(var(--lad-font-size-caption));
}
.section-action {
  @apply flex-shrink-0;
}
</style>
