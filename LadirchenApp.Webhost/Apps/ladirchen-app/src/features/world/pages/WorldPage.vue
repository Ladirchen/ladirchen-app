<template>
  <div class="page world-page">
    <section class="world-hero px-4 pt-5">
      <div class="d-flex align-start justify-space-between ga-3">
        <div v-if="store.viewerRole === 'child'">
          <p class="eyebrow">{{ store.activeChild.name }}s Familienwelt</p>
          <h1 class="world-title">Jeder Beitrag macht unser Zuhause lebendiger.</h1>
        </div>
        <div v-else>
          <p class="eyebrow">Bezugspersonen-Übersicht</p>
          <h1 class="world-title">So geht es allen Kindern heute.</h1>
        </div>
        <button :aria-label="`Hausenergie öffnen: ${store.familyEnergy} Prozent`" class="energy-trigger text-right flex-shrink-0" type="button" @click="energyDialogOpen = true">
          <AnimatedHouseEnergy :size="43" />
          <span class="energy-trigger-copy">
            <strong class="energy-value">{{ store.familyEnergy }} %</strong>
            <span class="energy-label">Hausenergie</span>
          </span>
          <v-icon class="energy-chevron" icon="mdi-chevron-right" size="15" />
        </button>
      </div>

      <FamilyWorldScene
        :accessories="store.accessories"
        :effects="store.activeWorldEffects"
        :energy="store.familyEnergy"
        :house-level="store.houseLevel"
        :house-layout="store.houseLayout"
        :house-theme-id="store.houseThemeId"
        :ladi-score="store.averageTaskRating"
        :members="store.members"
        :pets="store.pets"
        :rooms="store.unlockedHouseRooms"
        :can-arrange-house="store.canArrangeHouse"
        :reveal-version="store.revealVersion"
        :viewer-member-id="store.signedInMemberId"
        @move-entity="store.moveHouseEntity"
        @place-furniture="store.placeStoredHouseAccessory"
        @reset-entity="store.resetHouseEntityPosition"
        @store-furniture="store.storeHouseAccessory"
      />
    </section>

    <div class="page-padding pt-5">
      <template v-if="store.viewerRole === 'guardian' && store.permissions.canManageContent">
        <section class="guardian-world mb-6">
          <SectionHeader eyebrow="Heute" title="Kinder im Überblick">
            <template #action><span class="rating-rule">Hausziel: mindestens 60 % Energie</span></template>
          </SectionHeader>
          <div class="guardian-child-grid">
            <button
              v-for="child in childMembers"
              :key="child.id"
              class="guardian-child"
              :class="{ selected: child.id === store.activeChildId }"
              type="button"
              @click="store.selectChildForGuardian(child.id)"
            >
              <span class="guardian-child-avatar">{{ child.avatar }}</span>
              <span class="guardian-child-copy"><strong>{{ child.name }}</strong><small>{{ store.contributionProgress(child.id) }} % Energie</small></span>
              <span class="guardian-child-rating" :class="{ ready: store.contributionProgress(child.id) >= 60 }">{{ ratingLabel(child.id) }}</span>
            </button>
          </div>
        </section>

        <v-card class="management-card pa-4 mb-6" color="blue-lighten-5" elevation="0" rounded="xl">
          <div class="d-flex align-center justify-space-between ga-3 mb-3">
            <div><strong>Direkt verwalten</strong><p class="text-caption text-medium-emphasis">Neue Inhalte für {{ store.activeChild.name }} oder die Familie anlegen.</p></div>
            <v-icon color="info">mdi-shield-account-outline</v-icon>
          </div>
          <div class="management-actions">
            <v-btn color="primary" prepend-icon="mdi-plus" rounded="lg" size="small" to="/beitraege?new=1" variant="flat">Beitrag</v-btn>
            <v-btn color="info" prepend-icon="mdi-target" rounded="lg" size="small" to="/wuensche?new=1" variant="tonal">Sparziel</v-btn>
            <v-btn color="warning" prepend-icon="mdi-gift-outline" rounded="lg" size="small" to="/shop?new=1" variant="tonal">Shop-Element</v-btn>
            <v-btn color="secondary" prepend-icon="mdi-hand-coin-outline" rounded="lg" size="small" variant="tonal" @click="openGiftDialog">Ladirchen schenken</v-btn>
          </div>
        </v-card>

        <v-card class="energy-simulator pa-4 mb-6" elevation="0" rounded="xl">
          <div class="d-flex align-center justify-space-between ga-3">
            <div><p class="eyebrow mb-1">Frontend-Prototyp</p><strong>Energie-Level simulieren</strong></div>
            <strong class="simulated-value">{{ store.familyEnergy }} %</strong>
          </div>
          <v-slider
            class="mt-3"
            color="primary"
            hide-details
            :model-value="store.familyEnergy"
            step="10"
            thumb-label
            @update:model-value="store.setSimulatedEnergy(Number($event))"
          />
          <div class="d-flex align-center justify-space-between mt-2 ga-2">
            <span class="text-caption text-medium-emphasis">Verändert nur die Darstellung im Prototyp.</span>
            <v-btn v-if="store.simulatedEnergy !== null" rounded="lg" size="x-small" variant="text" @click="store.setSimulatedEnergy(null)">Zurücksetzen</v-btn>
          </div>
        </v-card>
      </template>

      <v-card v-else-if="store.viewerRole === 'guardian'" class="supporter-card pa-4 mb-6" elevation="0" rounded="xl">
        <div class="d-flex align-center ga-3">
          <v-avatar color="primary" size="46" variant="tonal"><v-icon icon="mdi-hand-heart-outline" /></v-avatar>
          <div class="flex-grow-1 min-w-0">
            <p class="eyebrow mb-1">Zielbegleitung</p>
            <strong>Du siehst freigegebene Kinderziele</strong>
            <p class="text-caption text-medium-emphasis mt-1">Familienziele und Verwaltungsbereiche bleiben für Administratoren geschützt.</p>
          </div>
        </div>
        <v-btn class="mt-4" color="primary" prepend-icon="mdi-gift-outline" rounded="lg" to="/wuensche" variant="flat" width="100%">Kinderziele unterstützen</v-btn>
      </v-card>

      <v-card v-if="store.viewerRole === 'child' && activePromotion" class="promotion-banner pa-4 mb-6" color="amber-lighten-5" elevation="0" rounded="xl" role="button" tabindex="0" @click="promoDetailsOpen = true" @keydown.enter="promoDetailsOpen = true">
        <div class="d-flex align-center ga-3">
          <div class="promotion-gem" aria-hidden="true"><v-icon icon="mdi-diamond-stone" size="27" /><i>✦</i><i>✧</i></div>
          <div class="flex-grow-1">
            <p class="eyebrow mb-1">Bonusaktion</p>
            <strong>{{ activePromotion.title }}</strong>
            <p class="text-caption text-medium-emphasis">{{ contributionTitle(activePromotion.contributionId) }} bis {{ activePromotion.deadline }} Uhr</p>
          </div>
          <v-chip color="warning" size="small">×{{ activePromotion.multiplier }}</v-chip>
        </div>
      </v-card>

      <section v-if="store.viewerRole === 'child'" class="contribution-board">
        <div class="contribution-board-heading">
          <div>
            <p class="eyebrow mb-1">Heute für dich</p>
            <h2>Meine Aufgaben</h2>
            <p>Bestätigte Grundbeiträge verändern eure Familienwelt.</p>
          </div>
          <RouterLink class="all-contributions" to="/beitraege"><v-icon icon="mdi-view-grid-plus-outline" size="19" /><span>Alle Beiträge</span><v-icon icon="mdi-chevron-right" size="17" /></RouterLink>
        </div>

        <div v-if="personalContributions.length" class="contribution-list">
          <v-card v-for="contribution in personalContributions" :key="contribution.id" class="contribution-card pa-3" elevation="0" rounded="xl">
            <div class="d-flex align-center ga-3">
              <div class="contribution-icon">{{ contribution.icon }}</div>
              <div class="flex-grow-1 min-w-0">
                <strong class="d-block text-body-small">{{ contribution.title }}</strong>
                <p class="text-caption text-medium-emphasis">{{ contribution.dueLabel }} · +{{ store.rewardForContribution(contribution.id) }} L</p>
              </div>
              <v-chip v-if="contribution.status === 'pending'" color="warning" size="small" variant="tonal">Wartet</v-chip>
              <v-btn
                v-else
                class="raised-button"
                color="info"
                rounded="lg"
                size="small"
                variant="flat"
                @click="store.submitContribution(contribution.id)"
              >
                Fertig
              </v-btn>
            </div>
          </v-card>
        </div>

        <v-card v-else class="contribution-empty pa-4" elevation="0" rounded="xl">
          <LadiMascot :score="emptyLadiScore" :show-score="false" :size="82" />
          <div class="flex-grow-1 min-w-0">
            <p class="eyebrow mb-1">{{ emptyTaskEyebrow }}</p>
            <strong>{{ emptyTaskTitle }}</strong>
            <p>{{ emptyTaskMessage }}</p>
            <RouterLink class="empty-task-link" to="/beitraege">Freie Aufgaben ansehen <v-icon icon="mdi-arrow-right" size="15" /></RouterLink>
          </div>
        </v-card>
      </section>

    </div>

    <v-dialog v-model="giftDialogOpen" max-width="420">
      <v-card class="pa-5" rounded="xl">
        <div class="direct-gift-icon mb-3">🎁</div>
        <v-card-title class="pa-0">Ladirchen verschenken</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-5">Für besondere Momente – unabhängig von einem Beitrag.</v-card-subtitle>
        <v-select v-model="gift.childId" :items="childOptions" item-title="title" item-value="value" label="Kind" variant="outlined" />
        <v-text-field v-model.number="gift.amount" label="Betrag" min="1" suffix="L" type="number" variant="outlined" />
        <v-textarea v-model="gift.reason" label="Anlass" placeholder="Zum Beispiel: tolle Hilfe beim Familienfest" rows="2" variant="outlined" />
        <v-alert class="mb-4" color="info" density="compact" variant="tonal">Das Kind sieht beim nächsten Login, wer das Geschenk geschickt hat.</v-alert>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="giftDialogOpen = false">Abbrechen</v-btn>
          <v-btn color="secondary" :disabled="!canGiveGift" prepend-icon="mdi-gift-outline" rounded="lg" variant="flat" @click="giveDirectGift">Verschenken</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="promoDetailsOpen" max-width="410">
      <v-card v-if="activePromotion" class="pa-5" rounded="xl">
        <div class="promotion-detail-gem" aria-hidden="true"><v-icon icon="mdi-diamond-stone" size="38" /><i>✦</i><i>✧</i></div>
        <p class="eyebrow mt-3 mb-1">Nur heute</p>
        <h2 class="promotion-dialog-title">{{ activePromotion.title }}</h2>
        <p class="text-body-small text-medium-emphasis mt-2">Erledige „{{ contributionTitle(activePromotion.contributionId) }}“ bis {{ activePromotion.deadline }} Uhr.</p>
        <v-card class="pa-4 mt-4" color="amber-lighten-5" elevation="0" rounded="lg">
          <span class="text-caption text-medium-emphasis">Du kannst erhalten</span>
          <strong class="promotion-total d-block">{{ store.rewardForContribution(activePromotion.contributionId) }} Ladirchen</strong>
          <span class="text-caption text-medium-emphasis">Aufgabenbonus ×{{ activePromotion.multiplier }} · Teamarbeit +{{ activePromotion.teamworkBonus }} L</span>
        </v-card>
        <v-btn class="mt-5" color="warning" rounded="lg" variant="flat" width="100%" @click="promoDetailsOpen = false">Los geht’s</v-btn>
      </v-card>
    </v-dialog>

    <HouseEnergyDialog v-model="energyDialogOpen" />
  </div>
