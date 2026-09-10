<template>
  <v-card class="interest-simulator pa-4 mb-5" elevation="0" rounded="xl">
    <div class="d-flex align-start justify-space-between ga-3">
      <div>
        <p class="eyebrow mb-1">{{ t('savings.simulator.eyebrow') }}</p>
        <h3>{{ t('savings.simulator.title') }}</h3>
        <p class="text-caption text-medium-emphasis mt-1">{{ t('savings.simulator.description') }}</p>
      </div>
      <v-chip color="info" size="x-small" variant="tonal">{{ t('savings.simulator.demo') }}</v-chip>
    </div>

    <div class="simulator-preview mt-4">
      <LadiMascot :score="simulatedRating" :show-score="false" :size="68" />
      <div class="flex-grow-1">
        <span>{{ t('savings.simulator.preview') }}</span>
        <strong>{{ t(simulatedLadiStage.nameKey) }}</strong>
        <small>{{ t(simulatedLadiStage.descriptionKey) }}</small>
      </div>
      <div class="simulator-rate"><strong>{{ formatRate(simulatedInterestRate) }} %</strong><span>{{ t('savings.simulator.weeklyRate') }}</span></div>
    </div>

    <label class="simulator-label mt-4">{{ t('savings.simulator.completion') }} <strong>{{ simulatedCompletion }} %</strong></label>
    <v-slider v-model="simulatedCompletion" color="primary" hide-details max="100" min="0" step="10" />
    <label class="simulator-label">{{ t('savings.simulator.rating') }} <strong>{{ formatRate(simulatedRating) }} / 5</strong></label>
    <v-slider v-model="simulatedRating" color="warning" hide-details max="5" min="0" step="0.1" />
    <label class="simulator-label">{{ t('savings.simulator.streak') }} <strong>{{ t('savings.simulator.days', { count: simulatedStreak }) }}</strong></label>
    <v-slider v-model="simulatedStreak" color="info" hide-details max="30" min="0" step="1" />

    <div class="payout-estimate mt-4">
      <div><span>{{ t('savings.simulator.nextPayout') }}</span><strong>+{{ simulatedWeeklyInterest }} L</strong></div>
      <small>{{ t('savings.simulator.calculation', { saved: store.totalSaved }) }}</small>
    </div>

    <div class="simulator-actions mt-3">
      <v-btn rounded="lg" size="small" variant="text" @click="resetSimulation">{{ t('common.reset') }}</v-btn>
      <v-btn color="primary" :disabled="simulatedWeeklyInterest <= 0" rounded="lg" size="small" variant="flat" @click="simulatePayout">{{ t('savings.simulator.simulate') }}</v-btn>
    </div>

    <AnimatePresence>
      <motion.div
        v-if="payoutVisible"
        :key="payoutVersion"
        class="payout-success mt-4"
        :initial="{ opacity: 0, y: 14, scale: .92 }"
        :animate="{ opacity: 1, y: 0, scale: 1 }"
        :exit="{ opacity: 0, y: -8, scale: .96 }"
        :transition="{ type: 'spring', stiffness: 360, damping: 24 }"
        aria-live="polite"
      >
        <div class="payout-visual" aria-hidden="true">
          <motion.div
            class="payout-coin"
            :initial="{ x: -58, y: -18, rotate: -160, scale: .55, opacity: 1 }"
            :animate="{ x: [-58, -4, 42], y: [-18, -13, 8], rotate: [-160, 160, 420], scale: [.55, .72, .12], opacity: [1, 1, 0] }"
            :transition="{ duration: 1.05, times: [0, .58, 1], ease: [.22, .8, .26, 1], delay: .12 }"
          ><LadirchenCoin /></motion.div>
          <AnimatedPiggyBank receiving :size="65" />
        </div>
        <div><strong>{{ t('savings.simulator.payout', { amount: lastPayout }) }}</strong><span>{{ t('savings.simulator.payoutDescription') }}</span></div>
      </motion.div>
    </AnimatePresence>
  </v-card>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { AnimatePresence, motion } from 'motion-v';
import { useI18n } from 'vue-i18n';

import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import LadiMascot from '@/shared/components/LadiMascot.vue';

import AnimatedPiggyBank from './AnimatedPiggyBank.vue';
import { getLadiStage } from '@/domain/ladi';
import { familyParticipationInterestStrategy } from '@/domain/savings-interest';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const { locale, t } = useI18n();
const simulatedCompletion = ref(0);
const simulatedRating = ref(0);
const simulatedStreak = ref(0);
const payoutVisible = ref(false);
const payoutVersion = ref(0);
const lastPayout = ref(0);
let payoutTimer: number | undefined;

