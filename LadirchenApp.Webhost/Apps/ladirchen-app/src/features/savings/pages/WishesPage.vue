<template>
  <div class="page page-padding wishes-page">
    <PageViewSwitch v-model="activeTab" class="mb-5" :label="t('wishes.viewLabel')" :options="wishViewOptions" />

    <template v-if="activeTab !== 'family'">
      <button v-if="canCreatePersonalGoal" class="create-goal-card mb-5" type="button" @click="openGoalDialog(personalGoalOwnerId)">
        <span class="create-goal-icon"><v-icon icon="mdi-plus" /></span>
        <span><strong>{{ t('wishes.create.title') }}</strong><small>{{ t('wishes.create.description') }}</small></span>
        <v-icon class="create-goal-arrow" icon="mdi-arrow-right" />
      </button>

      <SavingsInterestGuideCard v-if="store.viewerRole === 'child'" />

      <SectionHeader
        :description="t(activeTab === 'children' ? 'wishes.sections.childrenDescription' : 'wishes.sections.ownDescription')"
        :title="t(activeTab === 'children' ? 'wishes.sections.childrenTitle' : 'wishes.sections.ownTitle')"
      />

      <TransitionGroup class="goal-grid" name="goal-list" tag="div">
        <v-card v-for="goal in personalGoals" :key="goal.id" class="family-goal pa-4" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <div class="goal-icon">{{ goal.icon }}</div>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center justify-space-between ga-2">
                <div><strong>{{ goal.title }}</strong><p v-if="goal.ownerId !== store.signedInMemberId" class="text-caption text-medium-emphasis">{{ ownerName(goal.ownerId) }}</p></div>
                <v-chip v-if="goal.ownerId !== store.signedInMemberId" color="primary" size="x-small" variant="tonal">{{ goal.shared ? t('wishes.shared') : visibilityLabel(goal.visibility) }}</v-chip>
              </div>
              <div class="goal-account-stats mt-3">
                <span><v-icon icon="mdi-wallet-plus-outline" /><i><small>{{ t('wishes.stats.deposited') }}</small><strong>{{ depositedAmount(goal) }} L</strong></i></span>
                <span><v-icon icon="mdi-chart-line" /><i><small>{{ t('wishes.stats.interest') }}</small><strong>{{ formatInterestRate(store.savingsInterestRate) }} %</strong></i></span>
                <span><v-icon icon="mdi-calendar-star" /><i><small>{{ t('wishes.stats.nextWeek') }}</small><strong>+{{ weeklyInterestForGoal(goal) }} L</strong></i></span>
                <span><v-icon icon="mdi-star-four-points" /><i><small>{{ t('wishes.stats.earned') }}</small><strong>+{{ goal.interestEarned ?? 0 }} L</strong></i></span>
              </div>
              <v-progress-linear class="mt-3" color="primary" height="8" :model-value="progress(goal.saved, goal.target)" rounded />
              <div class="d-flex align-center justify-space-between mt-2">
                <span class="goal-total"><small>{{ t('wishes.stats.balance') }}</small><strong>{{ goal.saved }} / {{ goal.target }} L</strong></span>
                <v-btn v-if="goal.ownerId === store.signedInMemberId && store.viewerRole === 'child'" class="assign-button" color="info" rounded="lg" size="small" variant="tonal" @click="openSave(goal.id)">
                  <span class="assign-coin" aria-hidden="true"><LadirchenCoin small /></span>
                  <span>{{ t('wishes.assign') }}</span>
                  <i class="assign-spark" aria-hidden="true">✦</i>
                </v-btn>
                <v-btn v-else-if="goal.ownerId !== store.signedInMemberId" color="info" prepend-icon="mdi-gift-outline" rounded="lg" size="small" variant="tonal" @click="openSupport(goal.id)">{{ t('wishes.gift') }}</v-btn>
              </div>
            </div>
          </div>
        </v-card>
      </TransitionGroup>
    </template>

    <template v-else>
      <button class="create-goal-card create-family-goal-card mb-5" type="button" @click="openGoalDialog('family')">
        <span class="create-goal-icon"><v-icon icon="mdi-account-group-outline" /></span>
        <span><strong>{{ t('wishes.createFamily.title') }}</strong><small>{{ t('wishes.createFamily.description') }}</small></span>
        <v-icon class="create-goal-arrow" icon="mdi-plus" />
      </button>
      <TransitionGroup class="goal-grid" name="goal-list" tag="div">
        <v-card v-for="goal in visibleFamilyGoals" :key="goal.id" class="family-goal pa-4" elevation="0" rounded="xl">
          <div class="d-flex align-start ga-3">
            <div class="goal-icon">{{ goal.icon }}</div>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center justify-space-between ga-2">
                <div><strong>{{ goal.title }}</strong><p class="text-caption text-medium-emphasis">{{ ownerName(goal.ownerId) }}</p></div>
                <v-chip v-if="goal.shared" color="primary" size="x-small" variant="tonal">{{ t('wishes.shared') }}</v-chip>
              </div>
              <v-progress-linear class="mt-3" color="primary" height="8" :model-value="progress(goal.saved, goal.target)" rounded />
              <div class="d-flex align-center justify-space-between flex-wrap ga-2 mt-2">
                <span class="text-caption font-weight-bold">{{ goal.saved }} / {{ goal.target }} L</span>
                <div class="family-goal-actions">
                  <button
                    class="cheer-button"
                    :class="{ 'is-cheered': goal.cheered }"
                    :aria-label="t('wishes.cheerAria', { title: goal.title })"
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
                  ><span class="assign-coin" aria-hidden="true"><LadirchenCoin small /></span><span>{{ t('wishes.giftCoins') }}</span><i class="assign-spark" aria-hidden="true">✦</i></v-btn>
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
          <div class="save-goal-icon" aria-hidden="true">{{ activeGoal.icon }}</div>
          <div class="flex-grow-1 min-w-0">
            <p class="eyebrow mb-1">{{ t('wishes.save.title') }}</p>
            <h2>{{ activeGoal.title }}</h2>
            <span>{{ t('wishes.save.progress', { saved: activeGoal.saved, target: activeGoal.target }) }}</span>
          </div>
          <v-btn :aria-label="t('wishes.save.close')" icon="mdi-close" size="small" variant="text" @click="saveDialog = false" />
        </header>

        <div class="save-dialog-content">
          <div class="save-balance">
            <span class="save-balance-coin" aria-hidden="true">L</span>
            <div><small>{{ t('wishes.save.available') }}</small><strong>{{ store.availableBalance }} L</strong></div>
            <span class="save-journey" aria-hidden="true"><i /><i /><i /></span>
            <span class="save-goal-star" aria-hidden="true">★</span>
          </div>

          <v-slider v-model="saveAmount" class="save-slider mt-5" color="info" :disabled="maxAssignable <= 0" hide-details :max="Math.max(1, maxAssignable)" min="0" step="5" thumb-label />
          <div class="save-value"><span aria-hidden="true">L</span><strong>{{ saveAmount }}</strong><small>{{ t('wishes.save.currency') }}</small></div>
          <p v-if="maxAssignable <= 0" class="save-empty-note">{{ t('wishes.save.empty') }}</p>

          <button class="save-submit" :disabled="saveAmount <= 0 || saveMotion" type="button" @click="saveToGoal">
            <span class="save-submit-coin" aria-hidden="true">L</span>
            <strong>{{ t(saveMotion ? 'wishes.save.sending' : 'wishes.save.submit') }}</strong>
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
            <p class="eyebrow mb-1">{{ t('wishes.support.eyebrow') }}</p>
            <h2>{{ supportGoal.title }}</h2>
            <span>{{ t('wishes.support.description', { name: ownerName(supportGoal.ownerId) }) }}</span>
          </div>
        </div>
        <div class="support-journey mt-4" aria-hidden="true">
          <span class="support-coin">L</span><i /><i /><i /><span class="support-high-five">✋</span><span class="support-present">🎁</span>
        </div>
        <v-slider v-model="supportAmount" color="info" :disabled="supportMaximum <= 0" :max="Math.max(1, supportMaximum)" min="1" step="1" thumb-label />
        <div class="save-value text-center mb-2">{{ supportAmount }} L</div>
        <p class="text-caption text-medium-emphasis text-center mb-4">{{ supportExplanation }}</p>
        <div class="d-grid dialog-actions ga-2">
          <v-btn :disabled="supportSending" rounded="lg" variant="text" @click="supportDialog = false">{{ t('wishes.back') }}</v-btn>
          <v-btn class="support-submit" color="info" :disabled="supportMaximum <= 0 || supportAmount <= 0 || supportSending" rounded="lg" variant="flat" @click="giveSupport">
            <span aria-hidden="true">✋</span>{{ t(supportSending ? 'wishes.support.sending' : 'wishes.support.submit') }}<span aria-hidden="true">🎁</span>
          </v-btn>
        </div>
      </v-card>
    </v-dialog>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';

