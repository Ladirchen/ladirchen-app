<template>
  <v-dialog content-class="energy-summary-dialog-frame" :model-value="modelValue" max-width="460" scrollable @update:model-value="emit('update:modelValue', $event)">
    <v-card class="energy-dialog" rounded="xl">
      <div class="energy-dialog-header pa-4">
        <div class="energy-title-row">
          <div>
            <p class="dialog-kicker">{{ t('world.energy.eyebrow') }}</p>
            <h2>{{ t('world.energy.title') }}</h2>
            <p class="energy-subtitle mt-1">{{ t('world.energy.description') }}</p>
          </div>
          <AnimatedHouseEnergy class="energy-mascot" :size="86" />
          <button class="close-button" :aria-label="t('world.energy.close')" type="button" @click="close"><v-icon icon="mdi-close" /></button>
        </div>

        <div class="summary-grid mt-4">
          <div class="summary-tile summary-tile--energy" :class="{ 'is-achieved': store.houseMeetsMinimumEnergy }">
            <div class="energy-orb" :style="{ '--energy': `${store.familyEnergy * 3.6}deg` }">
              <span>{{ store.familyEnergy }}<small>%</small></span>
            </div>
            <div>
              <span>{{ t('world.energy.shared') }}</span>
              <strong class="energy-state-title">{{ energyState.title }}<span v-if="store.houseMeetsMinimumEnergy" class="energy-state-spark" aria-hidden="true">✦</span></strong>
              <small>{{ energyState.copy }}</small>
            </div>
          </div>
          <div class="summary-tile summary-tile--goal">
            <div class="summary-icon" :class="{ 'is-achieved': store.houseMeetsMinimumEnergy }" aria-hidden="true">
              <AnimatedEnergyStar v-if="store.houseMeetsMinimumEnergy" :size="38" />
              <v-icon v-else size="25">mdi-progress-clock</v-icon>
            </div>
            <div>
              <span>{{ t('world.energy.dailyGoal') }}</span>
              <strong>{{ store.houseMeetsMinimumEnergy ? t('world.energy.reached') : t('world.energy.remaining', { value: 60 - store.familyEnergy }) }}</strong>
              <small>{{ t('world.energy.threshold') }}</small>
            </div>
          </div>
        </div>
      </div>

      <v-card-text class="energy-content pa-4">
        <HouseProgressPanel
          class="mb-5"
          :completed-weeks="store.completedWeeklyStreak"
          :house-level="store.houseLevel"
          :show-evolution="false"
        />

        <div class="children-heading mb-3">
          <div><span class="section-kicker">{{ t('world.energy.collected') }}</span><h3>{{ t('world.energy.breakdown') }}</h3></div>
          <span class="average-label">{{ t('world.energy.average') }}</span>
        </div>
        <div class="child-energy-list">
          <div v-for="(child, childIndex) in children" :key="child.id" class="child-energy-row" :style="{ '--avatar-color': child.color }">
            <div class="child-avatar">
              <AvatarFigure :appearance="childAppearance(child, childIndex)" calm :size="54" />
            </div>
            <div class="child-energy-copy">
              <div class="child-energy-title"><strong>{{ child.name }}</strong><b>{{ store.contributionProgress(child.id) }} %</b></div>
              <v-progress-linear class="mt-2" color="primary" height="8" :model-value="store.contributionProgress(child.id)" rounded />
              <p class="child-energy-description">{{ t('world.energy.approved', { approved: approvedCount(child.id), total: baseCount(child.id) }) }}</p>
            </div>
          </div>
        </div>

      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import { createDefaultAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import type { FamilyMember, FamilyMemberId } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';

import AnimatedEnergyStar from './AnimatedEnergyStar.vue';
import AnimatedHouseEnergy from './AnimatedHouseEnergy.vue';
import HouseProgressPanel from './HouseProgressPanel.vue';

const props = defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>();
const store = useFamilyWorldStore();
const { t } = useI18n();
const guideStep = ref(-1);
let guideStartTimer: number | undefined;
const children = computed(() => store.members.filter((member) => member.role === 'child'));
const energyState = computed(() => {
  if (store.familyEnergy === 100) return { title: t('world.energy.states.full.title'), copy: t('world.energy.states.full.description') };
  if (store.familyEnergy >= 60) return { title: t('world.energy.states.good.title'), copy: t('world.energy.states.good.description') };
  return { title: t('world.energy.states.low.title'), copy: t('world.energy.states.low.description') };
});
const baseContributions = (childId: FamilyMemberId) => store.contributions.filter((item) => item.kind === 'basic' && item.assigneeId === childId);
const baseCount = (childId: FamilyMemberId) => baseContributions(childId).length;
const approvedCount = (childId: FamilyMemberId) => baseContributions(childId).filter((item) => item.status === 'approved').length;
const childAppearance = (child: FamilyMember, index: number): AvatarAppearance => {
  if (child.appearance) return child.appearance;
  const appearance = createDefaultAvatarAppearance();
  const variants: ReadonlyArray<Partial<AvatarAppearance>> = [
    { hair: 'ponytail', outfitColorId: 'outfit-blue' },
    { hair: 'short', hairColorId: 'hair-black', outfit: 'overalls', outfitColorId: 'outfit-gold' },
    { hair: 'curls', hairColorId: 'hair-brown', outfit: 'space', outfitColorId: 'outfit-ocean' },
  ];
  return { ...appearance, ...(variants[index % variants.length] ?? {}) };
};
const guideSteps = computed(() => [
  {
    heading: t('world.energy.guide.calculation.title'),
    message: t('world.energy.guide.calculation.message', { count: Math.max(1, children.value.length) }),
  },
  {
    heading: t('world.energy.guide.day.title'),
    message: t('world.energy.guide.day.message'),
  },
  {
    heading: t('world.energy.guide.growth.title'),
    message: t('world.energy.guide.growth.message'),
  },
  {
    heading: t('world.energy.guide.safe.title'),
    message: t('world.energy.guide.safe.message'),
  },
]);
const startEnergyGuide = () => {
  guideStep.value = guideStep.value >= guideSteps.value.length - 1 ? 0 : guideStep.value + 1;
  const step = guideSteps.value[guideStep.value];
  if (!step) return;
  window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
    heading: step.heading,
    message: step.message,
    smart: true,
    progress: `${guideStep.value + 1} / ${guideSteps.value.length}`,
    actionLabel: t(guideStep.value === guideSteps.value.length - 1 ? 'world.energy.guide.again' : 'common.next'),
    actionEvent: 'house-energy-guide:next',
  } }));
};
watch(() => props.modelValue, (isOpen) => {
  if (guideStartTimer !== undefined) window.clearTimeout(guideStartTimer);
  if (!isOpen) return;
  guideStep.value = -1;
  guideStartTimer = window.setTimeout(() => {
    startEnergyGuide();
    guideStartTimer = undefined;
  }, 280);
});
onMounted(() => window.addEventListener('house-energy-guide:next', startEnergyGuide));
onUnmounted(() => {
  if (guideStartTimer !== undefined) window.clearTimeout(guideStartTimer);
  window.removeEventListener('house-energy-guide:next', startEnergyGuide);
});
const close = () => emit('update:modelValue', false);
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.energy-dialog {
  max-height: min(820px, 94dvh);
  @apply overflow-hidden;
  @include dialog-frame(
    color-mix(in srgb, var(--lad-palette-orange-400-2) 20%, transparent),
    color-mix(in srgb, var(--lad-palette-orange-650) 12%, transparent)
  );
}
.energy-dialog-header {
  @apply position-relative overflow-hidden;
  flex: 0 0 auto;
  border-bottom: 1px solid
    color-mix(in srgb, var(--lad-palette-orange-500) 15%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-palette-background) 0%,
    var(--lad-palette-amber-100) 48%,
    var(--lad-palette-amber-100) 100%
  );
}
.energy-dialog-header::before,
.energy-dialog-header::after {
  content: "";
  @apply position-absolute;
  border-radius: 50%;
  @apply pointer-events-none;
}
.energy-dialog-header::before {
  width: 190px;
  height: 190px;
  top: -113px;
  right: -34px;
  background: color-mix(in srgb, var(--lad-palette-white) 35%, transparent);
  box-shadow: 0 0 0 22px
    color-mix(in srgb, var(--lad-palette-white) 15%, transparent);
}
.energy-dialog-header::after {
  width: 105px;
  height: 35px;
  right: 76px;
  bottom: -21px;
  background: color-mix(in srgb, var(--lad-palette-white) 30%, transparent);
  filter: blur(2px);
}
.energy-title-row {
  min-height: 108px;
  @apply position-relative;
  z-index: 1;
  @apply d-flex align-start;
}
.energy-title-row > div:first-child {
  max-width: 280px;
}
.energy-dialog-header h2 {
  @apply ma-0;
  @include heading(var(--lad-font-size-page), 1.1, -0.04em);
}
.energy-subtitle {
  max-width: 260px;
  @include body-copy(var(--lad-font-size-body));
}
.close-button {
  @apply position-absolute;
  top: -7px;
  right: -8px;
  z-index: 4;
}
.energy-mascot {
  @apply position-absolute;
  top: 10px;
  right: 19px;
}
.summary-grid {
  @apply position-relative;
  z-index: 1;
  @apply d-grid;
  grid-template-columns: 1.2fr 0.8fr;
  gap: 10px;
}
.summary-tile {
  @apply min-w-0;
  padding: 11px;
  @apply d-flex align-center;
  gap: 10px;
  @include raised-surface(
    color-mix(in srgb, var(--lad-palette-orange-650) 10%, transparent),
    color-mix(in srgb, var(--lad-palette-orange-600) 8%, transparent),
    var(--lad-radius-medium),
    0.3125rem
  );
  background: color-mix(in srgb, var(--lad-palette-white) 75%, transparent);
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-palette-orange-600) 8%, transparent),
    inset 0 1px 0 color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
}
.summary-tile--energy {
  @apply position-relative overflow-hidden;
}
.summary-tile--energy::after {
  width: 48px;
  height: 130%;
  content: "";
  @apply position-absolute pointer-events-none;
  top: -15%;
  left: -70px;
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent),
    transparent
  );
  transform: skewX(-18deg);
}
.summary-tile--energy.is-achieved::after {
  animation: energy-card-shimmer 4s 1s ease-in-out infinite;
}
.summary-tile > div:last-child {
  @apply min-w-0;
}
.summary-tile span,
.summary-tile strong,
.summary-tile small {
  @apply d-block;
}
.summary-tile span {
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.summary-tile strong {
  margin-top: 1px;
  font-size: 0.9375rem;
  line-height: 1.2;
}
.summary-tile small {
  margin-top: 3px;
  @apply overflow-hidden;
  color: var(--lad-palette-muted-600);
  font-size: 0.5rem;
  line-height: 1.3;
}
.energy-state-title {
  @apply d-flex align-center;
  gap: 4px;
}
.energy-state-spark {
  color: var(--lad-palette-amber-450) !important;
  font-size: 0.75rem !important;
  animation: energy-state-spark 2.4s ease-in-out infinite;
}
.energy-orb {
  width: 60px;
  height: 60px;
  @apply d-grid place-center;
  flex: 0 0 60px;
  border-radius: 50%;
  background: conic-gradient(
    var(--lad-mint) var(--energy),
    color-mix(in srgb, var(--lad-palette-teal-550) 12%, transparent) 0
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-mint-strong) 12%, transparent),
    0 0 0 5px color-mix(in srgb, var(--lad-palette-white) 50%, transparent);
  animation: energy-orb-breathe 2.9s ease-in-out infinite;
}
.energy-orb::before {
  width: 46px;
  height: 46px;
  content: "";
  grid-area: 1 / 1;
  border-radius: 50%;
  background: var(--lad-palette-surface);
}
.energy-orb span {
  z-index: 1;
  grid-area: 1 / 1;
  color: var(--lad-mint-dark);
  font-size: 1.25rem;
  font-weight: var(--lad-font-weight-black);
}
.energy-orb small {
  @apply d-inline;
  font-size: 0.5625rem;
}
.summary-icon {
  @include icon-tile(
    2.75rem,
    0.875rem,
    var(--lad-palette-amber-150),
    color-mix(in srgb, var(--lad-palette-amber-600) 12%, transparent),
    -4deg
  );
  flex-basis: 44px;
  color: var(--lad-palette-amber-600);
}
.summary-icon.is-achieved {
  background: linear-gradient(
    145deg,
    var(--lad-palette-amber-100),
    var(--lad-palette-amber-150)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-amber-600) 12%, transparent);
}
.energy-content {
  min-height: 0;
  flex: 1 1 auto;
  @apply overflow-y-auto;
  background: linear-gradient(
    180deg,
    var(--lad-palette-surface),
    var(--lad-palette-background)
  );
}
.section-kicker {
  @include overline(var(--lad-palette-mint-strong), var(--lad-font-size-micro));
}
.children-heading {
  @apply d-flex align-end justify-space-between ga-3;
}
.energy-dialog h3 {
  margin: 1px 0 0;
  font-size: 1.0625rem;
}
.average-label {
  color: var(--lad-muted);
  font-size: 0.5625rem;
  font-weight: 800;
}
.child-energy-list {
  @apply d-flex flex-column;
  gap: 10px;
}
.child-energy-row {
  min-height: 78px;
  padding: 11px 12px;
  @apply d-flex align-center;
  gap: 12px;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 15%, transparent);
  border-radius: 18px;
  background: linear-gradient(
    135deg,
    color-mix(in srgb, var(--lad-palette-white) 95%, transparent),
    color-mix(
      in srgb,
      var(--avatar-color, var(--lad-palette-indigo-350)) 7%,
      white
    )
  );
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent),
    0 10px 20px color-mix(in srgb, var(--lad-palette-muted-700) 5%, transparent);
  transition:
    transform 0.2s ease,
    border-color 0.2s ease;
}
.child-energy-row:hover {
  border-color: color-mix(in srgb, var(--lad-palette-mint) 35%, transparent);
  transform: translateY(-2px);
}
.child-avatar {
  width: 58px;
  height: 58px;
  @apply d-grid place-center overflow-hidden;
  flex: 0 0 58px;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  border-radius: 19px;
  background: color-mix(
    in srgb,
    var(--avatar-color, var(--lad-palette-indigo-350)) 18%,
    white
  );
  box-shadow: 0 4px 0
    color-mix(
      in srgb,
      var(--avatar-color, var(--lad-palette-indigo-350)) 18%,
      transparent
    );
  transform: rotate(-2deg);
}
.child-energy-copy {
  @apply min-w-0;
  flex: 1;
}
.child-energy-title {
  @apply d-flex align-center justify-space-between ga-2;
}
.child-energy-row strong {
  font-size: 0.8125rem;
}
.child-energy-row b {
  padding: 4px 8px;
  color: var(--lad-mint-dark);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-palette-background);
  font-size: 0.6875rem;
}
.child-energy-description {
  @apply ma-0 mt-2;
  color: var(--lad-palette-teal-600);
  font-size: 0.625rem;
  font-weight: 650;
  line-height: 1.35;
}
.child-energy-description span {
  @apply d-inline;
  color: var(--lad-palette-mint-strong);
  font-weight: var(--lad-font-weight-heavy);
}
@keyframes energy-orb-breathe {
  0%,
  100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.035);
  }
}
@keyframes energy-card-shimmer {
  0%,
  35% {
    left: -70px;
  }
  68%,
  100% {
    left: 120%;
  }
}
@keyframes energy-state-spark {
  0%,
  30%,
  100% {
    opacity: 0.42;
    transform: rotate(0) scale(0.72);
  }
  48%,
  62% {
    opacity: 1;
    transform: rotate(25deg) scale(1.2);
  }
}
.energy-dialog {
  height: min(660px, calc(100dvh - 28px));
  max-height: min(660px, calc(100dvh - 28px));
  @apply d-flex flex-column;
  @include dialog-frame;
}
.energy-dialog-header {
  border: 0;
  background: var(--lad-palette-surface);
}
.energy-dialog-header::before,
.energy-dialog-header::after {
  display: none;
}
.energy-title-row {
  min-height: 126px;
  padding: 18px;
  @apply align-center;
  @include dialog-title-panel;
}
.energy-title-row::before,
.energy-title-row::after {
  content: "";
  @apply position-absolute pointer-events-none;
  border-radius: 50%;
}
.energy-title-row::before {
  width: 120px;
  height: 120px;
  top: -72px;
  right: -28px;
  background: color-mix(in srgb, var(--lad-palette-blue-150) 60%, transparent);
  box-shadow: 0 0 0 17px
    color-mix(in srgb, var(--lad-palette-background) 40%, transparent);
}
.energy-title-row::after {
  width: 84px;
  height: 26px;
  right: 44px;
  bottom: -16px;
  background: color-mix(in srgb, var(--lad-palette-blue-150) 60%, transparent);
}
.energy-title-row > div:first-child {
  max-width: 245px;
  @apply position-relative;
  z-index: 1;
}
.dialog-kicker {
  margin: 0 0 5px;
  @include overline(var(--lad-blue), 0.5625rem);
}
.energy-dialog-header h2 {
  font-size: 1.5625rem;
}
.energy-subtitle {
  max-width: 225px;
  font-size: 0.6875rem;
}
.energy-mascot {
  width: 78px !important;
  height: 78px !important;
  @apply d-grid place-center;
  top: 24px;
  right: 45px;
  z-index: 2;
  border-radius: 24px;
  background: color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-blue) 12%, transparent);
}
.close-button {
  top: 8px;
  right: 8px;
  @include dialog-close-button;
}
.summary-grid {
  grid-template-columns: 1fr 1fr;
  gap: 9px;
}
.summary-tile {
  min-height: 72px;
  padding: 8px;
  gap: 8px;
  border-color: color-mix(in srgb, var(--lad-palette-blue) 18%, transparent);
  background:
    radial-gradient(
      circle at 90% 8%,
      color-mix(in srgb, var(--lad-palette-yellow) 20%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-background) 62%,
      var(--lad-palette-amber-100)
    );
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-palette-blue-strong) 12%, transparent);
}
.summary-tile--goal {
  border-color: color-mix(
    in srgb,
    var(--lad-palette-purple-350) 18%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-palette-surface),
    var(--lad-palette-background) 58%,
    var(--lad-palette-amber-100)
  );
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-palette-muted-500) 10%, transparent);
}
.energy-orb {
  width: 48px;
  height: 48px;
  flex-basis: 48px;
}
.energy-orb::before {
  width: 37px;
  height: 37px;
}
.energy-orb span {
  font-size: 1rem;
}
.summary-icon {
  width: 40px;
  height: 40px;
  flex-basis: 40px;
  color: var(--lad-palette-white);
  border: 3px solid var(--lad-palette-white);
  background: linear-gradient(
    145deg,
    var(--lad-palette-pink-300),
    var(--lad-palette-violet-400)
  );
  box-shadow: 0 4px 0 var(--lad-palette-violet-500);
}
.summary-icon.is-achieved {
  background: linear-gradient(
    145deg,
    var(--lad-palette-pink-300),
    var(--lad-palette-violet-400)
  );
  box-shadow: 0 4px 0 var(--lad-palette-violet-500);
}
.energy-content {
  background: linear-gradient(
    180deg,
    var(--lad-palette-surface),
    var(--lad-palette-background)
  );
}
.energy-content :deep(.house-progress-panel) {
  border-color: color-mix(in srgb, var(--lad-palette-blue) 15%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow: 0 7px 0
    color-mix(in srgb, var(--lad-palette-blue) 10%, transparent);
}
:global(.energy-summary-dialog-frame) {
  width: min(460px, calc(100vw - 32px));
  height: min(660px, calc(100dvh - 32px));
  max-height: calc(100dvh - 32px);
}
@include respond-down(mobile) {
  .summary-grid {
    grid-template-columns: 1fr;
  }
  .energy-title-row > div:first-child {
    max-width: 235px;
  }
  .energy-mascot {
    right: 12px;
    transform: scale(0.9);
    transform-origin: right top;
  }
}
@include reduced-motion {
  .energy-orb,
  .summary-tile--energy::after,
  .energy-state-spark {
    animation: none;
  }
  .child-energy-row {
    transition: none;
  }
}
</style>