const simulatedLadiStage = computed(() => getLadiStage(simulatedRating.value));
const simulationMatchesCurrentWeek = computed(() =>
  simulatedCompletion.value === store.dailyEnergy
  && simulatedRating.value === store.averageTaskRating
  && simulatedStreak.value === store.currentDailyStreak,
);
const simulatedInterestRate = computed(() => simulationMatchesCurrentWeek.value
  ? store.savingsInterestRate
  : familyParticipationInterestStrategy.calculateRate({
      baseRate: store.baseSavingsRatePercent,
      completionBonusRate: store.completionBonusRate,
      completionPercent: simulatedCompletion.value,
      maxRate: store.maxSavingsRatePercent,
      rating: simulatedRating.value,
      ratingBonusRate: store.ratingBonusRate,
      streakBonusRate: store.streakBonusRate,
      streakDays: simulatedStreak.value,
    }),
);
const simulatedWeeklyInterest = computed(() => store.ownSavingGoals.reduce(
  (sum, goal) => sum + interestForRate(goal.saved, goal.target, simulatedInterestRate.value),
  0,
));

const formatRate = (value: number) => value.toLocaleString(locale.value, {
  maximumFractionDigits: 2,
  minimumFractionDigits: 1,
});
function interestForRate(saved: number, target: number, rate: number): number {
  if (saved <= 0 || saved >= target) return 0;
  return Math.min(target - saved, Math.max(1, Math.round(saved * (rate / 100))));
}
function resetSimulation(): void {
  simulatedCompletion.value = store.dailyEnergy;
  simulatedRating.value = store.averageTaskRating;
  simulatedStreak.value = store.currentDailyStreak;
  payoutVisible.value = false;
}
function simulatePayout(): void {
  lastPayout.value = store.creditActiveChildInterestDemo(simulatedInterestRate.value);
  if (lastPayout.value <= 0) return;
  payoutVersion.value += 1;
  payoutVisible.value = true;
  if (payoutTimer !== undefined) window.clearTimeout(payoutTimer);
  payoutTimer = window.setTimeout(() => { payoutVisible.value = false; }, 3300);
}

onMounted(resetSimulation);
onUnmounted(() => {
  if (payoutTimer !== undefined) window.clearTimeout(payoutTimer);
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.interest-simulator {
  border: 1px solid var(--lad-border-info);
  background: var(--lad-gradient-info) !important;
}
h3 {
  @apply ma-0;
  font-size: 1.125rem;
  letter-spacing: -0.025em;
}
.simulator-preview {
  padding: 11px 12px;
  @apply d-flex align-center;
  gap: 10px;
  border: 1px solid var(--lad-border-subtle);
  border-radius: 18px;
  background: color-mix(in srgb, var(--lad-surface) 72%, transparent);
}
.simulator-preview > div:nth-child(2) span,
.simulator-preview > div:nth-child(2) strong,
.simulator-preview > div:nth-child(2) small {
  @apply d-block;
}
.simulator-preview > div:nth-child(2) span,
.simulator-preview > div:nth-child(2) small {
  color: var(--lad-muted);
  font-size: 0.625rem;
}
.simulator-preview > div:nth-child(2) strong {
  margin: 1px 0;
  font-size: 0.875rem;
}
.simulator-rate {
  min-width: 70px;
  @apply text-right;
}
.simulator-rate strong,
.simulator-rate span {
  @apply d-block;
}
.simulator-rate strong {
  color: var(--lad-color-info-strong);
  font-size: 1.25rem;
}
.simulator-rate span {
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.simulator-label {
  margin-top: 9px;
  @apply d-flex align-center justify-space-between;
  color: var(--lad-muted);
  font-size: 0.6875rem;
}
.simulator-label strong {
  color: var(--lad-text);
}
.payout-estimate {
  padding: 11px 12px;
  border-radius: 14px;
  background: color-mix(in srgb, var(--lad-palette-white) 75%, transparent);
}
.payout-estimate > div {
  @apply d-flex align-center justify-space-between ga-3;
}
.payout-estimate span,
.payout-estimate small {
  color: var(--lad-muted);
  font-size: 0.625rem;
}
.payout-estimate strong {
  color: var(--lad-mint-dark);
  font-size: 1.375rem;
}
.payout-estimate small {
  @apply d-block;
  margin-top: 3px;
  line-height: 1.35;
}
.simulator-actions {
  @apply d-flex align-center justify-end;
  gap: 6px;
}
.payout-success {
  @apply pa-3 d-flex align-center ga-3 overflow-hidden;
  border: 1px solid var(--lad-border-success);
  border-radius: 17px;
  background: var(--lad-gradient-success);
  box-shadow: 0 5px 0 var(--lad-shadow-raised-success);
}
.payout-success > div:last-child strong,
.payout-success > div:last-child span {
  @apply d-block;
}
.payout-success > div:last-child strong {
  color: var(--lad-color-success-strong);
  font-size: 1.125rem;
}
.payout-success > div:last-child span {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.625rem;
  line-height: 1.35;
}
.payout-visual {
  width: 96px;
  height: 70px;
  @apply position-relative d-grid place-center;
  flex: 0 0 96px;
}
.payout-visual > .animated-piggy {
  @apply position-absolute right-0;
}
.payout-coin {
  @apply position-absolute;
  z-index: 2;
  left: 9px;
}
@include respond-down(small) {
  .simulator-preview {
    @apply align-start flex-wrap;
  }
  .simulator-rate {
    @apply ms-auto;
  }
  .simulator-actions {
    @apply align-stretch flex-column-reverse;
  }
}
</style>
