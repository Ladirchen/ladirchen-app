<template>
  <v-dialog content-class="series-summary-dialog-frame" :model-value="modelValue" max-width="480" @update:model-value="emit('update:modelValue', $event)">
    <v-card class="streak-dialog" rounded="xl">
      <div class="streak-header pa-4">
        <div class="streak-title-row">
          <div>
            <p class="dialog-kicker">Gemeinsam dranbleiben</p>
            <h2>Tagesserie</h2>
            <p class="streak-subtitle mt-1">Sammelt gemeinsam Feuer für euer Zuhause.</p>
          </div>
          <div class="streak-hero" aria-hidden="true">
            <span class="hero-spark hero-spark--one">✦</span>
            <span class="hero-spark hero-spark--two">✦</span>
            <AnimatedStreakFlame :size="80" />
          </div>
          <button class="close-button" aria-label="Wochenübersicht schließen" type="button" @click="close"><v-icon icon="mdi-close" /></button>
        </div>

        <div class="summary-grid mt-4">
          <div class="summary-tile summary-tile--streak">
            <div class="summary-icon" aria-hidden="true"><v-icon size="25">mdi-fire</v-icon></div>
            <div><span>Eure Serie</span><strong>{{ store.currentDailyStreak }} {{ store.currentDailyStreak === 1 ? 'Tag' : 'Tage' }}</strong></div>
          </div>
          <div class="summary-tile summary-tile--week">
            <div class="summary-icon" aria-hidden="true"><v-icon size="23">mdi-calendar-heart</v-icon></div>
            <div><span>Diese Woche</span><strong>{{ store.currentWeekDays }} von {{ store.currentWeekTarget }}</strong></div>
          </div>
        </div>
      </div>

      <div class="streak-content pa-4">
        <div v-if="store.viewerRole === 'child'" class="ladi-level">
          <LadiMascot :score="store.averageTaskRating" :show-score="false" :size="88" />
          <div>
            <p class="section-kicker">MEIN LADI-LEVEL</p>
            <strong>{{ ladiStage.name }}</strong>
            <span>{{ ladiStage.description }}</span>
          </div>
        </div>

        <div class="week-heading" :class="{ 'mt-4': store.viewerRole === 'child' }">
          <div><span class="section-kicker">EUER WOCHENWEG</span><strong>Ein Tag nach dem anderen</strong></div>
        </div>

        <div aria-label="Fortschritt dieser Woche" class="week-days mt-3">
          <div v-for="(day, index) in weekDays" :key="day.label" :class="['week-day', `week-day--${day.status}`]" :style="{ '--day-index': index }" :aria-label="`${day.fullLabel}: ${statusLabel(day.status)}`">
            <span>{{ day.label }}</span>
            <div class="day-symbol">
              <v-icon v-if="day.status === 'done'" size="19">mdi-check-bold</v-icon>
              <v-icon v-else-if="day.status === 'today'" size="18">mdi-star-four-points</v-icon>
              <v-icon v-else size="14">mdi-circle-small</v-icon>
            </div>
            <small>{{ statusLabel(day.status) }}</small>
          </div>
          <div class="week-motivation">
            <AnimatedStreakFlame :size="27" />
            <span><strong>{{ weekMessage }}</strong><small>Jeder geschaffte Tag bringt neue Energie in euer Zuhause.</small></span>
          </div>
        </div>

      </div>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

import AnimatedStreakFlame from './AnimatedStreakFlame.vue';
import LadiMascot from '@/shared/components/LadiMascot.vue';
import { getLadiStage } from '@/domain/ladi';
import { approvedContributionDatesInCurrentWeek } from '@/domain/weekly-progress';
import { addCalendarDays, calendarDateInTimeZone, startOfIsoWeek } from '@/domain/zoned-calendar';
import { useFamilyWorldStore } from '@/stores/family-world';

type DayStatus = 'done' | 'today' | 'upcoming';

defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>();
const store = useFamilyWorldStore();
const labels = computed(() => {
  const monday = startOfIsoWeek(new Date(store.currentTimeMilliseconds), store.familyTimeZone);
  const shortFormatter = new Intl.DateTimeFormat('de-CH', { timeZone: 'UTC', weekday: 'short' });
  const longFormatter = new Intl.DateTimeFormat('de-CH', { timeZone: 'UTC', weekday: 'long' });
  return Array.from({ length: 7 }, (_, index) => {
    const calendarDate = addCalendarDays(monday, index);
    const date = new Date(`${calendarDate}T12:00:00.000Z`);
    return { calendarDate, label: shortFormatter.format(date), fullLabel: longFormatter.format(date) };
  });
});
const completedDates = computed(() => approvedContributionDatesInCurrentWeek(
  store.contributions,
  store.familyTimeZone,
  new Date(store.currentTimeMilliseconds),
));
const today = computed(() => calendarDateInTimeZone(new Date(store.currentTimeMilliseconds), store.familyTimeZone));
const weekDays = computed(() => labels.value.map(day => ({
  ...day,
  status: (completedDates.value.has(day.calendarDate)
    ? 'done'
    : day.calendarDate === today.value
      ? 'today'
      : 'upcoming') as DayStatus,
})));
const ladiStage = computed(() => getLadiStage(store.averageTaskRating));
const weekMessage = computed(() => store.currentWeekDays >= store.currentWeekTarget
  ? 'Starke Woche – ihr habt gemeinsam durchgehalten!'
  : 'Heute zählt für eure gemeinsame Serie!');

const statusLabel = (status: DayStatus) => {
  if (status === 'done') return 'Geschafft';
  if (status === 'today') return 'Heute';
  return 'Kommt noch';
};
const close = () => emit('update:modelValue', false);
</script>

