import { DateTime } from "luxon";
import { computed, onMounted, reactive, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";

import { FURNITURE_SETS, HOUSE_STAGES, HOUSE_THEMES } from "@/domain/house";
import type { FurnitureSetId, HouseAccessoryId, HouseThemeId } from "@/domain/house";
import { shopRedemptionIsOpen, shopRewardIsPublished } from "@/domain/shop";
import type { ShopReward, ShopRewardCategory } from "@/domain/shop";
import { calendarDateInTimeZone, calendarDateIsWithin } from "@/domain/shared/zoned-calendar";
import { HOUSE_EXTERIOR_ASSET_URLS, houseExteriorBackgroundAssetId, houseExteriorHouseAssetId } from "@/shared/visuals/house/house-exterior-assets";
import type { PageViewOption } from "@/shared/components/ui/PageViewSwitch.vue";
import { useLocalizedDomainContent } from "@/shared/composables/use-localized-domain-content";
import { UI_ICONS } from "@/shared/ui-icons";
import { useFamilyWorldStore } from "@/stores/family-world";
import { ladiGuideController } from "@/shared/services/ladi-guide-controller";
import { PAGE_INTRO_GUIDE_DELAY_MS } from "@/shared/runtime-timing";
import type { ShopRewardPublicationStatus } from "@/features/shop/shop-view-models";

type HouseCatalogArea = "inside" | "outside" | "special";
type PublicationMode = "now" | "scheduled" | "hidden";

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

const createEmptyReward = (): RewardDraft => ({
  title: "", description: "", icon: "🎁", price: 100, category: "activity", quantity: 1,
  conditions: "", publicationMode: "now", availableFrom: "", unlimited: true, availableUntil: "",
});

export const useShopPage = () => {
  const store = useFamilyWorldStore();
  const route = useRoute();
  const { locale, t } = useI18n();
  const localize = useLocalizedDomainContent();
  const activeTab = ref<"family" | "house">("family");
  const houseArea = ref<HouseCatalogArea>("inside");
  const rewardDialog = ref(false);
  const rewardToDelete = ref<ShopReward | null>(null);
  const currentTime = computed(() => new Date(store.currentTimeMilliseconds));
  const iconOptions = ["🎁", "🎮", "🍿", "🎨", "🎟️", "🧁", "🚲", "🌙", "💵", "🛍️"];
  const shopViewOptions = computed<ReadonlyArray<PageViewOption<"family" | "house">>>(() => [
    { id: "family", icon: UI_ICONS.shopView.family, subtitle: t("shop.views.family.subtitle"), title: t("shop.views.family.title") },
    { id: "house", icon: UI_ICONS.shopView.house, subtitle: t("shop.views.house.subtitle"), title: t("shop.views.house.title") },
  ]);
  const houseAreaOptions = computed<ReadonlyArray<PageViewOption<HouseCatalogArea>>>(() => [
    { id: "inside", icon: UI_ICONS.shopArea.inside, subtitle: t("shop.areas.inside.subtitle"), title: t("shop.areas.inside.title") },
    { id: "outside", icon: UI_ICONS.shopArea.outside, subtitle: t("shop.areas.outside.subtitle"), title: t("shop.areas.outside.title") },
    { id: "special", icon: UI_ICONS.shopArea.special, subtitle: t("shop.areas.special.subtitle"), title: t("shop.areas.special.title") },
  ]);
  const furnitureSetAreaLabel = computed(() => t(
    houseArea.value === "inside" ? "shop.catalog.sets.inside" : "shop.catalog.sets.outside",
  ));
  const catalogItemsEyebrow = computed(() => t(
    houseArea.value === "special" ? "shop.catalog.items.specialEyebrow" : "shop.catalog.items.eyebrow",
  ));
  const catalogItemsTitle = computed(() => t(
    houseArea.value === "special" ? "shop.catalog.items.specialTitle" : "shop.catalog.items.title",
  ));
  const categoryOptions = computed<Array<{ title: string; value: ShopRewardCategory }>>(() => [
    { title: t("shop.categories.time"), value: "time" },
    { title: t("shop.categories.activity"), value: "activity" },
    { title: t("shop.categories.allowance"), value: "allowance" },
    { title: t("shop.categories.gift"), value: "gift" },
    { title: t("shop.categories.privilege"), value: "privilege" },
    { title: t("shop.categories.custom"), value: "custom" },
  ]);
  const publicationModeOptions = computed<Array<{ title: string; value: PublicationMode }>>(() => [
    { title: t("shop.publication.now"), value: "now" },
    { title: t("shop.publication.scheduled"), value: "scheduled" },
    { title: t("shop.publication.hidden"), value: "hidden" },
  ]);
  const newReward = reactive(createEmptyReward());
  const minimumAvailableDate = computed(() => calendarDateInTimeZone(currentTime.value, store.familyTimeZone));
  const canAddReward = computed(() => Boolean(
    newReward.icon.trim() && newReward.title.trim() && newReward.description.trim() && newReward.conditions.trim() &&
    newReward.price >= 1 && newReward.quantity >= 1 &&
    (newReward.publicationMode !== "scheduled" || newReward.availableFrom) &&
    (newReward.unlimited || newReward.availableUntil) &&
    (!newReward.availableFrom || !newReward.availableUntil || newReward.availableFrom <= newReward.availableUntil),
  ));
  const displayedShopRewards = computed(() => store.viewerRole === "guardian"
    ? store.shopRewards.map(localize.reward)
    : store.shopRewards.filter(reward => shopRewardIsPublished(reward, store.familyTimeZone, currentTime.value)).map(localize.reward));
  const localizedAccessories = computed(() => store.accessories.map(localize.accessory));
  const visibleAccessories = computed(() => localizedAccessories.value.filter((accessory) => {
    if (houseArea.value === "special") {return accessory.category === "special";}
    if (accessory.category === "special") {return false;}
    return accessory.placement === houseArea.value;
  }));
  const houseEditions = computed(() => HOUSE_THEMES.map(edition => ({ ...edition, name: t(edition.nameKey), description: t(edition.descriptionKey) })));
  const visibleFurnitureSets = computed(() => FURNITURE_SETS
    .filter(set => houseArea.value === "outside" ? set.zoneId === "garden" : houseArea.value === "inside" && set.zoneId !== "garden")
    .map(set => ({ ...set, name: t(set.nameKey), description: t(set.descriptionKey) })));
  const previewHouseStage = computed(() => HOUSE_STAGES.find(stage => stage.level === store.houseLevel) ?? HOUSE_STAGES[0]!);
  const editionPreviewBackground = (themeId: HouseThemeId) => HOUSE_EXTERIOR_ASSET_URLS[houseExteriorBackgroundAssetId(themeId, 100)];
  const editionPreviewHouse = (themeId: HouseThemeId) => HOUSE_EXTERIOR_ASSET_URLS[houseExteriorHouseAssetId(previewHouseStage.value.id, themeId)];
  const setPreviewAccessories = (accessoryIds: ReadonlyArray<HouseAccessoryId>) => accessoryIds.slice(0, 3)
    .map(id => store.accessories.find(accessory => accessory.id === id)).filter(accessory => accessory !== undefined);
  const ownsEdition = (id: HouseThemeId) => store.ownedHouseThemeIds.includes(id);
  const ownsSet = (id: FurnitureSetId) => store.ownedFurnitureSetIds.includes(id);
  const redemptionOpen = computed(() => shopRedemptionIsOpen(store.familyTimeZone, currentTime.value));
  const canRequest = (reward: ShopReward) => redemptionOpen.value && reward.price <= store.availableBalance && reward.quantity > 0 && shopRewardIsPublished(reward, store.familyTimeZone, currentTime.value);
  const rewardIsScheduled = (reward: ShopReward) => Boolean(reward.isVisible !== false && reward.availableFrom && reward.availableFrom > minimumAvailableDate.value);
  const formatCalendarDate = (date: string) => DateTime.fromISO(date)
    .setLocale(locale.value === "en" ? "en-GB" : "de-CH")
    .toLocaleString({ day: "2-digit", month: "2-digit", year: "numeric" });
  const rewardPublicationStatus = (reward: ShopReward): ShopRewardPublicationStatus => {
    if (reward.isVisible === false) {return { icon: "i-mdi:eye-off-outline", label: t("shop.publication.hiddenStatus"), tone: "hidden" };}
    if (rewardIsScheduled(reward)) {return { icon: "i-mdi:calendar-clock-outline", label: t("shop.publication.from", { date: formatCalendarDate(reward.availableFrom!) }), tone: "scheduled" };}
    if (reward.availableUntil && !calendarDateIsWithin(minimumAvailableDate.value, undefined, reward.availableUntil)) {
      return { icon: "i-mdi:calendar-remove-outline", label: t("shop.publication.expired"), tone: "expired" };
    }
    return { icon: "i-mdi:eye-outline", label: t("shop.publication.visible"), tone: "visible" };
  };
  const addReward = () => {
    store.addShopReward({
      title: newReward.title.trim(), description: newReward.description.trim(), icon: newReward.icon.trim(),
      price: newReward.price, category: newReward.category, quantity: newReward.quantity,
      conditions: newReward.conditions.trim(),
      availableFrom: newReward.publicationMode === "scheduled" ? newReward.availableFrom : undefined,
      availableUntil: newReward.unlimited ? undefined : newReward.availableUntil,
      isVisible: newReward.publicationMode !== "hidden",
    });
    Object.assign(newReward, createEmptyReward());
    rewardDialog.value = false;
  };
  const confirmRewardDeletion = () => {
    if (!rewardToDelete.value) {return;}
    store.deleteShopReward(rewardToDelete.value.id);
    rewardToDelete.value = null;
  };
  watch(() => route.query.new, (value) => {
    if (value === "1" && store.permissions.canManageContent) { activeTab.value = "family"; rewardDialog.value = true; }
  }, { immediate: true });
  onMounted(() => {
    if (store.viewerRole === "child") {
      window.setTimeout(() => ladiGuideController.say({
        heading: redemptionOpen.value ? t("shop.guide.openTitle") : t("shop.guide.closedTitle"),
        message: redemptionOpen.value ? t("shop.guide.openMessage") : t("shop.guide.closedMessage"), pageIntro: true,
      }), PAGE_INTRO_GUIDE_DELAY_MS);
    }
  });

  return {
    activeTab, addReward, canAddReward, canRequest, catalogItemsEyebrow, catalogItemsTitle,
    categoryOptions, confirmRewardDeletion,
    displayedShopRewards, editionPreviewBackground, editionPreviewHouse, houseArea, houseAreaOptions,
    houseEditions, iconOptions, minimumAvailableDate, newReward, ownsEdition, ownsSet, furnitureSetAreaLabel,
    publicationModeOptions, redemptionOpen, rewardDialog, rewardPublicationStatus, rewardToDelete,
    setPreviewAccessories, shopViewOptions, store, visibleAccessories, visibleFurnitureSets,
  };
};
