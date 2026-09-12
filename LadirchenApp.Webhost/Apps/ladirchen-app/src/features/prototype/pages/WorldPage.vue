<template>
  <div class="page world-page">
    <section class="world-hero px-4 pt-5">
      <div v-if="store.viewerRole === 'child'" class="d-flex align-start justify-space-between ga-3">
        <div>
          <p class="eyebrow">{{ store.activeChild.name }}s Familienwelt</p>
          <h1 class="world-title">Jeder Beitrag macht unser Zuhause lebendiger.</h1>
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
      <div v-else class="d-flex align-start justify-space-between ga-3">
        <div>
          <p class="eyebrow">Bezugspersonen-Übersicht</p>
          <h1 class="world-title">So geht es allen Kindern heute.</h1>
        </div>
        <v-chip color="info" size="small" variant="tonal">{{ childMembers.length }} Kinder</v-chip>
      </div>

      <FamilyWorldScene
        :accessories="store.accessories"
        :effects="store.activeWorldEffects"
        :energy="store.familyEnergy"
        :house-level="store.houseLevel"
        :house-theme-id="store.houseThemeId"
        :members="store.members"
        :pets="store.pets"
        :reveal-version="store.revealVersion"
        :viewer-member-id="store.signedInMemberId"
      />
    </section>

    <div class="page-padding pt-5">
      <template v-if="store.viewerRole === 'guardian'">
        <section class="guardian-world mb-6">
          <div class="d-flex align-end justify-space-between mb-3">
            <div><p class="eyebrow mb-1">Heute</p><h2 class="section-title">Kinder im Überblick</h2></div>
            <span class="rating-rule">Hausziel: mindestens 60 % Energie</span>
          </div>
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

      <v-card v-if="store.viewerRole === 'child' && activePromotion" class="promotion-banner pa-4 mb-6" color="amber-lighten-5" elevation="0" rounded="xl" role="button" tabindex="0" @click="promoDetailsOpen = true" @keydown.enter="promoDetailsOpen = true">
        <div class="d-flex align-center ga-3">
          <div class="promotion-bolt">⚡</div>
          <div class="flex-grow-1">
            <p class="eyebrow mb-1">Bonusaktion</p>
            <strong>{{ activePromotion.title }}</strong>
            <p class="text-caption text-medium-emphasis">{{ contributionTitle(activePromotion.contributionId) }} bis {{ activePromotion.deadline }} Uhr</p>
          </div>
          <v-chip color="warning" size="small">×{{ activePromotion.multiplier }}</v-chip>
        </div>
      </v-card>

      <div v-if="store.viewerRole === 'child'" class="d-flex align-end justify-space-between mb-3">
        <div>
          <h2 class="section-title">Wo möchtest du helfen?</h2>
          <p class="text-caption text-medium-emphasis">Bestätigte Grundbeiträge verändern die Welt.</p>
        </div>
        <RouterLink class="section-link" to="/beitraege">Alle</RouterLink>
      </div>

      <div v-if="store.viewerRole === 'child'" class="d-flex flex-column ga-3">
        <v-card v-for="contribution in quickContributions" :key="contribution.id" class="contribution-card pa-3" elevation="0" rounded="xl">
          <div class="d-flex align-center ga-3">
            <v-avatar color="blue-lighten-5" rounded="lg" size="48">{{ contribution.icon }}</v-avatar>
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

      <v-card v-if="store.viewerRole === 'child'" class="goal-preview pa-4 mt-6" color="green-lighten-5" elevation="0" rounded="xl" to="/wuensche">
        <div class="d-flex align-center ga-3">
          <span class="goal-preview-icon">{{ store.activeGoal.icon }}</span>
          <div class="flex-grow-1">
            <p class="eyebrow mb-1">Mein nächster Wunsch</p>
            <strong>{{ store.activeGoal.title }}</strong>
            <v-progress-linear class="mt-2" color="primary" height="8" :model-value="goalProgress" rounded />
            <p class="text-caption text-medium-emphasis mt-1">{{ store.activeGoal.saved }} von {{ store.activeGoal.target }} Ladirchen</p>
          </div>
          <v-icon icon="mdi-chevron-right" />
        </div>
      </v-card>
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
        <div class="promotion-detail-bolt">⚡</div>
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
import { usePrototypeStore } from '../stores/prototype';

const store = usePrototypeStore();
const promoDetailsOpen = ref(false);
const giftDialogOpen = ref(false);
const energyDialogOpen = ref(false);
const gift = reactive({ childId: store.activeChildId, amount: 20, reason: '' });

