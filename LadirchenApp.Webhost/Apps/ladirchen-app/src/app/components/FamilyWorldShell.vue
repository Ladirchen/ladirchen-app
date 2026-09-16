<template>
  <div class="family-world-background">
    <AuthGateway v-if="!store.isAuthenticated" />
    <template v-else>
      <LadirchenIntro @finished="handleIntroFinished" />

      <div class="family-world-shell position-relative d-flex flex-column overflow-hidden mx-auto">
        <header class="family-world-header d-flex align-center justify-space-between">
          <RouterLink aria-label="Ladirchen Familienwelt" class="family-world-brand d-flex align-end text-decoration-none" to="/">
            <span class="family-world-logo position-relative flex-shrink-0 overflow-hidden" aria-hidden="true"><img alt="" src="/ladirchen-logo.png"></span>
            <span class="family-world-wordmark">Ladirchen</span>
          </RouterLink>
          <div class="header-stats d-grid align-center" :class="{ 'header-stats--guardian': store.viewerRole === 'guardian' }">
            <button v-if="store.viewerRole === 'guardian'" class="header-stat header-guardian-week d-flex align-center cursor-pointer" :aria-label="guardianWeekAriaLabel" aria-haspopup="dialog" data-ladi-heading="Wochenstatistik der Kinder" :data-ladi-tip="guardianWeekTip" type="button" @click="store.piggyBankOpen = true">
              <span class="header-stat-symbol"><LadirchenCoin animated small /></span>
              <span class="header-guardian-week-copy"><small>Diese Woche</small><strong>+{{ childrenWeekEarned }} L · {{ childrenWeekCompleted }} Aufgaben</strong><i>{{ childrenFamilyCurrency }} Familienvermögen · du {{ headerBalance }} L</i></span>
            </button>
            <template v-else>
              <button class="header-stat header-streak d-flex align-center justify-center cursor-pointer" :aria-label="`Tagesserie öffnen: ${store.currentDailyStreak} geschaffte Tage`" aria-haspopup="dialog" data-ladi-heading="Deine Tagesserie" :data-ladi-tip="`${store.currentDailyStreak} Tage hintereinander – stark! Jeder neue Tag lässt deine Serie weiterwachsen.`" type="button" @click="streakDialog = true">
                <span class="header-stat-symbol"><AnimatedStreakFlame :size="30" /></span><strong>{{ store.currentDailyStreak }}</strong><span class="header-stat-label">Tage</span>
              </button>
              <button class="header-stat header-balance d-flex align-center justify-center cursor-pointer" :aria-label="`Guthaben von ${store.displayNameFor(headerBalanceMemberId)} öffnen: ${headerBalance} Ladirchen`" aria-haspopup="dialog" data-ladi-heading="Dein Guthaben" :data-ladi-tip="`Du hast ${headerBalance} Ladirchen. Tippe hier, um die Geldübersicht anzusehen.`" type="button" @click="store.piggyBankOpen = true">
                <span class="header-stat-symbol"><LadirchenCoin animated small /></span>
                <strong>{{ headerBalance }} L</strong>
              </button>
            </template>
          </div>
        </header>

        <main class="family-world-content flex-grow-1 overflow-auto">
          <RouterView v-slot="{ Component, route }">
            <AnimatePresence :initial="false" mode="wait">
              <motion.div
                :key="route.path"
                class="family-world-route"
                :initial="pageMotion.initial"
                :animate="pageMotion.animate"
                :exit="pageMotion.exit"
                :transition="pageMotion.transition"
              >
                <component :is="Component" />
              </motion.div>
            </AnimatePresence>
          </RouterView>
        </main>

        <nav aria-label="Hauptnavigation" class="family-world-navigation position-absolute d-grid">
          <RouterLink v-for="item in navigation" :key="item.to" class="d-flex flex-column align-center justify-center text-decoration-none" :data-ladi-tip="item.tip" :to="item.to">
            <AppNavigationIcon :name="item.icon" />
            <span>{{ item.label }}</span>
          </RouterLink>
        </nav>

        <GlobalLadiGuide v-if="appHydrated && introFinished && store.viewerRole === 'child'" />
      </div>

      <v-snackbar v-model="store.snackbar.visible" color="secondary" location="bottom" rounded="lg" :timeout="2600">
        {{ t(store.snackbar.messageKey, store.snackbar.params) }}
      </v-snackbar>

      <v-dialog :model-value="store.rewardAnimation.visible" max-width="390" persistent>
        <v-card :key="store.rewardAnimation.version" class="contribution-reward-card pa-6 text-center" rounded="xl">
          <div class="reward-confetti" aria-hidden="true"><span>✦</span><span>★</span><span>●</span><span>◆</span><span>✧</span><span>★</span></div>
          <div class="reward-hero" :class="{ 'reward-hero--double': store.rewardAnimation.multiplier > 1 }" aria-hidden="true">
            <v-icon v-if="store.rewardAnimation.multiplier > 1" class="reward-rocket" icon="mdi-rocket-launch" />
            <LadirchenCoin animated />
            <strong v-if="store.rewardAnimation.multiplier > 1">×{{ store.rewardAnimation.multiplier }}</strong>
          </div>
          <p class="eyebrow reward-eyebrow mt-4 mb-1">Aufgabe bestätigt</p>
          <h2>Super gemacht!</h2>
          <p class="reward-task-title">{{ store.rewardAnimation.title }}</p>
          <div class="reward-results mt-4">
            <span class="reward-result reward-result--coins"><small>Du hast verdient</small><strong>{{ store.rewardAnimation.value }} Ladirchen</strong></span>
            <span v-if="store.rewardAnimation.energy > 0" class="reward-result reward-result--energy"><small>Für euer Haus</small><strong>+{{ store.rewardAnimation.energy }} Energie</strong></span>
          </div>
          <div v-if="store.rewardAnimation.stars > 0" class="reward-stars mt-4" :aria-label="`${store.rewardAnimation.stars} von 5 Sternen`">
            <v-icon v-for="star in 5" :key="star" :class="{ active: star <= store.rewardAnimation.stars }" icon="mdi-star" />
          </div>
          <p v-if="store.rewardAnimation.multiplier > 1" class="reward-double-copy mt-3"><v-icon icon="mdi-creation" size="16" />Doppelbonus geschafft!</p>
          <v-btn class="reward-dismiss mt-5" color="primary" rounded="lg" variant="flat" width="100%" @click="store.dismissRewardAnimation">Juhu!</v-btn>
        </v-card>
      </v-dialog>

      <v-dialog :model-value="store.guardianGiftAnimation.visible" max-width="390" persistent>
        <v-card :key="store.guardianGiftAnimation.version" class="guardian-gift-card pa-6 text-center" rounded="xl">
          <div class="gift-confetti" aria-hidden="true"><span>✦</span><span>●</span><span>★</span><span>◆</span><span>✦</span></div>
          <div class="gift-coin"><LadirchenCoin /></div>
          <p class="eyebrow mt-4 mb-1">Ein Geschenk für dich</p>
          <h2>{{ store.guardianGiftAnimation.guardianName }} schenkt dir Ladirchen!</h2>
          <strong class="gift-amount">+{{ store.guardianGiftAnimation.amount }} L</strong>
          <p v-if="store.guardianGiftAnimation.destination === 'goal'" class="text-body-small text-medium-emphasis mt-2">Die Ladirchen wurden direkt zu „{{ store.guardianGiftAnimation.goalTitle }}“ hinzugefügt.</p>
          <p v-else class="text-body-small text-medium-emphasis mt-2">Anlass: „{{ store.guardianGiftAnimation.goalTitle }}“. Die Ladirchen sind jetzt in deinem freien Guthaben.</p>
          <v-btn class="mt-5" color="primary" rounded="lg" variant="flat" width="100%" @click="store.dismissGuardianGift">Danke!</v-btn>
        </v-card>
      </v-dialog>

      <FamilySetupDialog />
      <SavingsPiggyDialog />
      <WeeklyStreakDialog v-model="streakDialog" />
    </template>
  </div>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { AnimatePresence, motion, useReducedMotion } from 'motion-v';
