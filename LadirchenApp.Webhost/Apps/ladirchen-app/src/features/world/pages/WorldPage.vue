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
          <v-icon class="energy-chevron" icon="i-mdi:chevron-right" size="15" />
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
        :room-designs="activeRoomDesigns"
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
        <BrandedCard class="management-card pa-4 mb-6" tone="world">
          <div class="d-flex align-center justify-space-between ga-3 mb-3">
            <div><strong>{{ t('world.management.title') }}</strong><p class="text-caption text-medium-emphasis">{{ t('world.management.description', { name: store.activeChild.name }) }}</p></div>
            <v-icon color="info">i-mdi:shield-account-outline</v-icon>
          </div>
          <div class="management-actions">
            <v-btn color="primary" prepend-icon="i-mdi:plus" rounded="lg" size="small" to="/beitraege?new=1" variant="flat">{{ t('world.management.contribution') }}</v-btn>
            <v-btn color="warning" prepend-icon="i-mdi:rocket-launch-outline" rounded="lg" size="small" to="/beitraege?promotion=1" variant="tonal">{{ t('world.management.promotion') }}</v-btn>
            <v-btn color="info" prepend-icon="i-mdi:target" rounded="lg" size="small" to="/wuensche?new=1" variant="tonal">{{ t('world.management.childGoal') }}</v-btn>
            <v-btn color="info" prepend-icon="i-mdi:account-group-outline" rounded="lg" size="small" to="/wuensche?family=1" variant="tonal">{{ t('world.management.familyGoal') }}</v-btn>
            <v-btn color="warning" prepend-icon="i-mdi:gift-outline" rounded="lg" size="small" to="/shop?new=1" variant="tonal">{{ t('world.management.reward') }}</v-btn>
            <v-btn color="secondary" prepend-icon="i-mdi:hand-coin-outline" rounded="lg" size="small" variant="tonal" @click="openGiftDialog">{{ t('world.management.gift') }}</v-btn>
            <v-btn color="primary" prepend-icon="i-mdi:account-cog-outline" rounded="lg" size="small" variant="tonal" @click="store.openFamilySetup">{{ t('world.management.family') }}</v-btn>
            <v-btn color="success" prepend-icon="i-mdi:piggy-bank-outline" rounded="lg" size="small" variant="tonal" @click="store.piggyBankOpen = true">{{ t('world.management.money') }}</v-btn>
          </div>
        </BrandedCard>

      </template>

      <BrandedCard v-else-if="store.viewerRole === 'guardian'" class="supporter-card pa-4 mb-6" tone="world">
        <div class="d-flex align-center ga-3">
          <v-avatar color="primary" size="46" variant="tonal"><v-icon icon="i-mdi:hand-heart-outline" /></v-avatar>
          <div class="flex-grow-1 min-w-0">
            <p class="eyebrow mb-1">{{ t('world.supporter.eyebrow') }}</p>
            <strong>{{ t('world.supporter.title') }}</strong>
            <p class="text-caption text-medium-emphasis mt-1">{{ t('world.supporter.description') }}</p>
          </div>
        </div>
        <v-btn class="mt-4" color="primary" prepend-icon="i-mdi:gift-outline" rounded="lg" to="/wuensche" variant="flat" width="100%">{{ t('world.supporter.action') }}</v-btn>
      </BrandedCard>

      <v-card v-if="store.viewerRole === 'child' && activePromotion" class="promotion-banner pa-4 mb-6" color="amber-lighten-5" elevation="0" rounded="xl" role="button" tabindex="0" @click="promoDetailsOpen = true" @keydown.enter="promoDetailsOpen = true">
        <div class="promotion-stars" aria-hidden="true"><i>✦</i><i>✧</i><i>★</i><i>✦</i><i>✧</i><i>·</i></div>
        <div class="d-flex align-center ga-3">
          <div class="promotion-gem" aria-hidden="true"><v-icon class="promotion-rocket" icon="i-mdi:rocket-launch" size="29" /><span><v-icon icon="i-mdi:diamond-stone" size="12" /></span><i>✦</i><i>✧</i></div>
          <div class="flex-grow-1">
            <p class="eyebrow mb-1">{{ t('world.promotion.eyebrow') }}</p>
            <strong>{{ activePromotion.title }}</strong>
            <p class="text-caption text-medium-emphasis">{{ t('world.promotion.until', { title: contributionTitle(activePromotion.contributionId), time: activePromotion.deadline }) }}</p>
            <PromotionCountdown class="promotion-banner-countdown mt-2" :deadline="activePromotion.deadline" :time-zone="store.familyTimeZone" />
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
          <RouterLink class="all-contributions" to="/beitraege"><v-icon icon="i-mdi:view-grid-plus-outline" size="19" /><span>{{ t('world.tasks.all') }}</span><v-icon icon="i-mdi:chevron-right" size="17" /></RouterLink>
        </div>

        <div v-if="personalContributions.length" class="contribution-list">
          <ChildContributionCard
            v-for="contribution in personalContributions"
            :key="contribution.id"
            :active-child-id="store.activeChildId"
            :allow-invite="false"
            :contribution="contribution"
            :family-members="store.members"
            :family-time-zone="store.familyTimeZone"
            :promotion="promotionFor(contribution.id)"
            :reward="store.rewardForContribution(contribution.id)"
            :tip="contribution.description"
            @claim="store.claimContribution($event.id)"
            @submit="store.submitContribution($event.id)"
          />
        </div>

        <v-card v-else class="contribution-empty pa-4" elevation="0" rounded="xl">
          <AnimatedCompletionMark :size="80" />
          <div class="flex-grow-1 min-w-0">
            <p class="eyebrow mb-1">{{ emptyTaskEyebrow }}</p>
            <strong>{{ emptyTaskTitle }}</strong>
            <p>{{ emptyTaskMessage }}</p>
            <RouterLink class="empty-task-link" to="/beitraege">{{ t('world.tasks.openAvailable') }} <v-icon icon="i-mdi:arrow-right" size="15" /></RouterLink>
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
          <v-btn color="secondary" :disabled="!canGiveGift" prepend-icon="i-mdi:gift-outline" rounded="lg" variant="flat" @click="giveDirectGift">{{ t('world.gift.submit') }}</v-btn>
        </div>
      </v-card>
    </v-dialog>

    <v-dialog v-model="promoDetailsOpen" max-width="410">
      <v-card v-if="activePromotion" class="promotion-detail-card pa-5" rounded="xl">
        <v-btn class="promotion-close" :aria-label="t('world.promotion.close')" icon="i-mdi:close" size="small" variant="tonal" @click="promoDetailsOpen = false" />
        <div class="promotion-detail-gem" aria-hidden="true"><v-icon icon="i-mdi:star-four-points" size="55" /><span><v-icon icon="i-mdi:diamond-stone" size="18" /></span><i>✦</i><i>✧</i></div>
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
        <v-btn class="promotion-start mt-5" color="warning" rounded="lg" variant="flat" width="100%" @click="promoDetailsOpen = false"><span aria-hidden="true">★</span>{{ t('world.promotion.cta') }}<v-icon icon="i-mdi:arrow-right" /></v-btn>
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
import ChildContributionCard from '@/shared/components/contributions/ChildContributionCard.vue';
import AnimatedCompletionMark from '@/shared/components/AnimatedCompletionMark.vue';
import PromotionCountdown from '@/shared/components/contributions/PromotionCountdown.vue';
import BrandedCard from '@/shared/components/ui/BrandedCard.vue';
import type { ContributionId } from '@/domain/shared/identifiers';
import { isPromotionAvailable } from '@/domain/contributions/promotions';
import { useLocalizedDomainContent } from '@/shared/composables/use-localized-domain-content';
import { useFamilyWorldStore } from '@/stores/family-world';
import { ROOM_DESIGNS } from '@/domain/house';

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
const activeRoomDesigns = computed(() => ROOM_DESIGNS.filter(
  design => store.selectedRoomDesignIds[design.zoneId] === design.id && store.ownedRoomDesignIds.includes(design.id),
));
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
  color: var(--lad-text);
  background: linear-gradient(
    180deg,
    var(--lad-surface-soft) 0%,
    var(--lad-surface-soft) 78%,
    var(--lad-color-reward-soft) 100%
  );
}
.world-title {
  max-width: 310px;
  @apply ma-0;
  font-size: rem(23);
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
  color: var(--lad-muted);
  font-size: rem(10);
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
    color-mix(in srgb, var(--lad-color-primary-muted) 10%, transparent);
  border-radius: 17px;
  background: color-mix(in srgb, var(--lad-surface-raised) 50%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary-supporting) 8%, transparent);
  font: inherit;
  @apply cursor-pointer;
  transition:
    background 160ms ease,
    transform 160ms ease,
    box-shadow 160ms ease;
}
.energy-trigger:hover {
  background: color-mix(in srgb, var(--lad-surface-raised) 80%, transparent);
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-primary-supporting) 10%, transparent);
}
.energy-trigger:active {
  transform: translateY(1px) scale(0.98);
  box-shadow: 0 1px 0
    color-mix(in srgb, var(--lad-color-primary-supporting) 8%, transparent);
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
    width: 35px;
    height: 31px;
  }
  .energy-value {
    font-size: rem(17);
  }
  .energy-label {
    font-size: rem(9);
  }
}
.contribution-board {
  --contribution-board-background:
    radial-gradient(
      circle at 92% 7%,
      color-mix(in srgb, var(--lad-color-reward-highlight) 20%, transparent),
      transparent 23%
    ),
    linear-gradient(145deg, var(--lad-surface), var(--lad-surface-soft));
  padding: 15px;
  @apply position-relative overflow-hidden;
  @include content-card(
    color-mix(in srgb, var(--lad-color-primary-muted) 18%, transparent),
    color-mix(in srgb, var(--lad-color-primary-supporting) 10%, transparent),
    var(--contribution-board-background),
    27px,
    2px,
    7px,
    0 15px 27px color-mix(in srgb, var(--lad-text-strong) 8%, transparent)
  );
}
.contribution-board::after {
  content: "✦";
  @apply position-absolute pointer-events-none;
  top: 10px;
  right: 13px;
  color: color-mix(in srgb, var(--lad-color-reward-border) 65%, transparent);
  font-size: rem(13);
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
  font-size: rem(10);
}
.all-contributions {
  min-height: 46px;
  padding: 7px 8px 7px 10px;
  @apply d-flex align-center flex-shrink-0;
  gap: 5px;
  color: var(--lad-color-primary-deep);
  border: 2px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 20%, transparent);
  border-radius: 16px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-surface-raised)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-primary-strong) 12%, transparent);
  font-size: rem(10);
  font-weight: var(--lad-font-weight-heavy);
  text-decoration: none;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.all-contributions:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 0
    color-mix(in srgb, var(--lad-color-primary-strong) 12%, transparent);
}
.contribution-list {
  @apply d-flex flex-column;
  gap: 11px;
}
.contribution-empty {
  @apply d-flex align-center position-relative overflow-hidden;
  gap: 13px;
  border: 2px dashed
    color-mix(in srgb, var(--lad-color-primary-muted) 30%, transparent);
  background:
    radial-gradient(
      circle at 88% 18%,
      color-mix(in srgb, var(--lad-color-reward-highlight) 25%, transparent),
      transparent 26%
    ),
    linear-gradient(145deg, var(--lad-surface-soft), var(--lad-surface));
  box-shadow: inset 0 0 0 5px
    color-mix(in srgb, var(--lad-surface-raised) 35%, transparent);
}
.contribution-empty strong {
  font-size: rem(15);
}
.contribution-empty p:not(.eyebrow) {
  margin: 3px 0 8px;
  color: var(--lad-muted);
  font-size: rem(10);
  line-height: 1.4;
}
.empty-task-link {
  @apply d-inline-flex align-center;
  gap: 4px;
  color: var(--lad-mint-dark);
  font-size: rem(10);
  font-weight: var(--lad-font-weight-heavy);
  text-decoration: none;
}
.promotion-banner {
  --promotion-banner-background: linear-gradient(
    135deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft) 54%,
    var(--lad-surface-soft)
  );
  @apply position-relative overflow-hidden cursor-pointer;
  @include content-card(
    color-mix(in srgb, var(--lad-color-info-subtle) 40%, transparent),
    color-mix(in srgb, var(--lad-color-info-muted) 18%, transparent),
    var(--promotion-banner-background),
    var(--lad-radius-large),
    2px,
    7px,
    0 14px 25px color-mix(in srgb, var(--lad-color-info-deep) 8%, transparent)
  );
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
    color-mix(in srgb, var(--lad-surface-raised) 80%, transparent),
    transparent
  );
  animation: bonus-shimmer 3.4s ease-in-out infinite;
}
.promotion-boost {
  min-width: 78px;
  padding: 7px 9px;
  @apply position-relative d-grid place-center flex-shrink-0 text-center;
  z-index: 1;
  color: var(--lad-color-info-deep);
  border: 3px solid
    color-mix(in srgb, var(--lad-surface-raised) 90%, transparent);
  border-radius: 18px;
  background: radial-gradient(
    circle at 25% 18%,
    var(--lad-surface-raised),
    var(--lad-surface-soft) 50%,
    var(--lad-color-primary-soft)
  );
  box-shadow:
    0 5px 0 var(--lad-border),
    0 10px 18px
      color-mix(in srgb, var(--lad-color-info-shadow) 15%, transparent);
  transform: rotate(3deg);
  animation: bonus-boost-pulse 2s ease-in-out infinite;
}
.promotion-boost strong,
.promotion-boost small {
  @apply d-block;
}
.promotion-boost strong {
  font-size: rem(23);
  line-height: 1;
}
.promotion-boost small {
  margin-top: 3px;
  color: var(--lad-color-primary-deep);
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
  color: var(--lad-color-reward-accent);
  text-shadow: 0 0 7px
    color-mix(in srgb, var(--lad-color-reward-highlight) 95%, transparent);
  font-style: normal;
  opacity: 0;
  transform: scale(0.35);
  animation: bonus-star-one 3.7s ease-in-out infinite;
}
.promotion-stars i:nth-child(1) {
  top: 4px;
  left: 2px;
  font-size: rem(11);
  animation-delay: -0.4s;
}
.promotion-stars i:nth-child(2) {
  top: 9px;
  left: 25px;
  color: var(--lad-color-bonus-highlight);
  font-size: rem(10);
  animation-name: bonus-star-two;
  animation-delay: -2.1s;
}
.promotion-stars i:nth-child(3) {
  top: 1px;
  right: 5px;
  font-size: rem(9);
  animation-name: bonus-star-three;
  animation-delay: -1.2s;
}
.promotion-stars i:nth-child(4) {
  top: 24px;
  left: 12px;
  color: var(--lad-text-subtle);
  font-size: 0.5rem;
  animation-name: bonus-star-two;
  animation-delay: -3.3s;
}
.promotion-stars i:nth-child(5) {
  right: 20px;
  bottom: 4px;
  color: var(--lad-color-accent-pink);
  font-size: rem(9);
  animation-name: bonus-star-three;
  animation-delay: -2.7s;
}
.promotion-stars i:nth-child(6) {
  right: 2px;
  bottom: 9px;
  color: var(--lad-color-primary-highlight);
  font-size: 1rem;
  animation-delay: -1.8s;
}
.promotion-gem,
.promotion-detail-gem {
  @apply position-relative d-grid place-center flex-shrink-0;
  color: var(--lad-color-reward-pale);
  border: 3px solid var(--lad-surface-raised);
  background: linear-gradient(
    145deg,
    var(--lad-color-info-highlight),
    var(--lad-color-bonus-info) 58%,
    var(--lad-color-bonus-highlight)
  );
  box-shadow:
    0 5px 0 var(--lad-color-bonus-muted),
    0 9px 16px color-mix(in srgb, var(--lad-color-info-deep) 20%, transparent);
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
  color: var(--lad-color-reward-pale);
  border: 2px solid var(--lad-surface-raised);
  border-radius: 7px;
  background: var(--lad-color-primary-highlight);
  box-shadow: 0 2px 0 var(--lad-color-primary-supporting);
}
.promotion-rocket {
  color: var(--lad-color-reward-pale);
  filter: drop-shadow(
    0 2px 1px color-mix(in srgb, var(--lad-color-bonus-strong) 25%, transparent)
  );
  animation: bonus-rocket-launch 2.1s ease-in-out infinite;
}
.promotion-banner-countdown {
  width: fit-content;
}
.promotion-detail-gem {
  width: 92px;
  height: 92px;
  @apply mx-auto;
  border-radius: 29px;
  box-shadow:
    0 7px 0 var(--lad-color-bonus-muted),
    0 14px 25px color-mix(in srgb, var(--lad-color-info-deep) 25%, transparent);
}
.promotion-detail-gem > span {
  width: 28px;
  height: 28px;
  @apply position-absolute d-grid place-center;
  right: -8px;
  bottom: -7px;
  color: var(--lad-surface-raised);
  border: 3px solid var(--lad-surface-raised);
  border-radius: 10px;
  background: var(--lad-color-primary-highlight);
  box-shadow: 0 3px 0 var(--lad-color-primary-supporting);
}
.promotion-gem i,
.promotion-detail-gem i {
  @apply position-absolute;
  color: var(--lad-color-reward-pale);
  font-style: normal;
  text-shadow: 0 0 7px var(--lad-surface-raised);
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
    color-mix(in srgb, var(--lad-color-info-muted) 20%, transparent);
  background:
    radial-gradient(
      circle at 88% 4%,
      color-mix(in srgb, var(--lad-color-primary-highlight) 18%, transparent),
      transparent 27%
    ),
    linear-gradient(
      155deg,
      var(--lad-surface-soft),
      var(--lad-surface-soft) 60%,
      var(--lad-surface-soft)
    );
  box-shadow:
    0 9px 0 color-mix(in srgb, var(--lad-color-info-shadow) 15%, transparent),
    0 25px 55px
      color-mix(in srgb, var(--lad-color-bonus-info-deep) 25%, transparent);
}
.promotion-detail-card::after {
  content: "✦";
  @apply position-absolute;
  top: 22px;
  right: 26px;
  color: var(--lad-color-primary-highlight);
  font-size: 1.5rem;
  opacity: 0.65;
}
.promotion-detail-eyebrow {
  color: var(--lad-color-primary-strong);
}
.promotion-reward-card {
  border: 2px solid
    color-mix(in srgb, var(--lad-color-info-muted) 18%, transparent);
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-surface-soft)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-info-shadow) 10%, transparent);
}
.promotion-start {
  min-height: 49px;
  color: var(--lad-surface-raised);
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-info-strong)
  );
  box-shadow:
    0 5px 0 var(--lad-color-info-deep),
    0 10px 17px color-mix(in srgb, var(--lad-color-info-deep) 15%, transparent);
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
  color: var(--lad-color-primary-supporting);
  background: color-mix(in srgb, var(--lad-surface-raised) 80%, transparent);
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
    color-mix(in srgb, var(--lad-color-info-muted) 15%, transparent);
  border-radius: 14px;
  background: color-mix(in srgb, var(--lad-surface-raised) 75%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-info-shadow) 8%, transparent);
}
.promotion-breakdown i,
.promotion-breakdown small {
  @apply d-block;
}
.promotion-breakdown i {
  color: var(--lad-color-info-deep);
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
  font-size: rem(22);
  letter-spacing: -0.03em;
}
.promotion-total {
  color: var(--lad-color-info-deep);
  font-size: 1.75rem;
}
.section-title {
  @include section-title(rem(19));
}
.rating-rule {
  color: var(--lad-color-reward-ink);
  font-size: rem(9);
  @apply font-weight-black;
}
.management-actions {
  @apply d-grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 7px;
}
.management-actions :deep(.v-btn) {
  @apply min-w-0;
  padding-inline: 8px;
  font-size: rem(9);
}
.direct-gift-icon {
  width: 58px;
  height: 58px;
  @apply d-grid place-center;
  border-radius: 19px;
  background: var(--lad-color-reward-soft);
  font-size: rem(31);
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
@include reduced-motion {
  .promotion-stars i,
  .promotion-boost,
  .promotion-rocket {
    animation: none;
  }
  .promotion-stars i {
    opacity: 0.75;
    transform: none;
  }
}
@include respond-down(mobile) {
  .contribution-board {
    --contribution-board-padding: 13px;
  }
  .contribution-board-heading {
    @apply align-stretch;
    @apply flex-column;
  }
  .all-contributions {
    width: fit-content;
    min-height: 38px;
  }
  .contribution-empty {
    @apply align-start;
  }
  .contribution-empty :deep(.ladi-mascot) {
    transform: scale(0.9);
    transform-origin: top left;
  }
}
</style>
