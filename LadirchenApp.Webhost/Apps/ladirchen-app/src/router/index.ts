import { createRouter, createWebHashHistory } from 'vue-router';

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'world', component: () => import('@/features/world/pages/WorldPage.vue') },
    { path: '/beitraege', name: 'contributions', component: () => import('@/features/contributions/pages/ContributionsPage.vue') },
    { path: '/wuensche', name: 'wishes', component: () => import('@/features/savings/pages/WishesPage.vue') },
    { path: '/shop', name: 'shop', component: () => import('@/features/shop/pages/ShopPage.vue') },
    { path: '/familie', name: 'family', component: () => import('@/features/family/pages/FamilyPage.vue') },
    { path: '/ich', name: 'profile', component: () => import('@/features/profile/pages/ProfilePage.vue') },
  ],
});

export default router;
