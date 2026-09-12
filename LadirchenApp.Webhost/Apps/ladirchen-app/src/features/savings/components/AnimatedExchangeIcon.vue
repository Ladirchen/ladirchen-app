<template>
  <svg
    class="exchange-icon"
    viewBox="0 0 52 52"
    role="img"
    :aria-label="t('savings.piggy.exchangeAria', { currency: currencyCode })"
  >
    <circle class="exchange-backdrop" cx="26" cy="26" r="25" />

    <g class="exchange-arrows" aria-hidden="true">
      <path d="M10 20C13 10 24 5 34 9" />
      <path d="m32 5 7 5-7 5" />
      <path d="M42 32c-3 10-14 15-24 11" />
      <path d="m20 47-7-5 7-5" />
    </g>

    <g class="exchange-coin exchange-coin--ladi">
      <circle cx="19" cy="29" r="11" />
      <circle class="coin-ring" cx="19" cy="29" r="8" />
      <text x="19" y="33">L</text>
      <path class="coin-glint" d="m14 23 2-2m-4 6 1-1" />
    </g>

    <g class="exchange-coin exchange-coin--family">
      <circle cx="34" cy="23" r="10" />
      <circle class="coin-ring" cx="34" cy="23" r="7" />
      <text x="34" y="26">{{ currencyMark }}</text>
      <path class="coin-glint" d="m30 18 2-2" />
    </g>

    <path class="exchange-spark" d="m42 17 1.4 3.2 3.2 1.4-3.2 1.4-1.4 3.2-1.4-3.2-3.2-1.4 3.2-1.4Z" />
  </svg>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps<{ currencyCode: string }>();
const { t } = useI18n();

const currencyMark = computed(() => ({ CHF: 'Fr', EUR: '€', HUF: 'Ft' })[props.currencyCode] ?? '¤');
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.exchange-icon {
  width: 42px;
  height: 42px;
  @apply d-block overflow-visible;
}
.exchange-backdrop {
  fill: var(--lad-palette-amber-100);
}
.exchange-arrows {
  fill: none;
  stroke: var(--lad-palette-teal-550);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2.5;
  transform-origin: center;
  animation: exchange-orbit 5.8s ease-in-out infinite;
}
.exchange-coin {
  transform-box: fill-box;
  transform-origin: center;
  filter: drop-shadow(
    0 2px 1px color-mix(in srgb, var(--lad-palette-orange-750) 18%, transparent)
  );
}
.exchange-coin > circle:first-child {
  stroke-width: 2;
}
.exchange-coin .coin-ring {
  fill: none;
  stroke: color-mix(in srgb, var(--lad-palette-white) 60%, transparent);
  stroke-width: 1.2;
}
.exchange-coin text {
  text-anchor: middle;
  font-family: inherit;
  font-size: 9px;
  font-weight: var(--lad-font-weight-black);
}
.exchange-coin--ladi {
  animation: coin-ladi-bob 2.8s ease-in-out infinite;
}
.exchange-coin--ladi > circle:first-child {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-500);
}
.exchange-coin--ladi text {
  fill: var(--lad-palette-amber-700);
  font-size: 12px;
}
.exchange-coin--family {
  animation: coin-family-bob 2.8s -0.7s ease-in-out infinite;
}
.exchange-coin--family > circle:first-child {
  fill: var(--lad-palette-teal-150);
  stroke: var(--lad-palette-mint);
}
.exchange-coin--family text {
  fill: var(--lad-palette-teal-700);
  font-size: 7px;
}
.coin-glint {
  fill: none;
  stroke: white;
  stroke-linecap: round;
  stroke-width: 1.8;
  animation: coin-shine 2.2s ease-in-out infinite;
}
.exchange-spark {
  fill: var(--lad-palette-white);
  transform-box: fill-box;
  transform-origin: center;
  filter: drop-shadow(0 0 3px var(--lad-palette-yellow));
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
    transform: translateY(1px) rotate(-2deg);
  }
  50% {
    transform: translateY(-2px) rotate(2deg);
  }
}
@keyframes coin-family-bob {
  0%,
  100% {
    transform: translateY(-1px) rotate(2deg);
  }
  50% {
    transform: translateY(2px) rotate(-2deg);
  }
}
@keyframes coin-shine {
  0%,
  55%,
  100% {
    opacity: 0.28;
  }
  70% {
    opacity: 1;
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
  .coin-glint,
  .exchange-spark {
    animation: none;
  }
}
</style>
