<template>
  <div class="page page-padding wishes-page">
    <div class="page-heading d-flex align-start justify-space-between ga-3">
      <div><p class="eyebrow">Clever sparen</p><h1>Wünsche</h1><p>{{ store.viewerRole === 'guardian' ? `Ziele für ${store.activeChild.name} und die Familie verwalten.` : 'Spare für deine eigenen Wünsche oder unterstütze die sichtbaren Ziele deiner Familie.' }}</p></div>
      <v-btn v-if="store.viewerRole === 'guardian'" aria-label="Sparziel direkt hinzufügen" color="primary" icon="mdi-plus" variant="flat" @click="goalDialog = true" />
    </div>

    <v-slide-group v-if="store.viewerRole === 'guardian'" class="child-selector mb-4" show-arrows>
      <v-slide-group-item v-for="child in childMembers" :key="child.id">
        <v-btn class="mr-2" :color="child.id === store.activeChildId ? 'info' : undefined" rounded="lg" size="small" :variant="child.id === store.activeChildId ? 'flat' : 'tonal'" @click="selectChild(child.id)">{{ child.avatar }} {{ child.name }}</v-btn>
      </v-slide-group-item>
    </v-slide-group>

    <v-btn-toggle v-model="activeTab" class="wish-tabs mb-5" color="primary" mandatory rounded="lg">
      <v-btn value="own">{{ store.viewerRole === 'guardian' ? `${store.activeChild.name}s Ziele` : 'Meine Ziele' }}</v-btn>
      <v-btn value="family">Familienziele</v-btn>
    </v-btn-toggle>

    <template v-if="activeTab === 'own'">
      <v-card class="active-goal pa-5 mb-6" color="blue-lighten-5" elevation="0" rounded="xl">
        <div class="d-flex align-start justify-space-between ga-3">
          <div>
            <p class="eyebrow mb-1">{{ store.viewerRole === 'guardian' ? `Ziel von ${store.activeChild.name}` : 'Mein aktives Ziel' }}</p>
            <div class="d-flex align-center flex-wrap ga-2">
              <h2>{{ store.activeGoal.title }}</h2>
              <v-chip color="secondary" size="x-small" variant="tonal">{{ visibilityLabel(store.activeGoal.visibility) }}</v-chip>
            </div>
          </div>
          <span class="active-goal-icon">{{ store.activeGoal.icon }}</span>
        </div>
        <div class="goal-numbers d-flex align-end justify-space-between mt-5">
          <div><strong>{{ store.activeGoal.saved }} L</strong><span>gespart</span></div>
          <div class="text-right"><strong>{{ activeGoalRemaining }} L</strong><span>noch benötigt</span></div>
        </div>
        <v-progress-linear class="my-3" color="info" height="12" :model-value="activeGoalProgress" rounded />
        <div class="d-flex align-center justify-space-between mb-3">
          <span class="text-caption text-medium-emphasis">Aktueller Wochenzins</span>
          <strong class="interest-hint">{{ store.savingsInterestRate.toLocaleString('de-DE') }} %</strong>
        </div>
        <v-btn v-if="store.viewerRole === 'child'" class="raised-button" color="info" rounded="lg" variant="flat" width="100%" @click="saveDialog = true">Ladirchen zuordnen</v-btn>
        <v-btn v-else class="raised-button" color="info" prepend-icon="mdi-gift-outline" rounded="lg" variant="flat" width="100%" @click="openSupport(store.activeGoal.id)">Ladirchen schenken</v-btn>
      </v-card>

      <div class="d-flex align-center justify-space-between mb-3">
        <div><h2 class="section-title">{{ store.viewerRole === 'guardian' ? `Weitere Ziele von ${store.activeChild.name}` : 'Meine weiteren Ziele' }}</h2><p class="text-caption text-medium-emphasis">{{ store.viewerRole === 'guardian' ? 'Private Kinderziele sind für Bezugspersonen nicht sichtbar.' : 'Private Ziele bleiben ausschließlich bei dir.' }}</p></div>
        <v-btn aria-label="Neues Sparziel" color="primary" icon="mdi-plus" size="small" variant="tonal" @click="goalDialog = true" />
      </div>

      <TransitionGroup class="goal-grid" name="goal-list" tag="div">
        <v-card v-for="goal in myOtherGoals" :key="goal.id" class="family-goal pa-4" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <div class="goal-icon">{{ goal.icon }}</div>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center justify-space-between ga-2">
                <div><strong>{{ goal.title }}</strong><p class="text-caption text-medium-emphasis">{{ ownerName(goal.ownerId) }}</p></div>
                <v-chip color="primary" size="x-small" variant="tonal">{{ goal.shared ? 'Gemeinsam' : visibilityLabel(goal.visibility) }}</v-chip>
              </div>
              <v-progress-linear class="mt-3" color="primary" height="8" :model-value="progress(goal.saved, goal.target)" rounded />
              <div class="d-flex align-center justify-space-between mt-2">
                <span class="text-caption font-weight-bold">{{ goal.saved }} / {{ goal.target }} L</span>
                <v-btn v-if="store.viewerRole === 'child'" color="info" rounded="lg" size="small" variant="tonal" @click="store.activeGoalId = goal.id">Aktivieren</v-btn>
                <v-btn v-else color="info" prepend-icon="mdi-gift-outline" rounded="lg" size="small" variant="tonal" @click="openSupport(goal.id)">Schenken</v-btn>
              </div>
            </div>
          </div>
        </v-card>
      </TransitionGroup>
    </template>

    <template v-else>
      <v-card class="family-goals-intro pa-4 mb-5" color="green-lighten-5" elevation="0" rounded="xl">
        <div class="d-flex align-center ga-3"><v-avatar color="primary" variant="tonal">👏</v-avatar><div><strong>Gemeinsam motivieren</strong><p class="text-caption text-medium-emphasis mt-1">Hier erscheinen nur Ziele, die für deine Ansicht freigegeben wurden.</p></div></div>
      </v-card>
      <TransitionGroup class="goal-grid" name="goal-list" tag="div">
        <v-card v-for="goal in visibleFamilyGoals" :key="goal.id" class="family-goal pa-4" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <div class="goal-icon">{{ goal.icon }}</div>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center justify-space-between ga-2">
                <div><strong>{{ goal.title }}</strong><p class="text-caption text-medium-emphasis">{{ ownerName(goal.ownerId) }}</p></div>
                <v-chip v-if="goal.shared" color="primary" size="x-small" variant="tonal">Gemeinsam</v-chip>
              </div>
              <v-progress-linear class="mt-3" color="primary" height="8" :model-value="progress(goal.saved, goal.target)" rounded />
              <div class="d-flex align-center justify-space-between flex-wrap ga-2 mt-2">
                <span class="text-caption font-weight-bold">{{ goal.saved }} / {{ goal.target }} L</span>
                <div class="family-goal-actions">
                  <v-btn :aria-label="`${goal.title} anfeuern`" :color="goal.cheered ? 'warning' : undefined" icon="mdi-hand-clap" size="x-small" variant="tonal" @click="store.toggleCheer(goal.id)" />
                  <v-btn
                    v-if="store.viewerRole === 'child' && goal.ownerId !== 'family'"
                    color="info"
                    prepend-icon="mdi-gift-outline"
                    rounded="lg"
                    size="small"
                    variant="tonal"
                    @click="openSupport(goal.id)"
                  >Ladirchen schenken</v-btn>
                </div>
              </div>
            </div>
          </div>
        </v-card>
      </TransitionGroup>
    </template>

    <v-dialog v-model="saveDialog" max-width="420">
      <v-card class="pa-5" rounded="xl">
        <v-card-title class="pa-0">Für {{ store.activeGoal.title }} sparen</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-5">Verfügbar: {{ store.availableBalance }} Ladirchen</v-card-subtitle>
        <v-slider v-model="saveAmount" color="info" :max="Math.min(store.availableBalance, activeGoalRemaining)" min="0" step="5" thumb-label />
        <div class="save-value text-center mb-4">{{ saveAmount }} L</div>
        <div class="d-grid dialog-actions ga-2">
          <v-btn rounded="lg" variant="text" @click="saveDialog = false">Abbrechen</v-btn>
          <v-btn color="info" :disabled="saveAmount <= 0" rounded="lg" variant="flat" @click="saveToGoal">Zuordnen</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <SavingGoalDialog v-model="goalDialog" @submit="addGoal" />

    <v-dialog v-model="supportDialog" max-width="420">
      <v-card v-if="supportGoal" class="pa-5" rounded="xl">
        <div class="support-icon mb-3">🎁</div>
        <v-card-title class="pa-0">{{ supportGoal.title }} unterstützen</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-5">{{ ownerName(supportGoal.ownerId) }} erhält die Ladirchen als Geschenk.</v-card-subtitle>
        <v-slider v-model="supportAmount" color="info" :disabled="supportMaximum <= 0" :max="Math.max(1, supportMaximum)" min="1" step="1" thumb-label />
        <div class="save-value text-center mb-2">{{ supportAmount }} L</div>
        <p class="text-caption text-medium-emphasis text-center mb-4">{{ supportExplanation }}</p>
        <div class="d-grid dialog-actions ga-2">
          <v-btn rounded="lg" variant="text" @click="supportDialog = false">Abbrechen</v-btn>
          <v-btn color="info" :disabled="supportMaximum <= 0 || supportAmount <= 0" prepend-icon="mdi-gift-outline" rounded="lg" variant="flat" @click="giveSupport">Verschenken</v-btn>
        </div>
      </v-card>
    </v-dialog>
  </div>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from 'vue';
