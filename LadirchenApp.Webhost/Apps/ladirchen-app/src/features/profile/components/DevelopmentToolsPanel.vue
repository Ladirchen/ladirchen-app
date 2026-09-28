<template>
  <section class="development-tools mt-5" aria-labelledby="development-tools-title">
    <div class="development-heading">
      <div class="development-icon" aria-hidden="true"><v-icon icon="i-mdi:test-tube" /></div>
      <div>
        <p class="eyebrow mb-1">{{ t('profile.development.eyebrow') }}</p>
        <h2 id="development-tools-title">{{ t('profile.development.title') }}</h2>
        <p>{{ t('profile.development.description') }}</p>
      </div>
    </div>

    <v-expansion-panels class="development-panels mt-4" variant="accordion">
      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="i-mdi:account-switch-outline" /><span><strong>{{ t('profile.development.session.title') }}</strong><small>{{ t('profile.development.session.current', { name: store.signedInMember.name }) }}</small></span></div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <div class="session-grid">
            <button
              v-for="member in store.members"
              :key="member.id"
              class="session-option"
              :class="{ active: member.id === store.signedInMemberId }"
              type="button"
              @click="store.switchSession(member.id)"
            >
              <span>{{ member.avatar }}</span>
              <span><strong>{{ member.name }}</strong><small>{{ memberRoleLabel(member.role) }}</small></span>
              <v-icon v-if="member.id === store.signedInMemberId" icon="i-mdi:check-circle" size="19" />
            </button>
          </div>
        </v-expansion-panel-text>
      </v-expansion-panel>

      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="i-mdi:home-lightning-bolt-outline" /><span><strong>{{ t('profile.development.energy.title') }}</strong><small>{{ store.familyEnergy }} %</small></span></div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <v-slider
            color="primary"
            hide-details
            :model-value="store.familyEnergy"
            step="10"
            thumb-label
            @update:model-value="store.setSimulatedEnergy(Number($event))"
          />
          <div class="simulator-footer mt-2">
            <span>{{ t('profile.development.energy.description') }}</span>
            <v-btn v-if="store.simulatedEnergy !== null" rounded="lg" size="x-small" variant="tonal" @click="store.setSimulatedEnergy(null)">{{ t('common.reset') }}</v-btn>
          </div>
        </v-expansion-panel-text>
      </v-expansion-panel>

      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="i-mdi:piggy-bank-outline" /><span><strong>{{ t('profile.development.interest.title') }}</strong><small>{{ t('profile.development.interest.description') }}</small></span></div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <SavingsInterestSimulator class="embedded-simulator" />
        </v-expansion-panel-text>
      </v-expansion-panel>

      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="i-mdi:calendar-refresh-outline" /><span><strong>{{ t('profile.development.week.title') }}</strong><small>{{ t('profile.development.week.description') }}</small></span></div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <v-alert class="mb-3" :color="weekAlertColor" density="compact" variant="tonal">
            {{ weekAlertMessage }}
          </v-alert>
          <div class="week-actions">
            <v-btn color="error" rounded="lg" size="small" variant="tonal" @click="finishWeek(false)">{{ t('profile.development.week.failed') }}</v-btn>
            <v-btn color="primary" :disabled="!store.houseMeetsMinimumEnergy" rounded="lg" size="small" variant="flat" @click="finishWeek(true)">{{ t('profile.development.week.succeeded') }}</v-btn>
          </div>
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>

    <v-dialog v-model="revealDialog" max-width="410">
      <v-card class="development-result pa-6 text-center" rounded="xl">
        <div class="result-house">{{ houseLevelIcon }}</div>
        <p class="eyebrow mt-3 mb-1">{{ t('profile.development.result.eyebrow') }}</p>
        <h2>{{ resultTitle }}</h2>
        <p class="text-body-small text-medium-emphasis mt-2 mb-5">{{ resultDescription }}</p>
        <v-btn color="primary" rounded="lg" variant="flat" width="100%" @click="revealDialog = false">{{ t('common.close') }}</v-btn>
      </v-card>
    </v-dialog>
  </section>
</template>

<script lang="ts" setup>
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";

import SavingsInterestSimulator from "./SavingsInterestSimulator.vue";
import { DEFAULT_HOUSE_STAGE, HOUSE_STAGES } from "@/domain/house/catalog";
import { MINIMUM_HOUSE_ENERGY_PERCENT } from "@/domain/contributions/energy";
import { useFamilyWorldStore } from "@/stores/family-world";
import type { ViewerRole } from "@/domain/family/types";

const store = useFamilyWorldStore();
const { t } = useI18n();
const revealDialog = ref(false);
const lastWeekSuccessful = ref(true);
const minimumHouseEnergyPercent = MINIMUM_HOUSE_ENERGY_PERCENT;
const currentLevel = computed(() => HOUSE_STAGES[store.houseLevel] ?? DEFAULT_HOUSE_STAGE);
const houseLevelName = computed(() => t(currentLevel.value.nameKey));
const houseLevelIcon = computed(() => currentLevel.value.icon);
const weekAlertColor = computed(() => store.houseMeetsMinimumEnergy ? "success" : "warning");
const weekAlertMessage = computed(() => store.houseMeetsMinimumEnergy
  ? t("profile.development.week.ready", { energy: store.familyEnergy })
  : t("profile.development.week.missing", { energy: store.familyEnergy, missing: minimumHouseEnergyPercent - store.familyEnergy }));
