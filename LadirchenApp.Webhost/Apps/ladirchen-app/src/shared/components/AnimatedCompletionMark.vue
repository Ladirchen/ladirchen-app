<template>
  <div class="completion-mark" :style="{ '--completion-size': `${size}px` }" aria-hidden="true">
    <span class="completion-orbit completion-orbit--one" />
    <span class="completion-orbit completion-orbit--two" />
    <span class="completion-core"><v-icon icon="mdi-check-bold" /></span>
    <i class="completion-spark completion-spark--one">✦</i>
    <i class="completion-spark completion-spark--two">✦</i>
    <i class="completion-spark completion-spark--three">●</i>
  </div>
</template>

<script lang="ts" setup>
withDefaults(defineProps<{ size?: number }>(), { size: 88 });
</script>

<style scoped>
.completion-mark {
  width: var(--completion-size);
  height: var(--completion-size);
  @apply position-relative d-grid place-center flex-shrink-0;
  animation: completion-float 3s ease-in-out infinite;
}
.completion-core {
  width: 64%;
  height: 64%;
  @apply position-relative d-grid place-center;
  z-index: 2;
  color: #fff;
  border: 4px solid rgba(255, 255, 255, 0.94);
  border-radius: 22px;
  background: linear-gradient(145deg, #62c9a0, #318d73);
  box-shadow:
    0 6px 0 #28745c,
    0 12px 23px rgba(43, 125, 96, 0.2);
  transform: rotate(-5deg);
  animation: completion-pop 2.8s ease-in-out infinite;
}
.completion-core :deep(.v-icon) {
  font-size: calc(var(--completion-size) * 0.31);
  filter: drop-shadow(0 2px 0 rgba(25, 91, 69, 0.18));
}
.completion-orbit {
  @apply position-absolute inset-0;
  border: 2px solid rgba(81, 185, 147, 0.25);
  border-radius: 32%;
  animation: completion-orbit 7s linear infinite;
}
.completion-orbit--two {
  inset: 9%;
  border-color: rgba(91, 158, 210, 0.22);
  border-style: dashed;
  animation-duration: 5s;
  animation-direction: reverse;
}
.completion-spark {
  @apply position-absolute;
  z-index: 3;
  color: #e7ad2f;
  font-style: normal;
  text-shadow: 0 0 8px rgba(255, 228, 132, 0.9);
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
  color: #709ed3;
  font-size: calc(var(--completion-size) * 0.13);
  animation-delay: -0.7s;
}
.completion-spark--three {
  top: 19%;
  left: 1%;
  color: #73c5a6;
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
@media (prefers-reduced-motion: reduce) {
  .completion-mark,
  .completion-core,
  .completion-orbit,
  .completion-spark {
    animation: none;
  }
}
</style>
