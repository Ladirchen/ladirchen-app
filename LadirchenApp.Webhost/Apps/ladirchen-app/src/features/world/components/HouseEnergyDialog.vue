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
          <button class="close-button" :aria-label="t('world.energy.close')" type="button" @click="close"><v-icon icon="i-mdi:close" /></button>
        </div>

        <div class="summary-grid mt-4">
          <MetricCard class="summary-tile summary-tile--energy" :class="{ 'is-achieved': store.houseMeetsMinimumEnergy }" tone="energy">
            <div class="energy-orb" :style="{ '--energy': `${store.familyEnergy * 3.6}deg` }">
              <span>{{ store.familyEnergy }}<small>%</small></span>
            </div>
            <div>
              <span>{{ t('world.energy.shared') }}</span>
              <strong class="energy-state-title">{{ energyState.title }}<span v-if="store.houseMeetsMinimumEnergy" class="energy-state-spark" aria-hidden="true">✦</span></strong>
              <small>{{ energyState.copy }}</small>
            </div>
          </MetricCard>
          <MetricCard class="summary-tile summary-tile--goal" tone="bonus">
            <div class="summary-icon" :class="{ 'is-achieved': store.houseMeetsMinimumEnergy }" aria-hidden="true">
              <AnimatedEnergyStar v-if="store.houseMeetsMinimumEnergy" :size="38" />
              <v-icon v-else size="25">i-mdi:progress-clock</v-icon>
            </div>
            <div>
              <span>{{ t('world.energy.dailyGoal') }}</span>
              <strong>{{ store.houseMeetsMinimumEnergy ? t('world.energy.reached') : t('world.energy.remaining', { value: 60 - store.familyEnergy }) }}</strong>
              <small>{{ t('world.energy.threshold') }}</small>
            </div>
          </MetricCard>
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
import { ladiGuideController } from '@/shared/services/ladi-guide-controller';

