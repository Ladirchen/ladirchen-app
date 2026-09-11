<template>
  <div class="page page-padding wishes-page">
    <PageViewSwitch v-model="activeTab" class="mb-5" :label="t('wishes.viewLabel')" :options="wishViewOptions" />

    <template v-if="activeTab !== 'family'">
      <button v-if="canCreatePersonalGoal" class="create-goal-card mb-5" type="button" @click="openGoalDialog(personalGoalOwnerId)">
        <span class="create-goal-icon"><v-icon icon="i-mdi:plus" /></span>
        <span><strong>{{ t('wishes.create.title') }}</strong><small>{{ t('wishes.create.description') }}</small></span>
        <v-icon class="create-goal-arrow" icon="i-mdi:arrow-right" />
      </button>

      <SavingsInterestGuideCard v-if="store.viewerRole === 'child'" />

      <SectionHeader
        :description="t(activeTab === 'children' ? 'wishes.sections.childrenDescription' : 'wishes.sections.ownDescription')"
        :title="t(activeTab === 'children' ? 'wishes.sections.childrenTitle' : 'wishes.sections.ownTitle')"
      />

      <TransitionGroup class="goal-grid" name="goal-list" tag="div">
        <BrandedCard v-for="goal in personalGoals" :key="goal.id" class="family-goal pa-3" tone="wishes">
          <div class="d-flex align-start ga-3">
            <div class="goal-icon">{{ goal.icon }}</div>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center justify-space-between ga-2">
                <div><strong>{{ goal.title }}</strong><p v-if="goal.ownerId !== store.signedInMemberId" class="text-caption text-medium-emphasis">{{ ownerName(goal.ownerId) }}</p></div>
                <v-chip v-if="goal.ownerId !== store.signedInMemberId" color="primary" size="x-small" variant="tonal">{{ goal.shared ? t('wishes.shared') : visibilityLabel(goal.visibility) }}</v-chip>
              </div>
              <div class="goal-account-stats mt-3">
                <MetricCard compact tone="info"><b class="goal-stat-icon"><v-icon icon="i-mdi:wallet-plus-outline" /></b><i><small>{{ t('wishes.stats.deposited') }}</small><strong><LadirchenAmount compact :value="depositedAmount(goal)" /></strong></i></MetricCard>
                <MetricCard compact tone="energy"><b class="goal-stat-icon"><v-icon icon="i-mdi:chart-line" /></b><i><small>{{ t('wishes.stats.interest') }}</small><strong>{{ formatInterestRate(store.savingsInterestRate) }} %</strong></i></MetricCard>
                <MetricCard compact tone="reward"><b class="goal-stat-icon"><v-icon icon="i-mdi:calendar-star" /></b><i><small>{{ t('wishes.stats.nextWeek') }}</small><strong><LadirchenAmount compact prefix="+" :value="weeklyInterestForGoal(goal)" /></strong></i></MetricCard>
                <MetricCard compact tone="bonus"><b class="goal-stat-icon"><v-icon icon="i-mdi:star-four-points" /></b><i><small>{{ t('wishes.stats.earned') }}</small><strong><LadirchenAmount compact prefix="+" :value="goal.interestEarned ?? 0" /></strong></i></MetricCard>
              </div>
              <v-progress-linear class="mt-3" color="primary" height="8" :model-value="progress(goal.saved, goal.target)" rounded />
              <div class="d-flex align-center justify-space-between mt-2">
                <span class="goal-total"><small>{{ t('wishes.stats.balance') }}</small><strong><LadirchenAmount :value="`${goal.saved} / ${goal.target}`" /></strong></span>
                <v-btn v-if="goal.ownerId === store.signedInMemberId && store.viewerRole === 'child'" class="assign-button" color="info" rounded="lg" size="small" variant="tonal" @click="openSave(goal.id)">
                  <span class="assign-coin" aria-hidden="true"><LadirchenCoin small /></span>
                  <span>{{ t('wishes.assign') }}</span>
                  <i class="assign-spark" aria-hidden="true">✦</i>
                </v-btn>
                <v-btn v-else-if="goal.ownerId !== store.signedInMemberId" color="info" prepend-icon="i-mdi:gift-outline" rounded="lg" size="small" variant="tonal" @click="openSupport(goal.id)">{{ t('wishes.gift') }}</v-btn>
              </div>
            </div>
          </div>
        </BrandedCard>
      </TransitionGroup>
    </template>

    <template v-else>
      <button class="create-goal-card create-family-goal-card mb-5" type="button" @click="openGoalDialog('family')">
        <span class="create-goal-icon"><v-icon icon="i-mdi:account-group-outline" /></span>
        <span><strong>{{ t('wishes.createFamily.title') }}</strong><small>{{ t('wishes.createFamily.description') }}</small></span>
        <v-icon class="create-goal-arrow" icon="i-mdi:plus" />
      </button>
      <TransitionGroup class="goal-grid" name="goal-list" tag="div">
        <BrandedCard v-for="goal in visibleFamilyGoals" :key="goal.id" class="family-goal pa-3" tone="wishes">
          <div class="d-flex align-start ga-3">
            <div class="goal-icon">{{ goal.icon }}</div>
            <div class="flex-grow-1 min-w-0">
              <div class="d-flex align-center justify-space-between ga-2">
                <div><strong>{{ goal.title }}</strong><p class="text-caption text-medium-emphasis">{{ ownerName(goal.ownerId) }}</p></div>
                <v-chip v-if="goal.shared" color="primary" size="x-small" variant="tonal">{{ t('wishes.shared') }}</v-chip>
              </div>
              <v-progress-linear class="mt-3" color="primary" height="8" :model-value="progress(goal.saved, goal.target)" rounded />
              <div class="d-flex align-center justify-space-between flex-wrap ga-2 mt-2">
                <LadirchenAmount class="text-caption font-weight-bold" :value="`${goal.saved} / ${goal.target}`" />
                <div class="family-goal-actions">
                  <button
                    class="cheer-button"
                    :class="{ 'is-cheered': goal.cheered }"
                    :aria-label="t('wishes.cheerAria', { title: goal.title })"
                    :aria-pressed="goal.cheered"
                    type="button"
                    @click="store.toggleCheer(goal.id)"
                  >
                    <span class="cheer-hands" aria-hidden="true"><i>🤚</i><i>✋</i></span>
                    <span class="cheer-spark cheer-spark--one" aria-hidden="true">✦</span>
                    <span class="cheer-spark cheer-spark--two" aria-hidden="true">✧</span>
                  </button>
                  <v-btn
                    v-if="goal.saved < goal.target && (store.viewerRole === 'child' || store.permissions.canSupportChildGoals)"
                    class="family-gift-button assign-button"
                    color="info"
                    rounded="lg"
                    size="small"
                    variant="tonal"
                    @click="openSupport(goal.id)"
                  ><span class="assign-coin" aria-hidden="true"><LadirchenCoin small /></span><span>{{ t('wishes.giftCoins') }}</span><i class="assign-spark" aria-hidden="true">✦</i></v-btn>
                </div>
              </div>
            </div>
          </div>
        </BrandedCard>
      </TransitionGroup>
    </template>

    <v-dialog v-model="saveDialog" max-width="420">
      <v-card class="save-dialog-card" :class="{ 'is-saving': saveMotion }" rounded="xl">
        <header class="save-dialog-header">
          <div class="save-goal-icon" aria-hidden="true">{{ activeGoal.icon }}</div>
          <div class="flex-grow-1 min-w-0">
            <p class="eyebrow mb-1">{{ t('wishes.save.title') }}</p>
            <h2>{{ activeGoal.title }}</h2>
            <span>{{ t('wishes.save.progress', { saved: activeGoal.saved, target: activeGoal.target }) }}</span>
          </div>
          <v-btn :aria-label="t('wishes.save.close')" icon="i-mdi:close" size="small" variant="text" @click="saveDialog = false" />
        </header>

        <div class="save-dialog-content">
          <div class="save-balance">
            <LadirchenCoin class="save-balance-coin" small />
            <div><small>{{ t('wishes.save.available') }}</small><strong><LadirchenAmount :value="store.availableBalance" /></strong></div>
            <span class="save-journey" aria-hidden="true"><i /><i /><i /></span>
            <span class="save-goal-star" aria-hidden="true">★</span>
          </div>

          <v-slider v-model="saveAmount" class="save-slider mt-5" color="info" :disabled="maxAssignable <= 0" hide-details :max="Math.max(1, maxAssignable)" min="0" step="5" thumb-label />
          <div class="save-value"><LadirchenCoin small /><strong>{{ saveAmount }}</strong><small>{{ t('wishes.save.currency') }}</small></div>
          <p v-if="maxAssignable <= 0" class="save-empty-note">{{ t('wishes.save.empty') }}</p>

          <button class="save-submit" :disabled="saveAmount <= 0 || saveMotion" type="button" @click="saveToGoal">
            <LadirchenCoin class="save-submit-coin" small />
            <strong>{{ t(saveMotion ? 'wishes.save.sending' : 'wishes.save.submit') }}</strong>
            <span aria-hidden="true">→</span>
            <i aria-hidden="true">✦</i>
          </button>
        </div>
      </v-card>
    </v-dialog>

    <SavingGoalDialog v-model="goalDialog" @submit="addGoal" />

    <v-dialog v-model="supportDialog" max-width="420" :persistent="supportSending">
      <v-card v-if="supportGoal" class="support-dialog-card pa-5" :class="{ 'is-sending': supportSending }" rounded="xl">
        <div class="support-dialog-heading">
          <div class="support-icon">🎁</div>
          <div>
            <p class="eyebrow mb-1">{{ t('wishes.support.eyebrow') }}</p>
            <h2>{{ supportGoal.title }}</h2>
            <span>{{ t('wishes.support.description', { name: ownerName(supportGoal.ownerId) }) }}</span>
          </div>
        </div>
        <div class="support-journey mt-4" aria-hidden="true">
          <LadirchenCoin class="support-coin" small /><i /><i /><i /><span class="support-high-five">✋</span><span class="support-present">🎁</span>
        </div>
        <v-slider v-model="supportAmount" color="info" :disabled="supportMaximum <= 0" :max="Math.max(1, supportMaximum)" min="1" step="1" thumb-label />
        <div class="save-value text-center mb-2"><LadirchenAmount :value="supportAmount" /></div>
        <p class="text-caption text-medium-emphasis text-center mb-4">{{ supportExplanation }}</p>
        <div class="d-grid dialog-actions ga-2">
          <v-btn :disabled="supportSending" rounded="lg" variant="text" @click="supportDialog = false">{{ t('wishes.back') }}</v-btn>
          <v-btn class="support-submit" color="info" :disabled="supportMaximum <= 0 || supportAmount <= 0 || supportSending" rounded="lg" variant="flat" @click="giveSupport">
            <span aria-hidden="true">✋</span>{{ t(supportSending ? 'wishes.support.sending' : 'wishes.support.submit') }}<span aria-hidden="true">🎁</span>
          </v-btn>
        </div>
      </v-card>
    </v-dialog>
  </div>
