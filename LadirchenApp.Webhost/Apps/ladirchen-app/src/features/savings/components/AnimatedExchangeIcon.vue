<template>
  <span
    class="exchange-icon"
    role="img"
    :aria-label="t('savings.piggy.exchangeAria', { currency: currencyCode })"
  >
    <v-icon aria-hidden="true" class="exchange-arrows" icon="i-mdi:swap-horizontal-bold" />
    <span class="exchange-coin exchange-coin--ladi" aria-hidden="true"><LadirchenCoin small /></span>
    <span class="exchange-coin exchange-coin--family" aria-hidden="true">{{ currencyMark }}</span>
    <span class="exchange-spark" aria-hidden="true">✦</span>
  </span>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

import type { FamilyCurrency } from '@/domain/savings/types';
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';

const familyCurrencyMarks = {
  CHF: 'Fr',
  EUR: '€',
  HUF: 'Ft',
} as const satisfies Record<FamilyCurrency, string>;

const props = defineProps<{ currencyCode: FamilyCurrency }>();
const { t } = useI18n();

const currencyMark = computed(() => familyCurrencyMarks[props.currencyCode]);
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.exchange-icon {
  width: 2.625rem;
  height: 2.625rem;
  @apply position-relative d-inline-block overflow-visible flex-shrink-0;
  border-radius: 50%;
  background: var(--lad-palette-amber-100);
}
.exchange-arrows {
  @apply position-absolute;
  inset: 0.5rem;
  color: var(--lad-palette-teal-550);
  font-size: 1.625rem;
  transform-origin: center;
  animation: exchange-orbit 5.8s ease-in-out infinite;
}
.exchange-coin {
  @apply position-absolute d-grid place-center;
  z-index: 2;
  transform-origin: center;
}
.exchange-coin--ladi {
  left: -0.0625rem;
  bottom: 0;
  animation: coin-ladi-bob 2.8s ease-in-out infinite;
}
.exchange-coin--ladi :deep(.ladirchen-coin) {
  width: 1.5625rem;
  height: 1.5625rem;
}
.exchange-coin--family {
  width: 1.5rem;
  height: 1.5rem;
  top: 0;
  right: -0.0625rem;
  color: var(--lad-palette-teal-700);
  border: 0.125rem solid var(--lad-palette-mint);
  border-radius: 50%;
  background: var(--lad-palette-teal-150);
  box-shadow:
    inset 0 0 0 0.0625rem
      color-mix(in srgb, var(--lad-palette-white) 60%, transparent),
    0 0.125rem 0
      color-mix(in srgb, var(--lad-palette-teal-700) 16%, transparent);
  font-size: 0.4375rem;
  font-weight: var(--lad-font-weight-black);
  animation: coin-family-bob 2.8s -0.7s ease-in-out infinite;
}
.exchange-spark {
  @apply position-absolute;
  right: -0.1875rem;
  bottom: 0.1875rem;
  z-index: 3;
  color: var(--lad-palette-white);
  font-size: 0.5625rem;
  filter: drop-shadow(0 0 0.1875rem var(--lad-palette-yellow));
  transform-origin: center;
  animation: exchange-sparkle 1.9s -0.4s ease-in-out infinite;
}
@keyframes exchange-orbit {
  0%,
  100% {
    transform: rotate(-3deg);
  }
  50% {
    transform: rotate(7deg);
  }
}
@keyframes coin-ladi-bob {
  0%,
  100% {
    transform: translateY(0.0625rem) rotate(-2deg);
  }
  50% {
    transform: translateY(-0.125rem) rotate(2deg);
  }
}
@keyframes coin-family-bob {
  0%,
  100% {
    transform: translateY(-0.0625rem) rotate(2deg);
  }
  50% {
    transform: translateY(0.125rem) rotate(-2deg);
  }
}
@keyframes exchange-sparkle {
  0%,
  100% {
    opacity: 0.2;
    transform: scale(0.55) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.15) rotate(22deg);
  }
}
@include reduced-motion {
  .exchange-arrows,
  .exchange-coin,
  .exchange-spark {
    animation: none;
  }
}
</style>
