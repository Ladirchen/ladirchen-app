<template>
  <v-dialog v-model="store.piggyBankOpen" max-width="500" scrollable>
    <v-card class="piggy-dialog" rounded="xl">
      <div class="piggy-header pa-5">
        <div class="d-flex align-start justify-space-between ga-3">
          <div>
            <p class="eyebrow mb-1">Ladirchen-Guthaben</p>
            <h2>{{ store.activeChild.name }}s Guthaben</h2>
          </div>
          <div class="d-flex align-center ga-2">
            <LadirchenCoin animated />
            <v-btn aria-label="Guthaben schließen" icon="mdi-close" size="small" variant="text" @click="store.piggyBankOpen = false" />
          </div>
        </div>
        <div class="balance-grid mt-5">
          <div class="balance-tile"><span>Frei verfügbar</span><strong>{{ store.availableBalance }} L</strong></div>
          <div class="balance-tile"><span>Reserviert</span><strong>{{ store.reservedBalance }} L</strong></div>
          <div class="balance-tile"><span>In Sparplänen</span><strong>{{ store.totalSaved }} L</strong></div>
          <div class="balance-tile interest-earned"><span>Nur durch Zinsen verdient</span><strong>+{{ store.totalInterestEarned }} L</strong></div>
        </div>
        <div class="family-currency-value mt-3">
          <AnimatedExchangeIcon :currency-code="store.familyCurrencyCode" />
          <div class="flex-grow-1">
            <span>In eurer Familienwährung</span>
            <strong>{{ store.availableBalance }} L = {{ formattedFamilyValue }}</strong>
          </div>
          <small>Familienkurs:<br>{{ store.ladirchenPerCurrencyUnit }} L für 1 {{ store.familyCurrencyCode }}</small>
        </div>
      </div>

      <v-card-text class="pa-5">
        <div class="ladi-profile d-flex align-center ga-3 mb-5">
          <LadiMascot :score="store.averageTaskRating" :size="76" />
          <div><p class="eyebrow mb-1">Dein Sparfaultier</p><strong>{{ ladiStage.name }}</strong><p class="text-caption text-medium-emphasis mt-1">{{ ladiStage.description }}</p></div>
        </div>

        <v-card class="interest-card pa-4 mb-5" color="green-lighten-5" elevation="0" rounded="xl">
          <div class="d-flex align-center justify-space-between ga-3">
            <div><p class="eyebrow mb-1">Aktuelle Kondition</p><strong>Wochenzins</strong></div>
            <div class="interest-rate">{{ formatRate(store.savingsInterestRate) }} %</div>
          </div>
          <div class="interest-factors mt-3">
            <span>Grund {{ formatRate(store.baseSavingsRatePercent) }} %</span>
            <span>Serie {{ store.currentDailyStreak }} Tage</span>
            <span>Aufgaben {{ store.dailyEnergy }} %</span>
            <span>Bewertung {{ store.averageTaskRating.toFixed(1) }} / 5</span>
          </div>
        </v-card>

        <v-card v-if="store.viewerRole === 'child'" class="child-interest-simulator pa-4 mb-5" elevation="0" rounded="xl">
          <div class="d-flex align-start justify-space-between ga-3">
            <div>
              <p class="eyebrow mb-1">Kinder-Simulator</p>
              <h3 class="dialog-section-title">Zins &amp; Ladi ausprobieren</h3>
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
            <v-btn rounded="lg" size="small" variant="text" @click="resetChildSimulation">Zurücksetzen</v-btn>
            <v-btn color="primary" :disabled="simulatedWeeklyInterest <= 0" rounded="lg" size="small" variant="flat" @click="simulateInterestPayout">Zinsauszahlung simulieren</v-btn>
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

        <v-card class="interest-help pa-4 mb-5" color="amber-lighten-5" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <v-avatar color="warning" variant="tonal">💡</v-avatar>
            <div class="flex-grow-1">
              <strong>Wie kann ich meinen Zins verbessern?</strong>
              <p class="text-caption text-medium-emphasis mt-1">Zinsen sind zusätzliche Ladirchen, die jede Woche automatisch in deinen Sparplänen landen. Du musst dafür nichts bezahlen.</p>
            </div>
          </div>
          <div class="interest-equation mt-4">
            <span>{{ formatRate(store.baseSavingsRatePercent) }} % Grundzins</span>
            <b>+</b><span>{{ formatRate(streakInterest) }} % Serie</span>
            <b>+</b><span>{{ formatRate(completionInterest) }} % Aufgaben</span>
            <b>+</b><span>{{ formatRate(ratingInterest) }} % Sterne</span>
          </div>
          <div class="tip-list mt-4">
            <div><span>🌱</span><p><strong>Mehr Beiträge abschließen</strong><small>Bei 100 % Aufgaben bekommst du den vollständigen Aufgabenbonus.</small></p></div>
            <div><span>🔥</span><p><strong>Tagesserie halten</strong><small>Jeder weitere erfolgreiche Tag verbessert deinen Zinssatz.</small></p></div>
            <div><span>⭐</span><p><strong>Sorgfältig mithelfen</strong><small>Gute Bewertungen bringen einen höheren Sterneanteil.</small></p></div>
          </div>
          <p class="interest-example mt-4">Mit deinen aktuell gesparten {{ store.totalSaved }} L erhältst du nächste Woche ungefähr <strong>+{{ estimatedWeeklyInterest }} L</strong>.</p>
        </v-card>

        <div class="saving-goals-heading">
          <div>
            <h3 class="dialog-section-title">Meine Sparpläne</h3>
            <p class="text-caption text-medium-emphasis mt-1">Spare für einen Wunsch oder starte direkt ein neues Ziel.</p>
          </div>
          <v-btn color="primary" prepend-icon="mdi-plus" rounded="lg" size="small" variant="tonal" @click="openNewGoal">Neues Sparziel</v-btn>
        </div>
        <div class="d-flex flex-column ga-3 mt-3">
          <v-card v-for="goal in store.ownSavingGoals" :key="goal.id" class="piggy-goal pa-4" elevation="0" rounded="xl">
            <div class="d-flex align-center ga-3">
              <div class="goal-emoji">{{ goal.icon }}</div>
              <div class="flex-grow-1 min-w-0">
                <div class="d-flex align-center justify-space-between ga-2">
                  <strong>{{ goal.title }}</strong>
                  <div class="d-flex align-center ga-1">
                    <span class="text-caption font-weight-bold">{{ goal.saved }} L</span>
                    <v-btn :aria-label="`${goal.title} bearbeiten`" color="primary" icon="mdi-pencil-outline" size="x-small" variant="text" @click="openEditGoal(goal)" />
                  </div>
                </div>
                <v-progress-linear class="mt-2" color="primary" height="7" :model-value="progress(goal.saved, goal.target)" rounded />
                <v-chip v-if="goal.starterBonus && goal.saved < goal.target" class="mt-2" color="warning" prepend-icon="mdi-lock-outline" size="x-small" variant="tonal">{{ goal.starterBonus }} L Startbonus geschützt</v-chip>
                <p class="text-caption text-medium-emphasis mt-1">Voraussichtlich +{{ weeklyInterest(goal.saved) }} L pro Woche · <strong>davon bisher +{{ goal.interestEarned ?? 0 }} L nur durch Zinsen</strong></p>
              </div>
            </div>
          </v-card>
        </div>

        <v-card class="transfer-card pa-4 mt-5" color="blue-lighten-5" elevation="0" rounded="xl">
          <h3 class="dialog-section-title">Ladirchen verschieben</h3>
          <v-select
            v-model="selectedGoalId"
            class="mt-3"
            density="comfortable"
            :items="goalOptions"
            item-title="title"
            item-value="value"
            label="Sparplan"
            variant="outlined"
          />
          <v-text-field v-model.number="amount" density="comfortable" label="Betrag" min="0" suffix="L" type="number" variant="outlined" />
          <div class="d-grid transfer-actions ga-2">
            <v-btn :disabled="amount <= 0 || amount > selectedGoalWithdrawable" rounded="lg" variant="tonal" @click="withdraw">Für Shop freigeben</v-btn>
            <v-btn color="info" :disabled="amount <= 0 || amount > maxDeposit" rounded="lg" variant="flat" @click="deposit">In Sparplan legen</v-btn>
          </div>
          <p v-if="selectedGoal?.starterBonus && selectedGoal.saved < selectedGoal.target" class="bonus-lock-note mt-3"><v-icon icon="mdi-lock-outline" size="15" /> {{ selectedGoal.starterBonus }} L Startbonus bleiben bis zum Erreichen des Ziels geschützt.</p>
          <p class="text-caption text-medium-emphasis mt-3">Eigene Einzahlungen bleiben flexibel. Geschützte Startboni werden erst nach Erreichen des Ziels frei.</p>
        </v-card>

        <v-card v-if="store.viewerRole === 'guardian'" class="conditions-card pa-4 mt-5" elevation="0" rounded="xl">
          <p class="eyebrow mb-1">Für Bezugspersonen</p>
          <h3 class="dialog-section-title">Zinskonditionen einstellen</h3>
          <p class="text-caption text-medium-emphasis mt-1 mb-4">Die Werte werden pro Woche addiert. Gute Mitarbeit und Bewertungen verbessern den Zinssatz.</p>
          <label class="setting-label">Grundzins <strong>{{ formatRate(store.baseSavingsRatePercent) }} %</strong></label>
          <v-slider v-model="store.baseSavingsRatePercent" color="primary" hide-details max="5" min="0" step="0.1" />
          <label class="setting-label">Pro Serientag <strong>+{{ formatRate(store.streakBonusRate) }} %</strong></label>
          <v-slider v-model="store.streakBonusRate" color="primary" hide-details max="2" min="0" step="0.1" />
          <label class="setting-label">Bei 100 % Aufgaben <strong>+{{ formatRate(store.completionBonusRate) }} %</strong></label>
          <v-slider v-model="store.completionBonusRate" color="primary" hide-details max="5" min="0" step="0.1" />
          <label class="setting-label">Bei 5 Sternen im Schnitt <strong>+{{ formatRate(store.ratingBonusRate) }} %</strong></label>
          <v-slider v-model="store.ratingBonusRate" color="primary" hide-details max="5" min="0" step="0.1" />
          <label class="setting-label">Maximaler Wochenzins <strong>{{ formatRate(store.maxSavingsRatePercent) }} %</strong></label>
          <v-slider v-model="store.maxSavingsRatePercent" color="warning" hide-details max="20" min="1" step="0.5" />
          <v-btn class="mt-4" color="primary" rounded="lg" variant="tonal" width="100%" @click="store.creditWeeklyInterestDemo">Wochenzinsen simulieren</v-btn>
        </v-card>
      </v-card-text>
    </v-card>
  </v-dialog>

  <SavingGoalDialog
    v-model="goalDialog"
    :initial-goal="editingGoal"
    :minimum-target="editingGoal?.saved ?? 10"
    @remove="cancelEditingGoal"
    @submit="saveGoal"
  />
