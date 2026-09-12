<template>
  <span class="countdown" :class="{ expired: remainingMilliseconds <= 0, urgent: remainingMilliseconds > 0 && remainingMilliseconds < 3_600_000 }">
    <v-icon size="14">mdi-timer-sand</v-icon>
    <span>{{ label }}</span>
  </span>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';

import { promotionDeadline } from '../domain/promotions';

const props = defineProps<{ deadline: string }>();
const now = ref(new Date());
let timer: ReturnType<typeof window.setInterval> | undefined;

const remainingMilliseconds = computed(() =>
  Math.max(0, promotionDeadline(props.deadline, now.value).getTime() - now.value.getTime()),
);
const label = computed(() => {
  if (remainingMilliseconds.value <= 0) return 'Abgelaufen';
  const totalSeconds = Math.floor(remainingMilliseconds.value / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  return `Noch ${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
});

onMounted(() => {
  timer = window.setInterval(() => { now.value = new Date(); }, 1000);
});
onBeforeUnmount(() => {
  if (timer) window.clearInterval(timer);
});
</script>

<style scoped>
.countdown {
  width: fit-content;
  padding: 5px 8px;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  color: #7d570d;
  border: 1px solid rgba(199, 128, 23, 0.2);
  border-radius: 9px;
  background: rgba(255, 246, 211, 0.92);
  font-size: 10px;
  font-variant-numeric: tabular-nums;
  font-weight: 900;
  letter-spacing: 0.01em;
}
.countdown.urgent {
  color: #a1422f;
  background: #fff0e6;
  animation: countdown-pulse 1.6s ease-in-out infinite;
}
.countdown.expired {
  color: var(--lad-muted);
  background: #f1f3f2;
}
@keyframes countdown-pulse {
  50% {
    box-shadow: 0 0 0 4px rgba(235, 113, 77, 0.1);
  }
}
@media (prefers-reduced-motion: reduce) {
  .countdown {
    animation: none !important;
  }
}
</style>
