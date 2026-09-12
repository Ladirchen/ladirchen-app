<template>
  <svg class="animated-wish-icon" viewBox="0 0 64 64" aria-hidden="true">
    <path class="wish-orbit" d="M11 39c5 12 27 16 41 3" />
    <g class="wish-jar">
      <path class="jar-lid" d="M21 17h24l2 6H19Z" />
      <path class="jar-body" d="M20 23h26l-2 27c-1 5-5 8-10 8h-3c-5 0-9-3-10-8Z" />
      <path class="jar-glow" d="M25 27h4l-1 23c0 2 1 4 3 5-5 0-7-2-7-7Z" />
      <path class="wish-heart" d="M33 43c-8-5-9-10-5-12 3-2 5 0 5 2 1-2 3-4 6-2 4 3 2 8-6 12Z" />
    </g>
    <g class="wish-star wish-star--large">
      <path d="m49 9 2 5 5 2-5 2-2 5-2-5-5-2 5-2Z" />
    </g>
    <circle class="wish-spark wish-spark--one" cx="13" cy="23" r="2.5" />
    <circle class="wish-spark wish-spark--two" cx="53" cy="34" r="2" />
  </svg>
</template>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.animated-wish-icon {
  width: 45px;
  height: 45px;
  @apply d-block overflow-visible;
}
.wish-orbit {
  fill: none;
  stroke: var(--lad-palette-blue-250);
  stroke-dasharray: 4 5;
  stroke-linecap: round;
  stroke-width: 2;
  animation: wish-orbit 5s linear infinite;
}
.wish-jar {
  transform-box: fill-box;
  transform-origin: center bottom;
  animation: wish-jar-bob 3.2s ease-in-out infinite;
}
.jar-lid {
  fill: var(--lad-palette-blue);
  stroke: var(--lad-palette-blue-strong);
  stroke-linejoin: round;
  stroke-width: 2;
}
.jar-body {
  fill: var(--lad-palette-background);
  stroke: var(--lad-palette-blue);
  stroke-linejoin: round;
  stroke-width: 2.5;
}
.jar-glow {
  fill: color-mix(in srgb, var(--lad-palette-white) 75%, transparent);
}
.wish-heart {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-550);
  stroke-linejoin: round;
  stroke-width: 1.5;
  transform-box: fill-box;
  transform-origin: center;
  animation: wish-heart-pulse 2.4s ease-in-out infinite;
}
.wish-star {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-500);
  stroke-linejoin: round;
  stroke-width: 1.5;
  transform-box: fill-box;
  transform-origin: center;
}
.wish-star--large {
  animation: wish-star-twinkle 2.1s ease-in-out infinite;
}
.wish-spark {
  fill: var(--lad-palette-teal-400);
  transform-box: fill-box;
  transform-origin: center;
  animation: wish-spark 2.8s ease-in-out infinite;
}
.wish-spark--two {
  animation-delay: -1.3s;
}
@keyframes wish-jar-bob {
  0%,
  100% {
    transform: translateY(1px) rotate(-2deg);
  }
  50% {
    transform: translateY(-3px) rotate(2deg);
  }
}
@keyframes wish-heart-pulse {
  0%,
  100% {
    transform: scale(0.92);
  }
  50% {
    transform: scale(1.12);
  }
}
@keyframes wish-star-twinkle {
  0%,
  100% {
    opacity: 0.65;
    transform: scale(0.8) rotate(-8deg);
  }
  50% {
    opacity: 1;
    transform: scale(1.18) rotate(8deg);
  }
}
@keyframes wish-spark {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.7);
  }
  45% {
    opacity: 1;
    transform: scale(1.2);
  }
}
@keyframes wish-orbit {
  to {
    stroke-dashoffset: -36;
  }
}
@include reduced-motion {
  .wish-orbit,
  .wish-jar,
  .wish-heart,
  .wish-star,
  .wish-spark {
    animation: none;
  }
}
</style>