</template>

<script lang="ts" setup>
import SavingGoalDialog from '../components/SavingGoalDialog.vue';
import SavingsInterestGuideCard from '../components/SavingsInterestGuideCard.vue';
import { useWishesPage } from '../composables/use-wishes-page';
import PageViewSwitch from '@/shared/components/ui/PageViewSwitch.vue';
import SectionHeader from '@/shared/components/ui/SectionHeader.vue';
import BrandedCard from '@/shared/components/ui/BrandedCard.vue';
import MetricCard from '@/shared/components/ui/MetricCard.vue';
import LadirchenAmount from '@/shared/components/LadirchenAmount.vue';
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const {
  activeGoal, activeTab, addGoal, canCreatePersonalGoal, depositedAmount, formatInterestRate, giveSupport,
  goalDialog, maxAssignable, openGoalDialog, openSave, openSupport, ownerName, personalGoalOwnerId,
  personalGoals, progress, saveAmount, saveDialog, saveMotion, saveToGoal, store, supportAmount,
  supportDialog, supportExplanation, supportGoal, supportMaximum, supportSending, visibilityLabel,
  visibleFamilyGoals, weeklyInterestForGoal, wishViewOptions,
} = useWishesPage();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.create-goal-card {
  width: 100%;
  min-height: 66px;
  padding: 9px 12px;
  @apply d-flex align-center text-left cursor-pointer;
  gap: 10px;
  color: var(--lad-text);
  border: 2px solid color-mix(in srgb, var(--lad-color-info) 25%, transparent);
  border-radius: 20px;
  background:
    radial-gradient(
      circle at 91% 10%,
      color-mix(in srgb, var(--lad-color-reward-highlight) 25%, transparent),
      transparent 29%
    ),
    linear-gradient(
      145deg,
      var(--lad-surface-soft),
      var(--lad-color-reward-soft)
    );
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--lad-color-info-strong) 15%, transparent),
    0 10px 19px color-mix(in srgb, var(--lad-color-info-deep) 8%, transparent);
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.create-goal-card:hover {
  transform: translateY(-2px);
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-color-info-strong) 15%, transparent),
    0 13px 22px color-mix(in srgb, var(--lad-color-info-deep) 8%, transparent);
}
.create-goal-card:active {
  transform: translateY(3px);
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-color-info-strong) 15%, transparent);
}
.create-goal-icon {
  width: 43px;
  height: 43px;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-text-inverse);
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 15px;
  background: linear-gradient(
    145deg,
    var(--lad-color-info-subtle),
    var(--lad-color-info)
  );
  box-shadow: 0 4px 0 var(--lad-color-info-strong);
}
.create-goal-card > span:nth-child(2) {
  @apply flex-grow-1 min-w-0;
}
.create-goal-card strong,
.create-goal-card small {
  @apply d-block;
}
.create-goal-card strong {
  font-size: 0.9375rem;
}
.create-goal-card small {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.create-goal-arrow {
  color: var(--lad-blue-dark);
}
.goal-grid {
  @apply d-grid;
  gap: 11px;
}
.goal-account-stats {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 6px;
}
.goal-account-stats > .metric-card {
  min-height: 47px;
  @apply text-left;
}
.goal-account-stats i {
  @apply min-w-0;
  font-style: normal;
}
.goal-account-stats strong {
  color: var(--lad-color-primary-deep);
}
.goal-account-stats > .metric-card:nth-child(3) strong,
.goal-account-stats > .metric-card:nth-child(4) strong {
  color: var(--lad-color-reward-ink);
}
.goal-total small,
.goal-total strong {
  @apply d-block;
}
.goal-total small {
  color: var(--lad-muted);
  font-size: 0.4375rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.goal-total strong {
  margin-top: 1px;
  font-size: 0.8125rem;
}
.goal-stat-icon {
  color: var(--lad-color-info-deep);
  @include icon-tile(
    29px,
    9px,
    var(--lad-color-info-soft),
    color-mix(in srgb, var(--lad-color-info-strong) 18%, transparent),
    0deg,
    2px solid var(--lad-border-on-accent),
    2px
  );
  animation: account-stat-float 3s ease-in-out infinite;
}
.goal-stat-icon :deep(.v-icon) {
  width: 1.125rem;
  height: 1.125rem;
  @apply ma-auto;
  color: inherit;
  background-color: currentColor;
  opacity: 1;
}
.goal-account-stats > .metric-card:nth-child(2) .goal-stat-icon {
  color: var(--lad-color-primary-deep);
  background: var(--lad-color-primary-soft);
  animation-delay: -0.7s;
}
.goal-account-stats > .metric-card:nth-child(3) .goal-stat-icon,
.goal-account-stats > .metric-card:nth-child(4) .goal-stat-icon {
  color: var(--lad-color-reward-strong);
  background: var(--lad-color-reward-muted);
  animation-delay: -1.4s;
}
.goal-account-stats > .metric-card:nth-child(4) .goal-stat-icon {
  animation-delay: -2.1s;
}
.goal-account-stats > .metric-card > i {
  flex: 1 1 0;
}
@keyframes account-stat-float {
  0%,
  100% {
    transform: translateY(1px) rotate(-3deg);
  }
  50% {
    transform: translateY(-2px) rotate(3deg);
  }
}
.goal-icon {
  width: 45px;
  height: 45px;
  @apply d-grid place-center flex-shrink-0;
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 15px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-color-info) 15%, transparent);
  font-size: 1.5rem;
  transform: rotate(-4deg);
}
@include respond-up(studio) {
  .goal-account-stats {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
  .goal-account-stats > .metric-card {
    @apply justify-center;
    min-height: 66px;
  }
  .goal-stat-icon {
    width: 27px;
    height: 27px;
  }
  .goal-stat-icon :deep(.v-icon) {
    width: 1rem;
    height: 1rem;
    font-size: 1rem;
  }
}
.child-selector {
  margin-inline: -4px;
}
.cheer-button {
  width: 48px;
  height: 42px;
  @apply position-relative d-grid place-center flex-shrink-0;
  border: 2px solid color-mix(in srgb, var(--lad-color-info) 18%, transparent);
  border-radius: 15px;
  color: var(--lad-color-info-deep);
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-surface-soft)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-info-shadow) 18%, transparent);
  cursor: pointer;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease,
    background 0.18s ease;
}
.cheer-button:hover {
  transform: translateY(-2px) rotate(-2deg);
  box-shadow: 0 6px 0
    color-mix(in srgb, var(--lad-color-info-shadow) 18%, transparent);
}
.cheer-button:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-color-info-shadow) 18%, transparent);
}
.cheer-button:focus-visible {
  @include focus-ring(color-mix(in srgb, var(--lad-blue) 42%, transparent));
}
.cheer-button.is-cheered {
  border-color: color-mix(
    in srgb,
    var(--lad-color-reward-accent) 35%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-color-reward-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-color-reward-shadow) 20%, transparent),
    0 0 18px color-mix(in srgb, var(--lad-color-reward) 30%, transparent);
}
.cheer-hands {
  @apply d-flex align-center justify-center;
  width: 34px;
  height: 27px;
}
.cheer-hands i {
  font-style: normal;
  font-size: 1.125rem;
  transform-origin: bottom center;
}
.cheer-hands i:first-child {
  margin-right: -4px;
  transform: rotate(19deg);
  animation: cheer-left 1.8s ease-in-out infinite;
}
.cheer-hands i:last-child {
  transform: rotate(-19deg);
  animation: cheer-right 1.8s ease-in-out infinite;
}
.cheer-button.is-cheered .cheer-hands i {
  animation-duration: 0.72s;
}
.cheer-spark {
  @apply position-absolute pointer-events-none;
  color: var(--lad-color-reward-accent);
  opacity: 0.45;
  font-size: 0.5625rem;
  animation: cheer-spark 1.8s ease-in-out infinite;
}
.cheer-spark--one {
  top: 3px;
  right: 7px;
}
.cheer-spark--two {
  bottom: 4px;
  left: 7px;
  animation-delay: -0.8s;
}
.cheer-button.is-cheered .cheer-spark {
  opacity: 1;
  animation-duration: 0.8s;
}
.support-icon {
  width: 58px;
  height: 58px;
  @apply d-grid place-center;
  border-radius: 19px;
  background: var(--lad-surface-soft);
  font-size: 1.9375rem;
  animation: goal-float 2.8s ease-in-out infinite;
}
.family-gift-button {
  @apply position-relative overflow-visible;
}
.support-dialog-card {
  @apply position-relative overflow-hidden;
  border: 2px solid color-mix(in srgb, var(--lad-color-info) 18%, transparent);
  background:
    radial-gradient(
      circle at 88% 5%,
      color-mix(in srgb, var(--lad-color-reward) 30%, transparent),
      transparent 27%
    ),
    linear-gradient(
      155deg,
      var(--lad-surface),
      var(--lad-surface-soft) 64%,
      var(--lad-surface-soft)
    );
  box-shadow:
    0 9px 0 color-mix(in srgb, var(--lad-color-primary-deep) 12%, transparent),
    0 24px 54px color-mix(in srgb, var(--lad-text) 25%, transparent);
}
.support-dialog-heading {
  @apply d-flex align-center;
  gap: 13px;
}
.support-dialog-heading h2 {
  @apply ma-0;
  font-size: 1.25rem;
  line-height: 1.15;
}
.support-dialog-heading span {
  @apply d-block;
  margin-top: 4px;
  color: var(--lad-muted);
  font-size: 0.625rem;
}
.support-journey {
  height: 54px;
  padding-inline: 8px;
  @apply position-relative d-flex align-center justify-space-between;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 12%, transparent);
  border-radius: 18px;
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
}
.support-journey > i {
  width: 10px;
  height: 5px;
  border-radius: var(--lad-radius-pill);
  background: var(--lad-neutral-soft);
}
.support-coin,
.support-present {
  width: 36px;
  height: 36px;
  @apply d-grid place-center;
  z-index: 2;
  border: 3px solid var(--lad-border-on-accent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-accent-warm-ink) 12%, transparent);
}
.support-coin {
  color: var(--lad-color-reward-strong);
  border-radius: 50%;
  background: var(--lad-color-reward);
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-black);
}
.support-present {
  border-radius: 12px;
  background: var(--lad-surface-soft);
  font-size: 1.25rem;
}
.support-high-five {
  font-size: 1.5625rem;
}
.support-dialog-card.is-sending .support-coin {
  animation: support-coin-flight 0.85s ease-in forwards;
}
.support-dialog-card.is-sending .support-present {
  animation: support-present-receive 0.75s 0.3s ease-in-out;
}
.support-dialog-card.is-sending .support-high-five {
  animation: support-high-five 0.75s ease-in-out;
}
.support-submit {
  min-height: 43px;
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-strong)
  );
  box-shadow: 0 4px 0 var(--lad-color-primary-deep);
  font-size: 0.625rem;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: none;
  letter-spacing: 0;
}
.support-submit :deep(.v-btn__content) {
  gap: 7px;
}
@keyframes cheer-left {
  0%,
  100% {
    transform: translateX(-1px) rotate(19deg);
  }
  45%,
  55% {
    transform: translateX(3px) rotate(7deg) scale(1.05);
  }
}
@keyframes cheer-right {
  0%,
  100% {
    transform: translateX(1px) rotate(-19deg);
  }
  45%,
  55% {
    transform: translateX(-3px) rotate(-7deg) scale(1.05);
  }
}
@keyframes cheer-spark {
  0%,
  35%,
  100% {
    opacity: 0.2;
    transform: scale(0.65) rotate(0);
  }
  52%,
  72% {
    opacity: 1;
    transform: scale(1.25) rotate(25deg);
  }
}
@keyframes support-coin-flight {
  0% {
    opacity: 1;
    transform: translateX(0) rotate(0);
  }
  80% {
    opacity: 1;
  }
  100% {
    opacity: 0;
    transform: translateX(230px) rotate(220deg) scale(0.65);
  }
}
@keyframes support-present-receive {
  0%,
  100% {
    transform: scale(1) rotate(0);
  }
  52% {
    transform: scale(1.2) rotate(8deg);
  }
}
@keyframes support-high-five {
  0%,
  100% {
    transform: rotate(0) scale(1);
  }
  45% {
    transform: rotate(-18deg) scale(1.25);
  }
}
@include reduced-motion {
  .cheer-hands i,
  .cheer-spark,
  .support-dialog-card.is-sending .support-coin,
  .support-dialog-card.is-sending .support-present,
  .support-dialog-card.is-sending .support-high-five {
    animation: none;
  }
}
.family-goal-actions {
  @apply d-flex align-center;
  gap: 6px;
}
.assign-button {
  min-height: 38px;
  padding-inline: 8px;
  @apply position-relative overflow-visible;
  border: 2px solid
    color-mix(in srgb, var(--lad-border-on-accent) 80%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-info-soft)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-color-info-strong) 20%, transparent),
    0 8px 14px color-mix(in srgb, var(--lad-color-info-strong) 10%, transparent);
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: none;
  letter-spacing: 0;
}
.assign-button :deep(.v-btn__content) {
  gap: 5px;
}
.assign-coin {
  width: 23px;
  height: 23px;
  @apply d-grid place-center flex-shrink-0;
  animation: assign-coin-travel 2.3s ease-in-out infinite;
}
.assign-coin :deep(.ladirchen-coin) {
  width: 23px;
  height: 23px;
}
.assign-spark {
  @apply position-absolute pointer-events-none;
  top: -7px;
  right: 5px;
  color: var(--lad-color-reward-border);
  font-size: 0.625rem;
  font-style: normal;
  animation: assign-spark 2.3s ease-in-out infinite;
}
.save-dialog-card {
  @apply overflow-hidden;
  border: 2px solid color-mix(in srgb, var(--lad-color-info) 20%, transparent);
  background: linear-gradient(
    180deg,
    var(--lad-surface),
    var(--lad-surface-raised)
  );
  box-shadow:
    0 9px 0 color-mix(in srgb, var(--lad-color-info-deep) 15%, transparent),
    0 25px 55px
      color-mix(in srgb, var(--lad-color-bonus-info-deep) 25%, transparent);
}
.save-dialog-header {
  padding: 16px 14px;
  @apply d-flex align-center;
  gap: 11px;
  border-bottom: 2px solid
    color-mix(in srgb, var(--lad-color-info) 12%, transparent);
  background:
    radial-gradient(
      circle at 88% 4%,
      color-mix(in srgb, var(--lad-color-reward) 30%, transparent),
      transparent 28%
    ),
    linear-gradient(
      145deg,
      var(--lad-surface-soft),
      var(--lad-color-reward-soft)
    );
}
.save-dialog-header h2 {
  @apply ma-0;
  font-size: 1.25rem;
  letter-spacing: -0.035em;
}
.save-dialog-header div > span {
  @apply d-block;
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.5625rem;
}
.save-goal-icon {
  width: 54px;
  height: 54px;
  @apply d-grid place-center flex-shrink-0;
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 18px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-info-strong) 15%, transparent);
  font-size: 1.8125rem;
  transform: rotate(-5deg);
}
.save-dialog-content {
  padding: 17px;
}
.save-balance {
  min-height: 65px;
  padding: 10px 12px;
  @apply d-flex align-center;
  gap: 9px;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 15%, transparent);
  border-radius: 18px;
  background: color-mix(in srgb, var(--lad-surface-raised) 75%, transparent);
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-primary-supporting) 8%, transparent);
}
.save-balance-coin,
.save-goal-star {
  width: 38px;
  height: 38px;
  @apply d-grid place-center flex-shrink-0;
  border: 3px solid var(--lad-border-on-accent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-reward-strong) 12%, transparent);
  font-weight: var(--lad-font-weight-black);
}
.save-balance-coin {
  color: var(--lad-color-reward-strong);
  border-radius: 50%;
  background: radial-gradient(
    circle at 35% 28%,
    var(--lad-color-reward-pale),
    var(--lad-color-reward) 48%,
    var(--lad-color-reward-accent)
  );
}
.save-goal-star {
  color: var(--lad-color-reward-pale);
  border-radius: 13px;
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-strong)
  );
}
.save-balance > div {
  min-width: 70px;
}
.save-balance small,
.save-balance strong {
  @apply d-block;
}
.save-balance small {
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.save-balance strong {
  font-size: 0.9375rem;
}
.save-journey {
  height: 8px;
  @apply position-relative flex-grow-1;
  border-radius: var(--lad-radius-pill);
  background: repeating-linear-gradient(
    90deg,
    var(--lad-color-info-soft) 0 7px,
    transparent 7px 12px
  );
}
.save-journey i {
  width: 7px;
  height: 7px;
  @apply position-absolute;
  top: 0;
  left: 2px;
  border-radius: 50%;
  background: var(--lad-color-reward);
  opacity: 0;
}
.save-journey i:nth-child(2) {
  animation-delay: 0.22s;
}
.save-journey i:nth-child(3) {
  animation-delay: 0.44s;
}
.save-slider :deep(.v-slider-thumb__surface) {
  width: 28px;
  height: 28px;
  border: 3px solid var(--lad-border-on-accent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-info-strong) 18%, transparent);
}
.save-value {
  margin: 12px auto 15px;
  @apply d-flex align-center justify-center;
  gap: 7px;
  color: var(--lad-blue-dark);
}
.save-value > span {
  width: 31px;
  height: 31px;
  @apply d-grid place-center;
  color: var(--lad-color-reward-strong);
  border: 2px solid var(--lad-color-reward-pale);
  border-radius: 50%;
  background: var(--lad-color-reward);
  box-shadow: 0 3px 0 var(--lad-color-reward-shadow);
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-black);
}
.save-value strong {
  font-size: 2.125rem;
  line-height: 1;
}
.save-value small {
  color: var(--lad-muted);
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-strong);
}
.save-empty-note {
  margin: -3px 8px 14px;
  color: var(--lad-muted);
  @apply text-center;
  font-size: 0.5625rem;
  line-height: 1.4;
}
.save-submit {
  width: 100%;
  min-height: 49px;
  padding: 8px 13px;
  @apply position-relative d-flex align-center justify-center overflow-hidden cursor-pointer;
  gap: 8px;
  color: var(--lad-text-inverse);
  border: 3px solid
    color-mix(in srgb, var(--lad-border-on-accent) 80%, transparent);
  border-radius: 17px;
  background: linear-gradient(
    145deg,
    var(--lad-color-info-subtle),
    var(--lad-color-info-strong)
  );
  box-shadow:
    0 5px 0 var(--lad-color-info-strong),
    0 10px 17px
      color-mix(in srgb, var(--lad-color-info-strong) 18%, transparent);
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.save-submit:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow:
    0 7px 0 var(--lad-color-info-strong),
    0 13px 20px
      color-mix(in srgb, var(--lad-color-info-strong) 20%, transparent);
}
.save-submit:active:not(:disabled) {
  transform: translateY(3px);
  box-shadow: 0 2px 0 var(--lad-color-info-strong);
}
.save-submit:disabled {
  cursor: default;
  filter: grayscale(0.3);
  opacity: 0.48;
}
.save-submit > span:first-child {
  width: 27px;
  height: 27px;
  @apply d-grid place-center;
  color: var(--lad-color-reward-strong);
  border: 2px solid var(--lad-color-reward-pale);
  border-radius: 50%;
  background: var(--lad-color-reward);
  font-size: 0.625rem;
  font-weight: var(--lad-font-weight-black);
  animation: save-button-coin 2.2s ease-in-out infinite;
}
.save-submit > span:nth-of-type(2) {
  font-size: 1.125rem;
}
.save-submit > i {
  @apply position-absolute;
  top: 4px;
  right: 9px;
  color: var(--lad-color-reward-pale);
  font-size: 0.6875rem;
  font-style: normal;
  animation: save-button-spark 1.7s ease-in-out infinite;
}
.save-dialog-card.is-saving .save-journey i {
  animation: save-journey 0.58s ease-in forwards;
}
.save-dialog-card.is-saving .save-goal-star {
  animation: save-goal-receive 0.55s 0.28s ease-in-out;
}
.save-dialog-card.is-saving .save-submit-coin {
  animation: save-submit-flight 0.58s ease-in forwards;
}
.dialog-actions {
  grid-template-columns: 1fr 1.3fr;
}
.goal-list-enter-active,
.goal-list-leave-active {
  transition:
    opacity 200ms ease,
    transform 200ms ease;
}
.goal-list-enter-from,
.goal-list-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
@keyframes goal-float {
  0%,
  100% {
    transform: translateY(2px) rotate(-2deg);
  }
  50% {
    transform: translateY(-5px) rotate(2deg);
  }
}
@keyframes assign-coin-travel {
  0%,
  25%,
  100% {
    transform: translateX(0) rotate(0);
  }
  52% {
    transform: translateX(4px) rotate(18deg) scale(1.08);
  }
  72% {
    transform: translateX(0) rotate(0);
  }
}
@keyframes assign-spark {
  0%,
  35%,
  100% {
    opacity: 0;
    transform: scale(0.5) rotate(0);
  }
  55%,
  70% {
    opacity: 1;
    transform: scale(1.22) rotate(22deg);
  }
}
@keyframes save-button-coin {
  0%,
  25%,
  100% {
    transform: translateX(0) rotate(0);
  }
  55% {
    transform: translateX(5px) rotate(20deg) scale(1.08);
  }
}
@keyframes save-button-spark {
  0%,
  35%,
  100% {
    opacity: 0.2;
    transform: scale(0.7) rotate(0);
  }
  55% {
    opacity: 1;
    transform: scale(1.25) rotate(20deg);
  }
}
@keyframes save-journey {
  0% {
    left: 2px;
    opacity: 0;
    transform: scale(0.5);
  }
  20%,
  80% {
    opacity: 1;
  }
  100% {
    left: calc(100% - 9px);
    opacity: 0;
    transform: scale(1.15);
  }
}
@keyframes save-goal-receive {
  0%,
  100% {
    transform: scale(1) rotate(0);
  }
  50% {
    transform: scale(1.18) rotate(8deg);
  }
}
@keyframes save-submit-flight {
  0% {
    transform: translateX(0) rotate(0);
    opacity: 1;
  }
  100% {
    transform: translateX(110px) rotate(180deg) scale(0.7);
    opacity: 0;
  }
}
@include reduced-motion {
  .assign-coin,
  .assign-spark,
  .save-submit > span:first-child,
  .save-submit > i,
  .save-dialog-card.is-saving .save-journey i,
  .save-dialog-card.is-saving .save-goal-star,
  .save-dialog-card.is-saving .save-submit-coin {
    animation: none;
  }
}
</style>