</template>

<script lang="ts" setup>
import { computed, reactive, ref } from 'vue';

import AnimatedHouseEnergy from '../components/AnimatedHouseEnergy.vue';
import FamilyWorldScene from '../components/FamilyWorldScene.vue';
import HouseEnergyDialog from '../components/HouseEnergyDialog.vue';
import LadiMascot from '@/shared/components/LadiMascot.vue';
import SectionHeader from '@/shared/components/ui/SectionHeader.vue';
import type { ContributionId, FamilyMemberId } from '@/domain/types';
import { getLadiStage } from '@/domain/ladi';
import { isPromotionAvailable } from '@/domain/promotions';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const promoDetailsOpen = ref(false);
const giftDialogOpen = ref(false);
const energyDialogOpen = ref(false);
const gift = reactive({ childId: store.activeChildId, amount: 20, reason: '' });

const personalContributions = computed(() =>
  store.contributions
    .filter((contribution) => contribution.assigneeId === store.activeChildId && contribution.status !== 'approved'),
);
const completedPersonalContributions = computed(() => store.contributions.filter(
  contribution => contribution.assigneeId === store.activeChildId && contribution.status === 'approved',
));
const isProudLadi = computed(() => store.averageTaskRating >= 4.3);
const emptyLadiScore = computed(() => completedPersonalContributions.value.length ? store.averageTaskRating : 0);
const emptyTaskEyebrow = computed(() => completedPersonalContributions.value.length ? 'Für heute geschafft' : 'Ladi wartet auf eine Mission');
const emptyTaskTitle = computed(() => completedPersonalContributions.value.length ? 'Alles erledigt!' : 'Heute ist noch nichts zugeteilt');
const emptyTaskMessage = computed(() => {
  if (!completedPersonalContributions.value.length) return 'Schau bei den freien Aufgaben vorbei – vielleicht braucht gerade jemand deine Hilfe.';
  if (isProudLadi.value) return `${getLadiStage(store.averageTaskRating).name} ist richtig stolz auf dich. Für heute hast du alles geschafft.`;
  return 'Für heute hast du alles geschafft. Ladi macht es sich nach eurem Einsatz gemütlich.';
});
const activePromotion = computed(() =>
  store.promotions.find((promotion) =>
    isPromotionAvailable(promotion, store.familyTimeZone, new Date(store.currentTimeMilliseconds)) && store.contributions.some(
      (contribution) => contribution.id === promotion.contributionId && contribution.assigneeId === store.activeChildId,
    ),
  ),
);
const contributionTitle = (contributionId: ContributionId) =>
  store.contributions.find((contribution) => contribution.id === contributionId)?.title ?? 'Beitrag';
