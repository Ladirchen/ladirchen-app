<template>
  <section class="achievement-area mb-6">
    <div class="achievement-stats" :aria-label="t('world.achievements.stats.aria')">
      <div>
        <v-icon color="warning" icon="i-mdi:trophy-variant-outline" size="22" />
        <strong>{{ weeklyAchievementCount }}</strong>
        <span>{{ t("world.achievements.stats.weeklyGoals") }}</span>
      </div>
      <div>
        <v-icon color="success" icon="i-mdi:check-circle-outline" size="22" />
        <strong>{{ totalCompletedTasks }}</strong>
        <span>{{ t("world.achievements.stats.completedTasks") }}</span>
      </div>
      <div>
        <v-icon color="amber-darken-2" icon="i-mdi:circle-multiple-outline" size="22" />
        <strong>{{ formattedTotalEarned }}</strong>
        <span>{{ t("world.achievements.stats.earnedLadirchen") }}</span>
      </div>
    </div>

    <BrandedCard class="achievement-collection pa-4 mt-3" tone="world">
      <div class="d-flex align-center justify-space-between ga-3 mb-3">
        <div>
          <p class="eyebrow mb-1">{{ t("world.achievements.eyebrow") }}</p>
          <h2>{{ t("world.achievements.title") }}</h2>
        </div>
        <v-avatar color="warning" size="46" variant="tonal">
          <v-icon icon="i-mdi:trophy-variant" />
        </v-avatar>
      </div>

      <div v-if="achievements.length" class="trophy-shelf">
        <button
          v-for="achievement in achievements"
          :key="achievement.id"
          :aria-label="t('world.achievements.openAria', { title: achievement.title })"
          class="trophy-button"
          :class="{ 'trophy-button--unlocked': achievement.id === unlockedAchievementId }"
          type="button"
          @click="openAchievement(achievement)"
        >
          <span class="trophy" aria-hidden="true">
            <v-icon icon="i-mdi:trophy-variant" size="42" />
            <i>{{ achievement.icon }}</i>
          </span>
          <strong>{{ achievement.title }}</strong>
        </button>
      </div>

      <div v-else class="achievement-empty">
        <v-icon color="warning" icon="i-mdi:trophy-outline" size="34" />
        <div>
          <strong>{{ t("world.achievements.emptyTitle") }}</strong>
          <p>{{ t("world.achievements.emptyDescription") }}</p>
        </div>
      </div>
    </BrandedCard>

    <v-dialog v-model="detailOpen" max-width="430">
      <v-card v-if="selectedAchievement" class="achievement-detail pa-5" rounded="xl">
        <div class="achievement-detail-trophy" aria-hidden="true">
          <v-icon icon="i-mdi:trophy-variant" size="72" />
          <span>{{ selectedAchievement.icon }}</span>
        </div>
        <p class="eyebrow text-center mt-3 mb-1">{{ t("world.achievements.detail.eyebrow") }}</p>
        <h2 class="text-center">{{ selectedAchievement.title }}</h2>
        <p class="achievement-description text-center mt-2">{{ selectedAchievement.description }}</p>
        <div class="achievement-period mt-4">
          <v-icon icon="i-mdi:calendar-check-outline" size="20" />
          <span>
            {{
              t("world.achievements.detail.period", {
                from: formatDate(selectedAchievement.startDate),
                to: formatDate(selectedAchievement.endDate),
              })
            }}
          </span>
        </div>
        <div class="achievement-rewards mt-3">
          <div>
            <v-icon color="success" icon="i-mdi:check-circle-outline" />
            <strong>{{ selectedAchievement.preparationCount }}</strong>
            <span>{{ t("world.achievements.detail.preparations") }}</span>
          </div>
          <div>
            <v-icon color="primary" icon="i-mdi:lightning-bolt" />
            <strong>{{ formatNumber(selectedAchievement.earnedEnergy) }}</strong>
            <span>{{ t("world.achievements.detail.energy") }}</span>
          </div>
          <div>
            <v-icon color="amber-darken-2" icon="i-mdi:circle-multiple-outline" />
            <strong>{{ formatNumber(selectedAchievement.earnedLadirchen) }}</strong>
            <span>{{ t("world.achievements.detail.ladirchen") }}</span>
          </div>
        </div>
        <p class="achievement-date mt-4">
          {{ t("world.achievements.detail.achievedAt", { date: formatInstant(selectedAchievement.achievedAt) }) }}
        </p>
        <v-btn class="mt-4" color="primary" rounded="lg" variant="flat" width="100%" @click="detailOpen = false">
          {{ t("common.close") }}
        </v-btn>
      </v-card>
    </v-dialog>
  </section>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { FocusAchievement } from "@/domain/contributions/types";
