<template>
  <BrandedCard class="reward-card pa-4 position-relative h-100 d-flex flex-column" :class="{ 'reward-card--hidden': reward.isVisible === false, 'reward-card--scheduled': publicationStatus.tone === 'scheduled' }" :data-ladi-heading="t('shop.rewards.rule')" :data-ladi-tip="reward.conditions" tone="shop">
    <div v-if="store.viewerRole === 'guardian'" class="reward-admin-toolbar d-flex align-center justify-space-between" data-ladi-ignore>
      <span class="reward-publication-state d-inline-flex align-center" :class="`reward-publication-state--${publicationStatus.tone}`"><v-icon :icon="publicationStatus.icon" />{{ publicationStatus.label }}</span>
      <span class="reward-admin-actions d-flex">
        <button :aria-label="visibilityActionLabel" class="reward-visibility-button d-grid place-center cursor-pointer" type="button" @click="store.setShopRewardVisibility(reward.id, reward.isVisible === false)"><v-icon :icon="visibilityActionIcon" /></button>
        <button :aria-label="t('shop.rewards.deleteAria', { title: reward.title })" class="reward-delete-button d-grid place-center cursor-pointer" type="button" @click="emit('delete', reward)"><v-icon icon="i-mdi:trash-can-outline" /></button>
      </span>
    </div>
    <div class="reward-icon d-grid place-center">{{ reward.icon }}</div><strong class="d-block mt-3">{{ reward.title }}</strong><p class="reward-description text-caption text-medium-emphasis mt-1">{{ reward.description }}</p>
    <div class="reward-actions d-flex align-center justify-space-between ga-2 mt-auto" data-ladi-ignore>
      <span class="price d-flex align-center"><LadirchenAmount :value="reward.price" /></span>
      <template v-if="store.viewerRole === 'child'">
        <v-btn v-if="reward.status === 'available'" class="request-reward-button" color="info" :disabled="!canRequest" prepend-icon="i-mdi:gift-open-outline" rounded="lg" size="small" variant="flat" @click="store.requestShopReward(reward.id)">{{ t('shop.rewards.request') }}</v-btn>
        <v-btn v-else-if="reward.status === 'requested' && reward.requesterId === store.activeChildId" color="warning" rounded="lg" size="small" variant="tonal" @click="store.cancelShopRewardRequest(reward.id)">{{ t('common.cancel') }}</v-btn>
        <v-chip v-else :color="childStatusColor" size="small" variant="tonal">{{ childStatusLabel }}</v-chip>
      </template>
      <template v-else><div v-if="reward.status === 'requested'" class="d-flex ga-1"><v-btn :aria-label="t('shop.rewards.reject')" icon="i-mdi:close" size="small" variant="tonal" @click="store.decideShopReward(reward.id, false)" /><v-btn :aria-label="t('shop.rewards.approve')" color="primary" :disabled="!redemptionOpen" icon="i-mdi:check" size="small" variant="flat" @click="store.decideShopReward(reward.id, true)" /></div><v-chip v-else :color="guardianStatusColor" size="small" variant="tonal">{{ guardianStatusLabel }}</v-chip></template>
    </div>
  </BrandedCard>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import type { ShopReward } from "@/domain/shop";
import LadirchenAmount from "@/shared/components/LadirchenAmount.vue";
import { useFamilyWorldStore } from "@/stores/family-world";
import BrandedCard from "@/shared/components/ui/BrandedCard.vue";
import type { ShopRewardPublicationStatus } from "@/features/shop/shop-view-models";

interface ShopRewardCardProps {
  readonly canRequest: boolean;
  readonly publicationStatus: ShopRewardPublicationStatus;
  readonly redemptionOpen: boolean;
  readonly reward: ShopReward;
}

const props = defineProps<ShopRewardCardProps>();
const emit = defineEmits<{ delete: [reward: ShopReward] }>();
const store = useFamilyWorldStore();
const { t } = useI18n();
const rewardIsHidden = computed(() => props.reward.isVisible === false);
const rewardIsRedeemed = computed(() => props.reward.status === "redeemed");
const visibilityActionLabel = computed(() => t(rewardIsHidden.value ? "shop.rewards.show" : "shop.rewards.hide", { title: props.reward.title }));
const visibilityActionIcon = computed(() => rewardIsHidden.value ? "i-mdi:eye-outline" : "i-mdi:eye-off-outline");
const childStatusColor = computed(() => rewardIsRedeemed.value ? "success" : "warning");
const childStatusLabel = computed(() => t(rewardIsRedeemed.value ? "shop.rewards.redeemed" : "shop.rewards.requested"));
const guardianStatusColor = computed(() => rewardIsRedeemed.value ? "success" : "primary");
const guardianStatusLabel = computed(() => t(rewardIsRedeemed.value ? "shop.rewards.redeemed" : "shop.rewards.available"));
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.reward-card--hidden {
  opacity: 0.68;
  filter: saturate(0.65);
}
.reward-card--scheduled {
  border-style: dashed;
}
.reward-admin-toolbar {
  min-height: rem(28);
  margin: rem(-5) rem(-5) rem(8);
  gap: rem(6);
}
.reward-publication-state {
  min-width: 0;
  padding: rem(4) rem(7);
  gap: rem(4);
  border-radius: var(--lad-radius-pill);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
}
.reward-publication-state--visible {
  color: var(--lad-color-primary-deep);
  background: var(--lad-surface-soft);
}
.reward-publication-state--scheduled {
  color: var(--lad-color-info-deep);
  background: var(--lad-surface-soft);
}
.reward-publication-state--hidden,
.reward-publication-state--expired {
  color: var(--lad-text-supporting);
  background: var(--lad-surface-soft);
}
.reward-admin-actions {
  gap: rem(4);
}
.reward-visibility-button,
.reward-delete-button {
  width: rem(28);
  height: rem(28);
  border: rem(1) solid var(--lad-border);
  border-radius: rem(10);
  background: var(--lad-surface-raised);
}
.reward-delete-button {
  color: var(--lad-color-danger-muted);
}
.reward-icon {
  width: rem(78);
  height: 6rem;
  border-radius: rem(16);
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  font-size: rem(42);
}
.reward-description {
  min-height: rem(47);
}
.reward-actions {
  padding-top: rem(16);
  flex-wrap: wrap;
}
.price {
  min-height: rem(38);
  padding: rem(5) rem(10);
  gap: rem(7);
  color: var(--lad-color-reward-strong);
  border-radius: var(--lad-radius-pill);
  background: var(--lad-color-reward-soft);
  font-size: rem(13);
  font-weight: var(--lad-font-weight-black);
}
.request-reward-button {
  @include action-button(
    linear-gradient(
      145deg,
      var(--lad-color-info-subtle),
      var(--lad-color-info)
    ),
    var(--lad-color-info-strong),
    var(--lad-text-inverse),
    2.5rem,
    rem(15),
    0.25rem rem(13)
  );
}
</style>