import SavingGoalDialog from '../components/SavingGoalDialog.vue';
import SavingsInterestGuideCard from '../components/SavingsInterestGuideCard.vue';
import PageViewSwitch from '@/shared/components/ui/PageViewSwitch.vue';
import type { PageViewOption } from '@/shared/components/ui/PageViewSwitch.vue';
import SectionHeader from '@/shared/components/ui/SectionHeader.vue';
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import type { FamilyMemberId, GoalVisibility, NewGoal, SavingGoal, SavingGoalId, SavingGoalOwnerId } from '@/domain/types';
import { useLocalizedDomainContent } from '@/shared/composables/use-localized-domain-content';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const route = useRoute();
const { locale, t } = useI18n();
const localize = useLocalizedDomainContent();
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

const localizedGoals = computed(() => store.goals.map(localize.goal));
const activeGoal = computed(() => localize.goal(store.activeGoal));
const activeGoalRemaining = computed(() => Math.max(0, activeGoal.value.target - activeGoal.value.saved));
const maxAssignable = computed(() => Math.min(store.availableBalance, activeGoalRemaining.value));
const personalGoalOwnerId = computed<FamilyMemberId>(() => store.signedInMemberId);
const childrenGoals = computed(() => localizedGoals.value.filter((goal) => {
  const owner = goal.ownerId === 'family' ? undefined : store.members.find((member) => member.id === goal.ownerId);
  return owner?.role === 'child' && goal.visibility !== 'private';
}));
const personalGoals = computed(() => activeTab.value === 'children'
  ? childrenGoals.value
  : localizedGoals.value.filter((goal) => goal.ownerId === store.signedInMemberId));