import { useRoute } from 'vue-router';

import SavingGoalDialog from '../components/SavingGoalDialog.vue';
import type { GoalVisibility, NewGoal } from '../domain/types';
import { usePrototypeStore } from '../stores/prototype';

const store = usePrototypeStore();
const route = useRoute();
const activeTab = ref<'own' | 'family'>('own');
const saveDialog = ref(false);
const goalDialog = ref(false);
const supportDialog = ref(false);
const supportGoalId = ref('');
const supportAmount = ref(25);
const saveAmount = ref(25);

const activeGoalProgress = computed(() => progress(store.activeGoal.saved, store.activeGoal.target));
const activeGoalRemaining = computed(() => Math.max(0, store.activeGoal.target - store.activeGoal.saved));
const myOtherGoals = computed(() =>
  store.ownSavingGoals.filter((goal) => goal.id !== store.activeGoal.id),
);
const visibleFamilyGoals = computed(() =>
  store.goals.filter(
    (goal) =>
      goal.ownerId !== store.activeChildId &&
      (goal.visibility === 'family' ||
        (store.viewerRole === 'child' && goal.ownerId === store.activeChildId) ||
        (store.viewerRole === 'guardian' && goal.visibility === 'guardians')),
  ),
);
const childMembers = computed(() => store.members.filter((member) => member.role === 'child'));
const supportGoal = computed(() => store.goals.find((goal) => goal.id === supportGoalId.value));
const supportMaximum = computed(() => {
  if (!supportGoal.value) return 0;
  const remaining = Math.max(0, supportGoal.value.target - supportGoal.value.saved);
  return store.viewerRole === 'child' ? Math.min(store.availableBalance, remaining) : remaining;
});
const supportExplanation = computed(() => store.viewerRole === 'child'
  ? 'Das Geschenk wird direkt von deinem freien Guthaben abgezogen und kann nicht zurückgeholt werden.'
  : 'Das Geschenk wird nicht vom Guthaben des Kindes abgezogen.');

