<template>
  <div class="family-world-background">
    <AsyncAuthGateway v-if="!store.isAuthenticated" />
    <template v-else>
      <LadirchenIntro @finished="handleIntroFinished" />

      <div class="family-world-shell position-relative d-flex flex-column overflow-hidden mx-auto">
        <header class="family-world-header d-flex align-center justify-space-between">
          <RouterLink :aria-label="t('common.familyWorldAria')" class="family-world-brand d-flex align-end text-decoration-none" to="/">
            <span class="family-world-logo position-relative flex-shrink-0 overflow-hidden" aria-hidden="true"><img alt="" src="/ladirchen-logo.png"></span>
            <span class="family-world-wordmark">{{ t('common.appName') }}</span>
          </RouterLink>
          <div class="header-stats d-grid align-center" :class="{ 'header-stats--guardian': store.viewerRole === 'guardian' }">
            <button v-if="store.viewerRole === 'guardian'" class="header-stat header-guardian-week d-flex align-center cursor-pointer" :aria-label="guardianWeekAriaLabel" aria-haspopup="dialog" :data-ladi-heading="t('shell.week.heading')" :data-ladi-tip="guardianWeekTip" type="button" @click="store.piggyBankOpen = true">
              <span class="header-stat-symbol"><LadirchenCoin animated small /></span>
              <span class="header-guardian-week-copy"><small>{{ t('shell.week.label') }}</small><strong>{{ t('shell.week.summary', { earned: childrenWeekEarned, completed: childrenWeekCompleted }) }}</strong><i>{{ t('shell.week.assets', { assets: childrenFamilyCurrency, balance: headerBalance }) }}</i></span>
            </button>
            <template v-else>
              <button class="header-stat header-streak d-flex align-center justify-center cursor-pointer" :aria-label="t('shell.streak.aria', { days: store.currentDailyStreak })" aria-haspopup="dialog" :data-ladi-heading="t('shell.streak.heading')" :data-ladi-tip="t('shell.streak.tip', { days: store.currentDailyStreak })" type="button" @click="streakDialog = true">
                <span class="header-stat-symbol"><AnimatedStreakFlame :size="30" /></span><strong>{{ store.currentDailyStreak }}</strong><span class="header-stat-label">{{ t('shell.streak.days') }}</span>
              </button>
              <button class="header-stat header-balance d-flex align-center justify-center cursor-pointer" :aria-label="t('shell.balance.aria', { name: store.displayNameFor(headerBalanceMemberId), balance: headerBalance })" aria-haspopup="dialog" :data-ladi-heading="t('shell.balance.heading')" :data-ladi-tip="t('shell.balance.tip', { balance: headerBalance })" type="button" @click="store.piggyBankOpen = true">
                <span class="header-stat-symbol"><LadirchenCoin animated small /></span>
                <strong>{{ headerBalance }}</strong>
              </button>
            </template>
          </div>
        </header>

        <main ref="contentElement" class="family-world-content flex-grow-1 overflow-auto">
          <RouterView v-slot="{ Component, route }">
            <Transition name="family-world-page" mode="out-in">
              <div
                :key="route.path"
                class="family-world-route"
              >
                <component :is="Component" />
              </div>
            </Transition>
          </RouterView>
        </main>

        <nav :aria-label="t('common.mainNavigation')" class="family-world-navigation position-absolute d-grid">
          <RouterLink v-for="item in navigation" :key="item.to" class="d-flex flex-column align-center justify-center text-decoration-none" :class="`navigation-tone--${item.icon}`" :data-ladi-tip="item.tip" :to="item.to">
            <AppNavigationIcon :name="item.icon" />
            <span>{{ item.label }}</span>
          </RouterLink>
        </nav>

        <AsyncGlobalLadiGuide v-if="appHydrated && introFinished && store.viewerRole === 'child'" />
      </div>

      <v-snackbar v-model="store.snackbar.visible" color="secondary" location="bottom" rounded="lg" :timeout="2600">
        {{ store.snackbar.messageKey ? t(store.snackbar.messageKey, store.snackbar.params) : '' }}
      </v-snackbar>

      <v-dialog :model-value="store.rewardAnimation.visible" max-width="390" persistent>
        <v-card :key="store.rewardAnimation.version" class="contribution-reward-card pa-6 text-center" rounded="xl">
          <div class="reward-confetti" aria-hidden="true"><span>✦</span><span>★</span><span>●</span><span>◆</span><span>✧</span><span>★</span></div>
          <div class="reward-hero" :class="{ 'reward-hero--double': store.rewardAnimation.multiplier > 1 }" aria-hidden="true">
            <v-icon v-if="store.rewardAnimation.multiplier > 1" class="reward-rocket" icon="i-mdi:rocket-launch" />
            <LadirchenCoin animated />
            <strong v-if="store.rewardAnimation.multiplier > 1">×{{ store.rewardAnimation.multiplier }}</strong>
          </div>
          <p class="eyebrow reward-eyebrow mt-4 mb-1">{{ t('shell.reward.confirmed') }}</p>
          <h2>{{ t('shell.reward.success') }}</h2>
          <p class="reward-task-title">{{ store.rewardAnimation.title }}</p>
          <div class="reward-results mt-4">
            <span class="reward-result reward-result--coins"><small>{{ t('shell.reward.earned') }}</small><strong>{{ t('shell.reward.coins', { value: store.rewardAnimation.value }) }}</strong></span>
            <span v-if="store.rewardAnimation.energy > 0" class="reward-result reward-result--energy"><small>{{ t('shell.reward.house') }}</small><strong>{{ t('shell.reward.energy', { value: store.rewardAnimation.energy }) }}</strong></span>
          </div>
          <div v-if="store.rewardAnimation.stars > 0" class="reward-stars mt-4" :aria-label="t('shell.reward.stars', { value: store.rewardAnimation.stars })">
            <v-icon v-for="star in 5" :key="star" :class="{ active: star <= store.rewardAnimation.stars }" icon="i-mdi:star" />
          </div>
          <p v-if="store.rewardAnimation.multiplier > 1" class="reward-double-copy mt-3"><v-icon icon="i-mdi:creation" size="16" />{{ t('shell.reward.double') }}</p>
          <v-btn class="reward-dismiss mt-5" color="primary" rounded="lg" variant="flat" width="100%" @click="store.dismissRewardAnimation">{{ t('shell.reward.dismiss') }}</v-btn>
        </v-card>
      </v-dialog>

      <v-dialog :model-value="store.guardianGiftAnimation.visible" max-width="390" persistent>
        <v-card :key="store.guardianGiftAnimation.version" class="guardian-gift-card pa-6 text-center" rounded="xl">
          <div class="gift-confetti" aria-hidden="true"><span>✦</span><span>●</span><span>★</span><span>◆</span><span>✦</span></div>
          <div class="gift-coin"><LadirchenCoin /></div>
          <p class="eyebrow mt-4 mb-1">{{ t('shell.gift.eyebrow') }}</p>
          <h2>{{ t('shell.gift.title', { guardian: store.guardianGiftAnimation.guardianName }) }}</h2>
          <strong class="gift-amount">+{{ store.guardianGiftAnimation.amount }}</strong>
          <p v-if="store.guardianGiftAnimation.destination === 'goal'" class="text-body-small text-medium-emphasis mt-2">{{ t('shell.gift.goal', { goal: store.guardianGiftAnimation.goalTitle }) }}</p>
          <p v-else class="text-body-small text-medium-emphasis mt-2">{{ store.guardianGiftAnimation.goalTitle
            ? t('shell.gift.balance', { goal: store.guardianGiftAnimation.goalTitle })
            : t('shell.gift.balanceDefault') }}</p>
          <v-btn class="mt-5" color="primary" rounded="lg" variant="flat" width="100%" @click="store.dismissGuardianGift">{{ t('shell.gift.dismiss') }}</v-btn>
        </v-card>
      </v-dialog>

      <AsyncFamilySetupDialog v-if="store.familySetupOpen" />
      <AsyncSavingsPiggyDialog v-if="store.piggyBankOpen" />
      <AsyncWeeklyStreakDialog v-if="streakDialog" v-model="streakDialog" />
    </template>
  </div>
