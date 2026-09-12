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

        <SavingsInterestSimulator v-if="store.viewerRole === 'child'" />

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
import { computed, ref, watch } from 'vue';

import AnimatedExchangeIcon from './AnimatedExchangeIcon.vue';
import LadirchenCoin from './LadirchenCoin.vue';
import LadiMascot from './LadiMascot.vue';
import SavingGoalDialog from './SavingGoalDialog.vue';
import SavingsInterestSimulator from './savings/SavingsInterestSimulator.vue';
import type { NewGoal, SavingGoal } from '../domain/types';
import { getLadiStage } from '../domain/ladi';
import { useFamilyWorldStore } from '../stores/family-world';

const store = useFamilyWorldStore();
const selectedGoalId = ref('');
const amount = ref(25);
const goalDialog = ref(false);
const editingGoal = ref<SavingGoal | null>(null);

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

const formatRate = (value: number) => value.toLocaleString('de-DE', { minimumFractionDigits: 1, maximumFractionDigits: 2 });
const progress = (saved: number, target: number) => Math.min(100, (saved / target) * 100);
const weeklyInterest = (saved: number) => saved <= 0 ? 0 : Math.max(1, Math.round(saved * (store.savingsInterestRate / 100)));
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
}
</style>
