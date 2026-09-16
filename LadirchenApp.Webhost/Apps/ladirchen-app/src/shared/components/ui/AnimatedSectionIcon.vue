<template>
  <svg class="animated-section-icon" :class="`animated-section-icon--${variant}`" viewBox="0 0 64 64" aria-hidden="true">
    <g v-if="variant === 'contributions'" class="section-main contribution-hands">
      <path class="icon-fill icon-fill--soft" d="M8 37c5-6 12-6 18 0l6 6-9 10C15 49 10 44 8 37Z" />
      <path class="icon-fill icon-fill--soft" d="M56 37c-5-6-12-6-18 0l-6 6 9 10c8-4 13-9 15-16Z" />
      <path class="icon-stroke" d="M9 37c6-5 11-4 17 1l6 5 6-5c6-5 11-6 17-1M14 43l9 9m27-9-9 9" />
      <path class="accent-fill contribution-heart" d="M32 38C19 30 18 20 25 17c4-2 7 1 7 5 1-4 4-7 8-5 8 4 5 14-8 21Z" />
    </g>

    <g v-else-if="variant === 'shop'" class="section-main shop-gift">
      <path class="icon-fill" d="M12 29h40v26H12Z" />
      <path class="icon-fill icon-fill--soft" d="M9 22h46v10H9Z" />
      <path class="icon-stroke" d="M12 32h40v23H12Zm-3-10h46v10H9Z" />
      <path class="accent-fill" d="M29 22h7v33h-7Z" />
      <path class="gift-ribbon" d="M32 21c-9 0-14-3-13-8 1-5 8-5 13 8Zm0 0c9 0 14-3 13-8-1-5-8-5-13 8Z" />
    </g>

    <g v-else class="section-main family-group">
      <circle class="icon-fill family-head family-head--left" cx="19" cy="27" r="8" />
      <circle class="icon-fill family-head family-head--right" cx="45" cy="27" r="8" />
      <circle class="accent-fill family-head family-head--center" cx="32" cy="21" r="10" />
      <path class="icon-fill icon-fill--soft" d="M6 52c1-11 7-17 16-17 5 0 8 2 10 5 3-3 6-5 11-5 9 0 15 6 16 17Z" />
      <path class="icon-stroke" d="M7 52c1-10 6-16 15-16 5 0 8 2 10 6 3-4 6-6 11-6 9 0 14 6 15 16" />
      <path class="family-heart" d="M32 37c-7-4-8-9-4-11 2-1 4 0 4 3 1-3 3-4 5-3 4 2 2 7-5 11Z" />
    </g>

    <path class="section-spark section-spark--one" d="m53 8 2 4 4 2-4 2-2 4-2-4-4-2 4-2Z" />
    <circle class="section-spark section-spark--two" cx="9" cy="18" r="2.5" />
  </svg>
</template>

<script lang="ts" setup>
export type AnimatedSectionIconVariant = 'contributions' | 'family' | 'shop';

defineProps<{ variant: AnimatedSectionIconVariant }>();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.animated-section-icon {
  width: 45px;
  height: 45px;
  @apply d-block overflow-visible;
}
.section-main {
  transform-box: fill-box;
  transform-origin: center bottom;
  animation: section-bob 3.4s ease-in-out infinite;
}
.icon-fill {
  fill: var(--lad-palette-background);
}
.icon-fill--soft {
  fill: var(--lad-palette-background);
}
.icon-stroke {
  fill: none;
  stroke: var(--lad-palette-mint-strong);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.accent-fill {
  fill: var(--lad-palette-amber-450);
  stroke: var(--lad-palette-amber-550);
  stroke-linejoin: round;
  stroke-width: 1.5;
}
.section-spark {
  fill: var(--lad-palette-yellow);
  transform-box: fill-box;
  transform-origin: center;
  animation: section-spark 2.5s ease-in-out infinite;
}
.section-spark--two {
  fill: var(--lad-palette-teal-400);
  animation-delay: -1.15s;
}
.contribution-heart {
  transform-box: fill-box;
  transform-origin: center;
  animation: section-pulse 2.3s ease-in-out infinite;
}
.animated-section-icon--shop .icon-fill {
  fill: var(--lad-palette-amber-150);
}
.animated-section-icon--shop .icon-fill--soft {
  fill: var(--lad-palette-amber-100);
}
.animated-section-icon--shop .icon-stroke {
  stroke: var(--lad-palette-amber-500);
}
.animated-section-icon--shop .accent-fill {
  fill: var(--lad-palette-amber-500);
  stroke: none;
}
.gift-ribbon {
  fill: var(--lad-palette-amber-100);
  stroke: var(--lad-palette-amber-500);
  stroke-linejoin: round;
  stroke-width: 2.5;
  transform-box: fill-box;
  transform-origin: center bottom;
  animation: ribbon-wiggle 3s ease-in-out infinite;
}
.family-head {
  stroke: var(--lad-palette-mint-strong);
  stroke-width: 2;
}
.family-head--center {
  stroke: var(--lad-palette-amber-550);
}
.family-heart {
  fill: var(--lad-palette-red-300);
  stroke: var(--lad-palette-red-500);
  stroke-width: 1.2;
  transform-box: fill-box;
  transform-origin: center;
  animation: section-pulse 2.6s ease-in-out infinite;
}
@keyframes section-bob {
  0%,
  100% {
    transform: translateY(1px) rotate(-1.5deg);
  }
  50% {
    transform: translateY(-2.5px) rotate(1.5deg);
  }
}
@keyframes section-pulse {
  0%,
  100% {
    transform: scale(0.92);
  }
  50% {
    transform: scale(1.1);
  }
}
@keyframes section-spark {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.7) rotate(-8deg);
  }
  50% {
    opacity: 1;
    transform: scale(1.15) rotate(8deg);
  }
}
@keyframes ribbon-wiggle {
  0%,
  100% {
    transform: rotate(-2deg);
  }
  50% {
    transform: rotate(3deg);
  }
}
@include reduced-motion {
  .section-main,
  .section-spark,
  .contribution-heart,
  .gift-ribbon,
  .family-heart {
    animation: none;
  }
}
</style>
