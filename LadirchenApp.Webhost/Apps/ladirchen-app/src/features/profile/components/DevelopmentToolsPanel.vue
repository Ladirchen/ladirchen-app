<template>
  <section class="development-tools mt-5" aria-labelledby="development-tools-title">
    <div class="development-heading">
      <div class="development-icon" aria-hidden="true"><v-icon icon="mdi-test-tube" /></div>
      <div>
        <p class="eyebrow mb-1">Nur im Entwicklungsmodus</p>
        <h2 id="development-tools-title">Entwicklung</h2>
        <p>Alle Simulatoren und Testzugänge an einem Ort.</p>
      </div>
    </div>

    <v-expansion-panels class="development-panels mt-4" variant="accordion">
      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="mdi-account-switch-outline" /><span><strong>Anmeldung simulieren</strong><small>Aktuell: {{ store.signedInMember.name }}</small></span></div>
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
              <span><strong>{{ member.name }}</strong><small>{{ member.role === 'guardian' ? 'Bezugsperson' : 'Kind' }}</small></span>
              <v-icon v-if="member.id === store.signedInMemberId" icon="mdi-check-circle" size="19" />
            </button>
          </div>
        </v-expansion-panel-text>
      </v-expansion-panel>

      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="mdi-home-lightning-bolt-outline" /><span><strong>Hausenergie simulieren</strong><small>{{ store.familyEnergy }} %</small></span></div>
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
            <span>Verändert nur die Darstellung des Prototyps.</span>
            <v-btn v-if="store.simulatedEnergy !== null" rounded="lg" size="x-small" variant="tonal" @click="store.setSimulatedEnergy(null)">Zurücksetzen</v-btn>
          </div>
        </v-expansion-panel-text>
      </v-expansion-panel>

      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="mdi-piggy-bank-outline" /><span><strong>Zins &amp; Ladi simulieren</strong><small>Kinder-Simulator und Demo-Auszahlung</small></span></div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <SavingsInterestSimulator class="embedded-simulator" />
        </v-expansion-panel-text>
      </v-expansion-panel>

      <v-expansion-panel rounded="xl">
        <v-expansion-panel-title>
          <div class="panel-title"><v-icon icon="mdi-calendar-refresh-outline" /><span><strong>Wochenabschluss simulieren</strong><small>Hausstufe und Serie testen</small></span></div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <v-alert class="mb-3" :color="store.houseMeetsMinimumEnergy ? 'success' : 'warning'" density="compact" variant="tonal">
            {{ store.houseMeetsMinimumEnergy ? `${store.familyEnergy} % Hausenergie – die nächste Stufe ist bereit.` : `${store.familyEnergy} % Hausenergie – es fehlen noch ${60 - store.familyEnergy} Prozentpunkte.` }}
          </v-alert>
          <div class="week-actions">
            <v-btn color="error" rounded="lg" size="small" variant="tonal" @click="finishWeek(false)">Serie verpasst</v-btn>
            <v-btn color="primary" :disabled="!store.houseMeetsMinimumEnergy" rounded="lg" size="small" variant="flat" @click="finishWeek(true)">Serie geschafft</v-btn>
          </div>
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>

    <v-dialog v-model="revealDialog" max-width="410">
      <v-card class="development-result pa-6 text-center" rounded="xl">
        <div class="result-house">{{ houseLevelIcon }}</div>
        <p class="eyebrow mt-3 mb-1">Wochenabschluss</p>
        <h2>{{ lastWeekSuccessful ? 'Neue Hausstufe enthüllt!' : 'Neue Woche, neuer Versuch' }}</h2>
        <p class="text-body-small text-medium-emphasis mt-2 mb-5">{{ lastWeekSuccessful ? `Eure Familienwelt ist jetzt ein ${houseLevelName}.` : `Das Haus ist jetzt wieder ein ${houseLevelName}. Alle gekauften Dinge sind noch da.` }}</p>
        <v-btn color="primary" rounded="lg" variant="flat" width="100%" @click="revealDialog = false">Schließen</v-btn>
      </v-card>
    </v-dialog>
  </section>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';

import SavingsInterestSimulator from '@/features/savings/components/SavingsInterestSimulator.vue';
import { DEFAULT_HOUSE_STAGE, HOUSE_STAGES } from '@/features/world/data/house-catalog';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const revealDialog = ref(false);
const lastWeekSuccessful = ref(true);
const currentLevel = computed(() => HOUSE_STAGES[store.houseLevel] ?? DEFAULT_HOUSE_STAGE);
const houseLevelName = computed(() => currentLevel.value.name);
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

