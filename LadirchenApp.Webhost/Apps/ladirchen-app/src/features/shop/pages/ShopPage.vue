<template>
  <div class="page page-padding shop-page">
    <template v-if="store.viewerRole === 'child' || store.permissions.canManageContent">
      <PageViewSwitch v-model="activeTab" class="shop-mode-switch mb-5" :label="t('shop.switchLabel')" :options="shopViewOptions" tone="amber" />

      <template v-if="activeTab === 'family'">
        <div class="d-flex align-end justify-space-between mb-3">
          <div><h2 class="section-title">{{ t('shop.rewards.title') }}</h2></div>
          <v-btn v-if="store.permissions.canManageContent" color="primary" prepend-icon="mdi-plus" rounded="lg" size="small" variant="tonal" @click="rewardDialog = true">{{ t('common.add') }}</v-btn>
        </div>

        <div class="reward-grid">
          <v-card
            v-for="reward in displayedShopRewards"
            :key="reward.id"
            class="reward-card pa-4"
            :class="{ 'reward-card--hidden': reward.isVisible === false, 'reward-card--scheduled': rewardIsScheduled(reward) }"
            :data-ladi-heading="t('shop.rewards.rule')"
            :data-ladi-tip="reward.conditions"
            elevation="0"
            rounded="xl"
          >
            <div v-if="store.viewerRole === 'guardian'" class="reward-admin-toolbar" data-ladi-ignore>
              <span class="reward-publication-state" :class="`reward-publication-state--${rewardPublicationStatus(reward).tone}`">
                <v-icon :icon="rewardPublicationStatus(reward).icon" />{{ rewardPublicationStatus(reward).label }}
              </span>
              <span class="reward-admin-actions">
                <button
                  :aria-label="reward.isVisible === false ? t('shop.rewards.show', { title: reward.title }) : t('shop.rewards.hide', { title: reward.title })"
                  class="reward-visibility-button"
                  type="button"
                  @click="store.setShopRewardVisibility(reward.id, reward.isVisible === false)"
                ><v-icon :icon="reward.isVisible === false ? 'mdi-eye-outline' : 'mdi-eye-off-outline'" /></button>
                <button :aria-label="t('shop.rewards.deleteAria', { title: reward.title })" class="reward-delete-button" type="button" @click="rewardToDelete = reward"><v-icon icon="mdi-trash-can-outline" /></button>
              </span>
            </div>
            <div class="reward-icon">{{ reward.icon }}</div>
            <strong class="d-block mt-3">{{ reward.title }}</strong>
            <p class="reward-description text-caption text-medium-emphasis mt-1">{{ reward.description }}</p>
            <div class="reward-actions d-flex align-center justify-space-between ga-2" data-ladi-ignore>
              <span class="price"><LadirchenCoin small />{{ reward.price }}</span>
              <template v-if="store.viewerRole === 'child'">
                <v-btn v-if="reward.status === 'available'" class="request-reward-button" color="info" :disabled="!canRequest(reward)" prepend-icon="mdi-gift-open-outline" rounded="lg" size="small" variant="flat" @click="store.requestShopReward(reward.id)">{{ t('shop.rewards.request') }}</v-btn>
                <v-btn v-else-if="reward.status === 'requested' && reward.requesterId === store.activeChildId" color="warning" rounded="lg" size="small" variant="tonal" @click="store.cancelShopRewardRequest(reward.id)">{{ t('common.cancel') }}</v-btn>
                <v-chip v-else :color="reward.status === 'redeemed' ? 'success' : 'warning'" size="small" variant="tonal">{{ reward.status === 'redeemed' ? t('shop.rewards.redeemed') : t('shop.rewards.requested') }}</v-chip>
              </template>
              <template v-else>
                <div v-if="reward.status === 'requested'" class="d-flex ga-1">
                  <v-btn :aria-label="t('shop.rewards.reject')" icon="mdi-close" size="small" variant="tonal" @click="store.decideShopReward(reward.id, false)" />
                  <v-btn :aria-label="t('shop.rewards.approve')" color="primary" :disabled="!redemptionOpen" icon="mdi-check" size="small" variant="flat" @click="store.decideShopReward(reward.id, true)" />
                </div>
                <v-chip v-else :color="reward.status === 'redeemed' ? 'success' : 'primary'" size="small" variant="tonal">{{ reward.status === 'redeemed' ? t('shop.rewards.redeemed') : t('shop.rewards.available') }}</v-chip>
              </template>
            </div>
          </v-card>
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
            <v-card
              v-for="edition in houseEditions"
              :key="edition.id"
              class="edition-card pa-3"
              :class="{ active: edition.id === store.houseThemeId }"
              elevation="0"
              rounded="xl"
            >
              <div
                class="edition-preview"
                :class="`edition-${edition.id}`"
                :style="{ '--edition-roof': edition.roof, '--edition-wall': edition.wall, '--edition-door': edition.door, '--edition-accent': edition.landscapeAccent }"
              >
                <span class="edition-sky" aria-hidden="true">{{ edition.icon }}</span>
                <i class="edition-cloud cloud-left" aria-hidden="true" /><i class="edition-cloud cloud-right" aria-hidden="true" />
                <i class="edition-house" aria-hidden="true">
                  <b /><span class="edition-window window-left" /><span class="edition-window window-right" />
                </i>
                <template v-if="edition.id === 'cotton-candy-dream'">
                  <i class="edition-candy candy-one" aria-hidden="true" /><i class="edition-candy candy-two" aria-hidden="true" />
                  <i class="edition-lollipop lollipop-left" aria-hidden="true" /><i class="edition-lollipop lollipop-right" aria-hidden="true" />
                </template>
                <template v-else-if="edition.id === 'halloween-night'">
                  <i class="edition-moon" aria-hidden="true" /><i class="edition-bat bat-left" aria-hidden="true" /><i class="edition-bat bat-right" aria-hidden="true" />
                </template>
                <template v-else-if="edition.id === 'starlight-palace'">
                  <i class="edition-crown" aria-hidden="true" /><i class="edition-banner banner-left" aria-hidden="true" /><i class="edition-banner banner-right" aria-hidden="true" />
                  <i class="edition-festive-star star-left" aria-hidden="true">✦</i><i class="edition-festive-star star-right" aria-hidden="true">✦</i>
                </template>
                <template v-else>
                  <i class="edition-flower flower-left" aria-hidden="true" /><i class="edition-flower flower-right" aria-hidden="true" />
                </template>
                <v-chip v-if="edition.id === store.houseThemeId" class="edition-status" color="success" size="x-small">{{ t('shop.catalog.active') }}</v-chip>
              </div>
              <strong class="d-block mt-2">{{ edition.name }}</strong>
              <p class="catalog-description text-caption text-medium-emphasis mt-1">{{ edition.description }}</p>
              <div class="catalog-actions mt-3">
                <span v-if="edition.id !== store.houseThemeId" class="price"><LadirchenCoin small />{{ edition.price }}</span>
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
                <span v-else class="edition-selected"><v-icon icon="mdi-check-bold" />{{ t('shop.catalog.selected') }}<i aria-hidden="true">✦</i></span>
              </div>
            </v-card>
          </div>
        </section>

        <section v-if="houseArea !== 'special'" class="catalog-section mb-5">
          <div class="catalog-heading mb-3">
            <div><p class="eyebrow mb-1">{{ t('shop.catalog.sets.eyebrow') }}</p><h2 class="section-title">{{ t('shop.catalog.sets.title') }}</h2></div>
            <span>{{ houseArea === 'inside' ? t('shop.catalog.sets.inside') : t('shop.catalog.sets.outside') }}</span>
          </div>
          <div class="set-list">
            <v-card v-for="set in visibleFurnitureSets" :key="set.id" class="set-card pa-3" elevation="0" rounded="xl">
              <div class="set-icon">{{ set.icon }}</div>
              <div class="set-copy min-w-0">
                <strong>{{ set.name }}</strong>
                <p class="text-caption text-medium-emphasis">{{ set.description }}</p>
                <small>{{ t('shop.catalog.sets.meta', { count: set.accessoryIds.length, level: set.minimumHouseLevel + 1 }) }}</small>
              </div>
              <div class="set-action">
                <span v-if="ownsSet(set.id)" class="catalog-status catalog-status--owned"><i>✦</i><v-icon icon="mdi-check-decagram" size="16" />{{ t('shop.catalog.owned') }}<i>★</i></span>
                <span v-else-if="set.minimumHouseLevel > store.houseLevel" class="catalog-status catalog-status--locked"><i>✦</i><v-icon icon="mdi-lock-star" size="16" />{{ t('shop.catalog.locked') }}<i>✧</i></span>
                <v-btn
                  v-else-if="store.viewerRole === 'child'"
                  class="catalog-buy-button"
                  color="info"
                  :disabled="set.price > store.availableBalance"
                  rounded="lg"
                  size="small"
                  variant="flat"
                  @click="store.purchaseFurnitureSet(set.id)"
                ><span class="catalog-buy-price"><LadirchenCoin small />{{ set.price }} L</span><span>{{ t('shop.catalog.buy') }}</span></v-btn>
                <v-chip v-else color="info" size="x-small" variant="tonal">{{ t('shop.catalog.childPurchase') }}</v-chip>
              </div>
            </v-card>
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
          <v-card v-for="accessory in visibleAccessories" :key="accessory.id" class="accessory-card pa-4" :class="{ owned: accessory.owned, special: accessory.category === 'special' }" elevation="0" rounded="xl">
            <div class="accessory-preview">
              <RoomFurniture v-if="accessory.category === 'special'" class="special-item-preview" :item="accessory" />
              <span v-else>{{ accessory.icon }}</span>
              <v-chip v-if="accessory.motion && accessory.motion !== 'none'" class="motion-chip" color="warning" prepend-icon="mdi-creation-outline" size="x-small">{{ t('shop.catalog.animated') }}</v-chip>
            </div>
            <strong class="d-block mt-3">{{ accessory.title }}</strong>
            <p class="accessory-description text-caption text-medium-emphasis mt-1">{{ accessory.description }}</p>
            <div v-if="accessory.owned" class="owned-status-row">
              <span class="catalog-status catalog-status--owned"><i>✦</i><v-icon icon="mdi-check-decagram" size="16" />{{ t('shop.catalog.owned') }}<i>★</i></span>
            </div>
            <div v-if="!accessory.owned" class="catalog-actions accessory-actions ga-2">
              <span class="price"><LadirchenCoin small />{{ accessory.price }}</span>
              <v-btn v-if="store.viewerRole === 'child'" class="catalog-buy-button catalog-buy-button--compact" color="info" :disabled="accessory.price > store.availableBalance" rounded="lg" size="small" variant="flat" @click="store.purchaseAccessory(accessory.id)">{{ t('shop.catalog.buy') }}</v-btn>
              <v-chip v-else color="info" size="x-small" variant="tonal">{{ t('shop.catalog.childPurchase') }}</v-chip>
            </div>
          </v-card>
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
    <v-alert v-else color="primary" icon="mdi-lock-outline" variant="tonal">{{ t('shop.protected') }}</v-alert>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';

