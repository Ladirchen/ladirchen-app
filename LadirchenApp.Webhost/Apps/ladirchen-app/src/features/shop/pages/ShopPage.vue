<template>
  <div class="page page-padding shop-page">
    <template v-if="store.viewerRole === 'child' || store.permissions.canManageContent">
      <PageViewSwitch v-model="activeTab" class="shop-mode-switch mb-5" :label="t('shop.switchLabel')" :options="shopViewOptions" tone="amber" />

      <template v-if="activeTab === 'family'">
        <div class="d-flex align-end justify-space-between mb-3">
          <div><h2 class="section-title">{{ t('shop.rewards.title') }}</h2></div>
          <v-btn v-if="store.permissions.canManageContent" color="primary" prepend-icon="i-mdi:plus" rounded="lg" size="small" variant="tonal" @click="rewardDialog = true">{{ t('common.add') }}</v-btn>
        </div>

        <div class="reward-grid">
          <ShopRewardCard
            v-for="reward in displayedShopRewards"
            :key="reward.id"
            :can-request="canRequest(reward)"
            :publication-status="rewardPublicationStatus(reward)"
            :redemption-open="redemptionOpen"
            :reward="reward"
            @delete="rewardToDelete = $event"
          />
        </div>

      </template>

      <template v-else>
        <PageViewSwitch v-model="houseArea" compact :label="t('shop.catalog.switchLabel')" class="shop-area-switch mb-4" :options="houseAreaOptions" tone="amber" />

        <section v-if="houseArea === 'outside'" class="catalog-section mb-5">
          <div class="catalog-heading mb-3">
            <div><p class="eyebrow mb-1">{{ t('shop.catalog.editions.eyebrow') }}</p><h2 class="section-title">{{ t('shop.catalog.editions.title') }}</h2></div>
            <span>{{ t('shop.catalog.editions.saved') }}</span>
          </div>
          <div class="edition-grid">
            <BrandedCard
              v-for="edition in houseEditions"
              :key="edition.id"
              class="edition-card pa-3"
              :class="{ active: edition.id === store.houseThemeId }"
              interactive
              tone="shop"
            >
              <div
                class="edition-preview"
                :class="`edition-${edition.id}`"
                :style="{ backgroundImage: `url(${editionPreviewBackground(edition.id)})` }"
              >
                <img class="edition-house-sprite" :src="editionPreviewHouse(edition.id)" alt="" draggable="false">
                <v-chip v-if="edition.id === store.houseThemeId" class="edition-status" color="success" size="x-small">{{ t('shop.catalog.active') }}</v-chip>
              </div>
              <strong class="d-block mt-2">{{ edition.name }}</strong>
              <p class="catalog-description text-caption text-medium-emphasis mt-1">{{ edition.description }}</p>
              <div class="catalog-actions mt-3">
                <span v-if="edition.id !== store.houseThemeId" class="price"><LadirchenAmount :value="edition.price" /></span>
                <v-btn
                  v-if="!ownsEdition(edition.id) && store.viewerRole === 'child'"
                  class="catalog-buy-button catalog-buy-button--compact"
                  color="info"
                  :disabled="edition.price > store.availableBalance"
                  rounded="lg"
                  size="small"
                  variant="flat"
                  @click="store.purchaseHouseTheme(edition.id)"
                >{{ t('shop.catalog.buy') }}</v-btn>
                <v-chip v-else-if="!ownsEdition(edition.id)" color="info" size="x-small" variant="tonal">{{ t('shop.catalog.childPurchase') }}</v-chip>
                <v-btn v-else-if="edition.id !== store.houseThemeId" color="primary" rounded="lg" size="small" variant="tonal" @click="store.selectHouseTheme(edition.id)">{{ t('shop.catalog.use') }}</v-btn>
                <span v-else class="edition-selected"><v-icon icon="i-mdi:check-decagram" size="16" />{{ t('shop.catalog.selected') }}<i aria-hidden="true">✦</i></span>
              </div>
            </BrandedCard>
          </div>
        </section>

        <section v-if="houseArea !== 'special'" class="catalog-section mb-5">
          <div class="catalog-heading mb-3">
            <div><p class="eyebrow mb-1">{{ t('shop.catalog.sets.eyebrow') }}</p><h2 class="section-title">{{ t('shop.catalog.sets.title') }}</h2></div>
            <span>{{ houseArea === 'inside' ? t('shop.catalog.sets.inside') : t('shop.catalog.sets.outside') }}</span>
          </div>
          <div class="set-list">
            <BrandedCard v-for="set in visibleFurnitureSets" :key="set.id" class="set-card pa-3" interactive tone="shop">
              <div class="set-preview" aria-hidden="true">
                <RoomFurniture v-for="accessory in setPreviewAccessories(set.accessoryIds)" :key="accessory.id" :item="accessory" />
              </div>
              <div class="set-copy min-w-0">
                <strong>{{ set.name }}</strong>
                <p class="text-caption text-medium-emphasis">{{ set.description }}</p>
                <small>{{ t('shop.catalog.sets.meta', { count: set.accessoryIds.length, level: set.minimumHouseLevel + 1 }) }}</small>
              </div>
              <div class="set-action">
                <span v-if="ownsSet(set.id)" class="catalog-status catalog-status--owned"><i>✦</i><v-icon icon="i-mdi:check-decagram" size="16" />{{ t('shop.catalog.owned') }}<i>★</i></span>
                <span v-else-if="set.minimumHouseLevel > store.houseLevel" class="catalog-status catalog-status--locked"><i>✦</i><v-icon icon="i-mdi:lock-star" size="16" />{{ t('shop.catalog.locked') }}<i>✧</i></span>
                <v-btn
                  v-else-if="store.viewerRole === 'child'"
                  class="catalog-buy-button"
                  color="info"
                  :disabled="set.price > store.availableBalance"
                  rounded="lg"
                  size="small"
                  variant="flat"
                  @click="store.purchaseFurnitureSet(set.id)"
                ><span class="catalog-buy-price"><LadirchenAmount suffix=" L" :value="set.price" /></span><span>{{ t('shop.catalog.buy') }}</span></v-btn>
                <v-chip v-else color="info" size="x-small" variant="tonal">{{ t('shop.catalog.childPurchase') }}</v-chip>
              </div>
            </BrandedCard>
          </div>
        </section>

        <div class="catalog-heading mb-3">
          <div>
            <p class="eyebrow mb-1">{{ houseArea === 'special' ? t('shop.catalog.items.specialEyebrow') : t('shop.catalog.items.eyebrow') }}</p>
            <h2 class="section-title">{{ houseArea === 'special' ? t('shop.catalog.items.specialTitle') : t('shop.catalog.items.title') }}</h2>
          </div>
          <span v-if="houseArea === 'special'">{{ t('shop.catalog.items.motionHint') }}</span>
        </div>

        <div class="accessory-grid">
          <BrandedCard v-for="accessory in visibleAccessories" :key="accessory.id" class="accessory-card pa-4" :class="{ owned: accessory.owned, special: accessory.category === 'special' }" interactive tone="shop">
            <div class="accessory-preview">
              <RoomFurniture class="catalog-item-preview" :item="accessory" />
              <v-chip v-if="accessory.motion && accessory.motion !== 'none'" class="motion-chip" color="warning" prepend-icon="i-mdi:creation-outline" size="x-small">{{ t('shop.catalog.animated') }}</v-chip>
            </div>
            <strong class="d-block mt-3">{{ accessory.title }}</strong>
            <p class="accessory-description text-caption text-medium-emphasis mt-1">{{ accessory.description }}</p>
            <div v-if="accessory.owned" class="owned-status-row">
              <span class="catalog-status catalog-status--owned"><i>✦</i><v-icon icon="i-mdi:check-decagram" size="16" />{{ t('shop.catalog.owned') }}<i>★</i></span>
            </div>
            <div v-if="!accessory.owned" class="catalog-actions accessory-actions ga-2">
              <span class="price"><LadirchenAmount :value="accessory.price" /></span>
              <v-btn v-if="store.viewerRole === 'child'" class="catalog-buy-button catalog-buy-button--compact" color="info" :disabled="accessory.price > store.availableBalance" rounded="lg" size="small" variant="flat" @click="store.purchaseAccessory(accessory.id)">{{ t('shop.catalog.buy') }}</v-btn>
              <v-chip v-else color="info" size="x-small" variant="tonal">{{ t('shop.catalog.childPurchase') }}</v-chip>
            </div>
          </BrandedCard>
        </div>
      </template>

      <v-dialog v-model="rewardDialog" max-width="430">
        <v-card class="pa-5" rounded="xl">
          <v-card-title class="pa-0 mb-1">{{ t('shop.form.title') }}</v-card-title>
          <v-card-subtitle class="pa-0 mb-4">{{ t('shop.form.description') }}</v-card-subtitle>
          <label class="field-label">{{ t('shop.form.selectIcon') }}</label>
          <div class="icon-picker mt-2 mb-4">
            <button v-for="icon in iconOptions" :key="icon" :class="{ active: newReward.icon === icon }" type="button" @click="newReward.icon = icon">{{ icon }}</button>
          </div>
          <v-text-field v-model="newReward.icon" :label="t('shop.form.customIcon')" maxlength="4" variant="outlined" />
          <v-text-field v-model="newReward.title" :label="t('common.title')" variant="outlined" />
          <v-textarea v-model="newReward.description" :label="t('common.description')" rows="2" variant="outlined" />
          <v-select v-model="newReward.category" :items="categoryOptions" item-title="title" item-value="value" :label="t('shop.form.category')" variant="outlined" />
          <div class="d-grid reward-form ga-3">
            <v-text-field v-model.number="newReward.price" :label="t('shop.form.price')" min="1" suffix="L" type="number" variant="outlined" />
            <v-text-field v-model.number="newReward.quantity" :label="t('shop.form.quantity')" min="1" type="number" variant="outlined" />
          </div>
          <v-textarea v-model="newReward.conditions" :label="t('shop.form.conditions')" rows="2" variant="outlined" />
          <v-select v-model="newReward.publicationMode" :items="publicationModeOptions" item-title="title" item-value="value" :label="t('shop.form.publication')" variant="outlined" />
          <v-text-field
            v-if="newReward.publicationMode === 'scheduled'"
            v-model="newReward.availableFrom"
            :min="minimumAvailableDate"
            :label="t('shop.form.visibleFrom')"
            type="date"
            variant="outlined"
          />
          <v-switch v-model="newReward.unlimited" color="primary" inset :label="t('shop.form.noExpiry')" />
          <v-text-field v-if="!newReward.unlimited" v-model="newReward.availableUntil" :min="minimumAvailableDate" :label="t('shop.form.availableUntil')" type="date" variant="outlined" />
          <div class="d-flex justify-end ga-2">
            <v-btn rounded="lg" variant="text" @click="rewardDialog = false">{{ t('common.cancel') }}</v-btn>
            <v-btn color="primary" :disabled="!canAddReward" rounded="lg" variant="flat" @click="addReward">{{ t('shop.form.save') }}</v-btn>
          </div>
        </v-card>
      </v-dialog>

      <v-dialog :model-value="rewardToDelete !== null" max-width="390" @update:model-value="value => { if (!value) rewardToDelete = null; }">
        <v-card class="pa-5" rounded="xl">
          <v-card-title class="pa-0">{{ t('shop.delete.title') }}</v-card-title>
          <v-card-text class="px-0 pb-2">{{ t('shop.delete.description', { title: rewardToDelete?.title }) }}</v-card-text>
          <v-card-actions class="px-0 pb-0 justify-end">
            <v-btn rounded="lg" variant="text" @click="rewardToDelete = null">{{ t('common.cancel') }}</v-btn>
            <v-btn color="error" rounded="lg" variant="flat" @click="confirmRewardDeletion">{{ t('common.delete') }}</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </template>
    <v-alert v-else color="primary" icon="i-mdi:lock-outline" variant="tonal">{{ t('shop.protected') }}</v-alert>
  </div>