import AvatarFigure from '@/shared/components/avatar/AvatarFigure.vue';
import { resolveFamilyMemberAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import type { FamilyMember } from '@/domain/family/types';
import type { FamilyMemberId } from '@/domain/shared/identifiers';
import { useFamilyWorldStore } from '@/stores/family-world';
import MetricCard from '@/shared/components/ui/MetricCard.vue';

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
const childAppearance = (child: FamilyMember, _index: number): AvatarAppearance => {
  return resolveFamilyMemberAvatarAppearance(child, store.members);
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
  ladiGuideController.say({
    heading: step.heading,
    message: step.message,
    smart: true,
    progress: `${guideStep.value + 1} / ${guideSteps.value.length}`,
    actionLabel: t(guideStep.value === guideSteps.value.length - 1 ? 'world.energy.guide.again' : 'common.next'),
    actionId: 'house-energy:next',
  });
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
let unregisterGuideAction: (() => void) | undefined;
onMounted(() => { unregisterGuideAction = ladiGuideController.registerAction('house-energy:next', startEnergyGuide); });
onUnmounted(() => {
  if (guideStartTimer !== undefined) window.clearTimeout(guideStartTimer);
  unregisterGuideAction?.();
});
const close = () => emit('update:modelValue', false);
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.energy-dialog {
  max-height: min(820px, 94dvh);
  @apply overflow-hidden;
  @include dialog-frame(
    color-mix(in srgb, var(--lad-color-accent-warm-muted) 20%, transparent),
    color-mix(in srgb, var(--lad-color-accent-warm-deep) 12%, transparent)
  );
}
.energy-dialog-header {
  @apply position-relative overflow-hidden;
  flex: 0 0 auto;
  border-bottom: 1px solid
    color-mix(in srgb, var(--lad-color-accent-warm-supporting) 15%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft) 0%,
    var(--lad-color-reward-soft) 48%,
    var(--lad-color-reward-soft) 100%
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
  background: color-mix(in srgb, var(--lad-surface-raised) 35%, transparent);
  box-shadow: 0 0 0 22px
    color-mix(in srgb, var(--lad-surface-raised) 15%, transparent);
}
.energy-dialog-header::after {
  width: 105px;
  height: 35px;
  right: 76px;
  bottom: -21px;
  background: color-mix(in srgb, var(--lad-surface-raised) 30%, transparent);
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
  gap: 10px;
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
    color-mix(in srgb, var(--lad-surface-raised) 80%, transparent),
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
  font-size: rem(9);
}
.summary-tile strong {
  margin-top: 1px;
  font-size: rem(15);
  line-height: 1.2;
}
.summary-tile small {
  margin-top: 3px;
  @apply overflow-hidden;
  color: var(--lad-text-supporting);
  font-size: 0.5rem;
  line-height: 1.3;
}
.energy-state-title {
  @apply d-flex align-center;
  gap: 4px;
}
.energy-state-spark {
  color: var(--lad-color-reward-border);
  font-size: 0.75rem;
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
    color-mix(in srgb, var(--lad-color-primary-muted) 12%, transparent) 0
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-color-primary-strong) 12%, transparent),
    0 0 0 5px color-mix(in srgb, var(--lad-surface-raised) 50%, transparent);
  animation: energy-orb-breathe 2.9s ease-in-out infinite;
}
.energy-orb::before {
  width: 46px;
  height: 46px;
  content: "";
  grid-area: 1 / 1;
  border-radius: 50%;
  background: var(--lad-surface);
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
  font-size: rem(9);
}
.summary-icon {
  @include icon-tile(
    2.75rem,
    rem(14),
    var(--lad-color-reward-pale),
    color-mix(in srgb, var(--lad-color-reward-deep) 12%, transparent),
    -4deg
  );
  flex-basis: 44px;
  color: var(--lad-color-reward-deep);
}
.summary-icon.is-achieved {
  background: linear-gradient(
    145deg,
    var(--lad-color-reward-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-reward-deep) 12%, transparent);
}
.energy-content {
  min-height: 0;
  flex: 1 1 auto;
  @apply overflow-y-auto;
  background: linear-gradient(
    180deg,
    var(--lad-surface),
    var(--lad-surface-soft)
  );
}
.section-kicker {
  @include overline(
    var(--lad-color-primary-strong),
    var(--lad-font-size-micro)
  );
}
.children-heading {
  @apply d-flex align-end justify-space-between ga-3;
}
.energy-dialog h3 {
  margin: 1px 0 0;
  font-size: rem(17);
}
.average-label {
  color: var(--lad-muted);
  font-size: rem(9);
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
    color-mix(in srgb, var(--lad-color-primary-muted) 15%, transparent);
  border-radius: 18px;
  background: linear-gradient(
    135deg,
    color-mix(in srgb, var(--lad-surface-raised) 95%, transparent),
    color-mix(
      in srgb,
      var(--avatar-color, var(--lad-color-bonus-info)) 7%,
      var(--lad-surface-raised)
    )
  );
  box-shadow:
    0 5px 0
      color-mix(in srgb, var(--lad-color-primary-supporting) 8%, transparent),
    0 10px 20px color-mix(in srgb, var(--lad-text-strong) 5%, transparent);
  transition:
    transform 0.2s ease,
    border-color 0.2s ease;
}
.child-energy-row:hover {
  border-color: color-mix(in srgb, var(--lad-color-primary) 35%, transparent);
  transform: translateY(-2px);
}
.child-avatar {
  width: 58px;
  height: 58px;
  @apply d-grid place-center overflow-hidden;
  flex: 0 0 58px;
  border: 2px solid
    color-mix(in srgb, var(--lad-border-on-accent) 90%, transparent);
  border-radius: 19px;
  background: color-mix(
    in srgb,
    var(--avatar-color, var(--lad-color-bonus-info)) 18%,
    var(--lad-surface-raised)
  );
  box-shadow: 0 4px 0
    color-mix(
      in srgb,
      var(--avatar-color, var(--lad-color-bonus-info)) 18%,
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
  font-size: rem(13);
}
.child-energy-row b {
  padding: 4px 8px;
  color: var(--lad-mint-dark);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-surface-soft);
  font-size: rem(11);
}
.child-energy-description {
  @apply ma-0 mt-2;
  color: var(--lad-color-primary-supporting);
  font-size: rem(10);
  font-weight: 650;
  line-height: 1.35;
}
.child-energy-description span {
  @apply d-inline;
  color: var(--lad-color-primary-strong);
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
  background: var(--lad-surface);
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
  background: color-mix(in srgb, var(--lad-color-info-soft) 60%, transparent);
  box-shadow: 0 0 0 17px
    color-mix(in srgb, var(--lad-surface-soft) 40%, transparent);
}
.energy-title-row::after {
  width: 84px;
  height: 26px;
  right: 44px;
  bottom: -16px;
  background: color-mix(in srgb, var(--lad-color-info-soft) 60%, transparent);
}
.energy-title-row > div:first-child {
  max-width: 245px;
  @apply position-relative;
  z-index: 1;
}
.dialog-kicker {
  margin: 0 0 5px;
  @include overline(var(--lad-blue), rem(9));
}
.energy-dialog-header h2 {
  font-size: rem(25);
}
.energy-subtitle {
  max-width: 225px;
  font-size: rem(11);
}
.energy-mascot {
  width: 78px;
  height: 78px;
  @apply d-grid place-center;
  top: 24px;
  right: 45px;
  z-index: 2;
  border-radius: 24px;
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-color-info) 12%, transparent);
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
  color: var(--lad-text-inverse);
  border: 3px solid var(--lad-border-on-accent);
  background: linear-gradient(
    145deg,
    var(--lad-color-accent-pink),
    var(--lad-color-bonus)
  );
  box-shadow: 0 4px 0 var(--lad-color-bonus-muted);
}
.summary-icon.is-achieved {
  background: linear-gradient(
    145deg,
    var(--lad-color-accent-pink),
    var(--lad-color-bonus)
  );
  box-shadow: 0 4px 0 var(--lad-color-bonus-muted);
}
.energy-content {
  background: linear-gradient(
    180deg,
    var(--lad-surface),
    var(--lad-surface-soft)
  );
}
.energy-content :deep(.house-progress-panel) {
  border-color: color-mix(in srgb, var(--lad-color-info) 15%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow: 0 7px 0 color-mix(in srgb, var(--lad-color-info) 10%, transparent);
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