const quickContributions = computed(() =>
  store.contributions
    .filter((contribution) => contribution.assigneeId === store.activeChildId && contribution.status !== 'approved')
    .slice(0, 2),
);
const activePromotion = computed(() =>
  store.promotions.find((promotion) =>
    promotion.active && store.contributions.some(
      (contribution) => contribution.id === promotion.contributionId && contribution.assigneeId === store.activeChildId,
    ),
  ),
);
const goalProgress = computed(() => (store.activeGoal.saved / store.activeGoal.target) * 100);
const contributionTitle = (contributionId: string) =>
  store.contributions.find((contribution) => contribution.id === contributionId)?.title ?? 'Beitrag';
const childMembers = computed(() => store.members.filter((member) => member.role === 'child'));
const ratingLabel = (memberId: string) => {
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
  position: relative;
  overflow: hidden;
  color: #233948;
  background: linear-gradient(180deg, #dff5ff 0%, #f7fbeb 78%, #f8f1dd 100%);
}
.world-title {
  max-width: 310px;
  margin: 0;
  font-size: 23px;
  line-height: 1.1;
  letter-spacing: -0.04em;
}
.energy-value {
  display: block;
  color: var(--lad-mint-dark);
  font-size: 20px;
  line-height: 1;
  white-space: nowrap;
}
.energy-label {
  display: block;
  margin-top: 4px;
  color: #65808f;
  font-size: 10px;
}
.energy-trigger-copy {
  min-width: 0;
  text-align: left;
}
.energy-trigger {
  min-width: 136px;
  position: relative;
  padding: 7px 22px 7px 8px;
  display: flex;
  align-items: center;
  gap: 6px;
  color: inherit;
  border: 1px solid rgba(73, 150, 121, 0.1);
  border-radius: 17px;
  background: rgba(255, 255, 255, 0.46);
  box-shadow: 0 3px 0 rgba(57, 137, 106, 0.07);
  font: inherit;
  cursor: pointer;
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
  position: absolute;
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
.contribution-card {
  border: 1px solid var(--lad-border);
  box-shadow: 0 4px 0 var(--lad-border) !important;
}
.promotion-banner {
  border: 1px solid rgba(242, 175, 66, 0.3);
  cursor: pointer;
}
.promotion-bolt {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  flex-shrink: 0;
  border-radius: 15px;
  background: #ffe5a8;
  font-size: 23px;
  animation: bonus-pulse 1.8s ease-in-out infinite;
}
.promotion-detail-bolt {
  width: 62px;
  height: 62px;
  display: grid;
  place-items: center;
  border-radius: 20px;
  background: #ffe7ac;
  font-size: 32px;
}
.promotion-dialog-title {
  margin: 0;
  font-size: 22px;
  letter-spacing: -0.03em;
}
.promotion-total {
  color: #955b06;
  font-size: 28px;
}
.section-title {
  margin: 0;
  font-size: 19px;
  letter-spacing: -0.025em;
}
.section-link {
  color: var(--lad-mint-dark);
  font-size: 12px;
  font-weight: 900;
}
.goal-preview {
  border: 1px solid rgba(62, 188, 140, 0.2);
}
.goal-preview-icon {
  font-size: 36px;
}
.guardian-child-grid {
  display: grid;
  gap: 9px;
}
.guardian-child {
  width: 100%;
  padding: 10px;
  display: flex;
  align-items: center;
  gap: 10px;
  color: var(--lad-text);
  text-align: left;
  border: 1px solid var(--lad-border);
  border-radius: 15px;
  background: var(--lad-surface);
  box-shadow: 0 3px 0 var(--lad-border);
  cursor: pointer;
}
.guardian-child.selected {
  border-color: rgba(62, 188, 140, 0.55);
  background: #effaf5;
  box-shadow: 0 3px 0 rgba(62, 188, 140, 0.25);
}
.guardian-child-avatar {
  width: 39px;
  height: 39px;
  display: grid;
  place-items: center;
  flex-shrink: 0;
  border-radius: 13px;
  background: var(--lad-surface-soft);
  font-size: 21px;
}
.guardian-child-copy {
  flex: 1;
  min-width: 0;
}
.guardian-child-copy strong,
.guardian-child-copy small {
  display: block;
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
  font-weight: 900;
}
.guardian-child-rating.ready {
  color: #22734f;
  background: #dcf5e8;
}
.rating-rule {
  color: #956117;
  font-size: 9px;
  font-weight: 900;
}
.management-card {
  border: 1px solid rgba(78, 143, 221, 0.2);
}
.management-actions {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 7px;
}
.management-actions :deep(.v-btn) {
  min-width: 0;
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
  display: grid;
  place-items: center;
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
</style>
