<template><span hidden aria-hidden="true" /></template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { ladiGuideController } from '@/shared/services/ladi-guide-controller';

import { useLocalizedDomainContent } from '@/shared/composables/use-localized-domain-content';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const { locale, t } = useI18n();
const { goal: localizeGoal } = useLocalizedDomainContent();
const conversationStep = ref(-1);

const streakInterest = computed(() => store.currentDailyStreak * store.streakBonusRate);
const weeklyInterest = (saved: number, target = Number.POSITIVE_INFINITY) => saved <= 0 || saved >= target
  ? 0
  : Math.min(target - saved, Math.max(1, Math.round(saved * (store.savingsInterestRate / 100))));
const goalInterestRows = computed(() => {
  const rows = store.ownSavingGoals.map((goal) => ({
    ...localizeGoal(goal),
    weeklyInterest: weeklyInterest(goal.saved, goal.target),
  }));
  const highestInterest = Math.max(0, ...rows.map((goal) => goal.weeklyInterest));
  return rows.map((goal) => ({ ...goal, isTop: highestInterest > 0 && goal.weeklyInterest === highestInterest }));
});
const estimatedWeeklyInterest = computed(() =>
  goalInterestRows.value.reduce((sum, goal) => sum + goal.weeklyInterest, 0),
);
const answers = computed(() => [
  {
    title: t('savings.interestGuide.bonusTitle'),
    text: t('savings.interestGuide.bonusMessage', { saved: store.totalSaved, interest: estimatedWeeklyInterest.value }),
  },
  {
    title: t('savings.interestGuide.goalsTitle'),
    text: goalInterestRows.value.length
      ? t('savings.interestGuide.goalsMessage', { goals: goalInterestRows.value.map(goal => t('savings.interestGuide.goalItem', { title: goal.title, interest: goal.weeklyInterest })).join(' · ') })
      : t('savings.interestGuide.goalsEmpty'),
  },
  {
    title: t('savings.interestGuide.streakTitle'),
    text: t('savings.interestGuide.streakMessage', { days: store.currentDailyStreak, rate: formatRate(streakInterest.value) }),
  },
  {
    title: t('savings.interestGuide.careTitle'),
    text: t('savings.interestGuide.careMessage', { completion: store.dailyEnergy, rating: formatRate(store.averageTaskRating) }),
  },
  {
    title: t('savings.interestGuide.tipTitle'),
    text: t('savings.interestGuide.tipMessage'),
  },
]);

const formatRate = (value: number) => value.toLocaleString(locale.value, { minimumFractionDigits: 1, maximumFractionDigits: 2 });
const nextAnswer = () => {
  conversationStep.value = conversationStep.value >= answers.value.length - 1
    ? 0
    : conversationStep.value + 1;
  const answer = answers.value[conversationStep.value];
  if (!answer) return;
  ladiGuideController.say({
    heading: answer.title,
    message: answer.text,
    smart: true,
    progress: `${conversationStep.value + 2} / ${answers.value.length + 1}`,
    actionLabel: t(conversationStep.value === answers.value.length - 1 ? 'savings.piggy.guide.again' : 'common.next'),
    actionId: 'savings-interest:next',
  });
};

let unregisterStartAction: (() => void) | undefined;
let unregisterNextAction: (() => void) | undefined;
onMounted(() => {
  unregisterStartAction = ladiGuideController.registerAction('savings-interest:start', nextAnswer);
  unregisterNextAction = ladiGuideController.registerAction('savings-interest:next', nextAnswer);
});
onUnmounted(() => {
  unregisterStartAction?.();
  unregisterNextAction?.();
});
</script>
