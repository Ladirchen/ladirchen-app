<template>
  <v-dialog v-model="store.piggyBankOpen" content-class="piggy-summary-dialog-frame" max-width="460" scrollable>
    <v-card class="piggy-dialog" :class="{ 'piggy-dialog--guardian': store.viewerRole === 'guardian' }" rounded="xl">
      <div class="piggy-header pa-3">
        <div class="piggy-title-row">
          <div class="piggy-title-copy">
            <p class="dialog-kicker">{{ store.viewerRole === 'guardian' ? 'Familienguthaben' : 'Mein Geld' }}</p>
            <h2>{{ store.viewerRole === 'guardian' ? 'Guthaben der Kinder' : `${store.activeChild.name}s Guthaben` }}</h2>
            <p class="piggy-subtitle">{{ store.viewerRole === 'guardian' ? 'Kontostände kompakt vergleichen und Zinskonditionen verwalten.' : 'Deine Ladirchen, Sparpläne und Geschenke auf einen Blick.' }}</p>
          </div>
          <div class="piggy-title-actions">
            <span class="header-coin"><LadirchenCoin animated /></span>
            <button class="piggy-close" aria-label="Guthaben schließen" type="button" @click="store.piggyBankOpen = false"><v-icon icon="mdi-close" /></button>
          </div>
        </div>
        <template v-if="store.viewerRole === 'child'">
          <div class="balance-grid mt-3">
            <div class="balance-tile balance-tile--wallet">
              <span class="balance-icon balance-icon--wallet" aria-hidden="true"><v-icon icon="mdi-wallet-outline" /></span>
              <span>
                <small>Frei verfügbar</small>
                <strong>{{ store.availableBalance }} L</strong>
                <span class="today-earned"><v-icon aria-hidden="true" icon="mdi-sparkles" />Heute verdient <b>+{{ store.todayEarned }} L</b></span>
              </span>
            </div>
            <div class="balance-tile balance-tile--plans"><span class="balance-icon balance-icon--plans" aria-hidden="true"><v-icon icon="mdi-star-four-points-outline" /></span><span><small>In Sparplänen</small><strong>{{ store.totalSaved }} L</strong></span></div>
          </div>
          <div class="balance-detail-grid mt-2">
            <div class="balance-tile interest-earned"><span class="balance-icon balance-icon--interest" aria-hidden="true"><v-icon icon="mdi-chart-line" /></span><span><small>Nur durch Zinsen verdient</small><strong>+{{ store.totalInterestEarned }} L</strong></span></div>
            <div class="family-currency-value">
              <AnimatedExchangeIcon :currency-code="store.familyCurrencyCode" />
              <div>
                <span>In eurer Familienwährung</span>
                <strong>{{ store.availableBalance }} L = {{ formattedFamilyValue }}</strong>
              </div>
              <small>Familienkurs: {{ store.ladirchenPerCurrencyUnit }} L für 1 {{ store.familyCurrencyCode }}</small>
            </div>
          </div>
        </template>
      </div>

      <v-card-text class="piggy-content pa-3">
        <section v-if="store.viewerRole === 'guardian'" class="guardian-balances" aria-labelledby="guardian-balances-title">
          <div class="guardian-balances-heading">
            <div><p class="eyebrow mb-1">Kinderkonten</p><h3 id="guardian-balances-title" class="dialog-section-title">Wer hat wie viele Ladirchen?</h3></div>
            <span>{{ guardianChildren.length }} Kinder</span>
          </div>
          <div class="guardian-balance-list mt-3">
            <article v-for="child in guardianChildren" :key="child.id" class="guardian-balance-row">
              <span class="guardian-child-avatar" aria-hidden="true"><AvatarFigure :appearance="memberAppearance(child.id)" :size="40" /></span>
              <span class="guardian-child-name"><strong>{{ child.name }}</strong><small>{{ child.available }} frei · {{ child.saved }} gespart</small></span>
              <span class="guardian-child-stat"><small>Guthaben</small><strong>{{ child.total }} L</strong></span>
              <span class="guardian-child-stat"><small>Diese Woche</small><strong>+{{ child.weekEarned }} L</strong></span>
              <span class="guardian-child-stat"><small>Aufgaben</small><strong>{{ child.completedThisWeek }}</strong></span>
              <span class="guardian-child-stat guardian-child-stat--level"><small>Ladi-Level</small><strong>{{ child.ladiLevel }}</strong></span>
            </article>
          </div>
        </section>

        <v-card v-if="store.viewerRole === 'guardian' && store.permissions.canManageFamily" class="currency-settings-card pa-4 mt-4" elevation="0" rounded="xl">
          <div class="currency-settings-heading">
            <span aria-hidden="true">↔</span>
            <div><p class="eyebrow mb-1">Familienkurs</p><h3 class="dialog-section-title">Familienwährung festlegen</h3></div>
          </div>
          <div class="currency-settings-fields mt-3">
            <v-select
              density="compact"
              hide-details
              :items="currencyOptions"
              item-title="title"
              item-value="value"
              label="Währung"
              :model-value="store.familyCurrencyCode"
              variant="outlined"
              @update:model-value="setCurrency"
            />
            <v-text-field
              density="compact"
              hide-details
              label="Ladirchen für 1 Einheit"
              min="1"
              :model-value="store.ladirchenPerCurrencyUnit"
              suffix="L"
              type="number"
              variant="outlined"
              @update:model-value="store.setLadirchenExchangeRate(Number($event))"
            />
          </div>
          <div class="currency-settings-example mt-2"><span>100 Ladirchen entsprechen</span><strong>{{ formattedGuardianFamilyValue }}</strong></div>
        </v-card>

        <v-card v-if="store.viewerRole === 'child'" class="transfer-card pa-3" :class="transferDirection ? `transfer-${transferDirection}` : ''" color="blue-lighten-5" elevation="0" rounded="xl">
          <div class="transfer-heading">
            <div class="transfer-title">
              <span class="transfer-title-icon" aria-hidden="true">
                <LadirchenCoin animated small />
              </span>
              <div>
                <p class="eyebrow mb-1">Dein Geldweg</p>
                <h3 class="dialog-section-title">Ladirchen verschieben</h3>
              </div>
            </div>
          </div>
          <div v-if="store.viewerRole === 'child'" class="destination-switch mt-3" aria-label="Ziel der Ladirchen wählen" role="group">
            <button :aria-pressed="transferDestination === 'goal'" :class="{ active: transferDestination === 'goal' }" type="button" @click="setTransferDestination('goal')">
              <span aria-hidden="true"><v-icon icon="mdi-piggy-bank-outline" /></span><span><strong>Sparplan</strong><small>Für deinen Wunsch</small></span>
            </button>
            <button :aria-pressed="transferDestination === 'member'" :class="{ active: transferDestination === 'member' }" type="button" @click="setTransferDestination('member')">
              <span aria-hidden="true"><v-icon icon="mdi-account-heart-outline" /></span><span><strong>Jemandem schenken</strong><small>Direkt ins Guthaben</small></span>
            </button>
          </div>
          <div class="transfer-fields mt-3">
            <label class="transfer-field">
              <span><i>1</i>Wohin?</span>
              <v-select
                v-if="transferDestination === 'goal'"
                v-model="selectedGoalId"
                class="transfer-input"
                density="compact"
                hide-details
                :items="goalOptions"
                item-title="title"
                item-value="value"
                aria-label="Sparziel auswählen"
                variant="solo"
              >
                <template #selection="{ item }">
                  <span class="selected-goal-option">
                    <span class="goal-option-icon" aria-hidden="true">{{ item.icon }}</span>
                    <span><strong>{{ item.title }}</strong><small>{{ item.saved }} von {{ item.target }} L</small></span>
                  </span>
                </template>
                <template #item="{ props: itemProps, item }">
                  <v-list-item v-bind="itemProps" class="goal-option" :title="undefined">
                    <template #prepend><span class="goal-option-icon" aria-hidden="true">{{ item.icon }}</span></template>
                    <v-list-item-title>{{ item.title }}</v-list-item-title>
                    <v-list-item-subtitle>{{ item.saved }} von {{ item.target }} L gespart</v-list-item-subtitle>
                    <template #append><span class="goal-option-progress">{{ goalProgress(item.saved, item.target) }} %</span></template>
                  </v-list-item>
                </template>
              </v-select>
              <v-select
                v-else
                v-model="selectedMemberId"
                class="transfer-input"
                density="compact"
                hide-details
                :items="memberOptions"
                item-title="title"
                item-value="value"
                aria-label="Familienmitglied auswählen"
                variant="solo"
              >
                <template #selection="{ item }">
                  <span class="selected-goal-option">
                    <span class="goal-option-icon member-option-icon" aria-hidden="true"><AvatarFigure :appearance="memberAppearance(item.value)" :size="34" /></span>
                    <span><strong>{{ item.title }}</strong><small>Ladirchen schenken</small></span>
                  </span>
                </template>
                <template #item="{ props: itemProps, item }">
                  <v-list-item v-bind="itemProps" class="goal-option" :title="undefined">
                    <template #prepend><span class="goal-option-icon member-option-icon" aria-hidden="true"><AvatarFigure :appearance="memberAppearance(item.value)" :size="36" /></span></template>
                    <v-list-item-title>{{ item.title }}</v-list-item-title>
                    <v-list-item-subtitle>Das Geschenk landet im freien Guthaben.</v-list-item-subtitle>
                  </v-list-item>
                </template>
              </v-select>
            </label>
            <div class="transfer-field">
              <span><i>2</i>Wie viele?</span>
              <div class="amount-stepper" role="group" aria-label="Anzahl Ladirchen auswählen">
                <button aria-label="Fünf Ladirchen weniger" :disabled="amount <= 0" type="button" @click="adjustTransferAmount(-5)">−</button>
                <output aria-live="polite"><span>{{ amount }}</span><LadirchenCoin small /></output>
                <button aria-label="Fünf Ladirchen mehr" :disabled="amount >= transferAmountMaximum" type="button" @click="adjustTransferAmount(5)">+</button>
              </div>
            </div>
          </div>
          <div v-if="transferDestination === 'goal'" class="d-grid transfer-actions ga-2 mt-3">
            <v-btn class="transfer-button transfer-button--withdraw" :disabled="amount <= 0 || amount > selectedGoalWithdrawable" rounded="lg" variant="tonal" @click="withdraw">Für Shop freigeben</v-btn>
            <v-btn class="transfer-button transfer-button--deposit" color="info" :disabled="amount <= 0 || amount > maxDeposit" rounded="lg" variant="flat" @click="deposit">In Sparplan legen</v-btn>
          </div>
          <v-btn v-else class="transfer-button transfer-button--gift mt-3" block :disabled="!selectedMemberId || amount <= 0 || amount > store.availableBalance" rounded="lg" variant="flat" @click="giftToMember">
            An {{ selectedMemberName }} senden<v-icon icon="mdi-send-variant-outline" />
          </v-btn>
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
        </v-card>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';

