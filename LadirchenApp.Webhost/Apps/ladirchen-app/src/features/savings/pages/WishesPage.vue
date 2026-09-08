<template>
  <div class="page page-padding wishes-page">
    <PageViewSwitch v-model="activeTab" class="mb-5" label="Zielansicht auswählen" :options="wishViewOptions" />

    <template v-if="activeTab !== 'family'">
      <button v-if="canCreatePersonalGoal" class="create-goal-card mb-5" type="button" @click="openGoalDialog(personalGoalOwnerId)">
        <span class="create-goal-icon"><v-icon icon="mdi-plus" /></span>
        <span><strong>Ziel anlegen</strong><small>Ein neuer Wunsch beginnt mit dem ersten Schritt.</small></span>
        <v-icon class="create-goal-arrow" icon="mdi-arrow-right" />
      </button>

      <SavingsInterestGuideCard v-if="store.viewerRole === 'child'" />

      <SectionHeader
        :description="activeTab === 'children' ? 'Alle für dich sichtbaren Ziele der Kinder auf einen Blick.' : 'Private Ziele bleiben ausschließlich bei dir.'"
        :title="activeTab === 'children' ? 'Ziele der Kinder' : 'Meine Ziele'"
      />

      <TransitionGroup class="goal-grid" name="goal-list" tag="div">
        <v-card v-for="goal in personalGoals" :key="goal.id" class="family-goal pa-4" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <div class="goal-icon">{{ goal.icon }}</div>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center justify-space-between ga-2">
                <div><strong>{{ goal.title }}</strong><p v-if="goal.ownerId !== store.signedInMemberId" class="text-caption text-medium-emphasis">{{ ownerName(goal.ownerId) }}</p></div>
                <v-chip v-if="goal.ownerId !== store.signedInMemberId" color="primary" size="x-small" variant="tonal">{{ goal.shared ? 'Gemeinsam' : visibilityLabel(goal.visibility) }}</v-chip>
              </div>
              <div class="goal-account-stats mt-3">
                <span><v-icon icon="mdi-wallet-plus-outline" /><i><small>Eingezahlt</small><strong>{{ depositedAmount(goal) }} L</strong></i></span>
                <span><v-icon icon="mdi-chart-line" /><i><small>Aktueller Zins</small><strong>{{ formatInterestRate(store.savingsInterestRate) }} %</strong></i></span>
                <span><v-icon icon="mdi-calendar-star" /><i><small>Nächste Woche</small><strong>+{{ weeklyInterestForGoal(goal) }} L</strong></i></span>
                <span><v-icon icon="mdi-star-four-points" /><i><small>Zinsen bisher</small><strong>+{{ goal.interestEarned ?? 0 }} L</strong></i></span>
              </div>
              <v-progress-linear class="mt-3" color="primary" height="8" :model-value="progress(goal.saved, goal.target)" rounded />
              <div class="d-flex align-center justify-space-between mt-2">
                <span class="goal-total"><small>Kontostand</small><strong>{{ goal.saved }} / {{ goal.target }} L</strong></span>
                <v-btn v-if="goal.ownerId === store.signedInMemberId && store.viewerRole === 'child'" class="assign-button" color="info" rounded="lg" size="small" variant="tonal" @click="openSave(goal.id)">
                  <span class="assign-coin" aria-hidden="true"><LadirchenCoin small /></span>
                  <span>Ladirchen zuordnen</span>
                  <i class="assign-spark" aria-hidden="true">✦</i>
                </v-btn>
                <v-btn v-else-if="goal.ownerId !== store.signedInMemberId" color="info" prepend-icon="mdi-gift-outline" rounded="lg" size="small" variant="tonal" @click="openSupport(goal.id)">Schenken</v-btn>
              </div>
            </div>
          </div>
        </v-card>
      </TransitionGroup>
    </template>

    <template v-else>
      <button class="create-goal-card create-family-goal-card mb-5" type="button" @click="openGoalDialog('family')">
        <span class="create-goal-icon"><v-icon icon="mdi-account-group-outline" /></span>
        <span><strong>Familienziel anlegen</strong><small>Alle dürfen dieses Ziel gemeinsam formulieren und füllen.</small></span>
        <v-icon class="create-goal-arrow" icon="mdi-plus" />
      </button>
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
                  <button
                    class="cheer-button"
                    :class="{ 'is-cheered': goal.cheered }"
                    :aria-label="`${goal.title} anfeuern`"
                    :aria-pressed="goal.cheered"
                    type="button"
                    @click="store.toggleCheer(goal.id)"
                  >
                    <span class="cheer-hands" aria-hidden="true"><i>🤚</i><i>✋</i></span>
                    <span class="cheer-spark cheer-spark--one" aria-hidden="true">✦</span>
                    <span class="cheer-spark cheer-spark--two" aria-hidden="true">✧</span>
                  </button>
                  <v-btn
                    v-if="goal.saved < goal.target && (store.viewerRole === 'child' || store.permissions.canSupportChildGoals)"
                    class="family-gift-button assign-button"
                    color="info"
                    rounded="lg"
                    size="small"
                    variant="tonal"
                    @click="openSupport(goal.id)"
                  ><span class="assign-coin" aria-hidden="true"><LadirchenCoin small /></span><span>Ladirchen schenken</span><i class="assign-spark" aria-hidden="true">✦</i></v-btn>
                </div>
              </div>
            </div>
          </div>
        </v-card>
      </TransitionGroup>
    </template>

    <v-dialog v-model="saveDialog" max-width="420">
      <v-card class="save-dialog-card" :class="{ 'is-saving': saveMotion }" rounded="xl">
        <header class="save-dialog-header">
          <div class="save-goal-icon" aria-hidden="true">{{ store.activeGoal.icon }}</div>
          <div class="flex-grow-1 min-w-0">
            <p class="eyebrow mb-1">Ladirchen ins Ziel legen</p>
            <h2>{{ store.activeGoal.title }}</h2>
            <span>{{ store.activeGoal.saved }} von {{ store.activeGoal.target }} L geschafft</span>
          </div>
          <v-btn aria-label="Zuordnen schließen" icon="mdi-close" size="small" variant="text" @click="saveDialog = false" />
        </header>

        <div class="save-dialog-content">
          <div class="save-balance">
            <span class="save-balance-coin" aria-hidden="true">L</span>
            <div><small>Frei verfügbar</small><strong>{{ store.availableBalance }} L</strong></div>
            <span class="save-journey" aria-hidden="true"><i /><i /><i /></span>
            <span class="save-goal-star" aria-hidden="true">★</span>
          </div>

          <v-slider v-model="saveAmount" class="save-slider mt-5" color="info" :disabled="maxAssignable <= 0" hide-details :max="Math.max(1, maxAssignable)" min="0" step="5" thumb-label />
          <div class="save-value"><span aria-hidden="true">L</span><strong>{{ saveAmount }}</strong><small>Ladirchen</small></div>
          <p v-if="maxAssignable <= 0" class="save-empty-note">Sobald du wieder freie Ladirchen hast, kannst du sie hier in dein Ziel legen.</p>

          <button class="save-submit" :disabled="saveAmount <= 0 || saveMotion" type="button" @click="saveToGoal">
            <span class="save-submit-coin" aria-hidden="true">L</span>
            <strong>{{ saveMotion ? 'Unterwegs zum Ziel …' : 'Ins Ziel legen' }}</strong>
            <span aria-hidden="true">→</span>
            <i aria-hidden="true">✦</i>
          </button>
        </div>
      </v-card>
    </v-dialog>

    <SavingGoalDialog v-model="goalDialog" @submit="addGoal" />

    <v-dialog v-model="supportDialog" max-width="420" :persistent="supportSending">
      <v-card v-if="supportGoal" class="support-dialog-card pa-5" :class="{ 'is-sending': supportSending }" rounded="xl">
        <div class="support-dialog-heading">
          <div class="support-icon">🎁</div>
          <div>
            <p class="eyebrow mb-1">Gemeinsam ans Ziel</p>
            <h2>{{ supportGoal.title }}</h2>
            <span>{{ ownerName(supportGoal.ownerId) }} freut sich über deine Hilfe.</span>
          </div>
        </div>
        <div class="support-journey mt-4" aria-hidden="true">
          <span class="support-coin">L</span><i /><i /><i /><span class="support-high-five">✋</span><span class="support-present">🎁</span>
        </div>
        <v-slider v-model="supportAmount" color="info" :disabled="supportMaximum <= 0" :max="Math.max(1, supportMaximum)" min="1" step="1" thumb-label />
        <div class="save-value text-center mb-2">{{ supportAmount }} L</div>
        <p class="text-caption text-medium-emphasis text-center mb-4">{{ supportExplanation }}</p>
        <div class="d-grid dialog-actions ga-2">
          <v-btn :disabled="supportSending" rounded="lg" variant="text" @click="supportDialog = false">Zurück</v-btn>
          <v-btn class="support-submit" color="info" :disabled="supportMaximum <= 0 || supportAmount <= 0 || supportSending" rounded="lg" variant="flat" @click="giveSupport">
            <span aria-hidden="true">✋</span>{{ supportSending ? 'Geschenk fliegt …' : 'High Five & schenken' }}<span aria-hidden="true">🎁</span>
          </v-btn>
        </div>
      </v-card>
    </v-dialog>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';

