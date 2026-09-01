<template>
  <v-card class="interest-simulator pa-4 mb-5" elevation="0" rounded="xl">
    <div class="d-flex align-start justify-space-between ga-3">
      <div>
        <p class="eyebrow mb-1">Kinder-Simulator</p>
        <h3>Zins &amp; Ladi ausprobieren</h3>
        <p class="text-caption text-medium-emphasis mt-1">Die Regler sind nur eine Vorschau und verändern deine echten Aufgaben nicht.</p>
      </div>
      <v-chip color="info" size="x-small" variant="tonal">Demo</v-chip>
    </div>

    <div class="simulator-preview mt-4">
      <LadiMascot :score="simulatedRating" :show-score="false" :size="68" />
      <div class="flex-grow-1">
        <span>Wenn deine Woche so endet</span>
        <strong>{{ simulatedLadiStage.name }}</strong>
        <small>{{ simulatedLadiStage.description }}</small>
      </div>
      <div class="simulator-rate"><strong>{{ formatRate(simulatedInterestRate) }} %</strong><span>Wochenzins</span></div>
    </div>

    <label class="simulator-label mt-4">Aufgaben erledigt <strong>{{ simulatedCompletion }} %</strong></label>
    <v-slider v-model="simulatedCompletion" color="primary" hide-details max="100" min="0" step="10" />
    <label class="simulator-label">Durchschnittliche Bewertung <strong>{{ formatRate(simulatedRating) }} / 5</strong></label>
    <v-slider v-model="simulatedRating" color="warning" hide-details max="5" min="0" step="0.1" />
    <label class="simulator-label">Tagesserie <strong>{{ simulatedStreak }} Tage</strong></label>
    <v-slider v-model="simulatedStreak" color="info" hide-details max="30" min="0" step="1" />

    <div class="payout-estimate mt-4">
      <div><span>Deine nächste Demo-Auszahlung</span><strong>+{{ simulatedWeeklyInterest }} L</strong></div>
      <small>Berechnet aus {{ store.totalSaved }} L in deinen Sparplänen. Jeder Klick simuliert eine weitere Woche.</small>
    </div>

    <div class="simulator-actions mt-3">
      <v-btn rounded="lg" size="small" variant="text" @click="resetSimulation">Zurücksetzen</v-btn>
      <v-btn color="primary" :disabled="simulatedWeeklyInterest <= 0" rounded="lg" size="small" variant="flat" @click="simulatePayout">Zinsauszahlung simulieren</v-btn>
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
        <div><strong>+{{ lastPayout }} L Zinsen!</strong><span>Deine Sparpläne und dein Zinsertrag wurden aktualisiert.</span></div>
      </motion.div>
    </AnimatePresence>
  </v-card>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { AnimatePresence, motion } from 'motion-v';

import AnimatedPiggyBank from '../AnimatedPiggyBank.vue';
import LadirchenCoin from '../LadirchenCoin.vue';
import LadiMascot from '../LadiMascot.vue';
import { getLadiStage } from '../../domain/ladi';
import { familyParticipationInterestStrategy } from '../../domain/savings-interest';
import { useFamilyWorldStore } from '../../stores/family-world';

const store = useFamilyWorldStore();
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

const formatRate = (value: number) => value.toLocaleString('de-DE', {
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

<style scoped>
.interest-simulator {
  border: 1px solid rgba(78, 143, 221, 0.22);
  background: linear-gradient(155deg, #eef7ff, #f1fbf6 62%, #fff8db) !important;
}
h3 {
  margin: 0;
  font-size: 18px;
  letter-spacing: -0.025em;
}
.simulator-preview {
  padding: 11px 12px;
  display: flex;
  align-items: center;
  gap: 10px;
  border: 1px solid rgba(62, 137, 116, 0.15);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.72);
}
.simulator-preview > div:nth-child(2) span,
.simulator-preview > div:nth-child(2) strong,
.simulator-preview > div:nth-child(2) small {
  display: block;
}
.simulator-preview > div:nth-child(2) span,
.simulator-preview > div:nth-child(2) small {
  color: var(--lad-muted);
  font-size: 10px;
}
.simulator-preview > div:nth-child(2) strong {
  margin: 1px 0;
  font-size: 14px;
}
.simulator-rate {
  min-width: 70px;
  text-align: right;
}
.simulator-rate strong,
.simulator-rate span {
  display: block;
}
.simulator-rate strong {
  color: var(--lad-blue-dark);
  font-size: 20px;
}
.simulator-rate span {
  color: var(--lad-muted);
  font-size: 9px;
}
.simulator-label {
  margin-top: 9px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: var(--lad-muted);
  font-size: 11px;
}
.simulator-label strong {
  color: var(--lad-text);
}
.payout-estimate {
  padding: 11px 12px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.76);
}
.payout-estimate > div {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}
.payout-estimate span,
.payout-estimate small {
  color: var(--lad-muted);
  font-size: 10px;
}
.payout-estimate strong {
  color: var(--lad-mint-dark);
  font-size: 22px;
}
.payout-estimate small {
  display: block;
  margin-top: 3px;
  line-height: 1.35;
}
.simulator-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 6px;
}
.payout-success {
  padding: 12px;
  display: flex;
  align-items: center;
  gap: 12px;
  overflow: hidden;
  border: 1px solid rgba(62, 188, 140, 0.25);
  border-radius: 17px;
  background: linear-gradient(145deg, #e2f8ec, #fff4c9);
  box-shadow: 0 5px 0 rgba(62, 188, 140, 0.12);
}
.payout-success > div:last-child strong,
.payout-success > div:last-child span {
  display: block;
}
.payout-success > div:last-child strong {
  color: #247b5d;
  font-size: 18px;
}
.payout-success > div:last-child span {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.35;
}
.payout-visual {
  width: 96px;
  height: 70px;
  position: relative;
  display: grid;
  place-items: center;
  flex: 0 0 96px;
}
.payout-visual > .animated-piggy {
  position: absolute;
  right: 0;
}
.payout-coin {
  position: absolute;
  z-index: 2;
  left: 9px;
}
@media (max-width: 400px) {
  .simulator-preview {
    align-items: flex-start;
    flex-wrap: wrap;
  }
  .simulator-rate {
    margin-left: auto;
  }
  .simulator-actions {
    align-items: stretch;
    flex-direction: column-reverse;
  }
}
</style>
