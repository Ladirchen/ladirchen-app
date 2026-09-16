<template><span class="interest-guide-controller" aria-hidden="true" /></template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';

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
  window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: answer.title,
    message: answer.text,
    smart: true,
    progress: `${conversationStep.value + 2} / ${answers.value.length + 1}`,
    actionLabel: t(conversationStep.value === answers.value.length - 1 ? 'savings.piggy.guide.again' : 'common.next'),
    actionEvent: 'savings-interest:next',
  } }));
};

onMounted(() => {
  window.addEventListener('savings-interest:start', nextAnswer);
  window.addEventListener('savings-interest:next', nextAnswer);
});
onUnmounted(() => {
  window.removeEventListener('savings-interest:start', nextAnswer);
  window.removeEventListener('savings-interest:next', nextAnswer);
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.interest-guide-controller {
  display: none;
}
.interest-guide-card {
  padding: 19px;
  @apply position-relative overflow-hidden;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-amber-450) 30%, transparent);
  background:
    radial-gradient(
      circle at 92% 5%,
      color-mix(in srgb, var(--lad-palette-yellow) 35%, transparent),
      transparent 28%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-surface),
      var(--lad-palette-background)
    );
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-palette-amber-550) 15%, transparent),
    0 15px 28px
      color-mix(in srgb, var(--lad-palette-muted-600-2) 8%, transparent) !important;
}
.interest-guide-card::before {
  content: "";
  width: 58px;
  height: 150%;
  @apply position-absolute pointer-events-none;
  top: -25%;
  left: -80px;
  transform: rotate(16deg);
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--lad-palette-white) 75%, transparent),
    transparent
  );
  animation: interest-shine 4.6s ease-in-out infinite;
}
.interest-guide-card::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 10px;
  right: 13px;
  color: var(--lad-palette-amber-450);
  font-size: 0.9375rem;
  animation: interest-twinkle 1.8s ease-in-out infinite;
}
.interest-heading {
  @apply position-relative d-flex align-start;
  z-index: 1;
  gap: 13px;
}
.interest-icon {
  width: 56px;
  height: 56px;
  @apply position-relative d-grid place-center flex-shrink-0;
  color: var(--lad-palette-amber-150);
  border: 3px solid var(--lad-palette-white);
  border-radius: 19px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-mint-strong)
  );
  box-shadow:
    0 5px 0 var(--lad-palette-teal-700),
    0 9px 16px color-mix(in srgb, var(--lad-palette-teal-700) 15%, transparent);
  font-size: 1.8125rem;
  transform: rotate(-5deg);
  animation: interest-icon-float 3s ease-in-out infinite;
}
.interest-icon i {
  min-width: 23px;
  height: 23px;
  @apply position-absolute d-grid place-center;
  right: -9px;
  bottom: -7px;
  color: var(--lad-palette-amber-700);
  border: 2px solid var(--lad-palette-white);
  border-radius: 50%;
  background: var(--lad-palette-yellow);
  box-shadow: 0 2px 0 var(--lad-palette-amber-550);
  font-size: 0.9375rem;
  font-style: normal;
  font-weight: var(--lad-font-weight-black);
  animation: interest-plus-pop 2.2s ease-in-out infinite;
}
.interest-heading h2 {
  @apply ma-0;
  font-size: 1.125rem;
  line-height: 1.2;
  letter-spacing: -0.025em;
}
.interest-heading p:last-child {
  max-width: 290px;
  margin-top: 5px;
  color: var(--lad-muted);
  font-size: 0.6875rem;
  line-height: 1.45;
}
.interest-rate {
  min-width: 92px;
  min-height: 92px;
  padding: 11px 8px 8px;
  @apply position-relative d-flex flex-column align-center justify-center flex-shrink-0 text-center;
  z-index: 1;
  color: var(--lad-palette-teal-700);
  border: 3px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  border-radius: 24px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface),
    var(--lad-palette-background)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-mint-strong) 12%, transparent),
    0 8px 15px
      color-mix(in srgb, var(--lad-palette-mint-strong) 8%, transparent);
}
.interest-rate > * {
  @apply position-relative;
  z-index: 1;
}
.interest-rate i {
  @apply position-absolute;
  top: 7px;
  right: 9px;
  color: var(--lad-palette-amber-450);
  font-size: 0.5625rem;
  font-style: normal;
  opacity: 0.72;
}
.interest-rate small {
  color: var(--lad-palette-muted);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.interest-rate strong {
  @apply d-block;
  margin-top: 1px;
  font-size: 1.4375rem;
  line-height: 1.05;
}
.interest-rate span {
  @apply d-block;
  margin-top: 3px;
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-heavy);
}
.interest-progress {
  padding: 14px;
  @apply position-relative;
  z-index: 1;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 85%, transparent);
  border-radius: 18px;
  background: color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
}
.bonus-preview {
  @apply d-flex align-center justify-space-between;
  gap: 8px;
}
.bonus-preview span {
  color: var(--lad-muted);
  font-size: 0.6875rem;
}
.bonus-preview strong {
  color: var(--lad-palette-teal-700);
  font-size: 1rem;
  animation: bonus-number-pulse 2.5s ease-in-out infinite;
}
.interest-factors {
  @apply d-grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 7px;
}
.interest-factors span {
  padding: 8px 5px;
  @apply text-center;
  color: var(--lad-palette-teal-600);
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
  border-radius: 12px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface),
    var(--lad-palette-amber-100)
  );
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-palette-teal-600) 5%, transparent);
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-strong);
  animation: factor-float 3.4s ease-in-out infinite;
}
.interest-factors span:nth-child(2) {
  animation-delay: -0.8s;
}
.interest-factors span:nth-child(3) {
  animation-delay: -1.6s;
}
.interest-factors span:nth-child(4) {
  animation-delay: -2.4s;
}
.interest-factors b {
  @apply d-block;
  margin-bottom: 2px;
  color: var(--lad-palette-teal-700);
  font-size: 0.625rem;
}
.goal-interest-breakdown {
  padding: 13px;
  @apply position-relative;
  z-index: 1;
  border: 2px solid color-mix(in srgb, var(--lad-palette-blue) 18%, transparent);
  border-radius: 19px;
  background:
    radial-gradient(
      circle at 92% 4%,
      color-mix(in srgb, var(--lad-palette-yellow) 20%, transparent),
      transparent 28%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-background)
    );
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-palette-blue-strong) 10%, transparent);
}
.goal-interest-heading {
  @apply d-flex align-end justify-space-between;
  gap: 8px;
}
.goal-interest-heading span,
.goal-interest-heading small,
.goal-interest-heading strong {
  @apply d-block;
}
.goal-interest-heading small {
  color: var(--lad-palette-blue);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.08em;
  text-transform: uppercase;
}
.goal-interest-heading strong {
  margin-top: 2px;
  font-size: 0.8125rem;
}
.goal-interest-heading > b {
  padding: 5px 8px;
  color: var(--lad-palette-teal-700);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-palette-background);
  font-size: 0.5rem;
  white-space: nowrap;
}
.goal-interest-list {
  @apply d-flex flex-column;
  gap: 8px;
}
.goal-interest-row {
  min-height: 58px;
  padding: 7px 8px;
  @apply position-relative d-grid align-center;
  grid-template-columns: 42px minmax(0, 1fr) auto;
  gap: 8px;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 12%, transparent);
  border-radius: 16px;
  background: color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
}
.goal-interest-row.is-top-interest {
  border-color: color-mix(
    in srgb,
    var(--lad-palette-amber-450) 25%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-amber-100)
  );
}
.goal-interest-icon {
  width: 40px;
  height: 40px;
  @apply d-grid place-center;
  border: 2px solid var(--lad-palette-white);
  border-radius: 13px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-150)
  );
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-blue-550) 10%, transparent);
  font-size: 1.375rem;
  transform: rotate(-4deg);
  animation: goal-interest-float 3s ease-in-out infinite;
}
.goal-interest-copy,
.goal-interest-copy strong,
.goal-interest-copy small,
.goal-interest-value,
.goal-interest-value b,
.goal-interest-value small {
  @apply d-block min-w-0;
}
.goal-interest-copy strong {
  overflow: hidden;
  font-size: 0.6875rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.goal-interest-copy small {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.goal-interest-value {
  @apply text-right;
}
.goal-interest-value b {
  color: var(--lad-palette-teal-700);
  font-size: 0.875rem;
}
.goal-interest-value small {
  margin-top: 2px;
  color: var(--lad-palette-muted);
  font-size: 0.4375rem;
  font-weight: 800;
}
.goal-interest-row > i {
  @apply position-absolute;
  top: -7px;
  right: 8px;
  padding: 3px 6px;
  color: var(--lad-palette-amber-700);
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-amber-500) 25%, transparent);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-palette-amber-150);
  font-size: 0.375rem;
  font-style: normal;
  font-weight: var(--lad-font-weight-black);
  text-transform: uppercase;
}
.goal-interest-breakdown > p {
  margin: 10px 2px 0;
  color: var(--lad-palette-teal-600);
  font-size: 0.5625rem;
  font-weight: 750;
  line-height: 1.4;
}
@keyframes interest-shine {
  0%,
  48% {
    left: -80px;
    opacity: 0;
  }
  61% {
    opacity: 0.9;
  }
  78%,
  100% {
    left: 115%;
    opacity: 0;
  }
}
@keyframes interest-twinkle {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.7) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(20deg);
  }
}
@keyframes interest-icon-float {
  0%,
  100% {
    transform: translateY(2px) rotate(-5deg);
  }
  50% {
    transform: translateY(-4px) rotate(3deg);
  }
}
@keyframes interest-plus-pop {
  0%,
  65%,
  100% {
    transform: scale(1) rotate(0);
  }
  78% {
    transform: scale(1.2) rotate(12deg);
  }
}
@keyframes bonus-number-pulse {
  0%,
  100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.06);
  }
}
@keyframes factor-float {
  0%,
  100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-2px);
  }
}
@keyframes goal-interest-float {
  0%,
  100% {
    transform: translateY(1px) rotate(-4deg);
  }
  50% {
    transform: translateY(-2px) rotate(3deg);
  }
}
@include respond-down(narrow) {
  .interest-heading {
    flex-wrap: wrap;
  }
  .interest-rate {
    margin-left: 60px;
  }
  .interest-factors {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
@include reduced-motion {
  .interest-guide-card::before,
  .interest-guide-card::after,
  .interest-icon,
  .interest-icon i,
  .bonus-preview strong,
  .interest-factors span,
  .goal-interest-icon {
    animation: none;
  }
}
</style>
