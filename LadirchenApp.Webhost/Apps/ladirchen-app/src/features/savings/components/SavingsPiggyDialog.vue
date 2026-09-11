<template>
  <v-dialog v-model="store.piggyBankOpen" content-class="piggy-summary-dialog-frame" max-width="460" scrollable>
    <v-card class="piggy-dialog" :class="{ 'piggy-dialog--guardian': store.viewerRole === 'guardian' }" rounded="xl">
      <div class="piggy-header pa-3">
        <div class="piggy-title-row">
          <HeaderDecoration tone="balance" />
          <div class="piggy-title-copy">
            <p class="dialog-kicker">{{ t(`savings.piggy.header.${store.viewerRole}.eyebrow`) }}</p>
            <h2>{{ store.viewerRole === 'guardian' ? t('savings.piggy.header.guardian.title') : t('savings.piggy.header.child.title', { name: store.activeChild.name }) }}</h2>
            <p class="piggy-subtitle">{{ t(`savings.piggy.header.${store.viewerRole}.description`) }}</p>
          </div>
          <div class="piggy-title-actions">
            <span class="header-coin"><LadirchenCoin animated /></span>
            <button class="piggy-close" :aria-label="t('savings.piggy.close')" type="button" @click="store.piggyBankOpen = false"><v-icon icon="i-mdi:close" /></button>
          </div>
        </div>
        <template v-if="store.viewerRole === 'child'">
          <div class="balance-grid mt-3">
            <MetricCard class="balance-tile balance-tile--wallet" tone="balance">
              <span class="balance-icon balance-icon--wallet" aria-hidden="true"><v-icon icon="i-mdi:wallet-outline" /></span>
              <span>
                <small>{{ t('savings.piggy.balance.available') }}</small>
                <strong><LadirchenAmount :value="store.availableBalance" /></strong>
                <span class="today-earned"><v-icon aria-hidden="true" icon="i-mdi:sparkles" />{{ t('savings.piggy.balance.earnedToday') }} <b><LadirchenAmount compact prefix="+" :value="store.todayEarned" /></b></span>
              </span>
            </MetricCard>
            <MetricCard class="balance-tile balance-tile--plans" tone="bonus"><span class="balance-icon balance-icon--plans" aria-hidden="true"><v-icon icon="i-mdi:star-four-points-outline" /></span><span><small>{{ t('savings.piggy.balance.inGoals') }}</small><strong><LadirchenAmount :value="store.totalSaved" /></strong></span></MetricCard>
          </div>
          <div class="balance-detail-grid mt-2">
            <MetricCard class="balance-tile interest-earned" tone="energy"><span class="balance-icon balance-icon--interest" aria-hidden="true"><v-icon icon="i-mdi:chart-line" /></span><span><small>{{ t('savings.piggy.balance.interestEarned') }}</small><strong><LadirchenAmount prefix="+" :value="store.totalInterestEarned" /></strong></span></MetricCard>
            <MetricCard class="family-currency-value" tone="balance">
              <AnimatedExchangeIcon :currency-code="store.familyCurrencyCode" />
              <div>
                <span>{{ t('savings.piggy.balance.familyCurrency') }}</span>
                <strong><LadirchenAmount :value="`${store.availableBalance} = ${formattedFamilyValue}`" /></strong>
              </div>
              <small>{{ t('savings.piggy.balance.exchangeRate', { amount: store.ladirchenPerCurrencyUnit, currency: store.familyCurrencyCode }) }}</small>
            </MetricCard>
          </div>
        </template>
      </div>

      <v-card-text class="piggy-content pa-3">
        <section v-if="store.viewerRole === 'guardian'" class="guardian-balances" aria-labelledby="guardian-balances-title">
          <div class="guardian-balances-heading">
            <div><p class="eyebrow mb-1">{{ t('savings.piggy.guardianAccounts.eyebrow') }}</p><h3 id="guardian-balances-title" class="dialog-section-title">{{ t('savings.piggy.guardianAccounts.title') }}</h3></div>
            <span>{{ t('savings.piggy.guardianAccounts.childCount', { count: guardianChildren.length }) }}</span>
          </div>
          <div class="guardian-balance-list mt-3">
            <article v-for="child in guardianChildren" :key="child.id" class="guardian-balance-row">
              <span class="guardian-child-avatar" aria-hidden="true"><AvatarFigure :appearance="memberAppearance(child.id)" :size="40" /></span>
              <span class="guardian-child-name"><strong>{{ child.name }}</strong><small>{{ t('savings.piggy.guardianAccounts.balanceSplit', { available: child.available, saved: child.saved }) }}</small></span>
              <span class="guardian-child-stat"><small>{{ t('savings.piggy.guardianAccounts.balance') }}</small><strong><LadirchenAmount compact :value="child.total" /></strong></span>
              <span class="guardian-child-stat"><small>{{ t('savings.piggy.guardianAccounts.thisWeek') }}</small><strong><LadirchenAmount compact prefix="+" :value="child.weekEarned" /></strong></span>
              <span class="guardian-child-stat"><small>{{ t('savings.piggy.guardianAccounts.tasks') }}</small><strong>{{ child.completedThisWeek }}</strong></span>
              <span class="guardian-child-stat guardian-child-stat--level"><small>{{ t('savings.piggy.guardianAccounts.level') }}</small><strong>{{ child.ladiLevel }}</strong></span>
            </article>
          </div>
        </section>

        <v-card v-if="store.viewerRole === 'guardian' && store.permissions.canManageFamily" class="currency-settings-card pa-4 mt-4" elevation="0" rounded="xl">
          <div class="currency-settings-heading">
            <span aria-hidden="true">↔</span>
            <div><p class="eyebrow mb-1">{{ t('savings.piggy.currency.eyebrow') }}</p><h3 class="dialog-section-title">{{ t('savings.piggy.currency.title') }}</h3></div>
          </div>
          <div class="currency-settings-fields mt-3">
            <v-select
              density="compact"
              hide-details
              :items="currencyOptions"
              item-title="title"
              item-value="value"
              :label="t('savings.piggy.currency.currencyLabel')"
              :model-value="store.familyCurrencyCode"
              variant="outlined"
              @update:model-value="setCurrency"
            />
            <v-text-field
              density="compact"
              hide-details
              :label="t('savings.piggy.currency.rateLabel')"
              min="1"
              :model-value="store.ladirchenPerCurrencyUnit"
              type="number"
              variant="outlined"
              @update:model-value="store.setLadirchenExchangeRate(Number($event))"
            />
          </div>
          <div class="currency-settings-example mt-2"><span>{{ t('savings.piggy.currency.example') }}</span><strong>{{ formattedGuardianFamilyValue }}</strong></div>
        </v-card>

        <v-card v-if="store.viewerRole === 'child'" class="transfer-card pa-3" :class="transferDirection ? `transfer-${transferDirection}` : ''" color="blue-lighten-5" elevation="0" rounded="xl">
          <div class="transfer-heading">
            <div class="transfer-title">
              <span class="transfer-title-icon" aria-hidden="true">
                <LadirchenCoin animated small />
              </span>
              <div>
                <p class="eyebrow mb-1">{{ t('savings.piggy.transfer.eyebrow') }}</p>
                <h3 class="dialog-section-title">{{ t('savings.piggy.transfer.title') }}</h3>
              </div>
            </div>
          </div>
          <div v-if="store.viewerRole === 'child'" class="destination-switch mt-3" :aria-label="t('savings.piggy.transfer.destinationAria')" role="group">
            <button :aria-pressed="transferDestination === 'goal'" :class="{ active: transferDestination === 'goal' }" type="button" @click="setTransferDestination('goal')">
              <span aria-hidden="true"><v-icon icon="i-mdi:piggy-bank-outline" /></span><span><strong>{{ t('savings.piggy.transfer.goal') }}</strong><small>{{ t('savings.piggy.transfer.goalHint') }}</small></span>
            </button>
            <button :aria-pressed="transferDestination === 'member'" :class="{ active: transferDestination === 'member' }" type="button" @click="setTransferDestination('member')">
              <span aria-hidden="true"><v-icon icon="i-mdi:account-heart-outline" /></span><span><strong>{{ t('savings.piggy.transfer.gift') }}</strong><small>{{ t('savings.piggy.transfer.giftHint') }}</small></span>
            </button>
          </div>
          <div class="transfer-fields mt-3">
            <label class="transfer-field">
              <span><i>1</i>{{ t('savings.piggy.transfer.destination') }}</span>
              <v-select
                v-if="transferDestination === 'goal'"
                v-model="selectedGoalId"
                class="transfer-input"
                density="compact"
                hide-details
                :items="goalOptions"
                item-title="title"
                item-value="value"
                :aria-label="t('savings.piggy.transfer.selectGoal')"
                variant="solo"
              >
                <template #selection="{ item }">
                  <span class="selected-goal-option">
                    <span class="goal-option-icon" aria-hidden="true">{{ item.icon }}</span>
                    <span><strong>{{ item.title }}</strong><small>{{ t('savings.piggy.transfer.goalProgress', { saved: item.saved, target: item.target }) }}</small></span>
                  </span>
                </template>
                <template #item="{ props: itemProps, item }">
                  <v-list-item v-bind="itemProps" class="goal-option" :title="undefined">
                    <template #prepend><span class="goal-option-icon" aria-hidden="true">{{ item.icon }}</span></template>
                    <v-list-item-title>{{ item.title }}</v-list-item-title>
                    <v-list-item-subtitle>{{ t('savings.piggy.transfer.goalSaved', { saved: item.saved, target: item.target }) }}</v-list-item-subtitle>
                    <template #append><span class="goal-option-progress">{{ goalProgress(item.saved, item.target) }} %</span></template>
                  </v-list-item>
                </template>
              </v-select>
              <v-select
                v-else
                v-model="selectedMemberId"
                class="transfer-input"
                density="compact"
                hide-details
                :items="memberOptions"
                item-title="title"
                item-value="value"
                :aria-label="t('savings.piggy.transfer.selectMember')"
                variant="solo"
              >
                <template #selection="{ item }">
                  <span class="selected-goal-option">
                    <span class="goal-option-icon member-option-icon" aria-hidden="true"><AvatarFigure :appearance="memberAppearance(item.value)" :size="34" /></span>
                    <span><strong>{{ item.title }}</strong><small>{{ t('savings.piggy.transfer.giftCoins') }}</small></span>
                  </span>
                </template>
                <template #item="{ props: itemProps, item }">
                  <v-list-item v-bind="itemProps" class="goal-option" :title="undefined">
                    <template #prepend><span class="goal-option-icon member-option-icon" aria-hidden="true"><AvatarFigure :appearance="memberAppearance(item.value)" :size="36" /></span></template>
                    <v-list-item-title>{{ item.title }}</v-list-item-title>
                    <v-list-item-subtitle>{{ t('savings.piggy.transfer.giftDescription') }}</v-list-item-subtitle>
                  </v-list-item>
                </template>
              </v-select>
            </label>
            <div class="transfer-field">
              <span><i>2</i>{{ t('savings.piggy.transfer.amount') }}</span>
              <div class="amount-stepper" role="group" :aria-label="t('savings.piggy.transfer.amountAria')">
                <button :aria-label="t('savings.piggy.transfer.decrease')" :disabled="amount <= 0" type="button" @click="adjustTransferAmount(-5)">−</button>
                <output aria-live="polite"><span>{{ amount }}</span><LadirchenCoin small /></output>
                <button :aria-label="t('savings.piggy.transfer.increase')" :disabled="amount >= transferAmountMaximum" type="button" @click="adjustTransferAmount(5)">+</button>
              </div>
            </div>
          </div>
          <div v-if="transferDestination === 'goal'" class="d-grid transfer-actions ga-2 mt-3">
            <v-btn class="transfer-button transfer-button--withdraw" :disabled="amount <= 0 || amount > selectedGoalWithdrawable" rounded="lg" variant="tonal" @click="withdraw">{{ t('savings.piggy.transfer.withdraw') }}</v-btn>
            <v-btn class="transfer-button transfer-button--deposit" color="info" :disabled="amount <= 0 || amount > maxDeposit" rounded="lg" variant="flat" @click="deposit">{{ t('savings.piggy.transfer.deposit') }}</v-btn>
          </div>
          <v-btn v-else class="transfer-button transfer-button--gift mt-3" block :disabled="!selectedMemberId || amount <= 0 || amount > store.availableBalance" rounded="lg" variant="flat" @click="giftToMember">
            {{ t('savings.piggy.transfer.send', { name: selectedMemberName }) }}<v-icon icon="i-mdi:send-variant-outline" />
          </v-btn>
        </v-card>

        <v-card v-if="store.viewerRole === 'guardian'" class="conditions-card pa-4 mt-5" elevation="0" rounded="xl">
          <p class="eyebrow mb-1">{{ t('savings.piggy.interest.eyebrow') }}</p>
          <h3 class="dialog-section-title">{{ t('savings.piggy.interest.title') }}</h3>
          <p class="text-caption text-medium-emphasis mt-1 mb-4">{{ t('savings.piggy.interest.description') }}</p>
          <label class="setting-label">{{ t('savings.piggy.interest.base') }} <strong>{{ formatRate(store.baseSavingsRatePercent) }} %</strong></label>
          <v-slider v-model="store.baseSavingsRatePercent" color="primary" hide-details max="5" min="0" step="0.1" />
          <label class="setting-label">{{ t('savings.piggy.interest.streak') }} <strong>+{{ formatRate(store.streakBonusRate) }} %</strong></label>
          <v-slider v-model="store.streakBonusRate" color="primary" hide-details max="2" min="0" step="0.1" />
          <label class="setting-label">{{ t('savings.piggy.interest.completion') }} <strong>+{{ formatRate(store.completionBonusRate) }} %</strong></label>
          <v-slider v-model="store.completionBonusRate" color="primary" hide-details max="5" min="0" step="0.1" />
          <label class="setting-label">{{ t('savings.piggy.interest.rating') }} <strong>+{{ formatRate(store.ratingBonusRate) }} %</strong></label>
          <v-slider v-model="store.ratingBonusRate" color="primary" hide-details max="5" min="0" step="0.1" />
          <label class="setting-label">{{ t('savings.piggy.interest.maximum') }} <strong>{{ formatRate(store.maxSavingsRatePercent) }} %</strong></label>
          <v-slider v-model="store.maxSavingsRatePercent" color="warning" hide-details max="20" min="1" step="0.5" />
        </v-card>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { useI18n } from 'vue-i18n';

