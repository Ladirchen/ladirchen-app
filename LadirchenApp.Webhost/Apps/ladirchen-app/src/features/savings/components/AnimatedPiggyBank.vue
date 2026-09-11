<template>
  <div
    class="animated-piggy"
    :class="{ receiving, 'play-hop': !receiving && playMode === 'hop', 'play-wiggle': !receiving && playMode === 'wiggle' }"
    :style="piggyStyle"
    role="img"
    :aria-label="t('savings.piggy.animatedAria')"
  >
    <svg viewBox="0 0 122 104" aria-hidden="true">
      <defs>
        <linearGradient id="piggyBody" x1="0" x2="1" y1="0" y2="1">
          <stop offset="0" stop-color="var(--lad-palette-rose-250)" />
          <stop offset=".55" stop-color="var(--lad-palette-rose-300)" />
          <stop offset="1" stop-color="var(--lad-palette-rose-300)" />
        </linearGradient>
        <linearGradient id="piggySnout" x1="0" x2="1">
          <stop offset="0" stop-color="var(--lad-palette-rose-200)" />
          <stop offset="1" stop-color="var(--lad-palette-rose-250)" />
        </linearGradient>
      </defs>
      <ellipse class="piggy-shadow" cx="62" cy="96" rx="41" ry="7" />
      <g class="piggy-body">
        <path class="tail" d="M22 52c-16-10-17 12-5 11 10-1 6-13 0-7" />
        <ellipse class="body" cx="65" cy="61" rx="45" ry="32" />
        <path class="belly-glow" d="M39 47c-9 14-4 34 12 41 13 6 30 0 36-13-18 8-40-2-48-28Z" />
        <path class="ear" d="m76 34 8-22 17 25Z" />
        <path class="inner-ear" d="m84 30 3-10 8 13Z" />
        <path class="leg left" d="M39 82v15h15l4-13Z" />
        <path class="leg right" d="M80 83v14h15l1-18Z" />
        <path class="coin-slot" d="M51 31q14-5 27 0" />
        <circle class="eye" cx="91" cy="48" r="4" />
        <circle class="eye-shine" cx="92" cy="47" r="1.3" />
        <ellipse class="snout" cx="105" cy="64" rx="15" ry="12" />
        <ellipse class="nostril" cx="101" cy="64" rx="2.3" ry="3" />
        <ellipse class="nostril" cx="110" cy="64" rx="2.3" ry="3" />
        <path class="smile" d="M94 72q7 8 15 1" />
        <path class="star" d="m46 50 4 8 9 1-7 6 2 9-8-5-8 5 2-9-7-6 9-1Z" />
      </g>
    </svg>
    <LadirchenCoin v-if="receiving" class="piggy-coin" small />
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';

const { t } = useI18n();

const props = withDefaults(defineProps<{
  receiving?: boolean;
  size?: number;
}>(), {
  receiving: false,
  size: 52,
});

const playMode = ref<'hop' | 'wiggle' | null>(null);
let playTimer: number | undefined;
let resetTimer: number | undefined;
const schedulePlay = () => {
  playTimer = window.setTimeout(() => {
    playMode.value = Math.random() > .5 ? 'hop' : 'wiggle';
    resetTimer = window.setTimeout(() => {
      playMode.value = null;
      schedulePlay();
    }, 950);
  }, 4000 + Math.random() * 7000);
};
onMounted(schedulePlay);
onUnmounted(() => {
  if (playTimer !== undefined) window.clearTimeout(playTimer);
  if (resetTimer !== undefined) window.clearTimeout(resetTimer);
});