const progress = (saved: number, target: number) => Math.min(100, (saved / target) * 100);
const ownerName = (ownerId: string) => ownerId === 'family' ? 'Meine Familie' : store.members.find((member) => member.id === ownerId)?.name ?? 'Familie';
const visibilityLabel = (visibility: GoalVisibility) => {
  if (visibility === 'private') return 'Nur für mich';
  if (visibility === 'guardians') return 'Mit Bezugspersonen';
  return 'Familie';
};
const saveToGoal = () => {
  store.saveToGoal(store.activeGoal.id, saveAmount.value);
  saveDialog.value = false;
  saveAmount.value = 25;
};
const selectChild = (childId: string) => {
  store.selectChildForGuardian(childId);
  const firstGoal = store.goals.find((goal) => goal.ownerId === childId);
  if (firstGoal) store.activeGoalId = firstGoal.id;
};
const openSupport = (goalId: string) => {
  supportGoalId.value = goalId;
  const goal = store.goals.find((item) => item.id === goalId);
  const remaining = goal ? Math.max(0, goal.target - goal.saved) : 0;
  supportAmount.value = Math.max(0, Math.min(25, store.viewerRole === 'child' ? store.availableBalance : remaining, remaining));
  supportDialog.value = true;
};
const giveSupport = () => {
  if (store.viewerRole === 'child') store.giftLadirchenToGoal(supportGoalId.value, supportAmount.value);
  else store.supportGoal(supportGoalId.value, supportAmount.value);
  supportDialog.value = false;
};
const addGoal = (goal: NewGoal) => store.addGoal(goal);
watch(
  () => route.query.new,
  (value) => { if (value === '1' && store.viewerRole === 'guardian') goalDialog.value = true; },
  { immediate: true },
);
</script>

