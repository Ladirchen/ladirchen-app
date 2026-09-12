<template>
  <span class="countdown" :class="{ expired: remainingMilliseconds <= 0, urgent: remainingMilliseconds > 0 && remainingMilliseconds < 3_600_000 }">
    <v-icon aria-hidden="true" size="14">i-mdi:timer-sand</v-icon>
    <span>{{ label }}</span>
  </span>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import { remainingPromotionMilliseconds } from '@/domain/contributions/promotions';
import { useFamilyWorldStore } from '@/stores/family-world';

const props = defineProps<{ deadline: string }>();
const { t } = useI18n();
const store = useFamilyWorldStore();
const now = ref(new Date());
let timer: ReturnType<typeof window.setInterval> | undefined;

const remainingMilliseconds = computed(() =>
  remainingPromotionMilliseconds(props.deadline, store.familyTimeZone, now.value),
);
const label = computed(() => {
  if (remainingMilliseconds.value <= 0) return t('contributions.countdown.expired');
  const totalSeconds = Math.floor(remainingMilliseconds.value / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  const time = `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  return t('contributions.countdown.remaining', { time });
});

onMounted(() => {
  timer = window.setInterval(() => { now.value = new Date(); }, 1000);
});
onBeforeUnmount(() => {
  if (timer) window.clearInterval(timer);
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.countdown {
  width: fit-content;
  padding: 5px 8px;
  @apply d-inline-flex align-center ga-1;
  color: var(--lad-color-reward-strong);
  border: 1px solid
    color-mix(in srgb, var(--lad-color-reward-shadow) 20%, transparent);
  border-radius: 9px;
  background: color-mix(in srgb, var(--lad-color-reward-soft) 90%, transparent);
  font-size: 0.625rem;
  font-variant-numeric: tabular-nums;
  @apply font-weight-black;
  letter-spacing: 0.01em;
}
.countdown.urgent {
  color: var(--lad-color-danger-strong);
  background: var(--lad-color-reward-soft);
  animation: countdown-pulse 1.6s ease-in-out infinite;
}
.countdown.expired {
  color: var(--lad-muted);
  background: var(--lad-surface-soft);
}
@keyframes countdown-pulse {
  50% {
    box-shadow: 0 0 0 4px
      color-mix(in srgb, var(--lad-color-accent-warm) 10%, transparent);
  }
}
@include reduced-motion {
  .countdown {
    animation: none !important;
  }
}
</style>
