<template>
  <section :aria-label="t('world.progress.aria')">
    <div class="house-stage-card mb-3">
      <div class="house-stage-visual" aria-hidden="true">
        <span class="house-halo" />
        <span class="house-spark house-spark--one">✦</span>
        <span class="house-spark house-spark--two">✦</span>
        <span class="house-stage-icon">{{ currentStage.icon }}</span>
      </div>
      <div class="house-stage-copy">
        <span class="section-kicker">{{ t('world.progress.currentStage') }}</span>
        <div class="house-stage-title">
          <strong>{{ t(currentStage.nameKey) }}</strong>
          <span>{{ t('world.progress.level', { value: houseLevel + 1 }) }}</span>
        </div>
        <p>{{ t('world.progress.completedWeeks', { count: completedWeeks }) }}</p>
        <div class="stage-track" :aria-label="t('world.progress.stageAria', { current: houseLevel + 1, total: houseStages.length })">
          <span
            v-for="stage in houseStages"
            :key="stage.level"
            :class="{ reached: stage.level <= houseLevel, current: stage.level === houseLevel }"
          />
        </div>
        <b>{{ t(houseLevel < houseStages.length - 1 ? 'world.progress.next' : 'world.progress.maximum') }}</b>
      </div>
    </div>

    <div v-if="showEvolution !== false" class="house-evolution">
      <div class="evolution-heading">
        <div class="evolution-icon" aria-hidden="true"><v-icon size="22">i-mdi:home-switch</v-icon></div>
        <div><span class="section-kicker">{{ t('world.progress.weeklyReview') }}</span><strong>{{ t('world.progress.evolutionTitle') }}</strong></div>
      </div>
      <div class="evolution-steps mt-3">
        <div><span>{{ minimumHouseEnergyPercent }} %</span><p>{{ t('world.progress.steps.daily', { value: minimumHouseEnergyPercent }) }}</p></div>
        <div><span>{{ t('world.progress.sevenDays') }}</span><p>{{ t('world.progress.steps.week') }}</p></div>
        <div><span>{{ t('world.progress.underThreshold', { value: minimumHouseEnergyPercent }) }}</span><p>{{ t('world.progress.steps.fallback') }}</p></div>
      </div>
      <p class="evolution-note mt-3">{{ t('world.progress.note') }}</p>
    </div>
  </section>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

import { DEFAULT_HOUSE_STAGE, HOUSE_STAGES } from '@/domain/house/catalog';
import { MINIMUM_HOUSE_ENERGY_PERCENT } from '@/domain/contributions/energy';
import type { HouseStageLevel } from '@/domain/house';

const props = defineProps<{
  completedWeeks: number;
  houseLevel: HouseStageLevel;
  showEvolution?: boolean;
}>();

const { t } = useI18n();