import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import PageViewSwitch from '@/shared/components/ui/PageViewSwitch.vue';
import type { PageViewOption } from '@/shared/components/ui/PageViewSwitch.vue';
import type { FurnitureSetId, HouseThemeId } from '@/domain/house';
import { shopRedemptionIsOpen, shopRewardIsPublished } from '@/domain/shop';
import type { ShopReward, ShopRewardCategory } from '@/domain/types';
import { useLocalizedDomainContent } from '@/shared/composables/use-localized-domain-content';
import { useFamilyWorldStore } from '@/stores/family-world';
import { FURNITURE_SETS, HOUSE_THEMES } from '@/domain/house-catalog';
import RoomFurniture from '@/features/world/components/RoomFurniture.vue';
import { calendarDateInTimeZone, calendarDateIsWithin } from '@/domain/zoned-calendar';

const store = useFamilyWorldStore();
const route = useRoute();
const { locale, t } = useI18n();
const localize = useLocalizedDomainContent();
const activeTab = ref<'family' | 'house'>('family');
const shopViewOptions = computed<ReadonlyArray<PageViewOption<'family' | 'house'>>>(() => [
  { id: 'family', icon: 'mdi-gift-outline', subtitle: t('shop.views.family.subtitle'), title: t('shop.views.family.title') },
  { id: 'house', icon: 'mdi-home-city-outline', subtitle: t('shop.views.house.subtitle'), title: t('shop.views.house.title') },
]);
type HouseCatalogArea = 'inside' | 'outside' | 'special';
const houseAreaOptions = computed<ReadonlyArray<PageViewOption<HouseCatalogArea>>>(() => [
  { id: 'inside', icon: 'mdi-sofa-outline', subtitle: t('shop.areas.inside.subtitle'), title: t('shop.areas.inside.title') },
  { id: 'outside', icon: 'mdi-flower-outline', subtitle: t('shop.areas.outside.subtitle'), title: t('shop.areas.outside.title') },
  { id: 'special', icon: 'mdi-creation-outline', subtitle: t('shop.areas.special.subtitle'), title: t('shop.areas.special.title') },
]);
const houseArea = ref<HouseCatalogArea>('inside');
const rewardDialog = ref(false);
const rewardToDelete = ref<ShopReward | null>(null);
const currentTime = ref(new Date());
let clockTimer: number | undefined;
const iconOptions = ['🎁', '🎮', '🍿', '🎨', '🎟️', '🧁', '🚲', '🌙', '💵', '🛍️'];
const categoryOptions = computed<Array<{ title: string; value: ShopRewardCategory }>>(() => [
  { title: t('shop.categories.time'), value: 'time' },
  { title: t('shop.categories.activity'), value: 'activity' },
  { title: t('shop.categories.allowance'), value: 'allowance' },
  { title: t('shop.categories.gift'), value: 'gift' },
  { title: t('shop.categories.privilege'), value: 'privilege' },
  { title: t('shop.categories.custom'), value: 'custom' },
]);
type PublicationMode = 'now' | 'scheduled' | 'hidden';
interface RewardDraft {
  title: string;
  description: string;
  icon: string;
  price: number;
  category: ShopRewardCategory;
  quantity: number;
  conditions: string;
  publicationMode: PublicationMode;
  availableFrom: string;
  unlimited: boolean;
  availableUntil: string;
}
const publicationModeOptions = computed<Array<{ title: string; value: PublicationMode }>>(() => [
  { title: t('shop.publication.now'), value: 'now' },
  { title: t('shop.publication.scheduled'), value: 'scheduled' },
  { title: t('shop.publication.hidden'), value: 'hidden' },
]);
const createEmptyReward = (): RewardDraft => ({
  title: '',
  description: '',
  icon: '🎁',
  price: 100,
  category: 'activity',
  quantity: 1,
  conditions: '',
  publicationMode: 'now',
  availableFrom: '',
  unlimited: true,
  availableUntil: '',
});
const newReward = reactive(createEmptyReward());
const minimumAvailableDate = computed(() => calendarDateInTimeZone(currentTime.value, store.familyTimeZone));
const canAddReward = computed(() => Boolean(
  newReward.icon.trim() && newReward.title.trim() && newReward.description.trim() && newReward.conditions.trim() &&
  newReward.price >= 1 && newReward.quantity >= 1 &&
  (newReward.publicationMode !== 'scheduled' || newReward.availableFrom) &&
  (newReward.unlimited || newReward.availableUntil) &&
  (!newReward.availableFrom || !newReward.availableUntil || newReward.availableFrom <= newReward.availableUntil),
));
const displayedShopRewards = computed(() => store.viewerRole === 'guardian'
  ? store.shopRewards.map(localize.reward)
  : store.shopRewards.filter(reward => shopRewardIsPublished(reward, store.familyTimeZone, currentTime.value)).map(localize.reward));
