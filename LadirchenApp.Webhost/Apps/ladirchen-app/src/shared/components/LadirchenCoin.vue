<template>
  <span class="ladirchen-coin" :class="{ small, animated }" aria-hidden="true">
    <img :src="LADIRCHEN_COIN_SPRITE_URL" alt="">
  </span>
</template>

<script lang="ts" setup>
import { LADIRCHEN_COIN_SPRITE_URL } from "@/shared/assets/currency-assets";

withDefaults(defineProps<{ small?: boolean; animated?: boolean }>(), { small: false, animated: false });
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.ladirchen-coin {
  width: 66px;
  height: 66px;
  --uno: position-relative d-inline-grid place-center flex-shrink-0;
}
.ladirchen-coin img {
  width: 100%;
  height: 100%;
  --uno: d-block;
  object-fit: contain;
  filter: drop-shadow(
    0 5px 4px color-mix(in srgb, var(--lad-palette-amber-700) 24%, transparent)
  );
}
.ladirchen-coin.small {
  width: 25px;
  height: 25px;
}
.ladirchen-coin.small img {
  filter: drop-shadow(
    0 2px 1px color-mix(in srgb, var(--lad-palette-amber-700) 24%, transparent)
  );
}
.ladirchen-coin.animated {
  transform-origin: center;
  animation: coin-glow 2.8s ease-in-out infinite;
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
  .ladirchen-coin.animated {
    animation: none;
  }
}
</style>
