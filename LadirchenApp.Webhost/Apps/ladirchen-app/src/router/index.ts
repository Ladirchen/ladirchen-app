import { createRouter, createWebHistory } from "vue-router";

const DEFAULT_DESCRIPTION =
  "Ladirli hilft Familien, Haushaltsaufgaben und Verantwortung fair, übersichtlich und kindgerecht zu organisieren.";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "landing",
      component: () => import("@/features/landing/pages/LandingPage.vue"),
      meta: { description: DEFAULT_DESCRIPTION, title: "Ladirli – Gemeinsam anpacken. Fair verteilen." },
    },
    {
      path: "/login",
      name: "login",
      component: () => import("@/features/auth/pages/LoginPage.vue"),
      meta: { robots: "noindex, nofollow", title: "Anmelden – Ladirli" },
    },
    {
      path: "/app",
      component: () => import("@/app/components/FamilyWorldShell.vue"),
      meta: { robots: "noindex, nofollow" },
      children: [
        { path: "", name: "world", component: () => import("@/features/world/pages/WorldPage.vue") },
        {
          path: "contributions",
          name: "contributions",
          component: () => import("@/features/contributions/pages/ContributionsPage.vue"),
        },
        { path: "wishes", name: "wishes", component: () => import("@/features/savings/pages/WishesPage.vue") },
        { path: "shop", name: "shop", component: () => import("@/features/shop/pages/ShopPage.vue") },
        { path: "family", name: "family", component: () => import("@/features/family/pages/FamilyPage.vue") },
        { path: "profile", name: "profile", component: () => import("@/features/profile/pages/ProfilePage.vue") },
      ],
    },
    { path: "/:pathMatch(.*)*", redirect: "/" },
  ],
});

router.afterEach((route) => {
  const title = typeof route.meta.title === "string" ? route.meta.title : "Ladirli – Familienwelt";
  const description = typeof route.meta.description === "string" ? route.meta.description : DEFAULT_DESCRIPTION;
  const robots = typeof route.meta.robots === "string" ? route.meta.robots : "index, follow";
  const canonical = new URL(route.path === "/" ? "/" : route.path, window.location.origin).toString();

  document.title = title;
  document.querySelector<HTMLMetaElement>('meta[name="description"]')?.setAttribute("content", description);
  document.querySelector<HTMLMetaElement>('meta[name="robots"]')?.setAttribute("content", robots);
  document.querySelector<HTMLLinkElement>('link[rel="canonical"]')?.setAttribute("href", canonical);
  document.querySelector<HTMLMetaElement>('meta[property="og:title"]')?.setAttribute("content", title);
  document.querySelector<HTMLMetaElement>('meta[property="og:description"]')?.setAttribute("content", description);
  document.querySelector<HTMLMetaElement>('meta[property="og:url"]')?.setAttribute("content", canonical);
});

export default router;
