<template>
  <div class="completion-mark" :style="{ '--completion-size': `${size}px` }" aria-hidden="true">
    <span class="completion-orbit completion-orbit--one" />
    <span class="completion-orbit completion-orbit--two" />
    <span class="completion-core"><v-icon icon="i-mdi:check-bold" /></span>
    <i class="completion-spark completion-spark--one">✦</i>
    <i class="completion-spark completion-spark--two">✦</i>
    <i class="completion-spark completion-spark--three">●</i>
  </div>
</template>

<script lang="ts" setup>
withDefaults(defineProps<{ size?: number }>(), { size: 88 });
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.completion-mark {
  width: var(--completion-size);
  height: var(--completion-size);
  --uno: position-relative d-grid place-center flex-shrink-0;
  animation: completion-float 3s ease-in-out infinite;
}
.completion-core {
  width: 64%;
  height: 64%;
  --uno: position-relative d-grid place-center;
  z-index: 2;
  color: var(--lad-palette-white);
  border: 4px solid
    color-mix(in srgb, var(--lad-palette-white) 95%, transparent);
  border-radius: 22px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-mint-strong)
  );
  box-shadow:
    0 6px 0 var(--lad-palette-teal-700),
    0 12px 23px color-mix(in srgb, var(--lad-palette-teal-700) 20%, transparent);
  transform: rotate(-5deg);
  animation: completion-pop 2.8s ease-in-out infinite;
}
.completion-core :deep(.v-icon) {
  font-size: calc(var(--completion-size) * 0.31);
  filter: drop-shadow(
    0 2px 0 color-mix(in srgb, var(--lad-palette-teal-700) 18%, transparent)
  );
}
.completion-orbit {
  --uno: position-absolute inset-0;
  border: 2px solid color-mix(in srgb, var(--lad-palette-mint) 25%, transparent);
  border-radius: 32%;
  animation: completion-orbit 7s linear infinite;
}
.completion-orbit--two {
  inset: 9%;
  border-color: color-mix(in srgb, var(--lad-palette-blue) 20%, transparent);
  border-style: dashed;
  animation-duration: 5s;
  animation-direction: reverse;
}
.completion-spark {
  --uno: position-absolute;
  z-index: 3;
  color: var(--lad-palette-amber-450);
  font-style: normal;
  text-shadow: 0 0 8px
    color-mix(in srgb, var(--lad-palette-amber-250) 90%, transparent);
  animation: completion-spark 1.9s ease-in-out infinite;
}
.completion-spark--one {
  top: 2%;
  right: 8%;
  font-size: calc(var(--completion-size) * 0.18);
}
.completion-spark--two {
  bottom: 6%;
  left: 4%;
  color: var(--lad-palette-indigo-350);
  font-size: calc(var(--completion-size) * 0.13);
  animation-delay: -0.7s;
}
.completion-spark--three {
  top: 19%;
  left: 1%;
  color: var(--lad-palette-teal-400);
  font-size: calc(var(--completion-size) * 0.09);
  animation-delay: -1.2s;
}
@keyframes completion-float {
  0%,
  100% {
    transform: translateY(2px);
  }
  50% {
    transform: translateY(-5px);
  }
}
@keyframes completion-pop {
  0%,
  70%,
  100% {
    transform: rotate(-5deg) scale(1);
  }
  80% {
    transform: rotate(4deg) scale(1.08);
  }
  90% {
    transform: rotate(-2deg) scale(0.98);
  }
}
@keyframes completion-orbit {
  to {
    transform: rotate(360deg);
  }
}
@keyframes completion-spark {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.65) rotate(-10deg);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(16deg);
  }
}
@include reduced-motion {
  .completion-mark,
  .completion-core,
  .completion-orbit,
  .completion-spark {
    animation: none;
  }
}
</style>
