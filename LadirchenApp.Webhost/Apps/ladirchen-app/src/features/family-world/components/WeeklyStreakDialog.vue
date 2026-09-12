<template>
  <v-dialog :model-value="modelValue" max-width="460" @update:model-value="emit('update:modelValue', $event)">
    <v-card class="streak-dialog" rounded="xl">
      <div class="streak-header pa-5">
        <div class="streak-title-row">
          <div>
            <p class="eyebrow mb-1">Jeden Tag gemeinsam</p>
            <h2>Tagesserie</h2>
            <p class="streak-subtitle mt-1">Sammelt gemeinsam Feuer für euer Zuhause.</p>
          </div>
          <div class="streak-hero" aria-hidden="true">
            <span class="hero-spark hero-spark--one">✦</span>
            <span class="hero-spark hero-spark--two">✦</span>
            <AnimatedStreakFlame :size="70" />
          </div>
          <v-btn class="close-button" aria-label="Wochenübersicht schließen" icon="mdi-close" size="small" variant="text" @click="close" />
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

      <div class="streak-content pa-5">
        <div class="week-heading">
          <div><span class="section-kicker">EUER WOCHENWEG</span><strong>Ein Tag nach dem anderen</strong></div>
          <span v-if="remainingDays > 0">Noch {{ remainingDays }}</span>
          <span v-else class="week-complete">Komplett!</span>
        </div>

        <div aria-label="Fortschritt dieser Woche" class="week-days mt-3">
          <div v-for="day in weekDays" :key="day.label" :class="['week-day', `week-day--${day.status}`]" :aria-label="`${day.fullLabel}: ${statusLabel(day.status)}`">
            <span>{{ day.label }}</span>
            <div class="day-symbol">
              <v-icon v-if="day.status === 'done'" size="19">mdi-check-bold</v-icon>
              <v-icon v-else-if="day.status === 'today'" size="18">mdi-star-four-points</v-icon>
              <v-icon v-else size="14">mdi-circle-small</v-icon>
            </div>
            <small>{{ statusLabel(day.status) }}</small>
          </div>
        </div>

        <div class="week-progress mt-4">
          <v-progress-linear color="primary" height="10" :model-value="weekProgress" rounded />
          <p class="week-message mt-3">{{ weekMessage }}</p>
        </div>

        <div class="energy-card mt-5">
          <div class="energy-illustration" aria-hidden="true">
            <v-icon size="31">mdi-home-heart</v-icon>
            <span>⚡</span>
          </div>
          <div class="energy-copy">
            <span class="section-kicker">HEUTIGES GEMEINSAMES ZIEL</span>
            <div class="energy-row">
              <strong>Hausenergie sammeln</strong>
              <b>{{ store.familyEnergy }} %</b>
            </div>
            <v-progress-linear :color="store.houseMeetsMinimumEnergy ? 'primary' : 'warning'" height="9" :model-value="store.familyEnergy" rounded />
            <p v-if="store.houseMeetsMinimumEnergy" class="energy-success mt-2"><v-icon size="15">mdi-check-circle</v-icon> Geschafft – heute zählt für eure Serie!</p>
            <p v-else class="energy-hint mt-2">Ab <b>60 %</b> zählt der Tag. Bestätigt dafür gemeinsam eure Grundbeiträge.</p>
          </div>
        </div>

        <v-btn class="mt-5" color="primary" rounded="lg" size="large" variant="flat" width="100%" @click="close">Weiter gemeinsam sammeln</v-btn>
      </div>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

import AnimatedStreakFlame from './AnimatedStreakFlame.vue';
import { useFamilyWorldStore } from '../stores/family-world';

type DayStatus = 'done' | 'today' | 'upcoming';

defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>();
const store = useFamilyWorldStore();
const labels = [
  { label: 'Mo', fullLabel: 'Montag' },
  { label: 'Di', fullLabel: 'Dienstag' },
  { label: 'Mi', fullLabel: 'Mittwoch' },
  { label: 'Do', fullLabel: 'Donnerstag' },
  { label: 'Fr', fullLabel: 'Freitag' },
  { label: 'Sa', fullLabel: 'Samstag' },
  { label: 'So', fullLabel: 'Sonntag' },
];