</template>

<script lang="ts" setup>
import { computed, defineAsyncComponent, onBeforeUnmount, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import LadirchenIntro from '@/features/onboarding/components/LadirchenIntro.vue';
import AnimatedStreakFlame from '@/features/streaks/components/AnimatedStreakFlame.vue';
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';

import AppNavigationIcon from './AppNavigationIcon.vue';
import { useFamilyWorldStore } from '@/stores/family-world';
import { CURRENT_TIME_REFRESH_INTERVAL_MS } from '@/shared/runtime-timing';
import { isInstantInIsoWeek } from '@/domain/shared/zoned-calendar';

const AsyncAuthGateway = defineAsyncComponent(() => import('@/features/auth/components/AuthGateway.vue'));
const AsyncFamilySetupDialog = defineAsyncComponent(() => import('@/features/family/components/FamilySetupDialog.vue'));
const AsyncGlobalLadiGuide = defineAsyncComponent(() => import('./GlobalLadiGuide.vue'));
const AsyncSavingsPiggyDialog = defineAsyncComponent(() => import('@/features/savings/components/SavingsPiggyDialog.vue'));
const AsyncWeeklyStreakDialog = defineAsyncComponent(() => import('@/features/streaks/components/WeeklyStreakDialog.vue'));

const store = useFamilyWorldStore();
const { locale, t } = useI18n();
const appHydrated = ref(false);
const introFinished = ref(false);
let clockTimer: ReturnType<typeof window.setInterval> | undefined;
let orientationMedia: MediaQueryList | undefined;
const contentElement = ref<HTMLElement | null>(null);
void store.hydrateFamilyAggregates().finally(() => { appHydrated.value = true; });
const streakDialog = ref(false);
const headerBalanceMemberId = computed(() => store.viewerRole === 'guardian' ? store.signedInMemberId : store.activeChildId);
const headerBalance = computed(() => store.viewerRole === 'guardian' ? store.balanceFor(store.signedInMemberId) : store.availableBalance);
const childMembers = computed(() => store.members.filter(member => member.role === 'child'));
const childrenWeekContributions = computed(() => store.contributions.filter(contribution =>
  contribution.status === 'approved' &&
  contribution.approvedAt !== undefined &&
  isInstantInIsoWeek(contribution.approvedAt, new Date(store.currentTimeMilliseconds), store.familyTimeZone) &&
  childMembers.value.some(child => child.id === contribution.assigneeId),
));
const childrenWeekEarned = computed(() => childrenWeekContributions.value.reduce((sum, contribution) =>
  sum + (contribution.earnedReward ?? contribution.reward) + (contribution.earnedRatingBonus ?? 0), 0));
const childrenWeekCompleted = computed(() => childrenWeekContributions.value.length);
const childrenTotalAssets = computed(() => childMembers.value.reduce((sum, child) => sum +
  store.balanceFor(child.id) +
  store.goals.filter(goal => goal.ownerId === child.id).reduce((goalSum, goal) => goalSum + goal.saved, 0), 0));
const childrenFamilyCurrency = computed(() => new Intl.NumberFormat(locale.value, {
  style: 'currency',
  currency: store.familyCurrencyCode,
  maximumFractionDigits: 2,
}).format(store.familyCurrencyValue(childrenTotalAssets.value)));
const guardianWeekAriaLabel = computed(() => t('shell.week.aria', { earned: childrenWeekEarned.value, completed: childrenWeekCompleted.value, assets: childrenFamilyCurrency.value, balance: headerBalance.value }));
const guardianWeekTip = computed(() => t('shell.week.tip', { earned: childrenWeekEarned.value, completed: childrenWeekCompleted.value }));
type NavigationIcon = 'family' | 'contributions' | 'profile' | 'world' | 'wishes' | 'shop';
interface NavigationItem { to: string; icon: NavigationIcon; label: string; tip: string }

const navigationItems = computed<NavigationItem[]>(() => [
  { to: '/', icon: 'world', label: t('navigation.world.label'), tip: t('navigation.world.tip') },
  { to: '/beitraege', icon: 'contributions', label: t('navigation.contributions.label'), tip: t('navigation.contributions.tip') },
  { to: '/wuensche', icon: 'wishes', label: t('navigation.wishes.label'), tip: t('navigation.wishes.tip') },
  { to: '/shop', icon: 'shop', label: t('navigation.shop.label'), tip: t('navigation.shop.tip') },
  { to: '/familie', icon: 'family', label: t('navigation.family.label'), tip: t('navigation.family.tip') },
  { to: '/ich', icon: 'profile', label: t('navigation.profile.label'), tip: t('navigation.profile.tip') },
]);
const navigation = computed(() => store.viewerRole === 'guardian' && !store.permissions.canManageContent
  ? navigationItems.value.filter(item => !['/beitraege', '/shop'].includes(item.to))
  : navigationItems.value,
);
const handleIntroFinished = () => {
  introFinished.value = true;
  store.revealNextContributionReward();
};
const resetContentScroll = () => {
  window.requestAnimationFrame(() => contentElement.value?.scrollTo({ top: 0, left: 0 }));
};
onMounted(() => {
  store.refreshCurrentTime();
  clockTimer = window.setInterval(() => store.refreshCurrentTime(), CURRENT_TIME_REFRESH_INTERVAL_MS);
  orientationMedia = window.matchMedia('(orientation: landscape)');
  orientationMedia.addEventListener('change', resetContentScroll);
});
onBeforeUnmount(() => {
  if (clockTimer !== undefined) {window.clearInterval(clockTimer);}
  orientationMedia?.removeEventListener('change', resetContentScroll);
});

</script>

<style src="@/styles/family-world.scss"></style>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.family-world-page-enter-active,
.family-world-page-leave-active {
  transition:
    opacity 0.22s ease,
    transform 0.22s cubic-bezier(0.22, 0.8, 0.26, 1);
}
.family-world-page-enter-from {
  opacity: 0;
  transform: translateY(14px) scale(0.992);
}
.family-world-page-leave-to {
  opacity: 0;
  transform: translateY(-8px) scale(0.996);
}
.contribution-reward-card {
  @apply position-relative overflow-hidden;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-info-muted) 20%, transparent);
  background:
    radial-gradient(
      circle at 88% 5%,
      color-mix(in srgb, var(--lad-color-reward) 30%, transparent),
      transparent 27%
    ),
    linear-gradient(
      155deg,
      var(--lad-surface-soft),
      var(--lad-surface-soft) 58%,
      var(--lad-color-reward-soft)
    );
  box-shadow:
    0 9px 0 color-mix(in srgb, var(--lad-color-info-shadow) 15%, transparent),
    0 28px 60px color-mix(in srgb, var(--lad-text) 25%, transparent);
}
.contribution-reward-card h2 {
  @apply position-relative ma-0;
  color: var(--lad-text);
  font-size: rem(26);
  letter-spacing: -0.04em;
}
.reward-eyebrow {
  color: var(--lad-color-primary-strong);
}
.reward-task-title {
  margin: 4px 0 0;
  color: var(--lad-muted);
  font-size: 0.75rem;
  font-weight: 750;
}
.reward-hero {
  width: 100px;
  height: 100px;
  margin: 14px auto 0;
  @apply position-relative d-grid place-center;
  border: 4px solid var(--lad-border-on-accent);
  border-radius: 32px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-color-info-shadow) 15%, transparent),
    0 15px 28px color-mix(in srgb, var(--lad-color-info-deep) 12%, transparent);
  animation: reward-hero-arrive 0.75s cubic-bezier(0.2, 0.9, 0.25, 1) both;
}
.reward-hero :deep(.ladirchen-coin) {
  transform: scale(1.65);
  animation: reward-coin-spin 1.25s 0.45s ease-in-out;
}
.reward-hero--double {
  background:
    radial-gradient(
      circle at 25% 18%,
      var(--lad-color-reward-soft),
      transparent 30%
    ),
    linear-gradient(
      145deg,
      var(--lad-color-info-subtle),
      var(--lad-color-bonus-info) 60%,
      var(--lad-color-bonus-highlight)
    );
  box-shadow:
    0 7px 0 var(--lad-color-bonus-muted),
    0 16px 30px color-mix(in srgb, var(--lad-color-info-deep) 20%, transparent);
}
.reward-hero > strong {
  min-width: 45px;
  height: 35px;
  padding: 4px 8px;
  @apply position-absolute d-grid place-center;
  right: -17px;
  bottom: -10px;
  color: var(--lad-text-inverse);
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 13px;
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-info-shadow)
  );
  box-shadow: 0 3px 0 var(--lad-color-info-deep);
  font-size: rem(19);
  animation: reward-double-pop 0.7s 0.65s cubic-bezier(0.2, 0.9, 0.25, 1) both;
}
.reward-rocket {
  @apply position-absolute;
  top: -17px;
  left: -16px;
  z-index: 2;
  color: var(--lad-color-reward-pale);
  font-size: rem(34);
  filter: drop-shadow(
    0 3px 1px
      color-mix(in srgb, var(--lad-color-bonus-info-strong) 30%, transparent)
  );
  animation: reward-rocket-flight 1.8s 0.35s ease-in-out infinite;
}
.reward-results {
  @apply d-grid;
  grid-template-columns: 1.25fr 1fr;
  gap: 8px;
}
.reward-result {
  min-height: 68px;
  padding: 9px;
  @apply d-grid place-center;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-reward-accent) 20%, transparent);
  border-radius: 17px;
  background: linear-gradient(
    145deg,
    var(--lad-color-reward-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-reward-deep) 15%, transparent);
}
.reward-result--energy {
  border-color: color-mix(
    in srgb,
    var(--lad-color-primary-muted) 20%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-primary-deep) 12%, transparent);
}
.reward-result small,
.reward-result strong {
  @apply d-block;
}
.reward-result small {
  color: var(--lad-color-accent-warm-strong);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: uppercase;
  letter-spacing: 0.025em;
}
.reward-result strong {
  margin-top: 3px;
  color: var(--lad-color-reward-strong);
  font-size: rem(17);
  font-weight: var(--lad-font-weight-black);
  line-height: 1.08;
}
.reward-result--energy strong {
  color: var(--lad-color-primary-deep);
}
.reward-stars {
  @apply d-flex justify-center;
  gap: 2px;
}
.reward-stars :deep(.v-icon) {
  color: var(--lad-color-primary-soft);
  opacity: 0.58;
}
.reward-stars :deep(.v-icon.active) {
  color: var(--lad-color-reward-border);
  opacity: 1;
  filter: drop-shadow(
    0 2px 2px color-mix(in srgb, var(--lad-color-reward-deep) 25%, transparent)
  );
  animation: reward-star-pop 1.7s ease-in-out infinite;
}
.reward-double-copy {
  @apply d-flex align-center justify-center;
  gap: 5px;
  color: var(--lad-color-info-deep);
  font-size: rem(11);
  font-weight: var(--lad-font-weight-black);
}
.reward-dismiss {
  min-height: 49px;
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-strong)
  );
  box-shadow:
    0 5px 0 var(--lad-color-primary-deep),
    0 10px 18px
      color-mix(in srgb, var(--lad-color-primary-deep) 18%, transparent);
  font-weight: var(--lad-font-weight-black);
  text-transform: none;
  letter-spacing: 0;
}
.reward-confetti span {
  @apply position-absolute;
  z-index: 0;
  color: var(--lad-color-reward-border);
  font-size: 1rem;
  animation: reward-confetti 1.7s ease-in-out infinite;
}
.reward-confetti span:nth-child(1) {
  top: 8%;
  left: 12%;
}
.reward-confetti span:nth-child(2) {
  top: 21%;
  right: 10%;
  color: var(--lad-color-bonus-info);
  animation-delay: -0.3s;
}
.reward-confetti span:nth-child(3) {
  top: 37%;
  left: 7%;
  color: var(--lad-color-primary-highlight);
  animation-delay: -0.6s;
}
.reward-confetti span:nth-child(4) {
  top: 48%;
  right: 6%;
  color: var(--lad-color-bonus-highlight);
  animation-delay: -0.9s;
}
.reward-confetti span:nth-child(5) {
  top: 68%;
  left: 11%;
  animation-delay: -1.2s;
}
.reward-confetti span:nth-child(6) {
  top: 74%;
  right: 12%;
  color: var(--lad-color-info-muted);
  animation-delay: -1.45s;
}
.guardian-gift-card {
  @apply position-relative overflow-hidden;
  background: linear-gradient(
    160deg,
    var(--lad-surface),
    var(--lad-color-reward-soft) 72%
  );
  border: 1px solid
    color-mix(in srgb, var(--lad-color-primary) 25%, transparent);
}
.guardian-gift-card h2 {
  @apply position-relative ma-0;
  font-size: rem(23);
  letter-spacing: -0.035em;
}
.gift-coin {
  width: 82px;
  height: 82px;
  @apply position-relative;
  z-index: 2;
  margin: 15px auto 0;
  @apply d-grid place-center;
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-surface-raised) 80%, transparent);
  box-shadow: 0 10px 28px
    color-mix(in srgb, var(--lad-color-reward-shadow) 20%, transparent);
  animation: gift-coin-arrive 850ms var(--lad-easing-pop);
}
.gift-coin :deep(.ladirchen-coin) {
  transform: scale(1.65);
  animation: gift-coin-spin 1.2s 700ms ease-in-out;
}
.gift-amount {
  @apply d-block position-relative mt-3;
  color: var(--lad-color-primary-strong);
  font-size: rem(35);
  animation: gift-amount-pop 700ms 500ms both var(--lad-easing-pop);
}
.gift-confetti span {
  @apply position-absolute;
  z-index: 1;
  color: var(--lad-color-reward-border);
  font-size: rem(18);
  animation: gift-confetti 1.8s infinite ease-in-out;
}
.gift-confetti span:nth-child(1) {
  top: 11%;
  left: 13%;
}
.gift-confetti span:nth-child(2) {
  top: 25%;
  right: 12%;
  color: var(--lad-color-bonus-info);
  animation-delay: 0.2s;
}
.gift-confetti span:nth-child(3) {
  top: 8%;
  right: 30%;
  color: var(--lad-color-danger-soft);
  animation-delay: 0.45s;
}
.gift-confetti span:nth-child(4) {
  top: 41%;
  left: 8%;
  color: var(--lad-color-primary);
  animation-delay: 0.7s;
}
.gift-confetti span:nth-child(5) {
  top: 49%;
  right: 7%;
  animation-delay: 0.9s;
}
@keyframes gift-coin-arrive {
  from {
    opacity: 0;
    transform: translateY(-70px) scale(0.4) rotate(-180deg);
  }
  70% {
    transform: translateY(8px) scale(1.12) rotate(12deg);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes gift-coin-spin {
  to {
    transform: scale(1.65) rotateY(720deg);
  }
}
@keyframes gift-amount-pop {
  from {
    opacity: 0;
    transform: scale(0.4);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}
@keyframes gift-confetti {
  0%,
  100% {
    opacity: 0.35;
    transform: translateY(0) rotate(0);
  }
  50% {
    opacity: 1;
    transform: translateY(7px) rotate(25deg);
  }
}
@keyframes reward-hero-arrive {
  from {
    opacity: 0;
    transform: translateY(-55px) scale(0.45) rotate(-14deg);
  }
  72% {
    transform: translateY(6px) scale(1.1) rotate(4deg);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes reward-coin-spin {
  to {
    transform: scale(1.65) rotateY(720deg);
  }
}
@keyframes reward-double-pop {
  from {
    opacity: 0;
    transform: scale(0.3) rotate(-14deg);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes reward-rocket-flight {
  0%,
  65%,
  100% {
    transform: translate(0, 0) rotate(-8deg);
  }
  75% {
    transform: translate(5px, -7px) rotate(5deg) scale(1.12);
  }
  87% {
    transform: translate(-1px, 1px) rotate(-4deg);
  }
}
@keyframes reward-star-pop {
  0%,
  100% {
    transform: scale(0.9) rotate(-4deg);
  }
  50% {
    transform: scale(1.15) rotate(5deg);
  }
}
@keyframes reward-confetti {
  0%,
  100% {
    opacity: 0.25;
    transform: translateY(0) rotate(0) scale(0.7);
  }
  50% {
    opacity: 1;
    transform: translateY(7px) rotate(25deg) scale(1.15);
  }
}
@include respond-down(narrow) {
  .reward-results {
    grid-template-columns: 1fr;
  }
}
@include reduced-motion {
  .family-world-page-enter-active,
  .family-world-page-leave-active {
    transition: none;
  }
  .gift-coin,
  .gift-coin :deep(.ladirchen-coin),
  .gift-amount,
  .gift-confetti span,
  .reward-hero,
  .reward-hero :deep(.ladirchen-coin),
  .reward-hero > strong,
  .reward-rocket,
  .reward-stars :deep(.v-icon.active),
  .reward-confetti span {
    animation: none;
  }
}
</style>