const canCreatePersonalGoal = computed(() => activeTab.value === 'own');
const visibleFamilyGoals = computed(() => localizedGoals.value.filter((goal) => {
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
  const ownGoals = localizedGoals.value.filter((goal) => goal.ownerId === store.signedInMemberId);
  const options: Array<PageViewOption<WishView>> = [{
    id: 'own',
    icon: 'mdi-account-star-outline',
    subtitle: t('wishes.views.ownCount', { count: ownGoals.length }),
    title: t('wishes.views.own'),
  }];
  if (store.viewerRole === 'guardian') {
    options.push({ id: 'children', icon: 'mdi-account-child-outline', subtitle: t('wishes.views.visibleCount', { count: childrenGoals.value.length }), title: t('wishes.views.children') });
  }
  if (store.permissions.canViewFamilyGoals) options.push({ id: 'family', icon: 'mdi-account-group-outline', subtitle: t('wishes.views.visibleCount', { count: visibleFamilyGoals.value.length }), title: t('wishes.views.family') });
  return options;
});
const supportGoal = computed(() => localizedGoals.value.find((goal) => goal.id === supportGoalId.value));
const supportMaximum = computed(() => {
  if (!supportGoal.value) return 0;
  const remaining = Math.max(0, supportGoal.value.target - supportGoal.value.saved);
  return store.viewerRole === 'child' ? Math.min(store.availableBalance, remaining) : remaining;
});
const supportExplanation = computed(() => store.viewerRole === 'child'
  ? t('wishes.support.childExplanation')
  : t('wishes.support.guardianExplanation'));

const progress = (saved: number, target: number) => Math.min(100, (saved / target) * 100);
const depositedAmount = (goal: SavingGoal) => Math.max(0, goal.saved - (goal.interestEarned ?? 0));
const weeklyInterestForGoal = (goal: SavingGoal) => goal.saved <= 0 || goal.saved >= goal.target
  ? 0
  : Math.min(goal.target - goal.saved, Math.max(1, Math.round(goal.saved * (store.savingsInterestRate / 100))));
const formatInterestRate = (value: number) => value.toLocaleString(locale.value, { minimumFractionDigits: 1, maximumFractionDigits: 2 });
const ownerName = (ownerId: SavingGoalOwnerId) => ownerId === 'family' ? t('wishes.owner.myFamily') : store.members.find((member) => member.id === ownerId)?.name ?? t('wishes.owner.family');
const visibilityLabel = (visibility: GoalVisibility) => {
  return t(`wishes.visibility.${visibility}`);
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
  const recipient = supportGoal.value ? ownerName(supportGoal.value.ownerId) : t('wishes.owner.yourFamily');
  supportSending.value = true;
  window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: t('wishes.support.guideTitle'),
    message: t('wishes.support.guideMessage', { recipient }),
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
    heading: t('wishes.guide.familyTitle'),
    message: t('wishes.guide.familyMessage'),
  } }));
});
onMounted(() => {
  if (store.viewerRole !== 'child') return;
  window.setTimeout(() => window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: t('guide.pages.wishes.heading'),
    message: t('guide.pages.wishes.message'),
    pageIntro: true,
  } })), 350);
});
onUnmounted(() => {
  if (saveTimer !== undefined) window.clearTimeout(saveTimer);
  if (supportTimer !== undefined) window.clearTimeout(supportTimer);
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.create-goal-card {
  width: 100%;
  min-height: 66px;
  padding: 9px 12px;
  @apply d-flex align-center text-left cursor-pointer;
  gap: 10px;
  color: var(--lad-text);
  border: 2px solid color-mix(in srgb, var(--lad-palette-blue) 25%, transparent);
  border-radius: 20px;
  background:
    radial-gradient(
      circle at 91% 10%,
      color-mix(in srgb, var(--lad-palette-amber-250) 25%, transparent),
      transparent 29%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-amber-100)
    );
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-palette-blue-strong) 15%, transparent),
    0 10px 19px color-mix(in srgb, var(--lad-palette-blue-600) 8%, transparent);
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.create-goal-card:hover {
  transform: translateY(-2px);
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-palette-blue-strong) 15%, transparent),
    0 13px 22px color-mix(in srgb, var(--lad-palette-blue-600) 8%, transparent);
}
.create-goal-card:active {
  transform: translateY(3px);
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-palette-blue-strong) 15%, transparent);
}
.create-goal-icon {
  width: 43px;
  height: 43px;
  @apply d-grid place-center flex-shrink-0;
  color: white;
  border: 3px solid var(--lad-palette-white);
  border-radius: 15px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-blue-350),
    var(--lad-palette-blue)
  );
  box-shadow: 0 4px 0 var(--lad-palette-blue-strong);
}
.create-goal-card > span:nth-child(2) {
  @apply flex-grow-1 min-w-0;
}
.create-goal-card strong,
.create-goal-card small {
  @apply d-block;
}
.create-goal-card strong {
  font-size: 0.9375rem;
}
.create-goal-card small {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.5625rem;
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
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-white)
  );
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
  border: 1px solid color-mix(in srgb, var(--lad-palette-blue) 15%, transparent);
  border-radius: 12px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-white)
  );
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-blue-550) 8%, transparent);
}
.goal-account-stats > span:nth-child(2) {
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  );
}
.goal-account-stats > span:nth-child(3),
.goal-account-stats > span:nth-child(4) {
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface),
    var(--lad-palette-amber-150)
  );
}
.goal-account-stats :deep(.v-icon) {
  width: 27px;
  height: 27px;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-palette-blue-strong);
  border: 2px solid var(--lad-palette-white);
  border-radius: 9px;
  background: var(--lad-palette-background);
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-palette-blue-strong) 12%, transparent);
  font-size: 1rem;
  animation: account-stat-float 3s ease-in-out infinite;
}
.goal-account-stats > span:nth-child(2) :deep(.v-icon) {
  color: var(--lad-palette-teal-700);
  background: var(--lad-palette-background);
  animation-delay: -0.7s;
}
.goal-account-stats > span:nth-child(3) :deep(.v-icon),
.goal-account-stats > span:nth-child(4) :deep(.v-icon) {
  color: var(--lad-palette-amber-600);
  background: var(--lad-palette-amber-150);
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
  font-size: 0.4375rem;
  font-weight: 800;
  line-height: 1.15;
}
.goal-account-stats strong {
  margin-top: 2px;
  color: var(--lad-palette-teal-700);
  font-size: 0.6875rem;
  line-height: 1.1;
}
.goal-account-stats > span:nth-child(3) strong,
.goal-account-stats > span:nth-child(4) strong {
  color: var(--lad-palette-amber-650);
}
.goal-total small,
.goal-total strong {
  @apply d-block;
}
.goal-total small {
  color: var(--lad-muted);
  font-size: 0.4375rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.goal-total strong {
  margin-top: 1px;
  font-size: 0.8125rem;
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
  border: 3px solid var(--lad-palette-white);
  border-radius: 15px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-blue) 15%, transparent);
  font-size: 1.5rem;
  transform: rotate(-4deg);
}
@include respond-up(studio) {
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
    font-size: 0.9375rem;
  }
}
.child-selector {
  margin-inline: -4px;
}
.cheer-button {
  width: 48px;
  height: 42px;
  @apply position-relative d-grid place-center flex-shrink-0;
  border: 2px solid color-mix(in srgb, var(--lad-palette-blue) 18%, transparent);
  border-radius: 15px;
  color: var(--lad-palette-blue-600);
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-background)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-blue-550) 18%, transparent);
  cursor: pointer;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease,
    background 0.18s ease;
}
.cheer-button:hover {
  transform: translateY(-2px) rotate(-2deg);
  box-shadow: 0 6px 0
    color-mix(in srgb, var(--lad-palette-blue-550) 18%, transparent);
}
.cheer-button:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-palette-blue-550) 18%, transparent);
}
.cheer-button:focus-visible {
  @include focus-ring(color-mix(in srgb, var(--lad-blue) 42%, transparent));
}
.cheer-button.is-cheered {
  border-color: color-mix(
    in srgb,
    var(--lad-palette-amber-500) 35%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-palette-amber-100),
    var(--lad-palette-amber-150)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-amber-550) 20%, transparent),
    0 0 18px color-mix(in srgb, var(--lad-palette-yellow) 30%, transparent);
}
.cheer-hands {
  @apply d-flex align-center justify-center;
  width: 34px;
  height: 27px;
}
.cheer-hands i {
  font-style: normal;
  font-size: 1.125rem;
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
  color: var(--lad-palette-amber-500);
  opacity: 0.45;
  font-size: 0.5625rem;
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
  background: var(--lad-palette-background);
  font-size: 1.9375rem;
  animation: goal-float 2.8s ease-in-out infinite;
}
.family-gift-button {
  @apply position-relative overflow-visible;
}
.support-dialog-card {
  @apply position-relative overflow-hidden;
  border: 2px solid color-mix(in srgb, var(--lad-palette-blue) 18%, transparent);
  background:
    radial-gradient(
      circle at 88% 5%,
      color-mix(in srgb, var(--lad-palette-yellow) 30%, transparent),
      transparent 27%
    ),
    linear-gradient(
      155deg,
      var(--lad-palette-surface),
      var(--lad-palette-background) 64%,
      var(--lad-palette-background)
    ) !important;
  box-shadow:
    0 9px 0 color-mix(in srgb, var(--lad-palette-teal-700) 12%, transparent),
    0 24px 54px color-mix(in srgb, var(--lad-palette-text) 25%, transparent) !important;
}
.support-dialog-heading {
  @apply d-flex align-center;
  gap: 13px;
}
.support-dialog-heading h2 {
  @apply ma-0;
  font-size: 1.25rem;
  line-height: 1.15;
}
.support-dialog-heading span {
  @apply d-block;
  margin-top: 4px;
  color: var(--lad-muted);
  font-size: 0.625rem;
}
.support-journey {
  height: 54px;
  padding-inline: 8px;
  @apply position-relative d-flex align-center justify-space-between;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 12%, transparent);
  border-radius: 18px;
  background: color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
}
.support-journey > i {
  width: 10px;
  height: 5px;
  border-radius: var(--lad-radius-pill);
  background: var(--lad-palette-muted-250);
}
.support-coin,
.support-present {
  width: 36px;
  height: 36px;
  @apply d-grid place-center;
  z-index: 2;
  border: 3px solid var(--lad-palette-white);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-orange-750) 12%, transparent);
}
.support-coin {
  color: var(--lad-palette-amber-700);
  border-radius: 50%;
  background: var(--lad-palette-yellow);
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-black);
}
.support-present {
  border-radius: 12px;
  background: var(--lad-palette-background);
  font-size: 1.25rem;
}
.support-high-five {
  font-size: 1.5625rem;
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
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-mint-strong)
  ) !important;
  box-shadow: 0 4px 0 var(--lad-palette-teal-700) !important;
  font-size: 0.625rem;
  font-weight: var(--lad-font-weight-heavy);
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
@include reduced-motion {
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
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent) !important;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-blue-150)
  ) !important;
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-blue-strong) 20%, transparent),
    0 8px 14px
      color-mix(in srgb, var(--lad-palette-blue-strong) 10%, transparent) !important;
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-heavy);
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
  color: var(--lad-palette-amber-450);
  font-size: 0.625rem;
  font-style: normal;
  animation: assign-spark 2.3s ease-in-out infinite;
}
.save-dialog-card {
  @apply overflow-hidden;
  border: 2px solid color-mix(in srgb, var(--lad-palette-blue) 20%, transparent);
  background: linear-gradient(
    180deg,
    var(--lad-palette-surface),
    var(--lad-palette-white)
  ) !important;
  box-shadow:
    0 9px 0 color-mix(in srgb, var(--lad-palette-blue-600) 15%, transparent),
    0 25px 55px
      color-mix(in srgb, var(--lad-palette-indigo-750) 25%, transparent) !important;
}
.save-dialog-header {
  padding: 16px 14px;
  @apply d-flex align-center;
  gap: 11px;
  border-bottom: 2px solid
    color-mix(in srgb, var(--lad-palette-blue) 12%, transparent);
  background:
    radial-gradient(
      circle at 88% 4%,
      color-mix(in srgb, var(--lad-palette-yellow) 30%, transparent),
      transparent 28%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-amber-100)
    );
}
.save-dialog-header h2 {
  @apply ma-0;
  font-size: 1.25rem;
  letter-spacing: -0.035em;
}
.save-dialog-header div > span {
  @apply d-block;
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.save-goal-icon {
  width: 54px;
  height: 54px;
  @apply d-grid place-center flex-shrink-0;
  border: 3px solid var(--lad-palette-white);
  border-radius: 18px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-150)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-blue-strong) 15%, transparent);
  font-size: 1.8125rem;
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
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 15%, transparent);
  border-radius: 18px;
  background: color-mix(in srgb, var(--lad-palette-white) 75%, transparent);
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
}
.save-balance-coin,
.save-goal-star {
  width: 38px;
  height: 38px;
  @apply d-grid place-center flex-shrink-0;
  border: 3px solid var(--lad-palette-white);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-amber-700) 12%, transparent);
  font-weight: var(--lad-font-weight-black);
}
.save-balance-coin {
  color: var(--lad-palette-amber-700);
  border-radius: 50%;
  background: radial-gradient(
    circle at 35% 28%,
    var(--lad-palette-amber-150),
    var(--lad-palette-yellow) 48%,
    var(--lad-palette-amber-500)
  );
}
.save-goal-star {
  color: var(--lad-palette-amber-150);
  border-radius: 13px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-mint-strong)
  );
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
  font-size: 0.5rem;
}
.save-balance strong {
  font-size: 0.9375rem;
}
.save-journey {
  height: 8px;
  @apply position-relative flex-grow-1;
  border-radius: var(--lad-radius-pill);
  background: repeating-linear-gradient(
    90deg,
    var(--lad-palette-blue-150) 0 7px,
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
  background: var(--lad-palette-yellow);
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
  border: 3px solid var(--lad-palette-white);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-blue-strong) 18%, transparent);
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
  color: var(--lad-palette-amber-700);
  border: 2px solid var(--lad-palette-amber-150);
  border-radius: 50%;
  background: var(--lad-palette-yellow);
  box-shadow: 0 3px 0 var(--lad-palette-amber-550);
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-black);
}
.save-value strong {
  font-size: 2.125rem;
  line-height: 1;
}
.save-value small {
  color: var(--lad-muted);
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-strong);
}
.save-empty-note {
  margin: -3px 8px 14px;
  color: var(--lad-muted);
  @apply text-center;
  font-size: 0.5625rem;
  line-height: 1.4;
}
.save-submit {
  width: 100%;
  min-height: 49px;
  padding: 8px 13px;
  @apply position-relative d-flex align-center justify-center overflow-hidden cursor-pointer;
  gap: 8px;
  color: var(--lad-palette-white);
  border: 3px solid
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  border-radius: 17px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-blue-350),
    var(--lad-palette-blue-strong)
  );
  box-shadow:
    0 5px 0 var(--lad-palette-blue-strong),
    0 10px 17px
      color-mix(in srgb, var(--lad-palette-blue-strong) 18%, transparent);
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.save-submit:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow:
    0 7px 0 var(--lad-palette-blue-strong),
    0 13px 20px
      color-mix(in srgb, var(--lad-palette-blue-strong) 20%, transparent);
}
.save-submit:active:not(:disabled) {
  transform: translateY(3px);
  box-shadow: 0 2px 0 var(--lad-palette-blue-strong);
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
  color: var(--lad-palette-amber-700);
  border: 2px solid var(--lad-palette-amber-150);
  border-radius: 50%;
  background: var(--lad-palette-yellow);
  font-size: 0.625rem;
  font-weight: var(--lad-font-weight-black);
  animation: save-button-coin 2.2s ease-in-out infinite;
}
.save-submit > span:nth-of-type(2) {
  font-size: 1.125rem;
}
.save-submit > i {
  @apply position-absolute;
  top: 4px;
  right: 9px;
  color: var(--lad-palette-amber-150);
  font-size: 0.6875rem;
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
@include reduced-motion {
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