</template>

<script lang="ts" setup>
import { useI18n } from 'vue-i18n';

import LadirchenAmount from '@/shared/components/LadirchenAmount.vue';
import PageViewSwitch from '@/shared/components/ui/PageViewSwitch.vue';
import RoomFurniture from '@/shared/components/house/RoomFurniture.vue';
import ShopRewardCard from '../components/ShopRewardCard.vue';
import BrandedCard from '@/shared/components/ui/BrandedCard.vue';
import { useShopPage } from '../composables/use-shop-page';

const { t } = useI18n();
const {
  activeTab, addReward, canAddReward, canRequest, categoryOptions, confirmRewardDeletion,
  displayedShopRewards, editionPreviewBackground, editionPreviewHouse, houseArea, houseAreaOptions,
  houseEditions, iconOptions, minimumAvailableDate, newReward, ownsEdition, ownsSet,
  publicationModeOptions, redemptionOpen, rewardDialog, rewardPublicationStatus, rewardToDelete,
  setPreviewAccessories, shopViewOptions, store, visibleAccessories, visibleFurnitureSets,
} = useShopPage();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.shop-mode-switch {
  @apply position-relative;
  isolation: isolate;
  gap: 10px;
  padding: 9px;
  @apply overflow-hidden;
  border: 2px solid var(--lad-color-reward-muted);
  border-radius: 28px;
  background:
    radial-gradient(
      circle at 10% 18%,
      color-mix(in srgb, var(--lad-surface-raised) 95%, transparent) 0 4px,
      transparent 5px
    ),
    radial-gradient(
      circle at 89% 78%,
      color-mix(in srgb, var(--lad-color-reward-border) 18%, transparent) 0 34px,
      transparent 35px
    ),
    linear-gradient(135deg, var(--lad-surface), var(--lad-color-reward-soft));
  box-shadow:
    0 6px 0 var(--lad-color-reward-soft),
    0 12px 24px color-mix(in srgb, var(--lad-color-reward-ink) 8%, transparent);
}
.shop-mode-switch::after {
  content: "";
  @apply position-absolute;
  right: 11%;
  bottom: -21px;
  left: 11%;
  height: 30px;
  z-index: -1;
  border-radius: 50%;
  background: color-mix(
    in srgb,
    var(--lad-color-reward-accent) 10%,
    transparent
  );
  filter: blur(1px);
}
.shop-mode-switch :deep(button) {
  min-height: 82px;
  padding: 12px 14px;
  @apply overflow-hidden;
  border: 1px solid
    color-mix(in srgb, var(--lad-color-accent-warm-muted) 12%, transparent);
  border-radius: 21px;
  background: color-mix(in srgb, var(--lad-surface-raised) 35%, transparent);
}
.shop-mode-switch :deep(button::before) {
  content: "";
  width: 68px;
  height: 68px;
  @apply position-absolute;
  top: -34px;
  right: -25px;
  border-radius: 50%;
  background: color-mix(in srgb, var(--switch-accent) 10%, transparent);
  transition: transform 180ms ease;
}
.shop-mode-switch :deep(button:nth-child(2)::before) {
  background: color-mix(in srgb, var(--lad-color-primary) 10%, transparent);
}
.shop-mode-switch :deep(button:hover::before),
.shop-mode-switch :deep(button.active::before) {
  transform: scale(1.28);
}
.shop-mode-switch :deep(button.active) {
  transform: translateY(-2px);
  border-color: color-mix(
    in srgb,
    var(--lad-color-reward-accent) 30%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-surface-raised),
    var(--lad-surface)
  );
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--lad-color-reward-accent) 18%, transparent),
    0 12px 18px color-mix(in srgb, var(--lad-color-reward-ink) 10%, transparent);
}
.shop-mode-switch :deep(button:nth-child(2).active) {
  border-color: color-mix(in srgb, var(--lad-color-primary) 35%, transparent);
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--lad-color-primary) 18%, transparent),
    0 12px 18px
      color-mix(in srgb, var(--lad-color-primary-deep) 8%, transparent);
}
.shop-mode-switch :deep(.page-view-icon) {
  width: 48px;
  height: 48px;
  border-radius: 17px;
  transform: rotate(-4deg);
  box-shadow: inset 0 0 0 2px
    color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
}
.shop-mode-switch :deep(button:nth-child(2) .page-view-icon) {
  transform: rotate(4deg);
}
.shop-mode-switch :deep(button.active .page-view-icon) {
  transform: rotate(-7deg) scale(1.07);
}
.shop-mode-switch :deep(button:nth-child(2).active .page-view-icon) {
  transform: rotate(7deg) scale(1.07);
}
.shop-mode-switch :deep(.page-view-copy strong) {
  font-size: rem(13);
}
.shop-mode-switch :deep(.page-view-copy small) {
  margin-top: 4px;
  font-size: rem(9);
}
.shop-mode-switch :deep(.page-view-check) {
  width: 23px;
  height: 23px;
  top: 9px;
  right: 9px;
  border: 3px solid var(--lad-surface-raised);
  border-radius: 8px;
  transform: rotate(12deg) scale(0.65);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-reward-strong) 15%, transparent);
}
.shop-mode-switch :deep(button.active .page-view-check) {
  transform: rotate(12deg) scale(1);
}

