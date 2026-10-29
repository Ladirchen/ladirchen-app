<template>
  <v-card class="interest-simulator pa-4 mb-5" elevation="0" rounded="xl">
    <div class="d-flex align-start justify-space-between ga-3">
      <div>
        <p class="eyebrow mb-1">{{ t("savings.simulator.eyebrow") }}</p>
        <h3>{{ t("savings.simulator.title") }}</h3>
        <p class="text-caption text-medium-emphasis mt-1">{{ t("savings.simulator.description") }}</p>
      </div>
      <v-chip color="info" size="x-small" variant="tonal">{{ t("savings.simulator.demo") }}</v-chip>
    </div>

    <div class="simulator-preview mt-4">
      <LadiMascot :stage-id="simulatedLadiStageId" :size="68" />
      <div class="flex-grow-1">
        <span>{{ t("savings.simulator.preview") }}</span>
        <strong>{{ t(simulatedLadiStage.nameKey) }}</strong>
        <small>{{ t(simulatedLadiStage.descriptionKey) }}</small>
      </div>
      <div class="simulator-rate">
        <strong>{{ formatRate(simulatedInterestRate) }} %</strong>
        <span>{{ t("savings.simulator.weeklyRate") }}</span>
      </div>
    </div>

    <div class="simulator-label mt-4">
      {{ t("savings.simulator.completion") }}
      <strong>{{ simulatedCompletion }} %</strong>
    </div>
    <v-slider
      v-model="simulatedCompletion"
      :aria-label="t('savings.simulator.completion')"
      color="primary"
      hide-details
      max="100"
      min="0"
      step="10"
    />
    <div class="simulator-label">
      {{ t("savings.simulator.rating") }}
      <strong>{{ formatRate(simulatedRating) }} / 5</strong>
    </div>
    <v-slider
      v-model="simulatedRating"
      :aria-label="t('savings.simulator.rating')"
      color="warning"
      hide-details
      max="5"
      min="0"
      step="0.1"
    />
    <div class="simulator-label">
      {{ t("savings.simulator.streak") }}
      <strong>{{ t("savings.simulator.days", { count: simulatedStreak }) }}</strong>
    </div>
    <v-slider
      v-model="simulatedStreak"
      :aria-label="t('savings.simulator.streak')"
      color="info"
      hide-details
      max="30"
      min="0"
      step="1"
    />

    <div class="payout-estimate mt-4">
      <div>
        <span>{{ t("savings.simulator.nextPayout") }}</span>
        <strong><LadirchenAmount prefix="+" :value="simulatedWeeklyInterest" /></strong>
      </div>
      <small>{{ t("savings.simulator.calculation", { saved: store.totalSaved }) }}</small>
    </div>

    <div class="simulator-actions mt-3">
      <v-btn rounded="lg" size="small" variant="text" @click="resetSimulation">{{ t("common.reset") }}</v-btn>
      <v-btn
        color="primary"
        :disabled="simulatedWeeklyInterest <= 0"
        rounded="lg"
        size="small"
        variant="flat"
        @click="simulatePayout"
      >
        {{ t("savings.simulator.simulate") }}
      </v-btn>
    </div>

    <Transition name="payout">
      <div v-if="payoutVisible" :key="payoutVersion" class="payout-success mt-4" aria-live="polite">
        <div class="payout-visual" aria-hidden="true">
          <div class="payout-coin"><LadirchenCoin /></div>
          <AnimatedPiggyBank receiving :size="65" />
        </div>
        <div>
          <strong>{{ t("savings.simulator.payout", { amount: lastPayout }) }}</strong>
          <span>{{ t("savings.simulator.payoutDescription") }}</span>
        </div>
      </div>
    </Transition>
  </v-card>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import LadirchenCoin from "@/shared/components/LadirchenCoin.vue";