<style scoped>
.development-tools {
  padding: 15px;
  border: 2px dashed rgba(112, 84, 171, 0.3);
  border-radius: 25px;
  background:
    radial-gradient(
      circle at 93% 5%,
      rgba(191, 160, 255, 0.22),
      transparent 25%
    ),
    linear-gradient(145deg, #f8f4ff, #eef9f5);
  box-shadow: 0 6px 0 rgba(100, 72, 156, 0.1);
}
.development-heading {
  @apply d-flex align-center;
  gap: 11px;
}
.development-heading h2 {
  @apply ma-0;
  font-size: 21px;
  letter-spacing: -0.035em;
}
.development-heading p:last-child {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 10px;
}
.development-icon {
  width: 48px;
  height: 48px;
  @apply d-grid place-center flex-shrink-0;
  color: #6d4bb1;
  border: 3px solid #fff;
  border-radius: 17px;
  background: linear-gradient(145deg, #efe6ff, #d9c5ff);
  box-shadow: 0 4px 0 rgba(96, 66, 157, 0.16);
}
.development-panels {
  gap: 8px;
}
.development-panels :deep(.v-expansion-panel) {
  border: 1px solid rgba(101, 75, 151, 0.14);
  background: rgba(255, 255, 255, 0.76);
  box-shadow: 0 4px 0 rgba(79, 58, 124, 0.08) !important;
}
.development-panels :deep(.v-expansion-panel-title) {
  min-height: 62px;
  padding: 11px 14px;
}
.development-panels :deep(.v-expansion-panel-text__wrapper) {
  padding: 5px 13px 15px;
}
.panel-title {
  @apply d-flex align-center;
  gap: 10px;
}
.panel-title > .v-icon {
  width: 36px;
  height: 36px;
  color: #5f8f7f;
  border-radius: 12px;
  background: #e8f7f1;
}
.panel-title span,
.panel-title strong,
.panel-title small {
  @apply d-block;
}
.panel-title strong {
  font-size: 13px;
}
.panel-title small {
  margin-top: 1px;
  color: var(--lad-muted);
  font-size: 9px;
}
.session-grid {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px;
}
.session-option {
  min-width: 0;
  padding: 9px;
  @apply d-flex align-center text-left cursor-pointer;
  gap: 8px;
  color: var(--lad-text);
  border: 2px solid rgba(85, 121, 112, 0.12);
  border-radius: 15px;
  background: #fff;
}
.session-option.active {
  border-color: rgba(62, 188, 140, 0.5);
  background: #eaf9f2;
}
.session-option > span:first-child {
  font-size: 22px;
}
.session-option > span:nth-child(2) {
  @apply flex-grow-1 min-w-0;
}
.session-option strong,
.session-option small {
  @apply d-block text-truncate;
}
.session-option strong {
  font-size: 11px;
}
.session-option small {
  color: var(--lad-muted);
  font-size: 8px;
}
.session-option > .v-icon {
  color: var(--lad-mint-dark);
}
.simulator-footer {
  @apply d-flex align-center justify-space-between;
  gap: 8px;
  color: var(--lad-muted);
  font-size: 9px;
}
.embedded-simulator {
  margin: 0 !important;
  border: 0;
  box-shadow: none !important;
}
.week-actions {
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}
.development-result {
  background:
    radial-gradient(
      circle at 50% 30%,
      rgba(255, 219, 105, 0.28),
      transparent 34%
    ),
    linear-gradient(145deg, #f2fbf7, #fff8df) !important;
}
.development-result h2 {
  @apply ma-0;
  font-size: 22px;
}
.result-house {
  font-size: 76px;
  animation: result-arrive 0.8s cubic-bezier(0.2, 0.9, 0.2, 1);
}
@keyframes result-arrive {
  0% {
    opacity: 0;
    transform: translateY(25px) scale(0.5) rotate(-7deg);
  }
  70% {
    transform: translateY(-5px) scale(1.08) rotate(2deg);
  }
  100% {
    opacity: 1;
    transform: none;
  }
}
@media (max-width: 390px) {
  .session-grid {
    grid-template-columns: 1fr;
  }
}
</style>