import { useI18n } from 'vue-i18n';

import AuthGateway from '@/features/auth/components/AuthGateway.vue';
import FamilySetupDialog from '@/features/family/components/FamilySetupDialog.vue';
import LadirchenIntro from '@/features/onboarding/components/LadirchenIntro.vue';
import SavingsPiggyDialog from '@/features/savings/components/SavingsPiggyDialog.vue';
import AnimatedStreakFlame from '@/features/streaks/components/AnimatedStreakFlame.vue';
import WeeklyStreakDialog from '@/features/streaks/components/WeeklyStreakDialog.vue';
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';

import AppNavigationIcon from './AppNavigationIcon.vue';
import GlobalLadiGuide from './GlobalLadiGuide.vue';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const { t } = useI18n();
const appHydrated = ref(false);
const introFinished = ref(false);
void store.hydrateFamilyAggregates().finally(() => { appHydrated.value = true; });
const reducedMotion = useReducedMotion();
const streakDialog = ref(false);
const headerBalanceMemberId = computed(() => store.viewerRole === 'guardian' ? store.signedInMemberId : store.activeChildId);
const headerBalance = computed(() => store.viewerRole === 'guardian' ? store.balanceFor(store.signedInMemberId) : store.availableBalance);
const childMembers = computed(() => store.members.filter(member => member.role === 'child'));
const weekStart = computed(() => {
  const now = new Date();
  const start = new Date(now);
  start.setHours(0, 0, 0, 0);
  start.setDate(now.getDate() - ((now.getDay() + 6) % 7));
  return start;
});
const childrenWeekContributions = computed(() => store.contributions.filter(contribution =>
  contribution.status === 'approved' &&
  contribution.approvedAt !== undefined &&
  new Date(contribution.approvedAt).getTime() >= weekStart.value.getTime() &&
  childMembers.value.some(child => child.id === contribution.assigneeId),
));
const childrenWeekEarned = computed(() => childrenWeekContributions.value.reduce((sum, contribution) =>
  sum + (contribution.earnedReward ?? contribution.reward) + (contribution.earnedRatingBonus ?? 0), 0));