const weekDays = computed(() => labels.map((day, index) => ({
  ...day,
  status: (index < store.currentWeekDays
    ? 'done'
    : index === store.currentWeekDays && store.currentWeekDays < store.currentWeekTarget
      ? 'today'
      : 'upcoming') as DayStatus,
})));
const weekProgress = computed(() => Math.min(100, (store.currentWeekDays / store.currentWeekTarget) * 100));
const remainingDays = computed(() => Math.max(0, store.currentWeekTarget - store.currentWeekDays));
const weekMessage = computed(() => remainingDays.value === 0
  ? 'Die Woche ist vollständig. Jetzt wird die nächste Hausentwicklung enthüllt!'
  : remainingDays.value === 1
    ? 'Nur noch ein Tag bis zum Wochenabschluss – die nächste Hausentwicklung bleibt eine Überraschung.'
    : `Noch ${remainingDays.value} Tage bis zum Wochenabschluss – die nächste Hausentwicklung bleibt eine Überraschung.`);

const statusLabel = (status: DayStatus) => {
  if (status === 'done') return 'Geschafft';
  if (status === 'today') return 'Heute';
  return 'Kommt noch';
};
const close = () => emit('update:modelValue', false);
</script>

<style scoped>
.streak-dialog {
  max-height: min(820px, 94dvh);
  overflow: hidden;
  color: #253843;
  background: #fffdf8 !important;
}
.streak-header {
  position: relative;
  overflow: hidden;
  flex: 0 0 auto;
  background: linear-gradient(145deg, #ffe2e9 0%, #ffedda 48%, #fff4c9 100%);
  border-bottom: 1px solid rgba(181, 107, 107, 0.14);
}
.streak-header::before,
.streak-header::after {
  content: "";
  position: absolute;
  border-radius: 50%;
  pointer-events: none;
}
.streak-header::before {
  width: 190px;
  height: 190px;
  top: -112px;
  right: -35px;
  background: rgba(255, 255, 255, 0.33);
  box-shadow: 0 0 0 22px rgba(255, 255, 255, 0.15);
}
.streak-header::after {
  width: 100px;
  height: 35px;
  right: 77px;
  bottom: -20px;
  background: rgba(255, 255, 255, 0.32);
  filter: blur(2px);
}
.streak-title-row {
  min-height: 104px;
  position: relative;
  z-index: 1;
  display: flex;
  align-items: flex-start;
}
.streak-title-row > div:first-child {
  max-width: 255px;
}
.streak-dialog h2 {
  margin: 0;
  font-size: 27px;
  letter-spacing: -0.04em;
}
.streak-subtitle {
  max-width: 235px;
  color: #65766f;
  font-size: 12px;
  line-height: 1.4;
}
.close-button {
  position: absolute;
  top: -7px;
  right: -8px;
  z-index: 3;
}
.streak-hero {
  width: 92px;
  height: 92px;
  position: absolute;
  top: 13px;
  right: 20px;
  display: grid;
  place-items: center;
}
.hero-spark {
  position: absolute;
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
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}
.summary-tile {
  min-width: 0;
  padding: 12px;
  display: flex;
  align-items: center;
  gap: 10px;
  border: 1px solid rgba(110, 82, 72, 0.13);
  border-radius: 17px;
  background: rgba(255, 255, 255, 0.69);
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.7);
}
.summary-icon {
  width: 42px;
  height: 42px;
  display: grid;
  place-items: center;
  flex: 0 0 42px;
  border-radius: 14px;
}
.summary-tile--streak .summary-icon {
  color: #f09c31;
  background: #fff0bc;
}
.summary-tile--week .summary-icon {
  color: #238765;
  background: #e2f5ea;
}
.summary-tile span,
.summary-tile strong {
  display: block;
}
.summary-tile span {
  color: var(--lad-muted);
  font-size: 9px;
}
.summary-tile strong {
  margin-top: 1px;
  font-size: 17px;
  white-space: nowrap;
}
.streak-content {
  min-height: 0;
  flex: 1 1 auto;
  overflow-y: auto;
  background: linear-gradient(180deg, #fffdf8, #f2fbf6);
}
.week-heading {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 10px;
}
.week-heading > div span,
.week-heading > div strong {
  display: block;
}
.week-heading > div strong {
  margin-top: 2px;
  font-size: 15px;
}
.section-kicker {
  color: #278568;
  font-size: 8px;
  font-weight: 950;
  letter-spacing: 0.11em;
}
.week-heading > span {
  padding: 6px 10px;
  color: #78510f;
  border-radius: 999px;
  background: #ffe6a8;
  font-size: 9px;
  font-weight: 900;
}
.week-heading > .week-complete {
  color: #176348;
  background: #dff5e8;
}
.week-days {
  padding: 12px 7px 10px;
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  gap: 4px;
  border: 1px solid rgba(79, 124, 105, 0.11);
  border-radius: 19px;
  background: rgba(255, 255, 255, 0.76);
}
.week-day {
  min-width: 0;
  position: relative;
  text-align: center;
}
.week-day:not(:last-child)::after {
  content: "";
  width: calc(100% - 30px);
  height: 3px;
  position: absolute;
  top: 29px;
  left: calc(50% + 17px);
  z-index: 0;
  border-radius: 999px;
  background: #e4ede8;
}
.week-day--done:not(:last-child)::after {
  background: #80cfaf;
}
.week-day > span {
  color: #40524b;
  font-size: 9px;
  font-weight: 900;
}
.day-symbol {
  width: 32px;
  height: 32px;
  margin: 5px auto;
  position: relative;
  z-index: 1;
  display: grid;
  place-items: center;
  color: #9aa8a2;
  border: 2px solid #dfe9e4;
  border-radius: 50%;
  background: #f5f8f6;
}
.week-day small {
  display: block;
  overflow: hidden;
  color: var(--lad-muted);
  font-size: 7px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.week-day--done .day-symbol {
  color: white;
  border-color: #177153;
  background: linear-gradient(145deg, #42bf91, #238765);
  box-shadow: 0 3px 0 #145b45;
}
.week-day--done .day-symbol :deep(.v-icon) {
  color: white !important;
  opacity: 1;
}
.week-day--today .day-symbol {
  color: #99600c;
  border-color: #e7aa3b;
  background: #fff1bd;
  box-shadow:
    0 0 0 5px rgba(255, 201, 92, 0.18),
    0 3px 0 #cc8726;
}
.week-day--done small {
  color: #164f3e;
  font-weight: 950;
}
.week-day--today small {
  color: #99600c;
  font-weight: 900;
}
.week-progress {
  padding: 12px 13px;
  border-radius: 15px;
  background: linear-gradient(
    90deg,
    rgba(225, 247, 235, 0.72),
    rgba(255, 244, 203, 0.58)
  );
}
.week-message {
  margin-bottom: 0;
  color: #52655d;
  text-align: center;
  font-size: 9px;
  font-weight: 800;
  line-height: 1.4;
}
.energy-card {
  padding: 14px;
  display: flex;
  align-items: center;
  gap: 12px;
  border: 1px solid rgba(62, 188, 140, 0.2);
  border-radius: 20px;
  background: linear-gradient(145deg, #ecfaf3, #f9fff8);
  box-shadow: 0 4px 0 rgba(57, 137, 106, 0.08);
}
.energy-illustration {
  width: 58px;
  height: 58px;
  position: relative;
  display: grid;
  place-items: center;
  flex: 0 0 58px;
  color: #268765;
  border-radius: 18px;
  background: #d9f4e6;
}
.energy-illustration > span {
  position: absolute;
  top: -6px;
  right: -5px;
  width: 25px;
  height: 25px;
  display: grid;
  place-items: center;
  border: 3px solid #f7fff9;
  border-radius: 50%;
  background: #fff0ad;
  font-size: 12px;
}
.energy-copy {
  min-width: 0;
  flex: 1;
}
.energy-row {
  margin: 2px 0 7px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}
.energy-row strong {
  color: var(--lad-text);
  font-size: 13px;
}
.energy-row b {
  color: #1c8d67;
  font-size: 18px;
}
.energy-success,
.energy-hint {
  margin-bottom: 0;
  color: #287458;
  font-size: 9px;
  font-weight: 750;
  line-height: 1.35;
}
.energy-success {
  display: flex;
  align-items: center;
  gap: 4px;
}
.energy-hint {
  color: var(--lad-muted);
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
@media (prefers-reduced-motion: reduce) {
  .hero-spark {
    animation: none;
  }
}
</style>
