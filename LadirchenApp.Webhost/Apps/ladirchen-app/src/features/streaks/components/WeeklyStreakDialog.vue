<template>
  <v-dialog content-class="series-summary-dialog-frame" :model-value="modelValue" max-width="480" @update:model-value="emit('update:modelValue', $event)">
    <v-card class="streak-dialog" rounded="xl">
      <div class="streak-header pa-4">
        <div class="streak-title-row">
          <HeaderDecoration tone="streak" />
          <div>
            <p class="dialog-kicker">{{ t('streaks.weekly.eyebrow') }}</p>
            <h2>{{ t('streaks.weekly.title') }}</h2>
            <p class="streak-subtitle mt-1">{{ t('streaks.weekly.description') }}</p>
          </div>
          <div class="streak-hero" aria-hidden="true">
            <span class="hero-spark hero-spark--one">✦</span>
            <span class="hero-spark hero-spark--two">✦</span>
            <AnimatedStreakFlame :size="80" />
          </div>
          <button class="close-button" :aria-label="t('streaks.weekly.close')" type="button" @click="close"><v-icon icon="i-mdi:close" /></button>
        </div>

        <div class="summary-grid mt-4">
          <MetricCard class="summary-tile summary-tile--streak" tone="streak">
            <div class="summary-icon" aria-hidden="true"><v-icon icon="i-mdi:fire" size="25" /></div>
            <div><span>{{ t('streaks.weekly.summary.streak') }}</span><strong>{{ t('streaks.weekly.summary.days', { count: store.currentDailyStreak }) }}</strong></div>
          </MetricCard>
          <MetricCard class="summary-tile summary-tile--week" tone="bonus">
            <div class="summary-icon" aria-hidden="true"><v-icon icon="i-mdi:calendar-heart" size="23" /></div>
            <div><span>{{ t('streaks.weekly.summary.thisWeek') }}</span><strong>{{ t('streaks.weekly.summary.progress', { current: store.currentWeekDays, target: store.currentWeekTarget }) }}</strong></div>
          </MetricCard>
        </div>
      </div>

      <div class="streak-content pa-4">
        <div v-if="store.viewerRole === 'child'" class="ladi-level">
          <LadiMascot :score="store.averageTaskRating" :show-score="false" :size="88" />
          <div>
            <p class="section-kicker">{{ t('streaks.weekly.ladiLevel') }}</p>
            <strong>{{ t(ladiStage.nameKey) }}</strong>
            <span>{{ t(ladiStage.descriptionKey) }}</span>
          </div>
        </div>

        <div class="week-heading" :class="{ 'mt-4': store.viewerRole === 'child' }">
          <div><span class="section-kicker">{{ t('streaks.weekly.path.eyebrow') }}</span><strong>{{ t('streaks.weekly.path.title') }}</strong></div>
        </div>

        <div :aria-label="t('streaks.weekly.path.progressAria')" class="week-days mt-3">
          <div v-for="(day, index) in weekDays" :key="day.fullLabel" :class="['week-day', `week-day--${day.status}`]" :style="{ '--day-index': index }" :aria-label="t('streaks.weekly.path.dayAria', { day: day.fullLabel, status: statusLabel(day.status) })">
            <span>{{ day.label }}</span>
            <div class="day-symbol">
              <v-icon v-if="day.status === DayStatus.Done" size="19">i-mdi:check-bold</v-icon>
              <v-icon v-else-if="day.status === DayStatus.Today" size="18">i-mdi:star-four-points</v-icon>
              <v-icon v-else size="14">i-mdi:circle-small</v-icon>
            </div>
            <small>{{ statusLabel(day.status) }}</small>
          </div>
          <div class="week-motivation">
            <AnimatedStreakFlame :size="27" />
            <span><strong>{{ weekMessage }}</strong><small>{{ t('streaks.weekly.motivation.description') }}</small></span>
          </div>
        </div>

      </div>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

import AnimatedStreakFlame from './AnimatedStreakFlame.vue';
import LadiMascot from '@/shared/components/LadiMascot.vue';
import MetricCard from '@/shared/components/ui/MetricCard.vue';
import HeaderDecoration from '@/shared/components/ui/HeaderDecoration.vue';
import { getLadiStage } from '@/domain/ladi';
import { approvedContributionDatesInCurrentWeek } from '@/domain/contributions/weekly-progress';
import { addCalendarDays, calendarDateInTimeZone, startOfIsoWeek } from '@/domain/shared/zoned-calendar';
import { useFamilyWorldStore } from '@/stores/family-world';

enum DayStatus {
  Done = 'done',
  Today = 'today',
  Upcoming = 'upcoming',
}

defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>();
const store = useFamilyWorldStore();
const { locale, t } = useI18n();
const labels = computed(() => {
  const now = new Date(store.currentTimeMilliseconds);
  const monday = startOfIsoWeek(now, store.familyTimeZone);
  const shortFormatter = new Intl.DateTimeFormat(locale.value, { timeZone: 'UTC', weekday: 'short' });
  const longFormatter = new Intl.DateTimeFormat(locale.value, { timeZone: 'UTC', weekday: 'long' });
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
const dayStatus = (calendarDate: string): DayStatus => completedDates.value.has(calendarDate)
  ? DayStatus.Done
  : calendarDate === today.value
    ? DayStatus.Today
    : DayStatus.Upcoming;
const weekDays = computed(() => labels.value.map(day => ({
  ...day,
  status: dayStatus(day.calendarDate),
})));
const ladiStage = computed(() => getLadiStage(store.averageTaskRating));
const weekMessage = computed(() => store.currentWeekDays >= store.currentWeekTarget
  ? t('streaks.weekly.motivation.complete')
  : t('streaks.weekly.motivation.active'));

const statusLabel = (status: DayStatus) => {
  return t(`streaks.weekly.status.${status}`);
};
const close = () => emit('update:modelValue', false);
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.streak-dialog {
  max-height: calc(100dvh - 28px);
  @apply d-flex flex-column overflow-hidden;
  @include dialog-frame;
}
.streak-header {
  @apply position-relative;
  flex: 0 0 auto;
  background: var(--lad-surface);
}
.streak-title-row {
  min-height: 126px;
  padding: 18px;
  @apply d-flex align-center;
  z-index: 1;
  @include dialog-title-panel;
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
  background: color-mix(in srgb, var(--lad-color-info-soft) 60%, transparent);
  box-shadow: 0 0 0 17px
    color-mix(in srgb, var(--lad-surface-soft) 40%, transparent);
}
.streak-title-row::after {
  width: 84px;
  height: 26px;
  right: 44px;
  bottom: -16px;
  background: color-mix(in srgb, var(--lad-color-info-soft) 60%, transparent);
}
.streak-title-row > div:first-child {
  max-width: 245px;
  @apply position-relative;
  z-index: 1;
}
.dialog-kicker {
  margin: 0 0 5px;
  @include overline(var(--lad-blue), rem(9));
}
.streak-dialog h2 {
  @apply ma-0;
  @include heading(rem(25), 1.1, -0.04em);
}
.streak-subtitle {
  max-width: 225px;
  @include body-copy(rem(11));
}
.close-button {
  @apply position-absolute;
  top: 8px;
  right: 8px;
  z-index: 5;
  @include dialog-close-button;
}
.streak-hero {
  width: 82px;
  height: 82px;
  @apply position-absolute d-grid place-center;
  top: 23px;
  right: 44px;
  z-index: 2;
  border-radius: 25px;
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-color-info) 12%, transparent);
}
.hero-spark {
  @apply position-absolute;
  z-index: 2;
  color: var(--lad-color-reward-accent);
  font-size: 1rem;
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
.streak-title-row::before,
.streak-title-row::after {
  content: none;
}
.summary-grid {
  @apply position-relative d-grid;
  z-index: 1;
  grid-template-columns: 1fr 1fr;
  gap: 9px;
}
.summary-tile {
  @apply min-w-0;
}
.summary-icon {
  @include icon-tile(
    2.5rem,
    rem(14),
    linear-gradient(
      145deg,
      var(--lad-color-primary-highlight),
      var(--lad-color-info)
    ),
    var(--lad-color-info-strong),
    -5deg,
    rem(3) solid var(--lad-surface-raised)
  );
  flex-basis: 40px;
  color: var(--lad-text-inverse);
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-info)
  );
  box-shadow: 0 4px 0 var(--lad-color-info-strong);
}
.summary-tile--week .summary-icon {
  background: linear-gradient(
    145deg,
    var(--lad-color-accent-pink),
    var(--lad-color-bonus)
  );
  box-shadow: 0 4px 0 var(--lad-color-bonus-muted);
}
.summary-icon :deep(.v-icon) {
  color: currentColor;
  opacity: 1;
}
.summary-tile span,
.summary-tile strong {
  @apply d-block;
}
.summary-tile span {
  color: var(--lad-muted);
  font-size: rem(9);
}
.summary-tile strong {
  margin-top: 1px;
  font-size: rem(17);
  @apply text-no-wrap;
}
.streak-content {
  min-height: 0;
  flex: 1 1 auto;
  @apply d-flex flex-column overflow-hidden;
  gap: 12px;
  background: linear-gradient(
    180deg,
    var(--lad-surface),
    var(--lad-surface-soft)
  );
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
  font-size: rem(18);
}
.section-kicker {
  @include overline(
    var(--lad-color-primary-strong),
    var(--lad-font-size-micro),
    0.11em
  );
}
.week-days {
  padding: 18px 11px 12px;
  @apply d-grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  @apply ga-1;
  border: 2px solid color-mix(in srgb, var(--lad-color-info) 18%, transparent);
  border-radius: 23px;
  background:
    radial-gradient(
      circle at 88% 2%,
      color-mix(in srgb, var(--lad-color-reward) 25%, transparent),
      transparent 30%
    ),
    linear-gradient(145deg, var(--lad-surface-raised), var(--lad-surface-soft));
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-color-info) 10%, transparent),
    0 14px 24px color-mix(in srgb, var(--lad-text-strong) 8%, transparent);
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
  border-radius: var(--lad-radius-pill);
  background: var(--lad-surface-soft);
}
.week-day--done:not(:nth-child(7))::after {
  background: var(--lad-color-primary-highlight);
}
.week-day > span {
  color: var(--lad-text-strong);
  font-size: rem(10);
  @apply font-weight-black;
}
.day-symbol {
  width: 40px;
  height: 40px;
  margin: 6px auto;
  @apply position-relative;
  z-index: 1;
  @apply d-grid place-center;
  color: var(--lad-text-subtle);
  border: 2px solid var(--lad-color-primary-soft);
  border-radius: 50%;
  background: var(--lad-surface-soft);
}
.week-day small {
  @apply d-block overflow-hidden;
  color: var(--lad-muted);
  font-size: 0.5rem;
  text-overflow: ellipsis;
  @apply text-no-wrap;
}
.week-day--done .day-symbol {
  color: var(--lad-text-inverse);
  border: 3px solid var(--lad-surface-soft);
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-strong)
  );
  box-shadow:
    0 4px 0 var(--lad-color-primary-deep),
    0 8px 13px
      color-mix(in srgb, var(--lad-color-primary-deep) 15%, transparent);
  animation: day-done-arrive 0.65s cubic-bezier(0.2, 0.8, 0.3, 1) both;
  animation-delay: calc(var(--day-index) * 70ms);
}
.week-day--done .day-symbol :deep(.v-icon) {
  color: var(--lad-text-inverse);
  opacity: 1;
}
.week-day--today .day-symbol {
  color: var(--lad-color-reward-ink);
  border: 3px solid var(--lad-color-reward-soft);
  background: linear-gradient(
    145deg,
    var(--lad-color-reward-soft),
    var(--lad-color-reward)
  );
  box-shadow:
    0 0 0 6px color-mix(in srgb, var(--lad-color-reward) 18%, transparent),
    0 4px 0 var(--lad-color-reward-shadow);
  animation: day-today-pulse 2.2s ease-in-out infinite;
}
.week-day--done small {
  color: var(--lad-text);
  font-weight: var(--lad-font-weight-black);
}
.week-day--today small {
  color: var(--lad-color-reward-ink);
  @apply font-weight-black;
}
.week-motivation {
  min-height: 48px;
  padding: 7px 11px;
  @apply d-flex align-center;
  grid-column: 1 / -1;
  gap: 9px;
  margin-top: 8px;
  color: var(--lad-color-primary-deep);
  border: 1px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 18%, transparent);
  border-radius: 16px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary-strong) 8%, transparent);
}
.week-motivation span,
.week-motivation strong,
.week-motivation small {
  @apply d-block;
}
.week-motivation strong {
  font-size: rem(11);
}
.week-motivation small {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.5rem;
  line-height: 1.3;
}
.ladi-level {
  min-height: 118px;
  padding: 13px 17px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 16px;
  border: 2px solid color-mix(in srgb, var(--lad-color-info) 15%, transparent);
  border-radius: 22px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow: 0 7px 0 color-mix(in srgb, var(--lad-color-info) 10%, transparent);
}
.ladi-level::after {
  content: "✦";
  @apply position-absolute;
  top: 9px;
  right: 12px;
  color: var(--lad-color-reward-border);
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
  font-size: rem(17);
}
.ladi-level span {
  margin-top: 4px;
  color: var(--lad-muted);
  font-size: rem(10);
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
      0 0 0 5px color-mix(in srgb, var(--lad-color-reward) 15%, transparent),
      0 4px 0 var(--lad-color-reward-shadow);
  }
  50% {
    transform: translateY(-3px) rotate(3deg);
    box-shadow:
      0 0 0 9px color-mix(in srgb, var(--lad-color-reward) 8%, transparent),
      0 6px 0 var(--lad-color-reward-shadow);
  }
}
@include reduced-motion {
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