const resultTitle = computed(() => t(lastWeekSuccessful.value
  ? "profile.development.result.successTitle"
  : "profile.development.result.failureTitle"));
const resultDescription = computed(() => t(lastWeekSuccessful.value
  ? "profile.development.result.successDescription"
  : "profile.development.result.failureDescription", { level: houseLevelName.value }));
const memberRoleLabel = (role: ViewerRole) => t(role === "guardian"
  ? "profile.development.session.guardian"
  : "profile.development.session.child");
const finishWeek = (successful: boolean) => {
  lastWeekSuccessful.value = successful;
  if (successful) {
    if (!store.completeWeekDemo()) return;
  } else {
    store.failWeekDemo();
  }
  revealDialog.value = true;
};
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.development-tools {
  padding: rem(15);
  border: rem(2) dashed
    color-mix(in srgb, var(--lad-color-bonus) 30%, transparent);
  border-radius: rem(25);
  background: var(--lad-gradient-bonus);
  box-shadow: 0 rem(6) 0
    color-mix(in srgb, var(--lad-color-bonus) 10%, transparent);
}
.development-heading {
  --uno: d-flex align-center;
  gap: rem(11);
}
.development-heading h2 {
  --uno: ma-0;
  font-size: rem(21);
  letter-spacing: -0.035em;
}
.development-heading p:last-child {
  margin-top: rem(2);
  color: var(--lad-muted);
  font-size: rem(10);
}
.development-icon {
  width: 3rem;
  height: 3rem;
  --uno: d-grid place-center flex-shrink-0;
  color: var(--lad-color-bonus);
  border: rem(3) solid var(--lad-surface);
  border-radius: rem(17);
  background: var(--lad-color-bonus-soft);
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-color-bonus) 16%, transparent);
}
.development-panels {
  gap: 0.5rem;
}
.development-panels :deep(.v-expansion-panel) {
  border: rem(1) solid
    color-mix(in srgb, var(--lad-color-bonus-muted) 15%, transparent);
  background: color-mix(in srgb, var(--lad-surface-raised) 75%, transparent);
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-color-bonus-strong) 8%, transparent);
}
.development-panels :deep(.v-expansion-panel-title) {
  min-height: rem(62);
  padding: rem(11) rem(14);
}
.development-panels :deep(.v-expansion-panel-text__wrapper) {
  padding: rem(5) rem(13) rem(15);
}
.panel-title {
  --uno: d-flex align-center;
  gap: rem(10);
}
.panel-title > .v-icon {
  width: 2.25rem;
  height: 2.25rem;
  color: var(--lad-muted);
  border-radius: 0.75rem;
  background: var(--lad-surface-soft);
}
.panel-title span,
.panel-title strong,
.panel-title small {
  --uno: d-block;
}
.panel-title strong {
  font-size: rem(13);
}
.panel-title small {
  margin-top: rem(1);
  color: var(--lad-muted);
  font-size: rem(9);
}
.session-grid {
  --uno: d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.5rem;
}
.session-option {
  min-width: 0;
  padding: rem(9);
  --uno: d-flex align-center text-left cursor-pointer;
  gap: 0.5rem;
  color: var(--lad-text);
  border: rem(2) solid
    color-mix(in srgb, var(--lad-color-primary-supporting) 12%, transparent);
  border-radius: rem(15);
  background: var(--lad-surface-raised);
}
.session-option.active {
  border-color: color-mix(in srgb, var(--lad-color-primary) 50%, transparent);
  background: var(--lad-surface-soft);
}
.session-option > span:first-child {
  font-size: rem(22);
}
.session-option > span:nth-child(2) {
  --uno: flex-grow-1 min-w-0;
}
.session-option strong,
.session-option small {
  --uno: d-block text-truncate;
}
.session-option strong {
  font-size: rem(11);
}
.session-option small {
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.session-option > .v-icon {
  color: var(--lad-mint-dark);
}
.simulator-footer {
  --uno: d-flex align-center justify-space-between;
  gap: 0.5rem;
  color: var(--lad-muted);
  font-size: rem(9);
}
.embedded-simulator {
  margin: 0;
  border: 0;
  box-shadow: none;
}
.week-actions {
  --uno: d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.5rem;
}
.development-result {
  background:
    radial-gradient(
      circle at 50% 30%,
      color-mix(in srgb, var(--lad-color-reward-highlight) 30%, transparent),
      transparent 34%
    ),
    linear-gradient(
      145deg,
      var(--lad-surface-soft),
      var(--lad-color-reward-soft)
    );
}
.development-result h2 {
  --uno: ma-0;
  font-size: rem(22);
}
.result-house {
  font-size: 4.75rem;
  animation: result-arrive 0.8s var(--lad-easing-pop);
}
@keyframes result-arrive {
  0% {
    opacity: 0;
    transform: translateY(rem(25)) scale(0.5) rotate(-7deg);
  }
  70% {
    transform: translateY(rem(-5)) scale(1.08) rotate(2deg);
  }
  100% {
    opacity: 1;
    transform: none;
  }
}
@include respond-down(narrow) {
  .session-grid {
    grid-template-columns: 1fr;
  }
}
</style>
