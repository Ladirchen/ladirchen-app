<template>
  <div class="page page-padding shop-page">
    <PageHeader
      description="Reale Familienbelohnungen und virtuelle Hausdekorationen bleiben klar voneinander getrennt."
      eyebrow="Belohnungen & Gestaltung"
      title="Shop"
      tone="amber"
    >
      <template #icon><AnimatedSectionIcon variant="shop" /></template>
    </PageHeader>

    <template v-if="store.viewerRole === 'child' || store.permissions.canManageContent">
      <PageViewSwitch v-model="activeTab" class="shop-mode-switch mb-5" label="Shopbereich auswählen" :options="shopViewOptions" tone="amber" />

      <template v-if="activeTab === 'family'">
        <v-card class="redemption-policy pa-4 mb-5" :color="redemptionOpen ? 'green-lighten-5' : 'red-lighten-5'" elevation="0" rounded="xl">
          <div class="d-flex align-center ga-3">
            <v-avatar class="redemption-clock" :color="redemptionOpen ? 'primary' : 'error'" size="48" variant="tonal"><v-icon>mdi-clock-outline</v-icon></v-avatar>
            <div class="flex-grow-1">
              <div class="d-flex align-center justify-space-between flex-wrap ga-2">
                <strong>Einlösen nur bis 18:00 Uhr</strong>
                <v-chip class="redemption-status" :color="redemptionOpen ? 'success' : 'error'" size="small">{{ redemptionOpen ? 'Heute geöffnet' : 'Für heute geschlossen' }}</v-chip>
              </div>
              <p class="text-caption text-medium-emphasis mt-1">Belohnungen müssen vor 18:00 Uhr angefragt und freigegeben werden. Danach ist die Einlösung erst am nächsten Tag wieder möglich.</p>
            </div>
          </div>
        </v-card>

        <div class="d-flex align-end justify-space-between mb-3">
          <div><h2 class="section-title">Belohnungen</h2></div>
          <v-btn v-if="store.permissions.canManageContent" color="primary" prepend-icon="mdi-plus" rounded="lg" size="small" variant="tonal" @click="rewardDialog = true">Hinzufügen</v-btn>
        </div>

        <div class="reward-grid">
          <v-card v-for="reward in store.shopRewards" :key="reward.id" class="reward-card pa-4" elevation="0" rounded="xl">
            <div class="reward-icon">{{ reward.icon }}</div>
            <strong class="d-block mt-3">{{ reward.title }}</strong>
            <p class="reward-description text-caption text-medium-emphasis mt-1">{{ reward.description }}</p>
            <p class="reward-conditions mt-3">
              <span class="condition-ladi" aria-hidden="true"><LadiMascot :score="4.4" :show-scene-base="false" :show-score="false" :size="42" /><i>☝️</i></span>
              <span><strong>Bedingung</strong>{{ reward.conditions }}</span>
            </p>
            <div class="reward-actions d-flex align-center justify-space-between ga-2">
              <span class="price"><LadirchenCoin small />{{ reward.price }}</span>
              <template v-if="store.viewerRole === 'child'">
                <v-btn v-if="reward.status === 'available'" class="request-reward-button" color="info" :disabled="!canRequest(reward)" prepend-icon="mdi-gift-open-outline" rounded="lg" size="small" variant="flat" @click="store.requestShopReward(reward.id)">Anfragen</v-btn>
                <v-btn v-else-if="reward.status === 'requested' && reward.requesterId === store.activeChildId" color="warning" rounded="lg" size="small" variant="tonal" @click="store.cancelShopRewardRequest(reward.id)">Stornieren</v-btn>
                <v-chip v-else :color="reward.status === 'redeemed' ? 'success' : 'warning'" size="small" variant="tonal">{{ reward.status === 'redeemed' ? 'Eingelöst' : 'Angefragt' }}</v-chip>
              </template>
              <template v-else>
                <div v-if="reward.status === 'requested'" class="d-flex ga-1">
                  <v-btn aria-label="Anfrage ablehnen" icon="mdi-close" size="small" variant="tonal" @click="store.decideShopReward(reward.id, false)" />
                  <v-btn aria-label="Anfrage freigeben" color="primary" :disabled="!redemptionOpen" icon="mdi-check" size="small" variant="flat" @click="store.decideShopReward(reward.id, true)" />
                </div>
                <v-chip v-else :color="reward.status === 'redeemed' ? 'success' : 'primary'" size="small" variant="tonal">{{ reward.status === 'redeemed' ? 'Eingelöst' : 'Verfügbar' }}</v-chip>
              </template>
            </div>
          </v-card>
        </div>

      </template>

      <template v-else>
        <PageViewSwitch v-model="houseArea" compact label="Hausbereich auswählen" class="shop-area-switch mb-4" :options="houseAreaOptions" tone="amber" />

        <section v-if="houseArea === 'outside'" class="catalog-section mb-5">
          <div class="catalog-heading mb-3">
            <div><p class="eyebrow mb-1">Hauseditionen</p><h2 class="section-title">Ein neuer Stil für euer Zuhause</h2></div>
            <span>Bleibt dauerhaft gespeichert</span>
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
                <template v-else>
                  <i class="edition-flower flower-left" aria-hidden="true" /><i class="edition-flower flower-right" aria-hidden="true" />
                </template>
                <v-chip v-if="edition.id === store.houseThemeId" class="edition-status" color="success" size="x-small">Aktiv</v-chip>
              </div>
              <strong class="d-block mt-2">{{ edition.name }}</strong>
              <p class="catalog-description text-caption text-medium-emphasis mt-1">{{ edition.description }}</p>
              <div class="catalog-actions mt-3">
                <span class="price"><LadirchenCoin small />{{ edition.price }}</span>
                <v-btn
                  v-if="!ownsEdition(edition.id) && store.viewerRole === 'child'"
                  class="catalog-buy-button catalog-buy-button--compact"
                  color="info"
                  :disabled="edition.price > store.availableBalance"
                  rounded="lg"
                  size="small"
                  variant="flat"
                  @click="store.purchaseHouseTheme(edition.id)"
                >Kaufen</v-btn>
                <v-chip v-else-if="!ownsEdition(edition.id)" color="info" size="x-small" variant="tonal">Kinderkauf</v-chip>
                <v-btn v-else-if="edition.id !== store.houseThemeId" color="primary" rounded="lg" size="small" variant="tonal" @click="store.selectHouseTheme(edition.id)">Verwenden</v-btn>
                <v-chip v-else color="success" size="small" variant="tonal">Ausgewählt</v-chip>
              </div>
            </v-card>
          </div>
        </section>

        <section v-if="houseArea !== 'special'" class="catalog-section mb-5">
          <div class="catalog-heading mb-3">
            <div><p class="eyebrow mb-1">Komplette Sets</p><h2 class="section-title">Aufeinander abgestimmt</h2></div>
            <span>{{ houseArea === 'inside' ? 'Für eure Räume' : 'Für Garten & Fassade' }}</span>
          </div>
          <div class="set-list">
            <v-card v-for="set in visibleFurnitureSets" :key="set.id" class="set-card pa-3" elevation="0" rounded="xl">
              <div class="set-icon">{{ set.icon }}</div>
              <div class="set-copy min-w-0">
                <strong>{{ set.name }}</strong>
                <p class="text-caption text-medium-emphasis">{{ set.description }}</p>
                <small>{{ set.accessoryIds.length }} Elemente · ab Hausstufe {{ set.minimumHouseLevel + 1 }}</small>
              </div>
              <div class="set-action">
                <v-chip v-if="ownsSet(set.id)" color="success" size="x-small" variant="tonal">In Besitz</v-chip>
                <v-chip v-else-if="set.minimumHouseLevel > store.houseLevel" class="locked-set-status" color="warning" prepend-icon="mdi-lock-star" size="small">Noch gesperrt</v-chip>
                <v-btn
                  v-else-if="store.viewerRole === 'child'"
                  class="catalog-buy-button"
                  color="info"
                  :disabled="set.price > store.availableBalance"
                  rounded="lg"
                  size="small"
                  variant="flat"
                  @click="store.purchaseFurnitureSet(set.id)"
                ><span class="catalog-buy-price"><LadirchenCoin small />{{ set.price }} L</span><span>Kaufen</span></v-btn>
                <v-chip v-else color="info" size="x-small" variant="tonal">Kinderkauf</v-chip>
              </div>
            </v-card>
          </div>
        </section>

        <div class="catalog-heading mb-3">
          <div>
            <p class="eyebrow mb-1">{{ houseArea === 'special' ? 'Besondere Sammlerstücke' : 'Einzelstücke' }}</p>
            <h2 class="section-title">{{ houseArea === 'special' ? 'Animierte Highlights' : 'Frei kombinieren' }}</h2>
          </div>
          <span v-if="houseArea === 'special'">Bewegen sich in eurer Familienwelt</span>
        </div>

        <div class="accessory-grid">
          <v-card v-for="accessory in visibleAccessories" :key="accessory.id" class="accessory-card pa-4" :class="{ owned: accessory.owned, special: accessory.category === 'special' }" elevation="0" rounded="xl">
            <div class="accessory-preview">
              <RoomFurniture v-if="accessory.category === 'special'" class="special-item-preview" :item="accessory" />
              <span v-else>{{ accessory.icon }}</span>
              <v-chip v-if="accessory.motion && accessory.motion !== 'none'" class="motion-chip" color="warning" prepend-icon="mdi-creation-outline" size="x-small">Animiert</v-chip>
            </div>
            <strong class="d-block mt-3">{{ accessory.title }}</strong>
            <p class="accessory-description text-caption text-medium-emphasis mt-1">{{ accessory.description }}</p>
            <div v-if="accessory.owned" class="owned-status-row">
              <span class="owned-status"><i>✦</i><v-icon icon="mdi-check-decagram" size="16" />In Besitz<i>★</i></span>
            </div>
            <div v-if="!accessory.owned" class="catalog-actions accessory-actions ga-2">
              <span class="price"><LadirchenCoin small />{{ accessory.price }}</span>
              <v-btn v-if="store.viewerRole === 'child'" class="catalog-buy-button catalog-buy-button--compact" color="info" :disabled="accessory.price > store.availableBalance" rounded="lg" size="small" variant="flat" @click="store.purchaseAccessory(accessory.id)">Kaufen</v-btn>
              <v-chip v-else color="info" size="x-small" variant="tonal">Kinderkauf</v-chip>
            </div>
          </v-card>
        </div>
      </template>

      <v-dialog v-model="rewardDialog" max-width="430">
        <v-card class="pa-5" rounded="xl">
          <v-card-title class="pa-0 mb-1">Shop-Element hinzufügen</v-card-title>
          <v-card-subtitle class="pa-0 mb-4">Alle Pflichtangaben werden den Kindern verständlich angezeigt.</v-card-subtitle>
          <label class="field-label">Icon auswählen</label>
          <div class="icon-picker mt-2 mb-4">
            <button v-for="icon in iconOptions" :key="icon" :class="{ active: newReward.icon === icon }" type="button" @click="newReward.icon = icon">{{ icon }}</button>
          </div>
          <v-text-field v-model="newReward.icon" label="Eigenes Icon oder Emoji" maxlength="4" variant="outlined" />
          <v-text-field v-model="newReward.title" label="Titel" variant="outlined" />
          <v-textarea v-model="newReward.description" label="Beschreibung" rows="2" variant="outlined" />
          <v-select v-model="newReward.category" :items="categoryOptions" item-title="title" item-value="value" label="Kategorie" variant="outlined" />
          <div class="d-grid reward-form ga-3">
            <v-text-field v-model.number="newReward.price" label="Preis" min="1" suffix="L" type="number" variant="outlined" />
            <v-text-field v-model.number="newReward.quantity" label="Verfügbare Anzahl" min="1" type="number" variant="outlined" />
          </div>
          <v-textarea v-model="newReward.conditions" label="Einlösebedingungen" rows="2" variant="outlined" />
          <v-switch v-model="newReward.unlimited" color="primary" inset label="Ohne Ablaufdatum" />
          <v-text-field v-if="!newReward.unlimited" v-model="newReward.availableUntil" :min="minimumAvailableDate" label="Verfügbar bis" type="date" variant="outlined" />
          <div class="d-flex justify-end ga-2">
            <v-btn rounded="lg" variant="text" @click="rewardDialog = false">Abbrechen</v-btn>
            <v-btn color="primary" :disabled="!canAddReward" rounded="lg" variant="flat" @click="addReward">In Shop stellen</v-btn>
          </div>
        </v-card>
      </v-dialog>
    </template>
    <v-alert v-else color="primary" icon="mdi-lock-outline" variant="tonal">Der Familien-Shop ist für Kinder und Administratoren geschützt. Du kannst öffentliche Kinderziele im Bereich „Wünsche“ unterstützen.</v-alert>
  </div>
