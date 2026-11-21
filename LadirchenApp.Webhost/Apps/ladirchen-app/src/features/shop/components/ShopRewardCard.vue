<template>
  <BrandedCard
    class="reward-card pa-4 position-relative h-100 d-flex flex-column"
    :class="cardClasses"
    :data-ladi-heading="t('shop.rewards.rule')"
    :data-ladi-tip="reward.conditions"
    tone="shop"
  >
    <div
      v-if="store.permissions.canManageShop"
      class="reward-admin-toolbar d-flex align-center justify-space-between"
      data-ladi-ignore
    >
      <span
        class="reward-publication-state d-inline-flex align-center"
        :class="`reward-publication-state--${publicationStatus.tone}`"
      >
        <v-icon :icon="publicationStatus.icon" />
        {{ publicationStatus.label }}
      </span>
      <span class="reward-admin-actions d-flex flex-wrap justify-end">
        <v-btn
          v-if="reward.status !== 'requested'"
          :aria-label="t('shop.rewards.editAria', { title: reward.title })"
          color="info"
          prepend-icon="i-mdi:pencil-outline"
          rounded="lg"
          size="small"
          variant="tonal"
          @click="emit('edit', reward)"
        >
          {{ t("shop.rewards.edit") }}
        </v-btn>
        <v-btn
          :aria-label="visibilityActionLabel"
          :color="visibilityActionColor"
          :prepend-icon="visibilityActionIcon"
          rounded="lg"
          size="small"
          variant="tonal"
          @click="store.setShopRewardVisibility(reward.id, rewardIsHidden)"
        >
          {{ visibilityActionText }}
        </v-btn>
        <v-btn
          v-if="canAdministerShop"
          :aria-label="t('shop.rewards.deleteAria', { title: reward.title })"
          color="error"
          prepend-icon="i-mdi:trash-can-outline"
          rounded="lg"
          size="small"
          variant="tonal"
          @click="emit('delete', reward)"
        >
          {{ t("common.delete") }}
        </v-btn>
      </span>
    </div>
    <div class="reward-icon d-grid place-center">{{ reward.icon }}</div>
    <strong class="d-block mt-3">{{ reward.title }}</strong>
    <v-chip
      v-if="reward.durationMinutes"
      class="mt-2 align-self-start"
      color="primary"
      prepend-icon="i-mdi:timer-outline"
      size="small"
      variant="tonal"
    >
      {{ durationLabel }}
    </v-chip>
    <p class="reward-description text-caption text-medium-emphasis mt-1">{{ reward.description }}</p>
    <div class="reward-actions d-flex align-center justify-space-between ga-2 mt-auto" data-ladi-ignore>
      <span class="price d-flex align-center">
        <del v-if="reward.promotionActive && reward.regularPrice" class="regular-price">
          {{ reward.regularPrice }}
        </del>
        <LadirchenAmount :value="reward.price" />
      </span>
      <template v-if="store.viewerRole === 'child'">
        <v-btn
          v-if="reward.status === 'available'"
          class="request-reward-button"
          color="info"
          :disabled="!canRequest"
          prepend-icon="i-mdi:gift-open-outline"
          rounded="lg"
          size="small"
          variant="flat"
          @click="store.requestShopReward(reward.id)"
        >
          {{ t("shop.rewards.request") }}
        </v-btn>
        <v-btn
          v-else-if="canCancelRequest"
          color="warning"
          rounded="lg"
          size="small"
          variant="tonal"
          @click="store.cancelShopRewardRequest(reward.id)"
        >
          {{ t("common.cancel") }}
        </v-btn>
        <v-chip v-else :color="childStatusColor" size="small" variant="tonal">{{ childStatusLabel }}</v-chip>
      </template>
      <template v-else>
        <div v-if="reward.status === 'requested' && canAdministerShop" class="d-flex ga-1">
          <v-btn
            :aria-label="t('shop.rewards.reject')"
            color="error"
            :disabled="!store.permissions.canManageContent"
            prepend-icon="i-mdi:close"
            rounded="lg"
            size="small"
            variant="tonal"
            @click="store.decideShopReward(reward.id, false)"
          >
            {{ t("shop.rewards.reject") }}
          </v-btn>
          <v-btn
            :aria-label="t('shop.rewards.approve')"
            color="primary"
            :disabled="!reward.approvableByMemberIds?.includes(store.signedInMemberId)"
            prepend-icon="i-mdi:check"
            rounded="lg"
            size="small"
            variant="flat"
            @click="store.decideShopReward(reward.id, true)"
          >
            {{ t("shop.rewards.approve") }}
          </v-btn>
        </div>
        <div v-else-if="rewardIsRedeemed" class="d-flex flex-wrap justify-end ga-1">
          <v-btn
            prepend-icon="i-mdi:pencil-outline"
            rounded="lg"
            size="small"
            variant="text"
            @click="emit('edit', reward)"
          >
            {{ t("shop.rewards.edit") }}
          </v-btn>
          <v-btn
            color="primary"
            prepend-icon="i-mdi:restore"
            rounded="lg"
            size="small"
            variant="tonal"
            @click="store.republishShopReward(reward.id)"
          >
            {{ t("shop.rewards.reactivate") }}
          </v-btn>
        </div>
        <v-chip v-else :color="guardianStatusColor" size="small" variant="tonal">{{ guardianStatusLabel }}</v-chip>
      </template>
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
export interface Emits {
  delete: [reward: ShopReward];
  edit: [reward: ShopReward];
}
export interface Props {
  readonly canRequest: boolean;
  readonly publicationStatus: ShopRewardPublicationStatus;
  readonly reward: ShopReward;
}
const props = defineProps<Props>();
const emit = defineEmits<Emits>();
const store = useFamilyWorldStore();
const { t } = useI18n();
const rewardIsHidden = computed(() => props.reward.isVisible === false);
const rewardIsRedeemed = computed(() => props.reward.status === "redeemed");
const canAdministerShop = computed(() => store.signedInMember?.guardianAccess === "admin");
const cardClasses = computed(() => ({
  "reward-card--hidden": rewardIsHidden.value,
  "reward-card--scheduled": props.publicationStatus.tone === "scheduled",
  "reward-card--promotion": props.reward.promotionActive === true,
}));
const canCancelRequest = computed(() => props.reward.cancellableByMemberIds?.includes(store.signedInMemberId) ?? false);
const visibilityActionLabel = computed(() =>
  t(rewardIsHidden.value ? "shop.rewards.show" : "shop.rewards.hide", { title: props.reward.title }),
);
const visibilityActionColor = computed(() => (rewardIsHidden.value ? "primary" : undefined));
const visibilityActionIcon = computed(() => (rewardIsHidden.value ? "i-mdi:eye-outline" : "i-mdi:eye-off-outline"));
const visibilityActionText = computed(() =>
  t(rewardIsHidden.value ? "shop.rewards.showAction" : "shop.rewards.hideAction"),
);
const childStatusColor = computed(() => (rewardIsRedeemed.value ? "success" : "warning"));
const childStatusLabel = computed(() => t(rewardIsRedeemed.value ? "shop.rewards.redeemed" : "shop.rewards.requested"));
const guardianStatusColor = computed(() => (rewardIsRedeemed.value ? "success" : "primary"));
const guardianStatusLabel = computed(() =>
  t(rewardIsRedeemed.value ? "shop.rewards.redeemed" : "shop.rewards.available"),
);
const durationLabel = computed(() => {
  const minutes = props.reward.durationMinutes ?? 0;
  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;

  if (remainingMinutes === 0) {
    return t("shop.timer.durationHours", { count: hours });
  }

  if (hours === 0) {
    return t("shop.timer.durationMinutes", { count: remainingMinutes });
  }

  return t("shop.timer.durationHoursMinutes", { hours, minutes: remainingMinutes });
});
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
.reward-icon {
  width: rem(78);
  height: 6rem;
  border-radius: rem(16);
  background: linear-gradient(145deg, var(--lad-surface-soft), var(--lad-color-reward-soft));
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
    linear-gradient(145deg, var(--lad-color-info-subtle), var(--lad-color-info)),
    var(--lad-color-info-strong),
    var(--lad-text-inverse),
    2.5rem,
    rem(15),
    0.25rem rem(13)
  );
}
</style>
