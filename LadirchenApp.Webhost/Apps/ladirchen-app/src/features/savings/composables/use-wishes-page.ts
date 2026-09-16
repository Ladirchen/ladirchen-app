import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';

import type { PageViewOption } from '@/shared/components/ui/PageViewSwitch.vue';
import type { GoalVisibility, NewGoal, SavingGoal, SavingGoalOwnerId } from '@/domain/savings/types';
import { calculateSavingsCredit } from '@/domain/savings/interest';
import { SAVINGS_RULES } from '@/domain/savings/rules';
import type { FamilyMemberId, SavingGoalId } from '@/domain/shared/identifiers';
import { percentageOfTotal } from '@/domain/shared/numbers';
import { useLocalizedDomainContent } from '@/shared/composables/use-localized-domain-content';
import { useFamilyWorldStore } from '@/stores/family-world';
import { ladiGuideController } from '@/shared/services/ladi-guide-controller';

const SAVE_ANIMATION_DURATION_MS = 620;
const SUPPORT_ANIMATION_DURATION_MS = 950;

export const useWishesPage = () => {
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
const supportAmount = ref<number>(SAVINGS_RULES.defaultTransferAmount);
const supportSending = ref(false);
const saveAmount = ref<number>(SAVINGS_RULES.defaultTransferAmount);
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
    if (goal.ownerId === 'family') {return true;}
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
    icon: 'i-mdi:account-star-outline',
    subtitle: t('wishes.views.ownCount', { count: ownGoals.length }),
    title: t('wishes.views.own'),
  }];
  if (store.viewerRole === 'guardian') {
    options.push({ id: 'children', icon: 'i-mdi:account-child-outline', subtitle: t('wishes.views.visibleCount', { count: childrenGoals.value.length }), title: t('wishes.views.children') });
  }
  if (store.permissions.canViewFamilyGoals) {options.push({ id: 'family', icon: 'i-mdi:account-group-outline', subtitle: t('wishes.views.visibleCount', { count: visibleFamilyGoals.value.length }), title: t('wishes.views.family') });}
  return options;
});
const supportGoal = computed(() => localizedGoals.value.find((goal) => goal.id === supportGoalId.value));
const supportMaximum = computed(() => {
  if (!supportGoal.value) {return 0;}
  const remaining = Math.max(0, supportGoal.value.target - supportGoal.value.saved);
  return store.viewerRole === 'child' ? Math.min(store.availableBalance, remaining) : remaining;
});
const supportExplanation = computed(() => store.viewerRole === 'child'
  ? t('wishes.support.childExplanation')
  : t('wishes.support.guardianExplanation'));

const progress = (saved: number, target: number) => percentageOfTotal(saved, target);
const depositedAmount = (goal: SavingGoal) => Math.max(0, goal.saved - (goal.interestEarned ?? 0));
const weeklyInterestForGoal = (goal: SavingGoal) =>
  calculateSavingsCredit(goal.saved, goal.target, store.savingsInterestRate);
const formatInterestRate = (value: number) => value.toLocaleString(locale.value, { minimumFractionDigits: 1, maximumFractionDigits: 2 });
const ownerName = (ownerId: SavingGoalOwnerId) => ownerId === 'family' ? t('wishes.owner.myFamily') : store.members.find((member) => member.id === ownerId)?.name ?? t('wishes.owner.family');
const visibilityLabel = (visibility: GoalVisibility) => {
  return t(`wishes.visibility.${visibility}`);
};
const saveToGoal = () => {
  if (saveAmount.value <= 0 || saveMotion.value) {return;}
  saveMotion.value = true;
  saveTimer = window.setTimeout(() => {
    store.saveToGoal(store.activeGoal.id, saveAmount.value);
    saveDialog.value = false;
    saveAmount.value = SAVINGS_RULES.defaultTransferAmount;
    saveMotion.value = false;
    saveTimer = undefined;
  }, SAVE_ANIMATION_DURATION_MS);
};
const openSave = (goalId: SavingGoalId) => {
  store.activeGoalId = goalId;
  saveAmount.value = Math.min(
    SAVINGS_RULES.defaultTransferAmount,
    store.availableBalance,
    Math.max(0, store.activeGoal.target - store.activeGoal.saved),
  );
  saveDialog.value = true;
};
const openSupport = (goalId: SavingGoalId) => {
  supportGoalId.value = goalId;
  const goal = store.goals.find((item) => item.id === goalId);
  const remaining = goal ? Math.max(0, goal.target - goal.saved) : 0;
  supportAmount.value = Math.max(0, Math.min(
    SAVINGS_RULES.defaultTransferAmount,
    store.viewerRole === 'child' ? store.availableBalance : remaining,
    remaining,
  ));
  supportDialog.value = true;
};
const giveSupport = () => {
  if (!supportGoalId.value || supportSending.value) {return;}
  const goalId = supportGoalId.value;
  const amount = supportAmount.value;
  const recipient = supportGoal.value ? ownerName(supportGoal.value.ownerId) : t('wishes.owner.yourFamily');
  supportSending.value = true;
  ladiGuideController.say({
    heading: t('wishes.support.guideTitle'),
    message: t('wishes.support.guideMessage', { recipient }),
    celebration: 'gift',
  });
  supportTimer = window.setTimeout(() => {
    if (store.viewerRole === 'child') {store.giftLadirchenToGoal(goalId, amount);}
    else {store.supportGoal(goalId, amount);}
    supportDialog.value = false;
    supportSending.value = false;
    supportTimer = undefined;
  }, SUPPORT_ANIMATION_DURATION_MS);
};
const openGoalDialog = (ownerId: SavingGoalOwnerId) => {
  newGoalOwnerId.value = ownerId;
  goalDialog.value = true;
};
const addGoal = (goal: NewGoal) => store.addGoal(goal, newGoalOwnerId.value);
watch(
  () => route.query.new,
  (value) => { if (value === '1' && store.permissions.canManageGoals) {goalDialog.value = true;} },
  { immediate: true },
);
watch(
  () => route.query.family,
  (value) => {
    if (value !== '1' || !store.permissions.canManageGoals) {return;}
    activeTab.value = 'family';
    openGoalDialog('family');
  },
  { immediate: true },
);
watch(() => store.permissions.canViewFamilyGoals, canViewFamilyGoals => {
  if (!canViewFamilyGoals) {activeTab.value = 'own';}
});
watch(activeTab, (tab) => {
  if (tab !== 'family' || store.viewerRole !== 'child') {return;}
  ladiGuideController.say({
    heading: t('wishes.guide.familyTitle'),
    message: t('wishes.guide.familyMessage'),
  });
});
onMounted(() => {
  if (store.viewerRole !== 'child') {return;}
  window.setTimeout(() => ladiGuideController.say({
    heading: t('guide.pages.wishes.heading'),
    message: t('guide.pages.wishes.message'),
    pageIntro: true,
  }), 350);
});
onUnmounted(() => {
  if (saveTimer !== undefined) {window.clearTimeout(saveTimer);}
  if (supportTimer !== undefined) {window.clearTimeout(supportTimer);}
});
  return {
    activeGoal, activeTab, addGoal, canCreatePersonalGoal, depositedAmount, formatInterestRate, giveSupport,
    goalDialog, maxAssignable, openGoalDialog, openSave, openSupport, ownerName, personalGoalOwnerId,
    personalGoals, progress, saveAmount, saveDialog, saveMotion, saveToGoal, store, supportAmount,
    supportDialog, supportExplanation, supportGoal, supportMaximum, supportSending, visibilityLabel,
    visibleFamilyGoals, weeklyInterestForGoal, wishViewOptions,
  };
};