const localizedAccessories = computed(() => store.accessories.map(localize.accessory));
const visibleAccessories = computed(() => localizedAccessories.value.filter((accessory) => {
  if (houseArea.value === 'special') {return accessory.category === 'special';}
  if (accessory.category === 'special') {return false;}
  return accessory.placement === houseArea.value;
}));
const houseEditions = computed(() => HOUSE_THEMES.map(edition => ({
  ...edition,
  name: t(edition.nameKey),
  description: t(edition.descriptionKey),
})));
const visibleFurnitureSets = computed(() => FURNITURE_SETS
  .filter((set) => houseArea.value === 'outside' ? set.zoneId === 'garden' : houseArea.value === 'inside' && set.zoneId !== 'garden')
  .map(set => ({ ...set, name: t(set.nameKey), description: t(set.descriptionKey) })));
const ownsEdition = (id: HouseThemeId) => store.ownedHouseThemeIds.includes(id);
const ownsSet = (id: FurnitureSetId) => store.ownedFurnitureSetIds.includes(id);
const redemptionOpen = computed(() => shopRedemptionIsOpen(store.familyTimeZone, currentTime.value));
const canRequest = (reward: ShopReward) => redemptionOpen.value && reward.price <= store.availableBalance && reward.quantity > 0 && shopRewardIsPublished(reward, store.familyTimeZone, currentTime.value);
const rewardIsScheduled = (reward: ShopReward) => Boolean(
  reward.isVisible !== false && reward.availableFrom && reward.availableFrom > minimumAvailableDate.value,
);
const formatCalendarDate = (date: string) => new Intl.DateTimeFormat(locale.value === 'en' ? 'en-GB' : 'de-CH', {
  day: '2-digit', month: '2-digit', timeZone: 'UTC', year: 'numeric',
}).format(new Date(`${date}T12:00:00.000Z`));
const rewardPublicationStatus = (reward: ShopReward) => {
  if (reward.isVisible === false) return { icon: 'mdi-eye-off-outline', label: t('shop.publication.hiddenStatus'), tone: 'hidden' };
  if (rewardIsScheduled(reward)) return { icon: 'mdi-calendar-clock-outline', label: t('shop.publication.from', { date: formatCalendarDate(reward.availableFrom!) }), tone: 'scheduled' };
  if (reward.availableUntil && !calendarDateIsWithin(minimumAvailableDate.value, undefined, reward.availableUntil)) {
    return { icon: 'mdi-calendar-remove-outline', label: t('shop.publication.expired'), tone: 'expired' };
  }
  return { icon: 'mdi-eye-outline', label: t('shop.publication.visible'), tone: 'visible' };
};
const addReward = () => {
  store.addShopReward({
    title: newReward.title.trim(), description: newReward.description.trim(), icon: newReward.icon.trim(),
    price: newReward.price, category: newReward.category, quantity: newReward.quantity,
    conditions: newReward.conditions.trim(),
    availableFrom: newReward.publicationMode === 'scheduled' ? newReward.availableFrom : undefined,
    availableUntil: newReward.unlimited ? undefined : newReward.availableUntil,
    isVisible: newReward.publicationMode !== 'hidden',
  });
  Object.assign(newReward, createEmptyReward());
  rewardDialog.value = false;
};
const confirmRewardDeletion = () => {
  if (!rewardToDelete.value) return;
  store.deleteShopReward(rewardToDelete.value.id);
  rewardToDelete.value = null;
};
watch(
  () => route.query.new,
  (value) => {
    if (value === '1' && store.permissions.canManageContent) {
      activeTab.value = 'family';
      rewardDialog.value = true;
    }
  },
  { immediate: true },
);
onMounted(() => {
  clockTimer = window.setInterval(() => { currentTime.value = new Date(); }, 30_000);
  if (store.viewerRole === 'child') {
    window.setTimeout(() => window.dispatchEvent(new CustomEvent('ladi-guide:say', { detail: {
      heading: redemptionOpen.value ? t('shop.guide.openTitle') : t('shop.guide.closedTitle'),
      message: redemptionOpen.value ? t('shop.guide.openMessage') : t('shop.guide.closedMessage'),
      pageIntro: true,
    } })), 350);
  }
});
onUnmounted(() => {
  if (clockTimer !== undefined) window.clearInterval(clockTimer);
});
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.shop-mode-switch {
  position: relative;
  isolation: isolate;
  gap: 10px;
  padding: 9px;
  overflow: hidden;
  border: 2px solid var(--lad-palette-amber-200);
  border-radius: 28px;
  background:
    radial-gradient(
      circle at 10% 18%,
      color-mix(in srgb, var(--lad-palette-white) 95%, transparent) 0 4px,
      transparent 5px
    ),
    radial-gradient(
      circle at 89% 78%,
      color-mix(in srgb, var(--lad-palette-amber-450) 18%, transparent) 0 34px,
      transparent 35px
    ),
    linear-gradient(
      135deg,
      var(--lad-palette-surface),
      var(--lad-palette-amber-100)
    );
  box-shadow:
    0 6px 0 var(--lad-palette-amber-100),
    0 12px 24px color-mix(in srgb, var(--lad-palette-amber-650) 8%, transparent);
}
.shop-mode-switch::after {
  content: "";
  position: absolute;
  right: 11%;
  bottom: -21px;
  left: 11%;
  height: 30px;
  z-index: -1;
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-palette-amber-500) 10%, transparent);
  filter: blur(1px);
}
.shop-mode-switch :deep(button) {
  min-height: 82px;
  padding: 12px 14px;
  overflow: hidden;
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-orange-400-2) 12%, transparent);
  border-radius: 21px;
  background: color-mix(in srgb, var(--lad-palette-white) 35%, transparent);
}
.shop-mode-switch :deep(button::before) {
  content: "";
  width: 68px;
  height: 68px;
  position: absolute;
  top: -34px;
  right: -25px;
  border-radius: 50%;
  background: color-mix(in srgb, var(--switch-accent) 10%, transparent);
  transition: transform 180ms ease;
}
.shop-mode-switch :deep(button:nth-child(2)::before) {
  background: color-mix(in srgb, var(--lad-palette-mint) 10%, transparent);
}
.shop-mode-switch :deep(button:hover::before),
.shop-mode-switch :deep(button.active::before) {
  transform: scale(1.28);
}
.shop-mode-switch :deep(button.active) {
  transform: translateY(-2px);
  border-color: color-mix(
    in srgb,
    var(--lad-palette-amber-500) 30%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-palette-white),
    var(--lad-palette-surface)
  );
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--lad-palette-amber-500) 18%, transparent),
    0 12px 18px
      color-mix(in srgb, var(--lad-palette-amber-650) 10%, transparent);
}
.shop-mode-switch :deep(button:nth-child(2).active) {
  border-color: color-mix(in srgb, var(--lad-palette-mint) 35%, transparent);
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--lad-palette-mint) 18%, transparent),
    0 12px 18px color-mix(in srgb, var(--lad-palette-teal-700) 8%, transparent);
}
.shop-mode-switch :deep(.page-view-icon) {
  width: 48px;
  height: 48px;
  border-radius: 17px;
  transform: rotate(-4deg);
  box-shadow: inset 0 0 0 2px
    color-mix(in srgb, var(--lad-palette-white) 70%, transparent);
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
  font-size: 0.8125rem;
}
.shop-mode-switch :deep(.page-view-copy small) {
  margin-top: 4px;
  font-size: 0.5625rem;
}
.shop-mode-switch :deep(.page-view-check) {
  width: 23px;
  height: 23px;
  top: 9px;
  right: 9px;
  border: 3px solid var(--lad-palette-white);
  border-radius: 8px;
  transform: rotate(12deg) scale(0.65);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-amber-700) 15%, transparent);
}
.shop-mode-switch :deep(button.active .page-view-check) {
  transform: rotate(12deg) scale(1);
}

