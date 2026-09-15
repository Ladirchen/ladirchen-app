<template>
  <BrandedCard class="reward-card pa-4" :class="{ 'reward-card--hidden': reward.isVisible === false, 'reward-card--scheduled': publicationStatus.tone === 'scheduled' }" :data-ladi-heading="t('shop.rewards.rule')" :data-ladi-tip="reward.conditions" tone="shop">
    <div v-if="store.viewerRole === 'guardian'" class="reward-admin-toolbar" data-ladi-ignore>
      <span class="reward-publication-state" :class="`reward-publication-state--${publicationStatus.tone}`"><v-icon :icon="publicationStatus.icon" />{{ publicationStatus.label }}</span>
      <span class="reward-admin-actions">
        <button :aria-label="reward.isVisible === false ? t('shop.rewards.show', { title: reward.title }) : t('shop.rewards.hide', { title: reward.title })" class="reward-visibility-button" type="button" @click="store.setShopRewardVisibility(reward.id, reward.isVisible === false)"><v-icon :icon="reward.isVisible === false ? 'i-mdi:eye-outline' : 'i-mdi:eye-off-outline'" /></button>
        <button :aria-label="t('shop.rewards.deleteAria', { title: reward.title })" class="reward-delete-button" type="button" @click="emit('delete', reward)"><v-icon icon="i-mdi:trash-can-outline" /></button>
      </span>
    </div>
    <div class="reward-icon">{{ reward.icon }}</div><strong class="d-block mt-3">{{ reward.title }}</strong><p class="reward-description text-caption text-medium-emphasis mt-1">{{ reward.description }}</p>
    <div class="reward-actions d-flex align-center justify-space-between ga-2" data-ladi-ignore>
      <span class="price"><LadirchenAmount :value="reward.price" /></span>
      <template v-if="store.viewerRole === 'child'">
        <v-btn v-if="reward.status === 'available'" class="request-reward-button" color="info" :disabled="!canRequest" prepend-icon="i-mdi:gift-open-outline" rounded="lg" size="small" variant="flat" @click="store.requestShopReward(reward.id)">{{ t('shop.rewards.request') }}</v-btn>
        <v-btn v-else-if="reward.status === 'requested' && reward.requesterId === store.activeChildId" color="warning" rounded="lg" size="small" variant="tonal" @click="store.cancelShopRewardRequest(reward.id)">{{ t('common.cancel') }}</v-btn>
        <v-chip v-else :color="reward.status === 'redeemed' ? 'success' : 'warning'" size="small" variant="tonal">{{ reward.status === 'redeemed' ? t('shop.rewards.redeemed') : t('shop.rewards.requested') }}</v-chip>
      </template>
      <template v-else><div v-if="reward.status === 'requested'" class="d-flex ga-1"><v-btn :aria-label="t('shop.rewards.reject')" icon="i-mdi:close" size="small" variant="tonal" @click="store.decideShopReward(reward.id, false)" /><v-btn :aria-label="t('shop.rewards.approve')" color="primary" :disabled="!redemptionOpen" icon="i-mdi:check" size="small" variant="flat" @click="store.decideShopReward(reward.id, true)" /></div><v-chip v-else :color="reward.status === 'redeemed' ? 'success' : 'primary'" size="small" variant="tonal">{{ reward.status === 'redeemed' ? t('shop.rewards.redeemed') : t('shop.rewards.available') }}</v-chip></template>
    </div>
  </BrandedCard>
</template>

<script lang="ts" setup>
import { useI18n } from 'vue-i18n';
import type { ShopReward } from '@/domain/shop';
import LadirchenAmount from '@/shared/components/LadirchenAmount.vue';
import { useFamilyWorldStore } from '@/stores/family-world';
import BrandedCard from '@/shared/components/ui/BrandedCard.vue';

defineProps<{ canRequest: boolean; publicationStatus: { icon: string; label: string; tone: string }; redemptionOpen: boolean; reward: ShopReward }>();
const emit = defineEmits<{ delete: [reward: ShopReward] }>();
const store = useFamilyWorldStore();
const { t } = useI18n();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.reward-card {
  @apply position-relative h-100 d-flex flex-column;
}
.reward-card--hidden {
  opacity: 0.68;
  filter: saturate(0.65);
}
.reward-card--scheduled {
  border-style: dashed;
}
.reward-admin-toolbar {
  min-height: 28px;
  margin: -5px -5px 8px;
  @apply d-flex align-center justify-space-between;
  gap: 6px;
}
.reward-publication-state {
  min-width: 0;
  padding: 4px 7px;
  @apply d-inline-flex align-center;
  gap: 4px;
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
  @apply d-flex;
  gap: 4px;
}
.reward-visibility-button,
.reward-delete-button {
  width: 28px;
  height: 28px;
  @apply d-grid place-center cursor-pointer;
  border: 1px solid var(--lad-border);
  border-radius: 10px;
  background: var(--lad-surface-raised);
}
.reward-delete-button {
  color: var(--lad-color-danger-muted);
}
.reward-icon {
  width: 78px;
  height: 6rem;
  @apply d-grid place-center;
  border-radius: 16px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  font-size: rem(42);
}
.reward-description {
  min-height: 47px;
}
.reward-actions {
  @apply mt-auto;
  padding-top: 16px;
  flex-wrap: wrap;
}
.price {
  min-height: 38px;
  padding: 5px 10px;
  @apply d-flex align-center;
  gap: 7px;
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