<style scoped>
.wish-tabs {
  width: 100%;
  display: grid;
  grid-template-columns: 1fr 1fr;
}
.wish-tabs :deep(.v-btn) {
  min-width: 0;
}
.active-goal {
  border: 1px solid rgba(78, 143, 221, 0.18);
}
.active-goal h2 {
  margin: 0;
  font-size: 21px;
  letter-spacing: -0.03em;
}
.active-goal-icon {
  font-size: 45px;
  animation: goal-float 2.8s ease-in-out infinite;
}
.goal-numbers strong {
  display: block;
  font-size: 20px;
}
.goal-numbers span {
  display: block;
  color: var(--lad-muted);
  font-size: 10px;
}
.goal-grid {
  display: grid;
  gap: 11px;
}
.family-goal {
  border: 1px solid var(--lad-border);
  box-shadow: 0 4px 0 var(--lad-border) !important;
}
.goal-icon {
  width: 47px;
  height: 47px;
  display: grid;
  place-items: center;
  flex-shrink: 0;
  border-radius: 14px;
  background: var(--lad-surface-soft);
  font-size: 25px;
}
.interest-hint {
  color: var(--lad-blue-dark);
}
.family-goals-intro {
  border: 1px solid rgba(62, 188, 140, 0.2);
}
.child-selector {
  margin-inline: -4px;
}
.support-icon {
  width: 58px;
  height: 58px;
  display: grid;
  place-items: center;
  border-radius: 19px;
  background: #eaf6ff;
  font-size: 31px;
  animation: goal-float 2.8s ease-in-out infinite;
}
.family-goal-actions {
  display: flex;
  align-items: center;
  gap: 6px;
}
.save-value {
  color: var(--lad-blue-dark);
  font-size: 30px;
  font-weight: 900;
}
.dialog-actions {
  grid-template-columns: 1fr 1.3fr;
}
.goal-list-enter-active,
.goal-list-leave-active {
  transition:
    opacity 200ms ease,
    transform 200ms ease;
}
.goal-list-enter-from,
.goal-list-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
@keyframes goal-float {
  0%,
  100% {
    transform: translateY(2px) rotate(-2deg);
  }
  50% {
    transform: translateY(-5px) rotate(2deg);
  }
}
</style>