import SavingGoalDialog from '../components/SavingGoalDialog.vue';
import SavingsInterestGuideCard from '../components/SavingsInterestGuideCard.vue';
import PageViewSwitch from '@/shared/components/ui/PageViewSwitch.vue';
import type { PageViewOption } from '@/shared/components/ui/PageViewSwitch.vue';
import SectionHeader from '@/shared/components/ui/SectionHeader.vue';
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import type { FamilyMemberId, GoalVisibility, NewGoal, SavingGoal, SavingGoalId, SavingGoalOwnerId } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const route = useRoute();
type WishView = 'own' | 'children' | 'family';
const activeTab = ref<WishView>('own');
const saveDialog = ref(false);
const goalDialog = ref(false);
const newGoalOwnerId = ref<SavingGoalOwnerId>(store.activeChildId);
const supportDialog = ref(false);
const supportGoalId = ref<SavingGoalId>();
const supportAmount = ref(25);
const supportSending = ref(false);
const saveAmount = ref(25);
const saveMotion = ref(false);
let saveTimer: number | undefined;
let supportTimer: number | undefined;

const activeGoalRemaining = computed(() => Math.max(0, store.activeGoal.target - store.activeGoal.saved));
const maxAssignable = computed(() => Math.min(store.availableBalance, activeGoalRemaining.value));
const personalGoalOwnerId = computed<FamilyMemberId>(() => store.signedInMemberId);
const childrenGoals = computed(() => store.goals.filter((goal) => {
  const owner = goal.ownerId === 'family' ? undefined : store.members.find((member) => member.id === goal.ownerId);
  return owner?.role === 'child' && goal.visibility !== 'private';
}));
const personalGoals = computed(() => activeTab.value === 'children'
  ? childrenGoals.value
  : store.goals.filter((goal) => goal.ownerId === store.signedInMemberId));