import type { IanaTimeZone } from "@/domain/family/time-zone";
import type { FamilyMemberId } from "@/domain/shared/identifiers";
import { isInstantInIsoWeek } from "@/domain/shared/zoned-calendar";
import BrandedCard from "@/shared/components/ui/BrandedCard.vue";

export interface Props {
  achievements: FocusAchievement[];
  childId: FamilyMemberId;
  currentTimeMilliseconds: number;
  initialAchievementId?: string;
  timeZone: IanaTimeZone;
  totalCompletedTasks: number;
  totalEarned: number;
}

const props = defineProps<Props>();
const { locale, t } = useI18n();
const detailOpen = ref(false);
const selectedAchievement = ref<FocusAchievement>();
const unlockedAchievementId = ref<string>();
const weeklyAchievementCount = computed(
  () =>
    props.achievements.filter((achievement) =>
      isInstantInIsoWeek(achievement.achievedAt, new Date(props.currentTimeMilliseconds), props.timeZone),
    ).length,
);
const formattedTotalEarned = computed(() => formatNumber(props.totalEarned));

function formatNumber(value: number): string {
  return new Intl.NumberFormat(locale.value, { maximumFractionDigits: 2 }).format(value);
}

function formatDate(value: string): string {
  return new Intl.DateTimeFormat(locale.value, { dateStyle: "medium" }).format(new Date(`${value}T12:00:00`));
}

function formatInstant(value: string): string {
  return new Intl.DateTimeFormat(locale.value, { dateStyle: "long" }).format(new Date(value));
}

function openAchievement(achievement: FocusAchievement): void {
  selectedAchievement.value = achievement;
  detailOpen.value = true;
}

function readSeenAchievementIds(storageKey: string): Set<string> {
  try {
    const storedValue = JSON.parse(localStorage.getItem(storageKey) ?? "[]") as unknown;

    return new Set(Array.isArray(storedValue) ? storedValue.filter((value) => typeof value === "string") : []);
  } catch {
    return new Set();
  }
}

function revealNewAchievement(): void {
  if (props.achievements.length === 0) {
    unlockedAchievementId.value = undefined;

    return;
  }

  const storageKey = `ladirli:focus-achievements:${props.childId}`;
  const seenIds = readSeenAchievementIds(storageKey);
  const unseenAchievement = props.achievements.find((achievement) => !seenIds.has(achievement.id));
  unlockedAchievementId.value = unseenAchievement?.id;

  for (const achievement of props.achievements) {
    seenIds.add(achievement.id);
  }

  try {
    localStorage.setItem(storageKey, JSON.stringify([...seenIds]));
  } catch {
    // The collection still works when storage is unavailable; only the one-time marker is skipped.
  }

  if (props.initialAchievementId) {
    const linkedAchievement = props.achievements.find((achievement) => achievement.id === props.initialAchievementId);

    if (linkedAchievement) {
      openAchievement(linkedAchievement);
    }
  }
}

watch(
  () => [props.childId, props.initialAchievementId, ...props.achievements.map((achievement) => achievement.id)],
  revealNewAchievement,
  { immediate: true },
);
</script>

<style lang="scss" scoped>
.achievement-stats {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;
}

.achievement-stats > div {
  min-width: 0;
  padding: 11px 8px;
  display: grid;
  grid-template-columns: auto 1fr;
  align-items: center;
  gap: 2px 7px;
  border: 1px solid color-mix(in srgb, var(--lad-color-primary-muted) 20%, transparent);
  border-radius: 17px;
  background: var(--lad-surface-raised);
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-color-primary-strong) 8%, transparent);
}

