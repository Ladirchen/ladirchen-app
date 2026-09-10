<template>
  <span class="ladirchen-coin" :class="{ small, animated }" aria-hidden="true">
    <span class="coin-star">★</span>
    <span class="coin-letter">L</span>
    <span class="coin-shine" />
  </span>
</template>

<script lang="ts" setup>
withDefaults(defineProps<{ small?: boolean; animated?: boolean }>(), { small: false, animated: false });
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.ladirchen-coin {
  width: 66px;
  height: 66px;
  @apply position-relative d-inline-grid place-center overflow-hidden flex-shrink-0;
  color: var(--lad-palette-amber-700);
  border: 4px solid var(--lad-palette-yellow);
  border-radius: 50%;
  background: radial-gradient(
    circle at 34% 27%,
    var(--lad-palette-amber-150) 0 8%,
    var(--lad-palette-amber-250) 25%,
    var(--lad-palette-amber-450) 72%,
    var(--lad-palette-amber-600) 100%
  );
  box-shadow:
    inset 0 0 0 3px
      color-mix(in srgb, var(--lad-palette-amber-150) 65%, transparent),
    inset 0 -6px 7px
      color-mix(in srgb, var(--lad-palette-amber-700) 18%, transparent),
    0 5px 0 var(--lad-palette-amber-600),
    0 9px 18px color-mix(in srgb, var(--lad-palette-amber-700) 25%, transparent);
  @apply font-weight-950;
}
.ladirchen-coin.small {
  width: 25px;
  height: 25px;
  border-width: 2px;
  box-shadow:
    inset 0 0 0 1px
      color-mix(in srgb, var(--lad-palette-amber-150) 65%, transparent),
    inset 0 -2px 3px
      color-mix(in srgb, var(--lad-palette-amber-700) 18%, transparent),
    0 2px 0 var(--lad-palette-amber-600);
}
.ladirchen-coin.animated {
  transform-origin: center;
  animation: coin-glow 2.8s ease-in-out infinite;
}
.ladirchen-coin.animated::after {
  content: "";
  width: 22%;
  height: 155%;
  @apply position-absolute;
  top: -28%;
  left: -48%;
  z-index: 4;
  transform: rotate(24deg);
  border-radius: 50%;
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--lad-palette-white) 95%, transparent),
    transparent
  );
  filter: blur(0.4px);
  animation: coin-sweep 2.8s ease-in-out infinite;
  @apply pointer-events-none;
}
.coin-letter {
  @apply position-relative;
  z-index: 2;
  font-size: 29px;
  line-height: 1;
  text-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-palette-amber-150) 80%, transparent);
}
.small .coin-letter {
  font-size: 12px;
}
.coin-star {
  @apply position-absolute;
  top: 5px;
  right: 8px;
  color: var(--lad-palette-amber-150);
  font-size: 11px;
  text-shadow: 0 1px 1px var(--lad-palette-amber-600);
}
.small .coin-star {
  top: 1px;
  right: 2px;
  font-size: 5px;
}
.coin-shine {
  width: 22px;
  height: 7px;
  @apply position-absolute;
  top: 10px;
  left: 8px;
  transform: rotate(-35deg);
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-palette-white) 60%, transparent);
  filter: blur(1px);
}
.small .coin-shine {
  width: 8px;
  height: 3px;
  top: 3px;
  left: 3px;
}
.animated .coin-shine {
  animation: coin-glint 2.8s ease-in-out infinite;
}
.animated .coin-star {
  animation: coin-star-twinkle 2.8s ease-in-out infinite;
}
@keyframes coin-glint {
  0%,
  28%,
  100% {
    opacity: 0.18;
    transform: translate(-5px, 4px) rotate(-35deg) scale(0.7);
  }
  50% {
    opacity: 1;
    transform: translate(5px, -3px) rotate(-35deg) scale(1.18);
  }
  68% {
    opacity: 0.32;
    transform: translate(9px, -5px) rotate(-35deg) scale(0.8);
  }
}
@keyframes coin-star-twinkle {
  0%,
  30%,
  100% {
    opacity: 0.55;
    transform: scale(0.8) rotate(-8deg);
  }
  52% {
    opacity: 1;
    transform: scale(1.25) rotate(8deg);
  }
}
@keyframes coin-sweep {
  0%,
  22% {
    left: -48%;
    opacity: 0;
  }
  34% {
    opacity: 1;
  }
  64% {
    left: 126%;
    opacity: 0.9;
  }
  72%,
  100% {
    left: 126%;
    opacity: 0;
  }
}
@keyframes coin-glow {
  0%,
  25%,
  100% {
    filter: drop-shadow(0 0 0 transparent);
  }
  52% {
    filter: drop-shadow(
      0 0 7px color-mix(in srgb, var(--lad-palette-amber-450) 80%, transparent)
    );
  }
}
@include reduced-motion {
  .ladirchen-coin.animated,
  .ladirchen-coin.animated::after,
  .animated .coin-shine,
  .animated .coin-star {
    animation: none;
  }
}
</style>