const houseStages = HOUSE_STAGES;
const minimumHouseEnergyPercent = MINIMUM_HOUSE_ENERGY_PERCENT;
const currentStage = computed(() => houseStages[props.houseLevel] ?? DEFAULT_HOUSE_STAGE);
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.house-stage-card {
  min-height: 126px;
  padding: 14px;
  @apply d-flex align-center;
  gap: 13px;
  @apply overflow-hidden;
  @include raised-surface(
    color-mix(in srgb, var(--lad-color-reward-border) 20%, transparent),
    color-mix(in srgb, var(--lad-color-reward-shadow) 8%, transparent),
    1.25rem,
    0.25rem,
    0.0625rem
  );
  background: linear-gradient(
    135deg,
    var(--lad-surface),
    var(--lad-color-reward-soft) 58%,
    var(--lad-surface-soft)
  );
}
.house-stage-visual {
  width: 88px;
  height: 88px;
  @apply position-relative d-grid place-center;
  flex: 0 0 88px;
}
.house-halo {
  width: 72px;
  height: 72px;
  @apply position-absolute;
  border-radius: 50%;
  background: repeating-conic-gradient(
    from 0deg,
    color-mix(in srgb, var(--lad-color-reward-border) 30%, transparent) 0 12deg,
    transparent 12deg 25deg
  );
  animation: house-halo-spin 12s linear infinite;
}
.house-stage-icon {
  @apply position-relative;
  z-index: 2;
  font-size: 3.25rem;
  line-height: 1;
  filter: drop-shadow(
    0 6px 5px
      color-mix(in srgb, var(--lad-color-accent-warm-deep) 18%, transparent)
  );
  animation: house-stage-bounce 3.2s ease-in-out infinite;
}
.house-spark {
  @apply position-absolute;
  z-index: 3;
  color: var(--lad-color-reward-accent);
  font-size: 0.875rem;
  animation: house-spark 1.8s ease-in-out infinite;
}
.house-spark--one {
  top: 7px;
  right: 7px;
}
.house-spark--two {
  bottom: 7px;
  left: 5px;
  animation-delay: -0.9s;
}
.house-stage-copy {
  @apply min-w-0;
  flex: 1;
}
.section-kicker {
  @include overline(
    var(--lad-color-primary-strong),
    var(--lad-font-size-micro)
  );
}
.house-stage-title {
  margin: 2px 0;
  @apply d-flex align-center flex-wrap;
  gap: 5px;
}
.house-stage-title strong {
  font-size: 0.875rem;
}
.house-stage-title span {
  padding: 3px 7px;
  color: var(--lad-color-reward-strong);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-color-reward-pale);
  font-size: 0.5rem;
  @apply font-weight-black;
}
.house-stage-copy > p {
  @apply ma-0;
  @include body-copy(0.5625rem, 1.35);
}
.house-stage-copy > b {
  @apply d-block;
  margin-top: 5px;
  color: var(--lad-color-reward-strong);
  font-size: 0.5rem;
}
.stage-track {
  @apply mt-2 d-grid;
  grid-template-columns: repeat(5, 1fr);
  @apply ga-1;
}
.stage-track span {
  height: 6px;
  border-radius: var(--lad-radius-pill);
  background: color-mix(
    in srgb,
    var(--lad-color-primary-supporting) 12%,
    transparent
  );
}
.stage-track span.reached {
  background: var(--lad-color-primary);
}
.stage-track span.current {
  box-shadow: 0 0 0 3px
    color-mix(in srgb, var(--lad-color-primary) 15%, transparent);
  animation: stage-pulse 1.8s ease-in-out infinite;
}
.house-evolution {
  padding: 14px;
  border: 1px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 18%, transparent);
  border-radius: 20px;
  background: color-mix(in srgb, var(--lad-surface-raised) 80%, transparent);
}
.evolution-heading {
  @apply d-flex align-center;
  gap: 10px;
}
.evolution-heading > div:last-child span,
.evolution-heading > div:last-child strong {
  @apply d-block;
}
.evolution-heading > div:last-child strong {
  margin-top: 1px;
  font-size: 0.8125rem;
}
.evolution-icon {
  width: 40px;
  height: 40px;
  @apply d-grid place-center;
  flex: 0 0 40px;
  color: var(--lad-color-primary-strong);
  border-radius: 13px;
  background: var(--lad-surface-soft);
}
.evolution-steps {
  @apply d-grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 6px;
}
.evolution-steps > div {
  @apply pa-2;
  border-radius: 12px;
  background: var(--lad-surface);
}
.evolution-steps span {
  color: var(--lad-color-primary-strong);
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-black);
}
.evolution-steps p {
  margin: 3px 0 0;
  color: var(--lad-neutral-success);
  font-size: 0.46875rem;
  line-height: 1.35;
}
.evolution-note {
  margin-bottom: 0;
  color: var(--lad-muted);
  font-size: 0.5rem;
  line-height: 1.4;
}
@keyframes house-spark {
  0%,
  100% {
    opacity: 0.3;
    transform: scale(0.7) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(20deg);
  }
}
@keyframes house-stage-bounce {
  0%,
  100% {
    transform: translateY(2px) rotate(-2deg);
  }
  50% {
    transform: translateY(-6px) rotate(2deg);
  }
}
@keyframes house-halo-spin {
  to {
    transform: rotate(360deg);
  }
}
@keyframes stage-pulse {
  0%,
  100% {
    transform: scaleY(1);
  }
  50% {
    transform: scaleY(1.5);
  }
}
@include respond-down(mobile) {
  .house-stage-visual {
    width: 72px;
    flex-basis: 72px;
  }
  .evolution-steps {
    grid-template-columns: 1fr;
  }
}
@include reduced-motion {
  .house-halo,
  .house-stage-icon,
  .house-spark,
  .stage-track span.current {
    animation: none;
  }
}
</style>
