<template>
  <Transition name="intro-fade">
    <div v-if="visible" class="app-intro" aria-live="polite" aria-label="Ladirchen wird geladen">
      <div class="intro-sun" aria-hidden="true" />
      <div class="intro-cloud intro-cloud-one" aria-hidden="true" />
      <div class="intro-cloud intro-cloud-two" aria-hidden="true" />
      <div class="intro-card">
        <div class="intro-mascot">
          <LadiMascot :score="5" :show-score="false" :size="142" />
        </div>
        <div class="intro-logo-row">
          <span class="intro-logo" aria-hidden="true"><img alt="" src="/ladirchen-logo.png"></span>
          <h1>Ladirchen</h1>
        </div>
        <p>Super-Ladi bringt die Familienwelt in Ordnung …</p>
        <div class="intro-dots" aria-hidden="true"><i /><i /><i /></div>
      </div>
    </div>
  </Transition>
</template>

<script lang="ts" setup>
import { onMounted, onUnmounted, ref } from 'vue';

import LadiMascot from '@/shared/components/LadiMascot.vue';

const visible = ref(true);
let timer: number | undefined;

onMounted(() => {
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  timer = window.setTimeout(() => { visible.value = false; }, reduceMotion ? 500 : 2200);
});

onUnmounted(() => {
  if (timer !== undefined) window.clearTimeout(timer);
});
</script>

<style scoped>
.app-intro {
  @apply position-fixed inset-0;
  z-index: 1000;
  @apply d-grid place-center overflow-hidden;
  background: linear-gradient(155deg, #dff7ff 0%, #f2ffe7 58%, #fff2c9 100%);
}
.intro-sun {
  width: 120px;
  height: 120px;
  @apply position-absolute;
  top: -30px;
  right: -25px;
  border-radius: 50%;
  background: #ffe078;
  box-shadow: 0 0 0 22px rgba(255, 224, 120, 0.2);
  animation: intro-sun-pulse 2.2s ease-in-out infinite;
}
.intro-cloud {
  width: 110px;
  height: 35px;
  @apply position-absolute;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.75);
  filter: blur(0.2px);
}
.intro-cloud::before,
.intro-cloud::after {
  content: "";
  @apply position-absolute;
  bottom: 5px;
  border-radius: 50%;
  background: inherit;
}
.intro-cloud::before {
  width: 46px;
  height: 46px;
  left: 16px;
}
.intro-cloud::after {
  width: 59px;
  height: 59px;
  right: 8px;
}
.intro-cloud-one {
  top: 16%;
  left: -24px;
  animation: intro-cloud-drift 4s ease-in-out infinite alternate;
}
.intro-cloud-two {
  right: -18px;
  bottom: 18%;
  transform: scale(0.75);
  animation: intro-cloud-drift 5s ease-in-out infinite alternate-reverse;
}
.intro-card {
  width: min(360px, calc(100vw - 40px));
  @apply position-relative;
  padding: 28px 26px 24px;
  @apply text-center;
  border: 3px solid rgba(64, 123, 101, 0.13);
  border-radius: 38px;
  background: rgba(255, 255, 255, 0.82);
  box-shadow:
    0 18px 0 rgba(69, 131, 105, 0.12),
    0 30px 55px rgba(54, 97, 83, 0.16);
  backdrop-filter: blur(10px);
  animation: intro-card-arrive 650ms cubic-bezier(0.18, 0.89, 0.32, 1.28);
}
.intro-mascot {
  width: 185px;
  height: 155px;
  @apply position-relative;
  margin: -16px auto 3px;
  @apply d-grid place-center;
}
.intro-logo-row {
  @apply d-flex align-center justify-center;
  gap: 9px;
}
.intro-logo {
  width: 40px;
  height: 40px;
  @apply position-relative overflow-hidden;
  border-radius: 11px;
  background: white;
}
.intro-logo img {
  width: 72px;
  height: 72px;
  @apply position-absolute;
  top: -5.5px;
  left: -16px;
  max-width: none;
}
.intro-logo-row h1 {
  @apply ma-0;
  color: #294d41;
  font-size: 35px;
  letter-spacing: -0.06em;
}
.intro-card > p {
  margin: 8px 0 0;
  color: #6b8077;
  font-size: 12px;
  @apply font-weight-bold;
}
.intro-dots {
  height: 18px;
  margin-top: 10px;
  @apply d-flex align-center justify-center;
  gap: 6px;
}
.intro-dots i {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #4db58b;
  animation: intro-dot 900ms ease-in-out infinite;
}
.intro-dots i:nth-child(2) {
  animation-delay: 150ms;
}
.intro-dots i:nth-child(3) {
  animation-delay: 300ms;
}
.intro-fade-leave-active {
  transition:
    opacity 380ms ease,
    transform 380ms ease;
}
.intro-fade-leave-to {
  opacity: 0;
  transform: scale(1.04);
}
@keyframes intro-card-arrive {
  from {
    opacity: 0;
    transform: translateY(35px) scale(0.82) rotate(-3deg);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes intro-dot {
  0%,
  100% {
    transform: translateY(3px) scale(0.75);
    opacity: 0.45;
  }
  50% {
    transform: translateY(-3px) scale(1.08);
    opacity: 1;
  }
}
@keyframes intro-sun-pulse {
  0%,
  100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.08);
  }
}
@keyframes intro-cloud-drift {
  from {
    translate: -8px 0;
  }
  to {
    translate: 14px 0;
  }
}
@media (prefers-reduced-motion: reduce) {
  .intro-card,
  .intro-sun,
  .intro-cloud,
  .intro-dots i {
    animation: none;
  }
}
</style>