const canCreatePersonalGoal = computed(() => activeTab.value === 'own');
const visibleFamilyGoals = computed(() => store.goals.filter((goal) => {
  if (store.viewerRole === 'guardian') {
    if (goal.ownerId === 'family') return true;
    const owner = store.members.find((member) => member.id === goal.ownerId);
    return owner?.role === 'guardian' &&
      goal.ownerId !== store.signedInMemberId &&
      (goal.visibility === 'family' || (store.permissions.canViewGuardianGoals && goal.visibility === 'guardians'));
  }

  return goal.ownerId !== store.signedInMemberId && goal.visibility === 'family';
}));
const wishViewOptions = computed<Array<PageViewOption<WishView>>>(() => {
  const ownGoals = store.goals.filter((goal) => goal.ownerId === store.signedInMemberId);
  const options: Array<PageViewOption<WishView>> = [{
    id: 'own',
    icon: 'mdi-account-star-outline',
    subtitle: `${ownGoals.length} ${ownGoals.length === 1 ? 'Wunsch' : 'Wünsche'}`,
    title: 'Meine Ziele',
  }];
  if (store.viewerRole === 'guardian') {
    options.push({ id: 'children', icon: 'mdi-account-child-outline', subtitle: `${childrenGoals.value.length} sichtbar`, title: 'Kinderziele' });
  }
  if (store.permissions.canViewFamilyGoals) options.push({ id: 'family', icon: 'mdi-account-group-outline', subtitle: `${visibleFamilyGoals.value.length} sichtbar`, title: 'Familienziele' });
  return options;
});
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
const depositedAmount = (goal: SavingGoal) => Math.max(0, goal.saved - (goal.interestEarned ?? 0));
const weeklyInterestForGoal = (goal: SavingGoal) => goal.saved <= 0 || goal.saved >= goal.target
  ? 0
  : Math.min(goal.target - goal.saved, Math.max(1, Math.round(goal.saved * (store.savingsInterestRate / 100))));
