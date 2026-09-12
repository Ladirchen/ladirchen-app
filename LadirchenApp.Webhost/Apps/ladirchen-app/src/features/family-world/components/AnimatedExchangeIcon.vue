<template>
  <svg
    class="exchange-icon"
    viewBox="0 0 52 52"
    role="img"
    :aria-label="`Wechselkurs zwischen Ladirchen und ${currencyCode}`"
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

const props = defineProps<{ currencyCode: string }>();

const currencyMark = computed(() => ({ CHF: 'Fr', EUR: '€', HUF: 'Ft' })[props.currencyCode] ?? '¤');
</script>

<style scoped>
.exchange-icon {
  width: 42px;
  height: 42px;
  display: block;
  overflow: visible;
}
.exchange-backdrop {
  fill: #fff0c3;
}
.exchange-arrows {
  fill: none;
  stroke: #32a77d;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2.5;
  transform-origin: center;
  animation: exchange-orbit 5.8s ease-in-out infinite;
}
.exchange-coin {
  transform-box: fill-box;
  transform-origin: center;
  filter: drop-shadow(0 2px 1px rgba(87, 61, 26, 0.18));
}
.exchange-coin > circle:first-child {
  stroke-width: 2;
}
.exchange-coin .coin-ring {
  fill: none;
  stroke: rgba(255, 255, 255, 0.62);
  stroke-width: 1.2;
}
.exchange-coin text {
  text-anchor: middle;
  font-family: inherit;
  font-size: 9px;
  font-weight: 950;
}
.exchange-coin--ladi {
  animation: coin-ladi-bob 2.8s ease-in-out infinite;
}
.exchange-coin--ladi > circle:first-child {
  fill: #ffc94f;
  stroke: #dd941f;
}
.exchange-coin--ladi text {
  fill: #7d520c;
  font-size: 12px;
}
.exchange-coin--family {
  animation: coin-family-bob 2.8s -0.7s ease-in-out infinite;
}
.exchange-coin--family > circle:first-child {
  fill: #d9f4e8;
  stroke: #42b68a;
}
.exchange-coin--family text {
  fill: #247759;
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
  fill: #fff;
  transform-box: fill-box;
  transform-origin: center;
  filter: drop-shadow(0 0 3px #ffd760);
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
@media (prefers-reduced-motion: reduce) {
  .exchange-arrows,
  .exchange-coin,
  .coin-glint,
  .exchange-spark {
    animation: none;
  }
}
</style>