const childMembers = computed(() => store.members.filter((member) => member.role === 'child'));
const ratingLabel = (memberId: FamilyMemberId) => {
  const rating = store.averageTaskRatingFor(memberId);
  return rating > 0 ? `${rating.toFixed(1)} ★` : 'Noch offen';
};
const childOptions = computed(() => childMembers.value.map((child) => ({ title: `${child.avatar} ${child.name}`, value: child.id })));
const canGiveGift = computed(() => gift.amount >= 1 && gift.reason.trim().length >= 3);
const openGiftDialog = () => {
  Object.assign(gift, { childId: store.activeChildId, amount: 20, reason: '' });
  giftDialogOpen.value = true;
};
const giveDirectGift = () => {
  store.giftLadirchenToChild(gift.childId, gift.amount, gift.reason);
  giftDialogOpen.value = false;
};
</script>

<style scoped>
.world-hero {
  padding-bottom: 16px;
  @apply position-relative overflow-hidden;
  color: #233948;
  background: linear-gradient(180deg, #dff5ff 0%, #f7fbeb 78%, #f8f1dd 100%);
}
.world-title {
  max-width: 310px;
  @apply ma-0;
  font-size: 23px;
  line-height: 1.1;
  letter-spacing: -0.04em;
}
.energy-value {
  @apply d-block;
  color: var(--lad-mint-dark);
  font-size: 20px;
  line-height: 1;
  @apply text-no-wrap;
}
.energy-label {
  @apply d-block mt-1;
  color: #65808f;
  font-size: 10px;
}
.energy-trigger-copy {
  @apply min-w-0 text-left;
}
.energy-trigger {
  min-width: 136px;
  @apply position-relative;
  padding: 7px 22px 7px 8px;
  @apply d-flex align-center;
  gap: 6px;
  color: inherit;
  border: 1px solid rgba(73, 150, 121, 0.1);
  border-radius: 17px;
  background: rgba(255, 255, 255, 0.46);
  box-shadow: 0 3px 0 rgba(57, 137, 106, 0.07);
  font: inherit;
  @apply cursor-pointer;
  transition:
    background 160ms ease,
    transform 160ms ease,
    box-shadow 160ms ease;
}
.energy-trigger:hover {
  background: rgba(255, 255, 255, 0.78);
  box-shadow: 0 4px 0 rgba(57, 137, 106, 0.11);
}
.energy-trigger:active {
  transform: translateY(1px) scale(0.98);
  box-shadow: 0 1px 0 rgba(57, 137, 106, 0.09);
}
.energy-chevron {
  @apply position-absolute;
  top: 50%;
  right: 5px;
  color: var(--lad-mint-dark);
  transform: translateY(-50%);
}
@media (max-width: 420px) {
  .energy-trigger {
    min-width: 103px;
    padding-left: 5px;
    gap: 2px;
  }
  .energy-trigger :deep(.animated-house-energy) {
    width: 35px !important;
    height: 31px !important;
  }
  .energy-value {
    font-size: 17px;
  }
  .energy-label {
    font-size: 9px;
  }
}
.contribution-board {
  padding: 15px;
  @apply position-relative overflow-hidden;
  border: 2px solid rgba(71, 153, 122, 0.17);
  border-radius: 27px;
  background:
    radial-gradient(
      circle at 92% 7%,
      rgba(255, 218, 112, 0.22),
      transparent 23%
    ),
    linear-gradient(145deg, #fffef9, #f0faf6);
  box-shadow:
    0 7px 0 rgba(58, 135, 105, 0.11),
    0 15px 27px rgba(62, 100, 85, 0.07);
}
.contribution-board::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 10px;
  right: 13px;
  color: rgba(226, 166, 45, 0.65);
  font-size: 13px;
}
.contribution-board-heading {
  @apply position-relative d-flex align-start justify-space-between;
  z-index: 1;
  gap: 12px;
  margin-bottom: 14px;
}
.contribution-board-heading h2 {
  @apply ma-0;
  font-size: 20px;
  letter-spacing: -0.035em;
}
.contribution-board-heading > div > p:last-child {
  margin-top: 3px;
  color: var(--lad-muted);
  font-size: 10px;
}
.all-contributions {
  min-height: 46px;
  padding: 7px 8px 7px 10px;
  @apply d-flex align-center flex-shrink-0;
  gap: 5px;
  color: #236f57;
  border: 2px solid rgba(62, 163, 124, 0.2);
  border-radius: 16px;
  background: linear-gradient(145deg, #eaf9f2, #fff);
  box-shadow: 0 4px 0 rgba(49, 139, 105, 0.13);
  font-size: 10px;
  font-weight: 900;
  text-decoration: none;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.all-contributions:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 0 rgba(49, 139, 105, 0.13);
}
.contribution-list {
  @apply d-flex flex-column;
  gap: 11px;
}
.contribution-card {
  border: 2px solid rgba(78, 143, 221, 0.14);
  background: linear-gradient(145deg, #fff, #f5faff);
  box-shadow: 0 5px 0 rgba(78, 143, 221, 0.11) !important;
}
.contribution-icon {
  width: 50px;
  height: 50px;
  @apply d-grid place-center flex-shrink-0;
  border: 3px solid #fff;
  border-radius: 17px;
  background: linear-gradient(145deg, #e7f6ff, #fff2c9);
  box-shadow: 0 4px 0 rgba(78, 143, 221, 0.13);
  font-size: 25px;
  transform: rotate(-4deg);
}
.contribution-empty {
  @apply d-flex align-center position-relative overflow-hidden;
  gap: 13px;
  border: 2px dashed rgba(77, 153, 122, 0.28);
  background:
    radial-gradient(
      circle at 88% 18%,
      rgba(255, 218, 112, 0.25),
      transparent 26%
    ),
    linear-gradient(145deg, #f4fbf7, #fff9e8);
  box-shadow: inset 0 0 0 5px rgba(255, 255, 255, 0.35) !important;
}
.contribution-empty strong {
  font-size: 15px;
}
.contribution-empty p:not(.eyebrow) {
  margin: 3px 0 8px;
  color: var(--lad-muted);
  font-size: 10px;
  line-height: 1.4;
}
.empty-task-link {
  @apply d-inline-flex align-center;
  gap: 4px;
  color: var(--lad-mint-dark);
  font-size: 10px;
  font-weight: 900;
  text-decoration: none;
}
.promotion-banner {
  @apply position-relative overflow-hidden cursor-pointer;
  border: 2px solid rgba(114, 174, 215, 0.38);
  background: linear-gradient(135deg, #eef9ff, #fff4d4 54%, #f5eaff) !important;
  box-shadow:
    0 7px 0 rgba(112, 164, 198, 0.18),
    0 14px 25px rgba(75, 105, 136, 0.09) !important;
}
.promotion-banner::before {
  content: "";
  width: 72px;
  height: 180%;
  @apply position-absolute;
  top: -40%;
  left: -95px;
  transform: rotate(18deg);
  background: linear-gradient(
    90deg,
    transparent,
    rgba(255, 255, 255, 0.8),
    transparent
  );
  animation: bonus-shimmer 3.4s ease-in-out infinite;
}
.promotion-banner::after {
  content: "✦  ✧  ★";
  @apply position-absolute;
  top: 9px;
  right: 11px;
  color: #e5a52a;
  text-shadow:
    0 0 7px rgba(255, 224, 126, 0.95),
    18px 12px 0 rgba(121, 106, 205, 0.55);
  font-size: 14px;
  letter-spacing: 6px;
  animation: bonus-stars 2.1s ease-in-out infinite;
}
.promotion-gem,
.promotion-detail-gem {
  @apply position-relative d-grid place-center flex-shrink-0;
  color: #fff;
  border: 3px solid #fff;
  background: linear-gradient(145deg, #7bd5ea, #6482dc 58%, #a66ed0);
  box-shadow:
    0 5px 0 #596ab1,
    0 9px 16px rgba(77, 89, 154, 0.22);
  transform: rotate(-5deg);
  animation: bonus-gem 2.4s ease-in-out infinite;
}
.promotion-gem {
  width: 49px;
  height: 49px;
  border-radius: 17px;
}
.promotion-detail-gem {
  width: 68px;
  height: 68px;
  border-radius: 22px;
}
.promotion-gem i,
.promotion-detail-gem i {
  @apply position-absolute;
  color: #fff7b0;
  font-style: normal;
  text-shadow: 0 0 7px #fff;
}
.promotion-gem i:first-of-type,
.promotion-detail-gem i:first-of-type {
  top: -9px;
  right: -8px;
}
.promotion-gem i:last-of-type,
.promotion-detail-gem i:last-of-type {
  bottom: -8px;
  left: -6px;
  animation-delay: -1s;
}
.promotion-dialog-title {
  @apply ma-0;
  font-size: 22px;
  letter-spacing: -0.03em;
}
.promotion-total {
  color: #955b06;
  font-size: 28px;
}
.section-title {
  @apply ma-0;
  font-size: 19px;
  letter-spacing: -0.025em;
}
.guardian-child-grid {
  @apply d-grid;
  gap: 9px;
}
.guardian-child {
  @apply w-100;
  padding: 10px;
  @apply d-flex align-center;
  gap: 10px;
  color: var(--lad-text);
  @apply text-left;
  border: 1px solid var(--lad-border);
  border-radius: 15px;
  background: var(--lad-surface);
  box-shadow: 0 3px 0 var(--lad-border);
  @apply cursor-pointer;
}
.guardian-child.selected {
  border-color: rgba(62, 188, 140, 0.55);
  background: #effaf5;
  box-shadow: 0 3px 0 rgba(62, 188, 140, 0.25);
}
.guardian-child-avatar {
  width: 39px;
  height: 39px;
  @apply d-grid place-center flex-shrink-0;
  border-radius: 13px;
  background: var(--lad-surface-soft);
  font-size: 21px;
}
.guardian-child-copy {
  flex: 1;
  @apply min-w-0;
}
.guardian-child-copy strong,
.guardian-child-copy small {
  @apply d-block;
}
.guardian-child-copy small {
  color: var(--lad-muted);
  font-size: 9px;
}
.guardian-child-rating {
  padding: 5px 7px;
  color: #956117;
  border-radius: 8px;
  background: #fff3d3;
  font-size: 9px;
  @apply font-weight-black;
}
.guardian-child-rating.ready {
  color: #22734f;
  background: #dcf5e8;
}
.rating-rule {
  color: #956117;
  font-size: 9px;
  @apply font-weight-black;
}
.management-card {
  border: 1px solid rgba(78, 143, 221, 0.2);
}
.supporter-card {
  border: 1px solid rgba(62, 188, 140, 0.24);
  background: linear-gradient(145deg, #effaf5, #fff8df);
  box-shadow: 0 4px 0 rgba(62, 188, 140, 0.12) !important;
}
.management-actions {
  @apply d-grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 7px;
}
.management-actions :deep(.v-btn) {
  @apply min-w-0;
  padding-inline: 8px;
  font-size: 9px;
}
.energy-simulator {
  border: 1px dashed rgba(62, 188, 140, 0.55);
  background: linear-gradient(145deg, #f0faf5, #fff9eb);
}
.simulated-value {
  color: var(--lad-mint-dark);
  font-size: 21px;
}
.direct-gift-icon {
  width: 58px;
  height: 58px;
  @apply d-grid place-center;
  border-radius: 19px;
  background: #fff0d5;
  font-size: 31px;
  animation: bonus-pulse 1.8s ease-in-out infinite;
}
@keyframes bonus-pulse {
  0%,
  100% {
    transform: scale(1) rotate(-3deg);
  }
  50% {
    transform: scale(1.08) rotate(3deg);
  }
}
@keyframes bonus-gem {
  0%,
  100% {
    transform: translateY(0) rotate(-5deg) scale(1);
  }
  50% {
    transform: translateY(-3px) rotate(4deg) scale(1.06);
  }
}
@keyframes bonus-shimmer {
  0%,
  34% {
    left: -95px;
  }
  70%,
  100% {
    left: 120%;
  }
}
@keyframes bonus-stars {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.8) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.15) rotate(14deg);
  }
}
@media (max-width: 420px) {
  .contribution-board {
    padding: 13px;
  }
  .contribution-board-heading {
    align-items: stretch;
    flex-direction: column;
  }
  .all-contributions {
    width: fit-content;
    min-height: 38px;
  }
  .contribution-empty {
    align-items: flex-start;
  }
  .contribution-empty :deep(.ladi-mascot) {
    transform: scale(0.9);
    transform-origin: top left;
  }
}
</style>
