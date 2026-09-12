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
      <PageViewSwitch v-model="activeTab" class="mb-5" label="Shopbereich auswählen" :options="shopViewOptions" tone="amber" />

      <template v-if="activeTab === 'family'">
        <v-card class="redemption-policy pa-4 mb-5" :color="redemptionOpen ? 'green-lighten-5' : 'red-lighten-5'" elevation="0" rounded="xl">
          <div class="d-flex align-center ga-3">
            <v-avatar :color="redemptionOpen ? 'primary' : 'error'" size="44" variant="tonal"><v-icon>mdi-clock-outline</v-icon></v-avatar>
            <div class="flex-grow-1">
              <div class="d-flex align-center justify-space-between flex-wrap ga-2">
                <strong>Einlösen nur bis 18:00 Uhr</strong>
                <v-chip :color="redemptionOpen ? 'success' : 'error'" size="x-small" variant="tonal">{{ redemptionOpen ? 'Heute geöffnet' : 'Für heute geschlossen' }}</v-chip>
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
            <div class="d-flex align-start justify-space-between ga-2">
              <div class="reward-icon">{{ reward.icon }}</div>
              <v-chip color="primary" size="x-small" variant="tonal">{{ categoryLabel(reward.category) }}</v-chip>
            </div>
            <strong class="d-block mt-3">{{ reward.title }}</strong>
            <p class="reward-description text-caption text-medium-emphasis mt-1">{{ reward.description }}</p>
            <div class="reward-metadata mt-3">
              <span><v-icon size="13">mdi-package-variant</v-icon>{{ reward.quantity }} verfügbar</span>
              <span><v-icon size="13">mdi-calendar-outline</v-icon>{{ availabilityLabel(reward.availableUntil) }}</span>
            </div>
            <p class="reward-conditions mt-2"><strong>Bedingung:</strong> {{ reward.conditions }}</p>
            <div class="reward-actions d-flex align-center justify-space-between ga-2">
              <span class="price"><LadirchenCoin small />{{ reward.price }}</span>
              <template v-if="store.viewerRole === 'child'">
                <v-btn v-if="reward.status === 'available'" color="info" :disabled="!canRequest(reward)" rounded="lg" size="small" variant="flat" @click="store.requestShopReward(reward.id)">Anfragen</v-btn>
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
        <PageViewSwitch v-model="houseArea" compact label="Hausbereich auswählen" class="mb-4" :options="houseAreaOptions" tone="amber" />

        <div class="accessory-grid">
          <v-card v-for="accessory in visibleAccessories" :key="accessory.id" class="accessory-card pa-4" :class="{ owned: accessory.owned }" elevation="0" rounded="xl">
            <div class="accessory-preview">
              <span>{{ accessory.icon }}</span>
              <v-chip v-if="accessory.owned" class="owned-chip" color="success" size="x-small">In Besitz</v-chip>
            </div>
            <strong class="d-block mt-3">{{ accessory.title }}</strong>
            <p class="accessory-description text-caption text-medium-emphasis mt-1">{{ accessory.description }}</p>
            <div class="d-flex align-center justify-space-between mt-4 ga-2">
              <span class="price"><LadirchenCoin small />{{ accessory.price }}</span>
              <v-btn v-if="!accessory.owned && store.viewerRole === 'child'" color="info" :disabled="accessory.price > store.availableBalance" rounded="lg" size="small" variant="flat" @click="store.purchaseAccessory(accessory.id)">Kaufen</v-btn>
              <v-chip v-else-if="!accessory.owned" color="info" size="x-small" variant="tonal">Kinderkauf</v-chip>
              <v-switch v-else color="primary" density="compact" hide-details inset :model-value="accessory.equipped" @update:model-value="store.toggleAccessory(accessory.id)" />
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
import LadirchenCoin from '@/shared/components/LadirchenCoin.vue';
import PageHeader from '@/shared/components/ui/PageHeader.vue';
import PageViewSwitch from '@/shared/components/ui/PageViewSwitch.vue';
import type { PageViewOption } from '@/shared/components/ui/PageViewSwitch.vue';
import { shopRedemptionIsOpen } from '@/domain/shop';
import type { ShopReward, ShopRewardCategory } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';

const store = useFamilyWorldStore();
const route = useRoute();
const activeTab = ref<'family' | 'house'>('family');
const shopViewOptions: ReadonlyArray<PageViewOption<'family' | 'house'>> = [
  { id: 'family', icon: 'mdi-gift-outline', subtitle: 'Belohnungen einlösen', title: 'Familien-Shop' },
  { id: 'house', icon: 'mdi-home-city-outline', subtitle: 'Eure Welt gestalten', title: 'Hauskatalog' },
];
const houseAreaOptions: ReadonlyArray<PageViewOption<'inside' | 'outside'>> = [
  { id: 'inside', icon: 'mdi-sofa-outline', subtitle: 'Möbel und Räume', title: 'Innen' },
  { id: 'outside', icon: 'mdi-flower-outline', subtitle: 'Garten und Fassade', title: 'Draußen' },
];
const houseArea = ref<'inside' | 'outside'>('inside');
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
const visibleAccessories = computed(() =>
  store.accessories.filter((accessory) => (accessory.placement ?? 'outside') === houseArea.value),
);
const redemptionOpen = computed(() => shopRedemptionIsOpen(currentTime.value));
const categoryLabel = (category: ShopRewardCategory) => categoryOptions.find((option) => option.value === category)?.title ?? 'Belohnung';
const availabilityLabel = (availableUntil?: string) => availableUntil
  ? `bis ${new Date(`${availableUntil}T12:00:00`).toLocaleDateString('de-CH')}`
  : 'dauerhaft';
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
.redemption-policy {
  border: 1px solid rgba(62, 188, 140, 0.2);
}
.reward-grid,
.accessory-grid {
  @apply d-grid;
  grid-template-columns: 1fr 1fr;
  gap: 11px;
}
.reward-card,
.accessory-card {
  @apply min-w-0;
  border: 1px solid var(--lad-border);
  box-shadow: 0 4px 0 var(--lad-border) !important;
}
.reward-card {
  @apply h-100 d-flex flex-column;
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
.owned-chip {
  @apply position-absolute;
  top: 7px;
  right: 7px;
  font-size: 8px;
}
.reward-icon {
  width: 78px;
  @apply flex-shrink-0;
}
.reward-description,
.accessory-description {
  min-height: 47px;
}
.reward-metadata {
  @apply d-flex flex-wrap;
  gap: 5px;
}
.reward-metadata span {
  padding: 4px 6px;
  @apply d-inline-flex align-center;
  gap: 3px;
  color: var(--lad-muted);
  border-radius: 8px;
  background: var(--lad-surface-soft);
  font-size: 9px;
  font-weight: 800;
}
.reward-conditions {
  min-height: 42px;
  color: var(--lad-muted);
  font-size: 9px;
  line-height: 1.4;
}
.reward-conditions strong {
  color: var(--lad-text);
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
@media (max-width: 380px) {
  .reward-grid,
  .accessory-grid {
    grid-template-columns: 1fr;
  }
  .reward-description,
  .accessory-description {
    min-height: 0;
  }
}
</style>
