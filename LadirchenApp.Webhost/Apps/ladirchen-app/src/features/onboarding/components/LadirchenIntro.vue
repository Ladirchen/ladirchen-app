<template>
  <Transition name="intro-fade" @after-leave="emit('finished')">
    <div v-if="visible" class="app-intro" aria-live="polite" :aria-label="t('onboarding.intro.loadingAria')">
      <div class="intro-sun" aria-hidden="true" />
      <div class="intro-cloud intro-cloud-one" aria-hidden="true" />
      <div class="intro-cloud intro-cloud-two" aria-hidden="true" />
      <div class="intro-card">
        <div class="intro-mascot">
          <LadiMascot :score="5" :show-score="false" :size="142" />
        </div>
        <div class="intro-logo-row">
          <span class="intro-logo" aria-hidden="true"><img alt="" src="/ladirchen-logo.png"></span>
          <h1>{{ t('common.appName') }}</h1>
        </div>
        <p>{{ t('onboarding.intro.message') }}</p>
        <div class="intro-dots" aria-hidden="true"><i /><i /><i /></div>
      </div>
    </div>
  </Transition>
</template>

<script lang="ts" setup>
import { onMounted, onUnmounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import LadiMascot from '@/shared/components/LadiMascot.vue';

const emit = defineEmits<{ finished: [] }>();
const { t } = useI18n();
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

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.app-intro {
  @apply position-fixed inset-0;
  z-index: 1000;
  @apply d-grid place-center overflow-hidden;
  background: linear-gradient(
    155deg,
    var(--lad-palette-background) 0%,
    var(--lad-palette-background) 58%,
    var(--lad-palette-amber-100) 100%
  );
}
.intro-sun {
  width: 120px;
  height: 120px;
  @apply position-absolute;
  top: -30px;
  right: -25px;
  border-radius: 50%;
  background: var(--lad-palette-amber-250);
  box-shadow: 0 0 0 22px
    color-mix(in srgb, var(--lad-palette-amber-250) 20%, transparent);
  animation: intro-sun-pulse 2.2s ease-in-out infinite;
}
.intro-cloud {
  width: 110px;
  height: 35px;
  @apply position-absolute;
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-palette-white) 75%, transparent);
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
  border: 3px solid
    color-mix(in srgb, var(--lad-palette-teal-600) 12%, transparent);
  border-radius: 38px;
  background: color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  box-shadow:
    0 18px 0 color-mix(in srgb, var(--lad-palette-teal-600) 12%, transparent),
    0 30px 55px
      color-mix(in srgb, var(--lad-palette-muted-700) 15%, transparent);
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
.intro-mascot :deep(.ladi-wrap) {
  width: var(--ladi-size);
  justify-content: center;
}
.intro-logo-row {
  @apply d-flex align-end justify-center;
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
  color: var(--lad-palette-text);
  font-size: 2.1875rem;
  line-height: 0.9;
  letter-spacing: -0.06em;
  transform: translateY(1px);
}
.intro-card > p {
  margin: 8px 0 0;
  color: var(--lad-palette-muted);
  font-size: 0.75rem;
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
  background: var(--lad-palette-mint);
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
@include reduced-motion {
  .intro-card,
  .intro-sun,
  .intro-cloud,
  .intro-dots i {
    animation: none;
  }
}
</style>