import AnimatedExchangeIcon from './AnimatedExchangeIcon.vue';
import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import LadirchenAmount from '@/shared/components/LadirchenAmount.vue';
import MetricCard from '@/shared/components/ui/MetricCard.vue';
import HeaderDecoration from '@/shared/components/ui/HeaderDecoration.vue';
import { useSavingsPiggy } from '../composables/use-savings-piggy';

const { t } = useI18n();
const {
  adjustTransferAmount, amount, currencyOptions, deposit, formatRate, formattedFamilyValue,
  formattedGuardianFamilyValue, giftToMember, goalOptions, goalProgress, guardianChildren,
  maxDeposit, memberAppearance, memberOptions, selectedGoalId, selectedGoalWithdrawable,
  selectedMemberId, selectedMemberName, setCurrency, setTransferDestination, store, transferAmountMaximum,
  transferDestination, transferDirection, withdraw,
} = useSavingsPiggy();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.piggy-dialog {
  height: min(660px, calc(100dvh - 28px));
  max-height: min(660px, calc(100dvh - 28px));
  @apply d-flex flex-column overflow-hidden;
  @include dialog-frame;
}
.piggy-dialog :deep(.v-card-text) {
  min-height: 0;
  flex: 1 1 auto;
  overflow-y: hidden;
}
.piggy-dialog--guardian :deep(.v-card-text) {
  overflow-y: auto;
}
.piggy-header {
  @apply position-relative;
  flex: 0 0 auto;
  background: var(--lad-surface);
}
.piggy-content {
  background: linear-gradient(
    180deg,
    var(--lad-surface),
    var(--lad-surface-soft)
  );
}
.piggy-title-row {
  min-height: 108px;
  padding: 15px 16px;
  @apply d-flex align-center justify-space-between;
  gap: 10px;
  @include dialog-title-panel(1.3125rem);
}
.piggy-title-row::before,
.piggy-title-row::after {
  content: "";
  @apply position-absolute pointer-events-none;
  border-radius: 50%;
}
.piggy-title-row::before {
  width: 110px;
  height: 110px;
  top: -68px;
  right: -25px;
  background: color-mix(in srgb, var(--lad-color-info-soft) 60%, transparent);
  box-shadow: 0 0 0 15px
    color-mix(in srgb, var(--lad-surface-soft) 40%, transparent);
}
.piggy-title-row::after {
  width: 78px;
  height: 24px;
  right: 42px;
  bottom: -15px;
  background: color-mix(in srgb, var(--lad-color-info-soft) 60%, transparent);
}
.piggy-title-copy {
  max-width: 245px;
  @apply position-relative min-w-0 flex-grow-1;
  z-index: 1;
}
.dialog-kicker {
  margin: 0 0 4px;
  @include overline(var(--lad-blue), var(--lad-font-size-micro));
}
.piggy-header h2 {
  @apply ma-0;
  @include heading(1.4375rem, 1.1, -0.04em);
}
.piggy-subtitle {
  max-width: 230px;
  margin: 5px 0 0;
  @include body-copy(var(--lad-font-size-caption), 1.32);
}
.piggy-title-actions {
  @apply position-relative d-flex align-start flex-shrink-0;
  z-index: 2;
  gap: 5px;
}
.header-coin {
  width: 64px;
  height: 64px;
  @apply d-grid place-center flex-shrink-0;
  border-radius: 21px;
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-color-info) 12%, transparent);
}
.header-coin :deep(.ladirchen-coin) {
  scale: 0.76;
  filter: drop-shadow(
    0 7px 7px color-mix(in srgb, var(--lad-color-reward-deep) 15%, transparent)
  );
}
.piggy-close {
  @apply position-absolute;
  top: -10px;
  right: -10px;
  z-index: 3;
  @include dialog-close-button(2.125rem, var(--lad-radius-small));
}
.balance-grid,
.balance-detail-grid {
  @apply d-grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 9px;
}
.balance-tile {
  @apply position-relative min-w-0;
}
.balance-tile.interest-earned {
  color: var(--lad-color-primary-deep);
}
.balance-icon {
  @include icon-tile(
    2.5rem,
    0.875rem,
    linear-gradient(
      145deg,
      var(--lad-color-primary-highlight),
      var(--lad-color-info)
    ),
    var(--lad-color-info-strong),
    -5deg,
    0.1875rem solid var(--lad-surface-raised)
  );
  color: var(--lad-text-inverse);
}
.balance-icon--plans {
  background: linear-gradient(
    145deg,
    var(--lad-color-accent-pink),
    var(--lad-color-bonus)
  );
  box-shadow: 0 4px 0 var(--lad-color-bonus-muted);
}
.balance-icon--interest {
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-muted)
  );
  box-shadow: 0 4px 0 var(--lad-color-primary-deep);
}
.today-earned {
  width: fit-content;
  margin-top: 4px;
  padding: 2px 5px 2px 3px;
  @apply d-flex align-center text-no-wrap;
  gap: 2px;
  color: var(--lad-color-primary-deep);
  border-radius: var(--lad-radius-pill);
  background: color-mix(
    in srgb,
    var(--lad-color-primary-soft) 90%,
    transparent
  );
  font-size: 0.4375rem;
  font-weight: var(--lad-font-weight-strong);
  line-height: 1;
}
.today-earned :deep(.v-icon) {
  font-size: 0.625rem;
}
.today-earned b {
  font-weight: var(--lad-font-weight-black);
}
.piggy-title-row::before,
.piggy-title-row::after {
  content: none;
}
.balance-icon :deep(.v-icon) {
  @apply d-block;
  @apply ma-auto;
  color: inherit;
  opacity: 1;
}
.family-currency-value {
  min-height: 68px;
  padding: 7px 8px;
  @apply position-relative d-grid min-w-0;
  grid-template-columns: 36px minmax(0, 1fr);
  @apply align-center;
  column-gap: 6px;
}
.family-currency-value > .exchange-icon {
  width: 36px;
  grid-row: 1;
  scale: 0.76;
}
.family-currency-value span,
.family-currency-value strong {
  @apply d-block min-w-0;
}
.family-currency-value span {
  @apply overflow-hidden;
  color: var(--lad-muted);
  font-size: 0.4375rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.family-currency-value strong {
  margin-top: 2px;
  @apply overflow-hidden;
  color: var(--lad-text);
  font-size: 0.75rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.family-currency-value small {
  grid-column: 1 / -1;
  color: var(--lad-text-strong);
  font-size: 0.4375rem;
  font-weight: var(--lad-font-weight-strong);
  line-height: 1.2;
  text-align: center;
}
.dialog-section-title {
  @apply ma-0;
  font-size: 1rem;
  letter-spacing: -0.025em;
}
.conditions-card {
  border: 2px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 18%, transparent);
}
.guardian-balances {
  padding: 12px;
  border: 2px solid color-mix(in srgb, var(--lad-color-info) 15%, transparent);
  border-radius: 20px;
  background:
    radial-gradient(
      circle at 94% 4%,
      color-mix(in srgb, var(--lad-color-reward) 20%, transparent),
      transparent 28%
    ),
    linear-gradient(145deg, var(--lad-surface-raised), var(--lad-surface-soft));
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-color-info-strong) 10%, transparent);
}
.guardian-balances-heading {
  @apply d-flex align-end justify-space-between;
  gap: 8px;
}
.guardian-balances-heading > span {
  padding: 4px 8px;
  color: var(--lad-color-primary-deep);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-surface-soft);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
  white-space: nowrap;
}
.guardian-balance-list {
  @apply d-flex flex-column;
  gap: 8px;
}
.guardian-balance-row {
  min-height: 62px;
  padding: 8px;
  @apply d-grid align-center min-w-0;
  grid-template-columns:
    42px minmax(76px, 1fr) repeat(3, minmax(47px, 0.62fr))
    minmax(45px, 0.56fr);
  gap: 6px;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 12%, transparent);
  border-radius: 15px;
  background: color-mix(in srgb, var(--lad-surface-raised) 80%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary-deep) 8%, transparent);
}
.guardian-child-avatar {
  width: 42px;
  height: 42px;
  @apply d-grid place-center overflow-hidden;
  border-radius: 12px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-pale)
  );
}
.guardian-child-avatar :deep(.avatar-figure) {
  border: 0;
  background: transparent;
  box-shadow: none;
}
.guardian-child-name,
.guardian-child-name strong,
.guardian-child-name small,
.guardian-child-stat,
.guardian-child-stat small,
.guardian-child-stat strong {
  @apply d-block min-w-0;
}
.guardian-child-name strong {
  @apply overflow-hidden;
  font-size: 0.75rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.guardian-child-name small {
  margin-top: 3px;
  color: var(--lad-color-primary-strong);
  font-size: 0.5rem;
  font-weight: 800;
  white-space: nowrap;
}
.guardian-child-stat {
  padding-left: 5px;
  border-left: 1px solid
    color-mix(in srgb, var(--lad-color-primary-supporting) 12%, transparent);
}
.guardian-child-stat small {
  @apply overflow-hidden;
  color: var(--lad-muted);
  font-size: 0.46875rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.guardian-child-stat strong {
  margin-top: 2px;
  @apply overflow-hidden;
  font-size: 0.6875rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.guardian-child-stat--level strong {
  color: var(--lad-color-primary-deep);
  font-size: 0.9375rem;
}
.currency-settings-card {
  border: 2px solid
    color-mix(in srgb, var(--lad-color-reward-border) 25%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-surface),
    var(--lad-surface-soft)
  );
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-color-reward-accent) 10%, transparent);
}
.currency-settings-heading {
  @apply d-flex align-center;
  gap: 9px;
}
.currency-settings-heading > span {
  width: 35px;
  height: 35px;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-color-reward-strong);
  border-radius: 12px;
  background: var(--lad-color-reward-pale);
  font-size: 1.0625rem;
  font-weight: var(--lad-font-weight-black);
}
.currency-settings-fields {
  @apply d-grid;
  grid-template-columns: 0.9fr 1.1fr;
  gap: 7px;
}
.currency-settings-fields :deep(.v-field) {
  background: color-mix(in srgb, var(--lad-surface-raised) 80%, transparent);
}
.currency-settings-example {
  padding: 7px 9px;
  @apply d-flex align-center justify-space-between;
  gap: 8px;
  border-radius: 11px;
  background: color-mix(in srgb, var(--lad-surface-raised) 75%, transparent);
}
.currency-settings-example span {
  color: var(--lad-muted);
  font-size: 0.5rem;
}
.currency-settings-example strong {
  color: var(--lad-color-primary-deep);
  font-size: 0.75rem;
}
.transfer-card {
  @apply position-relative overflow-hidden;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 18%, transparent);
  background:
    radial-gradient(
      circle at 94% 4%,
      color-mix(in srgb, var(--lad-color-reward) 25%, transparent),
      transparent 27%
    ),
    linear-gradient(
      145deg,
      var(--lad-surface),
      var(--lad-surface-soft) 62%,
      var(--lad-surface-soft)
    );
  box-shadow:
    0 5px 0
      color-mix(in srgb, var(--lad-color-primary-supporting) 10%, transparent),
    0 12px 22px
      color-mix(in srgb, var(--lad-color-primary-supporting) 8%, transparent);
}
.transfer-card::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 7px;
  right: 10px;
  color: var(--lad-color-reward-border);
  font-size: 0.625rem;
  animation: transfer-spark 2s ease-in-out infinite;
}
.transfer-card::after {
  @apply d-none;
}
.destination-switch {
  padding: 4px;
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 5px;
  border-radius: 16px;
  background: color-mix(
    in srgb,
    var(--lad-color-primary-soft) 65%,
    transparent
  );
  box-shadow: inset 0 0 0 1px
    color-mix(in srgb, var(--lad-color-primary-supporting) 10%, transparent);
}
.destination-switch button {
  min-width: 0;
  min-height: 47px;
  padding: 6px 8px;
  @apply d-flex align-center text-left cursor-pointer;
  gap: 7px;
  color: var(--lad-color-primary-supporting);
  border: 2px solid transparent;
  border-radius: 13px;
  background: transparent;
  transition:
    transform 0.16s ease,
    background 0.16s ease;
}
.destination-switch button > span:first-child {
  width: 28px;
  height: 28px;
  @apply d-grid place-center flex-shrink-0;
  border-radius: 10px;
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
  font-size: 1rem;
}
.destination-switch strong,
.destination-switch small {
  @apply d-block;
}
.destination-switch strong {
  font-size: 0.625rem;
  line-height: 1.15;
}
.destination-switch small {
  margin-top: 2px;
  font-size: 0.4375rem;
  opacity: 0.75;
}
.destination-switch button.active {
  color: var(--lad-color-primary-deep);
  border-color: color-mix(
    in srgb,
    var(--lad-border-on-accent) 90%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-color-reward-soft)
  );
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary-deep) 12%, transparent);
  transform: translateY(-1px);
}
.destination-switch button {
  @apply justify-center;
  text-align: center;
}
.destination-switch button > span:first-child :deep(.v-icon) {
  @apply d-block;
  @apply ma-auto;
  color: inherit;
  opacity: 1;
}
.transfer-heading {
  @apply d-flex align-center justify-space-between;
  gap: 10px;
}
.transfer-visual {
  width: 112px;
  height: 40px;
  @apply d-flex align-center justify-space-between flex-shrink-0;
}
.transfer-wallet,
.transfer-goal {
  width: 34px;
  height: 34px;
  @apply d-grid place-center;
  z-index: 2;
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 12px;
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-info-strong) 12%, transparent);
  font-weight: var(--lad-font-weight-black);
}
.transfer-wallet {
  color: var(--lad-color-reward-strong);
  background: linear-gradient(
    145deg,
    var(--lad-color-reward-highlight),
    var(--lad-color-reward)
  );
}
.transfer-goal {
  color: var(--lad-color-primary-strong);
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-primary-soft)
  );
}
.transfer-track {
  height: 6px;
  @apply position-relative flex-grow-1;
  margin-inline: -3px;
  border-radius: var(--lad-radius-pill);
  background: repeating-linear-gradient(
    90deg,
    var(--lad-color-info-soft) 0 7px,
    transparent 7px 11px
  );
}
.transfer-track i {
  width: 9px;
  height: 9px;
  @apply position-absolute;
  top: -2px;
  left: 4px;
  border: 2px solid var(--lad-border-on-accent);
  border-radius: 50%;
  background: var(--lad-color-reward);
  box-shadow: 0 2px 3px
    color-mix(in srgb, var(--lad-color-reward-strong) 18%, transparent);
  opacity: 0;
}
.transfer-card.transfer-deposit .transfer-track i {
  animation: transfer-to-goal 0.95s ease-in-out forwards;
}
.transfer-card.transfer-withdraw .transfer-track i {
  animation: transfer-to-wallet 0.95s ease-in-out forwards;
}
.transfer-track i:nth-child(2) {
  animation-delay: 0.12s;
}
.transfer-track i:nth-child(3) {
  animation-delay: 0.24s;
}
.transfer-card.transfer-deposit .transfer-goal,
.transfer-card.transfer-withdraw .transfer-wallet {
  animation: transfer-receive 0.6s 0.45s ease-in-out;
}
.transfer-actions {
  grid-template-columns: 1fr 1fr;
}
.transfer-button {
  min-height: 42px;
  border: 2px solid
    color-mix(in srgb, var(--lad-border-on-accent) 80%, transparent);
  border-radius: 14px;
  font-size: 0.5625rem;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: none;
  letter-spacing: 0;
}
.transfer-button :deep(.v-btn__content) {
  gap: 5px;
}
.transfer-button :deep(.v-btn__content) > span {
  font-size: 0.9375rem;
}
.transfer-button--withdraw {
  color: var(--lad-color-info-deep);
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-info-shadow) 15%, transparent);
}
.transfer-button--deposit {
  background: linear-gradient(
    145deg,
    var(--lad-color-info-subtle),
    var(--lad-color-info-strong)
  );
  box-shadow:
    0 4px 0 var(--lad-color-info-strong),
    0 8px 14px color-mix(in srgb, var(--lad-color-info-strong) 15%, transparent);
}
.transfer-card.transfer-gift .transfer-track i {
  animation: transfer-to-goal 0.95s ease-in-out forwards;
}
.transfer-card.transfer-gift .transfer-goal {
  animation: transfer-receive 0.6s 0.45s ease-in-out;
}
.transfer-button--gift {
  color: var(--lad-text-inverse);
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-muted)
  );
  box-shadow:
    0 4px 0 var(--lad-color-primary-deep),
    0 8px 14px
      color-mix(in srgb, var(--lad-color-primary-deep) 18%, transparent);
}
.transfer-title {
  @apply d-flex align-center;
  gap: 8px;
}
.transfer-title-icon {
  width: 35px;
  height: 35px;
  @apply d-flex align-center justify-center flex-shrink-0 overflow-hidden;
  color: var(--lad-color-primary-supporting);
  border: 2px solid var(--lad-border-on-accent);
  border-radius: 12px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary-supporting) 15%, transparent);
  animation: money-path-float 1.8s ease-in-out infinite;
}
.transfer-title-icon :deep(.ladirchen-coin) {
  width: 25px;
  height: 25px;
}
.transfer-fields {
  @apply d-grid;
  grid-template-columns: 1.4fr 0.8fr;
  gap: 8px;
}
.transfer-field {
  min-width: 0;
  padding: 7px;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 12%, transparent);
  border-radius: 15px;
  background: color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary-supporting) 8%, transparent);
}
.transfer-field > span {
  margin: 0 2px 5px;
  @apply d-flex align-center;
  gap: 5px;
  color: var(--lad-text-strong);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
}
.transfer-field > span i {
  width: 17px;
  height: 17px;
  @apply d-grid place-center;
  color: var(--lad-text-inverse);
  border-radius: 6px;
  background: var(--lad-color-primary-muted);
  font-size: 0.5rem;
  font-style: normal;
  transform: rotate(-4deg);
}
.transfer-input :deep(.v-field) {
  border: 0;
  border-radius: 11px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-surface)
  );
  box-shadow:
    inset 0 0 0 1px
      color-mix(in srgb, var(--lad-color-primary-supporting) 10%, transparent),
    0 2px 0 color-mix(in srgb, var(--lad-color-primary-deep) 8%, transparent);
}
.transfer-input :deep(.v-field__overlay),
.transfer-input :deep(.v-field__outline) {
  @apply d-none;
}
.transfer-input :deep(.v-field__input) {
  min-height: 38px;
  padding-inline: 9px;
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-strong);
}
.transfer-input :deep(.v-field__append-inner) {
  padding-top: 8px;
}
.transfer-amount :deep(input[type="number"]) {
  appearance: textfield;
}
.transfer-amount :deep(input[type="number"]::-webkit-inner-spin-button),
.transfer-amount :deep(input[type="number"]::-webkit-outer-spin-button) {
  @apply ma-0;
  appearance: none;
}
.selected-goal-option {
  min-width: 0;
  @apply d-flex align-center;
  gap: 7px;
}
.selected-goal-option > span:last-child,
.selected-goal-option strong,
.selected-goal-option small {
  @apply d-block min-w-0;
}
.selected-goal-option strong {
  @apply overflow-hidden;
  font-size: 0.6875rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.selected-goal-option small {
  color: var(--lad-muted);
  font-size: 0.4375rem;
  font-weight: 750;
}
.goal-option-icon {
  width: 31px;
  height: 31px;
  @apply d-grid place-center flex-shrink-0;
  border: 2px solid var(--lad-border-on-accent);
  border-radius: 10px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-color-info-deep) 12%, transparent);
  font-size: 1.0625rem;
  transform: rotate(-4deg);
}
.goal-option {
  min-height: 57px;
  margin: 5px 7px;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 12%, transparent);
  border-radius: 15px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-surface)
  );
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary-deep) 8%, transparent);
}
.goal-option :deep(.v-list-item-title) {
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-heavy);
}
.goal-option :deep(.v-list-item-subtitle) {
  font-size: 0.5rem;
  opacity: 0.72;
}
.goal-option-progress {
  padding: 4px 6px;
  color: var(--lad-color-primary-deep);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-surface-soft);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-black);
}
.member-option-icon {
  @apply overflow-hidden;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
}
.member-option-icon :deep(.avatar-figure) {
  transform-origin: center;
  animation: member-avatar-hello 3.2s ease-in-out infinite;
}
.amount-stepper {
  height: 40px;
  @apply d-grid align-center overflow-hidden;
  grid-template-columns: 36px 1fr 36px;
  border-radius: 11px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-surface)
  );
  box-shadow:
    inset 0 0 0 1px
      color-mix(in srgb, var(--lad-color-primary-supporting) 10%, transparent),
    0 2px 0 color-mix(in srgb, var(--lad-color-primary-deep) 8%, transparent);
}
.amount-stepper button {
  width: 30px;
  height: 30px;
  @apply d-grid place-center justify-self-center cursor-pointer;
  color: var(--lad-text-inverse);
  border: 2px solid var(--lad-border-on-accent);
  border-radius: 10px;
  background: linear-gradient(
    145deg,
    var(--lad-color-info-subtle),
    var(--lad-color-info)
  );
  box-shadow: 0 2px 0 var(--lad-color-info-strong);
  font-size: 1.25rem;
  font-weight: var(--lad-font-weight-heavy);
  line-height: 1;
  transition:
    transform 0.14s ease,
    box-shadow 0.14s ease;
}
.amount-stepper button:last-child {
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-strong)
  );
  box-shadow: 0 2px 0 var(--lad-color-primary-deep);
}
.amount-stepper button:hover:not(:disabled) {
  transform: translateY(-1px) scale(1.04);
}
.amount-stepper button:active:not(:disabled) {
  transform: translateY(2px);
  box-shadow: none;
}
.amount-stepper button:disabled {
  cursor: default;
  filter: grayscale(0.45);
  opacity: 0.35;
}
.amount-stepper output {
  @apply d-flex align-center justify-center;
  gap: 5px;
  color: var(--lad-text);
  font-size: 0.9375rem;
  font-weight: var(--lad-font-weight-black);
}
.amount-stepper output :deep(.ladirchen-coin) {
  width: 20px;
  height: 20px;
}
.balance-grid,
.balance-detail-grid {
  gap: 8px;
}
.balance-tile {
  min-height: 58px;
  padding: 7px 8px;
  border-radius: 16px;
}
.balance-icon {
  width: 36px;
  height: 36px;
  border-radius: 12px;
}
.family-currency-value {
  min-height: 58px;
  padding: 6px 7px;
  grid-template-columns: 32px minmax(0, 1fr);
  border-radius: 16px;
}
.family-currency-value > .exchange-icon {
  width: 32px;
  scale: 0.7;
}
.family-currency-value strong {
  font-size: 0.6875rem;
}
.family-currency-value small {
  font-size: 0.40625rem;
}
.transfer-heading {
  @apply justify-start;
}
.transfer-fields {
  grid-template-columns: 1.2fr 0.9fr;
}
.transfer-field {
  padding: 6px;
}
.transfer-field > span {
  margin-bottom: 4px;
}
.transfer-input :deep(.v-field__input) {
  min-height: 36px;
  padding-inline: 8px;
  font-size: 0.6875rem;
}
.amount-stepper {
  height: 38px;
  grid-template-columns: 32px minmax(52px, 1fr) 32px;
}
.amount-stepper button {
  width: 28px;
  height: 28px;
  border-radius: 9px;
  font-size: 1.125rem;
}
.amount-stepper output {
  @apply text-no-wrap;
  gap: 4px;
}
.transfer-button {
  min-height: 40px;
}
.transfer-button--gift :deep(.v-icon) {
  font-size: 1.0625rem;
}
.setting-label {
  @apply mt-3 d-flex justify-space-between;
  color: var(--lad-muted);
  font-size: 0.75rem;
}
.setting-label strong {
  color: var(--lad-text);
}
@include respond-down(small) {
  .transfer-actions,
  .balance-grid,
  .currency-settings-fields {
    grid-template-columns: 1fr;
  }
  .guardian-balance-row {
    min-height: 58px;
    grid-template-columns:
      36px minmax(64px, 1fr) repeat(3, minmax(39px, 0.55fr))
      minmax(32px, 0.45fr);
    gap: 3px;
    padding-inline: 5px;
  }
  .guardian-child-avatar {
    width: 36px;
    height: 36px;
  }
  .guardian-child-name strong {
    font-size: 0.625rem;
  }
  .guardian-child-name small {
    font-size: 0.4375rem;
  }
  .guardian-child-stat {
    padding-left: 3px;
  }
  .guardian-child-stat small {
    font-size: 0.40625rem;
  }
  .guardian-child-stat strong {
    font-size: 0.5625rem;
  }
  .guardian-child-stat--level strong {
    font-size: 0.8125rem;
  }
}
@keyframes transfer-to-goal {
  0% {
    opacity: 0;
    transform: translateX(0) scale(0.5);
  }
  20% {
    opacity: 1;
  }
  80% {
    opacity: 1;
  }
  100% {
    opacity: 0;
    transform: translateX(43px) scale(1.08);
  }
}
@keyframes transfer-to-wallet {
  0% {
    left: auto;
    right: 4px;
    opacity: 0;
    transform: translateX(0) scale(0.5);
  }
  20% {
    opacity: 1;
  }
  80% {
    opacity: 1;
  }
  100% {
    left: auto;
    right: 4px;
    opacity: 0;
    transform: translateX(-43px) scale(1.08);
  }
}
@keyframes transfer-receive {
  0%,
  100% {
    transform: scale(1) rotate(0);
  }
  50% {
    transform: scale(1.16) rotate(6deg);
  }
}
@keyframes transfer-spark {
  0%,
  100% {
    opacity: 0.25;
    transform: scale(0.65) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(20deg);
  }
}
@keyframes money-path-float {
  0%,
  100% {
    transform: translateY(0) rotate(-2deg);
  }
  50% {
    transform: translateY(-2px) rotate(2deg);
  }
}
@keyframes member-avatar-hello {
  0%,
  70%,
  100% {
    transform: translateY(1px) rotate(0);
  }
  80% {
    transform: translateY(-2px) rotate(-3deg);
  }
  90% {
    transform: rotate(3deg);
  }
}
:global(.piggy-summary-dialog-frame) {
  width: min(460px, calc(100vw - 32px));
  height: min(660px, calc(100dvh - 32px));
  max-height: calc(100dvh - 32px);
}
@include reduced-motion {
  .transfer-card::after,
  .transfer-card .transfer-track i,
  .transfer-card.transfer-deposit .transfer-goal,
  .transfer-card.transfer-withdraw .transfer-wallet,
  .member-option-icon :deep(.avatar-figure),
  .transfer-title-icon {
    animation: none;
  }
}
</style>