const formatInterestRate = (value: number) => value.toLocaleString('de-DE', { minimumFractionDigits: 1, maximumFractionDigits: 2 });
const ownerName = (ownerId: SavingGoalOwnerId) => ownerId === 'family' ? 'Meine Familie' : store.members.find((member) => member.id === ownerId)?.name ?? 'Familie';
const visibilityLabel = (visibility: GoalVisibility) => {
  if (visibility === 'private') return 'Nur für mich';
  if (visibility === 'guardians') return 'Mit Bezugspersonen';
  return 'Familie';
};
const saveToGoal = () => {
  if (saveAmount.value <= 0 || saveMotion.value) return;
  saveMotion.value = true;
  saveTimer = window.setTimeout(() => {
    store.saveToGoal(store.activeGoal.id, saveAmount.value);
    saveDialog.value = false;
    saveAmount.value = 25;
    saveMotion.value = false;
    saveTimer = undefined;
  }, 620);
};
const openSave = (goalId: SavingGoalId) => {
  store.activeGoalId = goalId;
  saveAmount.value = Math.min(25, store.availableBalance, Math.max(0, store.activeGoal.target - store.activeGoal.saved));
  saveDialog.value = true;
};
const openSupport = (goalId: SavingGoalId) => {
  supportGoalId.value = goalId;
  const goal = store.goals.find((item) => item.id === goalId);
  const remaining = goal ? Math.max(0, goal.target - goal.saved) : 0;
  supportAmount.value = Math.max(0, Math.min(25, store.viewerRole === 'child' ? store.availableBalance : remaining, remaining));
  supportDialog.value = true;
};
const giveSupport = () => {
  if (!supportGoalId.value || supportSending.value) {return;}
  const goalId = supportGoalId.value;
  const amount = supportAmount.value;
  const recipient = supportGoal.value ? ownerName(supportGoal.value.ownerId) : 'deiner Familie';
  supportSending.value = true;
  window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: 'High Five!',
    message: `Tolle Idee! Ladi schickt dein Geschenk jetzt zu ${recipient}.`,
    celebration: 'gift',
  } }));
  supportTimer = window.setTimeout(() => {
    if (store.viewerRole === 'child') store.giftLadirchenToGoal(goalId, amount);
    else store.supportGoal(goalId, amount);
    supportDialog.value = false;
    supportSending.value = false;
    supportTimer = undefined;
  }, 950);
};
const openGoalDialog = (ownerId: SavingGoalOwnerId) => {
  newGoalOwnerId.value = ownerId;
  goalDialog.value = true;
};
const addGoal = (goal: NewGoal) => store.addGoal(goal, newGoalOwnerId.value);
watch(
  () => route.query.new,
  (value) => { if (value === '1' && store.permissions.canManageGoals) goalDialog.value = true; },
  { immediate: true },
);
watch(
  () => route.query.family,
  (value) => {
    if (value !== '1' || !store.permissions.canManageGoals) return;
    activeTab.value = 'family';
    openGoalDialog('family');
  },
  { immediate: true },
);
watch(() => store.permissions.canViewFamilyGoals, canViewFamilyGoals => {
  if (!canViewFamilyGoals) activeTab.value = 'own';
});
watch(activeTab, (tab) => {
  if (tab !== 'family' || store.viewerRole !== 'child') return;
  window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: 'Gemeinsam motivieren',
    message: 'Hier siehst du nur Ziele, die deine Familie für dich freigegeben hat. Mit einem Anfeuern machst du jemandem Mut.',
  } }));
});
onMounted(() => {
  if (store.viewerRole !== 'child') return;
  window.setTimeout(() => window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: 'Clever sparen',
    message: 'Hier sammelst du Ladirchen für deine Wünsche oder hilfst bei sichtbaren Zielen deiner Familie mit.',
    pageIntro: true,
  } })), 350);
});
onUnmounted(() => {
  if (saveTimer !== undefined) window.clearTimeout(saveTimer);
  if (supportTimer !== undefined) window.clearTimeout(supportTimer);
});
</script>

