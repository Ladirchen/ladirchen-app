<template>
  <div class="prototype-background">
    <LadirchenIntro />

    <div class="prototype-shell">
      <header class="prototype-header">
        <RouterLink aria-label="Ladirchen Familienwelt" class="prototype-brand" to="/">
          <span class="prototype-logo" aria-hidden="true"><img alt="" src="/ladirchen-logo.png" /></span>
          <span class="prototype-wordmark">Ladirchen</span>
        </RouterLink>
        <div class="header-stats">
          <button class="header-stat header-streak" :aria-label="`Tagesserie öffnen: ${store.currentDailyStreak} geschaffte Tage`" aria-haspopup="dialog" type="button" @click="streakDialog = true">
            <AnimatedStreakFlame :size="25" /><strong>{{ store.currentDailyStreak }}</strong><span class="header-stat-label">Tage</span>
          </button>
          <button class="header-stat header-balance" :aria-label="`Guthaben von ${store.displayNameFor(store.activeChildId)} öffnen: ${store.availableBalance} verfügbare Ladirchen`" aria-haspopup="dialog" type="button" @click="store.piggyBankOpen = true">
            <LadirchenCoin animated small />
            <strong>{{ store.availableBalance }} L</strong>
          </button>
        </div>
      </header>

      <main class="prototype-content">
        <RouterView v-slot="{ Component, route }">
          <AnimatePresence :initial="false" mode="wait">
            <motion.div
              :key="route.path"
              class="prototype-route"
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

      <nav aria-label="Hauptnavigation" class="prototype-navigation">
        <RouterLink v-for="item in navigation" :key="item.to" :to="item.to">
          <AppNavigationIcon :name="item.icon" />
          <span>{{ item.label }}</span>
        </RouterLink>
      </nav>

      <Transition name="reward-flight">
        <div v-if="store.rewardAnimation.visible" :key="store.rewardAnimation.version" class="reward-animation" aria-live="polite">
          <div class="reward-value">+{{ store.rewardAnimation.value }}</div>
          <LadirchenCoin />
        </div>
      </Transition>
    </div>

    <v-snackbar v-model="store.snackbar.visible" color="secondary" location="bottom" rounded="lg" :timeout="2600">
      {{ store.snackbar.message }}
    </v-snackbar>

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
    <PrototypeSessionSwitcher v-if="isDevelopment" />
  </div>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { AnimatePresence, motion, useReducedMotion } from 'motion-v';

import FamilySetupDialog from './FamilySetupDialog.vue';
import AnimatedStreakFlame from './AnimatedStreakFlame.vue';
import AppNavigationIcon from './AppNavigationIcon.vue';
import LadirchenIntro from './LadirchenIntro.vue';
import LadirchenCoin from './LadirchenCoin.vue';
import PrototypeSessionSwitcher from './PrototypeSessionSwitcher.vue';
import SavingsPiggyDialog from './SavingsPiggyDialog.vue';
import WeeklyStreakDialog from './WeeklyStreakDialog.vue';
import { usePrototypeStore } from '../stores/prototype';

const store = usePrototypeStore();
const isDevelopment = import.meta.env.DEV;
const reducedMotion = useReducedMotion();
const streakDialog = ref(false);
const pageMotion = computed(() => reducedMotion.value ? {
  initial: { opacity: 1 }, animate: { opacity: 1 }, exit: { opacity: 1 }, transition: { duration: 0 },
} : {
  initial: { opacity: 0, y: 14, scale: .992 },
  animate: { opacity: 1, y: 0, scale: 1 },
  exit: { opacity: 0, y: -8, scale: .996 },
  transition: { duration: .22, ease: [.22, .8, .26, 1] },
});
const navigation = [
  { to: '/', icon: 'world', label: 'Unsere Welt' },
  { to: '/beitraege', icon: 'contributions', label: 'Beiträge' },
  { to: '/wuensche', icon: 'wishes', label: 'Wünsche' },
  { to: '/shop', icon: 'shop', label: 'Shop' },
  { to: '/familie', icon: 'family', label: 'Familie' },
  { to: '/ich', icon: 'profile', label: 'Ich' },
] as const;

</script>

<style src="../styles/prototype.scss"></style>

<style scoped>
.guardian-gift-card {
  position: relative;
  overflow: hidden;
  background: linear-gradient(160deg, #f3fff8, #fff7db 72%) !important;
  border: 1px solid rgba(62, 188, 140, 0.25);
}
.guardian-gift-card h2 {
  position: relative;
  margin: 0;
  font-size: 23px;
  letter-spacing: -0.035em;
}
.gift-coin {
  width: 82px;
  height: 82px;
  position: relative;
  z-index: 2;
  margin: 15px auto 0;
  display: grid;
  place-items: center;
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
  display: block;
  position: relative;
  margin-top: 12px;
  color: #279267;
  font-size: 35px;
  animation: gift-amount-pop 700ms 500ms both cubic-bezier(0.2, 0.9, 0.2, 1);
}
.gift-confetti span {
  position: absolute;
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
@media (prefers-reduced-motion: reduce) {
  .gift-coin,
  .gift-coin :deep(.ladirchen-coin),
  .gift-amount,
  .gift-confetti span {
    animation: none;
  }
}
</style>