import AnimatedExchangeIcon from './AnimatedExchangeIcon.vue';
import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import { createDefaultAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import { getLadiStage, LADI_STAGES } from '@/domain/ladi';
import type { FamilyCurrency, FamilyMemberId, SavingGoalId } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';
import { isInstantInIsoWeek } from '@/domain/zoned-calendar';

const store = useFamilyWorldStore();
const selectedGoalId = ref<SavingGoalId>();
const selectedMemberId = ref<FamilyMemberId>();
const transferDestination = ref<'goal' | 'member'>('goal');
const amount = ref(25);
const transferDirection = ref<'deposit' | 'withdraw' | 'gift' | ''>('');
const piggyGuideStep = ref(-1);
let transferTimer: number | undefined;
let guidanceTimer: number | undefined;
const currencyOptions: Array<{ title: string; value: FamilyCurrency }> = [
  { title: 'Schweizer Franken (CHF)', value: 'CHF' },
  { title: 'Euro (EUR)', value: 'EUR' },
  { title: 'Forint (HUF)', value: 'HUF' },
];

const guardianChildren = computed(() => store.members
  .filter((member) => member.role === 'child')
  .map((member) => {
    const now = new Date(store.currentTimeMilliseconds);
    const completedThisWeek = store.contributions.filter(contribution =>
      contribution.assigneeId === member.id &&
      contribution.status === 'approved' &&
      contribution.approvedAt !== undefined &&
      isInstantInIsoWeek(contribution.approvedAt, now, store.familyTimeZone),
    );
    const reserved = store.shopRewards
      .filter((reward) => reward.status === 'requested' && reward.requesterId === member.id)
      .reduce((sum, reward) => sum + reward.price, 0);
    const available = Math.max(0, store.balanceFor(member.id) - reserved);
    const saved = store.totalVisibleSavedFor(member.id);
    const ladiStage = getLadiStage(store.averageTaskRatingFor(member.id));
    return {
      available,
      completedThisWeek: completedThisWeek.length,
      id: member.id,
      ladiLevel: LADI_STAGES.length - LADI_STAGES.indexOf(ladiStage),
      name: member.nickname?.trim() || member.name,
      saved,
      total: available + saved,
      weekEarned: completedThisWeek.reduce((sum, contribution) => sum +
        (contribution.earnedReward ?? contribution.reward) +
        (contribution.earnedRatingBonus ?? 0), 0),
    };
  }));

const selectedGoal = computed(() => store.ownSavingGoals.find((goal) => goal.id === selectedGoalId.value));
const selectedGoalWithdrawable = computed(() => store.withdrawableGoalBalance(selectedGoal.value));
const maxDeposit = computed(() => Math.min(
  store.availableBalance,
  Math.max(0, (selectedGoal.value?.target ?? 0) - (selectedGoal.value?.saved ?? 0)),
));
const transferAmountMaximum = computed(() => transferDestination.value === 'member'
  ? store.availableBalance
  : Math.max(maxDeposit.value, selectedGoalWithdrawable.value));
const goalOptions = computed(() => store.ownSavingGoals.map((goal) => ({
  icon: goal.icon,
  saved: goal.saved,
  target: goal.target,
  title: goal.title,
  value: goal.id,
})));
const memberOptions = computed(() => store.members
  .filter((member) => member.role === 'child' && member.id !== store.activeChildId)
  .map((member) => ({ icon: member.avatar, title: member.nickname?.trim() || member.name, value: member.id })));
const selectedMemberName = computed(() => memberOptions.value.find((member) => member.value === selectedMemberId.value)?.title ?? 'Familienmitglied');
const memberAppearance = (memberId: unknown): AvatarAppearance => {
  const children = store.members.filter((member) => member.role === 'child');
  const member = children.find((child) => child.id === memberId);
  if (member?.appearance) return member.appearance;
  const appearance = createDefaultAvatarAppearance();
  const index = Math.max(0, children.findIndex((child) => child.id === memberId));
  const variants: ReadonlyArray<Partial<AvatarAppearance>> = [
    { hair: 'ponytail', outfitColorId: 'outfit-blue' },
    { hair: 'short', hairColorId: 'hair-black', outfit: 'overalls', outfitColorId: 'outfit-gold' },
    { hair: 'curls', hairColorId: 'hair-brown', outfit: 'space', outfitColorId: 'outfit-ocean' },
  ];
  return { ...appearance, ...(variants[index % variants.length] ?? {}) };
};
const formattedFamilyValue = computed(() => new Intl.NumberFormat('de-CH', {
  style: 'currency',
  currency: store.familyCurrencyCode,
}).format(store.familyCurrencyValue(store.availableBalance)));
const formattedGuardianFamilyValue = computed(() => new Intl.NumberFormat('de-CH', {
  style: 'currency',
  currency: store.familyCurrencyCode,
}).format(store.familyCurrencyValue(100)));

const formatRate = (value: number) => value.toLocaleString('de-DE', { minimumFractionDigits: 1, maximumFractionDigits: 2 });
const setCurrency = (value: unknown) => store.setFamilyCurrency(String(value) as FamilyCurrency);
const goalProgress = (saved: number, target: number) => Math.min(100, Math.round((saved / target) * 100));
const adjustTransferAmount = (change: number) => {
  amount.value = Math.max(0, Math.min(transferAmountMaximum.value, amount.value + change));
};
const setTransferDestination = (destination: 'goal' | 'member') => {
  transferDestination.value = destination;
  amount.value = Math.min(25, transferAmountMaximum.value);
};
const playTransfer = (direction: 'deposit' | 'withdraw' | 'gift') => {
  if (transferTimer !== undefined) window.clearTimeout(transferTimer);
  transferDirection.value = direction;
  transferTimer = window.setTimeout(() => {
    transferDirection.value = '';
    transferTimer = undefined;
  }, 1100);
};
const deposit = () => {
  if (!selectedGoalId.value) {return;}
  store.saveToGoal(selectedGoalId.value, amount.value);
  playTransfer('deposit');
  amount.value = 25;
};
const withdraw = () => {
  if (!selectedGoalId.value) {return;}
  store.withdrawFromGoal(selectedGoalId.value, amount.value);
  playTransfer('withdraw');
  amount.value = 25;
};
const giftToMember = () => {
  if (!selectedMemberId.value || amount.value <= 0) return;
  const recipientName = memberOptions.value.find((member) => member.value === selectedMemberId.value)?.title ?? 'deinem Familienmitglied';
  store.giftLadirchenToFamilyMember(selectedMemberId.value, amount.value);
  playTransfer('gift');
  window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: 'High Five!',
    message: `Dein Geschenk ist unterwegs zu ${recipientName}. Gemeinsam macht Sparen noch mehr Freude!`,
    celebration: 'gift',
  } }));
  amount.value = Math.min(25, store.availableBalance);
};
const piggyGuideSteps = computed(() => [
  {
    heading: 'Dein Guthaben',
    message: `${store.availableBalance} Ladirchen kannst du frei verwenden. ${store.totalSaved} Ladirchen arbeiten bereits in deinen Sparplänen für deine Wünsche.`,
  },
  {
    heading: 'Dein Geldweg',
    message: 'Mit Plus und Minus bestimmst du den Betrag. Danach legst du ihn in einen Sparplan oder schickst einem Familienmitglied ein Geschenk.',
  },
  {
    heading: 'Sicher gespart',
    message: 'Eigene Einzahlungen kannst du wieder für den Shop freigeben. Geschenkte Startboni bleiben geschützt, bis dein Ziel erreicht ist.',
  },
]);
const nextPiggyGuide = () => {
  piggyGuideStep.value = piggyGuideStep.value >= piggyGuideSteps.value.length - 1 ? 0 : piggyGuideStep.value + 1;
  const step = piggyGuideSteps.value[piggyGuideStep.value];
  if (!step) return;
  window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: step.heading,
    message: step.message,
    smart: true,
    progress: `${piggyGuideStep.value + 1} / ${piggyGuideSteps.value.length}`,
    actionLabel: piggyGuideStep.value === piggyGuideSteps.value.length - 1 ? 'Noch einmal' : 'Weiter',
    actionEvent: 'piggy-guide:next',
  } }));
};

