<template>
  <span v-if="kind === 'reward'" class="contribution-meta-icon contribution-meta-icon--reward" aria-hidden="true">
    <LadirchenCoin animated small />
  </span>
  <svg v-else :class="['contribution-meta-icon', `contribution-meta-icon--${kind}`]" viewBox="0 0 32 32" aria-hidden="true">
    <template v-if="kind === 'time'">
      <circle class="icon-disc" cx="16" cy="16" r="12" />
      <circle class="icon-line" cx="16" cy="16" r="8" />
      <path class="clock-hand clock-hand--hour" d="M16 16V11" />
      <path class="clock-hand clock-hand--minute" d="M16 16l4 2" />
      <circle class="icon-dot" cx="16" cy="16" r="1.4" />
    </template>
    <template v-else>
      <circle class="icon-disc" cx="16" cy="16" r="12" />
      <path class="energy-bolt" d="M18.2 5.5 9.5 17h5.3l-1 9.5L22.5 14h-5.2z" />
      <path class="energy-glow" d="M18.2 5.5 9.5 17h5.3l-1 9.5L22.5 14h-5.2z" />
    </template>
  </svg>
</template>

<script lang="ts" setup>
import LadirchenCoin from "@/shared/components/LadirchenCoin.vue";

defineProps<{ kind: "time" | "reward" | "energy" }>();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.contribution-meta-icon {
  width: 30px;
  height: 30px;
  display: block;
  overflow: visible;
}
.icon-disc {
  fill: color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
  stroke: currentColor;
  stroke-width: 1.6;
}
.icon-line,
.clock-hand {
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2;
}
.contribution-meta-icon--time {
  color: var(--lad-palette-blue-strong);
}
.contribution-meta-icon--time .icon-disc {
  fill: var(--lad-palette-background);
}
.clock-hand {
  transform-origin: 16px 16px;
}
.clock-hand--minute {
  animation: clock-tick 3.6s steps(6, end) infinite;
}
.icon-dot {
  fill: currentColor;
}
.contribution-meta-icon--reward {
  display: grid;
  place-items: center;
}
.contribution-meta-icon--reward :deep(.ladirchen-coin) {
  transform: scale(1.08);
}
.contribution-meta-icon--energy {
  color: var(--lad-palette-amber-550);
}
.contribution-meta-icon--energy .icon-disc {
  fill: var(--lad-palette-amber-100);
  stroke: var(--lad-palette-amber-450);
}
.energy-bolt {
  fill: var(--lad-palette-amber-450);
  stroke: var(--lad-palette-amber-550);
  stroke-linejoin: round;
  stroke-width: 1.2;
  animation: energy-hop 2s ease-in-out infinite;
}
.energy-glow {
  fill: none;
  stroke: var(--lad-palette-amber-250);
  stroke-width: 3;
  opacity: 0;
  animation: energy-glow 2s ease-in-out infinite;
}
@keyframes clock-tick {
  to {
    transform: rotate(360deg);
  }
}
@keyframes energy-hop {
  0%,
  65%,
  100% {
    transform: translateY(0) scale(1);
  }
  76% {
    transform: translateY(-2px) scale(1.08);
  }
  86% {
    transform: translateY(1px) scale(0.98);
  }
}
@keyframes energy-glow {
  0%,
  62%,
  100% {
    opacity: 0;
  }
  76% {
    opacity: 0.5;
  }
}
@include reduced-motion {
  .clock-hand--minute,
  .energy-bolt,
  .energy-glow {
    animation: none;
  }
}
</style>
