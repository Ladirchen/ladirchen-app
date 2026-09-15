<template>
  <div class="active-contribution-bonus">
    <span class="bonus-rocket" aria-hidden="true"><v-icon icon="i-mdi:rocket-launch" size="19" /></span>
    <span class="bonus-copy"><small>{{ t('contributions.activeBonus.label') }}</small><strong>{{ t('contributions.activeBonus.multiplier', { value: multiplier }) }}</strong></span>
    <span class="bonus-factor">×{{ multiplier }}</span>
    <PromotionCountdown class="bonus-countdown" :deadline="deadline" />
  </div>
</template>

<script lang="ts" setup>
import { useI18n } from 'vue-i18n';

import PromotionCountdown from './PromotionCountdown.vue';

const { t } = useI18n();

defineProps<{
  deadline: string;
  multiplier: number;
}>();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.active-contribution-bonus {
  min-height: 51px;
  padding: 6px 8px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 7px;
  color: var(--lad-color-primary-deep);
  border: 2px solid
    color-mix(in srgb, var(--lad-color-info-muted) 30%, transparent);
  border-radius: 16px;
  background:
    radial-gradient(
      circle at 88% 10%,
      color-mix(in srgb, var(--lad-surface-raised) 90%, transparent),
      transparent 25%
    ),
    linear-gradient(
      125deg,
      var(--lad-surface-soft),
      var(--lad-surface-soft) 50%,
      var(--lad-color-reward-pale)
    );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-color-info-shadow) 20%, transparent),
    0 9px 18px color-mix(in srgb, var(--lad-color-info-deep) 12%, transparent);
}
.active-contribution-bonus::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 3px;
  right: 7px;
  color: var(--lad-color-reward-accent);
  font-size: rem(10);
  animation: bonus-spark 1.7s ease-in-out infinite;
}
.bonus-rocket {
  width: 36px;
  height: 36px;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-color-reward-pale);
  border: 2px solid var(--lad-border-on-accent);
  border-radius: 12px;
  background: linear-gradient(
    145deg,
    var(--lad-color-info-subtle),
    var(--lad-color-bonus-info) 65%,
    var(--lad-color-bonus-highlight)
  );
  box-shadow: 0 3px 0 var(--lad-color-info-shadow);
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
  color: var(--lad-color-primary-supporting);
  font-size: rem(7);
  font-weight: var(--lad-font-weight-heavy);
  text-transform: uppercase;
  letter-spacing: 0.035em;
}
.bonus-copy strong {
  margin-top: 1px;
  font-size: rem(10);
  line-height: 1.15;
}
.bonus-factor {
  min-width: 45px;
  padding: 5px 7px;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-text-inverse);
  border: 2px solid
    color-mix(in srgb, var(--lad-border-on-accent) 90%, transparent);
  border-radius: 13px;
  background: linear-gradient(
    145deg,
    var(--lad-color-primary),
    var(--lad-color-info-shadow)
  );
  box-shadow: 0 3px 0 var(--lad-color-info-deep);
  font-size: rem(18);
  font-weight: var(--lad-font-weight-black);
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
@include respond-down(phone) {
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
@include reduced-motion {
  .bonus-rocket,
  .bonus-factor,
  .active-contribution-bonus::after {
    animation: none;
  }
}
</style>