</template>

<script lang="ts" setup>
import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { useRoute } from 'vue-router';

import AnimatedSectionIcon from '@/shared/components/ui/AnimatedSectionIcon.vue';
import LadiMascot from '@/shared/components/LadiMascot.vue';
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import PageHeader from '@/shared/components/ui/PageHeader.vue';
import PageViewSwitch from '@/shared/components/ui/PageViewSwitch.vue';
import type { PageViewOption } from '@/shared/components/ui/PageViewSwitch.vue';
import type { FurnitureSetId, HouseThemeId } from '@/domain/house';
import { shopRedemptionIsOpen } from '@/domain/shop';
import type { ShopReward, ShopRewardCategory } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';
import { FURNITURE_SETS, HOUSE_THEMES } from '@/features/world/data/house-catalog';
import RoomFurniture from '@/features/world/components/RoomFurniture.vue';

const store = useFamilyWorldStore();
const route = useRoute();
const activeTab = ref<'family' | 'house'>('family');
const shopViewOptions: ReadonlyArray<PageViewOption<'family' | 'house'>> = [
  { id: 'family', icon: 'mdi-gift-outline', subtitle: 'Belohnungen einlösen', title: 'Familien-Shop' },
  { id: 'house', icon: 'mdi-home-city-outline', subtitle: 'Eure Welt gestalten', title: 'Hauskatalog' },
];
type HouseCatalogArea = 'inside' | 'outside' | 'special';
const houseAreaOptions: ReadonlyArray<PageViewOption<HouseCatalogArea>> = [
  { id: 'inside', icon: 'mdi-sofa-outline', subtitle: 'Möbel und Räume', title: 'Innen' },
  { id: 'outside', icon: 'mdi-flower-outline', subtitle: 'Garten und Fassade', title: 'Draußen' },
  { id: 'special', icon: 'mdi-creation-outline', subtitle: 'Animierte Einzelstücke', title: 'Special' },
];
const houseArea = ref<HouseCatalogArea>('inside');
const rewardDialog = ref(false);
const currentTime = ref(new Date());
let clockTimer: number | undefined;
const iconOptions = ['🎁', '🎮', '🍿', '🎨', '🎟️', '🧁', '🚲', '🌙', '💵', '🛍️'];
const categoryOptions: Array<{ title: string; value: ShopRewardCategory }> = [
  { title: 'Zeit', value: 'time' },
  { title: 'Gemeinsame Aktivität', value: 'activity' },
  { title: 'Taschengeld', value: 'allowance' },
  { title: 'Geschenk', value: 'gift' },
  { title: 'Privileg', value: 'privilege' },
  { title: 'Eigene Kategorie', value: 'custom' },
];
const createEmptyReward = () => ({ title: '', description: '', icon: '🎁', price: 100, category: 'activity' as ShopRewardCategory, quantity: 1, conditions: '', unlimited: true, availableUntil: '' });
const newReward = reactive(createEmptyReward());
const minimumAvailableDate = new Date().toLocaleDateString('sv-SE');
const canAddReward = computed(() => Boolean(
  newReward.icon.trim() && newReward.title.trim() && newReward.description.trim() && newReward.conditions.trim() &&
  newReward.price >= 1 && newReward.quantity >= 1 && (newReward.unlimited || newReward.availableUntil),
));
const visibleAccessories = computed(() => store.accessories.filter((accessory) => {
  if (houseArea.value === 'special') {return accessory.category === 'special';}
  if (accessory.category === 'special') {return false;}
  return accessory.placement === houseArea.value;
}));
const houseEditions = HOUSE_THEMES;
const visibleFurnitureSets = computed(() => FURNITURE_SETS.filter((set) =>
  houseArea.value === 'outside' ? set.zoneId === 'garden' : houseArea.value === 'inside' && set.zoneId !== 'garden',
));
const ownsEdition = (id: HouseThemeId) => store.ownedHouseThemeIds.includes(id);
const ownsSet = (id: FurnitureSetId) => store.ownedFurnitureSetIds.includes(id);
const redemptionOpen = computed(() => shopRedemptionIsOpen(currentTime.value));
const rewardHasNotExpired = (reward: ShopReward) => !reward.availableUntil || new Date(`${reward.availableUntil}T23:59:59`).getTime() >= Date.now();
const canRequest = (reward: ShopReward) => redemptionOpen.value && reward.price <= store.availableBalance && reward.quantity > 0 && rewardHasNotExpired(reward);
const addReward = () => {
  store.addShopReward({
    title: newReward.title.trim(), description: newReward.description.trim(), icon: newReward.icon.trim(),
    price: newReward.price, category: newReward.category, quantity: newReward.quantity,
    conditions: newReward.conditions.trim(), availableUntil: newReward.unlimited ? undefined : newReward.availableUntil,
  });
  Object.assign(newReward, createEmptyReward());
  rewardDialog.value = false;
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
});
onUnmounted(() => {
  if (clockTimer !== undefined) window.clearInterval(clockTimer);
});
</script>

