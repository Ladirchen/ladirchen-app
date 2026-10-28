import { createRouter, createWebHashHistory } from "vue-router";
const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "world",
      component(this: void) {

        return import("@/features/world/pages/WorldPage.vue");
      },
    },
    {
      path: "/contributions",
      name: "contributions",
      component(this: void) {

        return import("@/features/contributions/pages/ContributionsPage.vue");
      },
    },
    {
      path: "/wishes",
      name: "wishes",
      component(this: void) {

        return import("@/features/savings/pages/WishesPage.vue");
      },
    },
    {
      path: "/shop",
      name: "shop",
      component(this: void) {

        return import("@/features/shop/pages/ShopPage.vue");
      },
    },
    {
      path: "/family",
      name: "family",
      component(this: void) {

        return import("@/features/family/pages/FamilyPage.vue");
      },
    },
    {
      path: "/profile",
      name: "profile",
      component(this: void) {

        return import("@/features/profile/pages/ProfilePage.vue");
      },
    },
  ],
});
export default router;