<style scoped>
.create-goal-card {
  width: 100%;
  min-height: 66px;
  padding: 9px 12px;
  @apply d-flex align-center text-left cursor-pointer;
  gap: 10px;
  color: var(--lad-text);
  border: 2px solid rgba(78, 143, 221, 0.26);
  border-radius: 20px;
  background:
    radial-gradient(
      circle at 91% 10%,
      rgba(255, 220, 118, 0.25),
      transparent 29%
    ),
    linear-gradient(145deg, #eef8ff, #fff9e3);
  box-shadow:
    0 5px 0 rgba(67, 127, 190, 0.15),
    0 10px 19px rgba(62, 108, 153, 0.07);
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.create-goal-card:hover {
  transform: translateY(-2px);
  box-shadow:
    0 7px 0 rgba(67, 127, 190, 0.15),
    0 13px 22px rgba(62, 108, 153, 0.09);
}
.create-goal-card:active {
  transform: translateY(3px);
  box-shadow: 0 2px 0 rgba(67, 127, 190, 0.15);
}
.create-goal-icon {
  width: 43px;
  height: 43px;
  @apply d-grid place-center flex-shrink-0;
  color: white;
  border: 3px solid #fff;
  border-radius: 15px;
  background: linear-gradient(145deg, #65b8f2, #3f82d5);
  box-shadow: 0 4px 0 #326fb7;
}
.create-goal-card > span:nth-child(2) {
  @apply flex-grow-1 min-w-0;
}
.create-goal-card strong,
.create-goal-card small {
  @apply d-block;
}
.create-goal-card strong {
  font-size: 15px;
}
.create-goal-card small {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 9px;
}
.create-goal-arrow {
  color: var(--lad-blue-dark);
}
.goal-grid {
  @apply d-grid;
  gap: 11px;
}
.family-goal {
  padding: 12px !important;
  border: 2px solid color-mix(in srgb, var(--lad-blue) 16%, white);
  background: linear-gradient(145deg, #fff, #f4faff);
  box-shadow: 0 5px 0 color-mix(in srgb, var(--lad-blue) 13%, transparent) !important;
}
.goal-account-stats {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 6px;
}
.goal-account-stats > span {
  min-width: 0;
  min-height: 47px;
  padding: 5px 6px;
  @apply d-flex align-center text-left;
  gap: 5px;
  border: 1px solid rgba(73, 143, 196, 0.14);
  border-radius: 12px;
  background: linear-gradient(145deg, #eef8ff, #fff);
  box-shadow: 0 3px 0 rgba(58, 121, 166, 0.08);
}
.goal-account-stats > span:nth-child(2) {
  background: linear-gradient(145deg, #effbf5, #fff7d2);
}
.goal-account-stats > span:nth-child(3),
.goal-account-stats > span:nth-child(4) {
  background: linear-gradient(145deg, #fffaf0, #ffefb8);
}
.goal-account-stats :deep(.v-icon) {
  width: 27px;
  height: 27px;
  @apply d-grid place-center flex-shrink-0;
  color: #3d86c2;
  border: 2px solid #fff;
  border-radius: 9px;
  background: #dff1ff;
  box-shadow: 0 2px 0 rgba(49, 115, 168, 0.13);
  font-size: 16px;
  animation: account-stat-float 3s ease-in-out infinite;
}
.goal-account-stats > span:nth-child(2) :deep(.v-icon) {
  color: #27795e;
  background: #dff5e9;
  animation-delay: -0.7s;
}
.goal-account-stats > span:nth-child(3) :deep(.v-icon),
.goal-account-stats > span:nth-child(4) :deep(.v-icon) {
  color: #a66b08;
  background: #ffe8a3;
  animation-delay: -1.4s;
}
.goal-account-stats > span:nth-child(4) :deep(.v-icon) {
  animation-delay: -2.1s;
}
.goal-account-stats i,
.goal-account-stats small,
.goal-account-stats strong {
  @apply d-block min-w-0;
}
.goal-account-stats i {
  font-style: normal;
}
.goal-account-stats small {
  color: var(--lad-muted);
  font-size: 7px;
  font-weight: 800;
  line-height: 1.15;
}
.goal-account-stats strong {
  margin-top: 2px;
  color: #2b745a;
  font-size: 11px;
  line-height: 1.1;
}
.goal-account-stats > span:nth-child(3) strong,
.goal-account-stats > span:nth-child(4) strong {
  color: #956008;
}
.goal-total small,
.goal-total strong {
  @apply d-block;
}
.goal-total small {
  color: var(--lad-muted);
  font-size: 7px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.goal-total strong {
  margin-top: 1px;
  font-size: 13px;
}
@keyframes account-stat-float {
  0%,
  100% {
    transform: translateY(1px) rotate(-3deg);
  }
  50% {
    transform: translateY(-2px) rotate(3deg);
  }
}
.goal-icon {
  width: 45px;
  height: 45px;
  @apply d-grid place-center flex-shrink-0;
  border: 3px solid #fff;
  border-radius: 15px;
  background: linear-gradient(145deg, #e6f4ff, #fff0c6);
  box-shadow: 0 4px 0 rgba(78, 143, 221, 0.16);
  font-size: 24px;
  transform: rotate(-4deg);
}
@media (min-width: 560px) {
  .goal-account-stats {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
  .goal-account-stats > span {
    align-items: flex-start;
    flex-direction: column;
    min-height: 61px;
  }
  .goal-account-stats :deep(.v-icon) {
    width: 24px;
    height: 24px;
    font-size: 15px;
  }
}
.child-selector {
  margin-inline: -4px;
}
.cheer-button {
  width: 48px;
  height: 42px;
  @apply position-relative d-grid place-center flex-shrink-0;
  border: 2px solid rgba(88, 156, 215, 0.18);
  border-radius: 15px;
  color: #396f9d;
  background: linear-gradient(145deg, #eff8ff, #dceeff);
  box-shadow: 0 4px 0 rgba(65, 122, 173, 0.17);
  cursor: pointer;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease,
    background 0.18s ease;
}
.cheer-button:hover {
  transform: translateY(-2px) rotate(-2deg);
  box-shadow: 0 6px 0 rgba(65, 122, 173, 0.17);
}
.cheer-button:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0 rgba(65, 122, 173, 0.17);
}
.cheer-button:focus-visible {
  outline: 3px solid color-mix(in srgb, var(--lad-blue) 42%, transparent);
  outline-offset: 2px;
}
.cheer-button.is-cheered {
  border-color: rgba(232, 161, 39, 0.35);
  background: linear-gradient(145deg, #fff8cf, #ffe39c);
  box-shadow:
    0 4px 0 rgba(204, 133, 26, 0.2),
    0 0 18px rgba(255, 194, 66, 0.28);
}
.cheer-hands {
  @apply d-flex align-center justify-center;
  width: 34px;
  height: 27px;
}
.cheer-hands i {
  font-style: normal;
  font-size: 18px;
  transform-origin: bottom center;
}
.cheer-hands i:first-child {
  margin-right: -4px;
  transform: rotate(19deg);
  animation: cheer-left 1.8s ease-in-out infinite;
}
.cheer-hands i:last-child {
  transform: rotate(-19deg);
  animation: cheer-right 1.8s ease-in-out infinite;
}
.cheer-button.is-cheered .cheer-hands i {
  animation-duration: 0.72s;
}
.cheer-spark {
  @apply position-absolute pointer-events-none;
  color: #e7a222;
  opacity: 0.45;
  font-size: 9px;
  animation: cheer-spark 1.8s ease-in-out infinite;
}
.cheer-spark--one {
  top: 3px;
  right: 7px;
}
.cheer-spark--two {
  bottom: 4px;
  left: 7px;
  animation-delay: -0.8s;
}
.cheer-button.is-cheered .cheer-spark {
  opacity: 1;
  animation-duration: 0.8s;
}
.support-icon {
  width: 58px;
  height: 58px;
  @apply d-grid place-center;
  border-radius: 19px;
  background: #eaf6ff;
  font-size: 31px;
  animation: goal-float 2.8s ease-in-out infinite;
}
.family-gift-button {
  @apply position-relative overflow-visible;
}
.support-dialog-card {
  @apply position-relative overflow-hidden;
  border: 2px solid rgba(78, 143, 221, 0.18);
  background:
    radial-gradient(
      circle at 88% 5%,
      rgba(255, 216, 92, 0.28),
      transparent 27%
    ),
    linear-gradient(155deg, #fffdf2, #edf9f5 64%, #edf6ff) !important;
  box-shadow:
    0 9px 0 rgba(45, 109, 81, 0.13),
    0 24px 54px rgba(38, 67, 55, 0.23) !important;
}
.support-dialog-heading {
  @apply d-flex align-center;
  gap: 13px;
}
.support-dialog-heading h2 {
  @apply ma-0;
  font-size: 20px;
  line-height: 1.15;
}
.support-dialog-heading span {
  @apply d-block;
  margin-top: 4px;
  color: var(--lad-muted);
  font-size: 10px;
}
.support-journey {
  height: 54px;
  padding-inline: 8px;
  @apply position-relative d-flex align-center justify-space-between;
  border: 2px solid rgba(65, 151, 116, 0.12);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.7);
}
.support-journey > i {
  width: 10px;
  height: 5px;
  border-radius: 999px;
  background: #b9dece;
}
.support-coin,
.support-present {
  width: 36px;
  height: 36px;
  @apply d-grid place-center;
  z-index: 2;
  border: 3px solid #fff;
  box-shadow: 0 3px 0 rgba(95, 78, 41, 0.13);
}
.support-coin {
  color: #80530d;
  border-radius: 50%;
  background: #ffd25a;
  font-size: 12px;
  font-weight: 950;
}
.support-present {
  border-radius: 12px;
  background: #dff5eb;
  font-size: 20px;
}
.support-high-five {
  font-size: 25px;
}
.support-dialog-card.is-sending .support-coin {
  animation: support-coin-flight 0.85s ease-in forwards;
}
.support-dialog-card.is-sending .support-present {
  animation: support-present-receive 0.75s 0.3s ease-in-out;
}
.support-dialog-card.is-sending .support-high-five {
  animation: support-high-five 0.75s ease-in-out;
}
.support-submit {
  min-height: 43px !important;
  background: linear-gradient(145deg, #55c59a, #2f9770) !important;
  box-shadow: 0 4px 0 #267456 !important;
  font-size: 10px;
  font-weight: 900;
  text-transform: none;
  letter-spacing: 0;
}
.support-submit :deep(.v-btn__content) {
  gap: 7px;
}
@keyframes cheer-left {
  0%,
  100% {
    transform: translateX(-1px) rotate(19deg);
  }
  45%,
  55% {
    transform: translateX(3px) rotate(7deg) scale(1.05);
  }
}
@keyframes cheer-right {
  0%,
  100% {
    transform: translateX(1px) rotate(-19deg);
  }
  45%,
  55% {
    transform: translateX(-3px) rotate(-7deg) scale(1.05);
  }
}
@keyframes cheer-spark {
  0%,
  35%,
  100% {
    opacity: 0.2;
    transform: scale(0.65) rotate(0);
  }
  52%,
  72% {
    opacity: 1;
    transform: scale(1.25) rotate(25deg);
  }
}
@keyframes support-coin-flight {
  0% {
    opacity: 1;
    transform: translateX(0) rotate(0);
  }
  80% {
    opacity: 1;
  }
  100% {
    opacity: 0;
    transform: translateX(230px) rotate(220deg) scale(0.65);
  }
}
@keyframes support-present-receive {
  0%,
  100% {
    transform: scale(1) rotate(0);
  }
  52% {
    transform: scale(1.2) rotate(8deg);
  }
}
@keyframes support-high-five {
  0%,
  100% {
    transform: rotate(0) scale(1);
  }
  45% {
    transform: rotate(-18deg) scale(1.25);
  }
}
@media (prefers-reduced-motion: reduce) {
  .cheer-hands i,
  .cheer-spark,
  .support-dialog-card.is-sending .support-coin,
  .support-dialog-card.is-sending .support-present,
  .support-dialog-card.is-sending .support-high-five {
    animation: none;
  }
}
.family-goal-actions {
  @apply d-flex align-center;
  gap: 6px;
}
.assign-button {
  min-height: 38px !important;
  padding-inline: 8px !important;
  @apply position-relative overflow-visible;
  border: 2px solid rgba(255, 255, 255, 0.82) !important;
  background: linear-gradient(145deg, #eaf6ff, #cfe8ff) !important;
  box-shadow:
    0 4px 0 rgba(57, 121, 184, 0.2),
    0 8px 14px rgba(57, 121, 184, 0.1) !important;
  font-size: 9px;
  font-weight: 900;
  text-transform: none;
  letter-spacing: 0;
}
.assign-button :deep(.v-btn__content) {
  gap: 5px;
}
.assign-coin {
  width: 23px;
  height: 23px;
  @apply d-grid place-center flex-shrink-0;
  animation: assign-coin-travel 2.3s ease-in-out infinite;
}
.assign-coin :deep(.ladirchen-coin) {
  width: 23px;
  height: 23px;
}
.assign-spark {
  @apply position-absolute pointer-events-none;
  top: -7px;
  right: 5px;
  color: #e8a629;
  font-size: 10px;
  font-style: normal;
  animation: assign-spark 2.3s ease-in-out infinite;
}
.save-dialog-card {
  @apply overflow-hidden;
  border: 2px solid rgba(78, 143, 221, 0.2);
  background: linear-gradient(180deg, #fffdf8, #f5fbff) !important;
  box-shadow:
    0 9px 0 rgba(55, 103, 148, 0.15),
    0 25px 55px rgba(34, 62, 86, 0.24) !important;
}
.save-dialog-header {
  padding: 16px 14px;
  @apply d-flex align-center;
  gap: 11px;
  border-bottom: 2px solid rgba(78, 143, 221, 0.12);
  background:
    radial-gradient(
      circle at 88% 4%,
      rgba(255, 220, 103, 0.28),
      transparent 28%
    ),
    linear-gradient(145deg, #eaf7ff, #fff8dd);
}
.save-dialog-header h2 {
  @apply ma-0;
  font-size: 20px;
  letter-spacing: -0.035em;
}
.save-dialog-header div > span {
  @apply d-block;
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 9px;
}
.save-goal-icon {
  width: 54px;
  height: 54px;
  @apply d-grid place-center flex-shrink-0;
  border: 3px solid #fff;
  border-radius: 18px;
  background: linear-gradient(145deg, #e6f5ff, #fff0bd);
  box-shadow: 0 4px 0 rgba(64, 124, 182, 0.16);
  font-size: 29px;
  transform: rotate(-5deg);
}
.save-dialog-content {
  padding: 17px;
}
.save-balance {
  min-height: 65px;
  padding: 10px 12px;
  @apply d-flex align-center;
  gap: 9px;
  border: 2px solid rgba(67, 151, 117, 0.14);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.75);
  box-shadow: 0 4px 0 rgba(56, 133, 99, 0.09);
}
.save-balance-coin,
.save-goal-star {
  width: 38px;
  height: 38px;
  @apply d-grid place-center flex-shrink-0;
  border: 3px solid #fff;
  box-shadow: 0 3px 0 rgba(119, 84, 31, 0.13);
  font-weight: 950;
}
.save-balance-coin {
  color: #81540b;
  border-radius: 50%;
  background: radial-gradient(circle at 35% 28%, #fff5a3, #ffd25b 48%, #e7a027);
}
.save-goal-star {
  color: #fff3a3;
  border-radius: 13px;
  background: linear-gradient(145deg, #58bb94, #2d8869);
}
.save-balance > div {
  min-width: 70px;
}
.save-balance small,
.save-balance strong {
  @apply d-block;
}
.save-balance small {
  color: var(--lad-muted);
  font-size: 8px;
}
.save-balance strong {
  font-size: 15px;
}
.save-journey {
  height: 8px;
  @apply position-relative flex-grow-1;
  border-radius: 999px;
  background: repeating-linear-gradient(
    90deg,
    #b5d9e9 0 7px,
    transparent 7px 12px
  );
}
.save-journey i {
  width: 7px;
  height: 7px;
  @apply position-absolute;
  top: 0;
  left: 2px;
  border-radius: 50%;
  background: #ffd05b;
  opacity: 0;
}
.save-journey i:nth-child(2) {
  animation-delay: 0.22s !important;
}
.save-journey i:nth-child(3) {
  animation-delay: 0.44s !important;
}
.save-slider :deep(.v-slider-thumb__surface) {
  width: 28px;
  height: 28px;
  border: 3px solid #fff;
  box-shadow: 0 3px 0 rgba(50, 111, 173, 0.18);
}
.save-value {
  margin: 12px auto 15px;
  @apply d-flex align-center justify-center;
  gap: 7px;
  color: var(--lad-blue-dark);
}
.save-value > span {
  width: 31px;
  height: 31px;
  @apply d-grid place-center;
  color: #81540b;
  border: 2px solid #fff3b3;
  border-radius: 50%;
  background: #ffd15a;
  box-shadow: 0 3px 0 #bd791c;
  font-size: 12px;
  font-weight: 950;
}
.save-value strong {
  font-size: 34px;
  line-height: 1;
}
.save-value small {
  color: var(--lad-muted);
  font-size: 9px;
  font-weight: 850;
}
.save-empty-note {
  margin: -3px 8px 14px;
  color: var(--lad-muted);
  @apply text-center;
  font-size: 9px;
  line-height: 1.4;
}
.save-submit {
  width: 100%;
  min-height: 49px;
  padding: 8px 13px;
  @apply position-relative d-flex align-center justify-center overflow-hidden cursor-pointer;
  gap: 8px;
  color: #fff;
  border: 3px solid rgba(255, 255, 255, 0.82);
  border-radius: 17px;
  background: linear-gradient(145deg, #5db9ef, #397fd2);
  box-shadow:
    0 5px 0 #2e6db5,
    0 10px 17px rgba(48, 116, 185, 0.17);
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.save-submit:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow:
    0 7px 0 #2e6db5,
    0 13px 20px rgba(48, 116, 185, 0.2);
}
.save-submit:active:not(:disabled) {
  transform: translateY(3px);
  box-shadow: 0 2px 0 #2e6db5;
}
.save-submit:disabled {
  cursor: default;
  filter: grayscale(0.3);
  opacity: 0.48;
}
.save-submit > span:first-child {
  width: 27px;
  height: 27px;
  @apply d-grid place-center;
  color: #7c5008;
  border: 2px solid #fff3b0;
  border-radius: 50%;
  background: #ffd15a;
  font-size: 10px;
  font-weight: 950;
  animation: save-button-coin 2.2s ease-in-out infinite;
}
.save-submit > span:nth-of-type(2) {
  font-size: 18px;
}
.save-submit > i {
  @apply position-absolute;
  top: 4px;
  right: 9px;
  color: #fff2a5;
  font-size: 11px;
  font-style: normal;
  animation: save-button-spark 1.7s ease-in-out infinite;
}
.save-dialog-card.is-saving .save-journey i {
  animation: save-journey 0.58s ease-in forwards;
}
.save-dialog-card.is-saving .save-goal-star {
  animation: save-goal-receive 0.55s 0.28s ease-in-out;
}
.save-dialog-card.is-saving .save-submit-coin {
  animation: save-submit-flight 0.58s ease-in forwards;
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
@keyframes assign-coin-travel {
  0%,
  25%,
  100% {
    transform: translateX(0) rotate(0);
  }
  52% {
    transform: translateX(4px) rotate(18deg) scale(1.08);
  }
  72% {
    transform: translateX(0) rotate(0);
  }
}
@keyframes assign-spark {
  0%,
  35%,
  100% {
    opacity: 0;
    transform: scale(0.5) rotate(0);
  }
  55%,
  70% {
    opacity: 1;
    transform: scale(1.22) rotate(22deg);
  }
}
@keyframes save-button-coin {
  0%,
  25%,
  100% {
    transform: translateX(0) rotate(0);
  }
  55% {
    transform: translateX(5px) rotate(20deg) scale(1.08);
  }
}
@keyframes save-button-spark {
  0%,
  35%,
  100% {
    opacity: 0.2;
    transform: scale(0.7) rotate(0);
  }
  55% {
    opacity: 1;
    transform: scale(1.25) rotate(20deg);
  }
}
@keyframes save-journey {
  0% {
    left: 2px;
    opacity: 0;
    transform: scale(0.5);
  }
  20%,
  80% {
    opacity: 1;
  }
  100% {
    left: calc(100% - 9px);
    opacity: 0;
    transform: scale(1.15);
  }
}
@keyframes save-goal-receive {
  0%,
  100% {
    transform: scale(1) rotate(0);
  }
  50% {
    transform: scale(1.18) rotate(8deg);
  }
}
@keyframes save-submit-flight {
  0% {
    transform: translateX(0) rotate(0);
    opacity: 1;
  }
  100% {
    transform: translateX(110px) rotate(180deg) scale(0.7);
    opacity: 0;
  }
}
@media (prefers-reduced-motion: reduce) {
  .assign-coin,
  .assign-spark,
  .save-submit > span:first-child,
  .save-submit > i,
  .save-dialog-card.is-saving .save-journey i,
  .save-dialog-card.is-saving .save-goal-star,
  .save-dialog-card.is-saving .save-submit-coin {
    animation: none;
  }
}
</style>
