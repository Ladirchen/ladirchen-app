<template><span class="interest-guide-controller" aria-hidden="true" /></template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref } from 'vue';

import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const conversationStep = ref(-1);

const streakInterest = computed(() => store.currentDailyStreak * store.streakBonusRate);
const weeklyInterest = (saved: number, target = Number.POSITIVE_INFINITY) => saved <= 0 || saved >= target
  ? 0
  : Math.min(target - saved, Math.max(1, Math.round(saved * (store.savingsInterestRate / 100))));
const goalInterestRows = computed(() => {
  const rows = store.ownSavingGoals.map((goal) => ({
    ...goal,
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
    title: 'Ladis Fleiß-Bonus',
    text: `Du hast ${store.totalSaved} Ladirchen in deinen Zielen. Mit deinem aktuellen Fleiß-Bonus kommen voraussichtlich etwa ${estimatedWeeklyInterest.value} Ladirchen pro Woche dazu – ganz automatisch.`,
  },
  {
    title: 'Jeder Wunsch bekommt Zinsen',
    text: goalInterestRows.value.length
      ? goalInterestRows.value.map((goal) => `${goal.title}: etwa ${goal.weeklyInterest} Ladirchen`).join(' · ') + '. Die Zinsen landen direkt auf dem jeweiligen Wunschkonto.'
      : 'Sobald Ladirchen auf einem Wunschkonto liegen, berechne ich die Zinsen für dieses Ziel und zahle sie direkt dort ein.',
  },
  {
    title: 'Dranbleiben lohnt sich',
    text: `Deine Tagesserie läuft seit ${store.currentDailyStreak} Tagen. Sie bringt dir gerade ${formatRate(streakInterest.value)} zusätzliche Prozentpunkte. Jeder erfolgreiche Tag kann deinen Bonus wachsen lassen.`,
  },
  {
    title: 'Sorgfältig helfen',
    text: `Du hast ${store.dailyEnergy} Prozent deiner Aufgaben geschafft und im Schnitt ${store.averageTaskRating.toFixed(1)} Sterne erhalten. Regelmäßiges und sorgfältiges Helfen stärkt deinen Fleiß-Bonus.`,
  },
  {
    title: 'Ladis Spartipp',
    text: 'Lege lieber öfter ein paar Ladirchen zurück, statt auf den einen großen Sprung zu warten. Viele kleine Schritte bringen dich sicher zu deinem Wunsch.',
  },
]);

const formatRate = (value: number) => value.toLocaleString('de-DE', { minimumFractionDigits: 1, maximumFractionDigits: 2 });
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
    actionLabel: conversationStep.value === answers.value.length - 1 ? 'Noch einmal' : 'Weiter',
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

<style scoped>
.interest-guide-controller {
  display: none;
}
.interest-guide-card {
  padding: 19px;
  @apply position-relative overflow-hidden;
  border: 2px solid rgba(226, 164, 49, 0.3);
  background:
    radial-gradient(
      circle at 92% 5%,
      rgba(255, 220, 95, 0.34),
      transparent 28%
    ),
    linear-gradient(145deg, #fffdf3, #edf9f3);
  box-shadow:
    0 7px 0 rgba(201, 137, 38, 0.14),
    0 15px 28px rgba(83, 105, 84, 0.09) !important;
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
    rgba(255, 255, 255, 0.74),
    transparent
  );
  animation: interest-shine 4.6s ease-in-out infinite;
}
.interest-guide-card::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 10px;
  right: 13px;
  color: #e7a52c;
  font-size: 15px;
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
  color: #fff4a9;
  border: 3px solid #fff;
  border-radius: 19px;
  background: linear-gradient(145deg, #66c29c, #318c6c);
  box-shadow:
    0 5px 0 #247257,
    0 9px 16px rgba(36, 114, 87, 0.14);
  font-size: 29px;
  transform: rotate(-5deg);
  animation: interest-icon-float 3s ease-in-out infinite;
}
.interest-icon i {
  min-width: 23px;
  height: 23px;
  @apply position-absolute d-grid place-center;
  right: -9px;
  bottom: -7px;
  color: #8c5b0f;
  border: 2px solid #fff;
  border-radius: 50%;
  background: #ffd45b;
  box-shadow: 0 2px 0 #c5861e;
  font-size: 15px;
  font-style: normal;
  font-weight: 950;
  animation: interest-plus-pop 2.2s ease-in-out infinite;
}
.interest-heading h2 {
  @apply ma-0;
  font-size: 18px;
  line-height: 1.2;
  letter-spacing: -0.025em;
}
.interest-heading p:last-child {
  max-width: 290px;
  margin-top: 5px;
  color: var(--lad-muted);
  font-size: 11px;
  line-height: 1.45;
}
.interest-rate {
  min-width: 92px;
  min-height: 92px;
  padding: 11px 8px 8px;
  @apply position-relative d-flex flex-column align-center justify-center flex-shrink-0 text-center;
  z-index: 1;
  color: #247b5d;
  border: 3px solid rgba(255, 255, 255, 0.9);
  border-radius: 24px;
  background: linear-gradient(145deg, #fffdf2, #e4f8ed);
  box-shadow:
    0 4px 0 rgba(44, 135, 99, 0.13),
    0 8px 15px rgba(44, 135, 99, 0.08);
}
.interest-rate > * {
  @apply position-relative;
  z-index: 1;
}
.interest-rate i {
  @apply position-absolute;
  top: 7px;
  right: 9px;
  color: #e4a52d;
  font-size: 9px;
  font-style: normal;
  opacity: 0.72;
}
.interest-rate small {
  color: #6a837a;
  font-size: 8px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.interest-rate strong {
  @apply d-block;
  margin-top: 1px;
  font-size: 23px;
  line-height: 1.05;
}
.interest-rate span {
  @apply d-block;
  margin-top: 3px;
  font-size: 9px;
  font-weight: 900;
}
.interest-progress {
  padding: 14px;
  @apply position-relative;
  z-index: 1;
  border: 2px solid rgba(255, 255, 255, 0.84);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.72);
  box-shadow: 0 3px 0 rgba(65, 126, 96, 0.07);
}
.bonus-preview {
  @apply d-flex align-center justify-space-between;
  gap: 8px;
}
.bonus-preview span {
  color: var(--lad-muted);
  font-size: 11px;
}
.bonus-preview strong {
  color: #277c5f;
  font-size: 16px;
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
  color: #596b65;
  border: 1px solid rgba(70, 138, 111, 0.08);
  border-radius: 12px;
  background: linear-gradient(145deg, #f5faf7, #fff8df);
  box-shadow: 0 2px 0 rgba(65, 122, 96, 0.06);
  font-size: 9px;
  font-weight: 850;
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
  color: #26775c;
  font-size: 10px;
}
.goal-interest-breakdown {
  padding: 13px;
  @apply position-relative;
  z-index: 1;
  border: 2px solid rgba(73, 151, 198, 0.17);
  border-radius: 19px;
  background:
    radial-gradient(circle at 92% 4%, rgba(255, 219, 91, 0.2), transparent 28%),
    linear-gradient(145deg, #eef8ff, #f1faf5);
  box-shadow: 0 5px 0 rgba(58, 127, 174, 0.1);
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
  color: #4e8fdd;
  font-size: 8px;
  font-weight: 950;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}
.goal-interest-heading strong {
  margin-top: 2px;
  font-size: 13px;
}
.goal-interest-heading > b {
  padding: 5px 8px;
  color: #28785c;
  border-radius: 999px;
  background: #ddf4e8;
  font-size: 8px;
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
  border: 2px solid rgba(77, 151, 122, 0.13);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.78);
  box-shadow: 0 3px 0 rgba(57, 126, 99, 0.08);
}
.goal-interest-row.is-top-interest {
  border-color: rgba(228, 168, 47, 0.24);
  background: linear-gradient(145deg, #fff, #fff6cf);
}
.goal-interest-icon {
  width: 40px;
  height: 40px;
  @apply d-grid place-center;
  border: 2px solid #fff;
  border-radius: 13px;
  background: linear-gradient(145deg, #e9f7ff, #fff0b9);
  box-shadow: 0 3px 0 rgba(57, 127, 166, 0.1);
  font-size: 22px;
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
  font-size: 11px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.goal-interest-copy small {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 8px;
}
.goal-interest-value {
  @apply text-right;
}
.goal-interest-value b {
  color: #277c5f;
  font-size: 14px;
}
.goal-interest-value small {
  margin-top: 2px;
  color: #658078;
  font-size: 7px;
  font-weight: 800;
}
.goal-interest-row > i {
  @apply position-absolute;
  top: -7px;
  right: 8px;
  padding: 3px 6px;
  color: #85570f;
  border: 1px solid rgba(222, 159, 42, 0.24);
  border-radius: 999px;
  background: #ffe9a8;
  font-size: 6px;
  font-style: normal;
  font-weight: 950;
  text-transform: uppercase;
}
.goal-interest-breakdown > p {
  margin: 10px 2px 0;
  color: #597069;
  font-size: 9px;
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
@media (max-width: 390px) {
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
@media (prefers-reduced-motion: reduce) {
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