const childrenWeekCompleted = computed(() => childrenWeekContributions.value.length);
const childrenTotalAssets = computed(() => childMembers.value.reduce((sum, child) => sum +
  store.balanceFor(child.id) +
  store.goals.filter(goal => goal.ownerId === child.id).reduce((goalSum, goal) => goalSum + goal.saved, 0), 0));
const childrenFamilyCurrency = computed(() => new Intl.NumberFormat('de-CH', {
  style: 'currency',
  currency: store.familyCurrencyCode,
  maximumFractionDigits: 2,
}).format(store.familyCurrencyValue(childrenTotalAssets.value)));
const guardianWeekAriaLabel = computed(() => `Diese Woche: ${childrenWeekEarned.value} Ladirchen aus ${childrenWeekCompleted.value} erledigten Aufgaben. Kindervermögen: ${childrenFamilyCurrency.value}. Eigenes Guthaben: ${headerBalance.value} Ladirchen.`);
const guardianWeekTip = computed(() => `Die Kinder haben diese Woche ${childrenWeekEarned.value} Ladirchen mit ${childrenWeekCompleted.value} erledigten Aufgaben verdient.`);
const pageMotion = computed(() => reducedMotion.value ? {
  initial: { opacity: 1 }, animate: { opacity: 1 }, exit: { opacity: 1 }, transition: { duration: 0 },
} : {
  initial: { opacity: 0, y: 14, scale: .992 },
  animate: { opacity: 1, y: 0, scale: 1 },
  exit: { opacity: 0, y: -8, scale: .996 },
  transition: { duration: .22, ease: [.22, .8, .26, 1] },
});
const navigationItems = [
  { to: '/', icon: 'world', label: 'Unsere Welt', tip: 'Zurück in eure Familienwelt – hier kannst du Haus und Garten entdecken.' },
  { to: '/beitraege', icon: 'contributions', label: 'Beiträge', tip: 'Bei den Beiträgen findest du Aufgaben, mit denen du eurer Familie helfen kannst.' },
  { to: '/wuensche', icon: 'wishes', label: 'Wünsche', tip: 'Hier warten eure Wünsche und gemeinsamen Sparziele.' },
  { to: '/shop', icon: 'shop', label: 'Shop', tip: 'Im Shop kannst du Ladirchen gegen Belohnungen oder Möbel tauschen.' },
  { to: '/familie', icon: 'family', label: 'Familie', tip: 'Hier siehst du deine Familie und eure gemeinsamen Fortschritte.' },
  { to: '/ich', icon: 'profile', label: 'Ich', tip: 'Das ist dein persönlicher Bereich mit Profil, Einstellungen und Ladi-Level.' },
] as const;
const navigation = computed(() => store.viewerRole === 'guardian' && !store.permissions.canManageContent
  ? navigationItems.filter(item => !['/beitraege', '/shop'].includes(item.to))
  : navigationItems,
);
const handleIntroFinished = () => {
  introFinished.value = true;
  store.revealNextContributionReward();
};

