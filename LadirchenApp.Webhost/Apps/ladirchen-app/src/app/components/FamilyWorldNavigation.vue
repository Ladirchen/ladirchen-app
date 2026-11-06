<template>
  <nav :aria-label="t('common.mainNavigation')" class="family-world-navigation position-absolute d-grid">
    <RouterLink
      v-for="item in navigation"
      :key="item.to"
      class="d-flex flex-column align-center justify-center text-decoration-none"
      :class="`navigation-tone--${item.icon}`"
      :data-ladi-tip="item.tip"
      :to="item.to"
    >
      <AppNavigationIcon :name="item.icon" />
      <span>{{ item.label }}</span>
    </RouterLink>
  </nav>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { useFamilyWorldStore } from "@/stores/family-world";
import AppNavigationIcon from "./AppNavigationIcon.vue";

type NavigationIcon = "family" | "contributions" | "profile" | "world" | "wishes" | "shop";

interface NavigationItem {
  to: string;
  icon: NavigationIcon;
  label: string;
  tip: string;
}

const store = useFamilyWorldStore();
const { t } = useI18n();
const navigationItems = computed<NavigationItem[]>(() => [
  { to: "/app", icon: "world", label: t("navigation.world.label"), tip: t("navigation.world.tip") },
  {
    to: "/app/contributions",
    icon: "contributions",
    label: t("navigation.contributions.label"),
    tip: t("navigation.contributions.tip"),
  },
  { to: "/app/wishes", icon: "wishes", label: t("navigation.wishes.label"), tip: t("navigation.wishes.tip") },
  { to: "/app/shop", icon: "shop", label: t("navigation.shop.label"), tip: t("navigation.shop.tip") },
  { to: "/app/family", icon: "family", label: t("navigation.family.label"), tip: t("navigation.family.tip") },
  { to: "/app/profile", icon: "profile", label: t("navigation.profile.label"), tip: t("navigation.profile.tip") },
]);
const navigation = computed(() => {
  if (store.signedInMember?.guardianAccess === "external") {
    return navigationItems.value.filter((item) =>
      ["/app/contributions", "/app/wishes", "/app/shop", "/app/profile"].includes(item.to),
    );
  }

  if (store.viewerRole === "guardian" && !store.permissions.canManageContent) {
    return navigationItems.value.filter((item) => !["/app/contributions", "/app/shop"].includes(item.to));
  }

  return navigationItems.value;
});
</script>
