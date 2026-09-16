<template>
  <div
    class="animated-pet"
    :class="{ reacting: isReacting }"
    :style="petStyle"
    role="img"
    :aria-label="t('familyPets.animatedAria', { name: pet.name, kind: t(`familyPets.kinds.${pet.kind}`) })"
    :title="pet.name"
    @click="react"
  >
    <svg viewBox="0 0 96 104" aria-hidden="true" :class="`pet-kind--${pet.kind}`">
      <ellipse class="pet-shadow" cx="49" cy="96" rx="30" ry="7" />
      <path v-if="pet.kind === 'dog'" class="pet-tail dog-tail" d="M69 76c13 3 20-7 17-18" />
      <path v-else class="pet-tail" d="M70 75c20 0 19-25 6-24-9 1-5 13 2 10" />
      <g class="pet-body">
        <ellipse class="body" cx="48" cy="72" rx="29" ry="25" />
        <path class="belly" d="M38 58c-8 9-8 25 2 32 10 7 21 1 22-10 1-13-10-23-24-22Z" />
        <path class="leg left" d="M27 78q-5 10-1 17h14l-2-16Z" />
        <path class="leg right" d="M58 79q-1 9 3 16h14q3-9-6-18Z" />
      </g>
      <g class="pet-head">
        <template v-if="pet.kind === 'dog'">
          <path class="ear dog-ear dog-ear-left" d="M28 24C17 18 12 25 17 39c3 9 10 13 17 7Z" />
          <path class="ear dog-ear dog-ear-right" d="M67 23c11-6 17 1 13 15-3 10-10 14-18 8Z" />
          <path class="inner-ear dog-inner-ear-left" d="M25 28c-6-3-8 1-5 9 2 5 5 7 9 4Z" />
          <path class="inner-ear dog-inner-ear-right" d="M70 27c6-3 9 1 6 9-2 5-5 7-10 4Z" />
        </template>
        <template v-else>
          <path class="ear ear-left" d="M22 31 25 8l20 18Z" />
          <path class="ear ear-right" d="m53 25 19-17 3 25Z" />
          <path class="inner-ear left" d="m27 25 1-10 9 10Z" />
          <path class="inner-ear right" d="m61 24 8-9 2 13Z" />
        </template>
        <path class="head" d="M21 36q3-18 27-19 26 0 29 22 3 24-28 28-31-3-28-31Z" />
        <path class="face-patch" d="M45 21c10-4 24 2 27 14-4 8-12 9-21 5-8-4-11-12-6-19Z" />
        <g class="pet-eyes">
          <ellipse class="eye-white" cx="37" cy="41" rx="8" ry="10" />
          <ellipse class="eye-white" cx="59" cy="41" rx="8" ry="10" />
          <circle class="pupil" cx="39" cy="43" r="4" />
          <circle class="pupil" cx="57" cy="43" r="4" />
          <circle class="eye-shine" cx="40" cy="41" r="1.5" />
          <circle class="eye-shine" cx="58" cy="41" r="1.5" />
        </g>
        <template v-if="pet.kind === 'dog'">
          <ellipse class="dog-muzzle" cx="48" cy="55" rx="14" ry="10" />
          <ellipse class="dog-nose" cx="48" cy="51" rx="6" ry="4.5" />
          <path class="mouth" d="M48 55v4m0 0q-5 5-9 0m9 0q5 5 9 0" />
          <path class="dog-tongue" d="M45 61q3 7 6 0" />
        </template>
        <template v-else>
          <path class="nose" d="m43 52 5-3 5 3-5 5Z" />
          <path class="mouth" d="M48 56q-4 6-9 1m9-1q4 6 9 1" />
          <path class="whiskers" d="M38 53 17 49m21 9-22 2m42-7 21-4m-21 9 22 2" />
        </template>
        <g class="pet-paw">
          <ellipse cx="69" cy="64" rx="10" ry="13" />
          <path d="M64 61q5 4 10 0m-8 6q4 3 8 0" />
        </g>
      </g>
      <path class="collar" d="M27 62q21 11 43-2" />
      <circle class="tag" cx="49" cy="68" r="5" />
      <path class="tag-star" d="m49 65 1 2 2 .3-1.5 1.4.4 2.2-1.9-1-1.9 1 .4-2.2-1.5-1.4 2-.3Z" />
    </svg>
  </div>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import type { FamilyPet } from '@/domain/family/types';

const { t } = useI18n();

const props = withDefaults(defineProps<{
  pet: FamilyPet;
  size?: number;
}>(), {
  size: 58,
});
const emit = defineEmits<{ interact: [] }>();
const isReacting = ref(false);
let reactionTimer: number | undefined;
const react = () => {
  emit('interact');
  isReacting.value = false;
  window.clearTimeout(reactionTimer);
  requestAnimationFrame(() => { isReacting.value = true; });
  reactionTimer = window.setTimeout(() => { isReacting.value = false; }, 620);
};