.shop-area-switch {
  position: relative;
  isolation: isolate;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;
  padding: 5px 3px 13px;
  overflow: visible;
  border: 0;
  border-radius: 0;
  background: transparent;
  box-shadow: none;
}
.shop-area-switch::before {
  content: "";
  position: absolute;
  right: 3px;
  bottom: 2px;
  left: 3px;
  height: 20px;
  z-index: -1;
  border: 2px solid var(--lad-palette-amber-200);
  border-radius: 50%;
  background: linear-gradient(
    var(--lad-palette-surface),
    var(--lad-palette-amber-100)
  );
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-palette-orange-350) 20%, transparent);
}
.shop-area-switch :deep(button) {
  min-height: 92px;
  padding: 9px 5px 12px;
  flex-direction: column;
  justify-content: center;
  gap: 5px;
  text-align: center;
  border: 2px solid var(--lad-palette-amber-200);
  border-radius: 24px 24px 17px 17px;
  background: linear-gradient(
    155deg,
    var(--lad-palette-surface),
    var(--lad-palette-amber-100)
  );
  box-shadow: 0 5px 0 var(--lad-palette-amber-200);
}
.shop-area-switch :deep(button:nth-child(2)) {
  border-color: var(--lad-palette-teal-150);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow: 0 5px 0 var(--lad-palette-teal-150);
}
.shop-area-switch :deep(button:nth-child(3)) {
  border-color: var(--lad-palette-blue-150);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow: 0 5px 0 var(--lad-palette-purple-200);
}
.shop-area-switch :deep(button:hover) {
  transform: translateY(-3px);
}
.shop-area-switch :deep(button.active) {
  transform: translateY(-6px) rotate(-1deg);
  border-color: var(--lad-palette-amber-450);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-amber-150)
  );
  box-shadow:
    0 8px 0 var(--lad-palette-orange-350),
    0 13px 18px
      color-mix(in srgb, var(--lad-palette-amber-650) 12%, transparent);
}
.shop-area-switch :deep(button:nth-child(2).active) {
  transform: translateY(-6px) rotate(1deg);
  border-color: var(--lad-palette-mint);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow:
    0 8px 0 var(--lad-palette-muted-250),
    0 13px 18px color-mix(in srgb, var(--lad-palette-teal-700) 10%, transparent);
}
.shop-area-switch :deep(button:nth-child(3).active) {
  transform: translateY(-6px) rotate(-1deg);
  border-color: var(--lad-palette-purple-350);
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow:
    0 8px 0 var(--lad-palette-purple-200),
    0 13px 18px
      color-mix(in srgb, var(--lad-palette-violet-650) 10%, transparent);
}
.shop-area-switch :deep(.page-view-icon) {
  width: 43px;
  height: 43px;
  border: 3px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  border-radius: 50%;
  color: var(--lad-palette-amber-550);
  background: var(--lad-palette-amber-150);
  box-shadow: 0 4px 0 var(--lad-palette-amber-200);
}
.shop-area-switch :deep(button:nth-child(2) .page-view-icon) {
  color: var(--lad-palette-mint-strong);
  background: var(--lad-palette-teal-150);
  box-shadow: 0 4px 0 var(--lad-palette-muted-250);
}
.shop-area-switch :deep(button:nth-child(3) .page-view-icon) {
  color: var(--lad-palette-violet-500);
  background: var(--lad-palette-background);
  box-shadow: 0 4px 0 var(--lad-palette-purple-200);
}
.shop-area-switch :deep(button.active .page-view-icon) {
  color: var(--lad-palette-white);
  background: linear-gradient(
    145deg,
    var(--lad-palette-yellow),
    var(--lad-palette-amber-500)
  );
  box-shadow: 0 4px 0 var(--lad-palette-amber-600);
  transform: rotate(-6deg) scale(1.06);
}
.shop-area-switch :deep(button:nth-child(2).active .page-view-icon) {
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-teal-550)
  );
  box-shadow: 0 4px 0 var(--lad-palette-teal-700);
  transform: rotate(6deg) scale(1.06);
}
.shop-area-switch :deep(button:nth-child(3).active .page-view-icon) {
  background: linear-gradient(
    145deg,
    var(--lad-palette-purple-350),
    var(--lad-palette-violet-400)
  );
  box-shadow: 0 4px 0 var(--lad-palette-violet-500);
  transform: rotate(-6deg) scale(1.06);
}
.shop-area-switch :deep(.page-view-copy strong) {
  font-size: 0.6875rem;
}
.shop-area-switch :deep(.page-view-copy small) {
  margin-top: 2px;
  font-size: 0.4375rem;
  line-height: 1.15;
}
.shop-area-switch :deep(.page-view-check) {
  width: 19px;
  height: 19px;
  top: 6px;
  right: 6px;
  border: 2px solid var(--lad-palette-white);
  background: var(--lad-palette-amber-500);
}
.shop-area-switch :deep(button:nth-child(2) .page-view-check) {
  background: var(--lad-palette-mint);
}
.shop-area-switch :deep(button:nth-child(3) .page-view-check) {
  background: var(--lad-palette-violet-400);
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
  font-size: 0.5625rem;
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
  border: 1px solid var(--lad-border);
  box-shadow: 0 3px 0 var(--lad-border) !important;
}
.edition-card.active {
  border-color: color-mix(in srgb, var(--lad-palette-mint) 60%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-mint) 25%, transparent) !important;
}
.edition-preview {
  height: 96px;
  flex: 0 0 96px;
  @apply position-relative overflow-hidden;
  border-radius: 14px;
  background: linear-gradient(
    160deg,
    var(--lad-palette-blue-150) 0 54%,
    var(--lad-palette-background) 55%
  );
  isolation: isolate;
}
.edition-preview::after {
  content: "";
  @apply position-absolute;
  right: 0;
  bottom: 0;
  left: 0;
  height: 30%;
  z-index: -1;
  background: linear-gradient(
    color-mix(in srgb, var(--edition-accent) 74%, white),
    var(--edition-accent)
  );
}
.edition-sky {
  @apply position-absolute;
  top: 6px;
  right: 8px;
  z-index: 1;
  font-size: 1.1875rem;
  filter: drop-shadow(
    0 2px 2px
      color-mix(in srgb, var(--lad-palette-muted-750-2) 15%, transparent)
  );
}
.edition-cloud {
  width: 29px;
  height: 10px;
  @apply position-absolute;
  top: 21px;
  z-index: -1;
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
}
.edition-cloud::before,
.edition-cloud::after {
  content: "";
  @apply position-absolute;
  bottom: 2px;
  border-radius: 50%;
  background: inherit;
}
.edition-cloud::before {
  width: 13px;
  height: 13px;
  left: 5px;
}
.edition-cloud::after {
  width: 17px;
  height: 17px;
  right: 3px;
}
.cloud-left {
  left: 7px;
}
.cloud-right {
  top: 35px;
  right: 2px;
  transform: scale(0.72);
}
.edition-house {
  width: 62px;
  height: 43px;
  @apply position-absolute;
  left: 50%;
  bottom: 13px;
  z-index: 2;
  transform: translateX(-50%);
  border: 3px solid
    color-mix(in srgb, var(--edition-roof) 64%, var(--lad-palette-muted-750-2));
  border-radius: 7px 7px 4px 4px;
  background: linear-gradient(
    90deg,
    color-mix(in srgb, var(--edition-wall) 88%, white),
    var(--edition-wall)
  );
  box-shadow:
    0 5px 0
      color-mix(
        in srgb,
        var(--edition-accent) 62%,
        var(--lad-palette-muted-600)
      ),
    0 7px 8px color-mix(in srgb, var(--lad-palette-muted-750) 20%, transparent);
}
.edition-house::before {
  content: "";
  @apply position-absolute;
  right: -10px;
  bottom: 34px;
  left: -10px;
  height: 32px;
  clip-path: polygon(50% 0, 100% 72%, 92% 100%, 50% 35%, 8% 100%, 0 72%);
  background: linear-gradient(
    150deg,
    color-mix(in srgb, var(--edition-roof) 72%, white),
    var(--edition-roof) 54%,
    color-mix(in srgb, var(--edition-roof) 78%, var(--lad-palette-muted-750-2))
  );
}
.edition-house b {
  width: 14px;
  height: 24px;
  @apply position-absolute;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-muted-750-2) 15%, transparent);
  border-bottom: 0;
  border-radius: 7px 7px 0 0;
  background: var(--edition-door);
}
.edition-window {
  width: 12px;
  height: 13px;
  @apply position-absolute;
  top: 8px;
  border: 3px solid var(--lad-palette-white);
  border-radius: 4px;
  background: var(--lad-palette-blue-250);
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-palette-muted-700) 12%, transparent);
}
.window-left {
  left: 6px;
}
.window-right {
  right: 6px;
}
.edition-cotton-candy-dream {
  background: linear-gradient(
    160deg,
    var(--lad-palette-background) 0 52%,
    var(--lad-palette-surface) 53%
  );
}
.edition-cotton-candy-dream::after {
  background: repeating-linear-gradient(
    135deg,
    var(--lad-palette-red-100) 0 9px,
    var(--lad-palette-blue-150) 9px 18px
  );
  opacity: 0.9;
}
.edition-cotton-candy-dream .edition-house {
  border-color: var(--lad-palette-purple-350);
  background: linear-gradient(
    90deg,
    var(--lad-palette-surface),
    var(--lad-palette-background)
  );
}
.edition-cotton-candy-dream .edition-house::before {
  bottom: 33px;
  height: 34px;
  filter: drop-shadow(0 -2px 0 var(--lad-palette-white));
  background:
    radial-gradient(
      circle at 10% 76%,
      var(--lad-palette-white) 0 8px,
      transparent 9px
    ),
    radial-gradient(
      circle at 28% 60%,
      var(--lad-palette-rose-200) 0 11px,
      transparent 12px
    ),
    radial-gradient(
      circle at 49% 48%,
      var(--lad-palette-blue-150) 0 12px,
      transparent 13px
    ),
    radial-gradient(
      circle at 69% 61%,
      var(--lad-palette-white) 0 11px,
      transparent 12px
    ),
    radial-gradient(
      circle at 89% 76%,
      var(--lad-palette-rose-200) 0 9px,
      transparent 10px
    );
}
.edition-cotton-candy-dream .edition-window {
  background: var(--lad-palette-amber-250);
}
.edition-candy {
  width: 12px;
  height: 8px;
  @apply position-absolute;
  z-index: 3;
  border: 2px solid var(--lad-palette-white);
  border-radius: var(--lad-radius-pill);
  background: repeating-linear-gradient(
    90deg,
    var(--lad-palette-rose-250) 0 4px,
    var(--lad-palette-blue-250) 4px 8px
  );
}
.candy-one {
  left: 17px;
  bottom: 13px;
  transform: rotate(18deg);
}
.candy-two {
  right: 15px;
  bottom: 26px;
  transform: rotate(-17deg);
}
.edition-lollipop {
  width: 14px;
  height: 14px;
  @apply position-absolute;
  bottom: 22px;
  z-index: 3;
  border: 2px solid var(--lad-palette-white);
  border-radius: 50%;
  background: conic-gradient(
    var(--lad-palette-rose-250) 0 25%,
    var(--lad-palette-white) 0 50%,
    var(--lad-palette-blue-250) 0 75%,
    var(--lad-palette-white) 0
  );
}
.edition-lollipop::after {
  content: "";
  width: 2px;
  height: 18px;
  @apply position-absolute;
  top: 11px;
  left: 4px;
  z-index: -1;
  transform: rotate(7deg);
  background: var(--lad-palette-orange-500);
}
.lollipop-left {
  left: 5px;
}
.lollipop-right {
  right: 5px;
  transform: scale(0.78);
}
.edition-halloween-night {
  background: linear-gradient(
    165deg,
    var(--lad-palette-text) 0 58%,
    var(--lad-palette-muted-600) 59%
  );
}
.edition-halloween-night::after {
  background: linear-gradient(
    var(--lad-palette-muted-600),
    var(--lad-palette-muted-600-2)
  );
}
.edition-halloween-night .edition-cloud {
  background: color-mix(in srgb, var(--lad-palette-muted-350) 35%, transparent);
}
.edition-moon {
  width: 24px;
  height: 24px;
  @apply position-absolute;
  top: 8px;
  left: 10px;
  border-radius: 50%;
  background: var(--lad-palette-amber-250);
  box-shadow: 0 0 12px
    color-mix(in srgb, var(--lad-palette-amber-250) 65%, transparent);
}
.edition-bat {
  width: 18px;
  height: 8px;
  @apply position-absolute;
  z-index: 3;
  clip-path: polygon(
    0 15%,
    25% 45%,
    50% 0,
    75% 45%,
    100% 15%,
    82% 100%,
    50% 65%,
    18% 100%
  );
  background: var(--lad-palette-text);
}
.bat-left {
  left: 13px;
  top: 40px;
  transform: rotate(-10deg);
}
.bat-right {
  right: 12px;
  top: 52px;
  transform: scale(0.72) rotate(12deg);
}
.edition-starlight-palace {
  background:
    radial-gradient(
      circle at 20% 20%,
      var(--lad-palette-amber-150) 0 9px,
      transparent 10px
    ),
    linear-gradient(
      165deg,
      var(--lad-palette-blue-150) 0 57%,
      var(--lad-palette-teal-150) 58%
    );
}
.edition-starlight-palace::after {
  background: linear-gradient(
    var(--lad-palette-green-250),
    var(--lad-palette-mint-450)
  );
}
.edition-starlight-palace .edition-house {
  border-color: var(--lad-palette-amber-550);
  background: linear-gradient(
    90deg,
    var(--lad-palette-surface),
    var(--lad-palette-background)
  );
  box-shadow:
    0 5px 0 var(--lad-palette-orange-400-2),
    0 7px 12px
      color-mix(in srgb, var(--lad-palette-muted-750-2) 20%, transparent);
}
.edition-starlight-palace .edition-house::before {
  background: linear-gradient(
    150deg,
    var(--lad-palette-blue-450),
    var(--lad-palette-blue-550) 55%,
    var(--lad-palette-indigo-750)
  );
}
.edition-starlight-palace .edition-window {
  background: var(--lad-palette-amber-250);
  box-shadow: 0 0 8px
    color-mix(in srgb, var(--lad-palette-yellow) 75%, transparent);
}
.edition-crown {
  width: 26px;
  height: 18px;
  @apply position-absolute;
  top: 5px;
  left: 50%;
  z-index: 4;
  transform: translateX(-50%);
  clip-path: polygon(
    0 100%,
    0 30%,
    25% 62%,
    50% 0,
    75% 62%,
    100% 30%,
    100% 100%
  );
  background: linear-gradient(
    var(--lad-palette-amber-150),
    var(--lad-palette-amber-450)
  );
  filter: drop-shadow(
    0 2px 1px color-mix(in srgb, var(--lad-palette-amber-700) 25%, transparent)
  );
}
.edition-banner {
  width: 3px;
  height: 23px;
  @apply position-absolute;
  bottom: 41px;
  z-index: 3;
  background: var(--lad-palette-amber-550);
}
.edition-banner::after {
  content: "";
  width: 12px;
  height: 9px;
  @apply position-absolute;
  top: 1px;
  left: 2px;
  clip-path: polygon(0 0, 100% 20%, 70% 100%, 0 82%);
  background: var(--lad-palette-muted-500);
}
.banner-left {
  left: 20px;
}
.banner-right {
  right: 22px;
  transform: scaleX(-1);
}
.edition-festive-star {
  @apply position-absolute;
  z-index: 4;
  color: var(--lad-palette-amber-450);
  font-size: 0.75rem;
  font-style: normal;
  text-shadow: 0 0 6px var(--lad-palette-amber-150);
  animation: edition-star-twinkle 1.9s ease-in-out infinite;
}
.star-left {
  top: 22px;
  left: 12px;
}
.star-right {
  top: 17px;
  right: 14px;
  animation-delay: -0.9s;
}
@keyframes edition-star-twinkle {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.7) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(25deg);
  }
}
.edition-flower {
  width: 8px;
  height: 8px;
  @apply position-absolute;
  bottom: 17px;
  z-index: 3;
  border: 2px solid var(--lad-palette-amber-150);
  border-radius: 50%;
  background: var(--lad-palette-red-300);
  box-shadow: 0 7px 0 -2px var(--lad-palette-teal-550);
}
.flower-left {
  left: 13px;
}
.flower-right {
  right: 12px;
  background: var(--lad-palette-purple-350);
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
  margin-top: auto !important;
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
  color: var(--lad-palette-white);
  border: 2px solid var(--lad-palette-white);
  border-radius: 14px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-teal-550)
  );
  box-shadow:
    0 4px 0 var(--lad-palette-teal-700),
    0 8px 14px color-mix(in srgb, var(--lad-palette-teal-700) 15%, transparent);
  font-size: 0.625rem;
  font-weight: var(--lad-font-weight-black);
  animation: edition-selected-breathe 2.5s ease-in-out infinite;
}
.edition-selected :deep(.v-icon) {
  font-size: 1.0625rem;
  animation: edition-check-pop 2.5s ease-in-out infinite;
}
.edition-selected i {
  @apply position-absolute;
  top: 3px;
  right: 6px;
  color: var(--lad-palette-amber-150);
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
  grid-template-columns: 64px minmax(0, 1fr) auto;
  gap: 13px;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-teal-550) 18%, transparent);
  background:
    radial-gradient(
      circle at 88% 5%,
      color-mix(in srgb, var(--lad-palette-amber-250) 25%, transparent),
      transparent 28%
    ),
    linear-gradient(
      145deg,
      var(--lad-palette-white),
      var(--lad-palette-surface)
    );
  box-shadow:
    0 6px 0 color-mix(in srgb, var(--lad-palette-teal-600) 15%, transparent),
    0 13px 25px color-mix(in srgb, var(--lad-palette-muted-700) 8%, transparent) !important;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.set-card:hover {
  transform: translateY(-2px);
  box-shadow:
    0 8px 0 color-mix(in srgb, var(--lad-palette-teal-600) 15%, transparent),
    0 16px 28px
      color-mix(in srgb, var(--lad-palette-muted-700) 10%, transparent) !important;
}
.set-icon {
  width: 64px;
  height: 64px;
  @apply d-grid place-center;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 90%, transparent);
  border-radius: 20px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  );
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-palette-teal-550) 12%, transparent);
  font-size: 2rem;
  transform: rotate(-3deg);
}
.set-copy strong {
  font-size: 0.9375rem;
}
.set-copy p {
  margin-top: 3px;
  line-height: 1.35;
}
.set-copy small {
  color: var(--lad-mint-dark);
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
  @apply d-inline-flex align-center;
  gap: 5px;
  color: var(--lad-palette-amber-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-amber-450) 25%, transparent);
  border-radius: var(--lad-radius-pill);
  background: linear-gradient(
    145deg,
    var(--lad-palette-amber-100),
    var(--lad-palette-amber-150)
  );
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--lad-palette-amber-550) 15%, transparent);
  font-size: 0.75rem;
  font-weight: var(--lad-font-weight-black);
}
.catalog-buy-button {
  min-height: 48px !important;
  padding-inline: 8px 12px !important;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 80%, transparent) !important;
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
  text-transform: none;
  letter-spacing: 0;
}
.catalog-buy-button:active {
  transform: translateY(3px);
  box-shadow: 0 2px 0 var(--lad-palette-blue-strong) !important;
}
.catalog-buy-price {
  padding: 4px 8px 4px 4px;
  @apply d-inline-flex align-center;
  gap: 4px;
  border-radius: var(--lad-radius-pill);
  background: color-mix(in srgb, var(--lad-palette-white) 20%, transparent);
  font-weight: var(--lad-font-weight-black);
}
.catalog-buy-button--compact {
  min-height: 38px !important;
  padding-inline: 13px !important;
}
.reward-card,
.accessory-card {
  @apply min-w-0;
  border: 1px solid var(--lad-border);
  box-shadow: 0 4px 0 var(--lad-border) !important;
}
.accessory-card {
  @apply h-100 d-flex flex-column;
}
.reward-card {
  @apply position-relative h-100 d-flex flex-column;
  transition:
    opacity 0.18s ease,
    filter 0.18s ease;
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
  overflow: hidden;
  border-radius: var(--lad-radius-pill);
  font-size: 0.5rem;
  font-weight: var(--lad-font-weight-heavy);
  line-height: 1;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.reward-publication-state :deep(.v-icon) {
  flex: 0 0 auto;
  font-size: 0.8125rem;
}
.reward-publication-state--visible {
  color: var(--lad-palette-teal-700);
  background: var(--lad-palette-background);
}
.reward-publication-state--scheduled {
  color: var(--lad-palette-blue-600);
  background: var(--lad-palette-background);
}
.reward-publication-state--hidden,
.reward-publication-state--expired {
  color: var(--lad-palette-muted-600);
  background: var(--lad-palette-background);
}
.reward-admin-actions {
  @apply d-flex flex-shrink-0;
  gap: 4px;
}
.reward-visibility-button,
.reward-delete-button {
  width: 28px;
  height: 28px;
  @apply d-grid place-center flex-shrink-0 cursor-pointer;
  color: var(--lad-palette-muted-700);
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-teal-600) 15%, transparent);
  border-radius: 10px;
  background: color-mix(in srgb, var(--lad-palette-white) 80%, transparent);
  box-shadow: 0 2px 0
    color-mix(in srgb, var(--lad-palette-teal-600) 10%, transparent);
}
.reward-delete-button {
  color: var(--lad-palette-red-500);
  border-color: color-mix(in srgb, var(--lad-palette-red-500) 15%, transparent);
  background: var(--lad-palette-surface);
}
.reward-visibility-button :deep(.v-icon),
.reward-delete-button :deep(.v-icon) {
  font-size: 1rem;
}
.reward-visibility-button:active,
.reward-delete-button:active {
  transform: translateY(1px);
  box-shadow: none;
}
.request-reward-button {
  min-height: 40px !important;
  padding-inline: 13px !important;
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 85%, transparent) !important;
  border-radius: 15px !important;
  background: linear-gradient(
    145deg,
    var(--lad-palette-blue-350),
    var(--lad-palette-blue)
  ) !important;
  box-shadow:
    0 5px 0 var(--lad-palette-blue-strong),
    0 9px 16px
      color-mix(in srgb, var(--lad-palette-blue-strong) 18%, transparent) !important;
  text-transform: none;
  letter-spacing: 0;
}
.request-reward-button:active {
  transform: translateY(3px);
  box-shadow: 0 2px 0 var(--lad-palette-blue-strong) !important;
}
.request-reward-button:disabled {
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-palette-blue-600) 15%, transparent) !important;
}
.reward-icon,
.accessory-preview {
  height: 78px;
  @apply d-grid place-center;
  border-radius: 16px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-amber-100)
  );
  font-size: 2.625rem;
}
.accessory-preview {
  @apply position-relative;
}
.accessory-card.special {
  border-color: color-mix(
    in srgb,
    var(--lad-palette-amber-450) 40%,
    transparent
  );
  background: linear-gradient(
    155deg,
    var(--lad-palette-surface),
    var(--lad-palette-amber-100)
  );
}
.special-item-preview {
  width: 104px;
  height: 104px;
}
.motion-chip {
  @apply position-absolute;
  top: 7px;
  left: 7px;
  font-size: 0.5rem;
}
.owned-status-row {
  margin-top: auto;
  padding-top: 14px;
  @apply d-flex justify-center;
}
.catalog-status {
  min-height: 38px;
  padding: 6px 13px;
  @apply position-relative d-inline-flex align-center justify-center;
  gap: 6px;
  border: 2px solid;
  border-radius: var(--lad-radius-pill);
  font-size: 0.6875rem;
  font-weight: var(--lad-font-weight-black);
  white-space: nowrap;
}
.catalog-status--owned {
  color: var(--lad-palette-teal-700);
  border-color: color-mix(
    in srgb,
    var(--lad-palette-teal-550) 25%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-palette-background),
    var(--lad-palette-teal-150)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-mint-strong) 15%, transparent),
    0 8px 15px color-mix(in srgb, var(--lad-palette-teal-600) 8%, transparent);
}
.catalog-status--locked {
  color: var(--lad-palette-amber-700);
  border-color: color-mix(
    in srgb,
    var(--lad-palette-amber-450) 30%,
    transparent
  );
  background: linear-gradient(
    145deg,
    var(--lad-palette-amber-100),
    var(--lad-palette-amber-150)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-palette-amber-550) 15%, transparent),
    0 8px 14px color-mix(in srgb, var(--lad-palette-amber-650) 8%, transparent);
}
.catalog-status i {
  color: var(--lad-palette-amber-500);
  font-style: normal;
  text-shadow: 0 0 6px
    color-mix(in srgb, var(--lad-palette-yellow) 80%, transparent);
  animation: owned-star 1.9s ease-in-out infinite;
}
.catalog-status i:last-child {
  animation-delay: -0.9s;
}
.catalog-status--locked :deep(.v-icon) {
  color: var(--lad-palette-amber-500);
  filter: drop-shadow(
    0 2px 0 color-mix(in srgb, var(--lad-palette-white) 80%, transparent)
  );
}
.reward-icon {
  width: 78px;
  @apply flex-shrink-0;
}
.reward-description,
.accessory-description {
  min-height: 47px;
}
.reward-actions {
  margin-top: auto;
  padding-top: 16px;
}
.price {
  @apply d-flex align-center;
  gap: 7px;
  font-size: 0.8125rem;
  @apply font-weight-black;
}
.accessory-card.owned {
  border-color: color-mix(in srgb, var(--lad-palette-mint) 40%, transparent);
}
.reward-form {
  grid-template-columns: 0.7fr 1.3fr;
}
.field-label {
  color: var(--lad-muted);
  font-size: 0.6875rem;
  font-weight: 800;
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
  font-size: 1.4375rem;
}
.icon-picker button.active {
  border: 2px solid var(--lad-mint);
  background: var(--lad-palette-background);
}
@include respond-down(catalog) {
  .edition-grid {
    grid-template-columns: 1fr;
  }
  .catalog-description {
    min-height: 0;
  }
  .edition-card {
    display: grid;
    grid-template-columns: 112px minmax(0, 1fr);
    column-gap: 10px;
  }
  .edition-preview {
    grid-row: span 3;
  }
  .edition-card > strong {
    margin-top: 2px !important;
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
  .reward-description,
  .accessory-description {
    min-height: 0;
  }
  .set-card {
    grid-template-columns: 52px minmax(0, 1fr);
  }
  .set-icon {
    width: 52px;
    height: 52px;
  }
  .set-action {
    grid-column: 2;
    justify-content: flex-start;
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
      0 4px 0 var(--lad-palette-teal-700),
      0 8px 14px
        color-mix(in srgb, var(--lad-palette-teal-700) 15%, transparent);
  }
  50% {
    box-shadow:
      0 4px 0 var(--lad-palette-teal-700),
      0 8px 18px
        color-mix(in srgb, var(--lad-palette-teal-700) 30%, transparent),
      0 0 0 4px color-mix(in srgb, var(--lad-palette-teal-400) 10%, transparent);
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