import LadirchenAmount from "@/shared/components/LadirchenAmount.vue";
import LadiMascot from "@/shared/components/LadiMascot.vue";
import AnimatedPiggyBank from "@/shared/components/AnimatedPiggyBank.vue";
import { LADI_STAGES } from "@/domain/ladi";
import type { LadiStageId } from "@/domain/ladi";
import { PAYOUT_CELEBRATION_DURATION_MS } from "@/shared/runtime-timing";
import { useFamilyWorldStore } from "@/stores/family-world";
import { familyContext, savingsInterestSimulationGateway } from "@/app/composition-root";
let payoutTimer: number | undefined;
let simulationRequest = 0;
const store = useFamilyWorldStore();
const { locale, t } = useI18n();
const simulatedCompletion = ref(0);
const simulatedRating = ref(0);
const simulatedStreak = ref(0);
const payoutVisible = ref(false);
const payoutVersion = ref(0);
const lastPayout = ref(0);
const simulatedInterestRate = ref(0);
const simulatedWeeklyInterest = ref(0);
const simulatedLadiStageId = ref<LadiStageId>("idle-ladi");
const simulatedLadiStage = computed(
  () => LADI_STAGES.find((stage) => stage.id === simulatedLadiStageId.value) ?? LADI_STAGES[LADI_STAGES.length - 1]!,
);
function formatRate(value: number) {
  return value.toLocaleString(locale.value, {
    maximumFractionDigits: 2,
    minimumFractionDigits: 1,
  });
}
function resetSimulation(): void {
  simulatedCompletion.value = store.dailyEnergy;
  simulatedRating.value = store.averageTaskRating;
  simulatedStreak.value = store.currentParticipationStreakDays;
  payoutVisible.value = false;
}
async function refreshSimulation(): Promise<void> {
  const request = ++simulationRequest;

  try {
    const result = await savingsInterestSimulationGateway.simulate(
      familyContext.activeFamilyId,
      store.activeChildId,
      simulatedStreak.value,
      simulatedCompletion.value,
      simulatedRating.value,
    );

    if (request !== simulationRequest) {
      return;
    }

    simulatedInterestRate.value = result.interestRate;
    simulatedWeeklyInterest.value = result.projectedCredit;
    simulatedLadiStageId.value = result.ladiStageId;
  } catch {
    if (request === simulationRequest) {
      simulatedInterestRate.value = 0;
      simulatedWeeklyInterest.value = 0;
      simulatedLadiStageId.value = "idle-ladi";
    }
  }
}
function simulatePayout(): void {
  lastPayout.value = simulatedWeeklyInterest.value;

  if (lastPayout.value <= 0) {
    return;
  }

  payoutVersion.value += 1;
  payoutVisible.value = true;

  if (payoutTimer !== undefined) {
    window.clearTimeout(payoutTimer);
  }

  payoutTimer = window.setTimeout(() => {
    payoutVisible.value = false;
  }, PAYOUT_CELEBRATION_DURATION_MS);
}
onMounted(resetSimulation);
watch([simulatedCompletion, simulatedRating, simulatedStreak], () => void refreshSimulation(), { immediate: true });
onUnmounted(function callback() {
  if (payoutTimer !== undefined) {
    window.clearTimeout(payoutTimer);
  }
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.interest-simulator {
  border: 1px solid var(--lad-border-info);
  background: var(--lad-gradient-info);
}
h3 {
  --uno: ma-0;
  font-size: rem(18);
  letter-spacing: -0.025em;
}
.simulator-preview {
  padding: 11px 12px;
  --uno: d-flex align-center;
  gap: 10px;
  border: 1px solid var(--lad-border-subtle);
  border-radius: 18px;
  background: color-mix(in srgb, var(--lad-surface) 72%, transparent);
}
.simulator-preview > div:nth-child(2) span,
.simulator-preview > div:nth-child(2) strong,
.simulator-preview > div:nth-child(2) small {
  --uno: d-block;
}
.simulator-preview > div:nth-child(2) span,
.simulator-preview > div:nth-child(2) small {
  color: var(--lad-muted);
  font-size: rem(10);
}
.simulator-preview > div:nth-child(2) strong {
  margin: 1px 0;
  font-size: rem(14);
}
.simulator-rate {
  min-width: 70px;
  --uno: text-right;
}
.simulator-rate strong,
.simulator-rate span {
  --uno: d-block;
}
.simulator-rate strong {
  color: var(--lad-color-info-strong);
  font-size: 1.25rem;
}
.simulator-rate span {
  color: var(--lad-muted);
  font-size: rem(9);
}
.simulator-label {
  margin-top: 9px;
  --uno: d-flex align-center justify-space-between;
  color: var(--lad-muted);
  font-size: rem(11);
}
.simulator-label strong {
  color: var(--lad-text);
}
.payout-estimate {
  padding: 11px 12px;
  border-radius: 14px;
  background: color-mix(in srgb, var(--lad-surface-raised) 75%, transparent);
}
.payout-estimate > div {
  --uno: d-flex align-center justify-space-between ga-3;
}
.payout-estimate span,
.payout-estimate small {
  color: var(--lad-muted);
  font-size: rem(10);
}
.payout-estimate strong {
  color: var(--lad-mint-dark);
  font-size: rem(22);
}
.payout-estimate small {
  --uno: d-block;
  margin-top: 3px;
  line-height: 1.35;
}
.simulator-actions {
  --uno: d-flex align-center justify-end;
  gap: 6px;
}
.payout-success {
  --uno: pa-3 d-flex align-center ga-3 overflow-hidden;
  border: 1px solid var(--lad-border-success);
  border-radius: 17px;
  background: var(--lad-gradient-success);
  box-shadow: 0 5px 0 var(--lad-shadow-raised-success);
}
.payout-success > div:last-child strong,
.payout-success > div:last-child span {
  --uno: d-block;
}
.payout-success > div:last-child strong {
  color: var(--lad-color-success-strong);
  font-size: rem(18);
}
.payout-success > div:last-child span {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: rem(10);
  line-height: 1.35;
}
.payout-visual {
  width: 96px;
  height: 70px;
  --uno: position-relative d-grid place-center;
  flex: 0 0 96px;
}
.payout-visual > .animated-piggy {
  --uno: position-absolute right-0;
}
.payout-coin {
  --uno: position-absolute;
  z-index: 2;
  left: 9px;
  animation: payout-coin-flight 1.05s 0.12s cubic-bezier(0.22, 0.8, 0.26, 1) both;
}
.payout-enter-active,
.payout-leave-active {
  transition:
    opacity 0.3s ease,
    transform 0.3s cubic-bezier(0.2, 0.9, 0.25, 1);
}
.payout-enter-from {
  opacity: 0;
  transform: translateY(14px) scale(0.92);
}
.payout-leave-to {
  opacity: 0;
  transform: translateY(-8px) scale(0.96);
}
@keyframes payout-coin-flight {
  0% {
    opacity: 1;
    transform: translate(-58px, -18px) rotate(-160deg) scale(0.55);
  }
  58% {
    opacity: 1;
    transform: translate(-4px, -13px) rotate(160deg) scale(0.72);
  }
  100% {
    opacity: 0;
    transform: translate(42px, 8px) rotate(420deg) scale(0.12);
  }
}
@include respond-down(small) {
  .simulator-preview {
    --uno: align-start flex-wrap;
  }
  .simulator-rate {
    --uno: ms-auto;
  }
  .simulator-actions {
    --uno: align-stretch flex-column-reverse;
  }
}
@include reduced-motion {
  .payout-enter-active,
  .payout-leave-active {
    transition: none;
  }
  .payout-coin {
    animation: none;
  }
}
</style>