</script>

<style src="@/styles/family-world.scss"></style>

<style scoped>
.contribution-reward-card {
  @apply position-relative overflow-hidden;
  border: 2px solid rgba(76, 158, 185, 0.22);
  background:
    radial-gradient(
      circle at 88% 5%,
      rgba(255, 221, 102, 0.3),
      transparent 27%
    ),
    linear-gradient(155deg, #eef9ff, #f1fbf5 58%, #fff7dc) !important;
  box-shadow:
    0 9px 0 rgba(64, 127, 144, 0.15),
    0 28px 60px rgba(38, 68, 76, 0.25) !important;
}
.contribution-reward-card h2 {
  @apply position-relative ma-0;
  color: #273d48;
  font-size: 26px;
  letter-spacing: -0.04em;
}
.reward-eyebrow {
  color: #27856a;
}
.reward-task-title {
  margin: 4px 0 0;
  color: var(--lad-muted);
  font-size: 12px;
  font-weight: 750;
}
.reward-hero {
  width: 100px;
  height: 100px;
  margin: 14px auto 0;
  @apply position-relative d-grid place-center;
  border: 4px solid #fff;
  border-radius: 32px;
  background: linear-gradient(145deg, #dff6ff, #fff2b9);
  box-shadow:
    0 7px 0 rgba(57, 133, 150, 0.16),
    0 15px 28px rgba(57, 112, 122, 0.13);
  animation: reward-hero-arrive 0.75s cubic-bezier(0.2, 0.9, 0.25, 1) both;
}
.reward-hero :deep(.ladirchen-coin) {
  transform: scale(1.65);
  animation: reward-coin-spin 1.25s 0.45s ease-in-out;
}
.reward-hero--double {
  background:
    radial-gradient(circle at 25% 18%, #fff9d7, transparent 30%),
    linear-gradient(145deg, #74cde4, #687ed8 60%, #a978cf);
  box-shadow:
    0 7px 0 #586bb0,
    0 16px 30px rgba(77, 88, 153, 0.22);
}
.reward-hero > strong {
  min-width: 45px;
  height: 35px;
  padding: 4px 8px;
  @apply position-absolute d-grid place-center;
  right: -17px;
  bottom: -10px;
  color: #fff;
  border: 3px solid #fff;
  border-radius: 13px;
  background: linear-gradient(145deg, #55c5a1, #3187a1);
  box-shadow: 0 3px 0 #286f83;
  font-size: 19px;
  animation: reward-double-pop 0.7s 0.65s cubic-bezier(0.2, 0.9, 0.25, 1) both;
}
.reward-rocket {
  @apply position-absolute;
  top: -17px;
  left: -16px;
  z-index: 2;
  color: #fff5a9;
  font-size: 34px;
  filter: drop-shadow(0 3px 1px rgba(43, 65, 132, 0.3));
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
  border: 2px solid rgba(218, 155, 32, 0.22);
  border-radius: 17px;
  background: linear-gradient(145deg, #fff9e3, #ffeaa8);
  box-shadow: 0 4px 0 rgba(186, 123, 23, 0.14);
}
.reward-result--energy {
  border-color: rgba(57, 158, 111, 0.2);
  background: linear-gradient(145deg, #e9faf2, #fff3bd);
  box-shadow: 0 4px 0 rgba(46, 129, 92, 0.12);
}
.reward-result small,
.reward-result strong {
  @apply d-block;
}
.reward-result small {
  color: #7e6a3e;
  font-size: 8px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.025em;
}
.reward-result strong {
  margin-top: 3px;
  color: #8b590c;
  font-size: 17px;
  font-weight: 950;
  line-height: 1.08;
}
.reward-result--energy strong {
  color: #267154;
}
.reward-stars {
  @apply d-flex justify-center;
  gap: 2px;
}
.reward-stars :deep(.v-icon) {
  color: #d8e1dc;
  opacity: 0.58;
}
.reward-stars :deep(.v-icon.active) {
  color: #f0b62a;
  opacity: 1;
  filter: drop-shadow(0 2px 2px rgba(183, 119, 12, 0.25));
  animation: reward-star-pop 1.7s ease-in-out infinite;
}
.reward-double-copy {
  @apply d-flex align-center justify-center;
  gap: 5px;
  color: #347989;
  font-size: 11px;
  font-weight: 950;
}
.reward-dismiss {
  min-height: 49px !important;
  background: linear-gradient(145deg, #58c59a, #328f70) !important;
  box-shadow:
    0 5px 0 #287458,
    0 10px 18px rgba(40, 117, 89, 0.17) !important;
  font-weight: 950;
  text-transform: none;
  letter-spacing: 0;
}
.reward-confetti span {
  @apply position-absolute;
  z-index: 0;
  color: #e9ad2d;
  font-size: 16px;
  animation: reward-confetti 1.7s ease-in-out infinite;
}
.reward-confetti span:nth-child(1) {
  top: 8%;
  left: 12%;
}
.reward-confetti span:nth-child(2) {
  top: 21%;
  right: 10%;
  color: #7b91dc;
  animation-delay: -0.3s;
}
.reward-confetti span:nth-child(3) {
  top: 37%;
  left: 7%;
  color: #5dbf9b;
  animation-delay: -0.6s;
}
.reward-confetti span:nth-child(4) {
  top: 48%;
  right: 6%;
  color: #b982cb;
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
  color: #5fa7ce;
  animation-delay: -1.45s;
}
.guardian-gift-card {
  @apply position-relative overflow-hidden;
  background: linear-gradient(160deg, #f3fff8, #fff7db 72%) !important;
  border: 1px solid rgba(62, 188, 140, 0.25);
}
.guardian-gift-card h2 {
  @apply position-relative ma-0;
  font-size: 23px;
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
  background: rgba(255, 255, 255, 0.78);
  box-shadow: 0 10px 28px rgba(204, 144, 35, 0.2);
  animation: gift-coin-arrive 850ms cubic-bezier(0.2, 0.9, 0.2, 1);
}
.gift-coin :deep(.ladirchen-coin) {
  transform: scale(1.65);
  animation: gift-coin-spin 1.2s 700ms ease-in-out;
}
.gift-amount {
  @apply d-block position-relative mt-3;
  color: #279267;
  font-size: 35px;
  animation: gift-amount-pop 700ms 500ms both cubic-bezier(0.2, 0.9, 0.2, 1);
}
.gift-confetti span {
  @apply position-absolute;
  z-index: 1;
  color: #f2b443;
  font-size: 18px;
  animation: gift-confetti 1.8s infinite ease-in-out;
}
.gift-confetti span:nth-child(1) {
  top: 11%;
  left: 13%;
}
.gift-confetti span:nth-child(2) {
  top: 25%;
  right: 12%;
  color: #6f8df5;
  animation-delay: 0.2s;
}
.gift-confetti span:nth-child(3) {
  top: 8%;
  right: 30%;
  color: #eb748b;
  animation-delay: 0.45s;
}
.gift-confetti span:nth-child(4) {
  top: 41%;
  left: 8%;
  color: #43b98b;
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
@media (max-width: 390px) {
  .reward-results {
    grid-template-columns: 1fr;
  }
}
@media (prefers-reduced-motion: reduce) {
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