.shop-area-switch {
  @apply position-relative;
  isolation: isolate;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;
  padding: 5px 3px 13px;
  @apply overflow-visible;
  border: 0;
  border-radius: 0;
  background: transparent;
  box-shadow: none;
}
.shop-area-switch::before {
  content: "";
  @apply position-absolute;
  right: 3px;
  bottom: 2px;
  left: 3px;
  height: 20px;
  z-index: -1;
  border: 2px solid var(--lad-color-reward-muted);
  border-radius: 50%;
  background: linear-gradient(var(--lad-surface), var(--lad-color-reward-soft));
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-color-accent-warm-soft) 20%, transparent);
}
.shop-area-switch :deep(button) {
  min-height: 92px;
  padding: 9px 5px 12px;
  @apply flex-column;
  @apply justify-center;
  gap: 5px;
  text-align: center;
  border: 2px solid var(--lad-color-reward-muted);
  border-radius: 24px 24px 17px 17px;
  background: linear-gradient(
    155deg,
    var(--lad-surface),
    var(--lad-color-reward-soft)
  );
  box-shadow: 0 5px 0 var(--lad-color-reward-muted);
}
.shop-area-switch :deep(button:nth-child(2)) {
  border-color: var(--lad-color-primary-soft);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow: 0 5px 0 var(--lad-color-primary-soft);
}
.shop-area-switch :deep(button:nth-child(3)) {
  border-color: var(--lad-color-info-soft);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow: 0 5px 0 var(--lad-color-bonus-soft);
}
.shop-area-switch :deep(button:hover) {
  transform: translateY(-3px);
}
.shop-area-switch :deep(button.active) {
  transform: translateY(-6px) rotate(-1deg);
  border-color: var(--lad-color-reward-border);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-color-reward-pale)
  );
  box-shadow:
    0 8px 0 var(--lad-color-accent-warm-soft),
    0 13px 18px color-mix(in srgb, var(--lad-color-reward-ink) 12%, transparent);
}
.shop-area-switch :deep(button:nth-child(2).active) {
  transform: translateY(-6px) rotate(1deg);
  border-color: var(--lad-color-primary);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow:
    0 8px 0 var(--lad-border),
    0 13px 18px
      color-mix(in srgb, var(--lad-color-primary-deep) 10%, transparent);
}
.shop-area-switch :deep(button:nth-child(3).active) {
  transform: translateY(-6px) rotate(-1deg);
  border-color: var(--lad-color-bonus-highlight);
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-surface-soft)
  );
  box-shadow:
    0 8px 0 var(--lad-color-bonus-soft),
    0 13px 18px
      color-mix(in srgb, var(--lad-color-bonus-strong) 10%, transparent);
}
.shop-area-switch :deep(.page-view-icon) {
  width: 43px;
  height: 43px;
  border: 3px solid
    color-mix(in srgb, var(--lad-surface-raised) 90%, transparent);
  border-radius: 50%;
  color: var(--lad-color-reward-shadow);
  background: var(--lad-color-reward-pale);
  box-shadow: 0 4px 0 var(--lad-color-reward-muted);
}
.shop-area-switch :deep(button:nth-child(2) .page-view-icon) {
  color: var(--lad-color-primary-strong);
  background: var(--lad-color-primary-soft);
  box-shadow: 0 4px 0 var(--lad-border);
}
.shop-area-switch :deep(button:nth-child(3) .page-view-icon) {
  color: var(--lad-color-bonus-muted);
  background: var(--lad-surface-soft);
  box-shadow: 0 4px 0 var(--lad-color-bonus-soft);
}
.shop-area-switch :deep(button.active .page-view-icon) {
  color: var(--lad-surface-raised);
  background: linear-gradient(
    145deg,
    var(--lad-color-reward),
    var(--lad-color-reward-accent)
  );
  box-shadow: 0 4px 0 var(--lad-color-reward-deep);
  transform: rotate(-6deg) scale(1.06);
}
.shop-area-switch :deep(button:nth-child(2).active .page-view-icon) {
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-muted)
  );
  box-shadow: 0 4px 0 var(--lad-color-primary-deep);
  transform: rotate(6deg) scale(1.06);
}
.shop-area-switch :deep(button:nth-child(3).active .page-view-icon) {
  background: linear-gradient(
    145deg,
    var(--lad-color-bonus-highlight),
    var(--lad-color-bonus)
  );
  box-shadow: 0 4px 0 var(--lad-color-bonus-muted);
  transform: rotate(-6deg) scale(1.06);
}
.shop-area-switch :deep(.page-view-copy strong) {
  font-size: rem(11);
}
.shop-area-switch :deep(.page-view-copy small) {
  margin-top: 2px;
  font-size: rem(7);
  line-height: 1.15;
}
.shop-area-switch :deep(.page-view-check) {
  width: 19px;
  height: 19px;
  top: 6px;
  right: 6px;
  border: 2px solid var(--lad-surface-raised);
  background: var(--lad-color-reward-accent);
}
.shop-area-switch :deep(button:nth-child(2) .page-view-check) {
  background: var(--lad-color-primary);
}
.shop-area-switch :deep(button:nth-child(3) .page-view-check) {
  background: var(--lad-color-bonus);
}