</template>

<script lang="ts" setup>
import { computed, onUnmounted, ref, watch } from 'vue';
import { AnimatePresence, motion } from 'motion-v';

import AnimatedPiggyBank from './AnimatedPiggyBank.vue';
import AnimatedExchangeIcon from './AnimatedExchangeIcon.vue';
import LadirchenCoin from './LadirchenCoin.vue';
import LadiMascot from './LadiMascot.vue';
import SavingGoalDialog from './SavingGoalDialog.vue';
import type { NewGoal, SavingGoal } from '../domain/types';
import { getLadiStage } from '../domain/ladi';
import { usePrototypeStore } from '../stores/prototype';

const store = usePrototypeStore();
const selectedGoalId = ref('');
const amount = ref(25);
const goalDialog = ref(false);
const editingGoal = ref<SavingGoal | null>(null);
const simulatedCompletion = ref(0);
const simulatedRating = ref(0);
const simulatedStreak = ref(0);
const payoutVisible = ref(false);
const payoutVersion = ref(0);
const lastPayout = ref(0);
let payoutTimer: number | undefined;

const selectedGoal = computed(() => store.ownSavingGoals.find((goal) => goal.id === selectedGoalId.value));
const selectedGoalWithdrawable = computed(() => store.withdrawableGoalBalance(selectedGoal.value));
const maxDeposit = computed(() => Math.min(
  store.availableBalance,
  Math.max(0, (selectedGoal.value?.target ?? 0) - (selectedGoal.value?.saved ?? 0)),
));
const goalOptions = computed(() => store.ownSavingGoals.map((goal) => ({ title: goal.title, value: goal.id })));
const streakInterest = computed(() => store.currentDailyStreak * store.streakBonusRate);
const completionInterest = computed(() => (store.dailyEnergy / 100) * store.completionBonusRate);
const ratingInterest = computed(() => (store.averageTaskRating / 5) * store.ratingBonusRate);
const estimatedWeeklyInterest = computed(() =>
  store.ownSavingGoals.reduce((sum, goal) => sum + weeklyInterest(goal.saved), 0),
);
const formattedFamilyValue = computed(() => new Intl.NumberFormat('de-CH', {
  style: 'currency',
  currency: store.familyCurrencyCode,
}).format(store.familyCurrencyValue(store.availableBalance)));
const ladiStage = computed(() => getLadiStage(store.averageTaskRating));
const simulatedLadiStage = computed(() => getLadiStage(simulatedRating.value));
const simulationMatchesCurrentWeek = computed(() =>
  simulatedCompletion.value === store.dailyEnergy &&
  simulatedRating.value === store.averageTaskRating &&
  simulatedStreak.value === store.currentDailyStreak,
);
const simulatedInterestRate = computed(() => simulationMatchesCurrentWeek.value
  ? store.savingsInterestRate
  : Number(Math.min(
      store.maxSavingsRatePercent,
      store.baseSavingsRatePercent +
        simulatedStreak.value * store.streakBonusRate +
        (simulatedCompletion.value / 100) * store.completionBonusRate +
        (simulatedRating.value / 5) * store.ratingBonusRate,
    ).toFixed(2)),
);
const simulatedWeeklyInterest = computed(() => store.ownSavingGoals.reduce(
  (sum, goal) => sum + interestForRate(goal.saved, goal.target, simulatedInterestRate.value),
  0,
));

