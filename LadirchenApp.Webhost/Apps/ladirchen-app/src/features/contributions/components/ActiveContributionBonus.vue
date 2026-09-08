<template>
  <div class="active-contribution-bonus">
    <span class="bonus-rocket" aria-hidden="true"><v-icon icon="mdi-rocket-launch" size="19" /></span>
    <span class="bonus-copy"><small>Bonus für diese Aufgabe</small><strong>{{ multiplier }}-fache Belohnung aktiv</strong></span>
    <span class="bonus-factor">×{{ multiplier }}</span>
    <PromotionCountdown class="bonus-countdown" :deadline="deadline" />
  </div>
</template>

<script lang="ts" setup>
import PromotionCountdown from './PromotionCountdown.vue';

defineProps<{
  deadline: string;
  multiplier: number;
}>();
</script>

<style scoped>
.active-contribution-bonus {
  min-height: 51px;
  padding: 6px 8px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 7px;
  color: #285f70;
  border: 2px solid rgba(77, 171, 183, 0.3);
  border-radius: 16px;
  background:
    radial-gradient(
      circle at 88% 10%,
      rgba(255, 255, 255, 0.9),
      transparent 25%
    ),
    linear-gradient(125deg, #dff6ff, #e2f8ee 50%, #fff0bc);
  box-shadow:
    0 4px 0 rgba(59, 139, 146, 0.2),
    0 9px 18px rgba(55, 125, 135, 0.13);
}
.active-contribution-bonus::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 3px;
  right: 7px;
  color: #df9e23;
  font-size: 10px;
  animation: bonus-spark 1.7s ease-in-out infinite;
}
.bonus-rocket {
  width: 36px;
  height: 36px;
  @apply d-grid place-center flex-shrink-0;
  color: #fff8b3;
  border: 2px solid #fff;
  border-radius: 12px;
  background: linear-gradient(145deg, #66bfe0, #667ed4 65%, #a26ccc);
  box-shadow: 0 3px 0 #5269ae;
  animation: bonus-launch 1.9s ease-in-out infinite;
}
.bonus-copy {
  @apply flex-grow-1 min-w-0;
}
.bonus-copy small,
.bonus-copy strong {
  @apply d-block;
}
.bonus-copy small {
  color: #317b69;
  font-size: 7px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.035em;
}
.bonus-copy strong {
  margin-top: 1px;
  font-size: 10px;
  line-height: 1.15;
}
.bonus-factor {
  min-width: 45px;
  padding: 5px 7px;
  @apply d-grid place-center flex-shrink-0;
  color: #fff;
  border: 2px solid rgba(255, 255, 255, 0.9);
  border-radius: 13px;
  background: linear-gradient(145deg, #4ebba0, #3189a8);
  box-shadow: 0 3px 0 #287489;
  font-size: 18px;
  font-weight: 950;
  line-height: 1;
  animation: bonus-factor-pulse 1.9s ease-in-out infinite;
}
.bonus-countdown {
  flex: 0 0 auto;
}
@keyframes bonus-launch {
  0%,
  68%,
  100% {
    transform: translate(0, 0) rotate(-4deg);
  }
  78% {
    transform: translate(3px, -4px) rotate(5deg) scale(1.08);
  }
  88% {
    transform: translate(-1px, 1px) rotate(-2deg);
  }
}
@keyframes bonus-factor-pulse {
  0%,
  100% {
    transform: scale(1) rotate(1deg);
  }
  50% {
    transform: scale(1.08) rotate(-2deg);
  }
}
@keyframes bonus-spark {
  0%,
  100% {
    opacity: 0.25;
    transform: scale(0.6) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.25) rotate(24deg);
  }
}
@media (max-width: 430px) {
  .active-contribution-bonus {
    align-items: flex-start;
    flex-wrap: wrap;
  }
  .bonus-copy {
    min-width: 120px;
  }
  .bonus-countdown {
    margin-left: 43px;
  }
}
@media (prefers-reduced-motion: reduce) {
  .bonus-rocket,
  .bonus-factor,
  .active-contribution-bonus::after {
    animation: none;
  }
}
</style>