<style scoped>
.shop-mode-switch {
  position: relative;
  isolation: isolate;
  gap: 10px;
  padding: 9px;
  overflow: hidden;
  border: 2px solid #f4deb4;
  border-radius: 28px;
  background:
    radial-gradient(
      circle at 10% 18%,
      rgba(255, 255, 255, 0.95) 0 4px,
      transparent 5px
    ),
    radial-gradient(
      circle at 89% 78%,
      rgba(239, 175, 69, 0.17) 0 34px,
      transparent 35px
    ),
    linear-gradient(135deg, #fff8e7, #fff0c8);
  box-shadow:
    0 6px 0 #f3e4c7,
    0 12px 24px rgba(147, 107, 44, 0.08);
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
  background: rgba(229, 158, 45, 0.1);
  filter: blur(1px);
}
.shop-mode-switch :deep(button) {
  min-height: 82px;
  padding: 12px 14px;
  overflow: hidden;
  border: 1px solid rgba(193, 145, 72, 0.12);
  border-radius: 21px;
  background: rgba(255, 255, 255, 0.34);
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
  background: rgba(57, 174, 130, 0.11);
}
.shop-mode-switch :deep(button:hover::before),
.shop-mode-switch :deep(button.active::before) {
  transform: scale(1.28);
}
.shop-mode-switch :deep(button.active) {
  transform: translateY(-2px);
  border-color: rgba(229, 158, 45, 0.32);
  background: linear-gradient(145deg, #fff, #fffbf0);
  box-shadow:
    0 6px 0 rgba(205, 144, 48, 0.18),
    0 12px 18px rgba(139, 100, 39, 0.1);
}
.shop-mode-switch :deep(button:nth-child(2).active) {
  border-color: rgba(57, 174, 130, 0.34);
  box-shadow:
    0 6px 0 rgba(57, 174, 130, 0.18),
    0 12px 18px rgba(39, 113, 86, 0.09);
}
.shop-mode-switch :deep(.page-view-icon) {
  width: 48px;
  height: 48px;
  border-radius: 17px;
  transform: rotate(-4deg);
  box-shadow: inset 0 0 0 2px rgba(255, 255, 255, 0.7);
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
  font-size: 13px;
}
.shop-mode-switch :deep(.page-view-copy small) {
  margin-top: 4px;
  font-size: 9px;
}
.shop-mode-switch :deep(.page-view-check) {
  width: 23px;
  height: 23px;
  top: 9px;
  right: 9px;
  border: 3px solid #fff;
  border-radius: 8px;
  transform: rotate(12deg) scale(0.65);
  box-shadow: 0 3px 0 rgba(129, 86, 27, 0.15);
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
  border: 2px solid #eddbba;
  border-radius: 50%;
  background: linear-gradient(#fff8e9, #f8e8c8);
  box-shadow: 0 5px 0 rgba(219, 189, 139, 0.22);
}
.shop-area-switch :deep(button) {
  min-height: 92px;
  padding: 9px 5px 12px;
  flex-direction: column;
  justify-content: center;
  gap: 5px;
  text-align: center;
  border: 2px solid #f0d59f;
  border-radius: 24px 24px 17px 17px;
  background: linear-gradient(155deg, #fffdf8, #fff2d3);
  box-shadow: 0 5px 0 #ead4ac;
}
.shop-area-switch :deep(button:nth-child(2)) {
  border-color: #cce4d7;
  background: linear-gradient(155deg, #fbfffc, #e6f6ec);
  box-shadow: 0 5px 0 #c9dfd1;
}
.shop-area-switch :deep(button:nth-child(3)) {
  border-color: #dfd5ee;
  background: linear-gradient(155deg, #fff, #f1eafa);
  box-shadow: 0 5px 0 #d9cde8;
}
.shop-area-switch :deep(button:hover) {
  transform: translateY(-3px);
}
.shop-area-switch :deep(button.active) {
  transform: translateY(-6px) rotate(-1deg);
  border-color: #eba735;
  background: linear-gradient(155deg, #fff, #ffebbd);
  box-shadow:
    0 8px 0 #e5bd72,
    0 13px 18px rgba(149, 102, 32, 0.12);
}
.shop-area-switch :deep(button:nth-child(2).active) {
  transform: translateY(-6px) rotate(1deg);
  border-color: #49b888;
  background: linear-gradient(155deg, #fff, #dff6e9);
  box-shadow:
    0 8px 0 #a9d8bf,
    0 13px 18px rgba(37, 113, 83, 0.11);
}
.shop-area-switch :deep(button:nth-child(3).active) {
  transform: translateY(-6px) rotate(-1deg);
  border-color: #9d7ccc;
  background: linear-gradient(155deg, #fff, #eee4fb);
  box-shadow:
    0 8px 0 #c8b3e4,
    0 13px 18px rgba(92, 65, 128, 0.11);
}
.shop-area-switch :deep(.page-view-icon) {
  width: 43px;
  height: 43px;
  border: 3px solid rgba(255, 255, 255, 0.9);
  border-radius: 50%;
  color: #bd7b1e;
  background: #ffe4ae;
  box-shadow: 0 4px 0 #e9c98d;
}
.shop-area-switch :deep(button:nth-child(2) .page-view-icon) {
  color: #278162;
  background: #d9f2e5;
  box-shadow: 0 4px 0 #b8ddca;
}
.shop-area-switch :deep(button:nth-child(3) .page-view-icon) {
  color: #795aa5;
  background: #e9ddf7;
  box-shadow: 0 4px 0 #d2c1e7;
}
.shop-area-switch :deep(button.active .page-view-icon) {
  color: #fff;
  background: linear-gradient(145deg, #f4bd59, #df8f22);
  box-shadow: 0 4px 0 #b96f16;
  transform: rotate(-6deg) scale(1.06);
}
.shop-area-switch :deep(button:nth-child(2).active .page-view-icon) {
  background: linear-gradient(145deg, #69c99f, #309a73);
  box-shadow: 0 4px 0 #24775a;
  transform: rotate(6deg) scale(1.06);
}
.shop-area-switch :deep(button:nth-child(3).active .page-view-icon) {
  background: linear-gradient(145deg, #b496da, #8462b4);
  box-shadow: 0 4px 0 #684593;
  transform: rotate(-6deg) scale(1.06);
}
.shop-area-switch :deep(.page-view-copy strong) {
  font-size: 11px;
}
.shop-area-switch :deep(.page-view-copy small) {
  margin-top: 2px;
  font-size: 7px;
  line-height: 1.15;
}
.shop-area-switch :deep(.page-view-check) {
  width: 19px;
  height: 19px;
  top: 6px;
  right: 6px;
  border: 2px solid #fff;
  background: #e49b2f;
}
.shop-area-switch :deep(button:nth-child(2) .page-view-check) {
  background: #39ae82;
}
.shop-area-switch :deep(button:nth-child(3) .page-view-check) {
  background: #916fc1;
}

.redemption-policy {
  @apply position-relative overflow-hidden;
  border: 2px solid rgba(218, 141, 84, 0.2);
  background:
    radial-gradient(
      circle at 91% 9%,
      rgba(255, 255, 255, 0.7),
      transparent 22%
    ),
    linear-gradient(145deg, #fff8e8, #fff0e7 56%, #eff9f3) !important;
  box-shadow:
    0 6px 0 rgba(172, 112, 63, 0.11),
    0 13px 22px rgba(105, 84, 65, 0.07) !important;
}
.redemption-policy::after {
  content: "◷";
  @apply position-absolute pointer-events-none;
  right: 13px;
  bottom: -18px;
  color: rgba(213, 139, 72, 0.08);
  font-size: 78px;
  font-weight: 900;
}
.redemption-policy strong {
  font-size: 14px;
}
.redemption-clock {
  border: 3px solid rgba(255, 255, 255, 0.9);
  box-shadow: 0 5px 0 rgba(196, 98, 82, 0.14);
  transform: rotate(-5deg);
}
.redemption-status {
  border: 2px solid rgba(255, 255, 255, 0.78) !important;
  box-shadow: 0 3px 0 rgba(161, 76, 67, 0.12);
  font-weight: 900;
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
  font-size: 9px;
  text-align: right;
}
.edition-grid {
  @apply d-grid align-stretch;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;
}
.edition-card {
  min-width: 0;
  height: 100%;
  @apply d-flex flex-column;
  border: 1px solid var(--lad-border);
  box-shadow: 0 3px 0 var(--lad-border) !important;
}
.edition-card.active {
  border-color: rgba(52, 176, 130, 0.6);
  box-shadow: 0 3px 0 rgba(52, 176, 130, 0.25) !important;
}
.edition-preview {
  height: 96px;
  flex: 0 0 96px;
  @apply position-relative overflow-hidden;
  border-radius: 14px;
  background: linear-gradient(160deg, #bfeaff 0 54%, #e8f8dd 55%);
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
  font-size: 19px;
  filter: drop-shadow(0 2px 2px rgba(69, 66, 56, 0.14));
}
.edition-cloud {
  width: 29px;
  height: 10px;
  @apply position-absolute;
  top: 21px;
  z-index: -1;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.78);
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
  border: 3px solid color-mix(in srgb, var(--edition-roof) 64%, #55453c);
  border-radius: 7px 7px 4px 4px;
  background: linear-gradient(
    90deg,
    color-mix(in srgb, var(--edition-wall) 88%, white),
    var(--edition-wall)
  );
  box-shadow:
    0 5px 0 color-mix(in srgb, var(--edition-accent) 62%, #6e6158),
    0 7px 8px rgba(65, 56, 49, 0.2);
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
    color-mix(in srgb, var(--edition-roof) 78%, #55453c)
  );
}
.edition-house b {
  width: 14px;
  height: 24px;
  @apply position-absolute;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  border: 2px solid rgba(74, 68, 59, 0.14);
  border-bottom: 0;
  border-radius: 7px 7px 0 0;
  background: var(--edition-door);
}
.edition-window {
  width: 12px;
  height: 13px;
  @apply position-absolute;
  top: 8px;
  border: 3px solid #fff;
  border-radius: 4px;
  background: #9edff0;
  box-shadow: 0 2px 0 rgba(66, 91, 92, 0.12);
}
.window-left {
  left: 6px;
}
.window-right {
  right: 6px;
}
.edition-cotton-candy-dream {
  background: linear-gradient(160deg, #d9f5ff 0 52%, #fff0fa 53%);
}
.edition-cotton-candy-dream::after {
  background: repeating-linear-gradient(
    135deg,
    #f9bdd8 0 9px,
    #cdeeff 9px 18px
  );
  opacity: 0.9;
}
.edition-cotton-candy-dream .edition-house {
  border-color: #b975a9;
  background: linear-gradient(90deg, #fff8fc, #e9f8ff);
}
.edition-cotton-candy-dream .edition-house::before {
  bottom: 33px;
  height: 34px;
  filter: drop-shadow(0 -2px 0 #fff);
  background:
    radial-gradient(circle at 10% 76%, #fff 0 8px, transparent 9px),
    radial-gradient(circle at 28% 60%, #f6acd0 0 11px, transparent 12px),
    radial-gradient(circle at 49% 48%, #bceafb 0 12px, transparent 13px),
    radial-gradient(circle at 69% 61%, #fff 0 11px, transparent 12px),
    radial-gradient(circle at 89% 76%, #f4a3cb 0 9px, transparent 10px);
}
.edition-cotton-candy-dream .edition-window {
  background: #ffe58c;
}
.edition-candy {
  width: 12px;
  height: 8px;
  @apply position-absolute;
  z-index: 3;
  border: 2px solid #fff;
  border-radius: 999px;
  background: repeating-linear-gradient(90deg, #ef83bb 0 4px, #78cfe6 4px 8px);
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
  border: 2px solid #fff;
  border-radius: 50%;
  background: conic-gradient(#ef83bb 0 25%, #fff 0 50%, #79cfe7 0 75%, #fff 0);
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
  background: #9b6c58;
}
.lollipop-left {
  left: 5px;
}
.lollipop-right {
  right: 5px;
  transform: scale(0.78);
}
.edition-halloween-night {
  background: linear-gradient(165deg, #342944 0 58%, #7a8d61 59%);
}
.edition-halloween-night::after {
  background: linear-gradient(#71845d, #4e6648);
}
.edition-halloween-night .edition-cloud {
  background: rgba(173, 159, 190, 0.34);
}
.edition-moon {
  width: 24px;
  height: 24px;
  @apply position-absolute;
  top: 8px;
  left: 10px;
  border-radius: 50%;
  background: #ffe18a;
  box-shadow: 0 0 12px rgba(255, 225, 138, 0.65);
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
  background: #30283c;
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
.edition-flower {
  width: 8px;
  height: 8px;
  @apply position-absolute;
  bottom: 17px;
  z-index: 3;
  border: 2px solid #fff4a8;
  border-radius: 50%;
  background: #f27f81;
  box-shadow: 0 7px 0 -2px #4d9a62;
}
.flower-left {
  left: 13px;
}
.flower-right {
  right: 12px;
  background: #8f8bd4;
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
.set-list {
  @apply d-flex flex-column;
  gap: 12px;
}
.set-card {
  @apply d-grid align-center position-relative overflow-hidden;
  grid-template-columns: 64px minmax(0, 1fr) auto;
  gap: 13px;
  border: 2px solid rgba(75, 143, 117, 0.18);
  background:
    radial-gradient(
      circle at 88% 5%,
      rgba(255, 226, 142, 0.24),
      transparent 28%
    ),
    linear-gradient(145deg, #fffefd, #f8fcf8);
  box-shadow:
    0 6px 0 rgba(73, 132, 109, 0.14),
    0 13px 25px rgba(63, 105, 91, 0.08) !important;
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease;
}
.set-card:hover {
  transform: translateY(-2px);
  box-shadow:
    0 8px 0 rgba(73, 132, 109, 0.14),
    0 16px 28px rgba(63, 105, 91, 0.1) !important;
}
.set-icon {
  width: 64px;
  height: 64px;
  @apply d-grid place-center;
  border: 2px solid rgba(255, 255, 255, 0.9);
  border-radius: 20px;
  background: linear-gradient(145deg, #e9f7ff, #edf7dc);
  box-shadow: 0 5px 0 rgba(75, 143, 117, 0.12);
  font-size: 32px;
  transform: rotate(-3deg);
}
.set-copy strong {
  font-size: 15px;
}
.set-copy p {
  margin-top: 3px;
  line-height: 1.35;
}
.set-copy small {
  color: var(--lad-mint-dark);
  font-size: 8px;
  font-weight: 900;
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
  color: #855814;
  border: 2px solid rgba(237, 169, 48, 0.25);
  border-radius: 999px;
  background: linear-gradient(145deg, #fff9dd, #ffe8a2);
  box-shadow: 0 4px 0 rgba(202, 139, 31, 0.14);
  font-size: 12px;
  font-weight: 950;
}
.catalog-buy-button {
  min-height: 48px !important;
  padding-inline: 8px 12px !important;
  border: 2px solid rgba(255, 255, 255, 0.82) !important;
  border-radius: 17px !important;
  background: linear-gradient(145deg, #56a9f0, #347fd3) !important;
  box-shadow:
    0 5px 0 #286eb9,
    0 10px 18px rgba(52, 127, 211, 0.2) !important;
  text-transform: none;
  letter-spacing: 0;
}
.catalog-buy-button:active {
  transform: translateY(3px);
  box-shadow: 0 2px 0 #286eb9 !important;
}
.catalog-buy-price {
  padding: 4px 8px 4px 4px;
  @apply d-inline-flex align-center;
  gap: 4px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.22);
  font-weight: 950;
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
  @apply h-100 d-flex flex-column;
}
.request-reward-button {
  min-height: 40px !important;
  padding-inline: 13px !important;
  border: 2px solid rgba(255, 255, 255, 0.84) !important;
  border-radius: 15px !important;
  background: linear-gradient(145deg, #64b9f2, #3987d8) !important;
  box-shadow:
    0 5px 0 #2d72ba,
    0 9px 16px rgba(49, 126, 199, 0.18) !important;
  text-transform: none;
  letter-spacing: 0;
}
.request-reward-button:active {
  transform: translateY(3px);
  box-shadow: 0 2px 0 #2d72ba !important;
}
.request-reward-button:disabled {
  box-shadow: 0 3px 0 rgba(84, 109, 126, 0.15) !important;
}
.reward-icon,
.accessory-preview {
  height: 78px;
  @apply d-grid place-center;
  border-radius: 16px;
  background: linear-gradient(145deg, #e9f5ff, #f2f7db);
  font-size: 42px;
}
.accessory-preview {
  @apply position-relative;
}
.accessory-card.special {
  border-color: rgba(236, 170, 55, 0.45);
  background: linear-gradient(155deg, #fffdfa, #fff7dc);
}
.special-item-preview {
  width: 104px;
  height: 104px;
}
.motion-chip {
  @apply position-absolute;
  top: 7px;
  left: 7px;
  font-size: 8px;
}
.owned-status-row {
  margin-top: auto;
  padding-top: 14px;
  @apply d-flex justify-center;
}
.owned-status {
  min-height: 38px;
  padding: 6px 13px;
  @apply position-relative d-inline-flex align-center justify-center;
  gap: 6px;
  color: #237257;
  border: 2px solid rgba(59, 170, 127, 0.27);
  border-radius: 999px;
  background: linear-gradient(145deg, #edfbf5, #d8f4e6);
  box-shadow:
    0 4px 0 rgba(48, 142, 105, 0.16),
    0 8px 15px rgba(57, 139, 107, 0.09);
  font-size: 11px;
  font-weight: 950;
}
.owned-status i {
  color: #e3a328;
  font-style: normal;
  text-shadow: 0 0 6px rgba(255, 216, 103, 0.8);
  animation: owned-star 1.9s ease-in-out infinite;
}
.owned-status i:last-child {
  animation-delay: -0.9s;
}
.reward-icon {
  width: 78px;
  @apply flex-shrink-0;
}
.reward-description,
.accessory-description {
  min-height: 47px;
}
.reward-conditions {
  min-height: 68px;
  padding: 9px;
  @apply d-flex align-center;
  gap: 8px;
  color: #5e6f78;
  border: 2px solid rgba(234, 170, 55, 0.2);
  border-radius: 15px;
  background:
    radial-gradient(
      circle at 9% 14%,
      rgba(255, 255, 255, 0.85),
      transparent 24%
    ),
    linear-gradient(145deg, #fffaf0, #fff0ca);
  box-shadow:
    0 4px 0 rgba(201, 137, 38, 0.09),
    inset 0 1px 0 rgba(255, 255, 255, 0.8);
  font-size: 9px;
  line-height: 1.4;
}
.reward-conditions > span:last-child,
.reward-conditions strong {
  @apply d-block;
}
.reward-conditions strong {
  margin-bottom: 2px;
  color: #6f4a15;
  font-size: 10px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.condition-ladi {
  width: 58px;
  height: 50px;
  @apply position-relative d-grid place-center flex-shrink-0;
}
.condition-ladi :deep(.ladi-wrap) {
  grid-area: 1 / 1;
}
.condition-ladi i {
  @apply position-absolute;
  right: -1px;
  bottom: 2px;
  z-index: 2;
  font-style: normal;
  font-size: 18px;
  filter: drop-shadow(0 2px 0 #fff);
  animation: condition-finger 1.8s ease-in-out infinite;
}
.reward-actions {
  margin-top: auto;
  padding-top: 16px;
}
.price {
  @apply d-flex align-center;
  gap: 7px;
  font-size: 13px;
  @apply font-weight-black;
}
.accessory-card.owned {
  border-color: rgba(62, 188, 140, 0.45);
}
.locked-set-status {
  min-height: 38px !important;
  padding-inline: 10px !important;
  border: 2px solid rgba(231, 166, 54, 0.24) !important;
  border-radius: 14px !important;
  color: #8c601b !important;
  background: linear-gradient(145deg, #fff9dc, #ffe8ad) !important;
  box-shadow:
    0 4px 0 rgba(197, 132, 28, 0.14),
    0 8px 14px rgba(155, 104, 27, 0.08) !important;
  font-weight: 900;
}
.locked-set-status :deep(.v-icon) {
  color: #d88d18;
  filter: drop-shadow(0 2px 0 rgba(255, 255, 255, 0.8));
}
.reward-form {
  grid-template-columns: 0.7fr 1.3fr;
}
.field-label {
  color: var(--lad-muted);
  font-size: 11px;
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
  font-size: 23px;
}
.icon-picker button.active {
  border: 2px solid var(--lad-mint);
  background: #eaf8f1;
}
@media (max-width: 460px) {
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
@media (max-width: 380px) {
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
@keyframes condition-finger {
  0%,
  100% {
    transform: translateY(0) rotate(-7deg);
  }
  45%,
  60% {
    transform: translateY(-4px) rotate(5deg) scale(1.08);
  }
}
@media (prefers-reduced-motion: reduce) {
  .owned-status i,
  .condition-ladi i {
    animation: none;
  }
}
</style>