const formatRate = (value: number) => value.toLocaleString('de-DE', { minimumFractionDigits: 1, maximumFractionDigits: 2 });
const progress = (saved: number, target: number) => Math.min(100, (saved / target) * 100);
const weeklyInterest = (saved: number) => saved <= 0 ? 0 : Math.max(1, Math.round(saved * (store.savingsInterestRate / 100)));
function interestForRate(saved: number, target: number, rate: number): number {
  if (saved <= 0 || saved >= target) return 0;
  return Math.min(target - saved, Math.max(1, Math.round(saved * (rate / 100))));
}
const resetChildSimulation = () => {
  simulatedCompletion.value = store.dailyEnergy;
  simulatedRating.value = store.averageTaskRating;
  simulatedStreak.value = store.currentDailyStreak;
};
const simulateInterestPayout = () => {
  lastPayout.value = store.creditActiveChildInterestDemo(simulatedInterestRate.value);
  if (lastPayout.value <= 0) return;
  payoutVersion.value += 1;
  payoutVisible.value = true;
  if (payoutTimer !== undefined) window.clearTimeout(payoutTimer);
  payoutTimer = window.setTimeout(() => { payoutVisible.value = false; }, 3300);
};
const deposit = () => {
  store.saveToGoal(selectedGoalId.value, amount.value);
  amount.value = 25;
};
const withdraw = () => {
  store.withdrawFromGoal(selectedGoalId.value, amount.value);
  amount.value = 25;
};
const openNewGoal = () => {
  editingGoal.value = null;
  goalDialog.value = true;
};
const openEditGoal = (goal: SavingGoal) => {
  editingGoal.value = goal;
  goalDialog.value = true;
};
const saveGoal = (goal: NewGoal) => {
  if (editingGoal.value) {
    store.updateGoal(editingGoal.value.id, goal);
    selectedGoalId.value = editingGoal.value.id;
    return;
  }
  store.addGoal(goal);
  selectedGoalId.value = store.activeGoalId;
};
const cancelEditingGoal = () => {
  if (!editingGoal.value) return;
  store.cancelGoal(editingGoal.value.id);
  editingGoal.value = null;
  selectedGoalId.value = store.activeGoal.id;
};