const petStyle = computed(() => {
  const phase = props.pet.id.split('').reduce((sum, character) => sum + character.charCodeAt(0), 0) % 29;
  return {
    '--pet-color': props.pet.color,
    '--pet-phase': `${-phase / 10}s`,
    width: `${props.size}px`,
    height: `${Math.round(props.size * 1.08)}px`,
  };
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.animated-pet {
  @apply d-inline-grid;
  flex: 0 0 auto;
  @apply place-center;
  transform-origin: center bottom;
  animation: pet-hop 11s var(--pet-phase) ease-in-out infinite;
}
.animated-pet {
  pointer-events: auto;
  @apply cursor-pointer;
}
.animated-pet.reacting {
  animation: pet-tap 620ms cubic-bezier(0.2, 0.9, 0.2, 1);
}
svg {
  @apply w-100 h-100 overflow-visible;
  filter: drop-shadow(
    0 5px 3px color-mix(in srgb, var(--lad-palette-muted-750) 15%, transparent)
  );
}
.pet-shadow {
  fill: color-mix(in srgb, var(--lad-palette-text) 18%, transparent);
  animation: shadow-breathe 11s var(--pet-phase) ease-in-out infinite;
  transform-box: fill-box;
  transform-origin: center;
}
.body,
.head,
.leg,
.pet-paw {
  fill: var(--pet-color);
  stroke: var(--lad-palette-muted-750-2);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}
.belly {
  fill: color-mix(in srgb, var(--lad-palette-white) 30%, transparent);
}
.ear {
  fill: var(--pet-color);
  stroke: var(--lad-palette-muted-750-2);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}
.inner-ear {
  fill: var(--lad-palette-rose-250);
}
.dog-ear {
  transform-box: fill-box;
  transform-origin: top center;
}
.dog-ear-right {
  animation: ear-twitch 15s var(--pet-phase) ease-in-out infinite;
}
.face-patch {
  fill: color-mix(in srgb, var(--lad-palette-white) 18%, transparent);
}
.eye-white {
  fill: var(--lad-palette-surface);
  stroke: var(--lad-palette-muted-750-2);
  stroke-width: 2;
}
.pupil {
  fill: var(--lad-palette-muted-750);
}
.eye-shine {
  fill: var(--lad-palette-white);
}
.nose {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-muted-750-2);
  stroke-linejoin: round;
  stroke-width: 1.5;
}
.dog-muzzle {
  fill: var(--lad-palette-amber-200);
  stroke: var(--lad-palette-muted-750-2);
  stroke-width: 1.5;
}
.dog-nose {
  fill: var(--lad-palette-muted-750);
  stroke: var(--lad-palette-muted-750-2);
  stroke-width: 1.2;
}
.dog-tongue {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-muted-750-2);
  stroke-linejoin: round;
  stroke-width: 1.4;
}
.mouth,
.whiskers {
  fill: none;
  stroke: var(--lad-palette-muted-750-2);
  stroke-linecap: round;
  stroke-width: 2;
}
.pet-tail {
  fill: none;
  stroke: var(--pet-color);
  stroke-linecap: round;
  stroke-width: 10;
  transform-box: fill-box;
  transform-origin: left center;
  animation: tail-swish 10s var(--pet-phase) ease-in-out infinite;
}
.dog-tail {
  stroke-width: 9;
  animation: dog-tail-wag 2.2s var(--pet-phase) ease-in-out infinite;
}
.ear-right {
  transform-box: fill-box;
  transform-origin: left bottom;
  animation: ear-twitch 15s var(--pet-phase) ease-in-out infinite;
}
.pet-eyes {
  transform-box: fill-box;
  transform-origin: center;
  animation: pet-blink 12s var(--pet-phase) linear infinite;
}
.pet-paw {
  transform-box: fill-box;
  transform-origin: center bottom;
  animation: paw-wave 17s var(--pet-phase) ease-in-out infinite;
}
.pet-paw path {
  fill: none;
  stroke: var(--lad-palette-muted-750-2);
  stroke-linecap: round;
  stroke-width: 1.4;
}
.collar {
  fill: none;
  stroke: var(--lad-palette-teal-400);
  stroke-linecap: round;
  stroke-width: 4;
}
.tag {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 1.5;
}
.tag-star {
  fill: var(--lad-palette-amber-100);
}

@keyframes pet-hop {
  0%,
  86%,
  100% {
    transform: translateY(0) rotate(0);
  }
  90% {
    transform: translateY(-5px) rotate(-3deg);
  }
  94% {
    transform: translateY(0) rotate(3deg);
  }
}
@keyframes pet-tap {
  0% {
    transform: translateY(0) scale(1);
  }
  35% {
    transform: translateY(-12px) scale(1.08) rotate(-5deg);
  }
  65% {
    transform: translateY(1px) scale(0.97) rotate(4deg);
  }
  100% {
    transform: translateY(0) scale(1);
  }
}
@keyframes shadow-breathe {
  0%,
  86%,
  100% {
    transform: scaleX(1);
    opacity: 1;
  }
  90% {
    transform: scaleX(0.78);
    opacity: 0.65;
  }
}
@keyframes tail-swish {
  0%,
  68%,
  100% {
    transform: rotate(-5deg);
  }
  76% {
    transform: rotate(13deg);
  }
  84% {
    transform: rotate(-9deg);
  }
}
@keyframes dog-tail-wag {
  0%,
  100% {
    transform: rotate(-12deg);
  }
  50% {
    transform: rotate(17deg);
  }
}
@keyframes ear-twitch {
  0%,
  74%,
  80%,
  100% {
    transform: rotate(0);
  }
  76% {
    transform: rotate(10deg);
  }
  78% {
    transform: rotate(-5deg);
  }
}
@keyframes pet-blink {
  0%,
  44%,
  48%,
  100% {
    transform: scaleY(1);
  }
  46% {
    transform: scaleY(0.1);
  }
}
@keyframes paw-wave {
  0%,
  68%,
  82%,
  100% {
    transform: rotate(0);
  }
  71%,
  77% {
    transform: translateY(-5px) rotate(-18deg);
  }
  74%,
  80% {
    transform: translateY(-3px) rotate(12deg);
  }
}

@include reduced-motion {
  .animated-pet,
  .pet-shadow,
  .pet-tail,
  .ear-right,
  .dog-ear-right,
  .pet-eyes,
  .pet-paw {
    animation: none;
  }
}
</style>
