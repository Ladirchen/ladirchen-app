<template>
  <section class="development-tools mt-5" aria-labelledby="development-tools-title">
    <div class="development-heading">
      <div class="development-icon" aria-hidden="true"><v-icon icon="mdi-test-tube" /></div>
      <div>
        <p class="eyebrow mb-1">{{ t('profile.development.eyebrow') }}</p>
        <h2 id="development-tools-title">{{ t('profile.development.title') }}</h2>
        <p>{{ t('profile.development.description') }}</p>
      </div>
    </div>

    <v-expansion-panels class="development-panels mt-4" variant="accordion">
      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="mdi-account-switch-outline" /><span><strong>{{ t('profile.development.session.title') }}</strong><small>{{ t('profile.development.session.current', { name: store.signedInMember.name }) }}</small></span></div>
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
              <span><strong>{{ member.name }}</strong><small>{{ t(member.role === 'guardian' ? 'profile.development.session.guardian' : 'profile.development.session.child') }}</small></span>
              <v-icon v-if="member.id === store.signedInMemberId" icon="mdi-check-circle" size="19" />
            </button>
          </div>
        </v-expansion-panel-text>
      </v-expansion-panel>

      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="mdi-home-lightning-bolt-outline" /><span><strong>{{ t('profile.development.energy.title') }}</strong><small>{{ store.familyEnergy }} %</small></span></div>
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
          <div class="panel-title"><v-icon icon="mdi-piggy-bank-outline" /><span><strong>{{ t('profile.development.interest.title') }}</strong><small>{{ t('profile.development.interest.description') }}</small></span></div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <SavingsInterestSimulator class="embedded-simulator" />
        </v-expansion-panel-text>
      </v-expansion-panel>

      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="mdi-calendar-refresh-outline" /><span><strong>{{ t('profile.development.week.title') }}</strong><small>{{ t('profile.development.week.description') }}</small></span></div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <v-alert class="mb-3" :color="store.houseMeetsMinimumEnergy ? 'success' : 'warning'" density="compact" variant="tonal">
            {{ store.houseMeetsMinimumEnergy
              ? t('profile.development.week.ready', { energy: store.familyEnergy })
              : t('profile.development.week.missing', { energy: store.familyEnergy, missing: minimumHouseEnergyPercent - store.familyEnergy }) }}
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
        <h2>{{ t(lastWeekSuccessful ? 'profile.development.result.successTitle' : 'profile.development.result.failureTitle') }}</h2>
        <p class="text-body-small text-medium-emphasis mt-2 mb-5">{{ t(lastWeekSuccessful ? 'profile.development.result.successDescription' : 'profile.development.result.failureDescription', { level: houseLevelName }) }}</p>
        <v-btn color="primary" rounded="lg" variant="flat" width="100%" @click="revealDialog = false">{{ t('common.close') }}</v-btn>
      </v-card>
    </v-dialog>
  </section>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import SavingsInterestSimulator from '@/features/savings/components/SavingsInterestSimulator.vue';