watch(() => store.piggyBankOpen, (isOpen) => {
  if (!isOpen) return;
  selectedGoalId.value = store.activeGoal.id;
  amount.value = Math.min(25, store.availableBalance);
  resetChildSimulation();
  payoutVisible.value = false;
});

onUnmounted(() => {
  if (payoutTimer !== undefined) window.clearTimeout(payoutTimer);
});
</script>

<style scoped>
.piggy-dialog {
  max-height: min(820px, 94dvh);
}
.piggy-header {
  color: #253843;
  background: linear-gradient(145deg, #ffe4e9, #fff2cf);
  border-bottom: 1px solid rgba(181, 107, 107, 0.14);
}
.piggy-header h2 {
  margin: 0;
  font-size: 23px;
  letter-spacing: -0.035em;
}
.ladi-profile {
  padding: 12px 14px;
  border: 1px solid var(--lad-border);
  border-radius: 18px;
  background: linear-gradient(145deg, #fffaf0, #eef9f4);
}
.balance-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}
.balance-tile {
  padding: 12px;
  border: 1px solid rgba(110, 82, 72, 0.13);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.62);
}
.balance-tile.interest-earned {
  color: #247b5d;
  background: rgba(229, 250, 239, 0.78);
}
.balance-tile span,
.balance-tile strong {
  display: block;
}
.balance-tile span {
  color: var(--lad-muted);
  font-size: 11px;
}
.balance-tile strong {
  margin-top: 2px;
  font-size: 21px;
}
.family-currency-value {
  padding: 10px 12px;
  display: flex;
  align-items: center;
  gap: 10px;
  border: 1px solid rgba(110, 82, 72, 0.13);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.76);
}
.family-currency-value > .exchange-icon {
  flex: 0 0 42px;
}
.family-currency-value span,
.family-currency-value strong {
  display: block;
}
.family-currency-value span {
  color: var(--lad-muted);
  font-size: 10px;
}
.family-currency-value strong {
  color: #214c3f;
  font-size: 20px;
}
.family-currency-value small {
  color: var(--lad-muted);
  font-size: 9px;
  font-weight: 800;
}
.interest-card {
  border: 1px solid rgba(62, 188, 140, 0.2);
}
.child-interest-simulator {
  border: 1px solid rgba(78, 143, 221, 0.22);
  background: linear-gradient(155deg, #eef7ff, #f1fbf6 62%, #fff8db) !important;
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
  flex-shrink: 0;
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
.interest-rate {
  color: var(--lad-mint-dark);
  font-size: 27px;
  font-weight: 950;
}
.interest-factors {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}
.interest-factors span {
  padding: 5px 8px;
  color: var(--lad-muted);
  border-radius: 9px;
  background: rgba(255, 255, 255, 0.68);
  font-size: 10px;
  font-weight: 800;
}
.interest-help {
  border: 1px solid rgba(242, 175, 66, 0.25);
}
.saving-goals-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}
.saving-goals-heading > div {
  min-width: 0;
}
.saving-goals-heading :deep(.v-btn) {
  flex-shrink: 0;
}
.interest-equation {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 5px;
}
.interest-equation span {
  padding: 5px 7px;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.72);
  font-size: 10px;
  font-weight: 800;
}
.interest-equation b {
  color: #b47a1f;
}
.tip-list {
  display: grid;
  gap: 9px;
}
.tip-list > div {
  display: flex;
  align-items: flex-start;
  gap: 9px;
}
.tip-list > div > span {
  font-size: 19px;
}
.tip-list p,
.tip-list strong,
.tip-list small {
  display: block;
}
.tip-list p {
  margin: 0;
}
.tip-list strong {
  font-size: 12px;
}
.tip-list small {
  margin-top: 1px;
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.35;
}
.interest-example {
  padding: 10px 12px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.72);
  font-size: 12px;
}
.dialog-section-title {
  margin: 0;
  font-size: 18px;
  letter-spacing: -0.025em;
}
.piggy-goal,
.conditions-card {
  border: 1px solid var(--lad-border);
}
.bonus-lock-note {
  display: flex;
  align-items: center;
  gap: 5px;
  color: #8a641f;
  font-size: 10px;
  line-height: 1.35;
}
.goal-emoji {
  width: 43px;
  height: 43px;
  display: grid;
  place-items: center;
  flex-shrink: 0;
  border-radius: 13px;
  background: var(--lad-surface-soft);
  font-size: 24px;
}
.transfer-card {
  border: 1px solid rgba(78, 143, 221, 0.2);
}
.transfer-actions {
  grid-template-columns: 1fr 1fr;
}
.setting-label {
  margin-top: 12px;
  display: flex;
  justify-content: space-between;
  color: var(--lad-muted);
  font-size: 12px;
}
.setting-label strong {
  color: var(--lad-text);
}
@media (max-width: 400px) {
  .transfer-actions {
    grid-template-columns: 1fr;
  }
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