watch(() => store.piggyBankOpen, (isOpen) => {
  if (guidanceTimer !== undefined) window.clearTimeout(guidanceTimer);
  if (!isOpen) return;
  if (store.viewerRole === 'guardian') return;
  selectedGoalId.value = store.ownSavingGoals.some(goal => goal.id === store.activeGoal.id)
    ? store.activeGoal.id
    : store.ownSavingGoals[0]?.id;
  selectedMemberId.value = memberOptions.value[0]?.value;
  transferDestination.value = 'goal';
  amount.value = Math.min(25, store.availableBalance);
  piggyGuideStep.value = -1;
  guidanceTimer = window.setTimeout(() => {
    nextPiggyGuide();
    guidanceTimer = undefined;
  }, 320);
});
onMounted(() => window.addEventListener('piggy-guide:next', nextPiggyGuide));
onUnmounted(() => {
  if (transferTimer !== undefined) window.clearTimeout(transferTimer);
  if (guidanceTimer !== undefined) window.clearTimeout(guidanceTimer);
  window.removeEventListener('piggy-guide:next', nextPiggyGuide);
});
</script>

<style scoped>
.piggy-dialog {
  height: min(660px, calc(100dvh - 28px));
  max-height: min(660px, calc(100dvh - 28px));
  @apply d-flex flex-column overflow-hidden;
  color: #253843;
  border: 2px solid rgba(78, 143, 221, 0.16);
  background: #fffdf8 !important;
  box-shadow:
    0 10px 0 rgba(58, 127, 174, 0.12),
    0 28px 70px rgba(62, 85, 75, 0.22) !important;
}
.piggy-dialog :deep(.v-card-text) {
  min-height: 0;
  flex: 1 1 auto;
  overflow-y: hidden;
}
.piggy-dialog--guardian :deep(.v-card-text) {
  overflow-y: auto;
}
.piggy-header {
  @apply position-relative;
  flex: 0 0 auto;
  background: #fffdf8;
}
.piggy-content {
  background: linear-gradient(180deg, #fffdf8, #f4faf7);
}
.piggy-title-row {
  min-height: 108px;
  padding: 15px 16px;
  @apply position-relative d-flex align-center justify-space-between overflow-hidden;
  gap: 10px;
  border: 2px solid rgba(78, 143, 221, 0.15);
  border-radius: 21px;
  background: linear-gradient(145deg, #fffdf8, #e7f3ff);
  box-shadow: 0 5px 0 rgba(78, 143, 221, 0.12);
}
.piggy-title-row::before,
.piggy-title-row::after {
  content: "";
  @apply position-absolute pointer-events-none;
  border-radius: 50%;
}
.piggy-title-row::before {
  width: 110px;
  height: 110px;
  top: -68px;
  right: -25px;
  background: rgba(214, 235, 255, 0.62);
  box-shadow: 0 0 0 15px rgba(226, 240, 255, 0.45);
}
.piggy-title-row::after {
  width: 78px;
  height: 24px;
  right: 42px;
  bottom: -15px;
  background: rgba(213, 234, 255, 0.55);
}
.piggy-title-copy {
  max-width: 245px;
  @apply position-relative min-w-0 flex-grow-1;
  z-index: 1;
}
.dialog-kicker {
  margin: 0 0 4px;
  color: #4e8fdd;
  font-size: 8px;
  font-weight: 950;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}
.piggy-header h2 {
  @apply ma-0;
  font-size: 23px;
  letter-spacing: -0.04em;
}
.piggy-subtitle {
  max-width: 230px;
  margin: 5px 0 0;
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.32;
}
.piggy-title-actions {
  @apply position-relative d-flex align-start flex-shrink-0;
  z-index: 2;
  gap: 5px;
}
.header-coin {
  width: 64px;
  height: 64px;
  @apply d-grid place-center flex-shrink-0;
  border-radius: 21px;
  background: rgba(255, 255, 255, 0.72);
  box-shadow: 0 4px 0 rgba(78, 143, 221, 0.12);
}
.header-coin :deep(.ladirchen-coin) {
  scale: 0.76;
  filter: drop-shadow(0 7px 7px rgba(166, 107, 19, 0.16));
}
.piggy-close {
  width: 34px;
  height: 34px;
  @apply position-absolute d-grid place-center cursor-pointer;
  top: -10px;
  right: -10px;
  z-index: 3;
  color: #35574f;
  border: 2px solid #fff;
  border-radius: 12px;
  background: #f4f8f6;
  box-shadow: 0 4px 0 rgba(82, 123, 106, 0.13);
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.piggy-close:hover {
  transform: translateY(-2px) rotate(4deg);
  box-shadow: 0 6px 0 rgba(82, 123, 106, 0.13);
}
.piggy-close:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0 rgba(82, 123, 106, 0.13);
}
.balance-grid,
.balance-detail-grid {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 9px;
}
.balance-tile {
  min-height: 68px;
  padding: 9px;
  @apply position-relative d-flex align-center min-w-0;
  gap: 8px;
  border: 2px solid rgba(73, 151, 198, 0.18);
  border-radius: 18px;
  background:
    radial-gradient(circle at 90% 8%, rgba(255, 215, 88, 0.2), transparent 27%),
    linear-gradient(145deg, #eaf7ff, #edf9f3 62%, #fff4cf);
  box-shadow: 0 5px 0 rgba(58, 127, 174, 0.12);
}
.balance-tile--plans {
  border-color: rgba(167, 118, 194, 0.17);
  background: linear-gradient(145deg, #fff1f8, #f1f0ff 58%, #fff6d8);
  box-shadow: 0 5px 0 rgba(143, 98, 157, 0.11);
}
.balance-tile.interest-earned {
  color: #247b5d;
  background: linear-gradient(145deg, #edfaf4, #f8fff3 60%, #fff3c9);
}
.balance-icon {
  width: 40px;
  height: 40px;
  @apply d-grid place-center flex-shrink-0;
  color: #fff;
  border: 3px solid #fff;
  border-radius: 14px;
  background: linear-gradient(145deg, #6bc3a0, #4387d2);
  box-shadow: 0 4px 0 #3574aa;
  transform: rotate(-5deg);
}
.balance-icon--plans {
  background: linear-gradient(145deg, #d589c7, #826ec5);
  box-shadow: 0 4px 0 #6d58a8;
}
.balance-icon--interest {
  background: linear-gradient(145deg, #6bcf91, #3b9d70);
  box-shadow: 0 4px 0 #2a7954;
}
.balance-tile > span:last-child,
.balance-tile small,
.balance-tile strong {
  @apply d-block min-w-0;
}
.balance-tile small {
  color: var(--lad-muted);
  font-size: 8px;
  line-height: 1.2;
}
.balance-tile strong {
  margin-top: 2px;
  font-size: 17px;
}
.today-earned {
  width: fit-content;
  margin-top: 4px;
  padding: 2px 5px 2px 3px;
  @apply d-flex align-center text-no-wrap;
  gap: 2px;
  color: #247b5d;
  border-radius: 999px;
  background: rgba(213, 245, 231, 0.88);
  font-size: 7px;
  font-weight: 850;
  line-height: 1;
}
.today-earned :deep(.v-icon) {
  font-size: 10px;
}
.today-earned b {
  font-weight: 950;
}
.family-currency-value {
  min-height: 68px;
  padding: 7px 8px;
  @apply position-relative d-grid min-w-0;
  grid-template-columns: 36px minmax(0, 1fr);
  align-items: center;
  column-gap: 6px;
  border: 2px solid rgba(78, 143, 221, 0.14);
  border-radius: 18px;
  background: linear-gradient(145deg, #fff, #eef7ff);
  box-shadow: 0 5px 0 rgba(78, 143, 221, 0.1);
}
.family-currency-value > .exchange-icon {
  width: 36px;
  grid-row: 1;
  scale: 0.76;
}
.family-currency-value span,
.family-currency-value strong {
  @apply d-block min-w-0;
}
.family-currency-value span {
  overflow: hidden;
  color: var(--lad-muted);
  font-size: 7px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.family-currency-value strong {
  margin-top: 2px;
  overflow: hidden;
  color: #214c3f;
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.family-currency-value small {
  grid-column: 1 / -1;
  color: #4d6860;
  font-size: 7px;
  font-weight: 850;
  line-height: 1.2;
  text-align: center;
}
.dialog-section-title {
  @apply ma-0;
  font-size: 16px;
  letter-spacing: -0.025em;
}
.conditions-card {
  border: 2px solid rgba(68, 155, 121, 0.17);
}
.guardian-balances {
  padding: 12px;
  border: 2px solid rgba(78, 143, 221, 0.16);
  border-radius: 20px;
  background:
    radial-gradient(circle at 94% 4%, rgba(255, 218, 93, 0.2), transparent 28%),
    linear-gradient(145deg, #f3faff, #f4fbf6);
  box-shadow: 0 5px 0 rgba(58, 127, 174, 0.1);
}
.guardian-balances-heading {
  @apply d-flex align-end justify-space-between;
  gap: 8px;
}
.guardian-balances-heading > span {
  padding: 4px 8px;
  color: #27795d;
  border-radius: 999px;
  background: #dff5e9;
  font-size: 8px;
  font-weight: 900;
  white-space: nowrap;
}
.guardian-balance-list {
  @apply d-flex flex-column;
  gap: 8px;
}
.guardian-balance-row {
  min-height: 62px;
  padding: 8px;
  @apply d-grid align-center min-w-0;
  grid-template-columns:
    42px minmax(76px, 1fr) repeat(3, minmax(47px, 0.62fr))
    minmax(45px, 0.56fr);
  gap: 6px;
  border: 2px solid rgba(76, 145, 119, 0.12);
  border-radius: 15px;
  background: rgba(255, 255, 255, 0.82);
  box-shadow: 0 3px 0 rgba(55, 120, 94, 0.08);
}
.guardian-child-avatar {
  width: 42px;
  height: 42px;
  @apply d-grid place-center overflow-hidden;
  border-radius: 12px;
  background: linear-gradient(145deg, #e7f5ff, #fff1bd);
}
.guardian-child-avatar :deep(.avatar-figure) {
  border: 0;
  background: transparent;
  box-shadow: none;
}
.guardian-child-name,
.guardian-child-name strong,
.guardian-child-name small,
.guardian-child-stat,
.guardian-child-stat small,
.guardian-child-stat strong {
  @apply d-block min-w-0;
}
.guardian-child-name strong {
  overflow: hidden;
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.guardian-child-name small {
  margin-top: 3px;
  color: #2d8666;
  font-size: 8px;
  font-weight: 800;
  white-space: nowrap;
}
.guardian-child-stat {
  padding-left: 5px;
  border-left: 1px solid rgba(71, 132, 110, 0.13);
}
.guardian-child-stat small {
  overflow: hidden;
  color: var(--lad-muted);
  font-size: 7.5px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.guardian-child-stat strong {
  margin-top: 2px;
  overflow: hidden;
  font-size: 11px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.guardian-child-stat--level strong {
  color: #27795d;
  font-size: 15px;
}
.currency-settings-card {
  border: 2px solid rgba(235, 171, 54, 0.23);
  background: linear-gradient(145deg, #fffaf0, #f2faf6) !important;
  box-shadow: 0 5px 0 rgba(211, 149, 44, 0.11) !important;
}
.currency-settings-heading {
  @apply d-flex align-center;
  gap: 9px;
}
.currency-settings-heading > span {
  width: 35px;
  height: 35px;
  @apply d-grid place-center flex-shrink-0;
  color: #85570e;
  border-radius: 12px;
  background: #ffe8a6;
  font-size: 17px;
  font-weight: 950;
}
.currency-settings-fields {
  @apply d-grid;
  grid-template-columns: 0.9fr 1.1fr;
  gap: 7px;
}
.currency-settings-fields :deep(.v-field) {
  background: rgba(255, 255, 255, 0.82);
}
.currency-settings-example {
  padding: 7px 9px;
  @apply d-flex align-center justify-space-between;
  gap: 8px;
  border-radius: 11px;
  background: rgba(255, 255, 255, 0.75);
}
.currency-settings-example span {
  color: var(--lad-muted);
  font-size: 8px;
}
.currency-settings-example strong {
  color: #287b5e;
  font-size: 12px;
}
.transfer-card {
  @apply position-relative overflow-hidden;
  border: 2px solid rgba(74, 157, 123, 0.18);
  background:
    radial-gradient(
      circle at 94% 4%,
      rgba(255, 213, 92, 0.25),
      transparent 27%
    ),
    linear-gradient(145deg, #fff8e8, #eef9f5 62%, #edf7ff) !important;
  box-shadow:
    0 5px 0 rgba(65, 139, 109, 0.11),
    0 12px 22px rgba(65, 126, 108, 0.07) !important;
}
.transfer-card::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 7px;
  right: 10px;
  color: #e5a52f;
  font-size: 10px;
  animation: transfer-spark 2s ease-in-out infinite;
}
.transfer-card::after {
  display: none;
}
.destination-switch {
  padding: 4px;
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 5px;
  border-radius: 16px;
  background: rgba(212, 234, 226, 0.65);
  box-shadow: inset 0 0 0 1px rgba(57, 126, 101, 0.1);
}
.destination-switch button {
  min-width: 0;
  min-height: 47px;
  padding: 6px 8px;
  @apply d-flex align-center text-left cursor-pointer;
  gap: 7px;
  color: #5d716a;
  border: 2px solid transparent;
  border-radius: 13px;
  background: transparent;
  transition:
    transform 0.16s ease,
    background 0.16s ease;
}
.destination-switch button > span:first-child {
  width: 28px;
  height: 28px;
  @apply d-grid place-center flex-shrink-0;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.72);
  font-size: 16px;
}
.destination-switch strong,
.destination-switch small {
  @apply d-block;
}
.destination-switch strong {
  font-size: 10px;
  line-height: 1.15;
}
.destination-switch small {
  margin-top: 2px;
  font-size: 7px;
  opacity: 0.75;
}
.destination-switch button.active {
  color: #236f56;
  border-color: rgba(255, 255, 255, 0.9);
  background: linear-gradient(145deg, #fff, #fff7d6);
  box-shadow: 0 3px 0 rgba(51, 123, 96, 0.13);
  transform: translateY(-1px);
}
.transfer-heading {
  @apply d-flex align-center justify-space-between;
  gap: 10px;
}
.transfer-visual {
  width: 112px;
  height: 40px;
  @apply d-flex align-center justify-space-between flex-shrink-0;
}
.transfer-wallet,
.transfer-goal {
  width: 34px;
  height: 34px;
  @apply d-grid place-center;
  z-index: 2;
  border: 3px solid #fff;
  border-radius: 12px;
  box-shadow: 0 3px 0 rgba(61, 119, 177, 0.13);
  font-weight: 950;
}
.transfer-wallet {
  color: #845510;
  background: linear-gradient(145deg, #ffe892, #ffc849);
}
.transfer-goal {
  color: #2b8265;
  background: linear-gradient(145deg, #e8fbf2, #bdebd8);
}
.transfer-track {
  height: 6px;
  @apply position-relative flex-grow-1;
  margin-inline: -3px;
  border-radius: 999px;
  background: repeating-linear-gradient(
    90deg,
    #b9d9eb 0 7px,
    transparent 7px 11px
  );
}
.transfer-track i {
  width: 9px;
  height: 9px;
  @apply position-absolute;
  top: -2px;
  left: 4px;
  border: 2px solid #fff;
  border-radius: 50%;
  background: #ffc94f;
  box-shadow: 0 2px 3px rgba(111, 77, 23, 0.18);
  opacity: 0;
}
.transfer-card.transfer-deposit .transfer-track i {
  animation: transfer-to-goal 0.95s ease-in-out forwards;
}
.transfer-card.transfer-withdraw .transfer-track i {
  animation: transfer-to-wallet 0.95s ease-in-out forwards;
}
.transfer-track i:nth-child(2) {
  animation-delay: 0.12s !important;
}
.transfer-track i:nth-child(3) {
  animation-delay: 0.24s !important;
}
.transfer-card.transfer-deposit .transfer-goal,
.transfer-card.transfer-withdraw .transfer-wallet {
  animation: transfer-receive 0.6s 0.45s ease-in-out;
}
.transfer-actions {
  grid-template-columns: 1fr 1fr;
}
.transfer-button {
  min-height: 42px !important;
  border: 2px solid rgba(255, 255, 255, 0.82) !important;
  border-radius: 14px !important;
  font-size: 9px;
  font-weight: 900;
  text-transform: none;
  letter-spacing: 0;
}
.transfer-button :deep(.v-btn__content) {
  gap: 5px;
}
.transfer-button :deep(.v-btn__content) > span {
  font-size: 15px;
}
.transfer-button--withdraw {
  color: #346a83 !important;
  background: linear-gradient(145deg, #f4fbff, #dceff9) !important;
  box-shadow: 0 4px 0 rgba(58, 125, 159, 0.16) !important;
}
.transfer-button--deposit {
  background: linear-gradient(145deg, #5cb4ed, #377fd1) !important;
  box-shadow:
    0 4px 0 #2d6eb7,
    0 8px 14px rgba(48, 119, 190, 0.16) !important;
}
.transfer-card.transfer-gift .transfer-track i {
  animation: transfer-to-goal 0.95s ease-in-out forwards;
}
.transfer-card.transfer-gift .transfer-goal {
  animation: transfer-receive 0.6s 0.45s ease-in-out;
}
.transfer-button--gift {
  color: #fff !important;
  background: linear-gradient(145deg, #55c79a, #2f9871) !important;
  box-shadow:
    0 4px 0 #267658,
    0 8px 14px rgba(38, 118, 88, 0.17) !important;
}
.transfer-title {
  @apply d-flex align-center;
  gap: 8px;
}
.transfer-title-icon {
  width: 35px;
  height: 35px;
  @apply d-flex align-center justify-center flex-shrink-0 overflow-hidden;
  color: #347f66;
  border: 2px solid #fff;
  border-radius: 12px;
  background: linear-gradient(145deg, #ddf6ea, #fff2bf);
  box-shadow: 0 3px 0 rgba(54, 131, 101, 0.14);
  animation: money-path-float 1.8s ease-in-out infinite;
}
.transfer-title-icon :deep(.ladirchen-coin) {
  width: 25px;
  height: 25px;
}
.transfer-fields {
  @apply d-grid;
  grid-template-columns: 1.4fr 0.8fr;
  gap: 8px;
}
.transfer-field {
  min-width: 0;
  padding: 7px;
  border: 2px solid rgba(68, 141, 115, 0.12);
  border-radius: 15px;
  background: rgba(255, 255, 255, 0.72);
  box-shadow: 0 3px 0 rgba(62, 121, 100, 0.08);
}
.transfer-field > span {
  margin: 0 2px 5px;
  @apply d-flex align-center;
  gap: 5px;
  color: #4c6960;
  font-size: 8px;
  font-weight: 900;
}
.transfer-field > span i {
  width: 17px;
  height: 17px;
  @apply d-grid place-center;
  color: #fff;
  border-radius: 6px;
  background: #4aa883;
  font-size: 8px;
  font-style: normal;
  transform: rotate(-4deg);
}
.transfer-input :deep(.v-field) {
  border: 0 !important;
  border-radius: 11px !important;
  background: linear-gradient(145deg, #f5fbff, #fffdf4) !important;
  box-shadow:
    inset 0 0 0 1px rgba(67, 125, 104, 0.11),
    0 2px 0 rgba(55, 110, 91, 0.07) !important;
}
.transfer-input :deep(.v-field__overlay),
.transfer-input :deep(.v-field__outline) {
  display: none;
}
.transfer-input :deep(.v-field__input) {
  min-height: 38px;
  padding-inline: 9px;
  font-size: 12px;
  font-weight: 850;
}
.transfer-input :deep(.v-field__append-inner) {
  padding-top: 8px;
}
.transfer-amount :deep(input[type="number"]) {
  appearance: textfield;
}
.transfer-amount :deep(input[type="number"]::-webkit-inner-spin-button),
.transfer-amount :deep(input[type="number"]::-webkit-outer-spin-button) {
  margin: 0;
  appearance: none;
}
.selected-goal-option {
  min-width: 0;
  @apply d-flex align-center;
  gap: 7px;
}
.selected-goal-option > span:last-child,
.selected-goal-option strong,
.selected-goal-option small {
  @apply d-block min-w-0;
}
.selected-goal-option strong {
  overflow: hidden;
  font-size: 11px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.selected-goal-option small {
  color: var(--lad-muted);
  font-size: 7px;
  font-weight: 750;
}
.goal-option-icon {
  width: 31px;
  height: 31px;
  @apply d-grid place-center flex-shrink-0;
  border: 2px solid #fff;
  border-radius: 10px;
  background: linear-gradient(145deg, #e9f6ff, #fff3bd);
  box-shadow: 0 2px 0 rgba(51, 113, 151, 0.12);
  font-size: 17px;
  transform: rotate(-4deg);
}
.goal-option {
  min-height: 57px !important;
  margin: 5px 7px;
  border: 2px solid rgba(61, 145, 111, 0.13);
  border-radius: 15px !important;
  background: linear-gradient(145deg, #f7fcff, #fffaf0);
  box-shadow: 0 3px 0 rgba(55, 120, 94, 0.08);
}
.goal-option :deep(.v-list-item-title) {
  font-size: 12px;
  font-weight: 900;
}
.goal-option :deep(.v-list-item-subtitle) {
  font-size: 8px;
  opacity: 0.72;
}
.goal-option-progress {
  padding: 4px 6px;
  color: #28775d;
  border-radius: 999px;
  background: #e2f6ec;
  font-size: 8px;
  font-weight: 950;
}
.member-option-icon {
  overflow: hidden;
  background: linear-gradient(145deg, #e8f9f1, #fff0c7);
}
.member-option-icon :deep(.avatar-figure) {
  transform-origin: center;
  animation: member-avatar-hello 3.2s ease-in-out infinite;
}
.amount-stepper {
  height: 40px;
  @apply d-grid align-center overflow-hidden;
  grid-template-columns: 36px 1fr 36px;
  border-radius: 11px;
  background: linear-gradient(145deg, #f5fbff, #fffdf4);
  box-shadow:
    inset 0 0 0 1px rgba(67, 125, 104, 0.11),
    0 2px 0 rgba(55, 110, 91, 0.07);
}
.amount-stepper button {
  width: 30px;
  height: 30px;
  @apply d-grid place-center justify-self-center cursor-pointer;
  color: #fff;
  border: 2px solid #fff;
  border-radius: 10px;
  background: linear-gradient(145deg, #65b7e9, #3f83d0);
  box-shadow: 0 2px 0 #306db2;
  font-size: 20px;
  font-weight: 900;
  line-height: 1;
  transition:
    transform 0.14s ease,
    box-shadow 0.14s ease;
}
.amount-stepper button:last-child {
  background: linear-gradient(145deg, #5cc097, #338d6c);
  box-shadow: 0 2px 0 #267254;
}
.amount-stepper button:hover:not(:disabled) {
  transform: translateY(-1px) scale(1.04);
}
.amount-stepper button:active:not(:disabled) {
  transform: translateY(2px);
  box-shadow: none;
}
.amount-stepper button:disabled {
  cursor: default;
  filter: grayscale(0.45);
  opacity: 0.35;
}
.amount-stepper output {
  @apply d-flex align-center justify-center;
  gap: 5px;
  color: #274b40;
  font-size: 15px;
  font-weight: 950;
}
.amount-stepper output :deep(.ladirchen-coin) {
  width: 20px;
  height: 20px;
}
.balance-grid,
.balance-detail-grid {
  gap: 8px;
}
.balance-tile {
  min-height: 58px;
  padding: 7px 8px;
  border-radius: 16px;
  box-shadow: 0 4px 0 rgba(58, 127, 174, 0.12);
}
.balance-icon {
  width: 36px;
  height: 36px;
  border-radius: 12px;
}
.family-currency-value {
  min-height: 58px;
  padding: 6px 7px;
  grid-template-columns: 32px minmax(0, 1fr);
  border-radius: 16px;
  box-shadow: 0 4px 0 rgba(78, 143, 221, 0.1);
}
.family-currency-value > .exchange-icon {
  width: 32px;
  scale: 0.7;
}
.family-currency-value strong {
  font-size: 11px;
}
.family-currency-value small {
  font-size: 6.5px;
}
.transfer-heading {
  justify-content: flex-start;
}
.transfer-fields {
  grid-template-columns: 1.2fr 0.9fr;
}
.transfer-field {
  padding: 6px;
}
.transfer-field > span {
  margin-bottom: 4px;
}
.transfer-input :deep(.v-field__input) {
  min-height: 36px;
  padding-inline: 8px;
  font-size: 11px;
}
.amount-stepper {
  height: 38px;
  grid-template-columns: 32px minmax(52px, 1fr) 32px;
}
.amount-stepper button {
  width: 28px;
  height: 28px;
  border-radius: 9px;
  font-size: 18px;
}
.amount-stepper output {
  @apply text-no-wrap;
  gap: 4px;
}
.transfer-button {
  min-height: 40px !important;
}
.transfer-button--gift :deep(.v-icon) {
  font-size: 17px;
}
.setting-label {
  @apply mt-3 d-flex justify-space-between;
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
@media (max-width: 400px) {
  .balance-grid {
    grid-template-columns: 1fr;
  }
}
@media (max-width: 400px) {
  .guardian-balance-row {
    min-height: 58px;
    grid-template-columns:
      36px minmax(64px, 1fr) repeat(3, minmax(39px, 0.55fr))
      minmax(32px, 0.45fr);
    gap: 3px;
    padding-inline: 5px;
  }
  .guardian-child-avatar {
    width: 36px;
    height: 36px;
  }
  .guardian-child-name strong {
    font-size: 10px;
  }
  .guardian-child-name small {
    font-size: 7px;
  }
  .guardian-child-stat {
    padding-left: 3px;
  }
  .guardian-child-stat small {
    font-size: 6.5px;
  }
  .guardian-child-stat strong {
    font-size: 9px;
  }
  .guardian-child-stat--level strong {
    font-size: 13px;
  }
}
@media (max-width: 400px) {
  .currency-settings-fields {
    grid-template-columns: 1fr;
  }
}
@keyframes transfer-to-goal {
  0% {
    opacity: 0;
    transform: translateX(0) scale(0.5);
  }
  20% {
    opacity: 1;
  }
  80% {
    opacity: 1;
  }
  100% {
    opacity: 0;
    transform: translateX(43px) scale(1.08);
  }
}
@keyframes transfer-to-wallet {
  0% {
    left: auto;
    right: 4px;
    opacity: 0;
    transform: translateX(0) scale(0.5);
  }
  20% {
    opacity: 1;
  }
  80% {
    opacity: 1;
  }
  100% {
    left: auto;
    right: 4px;
    opacity: 0;
    transform: translateX(-43px) scale(1.08);
  }
}
@keyframes transfer-receive {
  0%,
  100% {
    transform: scale(1) rotate(0);
  }
  50% {
    transform: scale(1.16) rotate(6deg);
  }
}
@keyframes transfer-spark {
  0%,
  100% {
    opacity: 0.25;
    transform: scale(0.65) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(20deg);
  }
}
@keyframes money-path-float {
  0%,
  100% {
    transform: translateY(0) rotate(-2deg);
  }
  50% {
    transform: translateY(-2px) rotate(2deg);
  }
}
@keyframes member-avatar-hello {
  0%,
  70%,
  100% {
    transform: translateY(1px) rotate(0);
  }
  80% {
    transform: translateY(-2px) rotate(-3deg);
  }
  90% {
    transform: rotate(3deg);
  }
}
:global(.piggy-summary-dialog-frame) {
  width: min(460px, calc(100vw - 32px));
  height: min(660px, calc(100dvh - 32px));
  max-height: calc(100dvh - 32px);
}
@media (prefers-reduced-motion: reduce) {
  .transfer-card::after,
  .transfer-card .transfer-track i,
  .transfer-card.transfer-deposit .transfer-goal,
  .transfer-card.transfer-withdraw .transfer-wallet,
  .member-option-icon :deep(.avatar-figure),
  .transfer-title-icon {
    animation: none;
  }
}
</style>
