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

<style scoped>
.ladirchen-coin {
  width: 66px;
  height: 66px;
  @apply position-relative d-inline-grid place-center overflow-hidden flex-shrink-0;
  color: #69460a;
  border: 4px solid #ffc84f;
  border-radius: 50%;
  background: radial-gradient(
    circle at 34% 27%,
    #fff4b5 0 8%,
    #ffd76c 25%,
    #f4a91f 72%,
    #c8780a 100%
  );
  box-shadow:
    inset 0 0 0 3px rgba(255, 246, 182, 0.65),
    inset 0 -6px 7px rgba(128, 70, 0, 0.18),
    0 5px 0 #b76b08,
    0 9px 18px rgba(116, 74, 9, 0.24);
  @apply font-weight-950;
}
.ladirchen-coin.small {
  width: 25px;
  height: 25px;
  border-width: 2px;
  box-shadow:
    inset 0 0 0 1px rgba(255, 246, 182, 0.65),
    inset 0 -2px 3px rgba(128, 70, 0, 0.18),
    0 2px 0 #b76b08;
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
    rgba(255, 255, 255, 0.96),
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
  text-shadow: 0 2px 0 rgba(255, 245, 178, 0.8);
}
.small .coin-letter {
  font-size: 12px;
}
.coin-star {
  @apply position-absolute;
  top: 5px;
  right: 8px;
  color: #fff2a2;
  font-size: 11px;
  text-shadow: 0 1px 1px #ba7307;
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
  background: rgba(255, 255, 255, 0.62);
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
    filter: drop-shadow(0 0 0 rgba(255, 210, 64, 0));
  }
  52% {
    filter: drop-shadow(0 0 7px rgba(255, 205, 54, 0.78));
  }
}
@media (prefers-reduced-motion: reduce) {
  .ladirchen-coin.animated,
  .ladirchen-coin.animated::after,
  .animated .coin-shine,
  .animated .coin-star {
    animation: none;
  }
}
</style>