.reward-grid,
.accessory-grid {
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 11px;
}
.catalog-heading {
  @apply d-flex align-end justify-space-between;
  gap: 10px;
}
.catalog-heading > span {
  color: var(--lad-muted);
  font-size: rem(9);
  text-align: right;
}
.edition-grid {
  @apply d-grid align-stretch;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}
.edition-card {
  min-width: 0;
  height: 100%;
  @apply d-flex flex-column;
}
.edition-card.active {
  border-color: color-mix(in srgb, var(--lad-color-primary) 60%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-color-primary) 25%, transparent);
}
.edition-preview {
  height: 8.5rem;
  flex: 0 0 8.5rem;
  @apply position-relative overflow-hidden;
  border-radius: rem(14);
  background-position: center;
  background-size: cover;
  box-shadow: inset 0 0 0 rem(2)
    color-mix(in srgb, var(--lad-surface-raised) 70%, transparent);
}
.edition-house-sprite {
  width: 86%;
  height: 90%;
  @apply position-absolute;
  bottom: -2%;
  left: 50%;
  transform: translateX(-50%);
  object-fit: contain;
  filter: drop-shadow(
    0 0.3rem 0.2rem
      color-mix(in srgb, var(--lad-text-warm-strong) 20%, transparent)
  );
}
.edition-status {
  @apply position-absolute;
  top: 5px;
  left: 5px;
  z-index: 5;
}
.catalog-description {
  min-height: 49px;
  flex: 1;
}
.catalog-actions {
  @apply mt-auto;
  padding-top: 12px;
  @apply d-flex align-center justify-space-between;
  gap: 6px;
}
.catalog-actions :deep(.v-btn),
.catalog-actions :deep(.v-chip) {
  min-height: 38px;
}
.edition-selected {
  width: 100%;
  min-height: 38px;
  padding: 6px 10px;
  @apply position-relative d-flex align-center justify-center overflow-hidden;
  gap: 6px;
  color: var(--lad-surface-raised);
  border: 2px solid var(--lad-surface-raised);
  border-radius: 14px;
  background: linear-gradient(
    145deg,
    var(--lad-color-primary-highlight),
    var(--lad-color-primary-muted)
  );
  box-shadow:
    0 4px 0 var(--lad-color-primary-deep),
    0 8px 14px
      color-mix(in srgb, var(--lad-color-primary-deep) 15%, transparent);
  font-size: rem(10);
  font-weight: var(--lad-font-weight-black);
  animation: edition-selected-breathe 2.5s ease-in-out infinite;
}
.edition-selected :deep(.v-icon) {
  font-size: rem(17);
  animation: edition-check-pop 2.5s ease-in-out infinite;
}
.edition-selected i {
  @apply position-absolute;
  top: 3px;
  right: 6px;
  color: var(--lad-color-reward-pale);
  font-size: 0.5rem;
  font-style: normal;
  animation: edition-selected-spark 1.7s ease-in-out infinite;
}
.set-list {
  @apply d-flex flex-column;
  gap: 12px;
}
.set-card {
  @apply d-grid align-center position-relative overflow-hidden;
  grid-template-columns: 76px minmax(0, 1fr) auto;
  gap: 13px;
}
.set-preview {
  width: 76px;
  height: 66px;
  @apply position-relative;
  border: 2px solid
    color-mix(in srgb, var(--lad-surface-raised) 90%, transparent);
  border-radius: 20px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-color-reward-border) 14%, transparent);
  transform: rotate(-2deg);
}
.set-preview :deep(.room-furniture) {
  width: 48px;
  height: 48px;
  @apply position-absolute;
  top: 8px;
}
.set-preview :deep(.room-furniture:nth-child(1)) {
  left: -2px;
  transform: rotate(-7deg);
}
.set-preview :deep(.room-furniture:nth-child(2)) {
  left: 16px;
  z-index: 2;
}
.set-preview :deep(.room-furniture:nth-child(3)) {
  right: -2px;
  transform: rotate(7deg);
}
.set-copy strong {
  font-size: rem(15);
}
.set-copy p {
  margin-top: 3px;
  line-height: 1.35;
}
.set-copy small {
  color: var(--lad-color-reward-strong);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
}
.set-action {
  @apply d-flex justify-end;
}
.set-action :deep(.v-btn__content) {
  gap: 6px;
}
.price {
  min-height: 38px;
  padding: 5px 10px 5px 6px;
  @apply d-flex align-center font-weight-black;
  gap: 7px;
  color: var(--lad-color-reward-strong);
  border: 2px solid
    color-mix(in srgb, var(--lad-color-reward-border) 25%, transparent);
  border-radius: var(--lad-radius-pill);
  background: linear-gradient(
    145deg,
    var(--lad-color-reward-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-color-reward-shadow) 15%, transparent);
  font-size: rem(13);
}
.catalog-buy-button {
  @include action-button;
}
.catalog-buy-price {
  padding: 4px 8px 4px 4px;
  @apply d-inline-flex align-center;
  gap: 4px;
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-surface-raised) 20%, transparent);
  font-weight: var(--lad-font-weight-black);
}
.catalog-buy-button--compact {
  min-height: 38px;
  padding-inline: 13px;
}
.accessory-card {
  @apply h-100 d-flex flex-column;
}
.accessory-preview {
  height: 6rem;
  padding: 0.5rem;
  @apply position-relative d-grid place-center overflow-hidden;
  border-radius: 16px;
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  font-size: rem(42);
}
.accessory-card.special {
  border-color: color-mix(
    in srgb,
    var(--lad-color-reward-border) 44%,
    transparent
  );
}
.catalog-item-preview {
  width: 4.75rem;
  height: 4.75rem;
  max-width: 100%;
  max-height: 100%;
}
.motion-chip {
  @apply position-absolute;
  top: 7px;
  left: 7px;
  font-size: 0.5rem;
}
.owned-status-row {
  @apply mt-auto;
  padding-top: 14px;
  @apply d-flex justify-center;
}
.catalog-status {
  @apply position-relative;
  @include status-pill(
    var(--lad-text-strong),
    currentColor,
    var(--lad-surface),
    transparent
  );
}
.catalog-status--owned {
  color: var(--lad-color-primary-deep);
  border-color: color-mix(
    in srgb,
    var(--lad-color-primary-muted) 25%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-primary-soft)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-color-primary-strong) 15%, transparent),
    0 8px 15px
      color-mix(in srgb, var(--lad-color-primary-supporting) 8%, transparent);
}
.catalog-status--locked {
  color: var(--lad-color-reward-strong);
  border-color: color-mix(
    in srgb,
    var(--lad-color-reward-border) 30%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-color-reward-soft),
    var(--lad-color-reward-pale)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-color-reward-shadow) 15%, transparent),
    0 8px 14px color-mix(in srgb, var(--lad-color-reward-ink) 8%, transparent);
}
.catalog-status i {
  color: var(--lad-color-reward-accent);
  font-style: normal;
  text-shadow: 0 0 6px
    color-mix(in srgb, var(--lad-color-reward) 80%, transparent);
  animation: owned-star 1.9s ease-in-out infinite;
}
.catalog-status i:last-child {
  animation-delay: -0.9s;
}
.catalog-status--locked :deep(.v-icon) {
  color: var(--lad-color-reward-accent);
  filter: drop-shadow(
    0 2px 0 color-mix(in srgb, var(--lad-surface-raised) 80%, transparent)
  );
}
.accessory-description {
  min-height: 47px;
}
.accessory-card.owned {
  border-color: color-mix(
    in srgb,
    var(--lad-color-reward-border) 50%,
    transparent
  );
}
.reward-form {
  grid-template-columns: 0.7fr 1.3fr;
}
.field-label {
  @include field-label;
}
.icon-picker {
  @apply d-grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 7px;
}
.icon-picker button {
  aspect-ratio: 1;
  border: 1px solid var(--lad-border);
  border-radius: 12px;
  background: var(--lad-surface);
  @apply cursor-pointer;
  font-size: rem(23);
}
.icon-picker button.active {
  border: 2px solid var(--lad-mint);
  background: var(--lad-surface-soft);
}
@include respond-down(catalog) {
  .edition-grid {
    grid-template-columns: 1fr;
  }
  .catalog-description {
    min-height: 0;
  }
  .edition-card {
    @apply d-grid;
    grid-template-columns: 112px minmax(0, 1fr);
    column-gap: 10px;
  }
  .edition-preview {
    grid-row: span 3;
  }
  .edition-card > strong {
    margin-top: 2px;
  }
  .edition-card .catalog-actions {
    align-self: end;
  }
}
@include respond-down(compact) {
  .reward-grid,
  .accessory-grid {
    grid-template-columns: 1fr;
  }
  .accessory-description {
    min-height: 0;
  }
  .set-card {
    grid-template-columns: 64px minmax(0, 1fr);
  }
  .set-preview {
    width: 64px;
    height: 58px;
  }
  .set-preview :deep(.room-furniture) {
    width: 42px;
    height: 42px;
  }
  .set-preview :deep(.room-furniture:nth-child(2)) {
    left: 12px;
  }
  .set-action {
    grid-column: 2;
    @apply justify-start;
  }
}
@keyframes owned-star {
  0%,
  100% {
    opacity: 0.45;
    transform: scale(0.72) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.18) rotate(22deg);
  }
}
@keyframes edition-selected-breathe {
  0%,
  100% {
    box-shadow:
      0 4px 0 var(--lad-color-primary-deep),
      0 8px 14px
        color-mix(in srgb, var(--lad-color-primary-deep) 15%, transparent);
  }
  50% {
    box-shadow:
      0 4px 0 var(--lad-color-primary-deep),
      0 8px 18px
        color-mix(in srgb, var(--lad-color-primary-deep) 30%, transparent),
      0 0 0 4px
        color-mix(in srgb, var(--lad-color-primary-highlight) 10%, transparent);
  }
}
@keyframes edition-check-pop {
  0%,
  68%,
  100% {
    transform: scale(1) rotate(0);
  }
  78% {
    transform: scale(1.22) rotate(-9deg);
  }
  88% {
    transform: scale(1.05) rotate(5deg);
  }
}
@keyframes edition-selected-spark {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.7) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(24deg);
  }
}
@include reduced-motion {
  .catalog-status i,
  .edition-selected,
  .edition-selected :deep(.v-icon),
  .edition-selected i {
    animation: none;
  }
}
</style>