<style scoped>
.streak-dialog {
  max-height: calc(100dvh - 28px);
  @apply d-flex flex-column overflow-hidden;
  color: #253843;
  border: 2px solid rgba(78, 143, 221, 0.16);
  background: #fffdf8 !important;
  box-shadow:
    0 10px 0 rgba(58, 127, 174, 0.12),
    0 28px 70px rgba(62, 85, 75, 0.22) !important;
}
.streak-header {
  @apply position-relative;
  flex: 0 0 auto;
  background: #fffdf8;
}
.streak-title-row {
  min-height: 126px;
  padding: 18px;
  @apply position-relative d-flex align-center overflow-hidden;
  z-index: 1;
  border: 2px solid rgba(78, 143, 221, 0.15);
  border-radius: 23px;
  background: linear-gradient(145deg, #fffdf8, #e7f3ff);
  box-shadow: 0 5px 0 rgba(78, 143, 221, 0.12);
}
.streak-title-row::before,
.streak-title-row::after {
  content: "";
  @apply position-absolute pointer-events-none;
  border-radius: 50%;
}
.streak-title-row::before {
  width: 120px;
  height: 120px;
  top: -72px;
  right: -28px;
  background: rgba(214, 235, 255, 0.62);
  box-shadow: 0 0 0 17px rgba(226, 240, 255, 0.45);
}
.streak-title-row::after {
  width: 84px;
  height: 26px;
  right: 44px;
  bottom: -16px;
  background: rgba(213, 234, 255, 0.55);
}
.streak-title-row > div:first-child {
  max-width: 245px;
  @apply position-relative;
  z-index: 1;
}
.dialog-kicker {
  margin: 0 0 5px;
  color: #4e8fdd;
  font-size: 9px;
  font-weight: 950;
  letter-spacing: 0.1em;
  text-transform: uppercase;
}
.streak-dialog h2 {
  @apply ma-0;
  font-size: 25px;
  letter-spacing: -0.04em;
}
.streak-subtitle {
  max-width: 225px;
  color: #65766f;
  font-size: 11px;
  line-height: 1.4;
}
.close-button {
  width: 36px;
  height: 36px;
  @apply position-absolute d-grid place-center cursor-pointer;
  top: 8px;
  right: 8px;
  z-index: 5;
  color: #35574f;
  border: 2px solid #fff;
  border-radius: 13px;
  background: #f4f8f6;
  box-shadow: 0 4px 0 rgba(82, 123, 106, 0.13);
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.close-button:hover {
  transform: translateY(-2px) rotate(4deg);
  box-shadow: 0 6px 0 rgba(82, 123, 106, 0.13);
}
.close-button:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0 rgba(82, 123, 106, 0.13);
}
.streak-hero {
  width: 82px;
  height: 82px;
  @apply position-absolute d-grid place-center;
  top: 23px;
  right: 44px;
  z-index: 2;
  border-radius: 25px;
  background: rgba(255, 255, 255, 0.7);
  box-shadow: 0 4px 0 rgba(78, 143, 221, 0.12);
}
.hero-spark {
  @apply position-absolute;
  z-index: 2;
  color: #df8b2a;
  font-size: 16px;
  animation: spark-pulse 1.8s ease-in-out infinite;
}
.hero-spark--one {
  top: 4px;
  left: 5px;
}
.hero-spark--two {
  right: 1px;
  bottom: 14px;
  animation-delay: -0.8s;
}
.summary-grid {
  @apply position-relative d-grid;
  z-index: 1;
  grid-template-columns: 1fr 1fr;
  gap: 9px;
}
.summary-tile {
  min-height: 68px;
  @apply min-w-0 pa-2 d-flex align-center;
  gap: 8px;
  border: 2px solid rgba(73, 151, 198, 0.18);
  border-radius: 18px;
  background:
    radial-gradient(circle at 90% 8%, rgba(255, 215, 88, 0.2), transparent 27%),
    linear-gradient(145deg, #eaf7ff, #edf9f3 62%, #fff4cf);
  box-shadow: 0 5px 0 rgba(58, 127, 174, 0.12);
}
.summary-tile--week {
  border-color: rgba(167, 118, 194, 0.17);
  background: linear-gradient(145deg, #fff1f8, #f1f0ff 58%, #fff6d8);
  box-shadow: 0 5px 0 rgba(143, 98, 157, 0.11);
}
.summary-icon {
  width: 40px;
  height: 40px;
  @apply d-grid place-center;
  flex: 0 0 40px;
  color: #fff !important;
  border: 3px solid #fff;
  border-radius: 14px;
  background: linear-gradient(145deg, #6bc3a0, #4387d2) !important;
  box-shadow: 0 4px 0 #3574aa !important;
  transform: rotate(-5deg);
}
.summary-tile--week .summary-icon {
  background: linear-gradient(145deg, #d589c7, #826ec5) !important;
  box-shadow: 0 4px 0 #6d58a8 !important;
}
.summary-tile span,
.summary-tile strong {
  @apply d-block;
}
.summary-tile span {
  color: var(--lad-muted);
  font-size: 9px;
}
.summary-tile strong {
  margin-top: 1px;
  font-size: 17px;
  @apply text-no-wrap;
}
.streak-content {
  min-height: 0;
  flex: 1 1 auto;
  @apply d-flex flex-column overflow-hidden;
  gap: 12px;
  background: linear-gradient(180deg, #fffdf8, #f4faf7);
}
.week-heading {
  @apply d-flex align-end justify-space-between;
  gap: 10px;
}
.week-heading > div span,
.week-heading > div strong {
  @apply d-block;
}
.week-heading > div strong {
  margin-top: 3px;
  font-size: 18px;
}
.section-kicker {
  color: #278568;
  font-size: 8px;
  font-weight: 950;
  letter-spacing: 0.11em;
}
.week-days {
  padding: 18px 11px 12px;
  @apply d-grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  @apply ga-1;
  border: 2px solid rgba(78, 143, 221, 0.17);
  border-radius: 23px;
  background:
    radial-gradient(
      circle at 88% 2%,
      rgba(255, 218, 90, 0.24),
      transparent 30%
    ),
    linear-gradient(145deg, #f8fcff, #eef9f5);
  box-shadow:
    0 7px 0 rgba(78, 143, 221, 0.1),
    0 14px 24px rgba(61, 112, 89, 0.07);
}
.week-day {
  @apply min-w-0 position-relative text-center;
}
.week-day:not(:nth-child(7))::after {
  content: "";
  width: calc(100% - 35px);
  height: 4px;
  @apply position-absolute;
  top: 34px;
  left: calc(50% + 20px);
  z-index: 0;
  border-radius: 999px;
  background: #e4ede8;
}
.week-day--done:not(:nth-child(7))::after {
  background: #80cfaf;
}
.week-day > span {
  color: #40524b;
  font-size: 10px;
  @apply font-weight-black;
}
.day-symbol {
  width: 40px;
  height: 40px;
  margin: 6px auto;
  @apply position-relative;
  z-index: 1;
  @apply d-grid place-center;
  color: #9aa8a2;
  border: 2px solid #dfe9e4;
  border-radius: 50%;
  background: #f5f8f6;
}
.week-day small {
  @apply d-block overflow-hidden;
  color: var(--lad-muted);
  font-size: 8px;
  text-overflow: ellipsis;
  @apply text-no-wrap;
}
.week-day--done .day-symbol {
  color: white;
  border: 3px solid #eafff6;
  background: linear-gradient(145deg, #4dcca0, #218461);
  box-shadow:
    0 4px 0 #145b45,
    0 8px 13px rgba(28, 119, 87, 0.16);
  animation: day-done-arrive 0.65s cubic-bezier(0.2, 0.8, 0.3, 1) both;
  animation-delay: calc(var(--day-index) * 70ms);
}
.week-day--done .day-symbol :deep(.v-icon) {
  color: white !important;
  opacity: 1;
}
.week-day--today .day-symbol {
  color: #99600c;
  border: 3px solid #fff9dc;
  background: linear-gradient(145deg, #fff5c8, #ffd46a);
  box-shadow:
    0 0 0 6px rgba(255, 201, 92, 0.18),
    0 4px 0 #cc8726;
  animation: day-today-pulse 2.2s ease-in-out infinite;
}
.week-day--done small {
  color: #164f3e;
  font-weight: 950;
}
.week-day--today small {
  color: #99600c;
  @apply font-weight-black;
}
.week-motivation {
  min-height: 48px;
  padding: 7px 11px;
  @apply d-flex align-center;
  grid-column: 1 / -1;
  gap: 9px;
  margin-top: 8px;
  color: #275d4d;
  border: 1px solid rgba(62, 170, 127, 0.17);
  border-radius: 16px;
  background: linear-gradient(145deg, #e7f8f0, #fff5cf);
  box-shadow: 0 3px 0 rgba(47, 139, 103, 0.09);
}
.week-motivation span,
.week-motivation strong,
.week-motivation small {
  @apply d-block;
}
.week-motivation strong {
  font-size: 11px;
}
.week-motivation small {
  margin-top: 2px;
  color: #647971;
  font-size: 8px;
  line-height: 1.3;
}
.ladi-level {
  min-height: 118px;
  padding: 13px 17px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 16px;
  border: 2px solid rgba(78, 143, 221, 0.16);
  border-radius: 22px;
  background: linear-gradient(145deg, #fff, #eef7ff);
  box-shadow: 0 7px 0 rgba(78, 143, 221, 0.11);
}
.ladi-level::after {
  content: "✦";
  @apply position-absolute;
  top: 9px;
  right: 12px;
  color: #e6a52d;
}
.ladi-level > div:last-child {
  @apply min-w-0;
}
.ladi-level strong,
.ladi-level span {
  @apply d-block;
}
.ladi-level strong {
  margin-top: 2px;
  font-size: 17px;
}
.ladi-level span {
  margin-top: 4px;
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.45;
}
@keyframes spark-pulse {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.75) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.15) rotate(18deg);
  }
}
@keyframes day-done-arrive {
  from {
    opacity: 0;
    transform: translateY(8px) scale(0.7) rotate(-12deg);
  }
  70% {
    transform: translateY(-2px) scale(1.08) rotate(4deg);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes day-today-pulse {
  0%,
  100% {
    transform: translateY(0) rotate(-2deg);
    box-shadow:
      0 0 0 5px rgba(255, 201, 92, 0.15),
      0 4px 0 #cc8726;
  }
  50% {
    transform: translateY(-3px) rotate(3deg);
    box-shadow:
      0 0 0 9px rgba(255, 201, 92, 0.09),
      0 6px 0 #cc8726;
  }
}
@media (prefers-reduced-motion: reduce) {
  .hero-spark,
  .week-day--done .day-symbol,
  .week-day--today .day-symbol {
    animation: none;
  }
}
:global(.series-summary-dialog-frame) {
  width: min(480px, calc(100vw - 24px));
  max-height: calc(100dvh - 24px);
}
</style>