const piggyStyle = computed(() => ({
  width: `${props.size}px`,
  height: `${Math.round(props.size * .86)}px`,
}));
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.animated-piggy {
  @apply d-inline-grid;
  flex: 0 0 auto;
  @apply place-center;
  transform-origin: center bottom;
  animation: piggy-breathe 8s ease-in-out infinite;
}
.animated-piggy.receiving {
  animation:
    piggy-celebrate 800ms cubic-bezier(0.2, 0.9, 0.2, 1),
    piggy-breathe 8s 800ms ease-in-out infinite;
}
.animated-piggy.play-hop {
  animation: piggy-hop 900ms cubic-bezier(0.2, 0.85, 0.25, 1);
}
.animated-piggy.play-wiggle {
  animation: piggy-wiggle 900ms ease-in-out;
}
svg {
  @apply w-100 h-100 overflow-visible;
  filter: drop-shadow(
    0 5px 3px
      color-mix(in srgb, var(--lad-palette-muted-750-2) 18%, transparent)
  );
}
.body,
.leg {
  fill: url(#piggyBody);
  stroke: var(--lad-palette-pink-650);
  stroke-linejoin: round;
  stroke-width: 3;
}
.belly-glow {
  fill: color-mix(in srgb, var(--lad-palette-white) 15%, transparent);
}
.ear {
  fill: var(--lad-palette-rose-300);
  stroke: var(--lad-palette-pink-650);
  stroke-linejoin: round;
  stroke-width: 3;
  transform-box: fill-box;
  transform-origin: left bottom;
  animation: piggy-ear 14s ease-in-out infinite;
}
.inner-ear {
  fill: var(--lad-palette-red-100);
}
.snout {
  fill: url(#piggySnout);
  stroke: var(--lad-palette-pink-650);
  stroke-width: 3;
}
.nostril,
.eye {
  fill: var(--lad-palette-pink-650);
}
.eye-shine {
  fill: var(--lad-palette-white);
}
.eye {
  transform-box: fill-box;
  transform-origin: center;
  animation: piggy-blink 12s linear infinite;
}
.coin-slot {
  fill: none;
  stroke: var(--lad-palette-pink-650);
  stroke-linecap: round;
  stroke-width: 5;
}
.smile {
  fill: none;
  stroke: var(--lad-palette-pink-650);
  stroke-linecap: round;
  stroke-width: 2;
}
.tail {
  fill: none;
  stroke: var(--lad-palette-rose-300);
  stroke-linecap: round;
  stroke-width: 6;
  transform-box: fill-box;
  transform-origin: right center;
  animation: piggy-tail 10s ease-in-out infinite;
}
.star {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-linejoin: round;
  stroke-width: 1.5;
  animation: star-glow 7s ease-in-out infinite;
  transform-box: fill-box;
  transform-origin: center;
}
.piggy-shadow {
  fill: color-mix(in srgb, var(--lad-palette-muted-750) 15%, transparent);
  animation: piggy-shadow 8s ease-in-out infinite;
  transform-box: fill-box;
  transform-origin: center;
}
.piggy-coin {
  @apply position-absolute;
  top: -0.25rem;
  left: calc(50% - 0.78125rem);
  z-index: 3;
  opacity: 0;
}
.receiving .piggy-coin {
  animation: coin-drop 800ms 120ms cubic-bezier(0.3, 0.9, 0.2, 1) both;
}

@keyframes piggy-breathe {
  0%,
  100% {
    transform: translateY(0) rotate(-1deg);
  }
  50% {
    transform: translateY(-2px) rotate(1deg);
  }
}
@keyframes piggy-celebrate {
  0% {
    transform: scale(1);
  }
  42% {
    transform: translateY(-8px) scale(1.15) rotate(-7deg);
  }
  72% {
    transform: translateY(1px) scale(0.94) rotate(5deg);
  }
  100% {
    transform: none;
  }
}
@keyframes piggy-hop {
  0%,
  100% {
    transform: translateY(0) scale(1);
  }
  35% {
    transform: translateY(-7px) scale(1.06, 0.95) rotate(-4deg);
  }
  62% {
    transform: translateY(1px) scale(0.96, 1.05) rotate(2deg);
  }
}
@keyframes piggy-wiggle {
  0%,
  100% {
    transform: rotate(0);
  }
  20% {
    transform: rotate(-7deg) scale(1.04);
  }
  42% {
    transform: rotate(7deg) scale(1.04);
  }
  64% {
    transform: rotate(-5deg);
  }
  82% {
    transform: rotate(3deg);
  }
}
@keyframes piggy-tail {
  0%,
  66%,
  100% {
    transform: rotate(-5deg);
  }
  76% {
    transform: rotate(12deg);
  }
  86% {
    transform: rotate(-9deg);
  }
}
@keyframes piggy-ear {
  0%,
  70%,
  76%,
  100% {
    transform: rotate(0);
  }
  73% {
    transform: rotate(10deg);
  }
}
@keyframes piggy-blink {
  0%,
  48%,
  52%,
  100% {
    transform: scaleY(1);
  }
  50% {
    transform: scaleY(0.12);
  }
}
@keyframes star-glow {
  0%,
  100% {
    transform: scale(0.95) rotate(-3deg);
  }
  50% {
    transform: scale(1.08) rotate(3deg);
  }
}
@keyframes piggy-shadow {
  0%,
  100% {
    transform: scaleX(1);
  }
  50% {
    transform: scaleX(0.92);
  }
}
@keyframes coin-drop {
  0% {
    opacity: 0;
    transform: translateY(-26px) rotateY(0);
  }
  35% {
    opacity: 1;
  }
  82% {
    opacity: 1;
    transform: translateY(15px) rotateY(540deg);
  }
  100% {
    opacity: 0;
    transform: translateY(21px) rotateY(720deg);
  }
}

@include reduced-motion {
  .animated-piggy,
  .animated-piggy.receiving,
  .animated-piggy.play-hop,
  .animated-piggy.play-wiggle,
  .ear,
  .eye,
  .tail,
  .star,
  .piggy-shadow,
  .receiving .piggy-coin {
    animation: none;
  }
}
</style>
