<template>
  <div class="page world-page">
    <section class="world-hero px-4 pt-5">
      <div class="d-flex align-start justify-space-between ga-3">
        <div v-if="store.viewerRole === 'child'">
          <p class="eyebrow">{{ t('world.hero.childEyebrow', { name: store.activeChild.name }) }}</p>
          <h1 class="world-title">{{ t('world.hero.childTitle') }}</h1>
        </div>
        <div v-else>
          <p class="eyebrow">{{ t('world.hero.guardianEyebrow') }}</p>
          <h1 class="world-title">{{ t('world.hero.guardianTitle') }}</h1>
        </div>
        <button :aria-label="t('world.hero.energyAria', { value: store.familyEnergy })" class="energy-trigger text-right flex-shrink-0" type="button" @click="energyDialogOpen = true">
          <AnimatedHouseEnergy :size="43" />
          <span class="energy-trigger-copy">
            <strong class="energy-value">{{ store.familyEnergy }} %</strong>
            <span class="energy-label">{{ t('world.hero.energy') }}</span>
          </span>
          <v-icon class="energy-chevron" icon="mdi-chevron-right" size="15" />
        </button>
      </div>

      <FamilyWorldScene
        :accessories="localizedAccessories"
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
        <v-card class="management-card pa-4 mb-6" color="blue-lighten-5" elevation="0" rounded="xl">
          <div class="d-flex align-center justify-space-between ga-3 mb-3">
            <div><strong>{{ t('world.management.title') }}</strong><p class="text-caption text-medium-emphasis">{{ t('world.management.description', { name: store.activeChild.name }) }}</p></div>
            <v-icon color="info">mdi-shield-account-outline</v-icon>
          </div>
          <div class="management-actions">
            <v-btn color="primary" prepend-icon="mdi-plus" rounded="lg" size="small" to="/beitraege?new=1" variant="flat">{{ t('world.management.contribution') }}</v-btn>
            <v-btn color="warning" prepend-icon="mdi-rocket-launch-outline" rounded="lg" size="small" to="/beitraege?promotion=1" variant="tonal">{{ t('world.management.promotion') }}</v-btn>
            <v-btn color="info" prepend-icon="mdi-target" rounded="lg" size="small" to="/wuensche?new=1" variant="tonal">{{ t('world.management.childGoal') }}</v-btn>
            <v-btn color="info" prepend-icon="mdi-account-group-outline" rounded="lg" size="small" to="/wuensche?family=1" variant="tonal">{{ t('world.management.familyGoal') }}</v-btn>
            <v-btn color="warning" prepend-icon="mdi-gift-outline" rounded="lg" size="small" to="/shop?new=1" variant="tonal">{{ t('world.management.reward') }}</v-btn>
            <v-btn color="secondary" prepend-icon="mdi-hand-coin-outline" rounded="lg" size="small" variant="tonal" @click="openGiftDialog">{{ t('world.management.gift') }}</v-btn>
            <v-btn color="primary" prepend-icon="mdi-account-cog-outline" rounded="lg" size="small" variant="tonal" @click="store.openFamilySetup">{{ t('world.management.family') }}</v-btn>
            <v-btn color="success" prepend-icon="mdi-piggy-bank-outline" rounded="lg" size="small" variant="tonal" @click="store.piggyBankOpen = true">{{ t('world.management.money') }}</v-btn>
          </div>
        </v-card>

      </template>

      <v-card v-else-if="store.viewerRole === 'guardian'" class="supporter-card pa-4 mb-6" elevation="0" rounded="xl">
        <div class="d-flex align-center ga-3">
          <v-avatar color="primary" size="46" variant="tonal"><v-icon icon="mdi-hand-heart-outline" /></v-avatar>
          <div class="flex-grow-1 min-w-0">
            <p class="eyebrow mb-1">{{ t('world.supporter.eyebrow') }}</p>
            <strong>{{ t('world.supporter.title') }}</strong>
            <p class="text-caption text-medium-emphasis mt-1">{{ t('world.supporter.description') }}</p>
          </div>
        </div>
        <v-btn class="mt-4" color="primary" prepend-icon="mdi-gift-outline" rounded="lg" to="/wuensche" variant="flat" width="100%">{{ t('world.supporter.action') }}</v-btn>
      </v-card>

      <v-card v-if="store.viewerRole === 'child' && activePromotion" class="promotion-banner pa-4 mb-6" color="amber-lighten-5" elevation="0" rounded="xl" role="button" tabindex="0" @click="promoDetailsOpen = true" @keydown.enter="promoDetailsOpen = true">
        <div class="promotion-stars" aria-hidden="true"><i>✦</i><i>✧</i><i>★</i><i>✦</i><i>✧</i><i>·</i></div>
        <div class="d-flex align-center ga-3">
          <div class="promotion-gem" aria-hidden="true"><v-icon class="promotion-rocket" icon="mdi-rocket-launch" size="29" /><span><v-icon icon="mdi-diamond-stone" size="12" /></span><i>✦</i><i>✧</i></div>
          <div class="flex-grow-1">
            <p class="eyebrow mb-1">{{ t('world.promotion.eyebrow') }}</p>
            <strong>{{ activePromotion.title }}</strong>
            <p class="text-caption text-medium-emphasis">{{ t('world.promotion.until', { title: contributionTitle(activePromotion.contributionId), time: activePromotion.deadline }) }}</p>
            <PromotionCountdown class="promotion-banner-countdown mt-2" :deadline="activePromotion.deadline" />
          </div>
          <div class="promotion-boost" :aria-label="t('world.promotion.boostAria')">
            <strong>×{{ activePromotion.multiplier }}</strong>
            <small>{{ t('world.promotion.upTo', { value: promotionRewardTotal }) }}</small>
          </div>
        </div>
      </v-card>

      <section v-if="store.viewerRole === 'child'" class="contribution-board">
        <div class="contribution-board-heading">
          <div>
            <p class="eyebrow mb-1">{{ t('world.tasks.eyebrow') }}</p>
            <h2>{{ t('world.tasks.title') }}</h2>
            <p>{{ t('world.tasks.description') }}</p>
          </div>
          <RouterLink class="all-contributions" to="/beitraege"><v-icon icon="mdi-view-grid-plus-outline" size="19" /><span>{{ t('world.tasks.all') }}</span><v-icon icon="mdi-chevron-right" size="17" /></RouterLink>
        </div>

        <div v-if="personalContributions.length" class="contribution-list">
          <v-card v-for="contribution in personalContributions" :key="contribution.id" class="home-contribution-item pa-4" :data-ladi-heading="t('world.tasks.tip')" :data-ladi-tip="contribution.description" elevation="0" rounded="xl">
            <div class="d-flex align-start ga-3">
              <v-avatar class="home-task-icon" color="blue-lighten-5" rounded="lg" size="58">{{ contribution.icon }}</v-avatar>
              <div class="flex-grow-1 min-w-0">
                <div class="d-flex align-center flex-wrap ga-2 mb-1">
                  <strong>{{ contribution.title }}</strong>
                  <span class="home-contribution-label" :class="contribution.kind === 'basic' ? 'home-contribution-label--basic' : 'home-contribution-label--special'">
                    <v-icon :icon="contribution.kind === 'basic' ? 'mdi-home-heart' : 'mdi-creation'" size="13" />{{ contribution.kind === 'basic' ? t('world.tasks.basic') : t('world.tasks.extra') }}
                  </span>
                  <span class="home-contribution-label home-contribution-label--mine"><v-icon icon="mdi-account-heart" size="13" />{{ t('world.tasks.forYou') }}</span>
                </div>
                <p class="text-caption text-medium-emphasis">{{ contribution.description }}</p>
                <div class="mt-3 home-contribution-meta">
                  <span class="home-meta-chip home-meta-chip--time"><ContributionMetaIcon kind="time" /><span><small>{{ t('world.tasks.when') }}</small><strong>{{ contribution.dueLabel }}</strong></span></span>
                  <span class="home-meta-chip home-meta-chip--reward"><ContributionMetaIcon kind="reward" /><span><small>{{ t('world.tasks.earn') }}</small><strong>{{ t('world.tasks.coins', { value: store.rewardForContribution(contribution.id) }) }}</strong></span></span>
                  <span v-if="contribution.kind === 'basic'" class="home-meta-chip home-meta-chip--energy"><ContributionMetaIcon kind="energy" /><span><small>{{ t('world.tasks.house') }}</small><strong>{{ t('world.tasks.energy', { value: contribution.energy }) }}</strong></span></span>
                </div>
                <ActiveContributionBonus
                  v-if="promotionFor(contribution.id)"
                  class="mt-2"
                  :deadline="promotionFor(contribution.id)?.deadline ?? '00:00'"
                  :multiplier="promotionFor(contribution.id)?.multiplier ?? 1"
                />
                <div v-if="contribution.invitedChildIds?.length" class="home-invited-team mt-3">
                  <span>{{ t('world.tasks.together') }}</span>
                  <v-chip v-for="name in invitedChildNames(contribution.invitedChildIds)" :key="name" color="info" size="x-small" variant="tonal">{{ name }}</v-chip>
                </div>
              </div>
            </div>
            <div class="d-flex align-center justify-end flex-wrap ga-2 mt-3">
              <div v-if="contribution.status === 'pending'" class="home-pending" role="status">
                <span class="home-pending-icon" aria-hidden="true">✨</span>
                <span><strong>{{ t('world.tasks.pendingTitle') }}</strong><small>{{ t('world.tasks.pendingDescription') }}</small></span>
              </div>
              <v-btn v-else class="home-finish-button" color="info" rounded="lg" variant="flat" @click="store.submitContribution(contribution.id)">
                <span class="home-finish-check" aria-hidden="true"><v-icon icon="mdi-check-bold" size="22" /></span>
                <span>{{ t('world.tasks.finish') }}</span>
              </v-btn>
            </div>
          </v-card>
        </div>

        <v-card v-else class="contribution-empty pa-4" elevation="0" rounded="xl">
          <AnimatedCompletionMark :size="80" />
          <div class="flex-grow-1 min-w-0">
            <p class="eyebrow mb-1">{{ emptyTaskEyebrow }}</p>
            <strong>{{ emptyTaskTitle }}</strong>
            <p>{{ emptyTaskMessage }}</p>
            <RouterLink class="empty-task-link" to="/beitraege">{{ t('world.tasks.openAvailable') }} <v-icon icon="mdi-arrow-right" size="15" /></RouterLink>
          </div>
        </v-card>
      </section>

    </div>

    <v-dialog v-model="giftDialogOpen" max-width="420">
      <v-card class="pa-5" rounded="xl">
        <div class="direct-gift-icon mb-3">🎁</div>
        <v-card-title class="pa-0">{{ t('world.gift.title') }}</v-card-title>
        <v-card-subtitle class="pa-0 mt-1 mb-5">{{ t('world.gift.description') }}</v-card-subtitle>
        <v-select v-model="gift.childId" :items="childOptions" item-title="title" item-value="value" :label="t('world.gift.child')" variant="outlined" />
        <v-text-field v-model.number="gift.amount" :label="t('world.gift.amount')" min="1" suffix="L" type="number" variant="outlined" />
        <v-textarea v-model="gift.reason" :label="t('world.gift.reason')" :placeholder="t('world.gift.placeholder')" rows="2" variant="outlined" />
        <v-alert class="mb-4" color="info" density="compact" variant="tonal">{{ t('world.gift.notice') }}</v-alert>
        <div class="d-flex justify-end ga-2">
          <v-btn rounded="lg" variant="text" @click="giftDialogOpen = false">{{ t('common.cancel') }}</v-btn>
          <v-btn color="secondary" :disabled="!canGiveGift" prepend-icon="mdi-gift-outline" rounded="lg" variant="flat" @click="giveDirectGift">{{ t('world.gift.submit') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="promoDetailsOpen" max-width="410">
      <v-card v-if="activePromotion" class="promotion-detail-card pa-5" rounded="xl">
        <v-btn class="promotion-close" :aria-label="t('world.promotion.close')" icon="mdi-close" size="small" variant="tonal" @click="promoDetailsOpen = false" />
        <div class="promotion-detail-gem" aria-hidden="true"><v-icon icon="mdi-star-four-points" size="55" /><span><v-icon icon="mdi-diamond-stone" size="18" /></span><i>✦</i><i>✧</i></div>
        <p class="eyebrow promotion-detail-eyebrow mt-4 mb-1">{{ t('world.promotion.onlyToday') }}</p>
        <h2 class="promotion-dialog-title">{{ activePromotion.title }}</h2>
        <p class="text-body-small text-medium-emphasis mt-2">{{ t('world.promotion.completeBy', { title: contributionTitle(activePromotion.contributionId), time: activePromotion.deadline }) }}</p>
        <v-card class="promotion-reward-card pa-4 mt-4" elevation="0" rounded="lg">
          <span class="text-caption text-medium-emphasis">{{ t('world.promotion.canEarn') }}</span>
          <strong class="promotion-total d-block">{{ t('world.promotion.coins', { value: promotionRewardTotal }) }}</strong>
          <div class="promotion-breakdown mt-3">
            <span><i>×{{ activePromotion.multiplier }}</i><small>{{ t('world.promotion.taskBonus') }}</small></span>
            <span><i>+{{ activePromotion.teamworkBonus }} L</i><small>{{ t('world.promotion.teamwork') }}</small></span>
          </div>
        </v-card>
        <v-btn class="promotion-start mt-5" color="warning" rounded="lg" variant="flat" width="100%" @click="promoDetailsOpen = false"><span aria-hidden="true">★</span>{{ t('world.promotion.cta') }}<v-icon icon="mdi-arrow-right" /></v-btn>
      </v-card>
    </v-dialog>

    <HouseEnergyDialog v-model="energyDialogOpen" />
  </div>
</template>

<script lang="ts" setup>
import { computed, reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';

import AnimatedHouseEnergy from '../components/AnimatedHouseEnergy.vue';
import FamilyWorldScene from '../components/FamilyWorldScene.vue';
import HouseEnergyDialog from '../components/HouseEnergyDialog.vue';
import ActiveContributionBonus from '@/features/contributions/components/ActiveContributionBonus.vue';
import AnimatedCompletionMark from '@/shared/components/AnimatedCompletionMark.vue';
import ContributionMetaIcon from '@/features/contributions/components/ContributionMetaIcon.vue';
import PromotionCountdown from '@/features/contributions/components/PromotionCountdown.vue';
import type { ContributionId, FamilyMemberId } from '@/domain/types';
import { isPromotionAvailable } from '@/domain/promotions';
import { useLocalizedDomainContent } from '@/shared/composables/use-localized-domain-content';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const { t } = useI18n();
const localize = useLocalizedDomainContent();
const promoDetailsOpen = ref(false);
const giftDialogOpen = ref(false);
const energyDialogOpen = ref(false);
const gift = reactive({ childId: store.activeChildId, amount: 20, reason: '' });

const localizedContributions = computed(() => store.contributions.map(localize.contribution));
const localizedPromotions = computed(() => store.promotions.map(localize.promotion));
const localizedAccessories = computed(() => store.accessories.map(localize.accessory));
const personalContributions = computed(() =>
  localizedContributions.value
    .filter((contribution) => contribution.assigneeId === store.activeChildId && contribution.status !== 'approved'),
);
const completedPersonalContributions = computed(() => localizedContributions.value.filter(
  contribution => contribution.assigneeId === store.activeChildId && contribution.status === 'approved',
));
const emptyTaskEyebrow = computed(() => t(completedPersonalContributions.value.length ? 'world.tasks.empty.completedEyebrow' : 'world.tasks.empty.availableEyebrow'));
const emptyTaskTitle = computed(() => t(completedPersonalContributions.value.length ? 'world.tasks.empty.completedTitle' : 'world.tasks.empty.availableTitle'));
const emptyTaskMessage = computed(() => {
  return t(completedPersonalContributions.value.length ? 'world.tasks.empty.completedMessage' : 'world.tasks.empty.availableMessage');
});
const activePromotion = computed(() =>
  localizedPromotions.value.find((promotion) =>
    isPromotionAvailable(promotion, store.familyTimeZone, new Date(store.currentTimeMilliseconds)) && localizedContributions.value.some(
      (contribution) => contribution.id === promotion.contributionId &&
        contribution.assigneeId === store.activeChildId &&
        contribution.status === 'available',
    ),
  ),
);
const contributionTitle = (contributionId: ContributionId) =>
  localizedContributions.value.find((contribution) => contribution.id === contributionId)?.title ?? t('contributions.singular');
const promotionRewardTotal = computed(() => {
  if (!activePromotion.value) return 0;
  const baseReward = localizedContributions.value.find((contribution) => contribution.id === activePromotion.value?.contributionId)?.reward ?? 0;
  return baseReward * activePromotion.value.multiplier + activePromotion.value.teamworkBonus;
});
const promotionFor = (contributionId: ContributionId) =>
  localizedPromotions.value.find((promotion) => promotion.contributionId === contributionId && isPromotionAvailable(promotion, store.familyTimeZone, new Date(store.currentTimeMilliseconds)));
const invitedChildNames = (memberIds: FamilyMemberId[]) => store.members
  .filter((member) => member.role === 'child' && memberIds.includes(member.id))
  .map((member) => `${member.avatar} ${member.name}`);
const childMembers = computed(() => store.members.filter((member) => member.role === 'child'));
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

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.world-hero {
  padding-bottom: 16px;
  @apply position-relative overflow-hidden;
  color: var(--lad-palette-text);
  background: linear-gradient(
    180deg,
    var(--lad-palette-background) 0%,
    var(--lad-palette-background) 78%,
    var(--lad-palette-amber-100) 100%
  );
}
.world-title {
  max-width: 310px;
  @apply ma-0;
  font-size: 1.4375rem;
  line-height: 1.1;
  letter-spacing: -0.04em;
}
.energy-value {
  @apply d-block;
  color: var(--lad-mint-dark);
  font-size: 1.25rem;
  line-height: 1;
  @apply text-no-wrap;
}
.energy-label {
  @apply d-block mt-1;
  color: var(--lad-palette-muted);
  font-size: 0.625rem;
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
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 10%, transparent);
  border-radius: 17px;
  background: color-mix(in srgb, var(--lad-palette-white) 50%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
  font: inherit;
  @apply cursor-pointer;
  transition:
    background 160ms ease,
    transform 160ms ease,
    box-shadow 160ms ease;
}
.energy-trigger:hover {
  background: color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-teal-600) 10%, transparent);
}
.energy-trigger:active {
  transform: translateY(1px) scale(0.98);
  box-shadow: 0 1px 0
    color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
}
.energy-chevron {
  @apply position-absolute;
  top: 50%;
  right: 5px;
  color: var(--lad-mint-dark);
  transform: translateY(-50%);
}
@include respond-down(mobile) {
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
    font-size: 1.0625rem;
  }
  .energy-label {
    font-size: 0.5625rem;
  }
}
.contribution-board {
  padding: 15px;
  @apply position-relative overflow-hidden;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 18%, transparent);
  border-radius: 27px;
  background:
    radial-gradient(
      circle at 92% 7%,
      color-mix(in srgb, var(--lad-palette-amber-250) 20%, transparent),
      transparent 23%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-surface),
      var(--lad-palette-background)
    );
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-palette-teal-600) 10%, transparent),
    0 15px 27px color-mix(in srgb, var(--lad-palette-muted-700) 8%, transparent);
}
.contribution-board::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 10px;
  right: 13px;
  color: color-mix(in srgb, var(--lad-palette-amber-450) 65%, transparent);
  font-size: 0.8125rem;
}
.contribution-board-heading {
  @apply position-relative d-flex align-start justify-space-between;
  z-index: 1;
  gap: 12px;
  margin-bottom: 14px;
}
.contribution-board-heading h2 {
  @apply ma-0;
  font-size: 1.25rem;
  letter-spacing: -0.035em;
}
.contribution-board-heading > div > p:last-child {
  margin-top: 3px;
  color: var(--lad-muted);
  font-size: 0.625rem;
}
.all-contributions {
  min-height: 46px;
  padding: 7px 8px 7px 10px;
  @apply d-flex align-center flex-shrink-0;
  gap: 5px;
  color: var(--lad-palette-teal-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 20%, transparent);
  border-radius: 16px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-white)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-mint-strong) 12%, transparent);
  font-size: 0.625rem;
  font-weight: var(--lad-font-weight-heavy);
  text-decoration: none;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.all-contributions:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 0
    color-mix(in srgb, var(--lad-palette-mint-strong) 12%, transparent);
}
.contribution-list {
  @apply d-flex flex-column;
  gap: 11px;
}
.home-contribution-item {
  @apply position-relative overflow-hidden;
  border: 1px solid var(--lad-border);
  box-shadow: 0 4px 0 var(--lad-border) !important;
}
.home-task-icon {
  flex: 0 0 58px;
  border: 3px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  ) !important;
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-blue-550) 12%, transparent),
    0 8px 15px color-mix(in srgb, var(--lad-palette-teal-700) 8%, transparent);
  font-size: 1.9375rem !important;
  transform: rotate(-3deg);
  animation: home-task-icon-float 3.4s ease-in-out infinite;
}
.home-contribution-label {
  @include contribution-label;
}
.home-contribution-label--basic {
  @include contribution-label-basic;
}
.home-contribution-label--special {
  @include contribution-label-special;
}
.home-contribution-label--mine {
  @include contribution-label-mine;
  animation: home-mine-label-breathe 2.6s ease-in-out infinite;
}
.home-contribution-meta {
  @include contribution-meta-grid;
}
.home-meta-chip {
  @include contribution-meta-chip;
}
.home-meta-chip--reward {
  @include contribution-meta-chip-reward;
}
.home-meta-chip--energy {
  @include contribution-meta-chip-energy;
}
.home-invited-team {
  @apply d-flex align-center flex-wrap;
  gap: 5px;
}
.home-invited-team > span {
  color: var(--lad-muted);
  font-size: 0.625rem;
  font-weight: 800;
}
.home-finish-button {
  min-height: 48px !important;
  padding: 4px 16px 4px 7px !important;
  overflow: visible !important;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent) !important;
  border-radius: 17px !important;
  background: linear-gradient(
    145deg,
    var(--lad-palette-blue-350),
    var(--lad-palette-blue-strong)
  ) !important;
  box-shadow:
    0 5px 0 var(--lad-palette-blue-strong),
    0 10px 18px
      color-mix(in srgb, var(--lad-palette-blue-strong) 20%, transparent) !important;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: none;
  letter-spacing: 0;
}
.home-finish-button :deep(.v-btn__content) {
  gap: 7px;
}
.home-finish-button:active {
  transform: translateY(3px) scale(0.97);
  box-shadow: 0 2px 0 var(--lad-palette-blue-strong) !important;
}
.home-finish-check {
  width: 38px;
  height: 38px;
  @apply d-grid place-center;
  color: var(--lad-palette-blue-strong);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  border-radius: 13px;
  background: var(--lad-palette-white);
  box-shadow: inset 0 -3px 0
    color-mix(in srgb, var(--lad-palette-blue-strong) 12%, transparent);
}
.home-pending {
  min-height: 49px;
  padding: 7px 12px 7px 7px;
  @apply position-relative d-flex align-center overflow-hidden;
  gap: 8px;
  color: var(--lad-palette-amber-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-amber-450) 25%, transparent);
  border-radius: 17px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-amber-100),
    var(--lad-palette-background)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-amber-550) 12%, transparent),
    0 8px 15px color-mix(in srgb, var(--lad-palette-orange-600) 8%, transparent);
}
.home-pending::after {
  content: "✦";
  @apply position-absolute;
  top: 4px;
  right: 7px;
  color: var(--lad-palette-amber-500);
  font-size: 0.5625rem;
  animation: home-pending-spark 2s ease-in-out infinite;
}
.home-pending-icon {
  width: 35px;
  height: 35px;
  @apply d-grid place-center flex-shrink-0;
  border: 2px solid var(--lad-palette-white);
  border-radius: 12px;
  background: var(--lad-palette-amber-250);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-amber-600) 12%, transparent);
  font-size: 1.125rem;
  animation: home-pending-ready 2.5s ease-in-out infinite;
}
.home-pending strong,
.home-pending small {
  @apply d-block;
}
.home-pending strong {
  font-size: 0.75rem;
}
.home-pending small {
  margin-top: 1px;
  color: var(--lad-palette-muted-600);
  font-size: 0.5rem;
}
.contribution-empty {
  @apply d-flex align-center position-relative overflow-hidden;
  gap: 13px;
  border: 2px dashed
    color-mix(in srgb, var(--lad-palette-teal-550) 30%, transparent);
  background:
    radial-gradient(
      circle at 88% 18%,
      color-mix(in srgb, var(--lad-palette-amber-250) 25%, transparent),
      transparent 26%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-surface)
    );
  box-shadow: inset 0 0 0 5px
    color-mix(in srgb, var(--lad-palette-white) 35%, transparent) !important;
}
.contribution-empty strong {
  font-size: 0.9375rem;
}
.contribution-empty p:not(.eyebrow) {
  margin: 3px 0 8px;
  color: var(--lad-muted);
  font-size: 0.625rem;
  line-height: 1.4;
}
.empty-task-link {
  @apply d-inline-flex align-center;
  gap: 4px;
  color: var(--lad-mint-dark);
  font-size: 0.625rem;
  font-weight: var(--lad-font-weight-heavy);
  text-decoration: none;
}
.promotion-banner {
  @apply position-relative overflow-hidden cursor-pointer;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-blue-350) 40%, transparent);
  background: linear-gradient(
    135deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100) 54%,
    var(--lad-palette-background)
  ) !important;
  box-shadow:
    0 7px 0 color-mix(in srgb, var(--lad-palette-blue-450) 18%, transparent),
    0 14px 25px color-mix(in srgb, var(--lad-palette-blue-600) 8%, transparent) !important;
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
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent),
    transparent
  );
  animation: bonus-shimmer 3.4s ease-in-out infinite;
}
.promotion-boost {
  min-width: 78px;
  padding: 7px 9px;
  @apply position-relative d-grid place-center flex-shrink-0 text-center;
  z-index: 1;
  color: var(--lad-palette-blue-600);
  border: 3px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  border-radius: 18px;
  background: radial-gradient(
    circle at 25% 18%,
    var(--lad-palette-white),
    var(--lad-palette-background) 50%,
    var(--lad-palette-teal-150)
  );
  box-shadow:
    0 5px 0 var(--lad-palette-muted-250),
    0 10px 18px color-mix(in srgb, var(--lad-palette-blue-550) 15%, transparent);
  transform: rotate(3deg);
  animation: bonus-boost-pulse 2s ease-in-out infinite;
}
.promotion-boost strong,
.promotion-boost small {
  @apply d-block;
}
.promotion-boost strong {
  font-size: 1.4375rem;
  line-height: 1;
}
.promotion-boost small {
  margin-top: 3px;
  color: var(--lad-palette-teal-700);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-black);
  white-space: nowrap;
}
.promotion-stars {
  width: 85px;
  height: 44px;
  @apply position-absolute pointer-events-none;
  top: 6px;
  right: 8px;
}
.promotion-stars i {
  @apply position-absolute;
  color: var(--lad-palette-amber-500);
  text-shadow: 0 0 7px
    color-mix(in srgb, var(--lad-palette-amber-250) 95%, transparent);
  font-style: normal;
  opacity: 0;
  transform: scale(0.35);
  animation: bonus-star-one 3.7s ease-in-out infinite;
}
.promotion-stars i:nth-child(1) {
  top: 4px;
  left: 2px;
  font-size: 0.6875rem;
  animation-delay: -0.4s;
}
.promotion-stars i:nth-child(2) {
  top: 9px;
  left: 25px;
  color: var(--lad-palette-purple-350);
  font-size: 0.625rem;
  animation-name: bonus-star-two;
  animation-delay: -2.1s;
}
.promotion-stars i:nth-child(3) {
  top: 1px;
  right: 5px;
  font-size: 0.5625rem;
  animation-name: bonus-star-three;
  animation-delay: -1.2s;
}
.promotion-stars i:nth-child(4) {
  top: 24px;
  left: 12px;
  color: var(--lad-palette-muted-350);
  font-size: 0.5rem;
  animation-name: bonus-star-two;
  animation-delay: -3.3s;
}
.promotion-stars i:nth-child(5) {
  right: 20px;
  bottom: 4px;
  color: var(--lad-palette-pink-300);
  font-size: 0.5625rem;
  animation-name: bonus-star-three;
  animation-delay: -2.7s;
}
.promotion-stars i:nth-child(6) {
  right: 2px;
  bottom: 9px;
  color: var(--lad-palette-teal-400);
  font-size: 1rem;
  animation-delay: -1.8s;
}
.promotion-gem,
.promotion-detail-gem {
  @apply position-relative d-grid place-center flex-shrink-0;
  color: var(--lad-palette-amber-150);
  border: 3px solid var(--lad-palette-white);
  background: linear-gradient(
    145deg,
    var(--lad-palette-blue-250),
    var(--lad-palette-indigo-350) 58%,
    var(--lad-palette-purple-350)
  );
  box-shadow:
    0 5px 0 var(--lad-palette-violet-500),
    0 9px 16px color-mix(in srgb, var(--lad-palette-blue-600) 20%, transparent);
  transform: rotate(-5deg);
  animation: bonus-gem 2.4s ease-in-out infinite;
}
.promotion-gem {
  width: 49px;
  height: 49px;
  border-radius: 17px;
}
.promotion-gem > span {
  width: 19px;
  height: 19px;
  @apply position-absolute d-grid place-center;
  right: -7px;
  bottom: -5px;
  color: var(--lad-palette-amber-150);
  border: 2px solid var(--lad-palette-white);
  border-radius: 7px;
  background: var(--lad-palette-teal-400);
  box-shadow: 0 2px 0 var(--lad-palette-teal-600);
}
.promotion-rocket {
  color: var(--lad-palette-amber-150);
  filter: drop-shadow(
    0 2px 1px color-mix(in srgb, var(--lad-palette-violet-650) 25%, transparent)
  );
  animation: bonus-rocket-launch 2.1s ease-in-out infinite;
}
.promotion-banner-countdown {
  width: fit-content;
}
.promotion-detail-gem {
  width: 92px;
  height: 92px;
  margin-inline: auto;
  border-radius: 29px;
  box-shadow:
    0 7px 0 var(--lad-palette-violet-500),
    0 14px 25px color-mix(in srgb, var(--lad-palette-blue-600) 25%, transparent);
}
.promotion-detail-gem > span {
  width: 28px;
  height: 28px;
  @apply position-absolute d-grid place-center;
  right: -8px;
  bottom: -7px;
  color: var(--lad-palette-white);
  border: 3px solid var(--lad-palette-white);
  border-radius: 10px;
  background: var(--lad-palette-teal-400);
  box-shadow: 0 3px 0 var(--lad-palette-teal-600);
}
.promotion-gem i,
.promotion-detail-gem i {
  @apply position-absolute;
  color: var(--lad-palette-amber-150);
  font-style: normal;
  text-shadow: 0 0 7px var(--lad-palette-white);
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
.promotion-detail-card {
  @apply position-relative overflow-hidden text-center;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-blue-450) 20%, transparent);
  background:
    radial-gradient(
      circle at 88% 4%,
      color-mix(in srgb, var(--lad-palette-teal-400) 18%, transparent),
      transparent 27%
    ),
    linear-gradient(
      155deg,
      var(--lad-palette-background),
      var(--lad-palette-background) 60%,
      var(--lad-palette-background)
    ) !important;
  box-shadow:
    0 9px 0 color-mix(in srgb, var(--lad-palette-blue-550) 15%, transparent),
    0 25px 55px
      color-mix(in srgb, var(--lad-palette-indigo-750) 25%, transparent) !important;
}
.promotion-detail-card::after {
  content: "✦";
  @apply position-absolute;
  top: 22px;
  right: 26px;
  color: var(--lad-palette-teal-400);
  font-size: 1.5rem;
  opacity: 0.65;
}
.promotion-detail-eyebrow {
  color: var(--lad-palette-mint-strong);
}
.promotion-reward-card {
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-blue-450) 18%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-background)
  ) !important;
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-blue-550) 10%, transparent) !important;
}
.promotion-start {
  min-height: 49px !important;
  color: var(--lad-palette-white) !important;
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-blue-strong)
  ) !important;
  box-shadow:
    0 5px 0 var(--lad-palette-blue-600),
    0 10px 17px color-mix(in srgb, var(--lad-palette-blue-600) 15%, transparent) !important;
  font-weight: var(--lad-font-weight-heavy);
  text-transform: none;
  letter-spacing: 0;
}
.promotion-start :deep(.v-btn__content) {
  gap: 8px;
}
.promotion-start :deep(.v-btn__content) > span {
  font-size: 1.25rem;
  animation: bonus-gem 2.2s ease-in-out infinite;
}
.promotion-close {
  @apply position-absolute;
  top: 12px;
  right: 12px;
  z-index: 3;
  color: var(--lad-palette-teal-600) !important;
  background: color-mix(
    in srgb,
    var(--lad-palette-white) 80%,
    transparent
  ) !important;
}
.promotion-breakdown {
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}
.promotion-breakdown > span {
  min-height: 55px;
  padding: 7px;
  @apply d-grid place-center;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-blue-450) 15%, transparent);
  border-radius: 14px;
  background: color-mix(in srgb, var(--lad-palette-white) 75%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-blue-550) 8%, transparent);
}
.promotion-breakdown i,
.promotion-breakdown small {
  @apply d-block;
}
.promotion-breakdown i {
  color: var(--lad-palette-blue-600);
  font-size: 1.25rem;
  font-style: normal;
  font-weight: var(--lad-font-weight-black);
  line-height: 1;
}
.promotion-breakdown small {
  margin-top: 3px;
  color: var(--lad-muted);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-strong);
}
.promotion-dialog-title {
  @apply ma-0;
  font-size: 1.375rem;
  letter-spacing: -0.03em;
}
.promotion-total {
  color: var(--lad-palette-blue-600);
  font-size: 1.75rem;
}
.section-title {
  @apply ma-0;
  font-size: 1.1875rem;
  letter-spacing: -0.025em;
}
.rating-rule {
  color: var(--lad-palette-amber-650);
  font-size: 0.5625rem;
  @apply font-weight-black;
}
.management-card {
  border: 1px solid color-mix(in srgb, var(--lad-palette-blue) 20%, transparent);
}
.supporter-card {
  border: 1px solid color-mix(in srgb, var(--lad-palette-mint) 25%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-mint) 12%, transparent) !important;
}
.management-actions {
  @apply d-grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 7px;
}
.management-actions :deep(.v-btn) {
  @apply min-w-0;
  padding-inline: 8px;
  font-size: 0.5625rem;
}
.direct-gift-icon {
  width: 58px;
  height: 58px;
  @apply d-grid place-center;
  border-radius: 19px;
  background: var(--lad-palette-amber-100);
  font-size: 1.9375rem;
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
@keyframes bonus-boost-pulse {
  0%,
  100% {
    transform: rotate(3deg) scale(1);
  }
  50% {
    transform: rotate(-2deg) scale(1.07);
    filter: brightness(1.04);
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
@keyframes bonus-rocket-launch {
  0%,
  70%,
  100% {
    transform: translate(0, 0) rotate(-4deg);
  }
  78% {
    transform: translate(3px, -4px) rotate(4deg) scale(1.08);
  }
  88% {
    transform: translate(-1px, 1px) rotate(-2deg);
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
@keyframes bonus-star-one {
  0%,
  18%,
  42%,
  100% {
    opacity: 0;
    transform: scale(0.3) rotate(-12deg);
  }
  25%,
  34% {
    opacity: 1;
    transform: scale(1.15) rotate(16deg);
  }
}
@keyframes bonus-star-two {
  0%,
  37%,
  63%,
  100% {
    opacity: 0;
    transform: scale(0.25) rotate(12deg);
  }
  44%,
  55% {
    opacity: 0.9;
    transform: scale(1.08) rotate(-15deg);
  }
}
@keyframes bonus-star-three {
  0%,
  58%,
  87%,
  100% {
    opacity: 0;
    transform: scale(0.3) rotate(-8deg);
  }
  67%,
  78% {
    opacity: 1;
    transform: scale(1.2) rotate(21deg);
  }
}
@keyframes home-task-icon-float {
  0%,
  68%,
  100% {
    transform: translateY(0) rotate(-3deg);
  }
  78% {
    transform: translateY(-3px) rotate(3deg) scale(1.04);
  }
  88% {
    transform: translateY(1px) rotate(-1deg);
  }
}
@keyframes home-mine-label-breathe {
  0%,
  100% {
    box-shadow: 0 3px 0
      color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
  }
  50% {
    box-shadow:
      0 3px 0 color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent),
      0 0 0 4px color-mix(in srgb, var(--lad-palette-green-500) 8%, transparent);
  }
}
@keyframes home-pending-ready {
  0%,
  75%,
  100% {
    transform: rotate(0) scale(1);
  }
  84% {
    transform: rotate(-8deg) scale(1.1);
  }
  92% {
    transform: rotate(6deg);
  }
}
@keyframes home-pending-spark {
  0%,
  100% {
    opacity: 0.3;
    transform: scale(0.7);
  }
  50% {
    opacity: 1;
    transform: scale(1.15) rotate(20deg);
  }
}
@include reduced-motion {
  .promotion-stars i,
  .promotion-boost,
  .promotion-rocket,
  .home-task-icon,
  .home-contribution-label--mine,
  .home-pending::after,
  .home-pending-icon {
    animation: none;
  }
  .promotion-stars i {
    opacity: 0.75;
    transform: none;
  }
}
@include respond-down(phone) {
  .home-contribution-meta {
    @include contribution-meta-grid;
  }
  .home-meta-chip--energy {
    @include contribution-meta-chip-energy;
  }
}
@include respond-down(mobile) {
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
