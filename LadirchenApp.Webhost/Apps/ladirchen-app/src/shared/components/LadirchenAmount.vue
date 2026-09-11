<template>
  <span class="ladirchen-amount" :class="{ 'ladirchen-amount--compact': compact }">
    <LadirchenCoin small />
    <span class="ladirchen-amount-value">{{ prefix }}{{ value }}{{ suffix }}</span>
  </span>
</template>

<script lang="ts" setup>
import LadirchenCoin from './LadirchenCoin.vue';

withDefaults(defineProps<{
  compact?: boolean;
  prefix?: string;
  suffix?: string;
  value: number | string;
}>(), {
  compact: false,
  prefix: '',
  suffix: '',
});
</script>

<style lang="scss" scoped>
.ladirchen-amount {
  min-width: 0;
  @apply d-inline-flex align-center;
  gap: 0.1875rem;
  line-height: 1;
  white-space: nowrap;
  vertical-align: middle;
}

.ladirchen-amount-value {
  @apply d-inline-flex align-center flex-shrink-0;
  align-self: stretch;
}

.ladirchen-amount--compact {
  gap: 0.125rem;
}

.ladirchen-amount--compact :deep(.ladirchen-coin) {
  width: 1.375rem;
  height: 1.375rem;
}
</style>