.achievement-stats strong {
  font-size: 1.08rem;
}

.achievement-stats span {
  grid-column: 1 / -1;
  color: var(--lad-muted);
  font-size: 0.64rem;
  line-height: 1.25;
}

.achievement-collection {
  overflow: hidden;
  border: 2px solid color-mix(in srgb, var(--lad-color-reward-border) 34%, transparent);
  background:
    radial-gradient(
      circle at 90% 5%,
      color-mix(in srgb, var(--lad-color-reward-highlight) 30%, transparent),
      transparent 28%
    ),
    var(--lad-surface-raised);
}

.achievement-collection h2 {
  font-size: 1.2rem;
}

.trophy-shelf {
  display: flex;
  gap: 12px;
  overflow-x: auto;
  padding: 8px 2px 10px;
  scrollbar-width: thin;
}

.trophy-button {
  width: 98px;
  min-width: 98px;
  padding: 10px 7px;
  color: inherit;
  border: 0;
  border-radius: 18px;
  background: linear-gradient(155deg, var(--lad-color-reward-soft), var(--lad-surface-soft));
  box-shadow: 0 5px 0 color-mix(in srgb, var(--lad-color-reward-deep) 13%, transparent);
  font: inherit;
  cursor: pointer;
}

.trophy-button strong {
  display: -webkit-box;
  margin-top: 7px;
  overflow: hidden;
  font-size: 0.68rem;
  line-height: 1.25;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

.trophy {
  position: relative;
  display: inline-grid;
  place-items: center;
  color: var(--lad-color-reward-deep);
  filter: drop-shadow(0 4px 4px color-mix(in srgb, var(--lad-color-reward-deep) 22%, transparent));
}

.trophy i {
  position: absolute;
  top: 9px;
  font-size: 0.72rem;
  font-style: normal;
}

.trophy-button--unlocked {
  animation: trophy-unlock 900ms cubic-bezier(0.16, 1.3, 0.3, 1) both;
}

.trophy-button--unlocked .trophy {
  animation: trophy-glow 900ms ease-out both;
}

.achievement-empty,
.achievement-period {
  display: flex;
  align-items: center;
  gap: 12px;
}

.achievement-empty {
  padding: 13px;
  border: 1px dashed color-mix(in srgb, var(--lad-color-reward-border) 55%, transparent);
  border-radius: 17px;
}

.achievement-empty p,
.achievement-description,
.achievement-date {
  color: var(--lad-muted);
  font-size: 0.76rem;
}

.achievement-detail-trophy {
  position: relative;
  display: grid;
  place-items: center;
  color: var(--lad-color-reward-deep);
}

.achievement-detail-trophy span {
  position: absolute;
  top: 16px;
  font-size: 1.2rem;
}

.achievement-period {
  padding: 11px 13px;
  border-radius: 14px;
  background: var(--lad-surface-soft);
  font-size: 0.78rem;
}

.achievement-rewards {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;
}

.achievement-rewards > div {
  min-width: 0;
  padding: 10px 5px;
  display: grid;
  place-items: center;
  border-radius: 15px;
  background: var(--lad-surface-soft);
  text-align: center;
}

.achievement-rewards strong {
  margin-top: 3px;
}

.achievement-rewards span {
  color: var(--lad-muted);
  font-size: 0.62rem;
}

.achievement-date {
  text-align: center;
}

@keyframes trophy-unlock {
  0% {
    opacity: 0;
    transform: translateY(18px) scale(0.55) rotate(-8deg);
  }
  60% {
    transform: translateY(-5px) scale(1.12) rotate(3deg);
  }
  100% {
    opacity: 1;
    transform: none;
  }
}

@keyframes trophy-glow {
  0%,
  100% {
    filter: drop-shadow(0 4px 4px color-mix(in srgb, var(--lad-color-reward-deep) 22%, transparent));
  }
  55% {
    filter: drop-shadow(0 0 15px var(--lad-color-reward-highlight));
  }
}

@media (prefers-reduced-motion: reduce) {
  .trophy-button--unlocked,
  .trophy-button--unlocked .trophy {
    animation: none;
  }
}
</style>