import { DEFAULT_HOUSE_STAGE, HOUSE_STAGES } from '@/domain/house-catalog';
import { MINIMUM_HOUSE_ENERGY_PERCENT } from '@/domain/energy';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const { t } = useI18n();
const revealDialog = ref(false);
const lastWeekSuccessful = ref(true);
const minimumHouseEnergyPercent = MINIMUM_HOUSE_ENERGY_PERCENT;
const currentLevel = computed(() => HOUSE_STAGES[store.houseLevel] ?? DEFAULT_HOUSE_STAGE);
const houseLevelName = computed(() => t(currentLevel.value.nameKey));
const houseLevelIcon = computed(() => currentLevel.value.icon);
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
  padding: 0.9375rem;
  border: 0.125rem dashed
    color-mix(in srgb, var(--lad-color-bonus) 30%, transparent);
  border-radius: 1.5625rem;
  background: var(--lad-gradient-bonus);
  box-shadow: 0 0.375rem 0
    color-mix(in srgb, var(--lad-color-bonus) 10%, transparent);
}
.development-heading {
  @apply d-flex align-center;
  gap: 0.6875rem;
}
.development-heading h2 {
  @apply ma-0;
  font-size: 1.3125rem;
  letter-spacing: -0.035em;
}
.development-heading p:last-child {
  margin-top: 0.125rem;
  color: var(--lad-muted);
  font-size: 0.625rem;
}
.development-icon {
  width: 3rem;
  height: 3rem;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-color-bonus);
  border: 0.1875rem solid var(--lad-surface);
  border-radius: 1.0625rem;
  background: var(--lad-color-bonus-soft);
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-color-bonus) 16%, transparent);
}
.development-panels {
  gap: 0.5rem;
}
.development-panels :deep(.v-expansion-panel) {
  border: 0.0625rem solid
    color-mix(in srgb, var(--lad-palette-violet-500) 15%, transparent);
  background: color-mix(in srgb, var(--lad-palette-white) 75%, transparent);
  box-shadow: 0 0.25rem 0
    color-mix(in srgb, var(--lad-palette-violet-650) 8%, transparent) !important;
}
.development-panels :deep(.v-expansion-panel-title) {
  min-height: 3.875rem;
  padding: 0.6875rem 0.875rem;
}
.development-panels :deep(.v-expansion-panel-text__wrapper) {
  padding: 0.3125rem 0.8125rem 0.9375rem;
}
.panel-title {
  @apply d-flex align-center;
  gap: 0.625rem;
}
.panel-title > .v-icon {
  width: 2.25rem;
  height: 2.25rem;
  color: var(--lad-palette-muted);
  border-radius: 0.75rem;
  background: var(--lad-palette-background);
}
.panel-title span,
.panel-title strong,
.panel-title small {
  @apply d-block;
}
.panel-title strong {
  font-size: 0.8125rem;
}
.panel-title small {
  margin-top: 0.0625rem;
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.session-grid {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.5rem;
}
.session-option {
  min-width: 0;
  padding: 0.5625rem;
  @apply d-flex align-center text-left cursor-pointer;
  gap: 0.5rem;
  color: var(--lad-text);
  border: 0.125rem solid
    color-mix(in srgb, var(--lad-palette-teal-600) 12%, transparent);
  border-radius: 0.9375rem;
  background: var(--lad-palette-white);
}
.session-option.active {
  border-color: color-mix(in srgb, var(--lad-palette-mint) 50%, transparent);
  background: var(--lad-palette-background);
}
.session-option > span:first-child {
  font-size: 1.375rem;
}
.session-option > span:nth-child(2) {
  @apply flex-grow-1 min-w-0;
}
.session-option strong,
.session-option small {
  @apply d-block text-truncate;
}
.session-option strong {
  font-size: 0.6875rem;
}
.session-option small {
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.session-option > .v-icon {
  color: var(--lad-mint-dark);
}
.simulator-footer {
  @apply d-flex align-center justify-space-between;
  gap: 0.5rem;
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.embedded-simulator {
  margin: 0 !important;
  border: 0;
  box-shadow: none !important;
}
.week-actions {
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.5rem;
}
.development-result {
  background:
    radial-gradient(
      circle at 50% 30%,
      color-mix(in srgb, var(--lad-palette-amber-250) 30%, transparent),
      transparent 34%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-amber-100)
    ) !important;
}
.development-result h2 {
  @apply ma-0;
  font-size: 1.375rem;
}
.result-house {
  font-size: 4.75rem;
  animation: result-arrive 0.8s cubic-bezier(0.2, 0.9, 0.2, 1);
}
@keyframes result-arrive {
  0% {
    opacity: 0;
    transform: translateY(1.5625rem) scale(0.5) rotate(-7deg);
  }
  70% {
    transform: translateY(-0.3125rem) scale(1.08) rotate(2deg);
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
